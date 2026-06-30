using System;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class WaitUntilTweenCase : TweenCase
    {
        private readonly Func<bool> predicate;

        public WaitUntilTweenCase(Func<bool> predicate)
        {
            this.predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        public override bool Validate()
        {
            return predicate != null;
        }

        public override void Invoke(float deltaTime)
        {
            if (predicate.Invoke())
                Complete();
        }

        public override void DefaultComplete()
        {
        }
    }
}