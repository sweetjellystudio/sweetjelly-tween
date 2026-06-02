using System;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.Domain
{
    public static class EasingFunctionFactory
    {
        public static IEasingFunction Create(EaseType easeType)
        {
            return easeType switch
            {
                EaseType.Linear => new LinearEasingFunction(),

                EaseType.QuadIn => new QuadInEasingFunction(),
                EaseType.QuadOut => new QuadOutEasingFunction(),
                EaseType.QuadInOut => new QuadInOutEasingFunction(),

                EaseType.CubicIn => new CubicInEasingFunction(),
                EaseType.CubicOut => new CubicOutEasingFunction(),
                EaseType.CubicInOut => new CubicInOutEasingFunction(),

                EaseType.QuartIn => new QuartInEasingFunction(),
                EaseType.QuartOut => new QuartOutEasingFunction(),
                EaseType.QuartInOut => new QuartInOutEasingFunction(),

                EaseType.QuintIn => new QuintInEasingFunction(),
                EaseType.QuintOut => new QuintOutEasingFunction(),
                EaseType.QuintInOut => new QuintInOutEasingFunction(),

                EaseType.SineIn => new SineInEasingFunction(),
                EaseType.SineOut => new SineOutEasingFunction(),
                EaseType.SineInOut => new SineInOutEasingFunction(),

                EaseType.CircIn => new CircInEasingFunction(),
                EaseType.CircOut => new CircOutEasingFunction(),
                EaseType.CircInOut => new CircInOutEasingFunction(),

                EaseType.ExpoIn => new ExpoInEasingFunction(),
                EaseType.ExpoOut => new ExpoOutEasingFunction(),
                EaseType.ExpoInOut => new ExpoInOutEasingFunction(),

                EaseType.ElasticIn => new ElasticInEasingFunction(),
                EaseType.ElasticOut => new ElasticOutEasingFunction(),
                EaseType.ElasticInOut => new ElasticInOutEasingFunction(),

                EaseType.BackIn => new BackInEasingFunction(),
                EaseType.BackOut => new BackOutEasingFunction(),
                EaseType.BackInOut => new BackInOutEasingFunction(),

                EaseType.BounceIn => new BounceInEasingFunction(),
                EaseType.BounceOut => new BounceOutEasingFunction(),
                EaseType.BounceInOut => new BounceInOutEasingFunction(),

                _ => throw new ArgumentOutOfRangeException(nameof(easeType), easeType, null)
            };
        }
    }
}