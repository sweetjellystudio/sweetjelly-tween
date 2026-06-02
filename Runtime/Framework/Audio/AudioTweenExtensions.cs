using System;
using UnityEngine;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.Framework
{
    public static class AudioTweenExtensions
    {
        public static TweenCase DOVolume(
            this AudioSource audioSource,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (audioSource == null)
                throw new ArgumentNullException(nameof(audioSource));

            return new AudioVolumeTweenCase(audioSource, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }
    }
}