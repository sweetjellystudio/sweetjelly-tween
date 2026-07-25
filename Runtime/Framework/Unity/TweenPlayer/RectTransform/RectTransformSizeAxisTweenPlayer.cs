using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class RectTransformSizeAxisTweenPlayer
        : RectTransformSizeAxisTweenPlayerBase
    {
        [SerializeField] private Axis axis = Axis.Height;

        protected override Axis TargetAxis => axis;
    }

    public abstract class RectTransformSizeAxisTweenPlayerBase
        : TweenPlayerBehaviour<RectTransform, float>
    {
        protected abstract Axis TargetAxis { get; }

        protected override float CurrentValue
        {
            get
            {
                Vector2 size = TargetComponent.sizeDelta;

                return TargetAxis == Axis.Width
                    ? size.x
                    : size.y;
            }
            set
            {
                Vector2 size = TargetComponent.sizeDelta;

                if (TargetAxis == Axis.Width)
                    size.x = value;
                else
                    size.y = value;

                TargetComponent.sizeDelta = size;
            }
        }

        protected override TweenCase CreateTweenCase(float endValue)
        {
            return new FloatTweenCase(CurrentValue, endValue, value => CurrentValue = value);
        }

        protected override void IncrementLoopChangeValues()
        {
            float delta = endValue - startValue;

            startValue = endValue;
            endValue += delta;
        }

        public enum Axis
        {
            Width = 0,
            Height = 1,
        }
    }
}
