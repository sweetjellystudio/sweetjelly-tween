using System;
using UnityEngine;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework
{
    public static class AsyncOperationTweenExtensions
    {
        public static TweenCase OnCompleted(
            this AsyncOperation asyncOperation,
            Action onCompleted)
        {
            if (asyncOperation == null)
                throw new ArgumentNullException(nameof(asyncOperation));

            return new AsyncOperationTweenCase(asyncOperation)
                .SetUnscaledMode(true)
                .SetUpdateMethod(UpdateMethod.Update)
                .AddOnCompleted(onCompleted);
        }

        public static TweenCase WaitForCompletion(
            this AsyncOperation asyncOperation)
        {
            if (asyncOperation == null)
                throw new ArgumentNullException(nameof(asyncOperation));

            return new AsyncOperationTweenCase(asyncOperation)
                .SetUnscaledMode(true)
                .SetUpdateMethod(UpdateMethod.Update);
        }
    }
}