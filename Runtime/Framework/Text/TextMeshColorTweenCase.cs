using UnityEngine;

namespace SWeetJelly.Tween.Framework
{
    public sealed class TextMeshColorTweenCase : TweenValueCase<TextMesh, Color>
    {
        private readonly GameObject parentObject;

        public TextMeshColorTweenCase(TextMesh textMesh, Color resultValue)
            : base(textMesh, resultValue)
        {
            parentObject = textMesh.gameObject;
            startValue = textMesh.color;
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            target.color = Color.LerpUnclamped(
                startValue,
                resultValue,
                EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.color = resultValue;
        }
    }
}