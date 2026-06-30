using System;

namespace SWeetJelly.Tween.Framework.Unity
{
    public abstract class TweenValueCase<TTarget, TValue> : TweenCase
        where TTarget : class
    {
        protected readonly TTarget target;
        protected TValue startValue;
        protected readonly TValue resultValue;

        public TTarget Target => target;
        public TValue StartValue => startValue;
        public TValue ResultValue => resultValue;

        protected TweenValueCase(TTarget target, TValue resultValue)
        {
            this.target = target ?? throw new ArgumentNullException(nameof(target));
            this.resultValue = resultValue;
        }
    }
}