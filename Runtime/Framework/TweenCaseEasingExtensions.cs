using System;
using UnityEngine;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework
{
    public static class TweenCaseEasingExtensions
    {
        public static T SetCurveEasing<T>(this T tweenCase, AnimationCurve easingCurve)
            where T : TweenCase
        {
            if (tweenCase == null)
                throw new ArgumentNullException(nameof(tweenCase));

            tweenCase.SetCustomEasing(new AnimationCurveEasingFunction(easingCurve));
            return tweenCase;
        }
    }
}