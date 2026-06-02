using System;
using UnityEngine;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.Framework
{
    [Serializable]
    public sealed class AnimationCurveEasingFunction : IEasingFunction
    {
        [SerializeField] private string name = string.Empty;
        [SerializeField] private AnimationCurve easingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        private float totalEasingTime = 1f;

        public string Name => name;

        public AnimationCurveEasingFunction(AnimationCurve animationCurve)
        {
            this.easingCurve = animationCurve ?? AnimationCurve.Linear(0f, 0f, 1f, 1f);
            Initialise();
        }

        public void Initialise()
        {
            if (easingCurve == null || easingCurve.length == 0)
            {
                totalEasingTime = 1f;
                return;
            }

            totalEasingTime = easingCurve.keys[easingCurve.length - 1].time;

            if (totalEasingTime <= 0f)
                totalEasingTime = 1f;
        }

        public float Evaluate(float t)
        {
            t = Clamp01(t);
            return easingCurve.Evaluate(t * totalEasingTime);
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
                return 0f;

            if (value > 1f)
                return 1f;

            return value;
        }
    }
}