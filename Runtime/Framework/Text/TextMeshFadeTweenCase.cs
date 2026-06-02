using UnityEngine;

namespace SWeetJelly.Tween.Framework
{
    public sealed class TextMeshFadeTweenCase : TweenValueCase<TextMesh, float>
    {
        private readonly GameObject parentObject;

        public TextMeshFadeTweenCase(TextMesh textMesh, float resultValue)
            : base(textMesh, resultValue)
        {
            parentObject = textMesh.gameObject;
            startValue = textMesh.color.a;
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            Color color = target.color;

            color.a = Mathf.LerpUnclamped(
                startValue,
                resultValue,
                EvaluateEase(State));

            target.color = color;
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            Color color = target.color;
            color.a = resultValue;
            target.color = color;
        }
    }
}