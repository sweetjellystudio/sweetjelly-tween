using System;
using UnityEngine;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.Framework
{
    public static class SpriteRendererTweenExtensions
    {
        /// <summary>
        /// Changes SpriteRenderer color to the target value.
        /// </summary>
        public static TweenCase DOColor(
            this SpriteRenderer spriteRenderer,
            Color resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (spriteRenderer == null)
                throw new ArgumentNullException(nameof(spriteRenderer));

            return new SpriteRendererColorTweenCase(spriteRenderer, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        /// <summary>
        /// Changes SpriteRenderer color alpha to the target value.
        /// </summary>
        public static TweenCase DOFade(
            this SpriteRenderer spriteRenderer,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (spriteRenderer == null)
                throw new ArgumentNullException(nameof(spriteRenderer));

            return new SpriteRendererFadeTweenCase(spriteRenderer, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }
    }
}