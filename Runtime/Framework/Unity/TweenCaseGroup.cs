using System;
using System.Collections.Generic;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TweenCaseGroup
    {
        private readonly List<TweenCase> cases = new();

        private int completedCount;
        private bool completedEventInvoked;
        private bool isKilled;

        public IReadOnlyList<TweenCase> Cases => cases;
        public int Count => cases.Count;

        public bool IsCompleted
        {
            get
            {
                if (cases.Count == 0)
                    return false;

                return completedCount >= cases.Count;
            }
        }

        public bool IsKilled => isKilled;

        public event Action Completed;

        public TweenCaseGroup Add(TweenCase tweenCase)
        {
            if (tweenCase == null)
                throw new ArgumentNullException(nameof(tweenCase));

            if (completedEventInvoked)
                throw new InvalidOperationException("Cannot add a tween to a completed TweenCaseGroup.");

            cases.Add(tweenCase);

            if (tweenCase.IsCompleted)
            {
                completedCount++;
                TryInvokeCompleted();
            }
            else
            {
                tweenCase.AddOnCompleted(OnTweenCaseCompleted);
            }

            return this;
        }

        public TweenCaseGroup AddOnCompleted(Action onCompleted)
        {
            if (onCompleted == null)
                return this;

            if (completedEventInvoked)
                onCompleted.Invoke();
            else
                Completed += onCompleted;

            return this;
        }

        public void Complete()
        {
            if (completedEventInvoked)
                return;

            for (int i = 0; i < cases.Count; i++)
            {
                TweenCase tweenCase = cases[i];

                if (tweenCase == null)
                    continue;

                if (!tweenCase.IsCompleted)
                    tweenCase.CompleteImmediately();
            }

            completedCount = cases.Count;
            TryInvokeCompleted();
        }

        public void Kill()
        {
            if (isKilled)
                return;

            isKilled = true;

            for (int i = 0; i < cases.Count; i++)
            {
                TweenCase tweenCase = cases[i];

                if (tweenCase == null)
                    continue;

                if (tweenCase.IsActive)
                    tweenCase.Kill();
            }
        }

        private void OnTweenCaseCompleted()
        {
            if (completedEventInvoked)
                return;

            completedCount++;
            TryInvokeCompleted();
        }

        private void TryInvokeCompleted()
        {
            if (completedEventInvoked)
                return;

            if (cases.Count == 0)
                return;

            if (completedCount < cases.Count)
                return;

            completedEventInvoked = true;
            Completed?.Invoke();
        }

        public static TweenCaseGroup operator +(TweenCaseGroup group, TweenCase tweenCase)
        {
            if (group == null)
                throw new ArgumentNullException(nameof(group));

            return group.Add(tweenCase);
        }
    }
}