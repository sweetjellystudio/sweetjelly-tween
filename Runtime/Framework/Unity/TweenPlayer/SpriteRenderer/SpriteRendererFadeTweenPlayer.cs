using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class SpriteRendererFadeTweenPlayer
        : TweenPlayerBehaviour<SpriteRenderer, float>
    {
        protected override float CurrentValue
        {
            get => TargetComponent.color.a;
            set
            {
                Color color = TargetComponent.color;
                color.a = Mathf.Clamp01(value);
                TargetComponent.color = color;
            }
        }

        protected override TweenCase CreateTweenCase(float endValue) => TargetComponent.DOFade(endValue, duration);

        protected override void IncrementLoopChangeValues()
        {
            float delta = endValue - startValue;
            startValue = endValue;
            endValue += delta;
        }
    }
}
