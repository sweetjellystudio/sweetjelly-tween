namespace SWeetJelly.Tween.Tests.PlayMode
{
    using System.Collections;
    using NUnit.Framework;
    using SWeetJelly.Tween.App;
    using SWeetJelly.Tween.Contract;
    using SWeetJelly.Tween.Framework.Unity;
    using UnityEngine;
    using UnityEngine.TestTools;

    public sealed class TweenTargetLifetimeTest
    {
        [UnityTest]
        public IEnumerator DestroyedTarget_CancelsItsAwaitEvenWhenScaledTimeIsStopped()
        {
            var service = new TweenService();
            var target = new GameObject("Destroyed tween target");
            var handle = service.Play(target.transform.DOMove(Vector3.one, 1f));
            var task = handle.ToTask();
            Object.Destroy(target);
            yield return null;
            service.Tick(UpdateMethod.Update, 0f, 0.1f, true);
            Assert.That(handle.IsKilled, Is.True);
            Assert.That(task.IsCanceled, Is.True);
        }

        [UnityTest]
        public IEnumerator BoundDriver_StopsScaledPlaybackAndContinuesUnscaledPlayback()
        {
            float previousScale = Time.timeScale;
            var holder = new GameObject("Scaled and unscaled driver");
            var service = new TweenService();
            var driver = holder.AddComponent<TweenBehaviour>();
            float scaled = 0f, unscaled = 0f;
            try
            {
                driver.Bind(service);
                Time.timeScale = 0f;
                var scaledRun = service.Play(TweenCases.DOFloat(0f, 1f, .02f, value => scaled = value));
                var unscaledRun = service.Play(TweenCases.DOFloat(0f, 1f, .02f, value => unscaled = value).SetUnscaledMode(true));
                float deadline = Time.realtimeSinceStartup + 2f;
                while (!unscaledRun.IsCompleted && Time.realtimeSinceStartup < deadline) yield return null;
                Assert.That(scaled, Is.Zero);
                Assert.That(scaledRun.IsActive, Is.True);
                Assert.That(unscaled, Is.EqualTo(1f).Within(.0001f));
                Assert.That(driver.Unbind(service), Is.True);
            }
            finally { Time.timeScale = previousScale; service.Dispose(); Object.Destroy(holder); }
        }
    }
}
