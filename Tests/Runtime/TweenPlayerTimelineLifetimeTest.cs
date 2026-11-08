namespace SWeetJelly.Tween.Tests.PlayMode
{
    using System;
    using System.Collections;
    using System.Reflection;
    using NUnit.Framework;
    using SWeetJelly.Tween.App;
    using SWeetJelly.Tween.Contract;
    using SWeetJelly.Tween.Framework.Unity;
    using UnityEngine;
    using UnityEngine.TestTools;
    using Object = UnityEngine.Object;

    public sealed class TweenPlayerTimelineLifetimeTest
    {
        private static void Set(object target, string field, object value)
        {
            for (var type = target.GetType(); type != null; type = type.BaseType)
            {
                var info = type.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (info != null) { info.SetValue(target, value); return; }
            }
            throw new MissingFieldException(field);
        }

        [UnityTest] public IEnumerator DisableAndDestroy_CancelManualAwaitAndReleaseOwnership()
        {
            var go = new GameObject("Timeline lifetime", typeof(CanvasGroup));
            var service = new TweenService();
            try
            {
                var player = go.AddComponent<CanvasGroupFadeTweenPlayer>(); player.Initialise(service);
                var control = player.PrepareTimeline().TakeControl();
                go.SetActive(false); Assert.That(control.Completion.IsCanceled, Is.True);
                go.SetActive(true); var next = player.PrepareTimeline().TakeControl();
                Object.Destroy(go); yield return null;
                Assert.That(next.Completion.IsCanceled, Is.True);
            }
            finally { service.Dispose(); if (go != null) Object.Destroy(go); }
        }

        [UnityTest] public IEnumerator DestroyedTarget_CancelsEvenWhenManualClockIsParked()
        {
            var owner = new GameObject("Owner", typeof(CanvasGroup));
            var target = new GameObject("Borrowed target", typeof(CanvasGroup));
            var service = new TweenService();
            try
            {
                var player = owner.AddComponent<CanvasGroupFadeTweenPlayer>(); player.Initialise(service);
                Set(player, "targetComponent", target.GetComponent<CanvasGroup>());
                var control = player.PrepareTimeline().TakeControl();
                Object.Destroy(target); yield return null; yield return null;
                Assert.That(control.Completion.IsCanceled, Is.True);
            }
            finally { service.Dispose(); Object.Destroy(owner); if (target != null) Object.Destroy(target); }
        }

        [UnityTest] public IEnumerator ActualFrames_AutoTransferSampleResumeAndRegrab_HaveOneWriter()
        {
            var go = new GameObject("Frame sampling", typeof(CanvasGroup));
            var service = new TweenService();
            try
            {
                var player = go.AddComponent<CanvasGroupFadeTweenPlayer>(); player.Initialise(service);
                go.GetComponent<CanvasGroup>().alpha = 0f; Set(player, "forwardValue", 1f); Set(player, "duration", 1f);
                var old = player.PlayAsync();
                float elapsed = 0f;
                while (elapsed < .15f) { yield return null; service.Tick(UpdateMethod.Update, Time.deltaTime, Time.unscaledDeltaTime, true); elapsed += Time.deltaTime; }
                float pose = go.GetComponent<CanvasGroup>().alpha;
                var prepared = player.PrepareTimeline(); using var control = prepared.TakeControl();
                Assert.That(go.GetComponent<CanvasGroup>().alpha, Is.EqualTo(pose).Within(.0001f));
                Assert.That(old.IsCanceled, Is.True);
                control.Sample(.4f);
                for (int i = 0; i < 3; i++) { yield return null; service.Tick(UpdateMethod.Update, Time.deltaTime, Time.unscaledDeltaTime, true); }
                Assert.That(go.GetComponent<CanvasGroup>().alpha, Is.EqualTo(.4f).Within(.0001f));
                var task = control.ResumeAsync();
                while (!task.IsCompleted) { yield return null; service.Tick(UpdateMethod.Update, Time.deltaTime, Time.unscaledDeltaTime, true); }
                Assert.That(task.IsCompletedSuccessfully, Is.True);
                Assert.That(go.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1f).Within(.0001f));
                Assert.That(Time.frameCount, Is.GreaterThan(3));
            }
            finally { service.Dispose(); Object.Destroy(go); }
        }
    }
}
