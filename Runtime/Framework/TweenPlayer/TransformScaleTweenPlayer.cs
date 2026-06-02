using UnityEngine;

namespace SWeetJelly.Tween.Framework
{
    public sealed class TransformScaleTweenPlayer
        : TweenPlayerBehaviour<Transform, Vector3>
    {
        protected override Vector3 CurrentValue
        {
            get => TargetComponent.localScale;
            set => TargetComponent.localScale = value;
        }

        protected override TweenCase CreateTweenCase(Vector3 endValue)
        {
            return TargetComponent.DOScale(endValue, duration);
        }

        protected override void IncrementLoopChangeValues()
        {
            Vector3 delta = endValue - startValue;

            startValue = endValue;
            endValue += delta;
        }
    }
}