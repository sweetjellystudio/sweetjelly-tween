using SWeetJelly.Tween.Contract;
using System;

namespace SWeetJelly.Tween.Domain
{
    public sealed class CustomEasingFunction : IEasingFunction
    {
        private readonly Func<float, float> evaluator;

        public CustomEasingFunction(Func<float, float> evaluator)
        {
            this.evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        }

        public float Evaluate(float t)
        {
            return evaluator(EasingMath.Clamp01(t));
        }
    }

    public class LinearEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            return EasingMath.Clamp01(t);
        }
    }

    public class QuadInEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = EasingMath.Clamp01(t);
            return t * t;
        }
    }

    public class QuadOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = EasingMath.Clamp01(t);
            return t * (2 - t);
        }
    }

    public class QuadInOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = EasingMath.Clamp01(t);
            return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
        }
    }

    public class CubicInEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = EasingMath.Clamp01(t);
            return t * t * t;
        }
    }

    public class CubicOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = EasingMath.Clamp01(t);
            return (--t) * t * t + 1;
        }
    }

    public class CubicInOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return t < 0.5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
        }
    }

    public class QuartInEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return t * t * t * t;
        }
    }

    public class QuartOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return 1 - (--t) * t * t * t;
        }
    }

    public class QuartInOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return t < 0.5f ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;
        }
    }

    public class QuintInEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return t * t * t * t * t;
        }
    }

    public class QuintOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return 1 + (--t) * t * t * t * t;
        }
    }

    public class QuintInOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return t < 0.5f ? 16 * t * t * t * t * t : 1 + 16 * (--t) * t * t * t * t;
        }
    }

    public class SineInEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return 1 - (float)Math.Cos((t * Math.PI) / 2);
        }
    }

    public class SineOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return (float)Math.Sin((t * Math.PI) / 2);
        }
    }

    public class SineInOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return -(float)(Math.Cos(Math.PI * t) - 1) / 2;
        }
    }

    public class CircInEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return 1 - (float)Math.Sqrt(1 - t * t);
        }
    }

    public class CircOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return (float)Math.Sqrt(1 - (--t) * t);
        }
    }

    public class CircInOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return t < 0.5f
                ? (1 - (float)Math.Sqrt(1 - 4 * t * t)) / 2
                : ((float)Math.Sqrt(1 - 4 * (--t) * t) + 1) / 2;
        }
    }

    public class ExpoInEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return t == 0 ? 0 : (float)Math.Pow(2, 10 * (t - 1));
        }
    }

    public class ExpoOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return t == 1 ? 1 : 1 - (float)Math.Pow(2, -10 * t);
        }
    }

    public class ExpoInOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            if (t == 0) return 0;
            if (t == 1) return 1;
            return t < 0.5f
                ? (float)Math.Pow(2, 20 * t - 10) / 2
                : (2 - (float)Math.Pow(2, -20 * t + 10)) / 2;
        }
    }

    public class ElasticInEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            if (t == 0) return 0;
            if (t == 1) return 1;
            return -(float)Math.Pow(2, 10 * (t - 1)) * (float)Math.Sin((t - 1.075f) * (2 * Math.PI) / 0.3f);
        }
    }

    public class ElasticOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            if (t == 0) return 0;
            if (t == 1) return 1;
            return (float)Math.Pow(2, -10 * t) * (float)Math.Sin((t - 0.075f) * (2 * Math.PI) / 0.3f) + 1;
        }
    }

    public class ElasticInOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            if (t == 0) return 0;
            if (t == 1) return 1;
            return t < 0.5f
                ? -0.5f * (float)Math.Pow(2, 20 * t - 10) * (float)Math.Sin((20 * t - 11.125f) * (2 * Math.PI) / 0.45f)
                : (float)Math.Pow(2, -20 * t + 10) * (float)Math.Sin((20 * t - 11.125f) * (2 * Math.PI) / 0.45f) * 0.5f + 1;
        }
    }

    public class BackInEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            const float s = 1.70158f;
            return t * t * ((s + 1) * t - s);
        }
    }

    public class BackOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            const float s = 1.70158f;
            return (--t) * t * ((s + 1) * t + s) + 1;
        }
    }

    public class BackInOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            const float s = 1.70158f * 1.525f;
            if (t < 0.5f)
                return (t * 2) * (t * 2) * ((s + 1) * (t * 2) - s) / 2;
            return ((t * 2 - 2) * (t * 2 - 2) * ((s + 1) * (t * 2 - 2) + s) + 2) / 2;
        }
    }

    public class BounceInEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return 1 - BounceOut(1 - t);
        }

        private static float BounceOut(float t)
        {
            if (t < 1 / 2.75f)
                return 7.5625f * t * t;
            if (t < 2 / 2.75f)
                return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
            if (t < 2.5 / 2.75f)
                return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
            return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
        }
    }

    public class BounceOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            if (t < 1 / 2.75f)
                return 7.5625f * t * t;
            if (t < 2 / 2.75f)
                return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
            if (t < 2.5 / 2.75f)
                return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
            return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
        }
    }

    public class BounceInOutEasingFunction : IEasingFunction
    {
        public float Evaluate(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            if (t < 0.5f)
                return BounceIn(t * 2) * 0.5f;
            return BounceOut(t * 2 - 1) * 0.5f + 0.5f;
        }

        private static float BounceIn(float t)
        {
            return 1 - BounceOut(1 - t);
        }

        private static float BounceOut(float t)
        {
            if (t < 1 / 2.75f)
                return 7.5625f * t * t;
            if (t < 2 / 2.75f)
                return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
            if (t < 2.5 / 2.75f)
                return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
            return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
        }
    }

}