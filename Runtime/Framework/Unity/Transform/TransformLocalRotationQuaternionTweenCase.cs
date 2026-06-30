using UnityEngine;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformLocalRotationQuaternionTweenCase : TweenValueCase<Transform, Quaternion>
    {
        private readonly GameObject parentObject;

        public TransformLocalRotationQuaternionTweenCase(Transform transform, Quaternion resultValue)
            : base(transform, resultValue)
        {
            parentObject = transform.gameObject;
            startValue = transform.localRotation;
        }

        public override bool Validate() => parentObject != null && target != null;

        public override void Invoke(float deltaTime)
        {
            target.localRotation = Quaternion.LerpUnclamped(startValue, resultValue, EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            if (target != null)
                target.localRotation = resultValue;
        }
    }
}