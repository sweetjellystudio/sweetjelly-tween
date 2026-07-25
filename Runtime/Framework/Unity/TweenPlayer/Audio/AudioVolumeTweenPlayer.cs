using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class AudioVolumeTweenPlayer
        : TweenPlayerBehaviour<AudioSource, float>
    {
        protected override float CurrentValue
        {
            get => TargetComponent.volume;
            set => TargetComponent.volume = Mathf.Clamp01(value);
        }

        protected override TweenCase CreateTweenCase(float endValue) => TargetComponent.DOVolume(endValue, duration);

        protected override void IncrementLoopChangeValues()
        {
            float delta = endValue - startValue;
            startValue = endValue;
            endValue += delta;
        }
    }
}
