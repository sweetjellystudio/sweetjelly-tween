using System;
using UnityEngine;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework.Unity
{
    public static class CameraTweenExtensions
    {
        /// <summary>
        /// Changes Camera orthographic size to the target value.
        /// </summary>
        public static TweenCase DOOrthographicSize(
            this Camera camera,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));

            return new CameraOrthographicSizeTweenCase(camera, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        /// <summary>
        /// Backward-compatible alias for DOOrthographicSize.
        /// </summary>
        public static TweenCase DOSize(
            this Camera camera,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return camera.DOOrthographicSize(
                resultValue,
                duration,
                delay,
                unscaledTime,
                updateMethod);
        }

        /// <summary>
        /// Changes Camera field of view to the target value.
        /// </summary>
        public static TweenCase DOFieldOfView(
            this Camera camera,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));

            return new CameraFieldOfViewTweenCase(camera, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }
    }
}