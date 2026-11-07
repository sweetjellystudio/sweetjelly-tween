namespace SWeetJelly.Tween.Tests.EditMode
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using SWeetJelly.Tween.App;
    using SWeetJelly.Tween.Contract;
    using SWeetJelly.Tween.Framework.Unity;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public sealed class TweenPlayerAsyncTest
    {
        private GameObject holder;
        private CanvasGroupFadeTweenPlayer player;
        private TweenService service;

        [SetUp] public void SetUp()
        {
            holder = new GameObject("Async property", typeof(CanvasGroup));
            holder.SetActive(false);
            player = holder.AddComponent<CanvasGroupFadeTweenPlayer>();
            service = new TweenService();
            player.Initialise(service);
            Set(player, "duration", 1f);
            holder.SetActive(true);
        }
        [TearDown] public void TearDown() { service.Dispose(); Object.DestroyImmediate(holder); }

        internal static void Set(object target, string name, object value)
        {
            for (Type type = target.GetType(); type != null; type = type.BaseType)
            {
                var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
                if (field == null) continue;
                field.SetValue(target, value); return;
            }
            throw new MissingFieldException(name);
        }
        private void Tick() => service.Tick(UpdateMethod.Update, 1f, 1f, true);

        [Test] public void NormalPlayback_CompletesItsTask()
        {
            var task = player.PlayAsync();
            Assert.That(task.IsCompleted, Is.False);
            Tick();
            Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
        }
        [Test] public void Replacement_CancelsOldTask_AndOldTokenCannotKillNewRun()
        {
            using var cancellation = new CancellationTokenSource();
            var old = player.PlayAsync(cancellation.Token);
            var next = player.PlayAsync(true);
            Assert.That(old.IsCanceled, Is.True);
            cancellation.Cancel();
            Assert.That(player.IsPlaying, Is.True);
            Tick();
            Assert.That(next.Status, Is.EqualTo(TaskStatus.RanToCompletion));
        }
        [Test] public void SynchronousReplacement_CancelsOnlyTheOldAwait()
        {
            using var cancellation = new CancellationTokenSource();
            var old = player.PlayAsync(cancellation.Token);
            player.PlayForward();
            cancellation.Cancel();
            Assert.That(old.IsCanceled, Is.True);
            Assert.That(player.IsPlaying, Is.True);
        }
        [Test] public void AlreadyCanceledToken_DoesNotReplaceRunningPlayback()
        {
            var current = player.PlayAsync();
            var rejected = player.PlayAsync(new CancellationToken(true));
            Assert.That(rejected.IsCanceled, Is.True);
            Tick();
            Assert.That(current.Status, Is.EqualTo(TaskStatus.RanToCompletion));
        }
        [Test] public void TokenCancellation_PreservesToken_AndStopsOwnedRun()
        {
            using var cancellation = new CancellationTokenSource();
            var task = player.PlayAsync(cancellation.Token);
            cancellation.Cancel();
            Assert.That(task.IsCanceled, Is.True);
            Assert.That(player.IsPlaying, Is.False);
            var error = Assert.Throws<TaskCanceledException>(() => task.GetAwaiter().GetResult());
            Assert.That(error.CancellationToken, Is.EqualTo(cancellation.Token));
        }
        [TestCase(true)] [TestCase(false)] public void Snap_CancelsTheExistingAwait(bool forward)
        {
            var task = player.PlayAsync();
            player.Snap(forward);
            Assert.That(task.IsCanceled, Is.True);
            Assert.That(player.IsPlaying, Is.False);
        }
        [Test] public void BackwardWithoutOrigin_CompletesSynchronously()
        {
            Assert.That(player.PlayAsync(false).Status, Is.EqualTo(TaskStatus.RanToCompletion));
        }
        [Test] public void FiniteLoop_AwaitsAllLoops()
        {
            Set(player, "loopAmount", 3);
            var task = player.PlayAsync();
            Tick(); Assert.That(task.IsCompleted, Is.False);
            Tick(); Assert.That(task.IsCompleted, Is.False);
            Tick(); Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
        }
        [Test] public void InfiniteLoop_AwaitsCancellation()
        {
            Set(player, "loopAmount", 0);
            var task = player.PlayAsync();
            Tick(); Tick();
            Assert.That(task.IsCompleted, Is.False);
            player.Kill(); Assert.That(task.IsCanceled, Is.True);
        }
        [Test] public void CompletedObserverFailure_DoesNotRewriteTaskResult()
        {
            player.Completed += () => throw new InvalidOperationException("observer");
            var task = player.PlayAsync();
            Assert.Throws<InvalidOperationException>(Tick);
            Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
        }
        [Test] public void PlayedFailure_StopsStartedRun_AndPropagatesSynchronously()
        {
            player.Played += () => throw new InvalidOperationException("played");
            Assert.Throws<InvalidOperationException>(() => player.PlayAsync());
            Assert.That(player.IsPlaying, Is.False);
        }
        [Test] public void PlayedReentry_ReturnsTheOriginalRun_NotTheReplacement()
        {
            bool replayed = false;
            Task next = null;
            player.Played += () => { if (!replayed) { replayed = true; next = player.PlayAsync(); } };
            var original = player.PlayAsync();
            Assert.That(original.IsCanceled, Is.True);
            Tick(); Assert.That(next.Status, Is.EqualTo(TaskStatus.RanToCompletion));
        }
        [Test] public void CompletedReentry_OldTokenCannotStopTheNewPlayback()
        {
            using var cancellation = new CancellationTokenSource();
            bool replayed = false;
            Task next = null;
            player.Completed += () => { if (!replayed) { replayed = true; next = player.PlayAsync(); } };
            var original = player.PlayAsync(cancellation.Token);
            Tick(); cancellation.Cancel();
            Assert.That(original.Status, Is.EqualTo(TaskStatus.RanToCompletion));
            Assert.That(next.IsCompleted, Is.False);
            Tick(); Assert.That(next.Status, Is.EqualTo(TaskStatus.RanToCompletion));
        }
        [Test] public void MissingInjection_FailsWithoutLeavingPendingPlayback()
        {
            var target = new GameObject("Not injected", typeof(CanvasGroup));
            try
            {
                var uninitialised = target.AddComponent<CanvasGroupFadeTweenPlayer>();
                Assert.Throws<InvalidOperationException>(() => uninitialised.PlayAsync());
                uninitialised.Initialise(service);
                Assert.That(uninitialised.IsPlaying, Is.False);
            }
            finally { Object.DestroyImmediate(target); }
        }
        [Test] public void LegacySubclass_StillPlays_ButAsyncIsExplicitlyUnsupported()
        {
            var legacy = holder.AddComponent<CompositionTestPlayer>();
            legacy.Play(); Assert.That(legacy.IsPlaying, Is.True);
            Assert.Throws<NotSupportedException>(() => legacy.PlayAsync());
        }
        [TestCase(false)] [TestCase(true)] public void Composition_ExternalChildReplacement_CancelsParentWithoutKillingNewChild(bool sequential)
        {
            var root = new GameObject("Composition");
            try
            {
                TweenPlayerBehaviour parent = sequential ? root.AddComponent<TweenPlayerSequenceBehaviour>() : root.AddComponent<TweenPlayerGroupBehaviour>();
                Set(parent, "players", new TweenPlayerBehaviour[] { player });
                var task = parent.PlayAsync();
                var replacement = player.PlayAsync();
                Assert.That(task.IsCanceled, Is.True);
                parent.Kill();
                Assert.That(player.IsPlaying, Is.True);
                Tick(); Assert.That(replacement.Status, Is.EqualTo(TaskStatus.RanToCompletion));
            }
            finally { Object.DestroyImmediate(root); }
        }
        [TestCase(false)] [TestCase(true)] public void EmptyComposition_CompletesSynchronously(bool sequential)
        {
            var root = new GameObject("Empty composition");
            try
            {
                TweenPlayerBehaviour parent = sequential ? root.AddComponent<TweenPlayerSequenceBehaviour>() : root.AddComponent<TweenPlayerGroupBehaviour>();
                Assert.That(parent.PlayAsync().Status, Is.EqualTo(TaskStatus.RanToCompletion));
            }
            finally { Object.DestroyImmediate(root); }
        }
        [TestCase(false)] [TestCase(true)] public void CompositionSnap_CancelsOldAwait_DespiteLegacyCompletedEvent(bool sequential)
        {
            var root = new GameObject("Snap composition");
            try
            {
                TweenPlayerBehaviour parent = sequential ? root.AddComponent<TweenPlayerSequenceBehaviour>() : root.AddComponent<TweenPlayerGroupBehaviour>();
                Set(parent, "players", new TweenPlayerBehaviour[] { player });
                var task = parent.PlayAsync();
                int completed = 0; parent.Completed += () => completed++;
                parent.SnapForward();
                Assert.That(task.IsCanceled, Is.True);
                Assert.That(completed, Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(root); }
        }
        [TestCase(false)] [TestCase(true)] public void CompositionCancellation_StopsItsOwnedChildren(bool sequential)
        {
            var root = new GameObject("Cancel composition");
            try
            {
                TweenPlayerBehaviour parent = sequential ? root.AddComponent<TweenPlayerSequenceBehaviour>() : root.AddComponent<TweenPlayerGroupBehaviour>();
                Set(parent, "players", new TweenPlayerBehaviour[] { player });
                using var cancellation = new CancellationTokenSource();
                var task = parent.PlayAsync(cancellation.Token);
                cancellation.Cancel();
                Assert.That(task.IsCanceled, Is.True);
                Assert.That(player.IsPlaying, Is.False);
            }
            finally { Object.DestroyImmediate(root); }
        }
        [Test] public void RuntimeValueFailure_CancelsAwait_AndPreservesTickException()
        {
            var failing = holder.AddComponent<FailingAsyncTestPlayer>();
            failing.Initialise(service);
            var task = failing.PlayAsync();
            Assert.Throws<InvalidOperationException>(Tick);
            Assert.That(task.IsCanceled, Is.True);
            Assert.That(failing.IsPlaying, Is.False);
        }
        [TestCase(false)] [TestCase(true)] public void ChildKillObserver_CanReplayParent_WithoutOldCleanupKillingReplacement(bool sequential)
        {
            var root = new GameObject("Reentrant parent");
            try
            {
                TweenPlayerBehaviour parent = sequential ? root.AddComponent<TweenPlayerSequenceBehaviour>() : root.AddComponent<TweenPlayerGroupBehaviour>();
                Set(parent, "players", new TweenPlayerBehaviour[] { player });
                Task next = null;
                bool replayed = false;
                player.Killed += () => { if (!replayed) { replayed = true; next = parent.PlayAsync(); } };
                var old = parent.PlayAsync();
                parent.Kill();
                Assert.That(old.IsCanceled, Is.True);
                Assert.That(next, Is.Not.Null);
                Assert.That(player.IsPlaying, Is.True);
                Tick(); Assert.That(next.Status, Is.EqualTo(TaskStatus.RanToCompletion));
            }
            finally { Object.DestroyImmediate(root); }
        }
        [TestCase(false)] [TestCase(true)] public void SynchronousComposition_PreservesPendingChildUntilInjection(bool sequential)
        {
            var root = new GameObject("Pending composition");
            var childObject = new GameObject("Pending child", typeof(CanvasGroup));
            try
            {
                var child = childObject.AddComponent<CanvasGroupFadeTweenPlayer>();
                TweenPlayerBehaviour parent = sequential ? root.AddComponent<TweenPlayerSequenceBehaviour>() : root.AddComponent<TweenPlayerGroupBehaviour>();
                Set(parent, "players", new TweenPlayerBehaviour[] { child });
                int completed = 0; parent.Completed += () => completed++;
                UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Tween service is not injected"));
                parent.Play();
                Assert.That(parent.IsPlaying, Is.True);
                Assert.That(child.IsPlaying, Is.False);
                parent.Initialise(service);
                Tick();
                Assert.That(completed, Is.EqualTo(1));
                Assert.That(parent.IsPlaying, Is.False);
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(childObject); }
        }
        [TestCase(false)] [TestCase(true)] public void ParentTerminalObserverFailure_DoesNotStarveChildTerminalObservers(bool sequential)
        {
            var root = new GameObject("Terminal notification parent");
            try
            {
                TweenPlayerBehaviour parent = sequential ? root.AddComponent<TweenPlayerSequenceBehaviour>() : root.AddComponent<TweenPlayerGroupBehaviour>();
                Set(parent, "players", new TweenPlayerBehaviour[] { player });
                parent.Completed += () => throw new InvalidOperationException("parent observer");
                int childCompleted = 0; player.Completed += () => childCompleted++;
                var task = parent.PlayAsync();
                Assert.Throws<InvalidOperationException>(Tick);
                Assert.That(task.Status, Is.EqualTo(TaskStatus.RanToCompletion));
                Assert.That(childCompleted, Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(root); }
        }
        [Test] public void OptInSubclass_IgnoresDelayedOldCompletion()
        {
            var custom = holder.AddComponent<RunAwareTestPlayer>();
            var old = custom.PlayAsync();
            var lateCompletion = custom.CompleteLater;
            var next = custom.PlayAsync();
            lateCompletion();
            Assert.That(old.IsCanceled, Is.True);
            Assert.That(next.IsCompleted, Is.False);
            custom.CompleteLater();
            Assert.That(next.Status, Is.EqualTo(TaskStatus.RanToCompletion));
        }
    }

    public sealed class FailingAsyncTestPlayer : TweenPlayerBehaviour<CanvasGroup, float>
    {
        protected override float CurrentValue { get => TargetComponent.alpha; set => TargetComponent.alpha = value; }
        protected override TweenCase CreateTweenCase(float endValue) => TweenCases.DOFloat(0, 1, 1, _ => throw new InvalidOperationException("value application"));
        protected override void IncrementLoopChangeValues() { }
    }

    public sealed class RunAwareTestPlayer : TweenPlayerBehaviour
    {
        public Action CompleteLater;
        protected override bool SupportsAsyncPlayback => true;
        public override bool IsPlaying => CurrentPlayback != null;
        public override void Play()
        {
            Kill();
            var run = BeginPlayback();
            CompleteLater = () => { if (IsCurrentPlayback(run)) { CompletePlayback(run); NotifyCompleted(); } };
            NotifyPlayed();
        }
        public override void Play(bool forward) => Play();
        public override void PlayForward() => Play();
        public override void PlayBackward() => Play();
        public override void Snap(bool forward) => Kill();
        public override void SnapForward() => Kill();
        public override void SnapBackward() => Kill();
        public override void Kill()
        {
            var run = CurrentPlayback;
            if (run == null) return;
            CancelPlayback(run); NotifyKilled();
        }
        public override void SetPlayOnEnable(bool playOnEnable) { }
        private void OnDisable() => Kill();
    }
}
