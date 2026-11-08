using System;

namespace SWeetJelly.Tween.Domain
{
    /// <summary>Pure finite timeline calculations, independent of a scheduler or Unity target.</summary>
    public static class TweenTimelineTiming
    {
        public static float ClampTime(float time, float duration)
        {
            if (float.IsNaN(time) || float.IsInfinity(time))
                throw new ArgumentOutOfRangeException(nameof(time));
            if (float.IsNaN(duration) || float.IsInfinity(duration) || duration < 0f)
                throw new ArgumentOutOfRangeException(nameof(duration));
            return Math.Max(0f, Math.Min(duration, time));
        }

        public static float Length(float duration, float delay, int repetitions)
        {
            if (repetitions <= 0) throw new ArgumentOutOfRangeException(nameof(repetitions));
            float result = TweenTiming.Delay(delay) + TweenTiming.Duration(duration) * repetitions;
            if (float.IsInfinity(result)) throw new ArgumentOutOfRangeException(nameof(duration));
            return result;
        }

        public static void Locate(float time, float duration, float delay, int repetitions,
            out int cycle, out float progress)
        {
            duration = TweenTiming.Duration(duration);
            float active = Math.Max(0f, ClampTime(time, Length(duration, delay, repetitions)) - TweenTiming.Delay(delay));
            cycle = Math.Min(repetitions - 1, (int)Math.Min(repetitions - 1d, Math.Floor(active / duration)));
            progress = Math.Min(1f, Math.Max(0f, (active - cycle * duration) / duration));
        }

        public static void Locate(float time, float duration, float delay, float[] observedSpans, int repetitions,
            out int cycle, out float progress)
        {
            float length = Length(duration, delay, repetitions, observedSpans);
            duration = TweenTiming.Duration(duration);
            delay = TweenTiming.Delay(delay);
            time = ClampTime(time, length);
            cycle = 0;
            while (cycle < observedSpans.Length && cycle < repetitions - 1 && time >= observedSpans[cycle])
            { time -= observedSpans[cycle]; cycle++; }
            if (cycle >= observedSpans.Length)
            {
                if (cycle == 0) time = Math.Max(0f, time - delay);
                int regular = (int)Math.Min(repetitions - cycle - 1d, Math.Floor(time / duration));
                time -= regular * duration;
                cycle += regular;
                progress = Math.Min(1f, Math.Max(0f, time / duration));
                return;
            }
            progress = Math.Min(1f, Math.Max(0f, (time - (cycle == 0 ? delay : 0f)) / duration));
        }

        public static float Length(float duration, float delay, int repetitions, float[] observedSpans)
        {
            if (observedSpans == null) throw new ArgumentNullException(nameof(observedSpans));
            if (repetitions <= 0 || observedSpans.Length > repetitions) throw new ArgumentOutOfRangeException(nameof(repetitions));
            duration = TweenTiming.Duration(duration);
            delay = TweenTiming.Delay(delay);
            float length = duration * (repetitions - observedSpans.Length) + (observedSpans.Length == 0 ? delay : 0f);
            for (int i = 0; i < observedSpans.Length; i++)
            {
                float span = observedSpans[i];
                if (float.IsNaN(span) || float.IsInfinity(span) || span < duration + (i == 0 ? delay : 0f))
                    throw new ArgumentOutOfRangeException(nameof(observedSpans));
                length += span;
            }
            if (float.IsInfinity(length)) throw new ArgumentOutOfRangeException(nameof(observedSpans));
            return length;
        }

        public static float Combine(float currentDuration, float childDuration, bool sequence)
        {
            ClampTime(0f, currentDuration);
            ClampTime(0f, childDuration);
            float result = sequence ? currentDuration + childDuration : Math.Max(currentDuration, childDuration);
            if (float.IsInfinity(result)) throw new ArgumentOutOfRangeException(nameof(childDuration));
            return result;
        }

        public static float ChildTime(float parentTime, float offset, float childDuration)
        {
            ClampTime(0f, offset);
            return ClampTime(ClampTime(parentTime, float.MaxValue) - offset, childDuration);
        }
    }
}
