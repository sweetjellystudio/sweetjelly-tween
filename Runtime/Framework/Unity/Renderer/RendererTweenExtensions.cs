using System;
using UnityEngine;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework.Unity
{
    public static class RendererTweenExtensions
    {
        /// <summary>
        /// Changes a color property through MaterialPropertyBlock.
        /// </summary>
        public static TweenCase DOPropertyBlockColor(
            this Renderer renderer,
            int colorId,
            Color resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (renderer == null)
                throw new ArgumentNullException(nameof(renderer));

            return new RendererPropertyBlockColorTweenCase(renderer, colorId, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        /// <summary>
        /// Changes a float property through MaterialPropertyBlock.
        /// </summary>
        public static TweenCase DOPropertyBlockFloat(
            this Renderer renderer,
            int floatId,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (renderer == null)
                throw new ArgumentNullException(nameof(renderer));

            return new RendererPropertyBlockFloatTweenCase(renderer, floatId, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        // 기존 호출부 호환용 overload.
        // materialPropertyBlock 인자는 더 이상 내부에서 사용하지 않는다.
        public static TweenCase DOPropertyBlockColor(
            this Renderer renderer,
            int colorId,
            MaterialPropertyBlock materialPropertyBlock,
            Color resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return renderer.DOPropertyBlockColor(
                colorId,
                resultValue,
                duration,
                delay,
                unscaledTime,
                updateMethod);
        }

        // 기존 호출부 호환용 overload.
        // materialPropertyBlock 인자는 더 이상 내부에서 사용하지 않는다.
        public static TweenCase DOPropertyBlockFloat(
            this Renderer renderer,
            int floatId,
            MaterialPropertyBlock materialPropertyBlock,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return renderer.DOPropertyBlockFloat(
                floatId,
                resultValue,
                duration,
                delay,
                unscaledTime,
                updateMethod);
        }
    }
}