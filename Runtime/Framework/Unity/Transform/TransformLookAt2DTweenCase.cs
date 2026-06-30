using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformLookAt2DTweenCase : TweenValueCase<Transform, Vector3>
    {
        private readonly GameObject parentObject;

        private readonly float startRotationZ;
        private readonly float targetRotationZ;

        public TransformLookAt2DTweenCase(
            Transform target,
            Vector3 lookAtPosition,
            LookAt2DType type)
            : base(target, lookAtPosition)
        {
            parentObject = target.gameObject;

            startRotationZ = target.eulerAngles.z;
            targetRotationZ = CalculateTargetRotationZ(target, lookAtPosition, type);
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            float z = Mathf.LerpAngle(
                startRotationZ,
                targetRotationZ,
                EvaluateEase(State));

            target.rotation = Quaternion.Euler(0f, 0f, z);
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.rotation = Quaternion.Euler(0f, 0f, targetRotationZ);
        }

        private static float CalculateTargetRotationZ(
            Transform target,
            Vector3 lookAtPosition,
            LookAt2DType type)
        {
            Vector3 direction = lookAtPosition - target.position;

            if (direction.sqrMagnitude <= Mathf.Epsilon)
                return target.eulerAngles.z;

            direction.Normalize();

            float rotationZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            if (type == LookAt2DType.Up)
                rotationZ -= 90f;

            return rotationZ;
        }
    }
}