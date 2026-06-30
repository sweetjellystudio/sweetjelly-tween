using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class AnchoredPositionShakeTweenCase : TweenCase
    {
        private readonly RectTransform target;
        private readonly GameObject parentObject;
        private readonly Vector2 startPosition;
        private readonly float magnitude;

        public AnchoredPositionShakeTweenCase(RectTransform target, float magnitude)
        {
            this.target = target != null
                ? target
                : throw new ArgumentNullException(nameof(target));

            parentObject = target.gameObject;
            startPosition = target.anchoredPosition;
            this.magnitude = Mathf.Max(0f, magnitude);
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            float strength = 1f - EvaluateEase(State);

            target.anchoredPosition =
                startPosition + Random.insideUnitCircle * magnitude * strength;
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.anchoredPosition = startPosition;
        }
    }
}