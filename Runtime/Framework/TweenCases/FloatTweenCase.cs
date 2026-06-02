using System;
using UnityEngine;

namespace SWeetJelly.Tween.Framework
{
    public sealed class FloatTweenCase : TweenCase
    {
        private readonly float startValue;
        private readonly float resultValue;
        private readonly Action<float> callback;

        public FloatTweenCase(
            float startValue,
            float resultValue,
            Action<float> callback)
        {
            this.startValue = startValue;
            this.resultValue = resultValue;
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        public override bool Validate()
        {
            return callback != null;
        }

        public override void Invoke(float deltaTime)
        {
            float value = Mathf.LerpUnclamped(
                startValue,
                resultValue,
                EvaluateEase(State));

            callback.Invoke(value);
        }

        public override void DefaultComplete()
        {
            callback?.Invoke(resultValue);
        }
    }
}