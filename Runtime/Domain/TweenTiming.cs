using System;

namespace SWeetJelly.Tween.Domain
{
    public static class TweenTiming
    {
        public const float MinimumDuration = 0.0001f;

        public static float Duration(float value) => Math.Max(MinimumDuration, Finite(value, nameof(value)));
        public static float Delay(float value) => Math.Max(0f, Finite(value, nameof(value)));
        public static float Delta(float value) => Math.Max(0f, Finite(value, nameof(value)));

        public static float Advance(float progress, float duration, float deltaTime)
        {
            Finite(duration, nameof(duration));
            float delta = Delta(deltaTime);
            return duration <= 0f ? 1f : Math.Min(1f, progress + Math.Min(1f, delta / duration));
        }

        public static float AdvanceDelay(float elapsed, float delay, float deltaTime)
        {
            Finite(delay, nameof(delay));
            return Math.Min(delay, elapsed + Delta(deltaTime));
        }

        private static float Finite(float value, string parameter)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(parameter, value, "Tween time must be finite.");
            return value;
        }
    }
}
