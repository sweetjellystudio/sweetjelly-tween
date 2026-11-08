using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using SWeetJelly.Tween.App;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Domain;

namespace SWeetJelly.Tween.Framework.Unity
{
    public abstract class TweenCase : ITweenPlayable, INotifyTweenCompletion
    {
        public bool IsActive { get; private set; }

        protected UpdateMethod updateMethod = UpdateMethod.Update;
        public UpdateMethod UpdateMethod => updateMethod;

        protected float state;
        public float State => state;

        protected float duration = 0.0001f;
        public float Duration => duration;

        protected float delay;
        public float Delay => delay;

        protected float currentDelay;
        public float CurrentDelay => currentDelay;
        internal float ObservedElapsed { get; private set; }

        protected bool isPaused;
        public bool IsPaused => isPaused;

        protected bool isUnscaled;
        public bool IsUnscaled => isUnscaled;

        protected bool isCompleted;
        public bool IsCompleted => isCompleted;

        protected bool isKilling;
        public bool IsKilling => isKilling;

        protected bool isKilled;
        public bool IsKilled => isKilled;

        protected IEasingFunction easingFunction;

        private readonly List<TimeReachedCallback> timeReachedCallbacks = new();

        private bool completionApplied;
        private bool completedEventInvoked;

        public event Action Completed;
        public event Action Killed;

        protected TweenCase()
        {
            SetEasing(EaseType.Linear);
        }

        public abstract bool Validate();

        public TweenCase SetDelay(float delay)
        {
            this.delay = TweenTiming.Delay(delay);
            currentDelay = 0f;
            return this;
        }

        public TweenCase SetDuration(float duration)
        {
            this.duration = TweenTiming.Duration(duration);
            return this;
        }

        public TweenCase SetUpdateMethod(UpdateMethod updateMethod)
        {
            if (IsActive)
                throw new InvalidOperationException("Cannot change UpdateMethod while tween is active.");

            this.updateMethod = updateMethod;
            return this;
        }

        public TweenCase SetUnscaledMode(bool isUnscaled)
        {
            this.isUnscaled = isUnscaled;
            return this;
        }

        public TweenCase SetEasing(EaseType easeType)
        {
            easingFunction = EasingFunctions.Get(easeType);
            return this;
        }

        public TweenCase SetCustomEasing(IEasingFunction easingFunction)
        {
            this.easingFunction = easingFunction ?? throw new ArgumentNullException(nameof(easingFunction));
            return this;
        }

        public float EvaluateEase(float t)
        {
            return easingFunction.Evaluate(t);
        }

        public void Activate(UpdateMethod updateMethod)
        {
            this.updateMethod = updateMethod;

            IsActive = true;

            state = 0f;
            currentDelay = 0f;
            ObservedElapsed = 0f;

            isPaused = false;
            isCompleted = false;
            isKilling = false;
            isKilled = false;

            completionApplied = false;
            completedEventInvoked = false;

            for (int i = 0; i < timeReachedCallbacks.Count; i++)
                timeReachedCallbacks[i].Reset();
            ResetPlaybackState();
        }

        protected virtual void ResetPlaybackState() { }

        public void Deactivate()
        {
            IsActive = false;
            isKilling = false;
            isPaused = false;
        }

        public TweenCase Kill()
        {
            if (isKilling || isKilled || completedEventInvoked)
                return this;

            IsActive = false;
            isKilling = true;
            isKilled = true;
            isCompleted = false;
            isPaused = false;

            NotifyTerminal(Killed);

            return this;
        }

        void ITweenPlayable.Kill()
        {
            Kill();
        }

        public TweenCase Complete()
        {
            if (isCompleted || isKilled)
                return this;

            isPaused = false;
            state = 1f;
            isCompleted = true;

            InvokeTimeReachedCallbacks();

            return this;
        }

