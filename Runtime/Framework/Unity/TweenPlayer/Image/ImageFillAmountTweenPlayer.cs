using UnityEngine;
using UnityEngine.UI;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class ImageFillAmountTweenPlayer
        : TweenPlayerBehaviour<Image, float>
    {
        protected override float CurrentValue
        {
            get => TargetComponent.fillAmount;
            set => TargetComponent.fillAmount = Mathf.Clamp01(value);
        }

        protected override TweenCase CreateTweenCase(float endValue) => TargetComponent.DOFillAmount(endValue, duration);

        protected override void IncrementLoopChangeValues()
        {
            float delta = endValue - startValue;
            startValue = endValue;
            endValue += delta;
        }
    }
}
