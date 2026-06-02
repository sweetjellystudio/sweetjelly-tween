using UnityEngine;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework
{
    public sealed class TransformBezierPositionTweenCase : TweenValueCase<Transform, Vector3>
    {
        private readonly GameObject parentObject;

        private readonly Vector3 keyPoint1;
        private readonly Vector3 keyPoint2;

        public TransformBezierPositionTweenCase(
            Transform target,
            Vector3 resultValue,
            float upOffset,
            float rightOffset)
            : base(target, resultValue)
        {
            parentObject = target.gameObject;
            startValue = target.position;

            Vector3 direction = resultValue - startValue;

            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                keyPoint1 = startValue;
                keyPoint2 = resultValue;
                return;
            }

            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, direction);
            Vector3 right = rotation * Vector3.right;
            Vector3 forwardNoise = direction.normalized * UnityEngine.Random.Range(-2f, 2f);

            keyPoint1 = startValue + Vector3.up * upOffset + right * rightOffset + forwardNoise;
            keyPoint2 = resultValue + Vector3.up * upOffset + right * rightOffset + forwardNoise;
        }

        public TransformBezierPositionTweenCase(
            Transform target,
            Vector3 resultValue,
            Vector3 keyPoint1,
            Vector3 keyPoint2)
            : base(target, resultValue)
        {
            parentObject = target.gameObject;
            startValue = target.position;

            this.keyPoint1 = keyPoint1;
            this.keyPoint2 = keyPoint2;
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            target.position = BezierUtility.EvaluateCubic(
                startValue,
                keyPoint1,
                keyPoint2,
                resultValue,
                EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.position = resultValue;
        }
    }
}