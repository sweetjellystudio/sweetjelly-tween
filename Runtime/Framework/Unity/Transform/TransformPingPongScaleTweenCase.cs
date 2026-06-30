using System;
using UnityEngine;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Domain;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformPingPongScaleTweenCase : TweenCase
    {
        private readonly Transform target;
        private readonly GameObject parentObject;

        private readonly float minValue;
        private readonly float maxValue;

        private readonly IEasingFunction positiveEase;
        private readonly IEasingFunction negativeEase;

        public TransformPingPongScaleTweenCase(
            Transform target,
            float minValue,
            float maxValue,
            float duration,
            EaseType positiveScaleEasing,
            EaseType negativeScaleEasing)
        {
            this.target = target != null
                ? target
                : throw new ArgumentNullException(nameof(target));

            parentObject = target.gameObject;

            this.minValue = minValue;
            this.maxValue = maxValue;

            positiveEase = EasingFunctions.Get(positiveScaleEasing);
            negativeEase = EasingFunctions.Get(negativeScaleEasing);

            SetDuration(duration);
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            float scale;

            if (State <= 0.5f)
            {
                float t = Mathf.InverseLerp(0f, 0.5f, State);
                scale = Mathf.LerpUnclamped(minValue, maxValue, positiveEase.Evaluate(t));
            }
            else
            {
                float t = Mathf.InverseLerp(0.5f, 1f, State);
                scale = Mathf.LerpUnclamped(maxValue, minValue, negativeEase.Evaluate(t));
            }

            target.localScale = new Vector3(scale, scale, scale);
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.localScale = new Vector3(minValue, minValue, minValue);
        }
    }
}