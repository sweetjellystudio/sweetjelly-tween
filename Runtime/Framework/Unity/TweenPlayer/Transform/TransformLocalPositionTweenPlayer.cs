using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformLocalPositionTweenPlayer
        : TweenPlayerBehaviour<Transform, Vector3>
    {
        protected override Vector3 CurrentValue
        {
            get => TargetComponent.localPosition;
            set => TargetComponent.localPosition = value;
        }

        protected override TweenCase CreateTweenCase(Vector3 endValue) => TargetComponent.DOLocalMove(endValue, duration);

        protected override void IncrementLoopChangeValues()
        {
            Vector3 delta = endValue - startValue;
            startValue = endValue;
            endValue += delta;
        }
    }
}
