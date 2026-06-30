using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformRotationQuaternionTweenCase : TweenValueCase<Transform, Quaternion>
    {
        private readonly GameObject parentObject;

        public TransformRotationQuaternionTweenCase(Transform transform, Quaternion resultValue)
            : base(transform, resultValue)
        {
            parentObject = transform.gameObject;
            startValue = transform.rotation;
        }

        public override bool Validate() => parentObject != null && target != null;

        public override void Invoke(float deltaTime)
        {
            target.rotation = Quaternion.LerpUnclamped(startValue, resultValue, EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            if (target != null)
                target.rotation = resultValue;
        }
    }
}