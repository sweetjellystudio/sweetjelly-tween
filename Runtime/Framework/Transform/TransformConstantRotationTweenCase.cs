using UnityEngine;

namespace SWeetJelly.Tween.Framework
{
    public sealed class TransformConstantRotationTweenCase : TweenCase
    {
        private readonly Transform target;
        private readonly GameObject parentObject;
        private readonly Vector3 rotationVector;

        public TransformConstantRotationTweenCase(Transform target, Vector3 rotationVector)
        {
            this.target = target;
            parentObject = target.gameObject;
            this.rotationVector = rotationVector;
        }

        public override bool Validate() => parentObject != null && target != null;

        public override void Invoke(float deltaTime)
        {
            target.Rotate(rotationVector * deltaTime);
        }

        public override void DefaultComplete()
        {
        }
    }
}