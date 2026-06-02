using System;
using UnityEngine;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework
{
    public static class AnimationTweenExtensions
    {
        /// <summary>
        /// Waits until the Animation component stops playing.
        /// This method does not call Animation.Play().
        /// </summary>
        public static TweenCase WaitForEnd(
            this Animation animation,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (animation == null)
                throw new ArgumentNullException(nameof(animation));

            return new WaitForAnimationEndTweenCase(animation)
                .SetDelay(delay)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        /// <summary>
        /// Plays the Animation component and waits until it stops playing.
        /// </summary>
        public static TweenCase PlayAndWaitForEnd(
            this Animation animation,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (animation == null)
                throw new ArgumentNullException(nameof(animation));

            animation.Play();

            return new WaitForAnimationEndTweenCase(animation)
                .SetDelay(delay)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }
    }
}