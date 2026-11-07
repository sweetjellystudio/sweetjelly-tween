using UnityEngine;
using UnityEngine.UIElements;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.Framework.Unity
{
    public static class VisualElementTweenExtensions
    {
        public static TweenCase DOFade(this VisualElement element, float resultValue, float duration,
            float delay = 0f, bool unscaledTime = false, UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return new VisualElementFadeTweenCase(element, resultValue)
                .SetDuration(duration).SetDelay(delay).SetUnscaledMode(unscaledTime).SetUpdateMethod(updateMethod);
        }

        public static TweenCase DOScale(this VisualElement element, Vector3 resultValue, float duration,
            float delay = 0f, bool unscaledTime = false, UpdateMethod updateMethod = UpdateMethod.Update)
        {
            return new VisualElementScaleTweenCase(element, resultValue)
                .SetDuration(duration).SetDelay(delay).SetUnscaledMode(unscaledTime).SetUpdateMethod(updateMethod);
        }
    }
}
