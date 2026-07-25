using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class CameraOrthographicSizeTweenPlayer
        : TweenPlayerBehaviour<Camera, float>
    {
        protected override float CurrentValue
        {
            get => TargetComponent.orthographicSize;
            set => TargetComponent.orthographicSize = value;
        }

        protected override TweenCase CreateTweenCase(float endValue) => TargetComponent.DOOrthographicSize(endValue, duration);

        protected override void IncrementLoopChangeValues()
        {
            float delta = endValue - startValue;
            startValue = endValue;
            endValue += delta;
        }
    }
}
