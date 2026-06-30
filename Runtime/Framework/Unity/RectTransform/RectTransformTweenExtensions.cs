using System;
using UnityEngine;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.Framework.Unity
{
    public static class RectTransformTweenExtensions
    {
        public static TweenCase DOAnchoredPosition(
            this RectTransform rectTransform,
            Vector2 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (rectTransform == null)
                throw new ArgumentNullException(nameof(rectTransform));

            return new AnchoredPositionTweenCase(rectTransform, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOAnchoredPosition3D(
            this RectTransform rectTransform,
            Vector3 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (rectTransform == null)
                throw new ArgumentNullException(nameof(rectTransform));

            return new AnchoredPosition3DTweenCase(rectTransform, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOAnchoredPositionWithVerticalOffset(
            this RectTransform rectTransform,
            Vector2 resultValue,
            AnimationCurve verticalOffset,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (rectTransform == null)
                throw new ArgumentNullException(nameof(rectTransform));

            return new AnchoredPositionWithVerticalOffsetTweenCase(
                    rectTransform,
                    resultValue,
                    verticalOffset)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOAnchoredPositionShake(
            this RectTransform rectTransform,
            float magnitude,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (rectTransform == null)
                throw new ArgumentNullException(nameof(rectTransform));

            return new AnchoredPositionShakeTweenCase(rectTransform, magnitude)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOSizeScale(
            this RectTransform rectTransform,
            float scale,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (rectTransform == null)
                throw new ArgumentNullException(nameof(rectTransform));

            return new RectTransformSizeTweenCase(
                    rectTransform,
                    rectTransform.sizeDelta * scale)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOSize(
            this RectTransform rectTransform,
            Vector2 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (rectTransform == null)
                throw new ArgumentNullException(nameof(rectTransform));

            return new RectTransformSizeTweenCase(rectTransform, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        // 기존 Vector3 API 호환용. sizeDelta는 Vector2이므로 내부에서는 Vector2로 변환.
        public static TweenCase DOSize(
            this RectTransform rectTransform,
            Vector3 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return rectTransform.DOSize(
                (Vector2)resultValue,
                duration,
                delay,
                unscaledTime,
                updateMethod);
        }
    }
}