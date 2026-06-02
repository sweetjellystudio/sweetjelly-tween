using UnityEngine;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework
{
    public sealed class SpriteRendererColorTweenCase : TweenValueCase<SpriteRenderer, Color>
    {
        private readonly GameObject parentObject;

        public SpriteRendererColorTweenCase(SpriteRenderer spriteRenderer, Color resultValue)
            : base(spriteRenderer, resultValue)
        {
            parentObject = spriteRenderer.gameObject;
            startValue = spriteRenderer.color;
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