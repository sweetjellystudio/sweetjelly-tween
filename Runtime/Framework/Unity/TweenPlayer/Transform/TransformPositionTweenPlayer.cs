using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformPositionTweenPlayer
        : TweenPlayerBehaviour<Transform, Vector3>
    {
        protected override Vector3 CurrentValue
        {
            get => TargetComponent.position;
            set => TargetComponent.position = value;
        }

        protected override TweenCase CreateTweenCase(Vector3 endValue) => TargetComponent.DOMove(endValue, duration);

        protected override void IncrementLoopChangeValues()
        {
            Vector3 delta = endValue - startValue;
            startValue = endValue;
            endValue += delta;
        }
    }
}
