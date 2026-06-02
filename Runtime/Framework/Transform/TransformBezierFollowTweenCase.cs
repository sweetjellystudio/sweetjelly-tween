using System;
using UnityEngine;

namespace SWeetJelly.Tween.Framework
{
    public sealed class TransformBezierFollowTweenCase : TweenCase
    {
        private readonly Transform fromTransform;
        private readonly Transform toTransform;
        private readonly GameObject parentObject;

        private readonly Vector3 startPosition;
        private readonly float upOffset;
        private readonly float rightOffset;

        private Vector3 keyPoint1;
        private Vector3 keyPoint2;

        public TransformBezierFollowTweenCase(
            Transform fromTransform,
            Transform toTransform,
            float upOffset,
            float rightOffset)
        {
            this.fromTransform = fromTransform != null
                ? fromTransform
                : throw new ArgumentNullException(nameof(fromTransform));

            this.toTransform = toTransform != null
                ? toTransform
                : throw new ArgumentNullException(nameof(toTransform));

            parentObject = fromTransform.gameObject;
            startPosition = fromTransform.position;

            this.upOffset = upOffset;
            this.rightOffset = rightOffset;

            RecalculateKeyPoints();
        }

        public override bool Validate()
        {
            return parentObject != null && fromTransform != null && toTransform != null;
        }

        public override void Invoke(float deltaTime)
        {
            fromTransform.position = BezierUtility.EvaluateCubic(
                startPosition,
                keyPoint1,
                keyPoint2,
                toTransform.position,
                EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            if (fromTransform == null || toTransform == null)
                return;

            fromTransform.position = toTransform.position;
        }

        private void RecalculateKeyPoints()
        {
            Vector3 direction = toTransform.position - fromTransform.position;

            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                keyPoint1 = fromTransform.position;
                keyPoint2 = toTransform.position;
                return;
            }

            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, direction);
            Vector3 right = rotation * Vector3.right;
            Vector3 forwardNoise = direction.normalized * UnityEngine.Random.Range(-2f, 2f);

            keyPoint1 = fromTransform.position + Vector3.up * upOffset + right * rightOffset + forwardNoise;
            keyPoint2 = toTransform.position + Vector3.up * upOffset + right * rightOffset + forwardNoise;
        }
    }
}