using UnityEngine;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class RendererPropertyBlockFloatTweenCase : TweenValueCase<Renderer, float>
    {
        private readonly GameObject parentObject;
        private readonly MaterialPropertyBlock propertyBlock = new();
        private readonly int propertyId;

        public RendererPropertyBlockFloatTweenCase(
            Renderer renderer,
            int propertyId,
            float resultValue)
            : base(renderer, resultValue)
        {
            parentObject = renderer.gameObject;
            this.propertyId = propertyId;

            renderer.GetPropertyBlock(propertyBlock);
            startValue = propertyBlock.GetFloat(propertyId);
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            target.GetPropertyBlock(propertyBlock);

            float value = Mathf.LerpUnclamped(
                startValue,
                resultValue,
                EvaluateEase(State));

            propertyBlock.SetFloat(propertyId, value);
            target.SetPropertyBlock(propertyBlock);
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(propertyId, resultValue);
            target.SetPropertyBlock(propertyBlock);
        }
    }
}