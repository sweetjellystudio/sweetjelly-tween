using System;
using UnityEngine;
using UnityEngine.UI;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework
{
    public static class GraphicTweenExtensions
    {
        /// <summary>
        /// Changes Graphic color to the target value.
        /// </summary>
        public static TweenCase DOColor(
            this Graphic graphic,
            Color resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (graphic == null)
                throw new ArgumentNullException(nameof(graphic));

            return new GraphicColorTweenCase(graphic, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        /// <summary>
        /// Changes Graphic alpha to the target value.
        /// </summary>
        public static TweenCase DOFade(
            this Graphic graphic,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (graphic == null)
                throw new ArgumentNullException(nameof(graphic));

            return new GraphicFadeTweenCase(graphic, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }
    }
}