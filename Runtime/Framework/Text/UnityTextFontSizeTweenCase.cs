using UnityEngine;
using UnityEngine.UI;

namespace SWeetJelly.Tween.Framework
{
    public sealed class UnityTextFontSizeTweenCase : TweenValueCase<Text, int>
    {
        private readonly GameObject parentObject;

        public UnityTextFontSizeTweenCase(Text text, int resultValue)
            : base(text, resultValue)
        {
            parentObject = text.gameObject;
            startValue = text.fontSize;
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            target.fontSize = Mathf.RoundToInt(
                Mathf.LerpUnclamped(
                    startValue,
                    resultValue,
                    EvaluateEase(State)));
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.fontSize = resultValue;
        }
    }
}