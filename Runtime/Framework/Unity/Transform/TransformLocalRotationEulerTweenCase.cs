using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformLocalRotationEulerTweenCase : TweenValueCase<Transform, Vector3>
    {
        private readonly GameObject parentObject;

        public TransformLocalRotationEulerTweenCase(Transform transform, Vector3 resultValue)
            : base(transform, resultValue)
        {
            parentObject = transform.gameObject;
            startValue = transform.localEulerAngles;
        }

        public override bool Validate() => parentObject != null && target != null;

        public override void Invoke(float deltaTime)
        {
            target.localEulerAngles = Vector3.LerpUnclamped(startValue, resultValue, EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            if (target != null)
                target.localEulerAngles = resultValue;
        }
    }
}