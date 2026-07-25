using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class CameraFieldOfViewTweenPlayer
        : TweenPlayerBehaviour<Camera, float>
    {
        protected override float CurrentValue
        {
            get => TargetComponent.fieldOfView;
            set => TargetComponent.fieldOfView = value;
        }

        protected override TweenCase CreateTweenCase(float endValue) => TargetComponent.DOFieldOfView(endValue, duration);

        protected override void IncrementLoopChangeValues()
        {
            float delta = endValue - startValue;
            startValue = endValue;
            endValue += delta;
        }
    }
}
