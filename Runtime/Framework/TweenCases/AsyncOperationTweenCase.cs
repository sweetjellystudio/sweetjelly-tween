using System;
using UnityEngine;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework
{
    public sealed class AsyncOperationTweenCase : TweenCase
    {
        private readonly AsyncOperation asyncOperation;

        public float Progress => asyncOperation != null ? asyncOperation.progress : 0f;
        public bool IsOperationDone => asyncOperation != null && asyncOperation.isDone;

        public AsyncOperationTweenCase(AsyncOperation asyncOperation)
        {
            this.asyncOperation = asyncOperation ?? throw new ArgumentNullException(nameof(asyncOperation));

            SetDuration(float.MaxValue);
        }

        public override bool Validate()
        {
            return asyncOperation != null;
        }

        public override void Invoke(float deltaTime)
        {
            if (asyncOperation.isDone)
                Complete();
        }

        public override void DefaultComplete()
        {
        }
    }
}