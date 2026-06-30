using UnityEngine;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformPositionTweenCase : TweenValueCase<Transform, Vector3>
    {
        private readonly GameObject parentObject;

        public TransformPositionTweenCase(Transform transform, Vector3 resultValue)
            : base(transform, resultValue)
        {
            parentObject = transform.gameObject;
            startValue = transform.position;
        }

        public override bool Validate() => parentObject != null && target != null;

        public override void Invoke(float deltaTime)
        {
            target.position = Vector3.LerpUnclamped(startValue, resultValue, EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            if (target != null)
                target.position = resultValue;
        }
    }
}