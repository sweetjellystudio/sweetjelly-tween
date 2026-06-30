using System;
using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformFollowTweenCase : TweenCase
    {
        private readonly Transform target;
        private readonly Transform followTarget;
        private readonly GameObject parentObject;

        private readonly float speed;
        private readonly float minimumDistanceSqr;

        public TransformFollowTweenCase(
            Transform target,
            Transform followTarget,
            float speed,
            float minimumDistance)
        {
            this.target = target != null
                ? target
                : throw new ArgumentNullException(nameof(target));

            this.followTarget = followTarget != null
                ? followTarget
                : throw new ArgumentNullException(nameof(followTarget));

            parentObject = target.gameObject;

            this.speed = Mathf.Max(0f, speed);
            minimumDistanceSqr = minimumDistance * minimumDistance;
        }

        public override bool Validate()
        {
            return parentObject != null && target != null && followTarget != null;
        }

        public override void Invoke(float deltaTime)
        {
            target.position = Vector3.MoveTowards(
                target.position,
                followTarget.position,
                deltaTime * speed);

            if (Vector3.SqrMagnitude(target.position - followTarget.position) <= minimumDistanceSqr)
                Complete();
        }

        public override void DefaultComplete()
        {
            if (target == null || followTarget == null)
                return;

            target.position = followTarget.position;
        }
    }
}