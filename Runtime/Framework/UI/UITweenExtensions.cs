using System;
using UnityEngine;
using UnityEngine.UI;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.Framework
{
    public static class UITweenExtensions
    {
        public static TweenCase DOPreferredHeight(
            this LayoutElement layoutElement,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (layoutElement == null)
                throw new ArgumentNullException(nameof(layoutElement));

            return new LayoutElementPreferredHeightTweenCase(layoutElement, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOFade(
            this CanvasGroup canvasGroup,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (canvasGroup == null)
                throw new ArgumentNullException(nameof(canvasGroup));

            return new CanvasGroupFadeTweenCase(canvasGroup, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }
    }
}