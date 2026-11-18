namespace SWeetJelly.Tween.Tests.PlayMode
{
    using System;
    using System.Collections;
    using System.Threading;
    using NUnit.Framework;
    using SWeetJelly.Tween.App;
    using SWeetJelly.Tween.Contract;
    using SWeetJelly.Tween.Framework.Unity;
    using UnityEngine;
    using UnityEngine.TestTools;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    public sealed class UITKTweenPlayerTest
    {
        private GameObject holder;
        private PanelSettings settings;
        private ThemeStyleSheet theme;
        private RenderTexture texture;
        private VisualElement root, a, b;
        private TweenService service;
        private UITKTweenAsset asset;
        private readonly System.Collections.Generic.List<UITKTweenPlayer> players = new();

        [UnitySetUp] public IEnumerator SetUp()
        {
            service = new TweenService();
            asset = ScriptableObject.CreateInstance<UITKTweenAsset>();
            asset.Root = Leaf();
            settings = ScriptableObject.CreateInstance<PanelSettings>();
            theme = ScriptableObject.CreateInstance<ThemeStyleSheet>(); settings.themeStyleSheet = theme;
            texture = new RenderTexture(320, 180, 0); settings.targetTexture = texture;
            holder = new GameObject("UITK asset test"); holder.SetActive(false);
            var document = holder.AddComponent<UIDocument>(); document.panelSettings = settings;
            holder.SetActive(true); yield return null;
            root = document.rootVisualElement;
            a = Copy(); b = Copy(); root.Add(a); root.Add(b);
            yield return null;
        }

        [UnityTearDown] public IEnumerator TearDown()
        {
            foreach (var player in players) player.Dispose(); players.Clear();
            service.Dispose(); Object.Destroy(holder); Object.Destroy(settings); Object.Destroy(texture); Object.Destroy(theme); Object.Destroy(asset);
            yield return null;
        }

        private static VisualElement Copy()
        {
            var copy = new VisualElement(); copy.style.width = 100; copy.style.height = 60;
            var title = new VisualElement { name = "title" }; title.style.width = 80; title.style.height = 20;
            title.style.opacity = 1; copy.Add(title); return copy;
        }
        private static UITKTweenNode Leaf(float from = 0, float to = 1) => new()
        { TargetName = "title", Property = UITKTweenProperty.Opacity, FromCurrent = false, From = new Vector2(from, 0), To = new Vector2(to, 0), Duration = 1 };
        private UITKTweenPlayer Bind(VisualElement target = null, UpdateMethod? lane = null, bool? unscaled = null)
        { var player = asset.Bind(target ?? a, service, lane, unscaled); players.Add(player); return player; }
        private void Tick(float scaled = 1, float unscaled = 1, UpdateMethod lane = UpdateMethod.Update) => service.Tick(lane, scaled, unscaled, true);
        private float Alpha(VisualElement copy) => copy.Q("title").style.opacity.value;

        [Test] public void SharedAsset_CopiesHaveIndependentCaptureExecutionAndCancellation()
        {
            asset.Root.FromCurrent = true; asset.Root.To = Vector2.zero;
            b.Q("title").style.opacity = .6f;
            using var pa = Bind(a); using var pb = Bind(b);
            var wa = pa.PlayAsync(); var wb = pb.PlayAsync(); Tick(.5f, .5f);
            Assert.That(Alpha(a), Is.EqualTo(.5f).Within(.001)); Assert.That(Alpha(b), Is.EqualTo(.3f).Within(.001));
            pa.Kill(); Assert.That(wa.IsCanceled, Is.True); Assert.That(wb.IsCompleted, Is.False);
            Tick(.5f, .5f); Assert.That(wb.IsCompletedSuccessfully, Is.True);
        }

        [Test] public void Sequence_CurrentStartUsesPrecedingCalculatedValue_AndRevisitsAreDeterministic()
        {
            var second = Leaf(0, 1); second.FromCurrent = true;
            asset.Root = new UITKTweenNode { Kind = UITKTweenNodeKind.Sequence, Children = new() { Leaf(.2f, .6f), second } };
            using var player = Bind(); var timeline = player.PrepareTimeline(); using var control = timeline.TakeControl();
            control.Sample(1.5f); Assert.That(Alpha(a), Is.EqualTo(.8f).Within(.001));
            control.Sample(.5f); Assert.That(Alpha(a), Is.EqualTo(.4f).Within(.001));
            control.Sample(1.5f); Assert.That(Alpha(a), Is.EqualTo(.8f).Within(.001));
            Assert.That(control.Completion.IsCompleted, Is.False);
        }

        [Test] public void PreparedCurveAndEndpointsAreFrozenUntilExplicitReprepare()
        {
            asset.Root.UseCurve = true; asset.Root.Curve = AnimationCurve.Linear(0, 0, 1, 1);
            using var player = Bind(); var timeline = player.PrepareTimeline();
            asset.Root.To = Vector2.zero; asset.Root.Curve = AnimationCurve.Linear(0, 1, 1, 0);
            using (var control = timeline.TakeControl()) { control.Sample(.3f); Assert.That(Alpha(a), Is.EqualTo(.3f).Within(.001)); }
            timeline.Restore(); Assert.That(Alpha(a), Is.EqualTo(1));
            using (var control = timeline.TakeControl()) { control.Sample(.3f); Assert.That(Alpha(a), Is.EqualTo(.3f).Within(.001)); }
            player.PrepareTimeline(); Assert.Throws<InvalidOperationException>(() => timeline.TakeControl());
        }

        [Test] public void ReplacementDetachesOldToken_AndPreCanceledStartDoesNotReplace()
        {
            using var player = Bind(); using var oldToken = new CancellationTokenSource(); using var canceled = new CancellationTokenSource();
            var old = player.PlayAsync(oldToken.Token); Tick(.2f, .2f); canceled.Cancel();
            var rejected = player.PlayAsync(canceled.Token); Assert.That(rejected.IsCanceled, Is.True); Assert.That(old.IsCompleted, Is.False);
            var current = player.PlayAsync(); Assert.That(old.IsCanceled, Is.True); oldToken.Cancel();
            Assert.That(current.IsCompleted, Is.False); Tick(); Assert.That(current.IsCompletedSuccessfully, Is.True);
        }

        [Test] public void ControlTransferCancelsOldAwait_EndpointSampleStaysPending_ResumeCompletes()
        {
            using var player = Bind(); var old = player.PlayAsync(); Tick(.3f, .3f);
            var timeline = player.PrepareTimeline(); using var control = timeline.TakeControl();
            Assert.That(old.IsCanceled, Is.True); Assert.That(control.Position, Is.EqualTo(.3f).Within(.001));
            control.SampleProgress(1); Assert.That(control.Completion.IsCompleted, Is.False);
            var done = control.ResumeAsync(); Assert.That(done.IsCompletedSuccessfully, Is.True); Assert.That(player.IsPlaying, Is.False);
        }

        [Test] public void PauseKeepsExecutionAndTokenOwnership()
        {
            using var player = Bind(); using var control = player.PrepareTimeline().TakeControl(); using var token = new CancellationTokenSource();
            var wait = control.ResumeAsync(ct: token.Token); Tick(.2f, .2f); control.Pause(); Tick();
            Assert.That(Alpha(a), Is.EqualTo(.2f).Within(.001)); Assert.That(wait.IsCompleted, Is.False);
            token.Cancel(); Assert.That(wait.IsCanceled, Is.True); Assert.That(player.IsPlaying, Is.False);
        }

        [Test] public void WriterConflictFailsBeforeMutatingOtherExecution_DisjointPropertiesCanRun()
        {
            using var first = Bind(); using var second = Bind();
            var running = first.PlayAsync(); Tick(.2f, .2f);
            Assert.Throws<InvalidOperationException>(() => second.PlayAsync());
            Assert.That(running.IsCompleted, Is.False); Assert.That(Alpha(a), Is.EqualTo(.2f).Within(.001));
            asset.Root.Property = UITKTweenProperty.Scale; asset.Root.From = Vector2.one; asset.Root.To = Vector2.one * 2;
            using var scale = Bind(); var scaled = scale.PlayAsync(); Tick();
            Assert.That(running.IsCompletedSuccessfully && scaled.IsCompletedSuccessfully, Is.True);
        }

        [Test] public void InvalidGraphsDoNotWriteAnyTargets()
        {
            var one = Leaf(); var two = Leaf();
            asset.Root = new UITKTweenNode { Kind = UITKTweenNodeKind.Group, Children = new() { one, two } };
            using var player = Bind(); Assert.Throws<NotSupportedException>(() => player.PlayAsync()); Assert.That(Alpha(a), Is.EqualTo(1));
            asset.Root.Children.Remove(two); asset.Root.Children.Add(asset.Root);
            Assert.Throws<NotSupportedException>(() => player.PrepareTimeline());
            asset.Root.Children.Clear(); asset.Root.Children.Add(Leaf()); asset.Root.Children.Add(Leaf()); asset.Root.Children[1].TargetName = "renamed";
            Assert.Throws<InvalidOperationException>(() => player.PlayAsync()); Assert.That(Alpha(a), Is.EqualTo(1));
        }

        [UnityTest] public IEnumerator DuplicateNamesAndDetachedStartAreRejected()
        {
            a.Add(new VisualElement { name = "title" }); yield return null;
            using var player = Bind(); Assert.Throws<InvalidOperationException>(() => player.PlayAsync());
            using var detached = Bind(new VisualElement()); Assert.Throws<InvalidOperationException>(() => detached.PlayAsync());
        }

        [UnityTest] public IEnumerator DetachInvalidatesParkedControlAndCapture_ExplicitRebindStartsFresh()
        {
            using var player = Bind(); var timeline = player.PrepareTimeline(); using var control = timeline.TakeControl();
            control.Sample(.4f); var wait = control.Completion;
            a.RemoveFromHierarchy(); Assert.That(wait.IsCanceled, Is.True);
            root.Add(a); yield return null;
            Assert.Throws<InvalidOperationException>(() => player.PlayAsync());
            player.Rebind(b); Assert.Throws<InvalidOperationException>(() => timeline.TakeControl());
            var current = player.PlayAsync(); Tick(); Assert.That(current.IsCompletedSuccessfully, Is.True);
            Assert.That(Alpha(a), Is.EqualTo(.4f).Within(.001));
        }

        [UnityTest] public IEnumerator TargetLossCancelsWithoutLosingTheRoot()
        {
            using var player = Bind(); var wait = player.PlayAsync(); a.Q("title").RemoveFromHierarchy();
            Assert.That(wait.IsCanceled, Is.True); Assert.That(a.panel, Is.Not.Null); yield return null;
        }

        [Test] public void ImmediateApplyCancelsAwait_AndRestorePreservesAbsentInlineStyles()
        {
            var element = a.Q("title"); element.style.opacity = StyleKeyword.Null;
            element.style.translate = StyleKeyword.Null; element.style.scale = StyleKeyword.Null;
            asset.Root = new UITKTweenNode { Kind = UITKTweenNodeKind.Group, Children = new() { Leaf(), new UITKTweenNode
                { TargetName = "title", Property = UITKTweenProperty.Translate, FromCurrent = false, To = new Vector2(10, 20) }, new UITKTweenNode
                { TargetName = "title", Property = UITKTweenProperty.Scale, FromCurrent = false, From = Vector2.one, To = Vector2.one * 2 } } };
            using var player = Bind(); var pending = player.PlayAsync(); player.ApplyEnd(); Assert.That(pending.IsCanceled, Is.True);
            Assert.That(element.style.translate.value.x.value, Is.EqualTo(10));
            player.Restore(); Assert.That(element.style.opacity.keyword, Is.EqualTo(StyleKeyword.Null));
            Assert.That(element.style.translate.keyword, Is.EqualTo(StyleKeyword.Null)); Assert.That(element.style.scale.keyword, Is.EqualTo(StyleKeyword.Null));
            player.ApplyStart(); Assert.That(element.style.opacity.value, Is.EqualTo(0));
        }

        [Test] public void ClockOverrideAppliesToWholeGraph_AndUnscaledAdvancesAtScaledZero()
        {
            using var player = Bind(lane: UpdateMethod.FixedUpdate, unscaled: true); var wait = player.PlayAsync();
            Tick(0, .5f); Assert.That(Alpha(a), Is.Zero);
            Tick(0, .5f, UpdateMethod.FixedUpdate); Assert.That(Alpha(a), Is.EqualTo(.5f).Within(.001));
            Tick(0, .5f, UpdateMethod.FixedUpdate); Assert.That(wait.IsCompletedSuccessfully, Is.True);
        }

        [Test] public void ReverseRetracesCurve_AndSamplingMatchesOrdinaryPlayback()
        {
            asset.Root.Easing = EaseType.QuadIn; using var player = Bind(); var wait = player.PlayAsync(false);
            Tick(.25f, .25f); Assert.That(Alpha(a), Is.EqualTo(.5625f).Within(.001));
            using var control = player.PrepareTimeline().TakeControl(); Assert.That(wait.IsCanceled, Is.True);
            control.Sample(.75f); Assert.That(Alpha(a), Is.EqualTo(.5625f).Within(.001));
            control.Sample(.25f); Assert.That(Alpha(a), Is.EqualTo(.0625f).Within(.001));
        }

        [TestCase(UITKTweenLoop.Repeat, 1f)]
        [TestCase(UITKTweenLoop.Yoyo, 0f)]
        [TestCase(UITKTweenLoop.Increment, 2f)]
        public void FiniteLoopsAndDelayHaveStableEndpoints(UITKTweenLoop loop, float endpoint)
        {
            asset.Root.Property = UITKTweenProperty.Translate; asset.Root.From = Vector2.zero; asset.Root.To = Vector2.one;
            asset.Root.Delay = .5f; asset.Root.Repetitions = 2; asset.Root.Loop = loop;
            using var player = Bind(); using var control = player.PrepareTimeline().TakeControl();
            Assert.That(control.Duration, Is.EqualTo(2.5f)); control.Sample(.25f);
            Assert.That(a.Q("title").style.translate.value.x.value, Is.Zero);
            control.SampleProgress(1); Assert.That(a.Q("title").style.translate.value.x.value, Is.EqualTo(endpoint).Within(.001));
            control.Sample(.75f); Assert.That(a.Q("title").style.translate.value.x.value, Is.EqualTo(.25f).Within(.001));
        }

        [Test] public void InvalidSampleDoesNotStopAutomaticProgress_AndDisposedBindingRejectsUse()
        {
            using var player = Bind(); using var control = player.PrepareTimeline().TakeControl(); control.ResumeAsync();
            Assert.Throws<ArgumentOutOfRangeException>(() => control.Sample(float.NaN)); Assert.That(control.IsAutomatic, Is.True);
            Tick(); Assert.That(control.Completion.IsCompletedSuccessfully, Is.True);
            player.Dispose(); Assert.Throws<ObjectDisposedException>(() => player.PlayAsync());
        }
    }
}
