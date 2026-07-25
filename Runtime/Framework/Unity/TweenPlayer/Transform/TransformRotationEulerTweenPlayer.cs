using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformRotationEulerTweenPlayer
        : TweenPlayerBehaviour<Transform, Vector3>
    {
        protected override Vector3 CurrentValue
        {
            get => TargetComponent.eulerAngles;
            set => TargetComponent.eulerAngles = value;
        }

        protected override TweenCase CreateTweenCase(Vector3 endValue) => TargetComponent.DORotate(endValue, duration);

        protected override void IncrementLoopChangeValues()
        {
            Vector3 delta = endValue - startValue;
            startValue = endValue;
            endValue += delta;
        }
    }
}