        public TweenCase CompleteImmediately()
        {
            if (isKilled || completedEventInvoked) return this;
            Complete();
            if (isKilled) return this;
            try { ApplyCompletion(); }
            finally
            {
                if (completedEventInvoked)
                {
                    IsActive = false;
                    isKilling = true;
                    isPaused = false;
                }
            }

            // 완료된 tween도 playback lane에서 제거되어야 하므로 removal flag는 세움.
            // 단, 정상 완료는 Kill 이벤트를 발생시키지 않음.
            IsActive = false;
            isKilling = true;
            isPaused = false;

            return this;
        }

        void ITweenPlayable.CompleteImmediately()
        {
            CompleteImmediately();
        }

        protected void ApplyCompletion()
        {
            if (completionApplied)
                return;

            completionApplied = true;

            DefaultComplete();
            InvokeCompleteEvent();
        }

        public TweenCase Pause()
        {
            if (isCompleted || isKilling)
                return this;

            isPaused = true;
            return this;
        }

        void ITweenPlayable.Pause()
        {
            Pause();
        }

        public TweenCase Resume()
        {
            if (isCompleted || isKilling)
                return this;

            isPaused = false;
            return this;
        }

        void ITweenPlayable.Resume()
        {
            Resume();
        }

        public TweenCase Reset()
        {
            if (IsActive) Kill();
            IsActive = false;

            state = 0f;
            currentDelay = 0f;

            isPaused = false;
            isCompleted = false;
            isKilling = false;
            isKilled = false;

            completionApplied = false;
            completedEventInvoked = false;

            for (int i = 0; i < timeReachedCallbacks.Count; i++)
                timeReachedCallbacks[i].Reset();
            ResetPlaybackState();

            return this;
        }

        public void UpdateState(float deltaTime)
        {
            state = TweenTiming.Advance(state, duration, deltaTime);
            ObservedElapsed += TweenTiming.Delta(deltaTime);
            if (state >= 1f) isCompleted = true;
            InvokeTimeReachedCallbacks();
        }

        public void UpdateDelay(float deltaTime)
        {
            currentDelay = TweenTiming.AdvanceDelay(currentDelay, delay, deltaTime);
            ObservedElapsed += TweenTiming.Delta(deltaTime);
        }

        public TweenCase AddOnCompleted(Action onCompleted)
        {
            if (onCompleted == null)
                return this;

            if (completedEventInvoked)
                onCompleted.Invoke();
            else
                Completed += onCompleted;

            return this;
        }

        public TweenCase AddOnTimeReached(float t, Action onTimeReached)
        {
            if (onTimeReached == null)
                return this;

            t = EasingMath.Clamp01(t);
            timeReachedCallbacks.Add(new TimeReachedCallback(t, onTimeReached));

            return this;
        }

        public void InvokeCompleteEvent()
        {
            if (completedEventInvoked || isKilled)
                return;

            completedEventInvoked = true;
            NotifyTerminal(Completed);
        }

        private static void NotifyTerminal(Action callbacks)
        {
            if (callbacks == null) return;
            ExceptionDispatchInfo failure = null;
            foreach (Action callback in callbacks.GetInvocationList())
            {
                try { callback(); }
                catch (Exception error) { if (failure == null) failure = ExceptionDispatchInfo.Capture(error); }
            }
            failure?.Throw();
        }

        public abstract void Invoke(float deltaTime);

        public abstract void DefaultComplete();

        private void InvokeTimeReachedCallbacks()
        {
            if (timeReachedCallbacks.Count == 0)
                return;

            int count = timeReachedCallbacks.Count;
            for (int i = 0; i < count && !isKilled; i++)
                timeReachedCallbacks[i].TryInvoke(state);
        }

        private sealed class TimeReachedCallback
        {
            private readonly float time;
            private readonly Action callback;

            private bool invoked;

            public TimeReachedCallback(float time, Action callback)
            {
                this.time = time;
                this.callback = callback;
            }

            public void TryInvoke(float state)
            {
                if (invoked)
                    return;

                if (state < time)
                    return;

                invoked = true;
                callback?.Invoke();
            }

            public void Reset()
            {
                invoked = false;
            }
        }
    }
}
