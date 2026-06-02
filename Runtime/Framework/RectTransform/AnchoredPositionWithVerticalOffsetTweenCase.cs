using System;
using UnityEngine;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework
{
    public sealed class AnchoredPositionWithVerticalOffsetTweenCase : TweenValueCase<RectTransform, Vector2>
    {
        private readonly GameObject parentObject;
        private readonly AnimationCurve verticalOffset;

        public AnchoredPositionWithVerticalOffsetTweenCase(
            RectTransform rectTransform,
            Vector2 resultValue,
            AnimationCurve verticalOffset)
            : base(rectTransform, resultValue)
        {
            parentObject = rectTransform.gameObject;
            startValue = rectTransform.anchoredPosition;

            this.verticalOffset = verticalOffset
                ?? throw new ArgumentNullException(nameof(verticalOffset));
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            float eased = EvaluateEase(State);

            Vector2 basePosition = Vector2.LerpUnclamped(
                startValue,
                resultValue,
                eased);

            basePosition.y += verticalOffset.Evaluate(State);

            target.anchoredPosition = basePosition;
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.anchoredPosition = resultValue;
        }
    }
}