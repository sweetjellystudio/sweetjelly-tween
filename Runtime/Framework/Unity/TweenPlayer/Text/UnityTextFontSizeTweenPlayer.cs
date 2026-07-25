using UnityEngine.UI;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class UnityTextFontSizeTweenPlayer
        : TweenPlayerBehaviour<Text, int>
    {
        protected override int CurrentValue
        {
            get => TargetComponent.fontSize;
            set => TargetComponent.fontSize = value;
        }

        protected override TweenCase CreateTweenCase(int endValue) => TargetComponent.DOFontSize(endValue, duration);

        protected override void IncrementLoopChangeValues()
        {
            int delta = endValue - startValue;
            startValue = endValue;
            endValue += delta;
        }
    }
}
