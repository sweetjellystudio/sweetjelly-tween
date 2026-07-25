using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class RectTransformAnchoredPositionAxisTweenPlayer
        : RectTransformAnchoredPositionAxisTweenPlayerBase
    {
        [SerializeField] private Axis axis = Axis.Y;

        protected override Axis TargetAxis => axis;
    }

    public abstract class RectTransformAnchoredPositionAxisTweenPlayerBase
        : TweenPlayerBehaviour<RectTransform, float>
    {
        protected abstract Axis TargetAxis { get; }

        protected override float CurrentValue
        {
            get
            {
                Vector2 position = TargetComponent.anchoredPosition;

                return TargetAxis == Axis.X
                    ? position.x
                    : position.y;
            }
            set
            {
                Vector2 position = TargetComponent.anchoredPosition;

                if (TargetAxis == Axis.X)
                    position.x = value;
                else
                    position.y = value;

                TargetComponent.anchoredPosition = position;
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
            X = 0,
            Y = 1,
        }
    }
}
