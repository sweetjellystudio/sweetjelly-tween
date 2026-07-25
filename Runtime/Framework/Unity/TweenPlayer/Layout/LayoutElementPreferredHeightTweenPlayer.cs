using UnityEngine.UI;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class LayoutElementPreferredHeightTweenPlayer
        : TweenPlayerBehaviour<LayoutElement, float>
    {
        protected override float CurrentValue
        {
            get => TargetComponent.preferredHeight;
            set => TargetComponent.preferredHeight = value;
        }

        protected override TweenCase CreateTweenCase(float endValue) => TargetComponent.DOPreferredHeight(endValue, duration);

        protected override void IncrementLoopChangeValues()
        {
            float delta = endValue - startValue;
            startValue = endValue;
            endValue += delta;
        }
    }
}
