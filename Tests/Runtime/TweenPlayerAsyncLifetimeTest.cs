namespace SWeetJelly.Tween.Tests.PlayMode
{
    using System.Collections;
    using NUnit.Framework;
    using SWeetJelly.Tween.App;
    using SWeetJelly.Tween.Framework.Unity;
    using UnityEngine;
    using UnityEngine.TestTools;

    public sealed class TweenPlayerAsyncLifetimeTest
    {
        [UnityTest] public IEnumerator Disable_CancelsPendingAwait_InRuntimeLifecycle()
        {
            var holder = new GameObject("Async lifecycle", typeof(CanvasGroup));
            var service = new TweenService();
            try
            {
                var player = holder.AddComponent<CanvasGroupFadeTweenPlayer>();
                player.Initialise(service);
                var task = player.PlayAsync();
                holder.SetActive(false);
                Assert.That(task.IsCanceled, Is.True);
                yield return null;
            }
            finally { service.Dispose(); Object.Destroy(holder); }
        }
        [UnityTest] public IEnumerator Destroy_CancelsPendingAwait_InRuntimeLifecycle()
        {
            var holder = new GameObject("Async destruction", typeof(CanvasGroup));
            var service = new TweenService();
            try
            {
                var player = holder.AddComponent<CanvasGroupFadeTweenPlayer>();
                player.Initialise(service);
                var task = player.PlayAsync();
                Object.Destroy(holder);
                yield return null;
                Assert.That(task.IsCanceled, Is.True);
            }
            finally { service.Dispose(); if (holder != null) Object.Destroy(holder); }
        }
    }
}
