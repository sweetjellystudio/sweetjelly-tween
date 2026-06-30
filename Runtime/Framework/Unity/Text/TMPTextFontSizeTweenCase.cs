using TMPro;
using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TMPTextFontSizeTweenCase : TweenValueCase<TMP_Text, float>
    {
        private readonly GameObject parentObject;

        public TMPTextFontSizeTweenCase(TMP_Text text, float resultValue)
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
            target.fontSize = Mathf.LerpUnclamped(
                startValue,
                resultValue,
                EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.fontSize = resultValue;
        }
    }
}