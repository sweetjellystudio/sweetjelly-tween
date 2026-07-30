namespace SWeetJelly.Tween.Tests.PlayMode
{
    using System.Reflection;
    using NUnit.Framework;
    using SWeetJelly.Tween.App;
    using SWeetJelly.Tween.Contract;
    using SWeetJelly.Tween.Framework.Unity;
    using UnityEngine;

    public sealed class TweenPlayerEndpointTest
    {
        [Test]
        public void CaptureOnForward_CapturesAtPlayTime_AndBackwardRestoresCapturedValue()
        {
            var holder = new GameObject("Tween Player Capture Test Holder");
            CanvasGroup canvasGroup = holder.AddComponent<CanvasGroup>();
            CanvasGroupFadeTweenPlayer player = holder.AddComponent<CanvasGroupFadeTweenPlayer>();
            var service = new TweenService();

            try
            {
                canvasGroup.alpha = 0.25f;
                SetBaseField(player, "forwardValue", 1f);
                SetBaseField(player, "duration", 1f);

                player.Initialise(service);
                player.PlayForward();
                Tick(service, 1f);

                Assert.That(canvasGroup.alpha, Is.EqualTo(1f).Within(0.0001f));

                player.PlayBackward();
                Tick(service, 1f);

                Assert.That(canvasGroup.alpha, Is.EqualTo(0.25f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(holder);
            }
        }

        [Test]
        public void CaptureOnForward_ReplayedForward_DoesNotReplaceCapturedOrigin()
        {
            var holder = new GameObject("Tween Player Capture Test Holder");
            CanvasGroup canvasGroup = holder.AddComponent<CanvasGroup>();
            CanvasGroupFadeTweenPlayer player = holder.AddComponent<CanvasGroupFadeTweenPlayer>();
            var service = new TweenService();

            try
            {
                canvasGroup.alpha = 0f;
                SetBaseField(player, "forwardValue", 1f);
                SetBaseField(player, "duration", 1f);

                player.Initialise(service);
                player.PlayForward();
                Tick(service, 0.5f);
                player.PlayForward();
                Tick(service, 1f);
                player.PlayBackward();
                Tick(service, 1f);

                Assert.That(canvasGroup.alpha, Is.EqualTo(0f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(holder);
            }
        }

        [Test]
        public void CaptureOnForward_BackwardWithoutOrigin_CompletesWithoutChangingValue()
        {
            var holder = new GameObject("Tween Player Capture Test Holder");
            CanvasGroup canvasGroup = holder.AddComponent<CanvasGroup>();
            CanvasGroupFadeTweenPlayer player = holder.AddComponent<CanvasGroupFadeTweenPlayer>();
            var service = new TweenService();
            int playedCount = 0;
            int completedCount = 0;

            try
            {
                canvasGroup.alpha = 0.4f;
                player.Played += () => playedCount++;
                player.Completed += () => completedCount++;
                player.Initialise(service);

                player.PlayBackward();

                Assert.That(canvasGroup.alpha, Is.EqualTo(0.4f).Within(0.0001f));
                Assert.That(player.IsPlaying, Is.False);
                Assert.That(playedCount, Is.EqualTo(1));
                Assert.That(completedCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(holder);
            }
        }

        [Test]
        public void PlayForward_WithConfiguredEndpoints_StartsFromBackwardValue()
        {
            var holder = new GameObject("Tween Player Endpoint Test Holder");
            CanvasGroup canvasGroup = holder.AddComponent<CanvasGroup>();
            CanvasGroupFadeTweenPlayer player = holder.AddComponent<CanvasGroupFadeTweenPlayer>();
            var service = new TweenService();

            try
            {
                canvasGroup.alpha = 1f;
                SetBaseField(player, "endpointMode", TweenPlayerBehaviour<CanvasGroup, float>.EndpointMode.ConfiguredEndpoints);
                SetBaseField(player, "backwardValue", 0f);
                SetBaseField(player, "forwardValue", 1f);
                SetBaseField(player, "duration", 1f);

                player.Initialise(service);
                player.PlayForward();

                Assert.That(canvasGroup.alpha, Is.EqualTo(0f).Within(0.0001f));

                Tick(service, 0.5f);

                Assert.That(canvasGroup.alpha, Is.EqualTo(0.5f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(holder);
            }
        }

        [Test]
        public void PlayBackward_WithConfiguredEndpoints_StartsFromForwardValue()
        {
            var holder = new GameObject("Tween Player Endpoint Test Holder");
            CanvasGroup canvasGroup = holder.AddComponent<CanvasGroup>();
            CanvasGroupFadeTweenPlayer player = holder.AddComponent<CanvasGroupFadeTweenPlayer>();
            var service = new TweenService();

            try
            {
                canvasGroup.alpha = 0f;
                SetBaseField(player, "endpointMode", TweenPlayerBehaviour<CanvasGroup, float>.EndpointMode.ConfiguredEndpoints);
                SetBaseField(player, "backwardValue", 0f);
                SetBaseField(player, "forwardValue", 1f);
                SetBaseField(player, "duration", 1f);

                player.Initialise(service);
                player.PlayBackward();

                Assert.That(canvasGroup.alpha, Is.EqualTo(1f).Within(0.0001f));

                Tick(service, 0.5f);

                Assert.That(canvasGroup.alpha, Is.EqualTo(0.5f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(holder);
            }
        }

        private static void SetBaseField<TValue>(
            CanvasGroupFadeTweenPlayer player,
            string fieldName,
            TValue value)
        {
            FieldInfo field = typeof(TweenPlayerBehaviour<CanvasGroup, float>).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(field, Is.Not.Null, $"Field '{fieldName}' was not found.");
            field.SetValue(player, value);
        }

        private static void Tick(TweenService service, float deltaTime)
        {
            service.Tick(
                UpdateMethod.Update,
                deltaTime,
                deltaTime,
                skipScaledWhenTimeScaleIsZero: false);
        }
    }
}
