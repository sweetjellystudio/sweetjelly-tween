namespace SWeetJelly.Tween.Domain
{
    using System;
    using SWeetJelly.Tween.Contract;

    public static class EasingFunctions
    {
        public static readonly IEasingFunction Linear = new LinearEasingFunction();
        public static readonly IEasingFunction QuadIn = new QuadInEasingFunction();
        public static readonly IEasingFunction QuadOut = new QuadOutEasingFunction();
        public static readonly IEasingFunction QuadInOut = new QuadInOutEasingFunction();

        public static IEasingFunction Get(EaseType easeType)
        {
            return easeType switch
            {
                EaseType.Linear => Linear,
                EaseType.QuadIn => QuadIn,
                EaseType.QuadOut => QuadOut,
                EaseType.QuadInOut => QuadInOut,
                _ => EasingFunctionFactory.Create(easeType)
            };
        }
    }
}