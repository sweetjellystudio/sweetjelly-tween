using System;
using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class WaitForAnimationEndTweenCase : TweenCase
    {
        private readonly Animation animation;

        public WaitForAnimationEndTweenCase(Animation animation)
        {
            this.animation = animation != null
                ? animation
                : throw new ArgumentNullException(nameof(animation));

            duration = float.MaxValue;
        }

        public override bool Validate()
        {
            return animation != null;
        }

        public override void Invoke(float deltaTime)
        {
            if (!animation.isPlaying)
                Complete();
        }

        public override void DefaultComplete()
        {
        }
    }
}