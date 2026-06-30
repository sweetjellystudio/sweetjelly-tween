using UnityEngine;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class RendererPropertyBlockColorTweenCase : TweenValueCase<Renderer, Color>
    {
        private readonly GameObject parentObject;
        private readonly MaterialPropertyBlock propertyBlock = new();
        private readonly int propertyId;

        public RendererPropertyBlockColorTweenCase(
            Renderer renderer,
            int propertyId,
            Color resultValue)
            : base(renderer, resultValue)
        {
            parentObject = renderer.gameObject;
            this.propertyId = propertyId;

            renderer.GetPropertyBlock(propertyBlock);
            startValue = propertyBlock.GetColor(propertyId);
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            target.GetPropertyBlock(propertyBlock);

            Color value = Color.LerpUnclamped(
                startValue,
                resultValue,
                EvaluateEase(State));

            propertyBlock.SetColor(propertyId, value);
            target.SetPropertyBlock(propertyBlock);
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(propertyId, resultValue);
            target.SetPropertyBlock(propertyBlock);
        }
    }
}