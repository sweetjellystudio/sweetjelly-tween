using UnityEngine;
using UnityEngine.UI;

namespace SWeetJelly.Tween.Framework
{
    public sealed class LayoutElementPreferredHeightTweenCase : TweenValueCase<LayoutElement, float>
    {
        private readonly GameObject parentObject;

        public LayoutElementPreferredHeightTweenCase(
            LayoutElement layoutElement,
            float resultValue)
            : base(layoutElement, resultValue)
        {
            parentObject = layoutElement.gameObject;
            startValue = layoutElement.preferredHeight;
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            target.preferredHeight = Mathf.LerpUnclamped(
                startValue,
                resultValue,
                EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.preferredHeight = resultValue;
        }
    }
}