using UnityEngine;
using UnityEngine.UIElements;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class VisualElementScaleTweenCase : TweenValueCase<VisualElement, Vector3>
    {
        private readonly VisualElementTweenLifetime lifetime;

        public VisualElementScaleTweenCase(VisualElement element, Vector3 resultValue) : base(element, resultValue)
        {
            VisualElementTweenLifetime.RequirePanel(element);
            RequireFinite(resultValue, nameof(resultValue));
            startValue = (element.style.scale.keyword == StyleKeyword.Undefined
                ? element.style.scale.value : element.resolvedStyle.scale).value;
            RequireFinite(startValue, nameof(startValue));
            lifetime = new VisualElementTweenLifetime(element, this);
        }

        private static void RequireFinite(Vector3 value, string name)
        {
            VisualElementTweenLifetime.RequireFinite(value.x, name);
            VisualElementTweenLifetime.RequireFinite(value.y, name);
            VisualElementTweenLifetime.RequireFinite(value.z, name);
        }

        protected override void ResetPlaybackState() => lifetime.Activate();
        public override bool Validate() => lifetime.Validate();
        public override void Invoke(float deltaTime)
        {
            if (lifetime.CanWrite()) target.style.scale = new Scale(Vector3.LerpUnclamped(startValue, resultValue, EvaluateEase(State)));
        }
        public override void DefaultComplete()
        {
            if (lifetime.CanWrite()) target.style.scale = new Scale(resultValue);
        }
    }
}
