using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformPositionAxisTweenCase : TweenValueCase<Transform, float>
    {
        private readonly GameObject parentObject;
        private readonly Axis axis;

        public TransformPositionAxisTweenCase(Transform transform, Axis axis, float resultValue)
            : base(transform, resultValue)
        {
            parentObject = transform.gameObject;
            this.axis = axis;

            startValue = axis switch
            {
                Axis.X => transform.position.x,
                Axis.Y => transform.position.y,
                Axis.Z => transform.position.z,
                _ => 0f
            };
        }

        public override bool Validate() => parentObject != null && target != null;

        public override void Invoke(float deltaTime)
        {
            Vector3 position = target.position;
            float value = Mathf.LerpUnclamped(startValue, resultValue, EvaluateEase(State));
            Apply(ref position, value);
            target.position = position;
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            Vector3 position = target.position;
            Apply(ref position, resultValue);
            target.position = position;
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
