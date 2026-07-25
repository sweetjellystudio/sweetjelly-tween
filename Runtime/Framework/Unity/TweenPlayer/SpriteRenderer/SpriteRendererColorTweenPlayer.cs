using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class SpriteRendererColorTweenPlayer
        : TweenPlayerBehaviour<SpriteRenderer, Color>
    {
        protected override Color CurrentValue
        {
            get => TargetComponent.color;
            set => TargetComponent.color = value;
        }

        protected override TweenCase CreateTweenCase(Color endValue) => TargetComponent.DOColor(endValue, duration);

        protected override void IncrementLoopChangeValues()
        {
            Color delta = endValue - startValue;
            startValue = endValue;
            endValue += delta;
        }
    }
}
