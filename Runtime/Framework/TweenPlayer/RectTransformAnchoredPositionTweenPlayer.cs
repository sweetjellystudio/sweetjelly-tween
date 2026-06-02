using UnityEngine;

namespace SWeetJelly.Tween.Framework
{
    public sealed class RectTransformAnchoredPositionTweenPlayer
        : TweenPlayerBehaviour<RectTransform, Vector2>
    {
        protected override Vector2 CurrentValue
        {
            get => TargetComponent.anchoredPosition;
            set => TargetComponent.anchoredPosition = value;
        }

        protected override TweenCase CreateTweenCase(Vector2 endValue)
        {
            return TargetComponent.DOAnchoredPosition(endValue, duration);
        }

        protected override void IncrementLoopChangeValues()
        {
            Vector2 delta = endValue - startValue;

            startValue = endValue;
            endValue += delta;
        }
    }
}