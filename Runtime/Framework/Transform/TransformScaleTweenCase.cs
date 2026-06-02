using UnityEngine;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework
{
    public sealed class TransformScaleTweenCase : TweenValueCase<Transform, Vector3>
    {
        private readonly GameObject parentObject;

        public TransformScaleTweenCase(Transform transform, Vector3 resultValue)
            : base(transform, resultValue)
        {
            parentObject = transform.gameObject;
            startValue = transform.localScale;
        }

        public override bool Validate() => parentObject != null && target != null;

        public override void Invoke(float deltaTime)
        {
            target.localScale = Vector3.LerpUnclamped(startValue, resultValue, EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            if (target != null)
                target.localScale = resultValue;
        }
    }
}