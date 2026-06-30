using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class RectTransformSizeTweenCase : TweenValueCase<RectTransform, Vector2>
    {
        private readonly GameObject parentObject;

        public RectTransformSizeTweenCase(RectTransform rectTransform, Vector2 resultValue)
            : base(rectTransform, resultValue)
        {
            parentObject = rectTransform.gameObject;
            startValue = rectTransform.sizeDelta;
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            target.sizeDelta = Vector2.LerpUnclamped(
                startValue,
                resultValue,
                EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.sizeDelta = resultValue;
        }
    }
}