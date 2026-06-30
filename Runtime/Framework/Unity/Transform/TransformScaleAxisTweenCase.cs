using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformScaleAxisTweenCase : TweenValueCase<Transform, float>
    {
        private readonly GameObject parentObject;
        private readonly Axis axis;

        public TransformScaleAxisTweenCase(Transform transform, Axis axis, float resultValue)
            : base(transform, resultValue)
        {
            parentObject = transform.gameObject;
            this.axis = axis;

            startValue = axis switch
            {
                Axis.X => transform.localScale.x,
                Axis.Y => transform.localScale.y,
                Axis.Z => transform.localScale.z,
                _ => 0f
            };
        }

        public override bool Validate() => parentObject != null && target != null;

        public override void Invoke(float deltaTime)
        {
            Vector3 scale = target.localScale;
            float value = Mathf.LerpUnclamped(startValue, resultValue, EvaluateEase(State));
            Apply(ref scale, value);
            target.localScale = scale;
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            Vector3 scale = target.localScale;
            Apply(ref scale, resultValue);
            target.localScale = scale;
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