using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public abstract class TransformVector3AxisTweenPlayerBase
        : TweenPlayerBehaviour<Transform, float>
    {
        protected abstract Axis TargetAxis { get; }
        protected abstract Vector3 CurrentVectorValue { get; set; }

        protected override float CurrentValue
        {
            get
            {
                Vector3 value = CurrentVectorValue;

                return TargetAxis switch
                {
                    Axis.X => value.x,
                    Axis.Y => value.y,
                    _ => value.z,
                };
            }
            set
            {
                Vector3 vectorValue = CurrentVectorValue;

                switch (TargetAxis)
                {
                    case Axis.X:
                        vectorValue.x = value;
                        break;
                    case Axis.Y:
                        vectorValue.y = value;
                        break;
                    default:
                        vectorValue.z = value;
                        break;
                }

                CurrentVectorValue = vectorValue;
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
            Z = 2,
        }
    }
}
