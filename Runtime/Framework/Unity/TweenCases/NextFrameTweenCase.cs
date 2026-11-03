using System;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class NextFrameTweenCase : TweenCase
    {
        private readonly Action callback;
        private readonly int framesOffset;

        private int elapsedFrames;

        public NextFrameTweenCase(Action callback, int framesOffset)
        {
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
            this.framesOffset = Math.Max(1, framesOffset);
            SetDuration(float.MaxValue);
        }

        protected override void ResetPlaybackState() => elapsedFrames = 0;

        public override bool Validate()
        {
            return callback != null;
        }

        public override void Invoke(float deltaTime)
        {
            elapsedFrames++;

            if (elapsedFrames >= framesOffset)
                Complete();
        }

        public override void DefaultComplete()
        {
            callback?.Invoke();
        }
    }
}
