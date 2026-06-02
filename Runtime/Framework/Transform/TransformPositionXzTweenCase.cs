using UnityEngine;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework
{
    public sealed class TransformPositionXzTweenCase : TweenCase
    {
        private readonly Transform target;
        private readonly GameObject parentObject;

        private readonly float startX;
        private readonly float startZ;
        private readonly float resultX;
        private readonly float resultZ;

        public TransformPositionXzTweenCase(Transform target, float resultX, float resultZ)
        {
            this.target = target;
            parentObject = target.gameObject;

            startX = target.position.x;
            startZ = target.position.z;

            this.resultX = resultX;
            this.resultZ = resultZ;
        }

        public override bool Validate() => parentObject != null && target != null;

        public override void Invoke(float deltaTime)
        {
            float eased = EvaluateEase(State);

            Vector3 position = target.position;
            position.x = Mathf.LerpUnclamped(startX, resultX, eased);
            position.z = Mathf.LerpUnclamped(startZ, resultZ, eased);
            target.position = position;
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            Vector3 position = target.position;
            position.x = resultX;
            position.z = resultZ;
            target.position = position;
        }
    }
}