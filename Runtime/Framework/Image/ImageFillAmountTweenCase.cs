using UnityEngine;
using UnityEngine.UI;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework
{
    public sealed class ImageFillAmountTweenCase : TweenValueCase<Image, float>
    {
        private readonly GameObject parentObject;

        public ImageFillAmountTweenCase(Image image, float resultValue)
            : base(image, Mathf.Clamp01(resultValue))
        {
            parentObject = image.gameObject;
            startValue = image.fillAmount;
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            float value = Mathf.LerpUnclamped(
                startValue,
                resultValue,
                EvaluateEase(State));

            target.fillAmount = Mathf.Clamp01(value);
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.fillAmount = resultValue;
        }
    }
}