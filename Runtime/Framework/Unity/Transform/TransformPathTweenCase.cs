using System;
using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformPathTweenCase : TweenCase
    {
        private readonly Transform target;
        private readonly GameObject parentObject;

        private readonly Vector3 startValue;
        private readonly Vector3[] path;
        private readonly float[] maxStateValues;

        private int index;

        public TransformPathTweenCase(Transform target, Vector3[] path)
        {
            this.target = target != null
                ? target
                : throw new ArgumentNullException(nameof(target));

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Length == 0)
                throw new ArgumentException("Path must contain at least one point.", nameof(path));

            parentObject = target.gameObject;

            startValue = target.position;
            this.path = (Vector3[])path.Clone();
            maxStateValues = new float[path.Length];

            BuildStateRanges();
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            UpdateIndex();

            float eased;

            if (index == 0)
            {
                eased = EvaluateEase(Mathf.InverseLerp(0f, maxStateValues[0], State));

                target.position = Vector3.LerpUnclamped(
                    startValue,
                    path[0],
                    eased);
            }
            else
            {
                eased = EvaluateEase(Mathf.InverseLerp(
                    maxStateValues[index - 1],
                    maxStateValues[index],
                    State));

                target.position = Vector3.LerpUnclamped(
                    path[index - 1],
                    path[index],
                    eased);
            }
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.position = path[path.Length - 1];
        }

        private void BuildStateRanges()
        {
            float[] distances = new float[path.Length];
            float totalDistance = 0f;

            distances[0] = Vector3.Distance(startValue, path[0]);
            totalDistance += distances[0];

            for (int i = 1; i < path.Length; i++)
            {
                distances[i] = Vector3.Distance(path[i - 1], path[i]);
                totalDistance += distances[i];
            }

            if (totalDistance <= Mathf.Epsilon)
            {
                for (int i = 0; i < maxStateValues.Length; i++)
                    maxStateValues[i] = (i + 1f) / maxStateValues.Length;

                return;
            }

            float state = 0f;

            for (int i = 0; i < path.Length; i++)
            {
                state += distances[i] / totalDistance;
                maxStateValues[i] = state;
            }

            maxStateValues[maxStateValues.Length - 1] = 1f;
        }

        private void UpdateIndex()
        {
            for (int i = 0; i < maxStateValues.Length; i++)
            {
                if (State <= maxStateValues[i])
                {
                    index = i;
                    return;
                }
            }

            index = maxStateValues.Length - 1;
        }
    }
}