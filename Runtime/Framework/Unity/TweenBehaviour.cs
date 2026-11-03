using UnityEngine;
using SWeetJelly.Tween.App;
using SWeetJelly.Tween.Contract;
using System;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TweenBehaviour : MonoBehaviour
    {
        private TweenService service;
        public bool IsBound => service != null;

        public void Bind(TweenService tweenService)
        {
            if (tweenService == null) throw new ArgumentNullException(nameof(tweenService));
            if (service != null && !ReferenceEquals(service, tweenService))
                throw new InvalidOperationException("The tween driver is already bound.");
            service = tweenService;
        }

        public bool Unbind(TweenService expectedService)
        {
            if (!ReferenceEquals(service, expectedService) || service == null) return false;
            service = null;
            return true;
        }

        private void Update()
        {
            if (service == null)
                return;

            float scaledDelta = Time.timeScale == 0f ? 0f : Time.deltaTime;

            service.Tick(
                UpdateMethod.Update,
                scaledDelta,
                Time.unscaledDeltaTime,
                skipScaledWhenTimeScaleIsZero: true);
        }

        private void FixedUpdate()
        {
            if (service == null)
                return;

            float scaledDelta = Time.timeScale == 0f ? 0f : Time.fixedDeltaTime;

            service.Tick(
                UpdateMethod.FixedUpdate,
                scaledDelta,
                Time.fixedUnscaledDeltaTime,
                skipScaledWhenTimeScaleIsZero: true);
        }

        private void LateUpdate()
        {
            if (service == null)
                return;

            float scaledDelta = Time.timeScale == 0f ? 0f : Time.deltaTime;

            service.Tick(
                UpdateMethod.LateUpdate,
                scaledDelta,
                Time.unscaledDeltaTime,
                skipScaledWhenTimeScaleIsZero: true);
        }
    }
}
