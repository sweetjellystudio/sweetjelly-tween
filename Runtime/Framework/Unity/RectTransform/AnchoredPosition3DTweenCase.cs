using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class AnchoredPosition3DTweenCase : TweenValueCase<RectTransform, Vector3>
    {
        private readonly GameObject parentObject;

        public AnchoredPosition3DTweenCase(RectTransform rectTransform, Vector3 resultValue)
            : base(rectTransform, resultValue)
        {
            parentObject = rectTransform.gameObject;
            startValue = rectTransform.anchoredPosition3D;
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            target.anchoredPosition3D = Vector3.LerpUnclamped(
                startValue,
                resultValue,
                EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.anchoredPosition3D = resultValue;
        }
    }
}