using UnityEngine;

namespace SWeetJelly.Tween.Framework
{
    public sealed class CanvasGroupFadeTweenCase : TweenValueCase<CanvasGroup, float>
    {
        private readonly GameObject parentObject;

        public CanvasGroupFadeTweenCase(
            CanvasGroup canvasGroup,
            float resultValue)
            : base(canvasGroup, Mathf.Clamp01(resultValue))
        {
            parentObject = canvasGroup.gameObject;
            startValue = canvasGroup.alpha;
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            float alpha = Mathf.LerpUnclamped(
                startValue,
                resultValue,
                EvaluateEase(State));

            target.alpha = Mathf.Clamp01(alpha);
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.alpha = resultValue;
        }
    }
}