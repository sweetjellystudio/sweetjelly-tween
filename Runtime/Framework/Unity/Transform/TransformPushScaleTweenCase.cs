using System;
using UnityEngine;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Domain;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformPushScaleTweenCase : TweenCase
    {
        private readonly Transform target;
        private readonly GameObject parentObject;

        private readonly Vector3 startValue;
        private readonly Vector3 firstScaleValue;
        private readonly Vector3 secondScaleValue;

        private readonly float firstTime;
        private readonly float secondTime;

        private readonly IEasingFunction firstScaleEasing;
        private readonly IEasingFunction secondScaleEasing;

        public TransformPushScaleTweenCase(
            Transform target,
            Vector3 firstScaleValue,
            Vector3 secondScaleValue,
            float firstTime,
            float secondTime,
            EaseType firstScaleEasing,
            EaseType secondScaleEasing)
        {
            this.target = target != null
                ? target
                : throw new ArgumentNullException(nameof(target));

            parentObject = target.gameObject;

            startValue = target.localScale;
            this.firstScaleValue = firstScaleValue;
            this.secondScaleValue = secondScaleValue;

            this.firstTime = Mathf.Max(0f, firstTime);
            this.secondTime = Mathf.Max(0f, secondTime);

            this.firstScaleEasing = EasingFunctions.Get(firstScaleEasing);
            this.secondScaleEasing = EasingFunctions.Get(secondScaleEasing);
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            float elapsed = Duration * State;

            if (firstTime <= Mathf.Epsilon)
            {
                ApplySecondPhase(elapsed);
                return;
            }

            if (elapsed <= firstTime)
            {
                float t = Mathf.InverseLerp(0f, firstTime, elapsed);
                float eased = firstScaleEasing.Evaluate(t);

                target.localScale = Vector3.LerpUnclamped(
                    startValue,
                    firstScaleValue,
                    eased);
            }
            else
            {
                ApplySecondPhase(elapsed);
            }
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.localScale = secondScaleValue;
        }

        private void ApplySecondPhase(float elapsed)
        {
            if (secondTime <= Mathf.Epsilon)
            {
                target.localScale = secondScaleValue;
                return;
            }

            float totalTime = firstTime + secondTime;
            float t = Mathf.InverseLerp(firstTime, totalTime, elapsed);
            float eased = secondScaleEasing.Evaluate(t);

            target.localScale = Vector3.LerpUnclamped(
                firstScaleValue,
                secondScaleValue,
                eased);
        }
    }
}