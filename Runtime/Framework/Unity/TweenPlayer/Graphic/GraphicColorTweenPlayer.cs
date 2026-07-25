using UnityEngine;
using UnityEngine.UI;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class GraphicColorTweenPlayer
        : TweenPlayerBehaviour<Graphic, Color>
    {
        protected override Color CurrentValue
        {
            get => TargetComponent.color;
            set => TargetComponent.color = value;
        }

        protected override TweenCase CreateTweenCase(Color endValue)
        {
            return TargetComponent.DOColor(endValue, duration);
        }

        protected override void IncrementLoopChangeValues()
        {
            Color delta = endValue - startValue;

            startValue = endValue;
            endValue += delta;
        }
    }
}