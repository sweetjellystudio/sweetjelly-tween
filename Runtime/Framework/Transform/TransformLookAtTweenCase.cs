using UnityEngine;

namespace SWeetJelly.Tween.Framework
{
    public sealed class TransformLookAtTweenCase : TweenValueCase<Transform, Vector3>
    {
        private readonly GameObject parentObject;
        private readonly Quaternion startRotation;
        private readonly Quaternion targetRotation;

        public TransformLookAtTweenCase(Transform target, Vector3 lookAtPosition)
            : base(target, lookAtPosition)
        {
            parentObject = target.gameObject;

            startValue = target.position;
            startRotation = target.rotation;

            Vector3 direction = lookAtPosition - target.position;

            targetRotation = direction.sqrMagnitude <= Mathf.Epsilon
                ? target.rotation
                : Quaternion.LookRotation(direction);
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            target.rotation = Quaternion.SlerpUnclamped(
                startRotation,
                targetRotation,
                EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.rotation = targetRotation;
        }
    }
}