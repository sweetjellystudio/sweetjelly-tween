using System;
using UnityEngine;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Domain;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework.Unity
{
    public abstract class TweenPlayerBehaviour<C, T> : MonoBehaviour
        where C : Component
    {
        [Header("Timing")]
        [SerializeField] protected float duration = 0.3f;
        [SerializeField] protected float delay = 0f;
        [SerializeField] protected bool unscaledTime = false;
        [SerializeField] protected UpdateMethod updateMethod = UpdateMethod.Update;

        [Header("Easing")]
        [SerializeField] protected EaseType easing = EaseType.Linear;
        [SerializeField] protected bool hasCustomCurve = false;
        [SerializeField] protected AnimationCurve customCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Loop")]
        [SerializeField] protected int loopAmount = 1;
        [SerializeField] protected LoopType loopType = LoopType.Repeat;

        [Header("Target")]
        [SerializeField] protected TweenTargetType targetType = TweenTargetType.To;
        [SerializeField] protected T targetValue;

        private C targetComponent;
        private ITweenService tweenService;

        private bool isInjected;
        private bool pendingPlay;

        protected ITweenHandle activeHandle;
        protected int loopIndex;

        protected T initialValue;
        protected T startValue;
        protected T endValue;

        protected C TargetComponent
        {
            get
            {
                if (targetComponent != null)
                    return targetComponent;

                targetComponent = GetComponent<C>();
                return targetComponent;
            }
        }

        protected abstract T CurrentValue { get; set; }

        public void Inject(ITweenService tweenService)
        {
            if (tweenService == null)
                throw new ArgumentNullException(nameof(tweenService));

            this.tweenService = tweenService;
            isInjected = true;

            if (pendingPlay && isActiveAndEnabled)
            {
                pendingPlay = false;
                PlayFromEnableState();
            }
        }

        protected virtual void Awake()
        {
            if (TargetComponent == null)
            {
                Debug.LogError(
                    $"[{nameof(TweenPlayerBehaviour<C, T>)}]: Target component {typeof(C).Name} is missing.",
                    this);
                enabled = false;
                return;
            }

            initialValue = CurrentValue;
        }

        protected virtual void OnEnable()
        {
            if (TargetComponent == null)
                return;

            if (!isInjected)
            {
                pendingPlay = true;
                return;
            }

            PlayFromEnableState();
        }

        protected virtual void OnDisable()
        {
            pendingPlay = false;

            if (activeHandle != null)
            {
                activeHandle.Kill();
                activeHandle = null;
            }
        }

        private void PlayFromEnableState()
        {
            PrepareValues();

            loopIndex = 0;

            PlayLoop(delay);
        }

        protected virtual void PrepareValues()
        {
            if (targetType == TweenTargetType.From)
            {
                startValue = targetValue;
                endValue = initialValue;
            }
            else
            {
                startValue = initialValue;
                endValue = targetValue;
            }

            CurrentValue = startValue;
        }

        protected virtual void PlayLoop(float loopDelay)
        {
            if (tweenService == null)
            {
                Debug.LogError(
                    $"[{nameof(TweenPlayerBehaviour<C, T>)}]: Tween service is not injected.",
                    this);
                return;
            }

            loopIndex++;

            TweenCase tweenCase = CreateTweenCase(endValue)
                .SetDelay(loopDelay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);

            ApplyEasing(tweenCase);

            ITweenHandle handle = tweenService.Play(tweenCase);
            activeHandle = handle;

            if (ShouldContinueLoop())
                handle.Completed += OnLoopCompleted;
        }

        protected virtual bool ShouldContinueLoop()
        {
            if (loopAmount <= 0)
                return true; // 0 이하를 infinite loop로 사용할 경우

            return loopIndex < loopAmount;
        }

        protected virtual void OnLoopCompleted()
        {
            switch (loopType)
            {
                case LoopType.Repeat:
                    CurrentValue = startValue;
                    break;

                case LoopType.Yoyo:
                    (startValue, endValue) = (endValue, startValue);
                    CurrentValue = startValue;
                    break;

                case LoopType.Increment:
                    IncrementLoopChangeValues();
                    CurrentValue = startValue;
                    break;
            }

            PlayLoop(0f);
        }

        protected virtual void ApplyEasing(TweenCase tweenCase)
        {
            if (hasCustomCurve && customCurve != null)
                tweenCase.SetCurveEasing(customCurve);
            else
                tweenCase.SetEasing(easing);
        }

        protected abstract TweenCase CreateTweenCase(T endValue);

        protected abstract void IncrementLoopChangeValues();

        public enum LoopType
        {
            Repeat = 0,
            Yoyo = 1,
            Increment = 2,
        }

        public enum TweenTargetType
        {
            From = 0,
            To = 1
        }
    }
}