using UnityEngine;

namespace SWeetJelly.Tween.Framework
{
    public sealed class SpriteRendererFadeTweenCase : TweenValueCase<SpriteRenderer, float>
    {
        private readonly GameObject parentObject;

        public SpriteRendererFadeTweenCase(SpriteRenderer spriteRenderer, float resultValue)
            : base(spriteRenderer, resultValue)
        {
            parentObject = spriteRenderer.gameObject;
            startValue = spriteRenderer.color.a;
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