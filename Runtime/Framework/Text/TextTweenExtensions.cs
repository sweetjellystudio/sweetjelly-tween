using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.Framework
{
    public static class TextTweenExtensions
    {
        public static TweenCase DOFontSize(
            this Text text,
            int resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            return new UnityTextFontSizeTweenCase(text, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOFontSize(
            this TextMesh textMesh,
            int resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (textMesh == null)
                throw new ArgumentNullException(nameof(textMesh));

            return new TextMeshFontSizeTweenCase(textMesh, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOFade(
            this TextMesh textMesh,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (textMesh == null)
                throw new ArgumentNullException(nameof(textMesh));

            return new TextMeshFadeTweenCase(textMesh, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOColor(
            this TextMesh textMesh,
            Color resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (textMesh == null)
                throw new ArgumentNullException(nameof(textMesh));

            return new TextMeshColorTweenCase(textMesh, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOFontSize(
            this TMP_Text text,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            return new TMPTextFontSizeTweenCase(text, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }
    }
}