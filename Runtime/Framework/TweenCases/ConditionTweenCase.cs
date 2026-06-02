using System;

namespace SWeetJelly.Tween.Framework
{
    public sealed class ConditionTweenCase : TweenCase
    {
        private readonly Action<ConditionTweenCase> callback;

        public ConditionTweenCase(Action<ConditionTweenCase> callback)
        {
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        public override bool Validate()
        {
            return callback != null;
        }

        public override void Invoke(float deltaTime)
        {
            callback.Invoke(this);
        }

        public override void DefaultComplete()
        {
        }
    }
}