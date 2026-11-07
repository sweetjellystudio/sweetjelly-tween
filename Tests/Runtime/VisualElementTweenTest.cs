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

    public sealed class VisualElementTweenTest
    {
        private GameObject holder;
        private PanelSettings settings;
        private ThemeStyleSheet theme;
        private RenderTexture texture;
        private VisualElement element;
        private TweenService service;

        [UnitySetUp] public IEnumerator SetUp()
        {
            service = new TweenService();
            settings = ScriptableObject.CreateInstance<PanelSettings>();
            theme = ScriptableObject.CreateInstance<ThemeStyleSheet>();
            settings.themeStyleSheet = theme;
            texture = new RenderTexture(320, 180, 0);
            settings.targetTexture = texture;
            holder = new GameObject("UITK tween panel");
            holder.SetActive(false);
            var document = holder.AddComponent<UIDocument>();
            document.panelSettings = settings;
            holder.SetActive(true);
            yield return null;
            element = new VisualElement();
            element.style.width = 100; element.style.height = 80;
            element.style.opacity = 1;
            element.style.scale = new Scale(Vector3.one);
            document.rootVisualElement.Add(element);
            yield return null;
            Assert.That(element.panel, Is.Not.Null);
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            service.Dispose(); Object.Destroy(holder); Object.Destroy(settings); Object.Destroy(texture); Object.Destroy(theme);
            yield return null;
        }
        private void Tick(float scaled = 1, float unscaled = 1) => service.Tick(UpdateMethod.Update, scaled, unscaled, true);

        [UnityTest] public IEnumerator FadeAndScale_ApplyFinalInlineValues()
        {
            var fade = service.Play(element.DOFade(0.25f, 1f));
            var scale = service.Play(element.DOScale(new Vector3(0, -1, 1), 1f));
            Tick();
            Assert.That(fade.IsCompleted && scale.IsCompleted, Is.True);
            Assert.That(element.style.opacity.value, Is.EqualTo(.25f));
            Assert.That(element.style.scale.value.value, Is.EqualTo(new Vector3(0, -1, 1)));
            yield return null;
        }
        [UnityTest] public IEnumerator Delay_UsesConstructionCapture_AndScaledZeroPauses()
        {
            element.style.opacity = .8f;
            var tween = element.DOFade(0, 1, delay: 1);
            element.style.opacity = .4f;
            var handle = service.Play(tween);
            Tick(0, 1);
            Assert.That(element.style.opacity.value, Is.EqualTo(.4f));
            Tick(.5f, .5f);
            Assert.That(element.style.opacity.value, Is.EqualTo(.4f));
            Tick(1, 1);
            Assert.That(element.style.opacity.value, Is.EqualTo(.4f).Within(.001f));
            Tick(); Assert.That(handle.IsCompleted, Is.True);
            yield return null;
        }
        [UnityTest] public IEnumerator Detach_CancelsImmediately_AndReattachDoesNotReviveRun()
        {
            var parent = element.parent;
            var handle = service.Play(element.DOFade(0, 1));
            var wait = handle.ToTask();
            element.RemoveFromHierarchy();
            Assert.That(handle.IsKilled && wait.IsCanceled, Is.True);
            parent.Add(element);
            Tick();
            Assert.That(element.style.opacity.value, Is.EqualTo(1));
            yield return null;
        }
        [UnityTest] public IEnumerator TokenCancellation_PreservesPartialInlineValue()
        {
            using var token = new CancellationTokenSource();
            var handle = service.Play(element.DOFade(0, 1));
            var task = handle.ToTask(token.Token);
            Tick(.25f, .25f);
            token.Cancel(); Tick();
            Assert.That(task.IsCanceled, Is.True);
            Assert.That(element.style.opacity.value, Is.EqualTo(.75f).Within(.001f));
            yield return null;
        }
        [UnityTest] public IEnumerator CompletedAndKilledCases_ReleaseDetachSubscriptions()
        {
            int completed = 0, killed = 0;
            var tween = element.DOFade(0, 1);
            tween.Completed += () => completed++;
            tween.Killed += () => killed++;
            service.Play(tween); Tick();
            element.RemoveFromHierarchy();
            Assert.That(completed, Is.EqualTo(1)); Assert.That(killed, Is.Zero);
            yield return null;
        }
        [UnityTest] public IEnumerator MovingToAnotherPanel_CancelsTheOldRun()
        {
            var previousPanel = element.panel;
            var otherSettings = Object.Instantiate(settings);
            var otherObject = new GameObject("Other runtime panel");
            otherObject.SetActive(false);
            var otherDocument = otherObject.AddComponent<UIDocument>();
            otherDocument.panelSettings = otherSettings;
            otherObject.SetActive(true);
            try
            {
                yield return null;
                var handle = service.Play(element.DOScale(Vector3.zero, 1));
                otherDocument.rootVisualElement.Add(element);
                Assert.That(ReferenceEquals(previousPanel, element.panel), Is.False);
                Assert.That(handle.IsKilled, Is.True);
                Tick();
                Assert.That(element.style.scale.value.value, Is.EqualTo(Vector3.one));
            }
            finally { Object.Destroy(otherObject); Object.Destroy(otherSettings); }
        }
        [UnityTest] public IEnumerator DescendantDetach_DoesNotCancelParentTarget()
        {
            var child = new VisualElement(); element.Add(child);
            var handle = service.Play(element.DOFade(0, 1));
            child.RemoveFromHierarchy();
            Assert.That(handle.IsActive, Is.True);
            Tick(); Assert.That(handle.IsCompleted, Is.True);
            yield return null;
        }
        [UnityTest] public IEnumerator UnscaledFade_ContinuesAtScaledZero_AndClampsOpacity()
        {
            var handle = service.Play(element.DOFade(2, 1, unscaledTime: true));
            Tick(0, 1);
            Assert.That(handle.IsCompleted, Is.True);
            Assert.That(element.style.opacity.value, Is.EqualTo(1));
            yield return null;
        }
        [UnityTest] public IEnumerator NonfiniteAndDetachedInputs_AreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => element.DOFade(float.NaN, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => element.DOScale(new Vector3(float.PositiveInfinity, 1, 1), 1));
            element.RemoveFromHierarchy();
            Assert.Throws<InvalidOperationException>(() => element.DOFade(0, 1));
            yield return null;
        }
        [UnityTest] public IEnumerator ResolvedStyles_AreCaptured_WhenNoInlineOverrideExists()
        {
            element.style.opacity = StyleKeyword.Null;
            element.style.scale = StyleKeyword.Null;
            yield return null;
            var fade = new VisualElementFadeTweenCase(element, .5f);
            var scale = new VisualElementScaleTweenCase(element, Vector3.zero);
            Assert.That(fade.StartValue, Is.EqualTo(element.resolvedStyle.opacity));
            Assert.That(scale.StartValue, Is.EqualTo(element.resolvedStyle.scale.value));
        }
    }
}
