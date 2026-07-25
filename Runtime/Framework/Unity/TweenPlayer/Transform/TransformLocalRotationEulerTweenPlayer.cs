using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformLocalRotationEulerTweenPlayer
        : TweenPlayerBehaviour<Transform, Vector3>
    {
        protected override Vector3 CurrentValue
        {
            get => TargetComponent.localEulerAngles;
            set => TargetComponent.localEulerAngles = value;
        }

        protected override TweenCase CreateTweenCase(Vector3 endValue) => TargetComponent.DOLocalRotate(endValue, duration);

        protected override void IncrementLoopChangeValues()
        {
            Vector3 delta = endValue - startValue;
            startValue = endValue;
            endValue += delta;
        }
    }
}
