using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformLocalPositionAxisTweenCase : TweenValueCase<Transform, float>
    {
        private readonly GameObject parentObject;
        private readonly Axis axis;

        public TransformLocalPositionAxisTweenCase(Transform transform, Axis axis, float resultValue)
            : base(transform, resultValue)
        {
            parentObject = transform.gameObject;
            this.axis = axis;

            startValue = axis switch
            {
                Axis.X => transform.localPosition.x,
                Axis.Y => transform.localPosition.y,
                Axis.Z => transform.localPosition.z,
                _ => 0f
            };
        }

        public override bool Validate() => parentObject != null && target != null;

        public override void Invoke(float deltaTime)
        {
            Vector3 position = target.localPosition;
            float value = Mathf.LerpUnclamped(startValue, resultValue, EvaluateEase(State));
            Apply(ref position, value);
            target.localPosition = position;
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            Vector3 position = target.localPosition;
            Apply(ref position, resultValue);
            target.localPosition = position;
        }

        private void Apply(ref Vector3 value, float axisValue)
        {
            switch (axis)
            {
                case Axis.X:
                    value.x = axisValue;
                    break;
                case Axis.Y:
                    value.y = axisValue;
                    break;
                case Axis.Z:
                    value.z = axisValue;
                    break;
            }
        }
    }
}