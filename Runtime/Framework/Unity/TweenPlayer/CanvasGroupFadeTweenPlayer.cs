using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class CanvasGroupFadeTweenPlayer 
        : TweenPlayerBehaviour<CanvasGroup, float>
    {
        protected override float CurrentValue
        {
            get => TargetComponent.alpha;
            set => TargetComponent.alpha = Mathf.Clamp01(value);
        }

        protected override TweenCase CreateTweenCase(float endValue)
        {
            return TargetComponent.DOFade(endValue, duration);
        }

        protected override void IncrementLoopChangeValues()
        {
            float delta = endValue - startValue;

            startValue = endValue;
            endValue += delta;
        }
    }
}