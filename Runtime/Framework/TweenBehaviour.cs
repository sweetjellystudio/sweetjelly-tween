using UnityEngine;
using SWeetJelly.Tween.App;
using SWeetJelly.Tween.Contract;
using System;

namespace SWeetJelly.Tween.Framework
{
    public sealed class TweenBehaviour : MonoBehaviour
    {
        private TweenService service;

        public void Bind(TweenService tweenService)
        {
            service = tweenService ?? throw new ArgumentNullException(nameof(tweenService));
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