using System;
using UnityEngine;
using UnityEngine.UI;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework.Unity
{
    public static class GraphicRectTransformTweenExtensions
    {
        public static TweenCase DOAnchoredPosition(
            this Graphic graphic,
            Vector2 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (graphic == null)
                throw new ArgumentNullException(nameof(graphic));

            return graphic.rectTransform.DOAnchoredPosition(
                resultValue,
                duration,
                delay,
                unscaledTime,
                updateMethod);
        }

        public static TweenCase DOAnchoredPosition3D(
            this Graphic graphic,
            Vector3 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (graphic == null)
                throw new ArgumentNullException(nameof(graphic));

            return graphic.rectTransform.DOAnchoredPosition3D(
                resultValue,
                duration,
                delay,
                unscaledTime,
                updateMethod);
        }

        public static TweenCase DOAnchoredPositionShake(
            this Graphic graphic,
            float magnitude,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (graphic == null)
                throw new ArgumentNullException(nameof(graphic));

            return graphic.rectTransform.DOAnchoredPositionShake(
                magnitude,
                duration,
                delay,
                unscaledTime,
                updateMethod);
        }

        public static TweenCase DOSizeScale(
            this Graphic graphic,
            float scale,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (graphic == null)
                throw new ArgumentNullException(nameof(graphic));

            return graphic.rectTransform.DOSizeScale(
                scale,
                duration,
                delay,
                unscaledTime,
                updateMethod);
        }

        public static TweenCase DOSize(
            this Graphic graphic,
            Vector2 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (graphic == null)
                throw new ArgumentNullException(nameof(graphic));

            return graphic.rectTransform.DOSize(
                resultValue,
                duration,
                delay,
                unscaledTime,
                updateMethod);
        }

        public static TweenCase DOSize(
            this Graphic graphic,
            Vector3 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (graphic == null)
                throw new ArgumentNullException(nameof(graphic));

            return graphic.rectTransform.DOSize(
                resultValue,
                duration,
                delay,
                unscaledTime,
                updateMethod);
        }
    }
}