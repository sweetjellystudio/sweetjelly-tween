using UnityEngine;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TextMeshFontSizeTweenCase : TweenValueCase<TextMesh, int>
    {
        private readonly GameObject parentObject;

        public TextMeshFontSizeTweenCase(TextMesh textMesh, int resultValue)
            : base(textMesh, resultValue)
        {
            parentObject = textMesh.gameObject;
            startValue = textMesh.fontSize;
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