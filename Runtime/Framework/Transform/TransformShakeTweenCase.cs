using System;
using UnityEngine;
using Random = UnityEngine.Random;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework
{
    public sealed class TransformShakeTweenCase : TweenCase
    {
        private readonly Transform target;
        private readonly GameObject parentObject;

        private readonly Vector3 startPosition;
        private readonly float magnitude;

        public TransformShakeTweenCase(Transform target, float magnitude)
        {
            this.target = target != null
                ? target
                : throw new ArgumentNullException(nameof(target));

            parentObject = target.gameObject;
            startPosition = target.position;
            this.magnitude = Mathf.Max(0f, magnitude);
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            float strength = 1f - EvaluateEase(State);

            target.position =
                startPosition + Random.onUnitSphere * magnitude * strength;
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.position = startPosition;
        }
    }
}