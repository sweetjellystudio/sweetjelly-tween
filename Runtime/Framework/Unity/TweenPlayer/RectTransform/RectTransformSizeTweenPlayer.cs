using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class RectTransformSizeTweenPlayer
        : TweenPlayerBehaviour<RectTransform, Vector2>
    {
        protected override Vector2 CurrentValue
        {
            get => TargetComponent.sizeDelta;
            set => TargetComponent.sizeDelta = value;
        }

        protected override TweenCase CreateTweenCase(Vector2 endValue) => TargetComponent.DOSize(endValue, duration);

        protected override void IncrementLoopChangeValues()
        {
            Vector2 delta = endValue - startValue;
            startValue = endValue;
            endValue += delta;
        }
    }
}
