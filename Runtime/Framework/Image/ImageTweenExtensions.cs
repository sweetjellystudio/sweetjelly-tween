using System;
using UnityEngine;
using UnityEngine.UI;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.Framework
{
    public static class ImageTweenExtensions
    {
        /// <summary>
        /// Changes Image fillAmount to the target value.
        /// </summary>
        public static TweenCase DOFillAmount(
            this Image image,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            return new ImageFillAmountTweenCase(image, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }
    }
}