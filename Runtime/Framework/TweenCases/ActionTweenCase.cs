using System;

namespace SWeetJelly.Tween.Framework
{
    public sealed class ActionTweenCase<T> : TweenCase
    {
        private readonly T startValue;
        private readonly T resultValue;
        private readonly Action<T, T, float> callback;

        public ActionTweenCase(
            T startValue,
            T resultValue,
            Action<T, T, float> callback)
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
            callback.Invoke(startValue, resultValue, EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            callback?.Invoke(startValue, resultValue, 1f);
        }
    }
}