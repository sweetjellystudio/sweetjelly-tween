using System;
using UnityEngine;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.Framework
{
    public static class TweenCases
    {
        public static TweenCase DelayedCall(
            float delay,
            Action callback,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return new DelayTweenCase()
                .SetDuration(delay)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod)
                .AddOnCompleted(callback);
        }

        public static TweenCase DOFloat(
            float startValue,
            float resultValue,
            float duration,
            Action<float> callback,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return new FloatTweenCase(startValue, resultValue, callback)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOColor(
            Color startValue,
            Color resultValue,
            float duration,
            Action<Color> callback,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return new ColorTweenCase(startValue, resultValue, callback)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOAction<T>(
            T startValue,
            T resultValue,
            float duration,
            Action<T, T, float> callback,
            float delay = 0f,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return new ActionTweenCase<T>(startValue, resultValue, callback)
                .SetDelay(delay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOWaitForCondition(
            Action<ConditionTweenCase> callback,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return new ConditionTweenCase(callback)
                .SetDuration(float.MaxValue)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOWaitUntil(
            Func<bool> predicate,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return new WaitUntilTweenCase(predicate)
                .SetDuration(float.MaxValue)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }

        public static TweenCase NextFrame(
            Action callback,
            int framesOffset = 1,
            bool unscaledTime = false,
            UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return new NextFrameTweenCase(callback, framesOffset)
                .SetDuration(float.MaxValue)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);
        }
    }
}