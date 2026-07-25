using TMPro;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TMPTextFontSizeTweenPlayer
        : TweenPlayerBehaviour<TMP_Text, float>
    {
        protected override float CurrentValue
        {
            get => TargetComponent.fontSize;
            set => TargetComponent.fontSize = value;
        }

        protected override TweenCase CreateTweenCase(float endValue) => TargetComponent.DOFontSize(endValue, duration);

        protected override void IncrementLoopChangeValues()
        {
            float delta = endValue - startValue;
            startValue = endValue;
            endValue += delta;
        }
    }
}
