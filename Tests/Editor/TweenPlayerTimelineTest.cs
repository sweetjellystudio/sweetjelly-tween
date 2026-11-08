namespace SWeetJelly.Tween.Tests.EditMode
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using SWeetJelly.Tween.App;
    using SWeetJelly.Tween.Contract;
    using SWeetJelly.Tween.Domain;
    using SWeetJelly.Tween.Framework.Unity;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public sealed class TweenPlayerTimelineTest
    {
        private readonly List<GameObject> objects = new();
        private TweenService service;
        [SetUp] public void Setup() => service = new TweenService();
        [TearDown] public void Cleanup()
        {
            foreach (var item in objects) if (item != null) Object.DestroyImmediate(item);
            objects.Clear(); service.Dispose();
        }
        private static void Set(object target, string field, object value) => TweenPlayerAsyncTest.Set(target, field, value);
        private T Player<T>() where T : TweenPlayerBehaviour
        {
            var go = new GameObject(typeof(T).Name, typeof(RectTransform), typeof(CanvasGroup));
            objects.Add(go); go.SetActive(false);
            var player = go.AddComponent<T>(); player.Initialise(service); go.SetActive(true);
            return player;
        }
        private CanvasGroupFadeTweenPlayer Fade(float end = 1f, float seconds = 1f, CanvasGroup target = null)
        {
            var player = Player<CanvasGroupFadeTweenPlayer>();
            if (target != null) Set(player, "targetComponent", target);
            else player.GetComponent<CanvasGroup>().alpha = 0f;
            Set(player, "duration", seconds); Set(player, "forwardValue", end);
            return player;
        }
        private T Compose<T>(params TweenPlayerBehaviour[] children) where T : TweenPlayerBehaviour
        { var root = Player<T>(); Set(root, "players", children); root.Initialise(service); return root; }
        private void Tick(float delta) => service.Tick(UpdateMethod.Update, delta, delta, true);
        private static void Alpha(CanvasGroupFadeTweenPlayer player, float expected) =>
            Assert.That(player.GetComponent<CanvasGroup>().alpha, Is.EqualTo(expected).Within(.0001f));

        [Test] public void LiveTransfer_PreservesPose_CancelsOldAwait_AndIgnoresOldToken()
        {
            var player = Fade(); using var token = new CancellationTokenSource();
            var old = player.PlayAsync(token.Token); Tick(.4f);
            var prepared = player.PrepareTimeline(); using var control = prepared.TakeControl();
            Alpha(player, .4f); Assert.That(control.Position, Is.EqualTo(.4f).Within(.0001f));
            Assert.That(old.IsCanceled, Is.True); token.Cancel(); Tick(.2f); Alpha(player, .4f);
            foreach (float value in new[] { .4f, .6f, .2f, .4f }) { control.Sample(value); Alpha(player, value); }
            Assert.That(control.IsActive, Is.True);
        }
        [Test] public void SamplingEndpoints_IsSilent_AndDoesNotFinishTheControl()
        {
            var player = Fade(); int played = 0, completed = 0, killed = 0;
            player.Played += () => played++; player.Completed += () => completed++; player.Killed += () => killed++;
            using var control = player.PrepareTimeline().TakeControl();
            control.Sample(1f); control.Sample(0f); control.Sample(1f);
            Assert.That((played, completed, killed), Is.EqualTo((0, 0, 0)));
            Assert.That(control.Completion.IsCompleted, Is.False);
            Assert.That(control.ResumeAsync().Status, Is.EqualTo(TaskStatus.RanToCompletion));
            Assert.That((played, completed, killed), Is.EqualTo((0, 1, 0)));
        }
        [Test] public void AutomaticResume_Regrab_AndReverseSettleUseOnlyOneWriter()
        {
            var player = Fade(); using var control = player.PrepareTimeline().TakeControl();
            control.Sample(.4f); var task = control.ResumeAsync(); Tick(.2f); Alpha(player, .6f);
            control.Sample(.5f); Tick(.2f); Alpha(player, .5f);
            var reverse = control.ResumeAsync(false); Tick(.5f); Alpha(player, 0f);
            Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
            Assert.That(reverse.Status, Is.EqualTo(TaskStatus.RanToCompletion));
        }
        [Test] public void RestoreAndReacquire_RetainTheSameCapture()
        {
            var player = Fade(); var timeline = player.PrepareTimeline();
            using (var control = timeline.TakeControl()) control.Sample(.6f);
            Alpha(player, .6f); timeline.Restore(); Alpha(player, 0f);
            using var again = timeline.TakeControl(); again.Sample(.6f); Alpha(player, .6f);
        }
        [TestCase(false)] [TestCase(true)] public void DelayEasingAndEndpointModes_AreCaptured(bool configured)
        {
            var player = Fade(); Set(player, "delay", .2f); Set(player, "easing", EaseType.QuadIn);
            if (configured) { Set(player, "endpointMode", TweenPlayerBehaviour<CanvasGroup, float>.EndpointMode.ConfiguredEndpoints); Set(player, "backwardValue", .2f); }
            using var control = player.PrepareTimeline().TakeControl();
            control.Sample(.1f); Alpha(player, configured ? .2f : 0f);
            control.Sample(.7f); Alpha(player, configured ? .4f : .25f);
            control.Sample(.3f); control.Sample(.7f); Alpha(player, configured ? .4f : .25f);
        }
        [Test] public void SequentialSameProperty_UsesVirtualPreviousEndpoint_AndRestoresOnBacktracking()
        {
            var a = Fade(.5f); var target = a.GetComponent<CanvasGroup>(); var b = Fade(1f, 1f, target);
            var sequence = Compose<TweenPlayerSequenceBehaviour>(a, b);
            using var control = sequence.PrepareTimeline().TakeControl();
            control.Sample(1.5f); Assert.That(target.alpha, Is.EqualTo(.75f).Within(.0001f));
            control.Sample(.5f); Assert.That(target.alpha, Is.EqualTo(.25f).Within(.0001f));
            control.Sample(1.5f); Assert.That(target.alpha, Is.EqualTo(.75f).Within(.0001f));
        }
        [Test] public void NestedSequenceAndGroup_PreserveParallelAndSequentialPlacement()
        {
            var a = Fade(1f, .5f); var b = Fade(1f, 1f); var c = Fade(1f, 1f);
            var group = Compose<TweenPlayerGroupBehaviour>(a, b);
            var root = Compose<TweenPlayerSequenceBehaviour>(group, c);
            using var control = root.PrepareTimeline().TakeControl();
            Assert.That(control.Duration, Is.EqualTo(2f));
            control.Sample(.25f); Alpha(a, .5f); Alpha(b, .25f); Alpha(c, 0f);
            control.Sample(1.5f); Alpha(a, 1f); Alpha(b, 1f); Alpha(c, .5f);
            control.Sample(.25f); Alpha(c, 0f);
        }
        [Test] public void LiveNestedSequenceTransfer_PreservesCurrentPlacement()
        {
            var a = Fade(1f, .5f); var b = Fade();
            var group = Compose<TweenPlayerGroupBehaviour>(b);
            var root = Compose<TweenPlayerSequenceBehaviour>(a, group);
            var old = root.PlayAsync(); Tick(.5f); Tick(.4f);
            using var control = root.PrepareTimeline().TakeControl();
            Assert.That(old.IsCanceled, Is.True); Assert.That(control.Position, Is.EqualTo(.9f).Within(.0001f));
            Alpha(b, .4f); control.Sample(.7f); Alpha(b, .2f); control.Sample(.2f); Alpha(a, .4f); Alpha(b, 0f);
        }
        [TestCase(0, .25f)] [TestCase(1, .75f)] [TestCase(2, 1.25f)]
        public void FiniteLoops_AreSampledInBothDirections(int loop, float expected)
        {
            var player = Player<TransformScaleTweenPlayer>(); player.transform.localScale = Vector3.zero;
            Set(player, "duration", 1f); Set(player, "forwardValue", Vector3.one); Set(player, "loopAmount", 2);
            Set(player, "loopType", (TweenPlayerBehaviour<Transform, Vector3>.LoopType)loop);
            using var control = player.PrepareTimeline().TakeControl();
            control.Sample(1.25f); Assert.That(player.transform.localScale.x, Is.EqualTo(expected).Within(.0001f));
            control.Sample(.2f); control.Sample(1.25f); Assert.That(player.transform.localScale.x, Is.EqualTo(expected).Within(.0001f));
        }
        [Test] public void ZeroDuration_UsesExistingMinimum_AndEmptyCompositionCompletesSynchronously()
        {
            var player = Fade(1f, 0f); using (var control = player.PrepareTimeline().TakeControl())
            { Assert.That(control.Duration, Is.EqualTo(.0001f)); control.Sample(control.Duration); Alpha(player, 1f); }
            var empty = Compose<TweenPlayerSequenceBehaviour>(); using var operation = empty.PrepareTimeline().TakeControl();
            Assert.That(operation.ResumeAsync().IsCompletedSuccessfully, Is.True);
        }
        [Test] public void ParallelConflictsCyclesAndMixedClocks_AreRejectedBeforeMutation()
        {
            var a = Fade(); var b = Fade(1f, 1f, a.GetComponent<CanvasGroup>());
            var group = Compose<TweenPlayerGroupBehaviour>(a, b);
            Assert.That(group.CanPrepareTimeline(out var conflict), Is.False); Assert.That(conflict, Does.Contain("same target")); Alpha(a, 0f);
            Set(group, "players", new TweenPlayerBehaviour[] { group });
            Assert.That(group.CanPrepareTimeline(out var cycle), Is.False); Assert.That(cycle, Does.Contain("cycle"));
            b = Fade(); Set(b, "unscaledTime", true); Set(group, "players", new TweenPlayerBehaviour[] { a, b });
            Assert.That(group.CanPrepareTimeline(out var clock), Is.False); Assert.That(clock, Does.Contain("clock")); Alpha(a, 0f);
        }
        [Test] public void InfiniteLoopAndAnimator_AreUnsupported_WithoutChangingLegacyPlayback()
        {
            var player = Fade(); Set(player, "loopAmount", 0);
            Assert.That(player.CanPrepareTimeline(out _), Is.False); player.Play(); Tick(.4f); Alpha(player, .4f);
            var animator = Player<AnimatorTweenPlayerBehaviour>(); Assert.That(animator.CanPrepareTimeline(out _), Is.False);
        }
        [Test] public void ReplacementKillAndStaleControl_SettleOnlyTheirOwnExecution()
        {
            var player = Fade(); var timeline = player.PrepareTimeline(); var old = timeline.TakeControl();
            old.Sample(.4f); var replacement = player.PlayAsync();
            Assert.That(old.Completion.IsCanceled, Is.True); old.Dispose();
            Assert.Throws<InvalidOperationException>(() => old.Sample(.9f));
            Assert.That(replacement.IsCompleted, Is.False);
            player.Kill(); Assert.That(replacement.IsCanceled, Is.True);
            var control = player.PrepareTimeline().TakeControl();
            player.Kill(); Assert.That(control.Completion.IsCanceled, Is.True);
        }
        [Test] public void BackwardCommandIsDistinctFromReversingTheForwardCurve()
        {
            var player = Fade(); Set(player, "endpointMode", TweenPlayerBehaviour<CanvasGroup, float>.EndpointMode.ConfiguredEndpoints);
            Set(player, "easing", EaseType.QuadIn);
            using (var forward = player.PrepareTimeline(true).TakeControl()) { forward.Sample(.75f); Alpha(player, .5625f); }
            using var backward = player.PrepareTimeline(false).TakeControl(); backward.Sample(.25f); Alpha(player, .9375f);
        }
        [Test] public void NonfiniteSample_DoesNotStopTheAutomaticClock()
        {
            var player = Fade(); using var control = player.PrepareTimeline().TakeControl(); control.ResumeAsync();
            Assert.Throws<ArgumentOutOfRangeException>(() => control.Sample(float.NaN));
            Tick(.4f); Alpha(player, .4f);
        }

        [Test] public void LiveGroupOfSequences_FreezesObservedFrameBoundaryHoldsWithoutJump()
        {
            var a = Fade(1f, .3f); var b = Fade(); var c = Fade(1f, .4f); var d = Fade();
            var first = Compose<TweenPlayerSequenceBehaviour>(a, b);
            var second = Compose<TweenPlayerSequenceBehaviour>(c, d);
            var root = Compose<TweenPlayerGroupBehaviour>(first, second);
            var old = root.PlayAsync(); Tick(.35f); Tick(.1f); Tick(.2f);
            float poseB = b.GetComponent<CanvasGroup>().alpha, poseD = d.GetComponent<CanvasGroup>().alpha;
            using var control = root.PrepareTimeline().TakeControl();
            Assert.That(old.IsCanceled, Is.True);
            control.Sample(control.Position); Alpha(b, poseB); Alpha(d, poseD);
            float position = control.Position; control.Sample(.1f); control.Sample(position); Alpha(b, poseB); Alpha(d, poseD);
        }
        [Test] public void LiveFiniteLoops_RetainObservedBoundaryHoldsAndTheOriginalEndpoints()
        {
            var player = Fade(); Set(player, "loopAmount", 2);
            player.Play(); Tick(1.2f); Tick(.4f);
            using var control = player.PrepareTimeline().TakeControl();
            control.Sample(control.Position); Alpha(player, .4f);
            control.Sample(.2f); Alpha(player, .2f);
            control.Sample(1.6f); Alpha(player, .4f);
        }

        [Test] public void TransferObserverReplacement_IsPreservedAndTheFailedTransferOwnsNothing()
        {
            var player = Fade(); player.Play(); Tick(.4f); var snapshot = player.PrepareTimeline();
            Task replacement = null; player.Killed += () => { if (replacement == null) replacement = player.PlayAsync(); };
            Assert.Throws<InvalidOperationException>(() => snapshot.TakeControl());
            Assert.That(replacement, Is.Not.Null); Assert.That(replacement.IsCompleted, Is.False);
            Tick(1f); Assert.That(replacement.IsCompletedSuccessfully, Is.True);
        }

        [Test] public void AnotherPlayerOfTheSameProperty_CannotCompeteWithAnOwnedControl()
        {
            var a = Fade(); var b = Fade(1f, 1f, a.GetComponent<CanvasGroup>());
            using var first = a.PrepareTimeline().TakeControl(); first.Sample(.4f);
            Assert.Throws<InvalidOperationException>(() => b.PrepareTimeline().TakeControl());
            b.Play(); Assert.That(first.Completion.IsCanceled, Is.True);
            Assert.Throws<InvalidOperationException>(() => a.PrepareTimeline().TakeControl());
            b.Kill(); using var next = a.PrepareTimeline().TakeControl(); next.Sample(.8f); Alpha(a, .88f);
        }

        [Test] public void AnchoredPositionAndScaleOnTheSameTarget_AreDistinctProperties()
        {
            var position = Player<RectTransformAnchoredPositionTweenPlayer>();
            var scale = position.gameObject.AddComponent<TransformScaleTweenPlayer>(); scale.Initialise(service);
            Set(position, "duration", 1f); Set(scale, "duration", 1f);
            Set(position, "forwardValue", new Vector2(100f, 200f)); Set(scale, "forwardValue", Vector3.one * 2f);
            var root = Compose<TweenPlayerGroupBehaviour>(position, scale);
            using var control = root.PrepareTimeline().TakeControl(); control.Sample(.5f);
            Assert.That(position.GetComponent<RectTransform>().anchoredPosition, Is.EqualTo(new Vector2(50f, 100f)));
            Assert.That(scale.transform.localScale, Is.EqualTo(Vector3.one * 1.5f));
        }

        [Test] public void LargeFiniteLoopCount_DoesNotAllocateAnArrayPerFutureLoop()
        {
            var player = Fade(); Set(player, "loopAmount", int.MaxValue);
            using var control = player.PrepareTimeline().TakeControl(); control.Sample(.25f); Alpha(player, .25f);
        }

        [Test] public void TimelineTiming_RejectsNonfiniteRangesAndInvalidObservedSpans()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TweenTimelineTiming.ClampTime(0f, float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => TweenTimelineTiming.Locate(.2f, 1f, 0f, new[] { .5f }, 2, out _, out _));
            Assert.Throws<ArgumentOutOfRangeException>(() => TweenTimelineTiming.Locate(float.PositiveInfinity, 1f, 0f, Array.Empty<float>(), 2, out _, out _));
            TweenTimelineTiming.Locate(1.6f, 1f, 0f, new[] { 1.2f }, 2, out int cycle, out float progress);
            Assert.That(cycle, Is.EqualTo(1)); Assert.That(progress, Is.EqualTo(.4f).Within(.0001f));
        }

        [Test] public void FractionalDurationAndLargeCoordinates_AllowNormalFloatingPointAccumulation()
        {
            var player = Player<RectTransformAnchoredPositionTweenPlayer>();
            Set(player, "duration", 1.4f); Set(player, "forwardValue", new Vector2(3000, 2000));
            player.Play(); for (int i = 0; i < 60; i++) Tick(.01f);
            var before = player.GetComponent<RectTransform>().anchoredPosition;
            using var control = player.PrepareTimeline().TakeControl();
            Assert.That(player.GetComponent<RectTransform>().anchoredPosition, Is.EqualTo(before));
            control.Sample(control.Position);
            Assert.That(Vector2.Distance(player.GetComponent<RectTransform>().anchoredPosition, before), Is.LessThan(.005f));
        }

        [Test] public void CustomCurveCapture_IsUnaffectedByLaterAuthoringChanges()
        {
            var player = Fade(); var curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            Set(player, "hasCustomCurve", true); Set(player, "customCurve", curve);
            float expected = curve.Evaluate(.4f);
            using var control = player.PrepareTimeline().TakeControl();
            curve.keys = AnimationCurve.Linear(0, 0, 1, 1).keys;
            control.Sample(.4f); Alpha(player, expected);
            control.Sample(.8f); control.Sample(.4f); Alpha(player, expected);
        }

        [Test] public void ResumeTokenCancellationWhileRegrabbed_CancelsOnlyThatControl()
        {
            var player = Fade(); var prepared = player.PrepareTimeline();
            using var token = new CancellationTokenSource();
            var control = prepared.TakeControl(); var task = control.ResumeAsync(ct: token.Token);
            control.Sample(.4f); token.Cancel();
            Assert.That(task.IsCanceled, Is.True); Assert.That(control.Completion.IsCanceled, Is.True);
            using var replacement = prepared.TakeControl(); token.Cancel(); replacement.Sample(.6f); Alpha(player, .6f);
        }

        [Test] public void CompletionObserverFailure_DoesNotLeaveControlWaitersPending()
        {
            var player = Fade(); var error = new InvalidOperationException("Terminal observer");
            player.Completed += () => throw error;
            using var control = player.PrepareTimeline().TakeControl(); var task = control.ResumeAsync();
            Assert.That(Assert.Throws<InvalidOperationException>(() => Tick(1f)), Is.SameAs(error));
            Assert.That(task.IsCompletedSuccessfully, Is.True); Assert.That(control.Completion.IsCompletedSuccessfully, Is.True);
        }
    }
}
