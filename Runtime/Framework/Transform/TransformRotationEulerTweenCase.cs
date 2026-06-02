using UnityEngine;

namespace SWeetJelly.Tween.Framework
{
    public sealed class TransformRotationEulerTweenCase : TweenValueCase<Transform, Vector3>
    {
        private readonly GameObject parentObject;

        public TransformRotationEulerTweenCase(Transform transform, Vector3 resultValue)
            : base(transform, NormalizeTargetEuler(transform.eulerAngles, resultValue))
        {
            parentObject = transform.gameObject;
            startValue = transform.eulerAngles;
        }

        public override bool Validate() => parentObject != null && target != null;

        public override void Invoke(float deltaTime)
        {
            target.eulerAngles = Vector3.LerpUnclamped(startValue, resultValue, EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            if (target != null)
                target.eulerAngles = resultValue;
        }

        private static Vector3 NormalizeTargetEuler(Vector3 start, Vector3 result)
        {
            result.x = NormalizeAxis(start.x, result.x);
            result.y = NormalizeAxis(start.y, result.y);
            result.z = NormalizeAxis(start.z, result.z);
            return result;
        }

        private static float NormalizeAxis(float start, float result)
        {
            float delta = result - start;

            if (delta > 180f)
                result -= 360f;
            else if (delta < -180f)
                result += 360f;

            return result;
        }
    }
}