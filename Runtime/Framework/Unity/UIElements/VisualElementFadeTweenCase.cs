using UnityEngine;
using UnityEngine.UIElements;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class VisualElementFadeTweenCase : TweenValueCase<VisualElement, float>
    {
        private readonly VisualElementTweenLifetime lifetime;

        public VisualElementFadeTweenCase(VisualElement element, float resultValue) : base(element, resultValue)
        {
            VisualElementTweenLifetime.RequirePanel(element);
            VisualElementTweenLifetime.RequireFinite(resultValue, nameof(resultValue));
            startValue = element.style.opacity.keyword == StyleKeyword.Undefined
                ? element.style.opacity.value : element.resolvedStyle.opacity;
            VisualElementTweenLifetime.RequireFinite(startValue, nameof(startValue));
            lifetime = new VisualElementTweenLifetime(element, this);
        }

        protected override void ResetPlaybackState() => lifetime.Activate();
        public override bool Validate() => lifetime.Validate();
        public override void Invoke(float deltaTime)
        {
            if (lifetime.CanWrite()) target.style.opacity = Mathf.Clamp01(Mathf.LerpUnclamped(startValue, resultValue, EvaluateEase(State)));
        }
        public override void DefaultComplete()
        {
            if (lifetime.CanWrite()) target.style.opacity = Mathf.Clamp01(resultValue);
        }
    }
}
