using System;
using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class ColorTweenCase : TweenCase
    {
        private readonly Color startValue;
        private readonly Color resultValue;
        private readonly Action<Color> callback;

        public ColorTweenCase(
            Color startValue,
            Color resultValue,
            Action<Color> callback)
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
            Color value = Color.LerpUnclamped(
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