using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class AnchoredPositionTweenCase : TweenValueCase<RectTransform, Vector2>
    {
        private readonly GameObject parentObject;

        public AnchoredPositionTweenCase(RectTransform rectTransform, Vector2 resultValue)
            : base(rectTransform, resultValue)
        {
            parentObject = rectTransform.gameObject;
            startValue = rectTransform.anchoredPosition;
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            target.anchoredPosition = Vector2.LerpUnclamped(
                startValue,
                resultValue,
                EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.anchoredPosition = resultValue;
        }
    }
}
