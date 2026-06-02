using System;
using UnityEngine;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework
{
    public enum Axis
    {
        X,
        Y,
        Z
    }

    public enum LookAt2DType
    {
        Up,
        Right,
        Forward
    }

    public static class TransformTweenExtensions
    {
        public static TweenCase DOMove(
            this Component component,
            Vector3 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOMove(resultValue, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOMove(
            this Transform transform,
            Vector3 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformPositionTweenCase(transform, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOLocalMove(
            this Component component,
            Vector3 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOLocalMove(resultValue, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOLocalMove(
            this Transform transform,
            Vector3 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformLocalPositionTweenCase(transform, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOMoveX(
            this Component component,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOMoveX(resultValue, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOMoveX(
            this Transform transform,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformPositionAxisTweenCase(transform, Axis.X, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOMoveY(
            this Component component,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOMoveY(resultValue, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOMoveY(
            this Transform transform,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformPositionAxisTweenCase(transform, Axis.Y, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOMoveZ(
            this Component component,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOMoveZ(resultValue, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOMoveZ(
            this Transform transform,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformPositionAxisTweenCase(transform, Axis.Z, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOMoveXZ(
            this Component component,
            float resultValueX,
            float resultValueZ,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOMoveXZ(resultValueX, resultValueZ, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOMoveXZ(
            this Transform transform,
            float resultValueX,
            float resultValueZ,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformPositionXzTweenCase(transform, resultValueX, resultValueZ)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOLocalMoveX(
            this Component component,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOLocalMoveX(resultValue, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOLocalMoveX(
            this Transform transform,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformLocalPositionAxisTweenCase(transform, Axis.X, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOLocalMoveY(
            this Component component,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOLocalMoveY(resultValue, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOLocalMoveY(
            this Transform transform,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformLocalPositionAxisTweenCase(transform, Axis.Y, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOLocalMoveZ(
            this Component component,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOLocalMoveZ(resultValue, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOLocalMoveZ(
            this Transform transform,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformLocalPositionAxisTweenCase(transform, Axis.Z, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOScale(
            this Component component,
            Vector3 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOScale(resultValue, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOScale(
            this Transform transform,
            Vector3 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformScaleTweenCase(transform, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOScale(
            this Component component,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOScale(resultValue, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOScale(
            this Transform transform,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return transform.DOScale(
                new Vector3(resultValue, resultValue, resultValue),
                duration,
                delay,
                unscaledTime,
                updateMethod);
        }

        public static TweenCase DOScaleX(
            this Component component,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOScaleX(resultValue, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOScaleX(
            this Transform transform,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformScaleAxisTweenCase(transform, Axis.X, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOScaleY(
            this Component component,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOScaleY(resultValue, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOScaleY(
            this Transform transform,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformScaleAxisTweenCase(transform, Axis.Y, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOScaleZ(
            this Component component,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOScaleZ(resultValue, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOScaleZ(
            this Transform transform,
            float resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformScaleAxisTweenCase(transform, Axis.Z, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DORotate(
            this Component component,
            Vector3 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DORotate(resultValue, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DORotate(
            this Transform transform,
            Vector3 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformRotationEulerTweenCase(transform, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DORotate(
            this Component component,
            Quaternion resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DORotate(resultValue, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DORotate(
            this Transform transform,
            Quaternion resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformRotationQuaternionTweenCase(transform, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOLocalRotate(
            this Component component,
            Vector3 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOLocalRotate(resultValue, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOLocalRotate(
            this Transform transform,
            Vector3 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformLocalRotationEulerTweenCase(transform, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOLocalRotate(
            this Component component,
            Quaternion resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOLocalRotate(resultValue, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOLocalRotate(
            this Transform transform,
            Quaternion resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformLocalRotationQuaternionTweenCase(transform, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DORotateConstant(
            this Component component,
            Vector3 rotationVector,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DORotateConstant(rotationVector, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DORotateConstant(
            this Transform transform,
            Vector3 rotationVector,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformConstantRotationTweenCase(transform, rotationVector)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOPath(
            this Component component,
            Vector3[] path,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOPath(path, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOPath(
            this Transform transform,
            Vector3[] path,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformPathTweenCase(transform, path)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOBezierMove(
            this Component component,
            Vector3 resultValue,
            float upOffset,
            float rightOffset,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOBezierMove(resultValue, upOffset, rightOffset, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOBezierMove(
            this Transform transform,
            Vector3 resultValue,
            float upOffset,
            float rightOffset,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformBezierPositionTweenCase(transform, resultValue, upOffset, rightOffset)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOFollow(
            this Component component,
            Transform target,
            float speed,
            float minimumDistance,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOFollow(target, speed, minimumDistance, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOFollow(
            this Transform transform,
            Transform target,
            float speed,
            float minimumDistance,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformFollowTweenCase(transform, target, speed, minimumDistance)
                .SetDelay(delay)
                .SetDuration(float.MaxValue)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOLookAt(
            this Component component,
            Vector3 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOLookAt(resultValue, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOLookAt(
            this Transform transform,
            Vector3 resultValue,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformLookAtTweenCase(transform, resultValue)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOLookAt2D(
            this Component component,
            Vector3 resultValue,
            LookAt2DType type,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOLookAt2D(resultValue, type, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOLookAt2D(
            this Transform transform,
            Vector3 resultValue,
            LookAt2DType type,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformLookAt2DTweenCase(transform, resultValue, type)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOShake(
            this Component component,
            float magnitude,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return component.RequireTransform()
                .DOShake(magnitude, duration, delay, unscaledTime, updateMethod);
        }

        public static TweenCase DOShake(
            this Transform transform,
            float magnitude,
            float duration,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new TransformShakeTweenCase(transform, magnitude)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        private static Transform RequireTransform(this Component component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            return component.transform;
        }
    }
}