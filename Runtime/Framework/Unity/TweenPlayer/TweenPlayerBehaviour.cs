using System;
using UnityEngine;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Domain;
using SWeetJelly.Tween.App;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace SWeetJelly.Tween.Framework.Unity
{
    public abstract class TweenPlayerBehaviour : MonoBehaviour
    {
        public event Action Played;
        public event Action Completed;
        public event Action Killed;

        public abstract bool IsPlaying { get; }

        public abstract void Play();
        public abstract void Play(bool forward);
        public abstract void PlayForward();
        public abstract void PlayBackward();
        public abstract void Snap(bool forward);
        public abstract void SnapForward();
        public abstract void SnapBackward();
        public abstract void Kill();
        public abstract void SetPlayOnEnable(bool playOnEnable);

        public virtual void Inject(ITweenService tweenService)
        {
            Initialise(tweenService);
        }

        public virtual void Initialise(ITweenService tweenService)
        {
        }

        protected void NotifyPlayed()
        {
            Played?.Invoke();
        }

        protected void NotifyCompleted()
        {
            Completed?.Invoke();
        }

        protected void NotifyKilled()
        {
            Killed?.Invoke();
        }
    }

    public abstract class TweenPlayerBehaviour<C, T> : TweenPlayerBehaviour
        where C : Component
    {
        [Header("Timing")]
        [SerializeField] protected float duration = 0.3f;
        [SerializeField] protected float delay = 0f;
        [SerializeField] protected bool unscaledTime = false;
        [SerializeField] protected UpdateMethod updateMethod = UpdateMethod.Update;

        [Header("Playback")]
        [SerializeField] private bool playOnEnable = false;
        [Tooltip("Capture On Forward stores the value when a forward cycle begins and returns to it on backward. " +
                 "Configured Endpoints always plays between the serialized Backward and Forward values.")]
        [SerializeField] private EndpointMode endpointMode = EndpointMode.CaptureOnForward;

        [Header("Easing")]
        [SerializeField] protected EaseType easing = EaseType.Linear;
        [SerializeField] protected bool hasCustomCurve = false;
        [SerializeField] protected AnimationCurve customCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Loop")]
        [SerializeField] protected int loopAmount = 1;
        [SerializeField] protected LoopType loopType = LoopType.Repeat;

        [Header("Target")]
        [SerializeField] private C targetComponent;
        [FormerlySerializedAs("targetType")]
        [SerializeField] protected PlaybackDirection playOnEnableDirection = PlaybackDirection.Forward;
        [SerializeField] protected T backwardValue;
        [FormerlySerializedAs("targetValue")]
        [SerializeField] protected T forwardValue;

        [Header("Events")]
        [SerializeField] private UnityEvent played;
        [SerializeField] private UnityEvent completed;
        [SerializeField] private UnityEvent killed;

        private ITweenService tweenService;

        private bool isInjected;
        private bool pendingPlay;
        private PlaybackDirection pendingPlayDirection;
        private bool missingServiceWarningLogged;
        private bool suppressKilledEvent;
        private bool hasCapturedOrigin;
        private bool releaseCapturedOriginOnComplete;

        protected ITweenHandle activeHandle;
        protected int loopIndex;

        protected T capturedOrigin;
        protected T startValue;
        protected T endValue;

        public override bool IsPlaying => activeHandle != null && activeHandle.IsActive;

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

        public override void Initialise(ITweenService tweenService)
        {
            if (tweenService == null)
                throw new ArgumentNullException(nameof(tweenService));

            this.tweenService = tweenService;
            isInjected = true;

            if (pendingPlay && isActiveAndEnabled)
            {
                PlaybackDirection direction = pendingPlayDirection;
                pendingPlay = false;
                Play(direction == PlaybackDirection.Forward);
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
        }

        protected virtual void OnEnable()
        {
            if (TargetComponent == null)
                return;

            if (!playOnEnable)
                return;

            if (!isInjected)
            {
                QueuePendingPlay(playOnEnableDirection);
                LogMissingServiceWarning();
                return;
            }

            Play(playOnEnableDirection == PlaybackDirection.Forward);
        }

        protected virtual void OnDisable()
        {
            pendingPlay = false;
            Kill();
            ClearCapturedOrigin();
        }

        public override void Play()
        {
            if (!isActiveAndEnabled)
                return;

            if (!isInjected)
            {
                QueuePendingPlay(playOnEnableDirection);
                LogMissingServiceWarning();
                return;
            }

            Play(playOnEnableDirection == PlaybackDirection.Forward);
        }

        public override void Play(bool forward)
        {
            if (forward)
                PlayForward();
            else
                PlayBackward();
        }

        public override void PlayForward()
        {
            if (!CanStartPlayback(PlaybackDirection.Forward))
                return;

            if (endpointMode == EndpointMode.ConfiguredEndpoints)
            {
                PlayDirection(backwardValue, forwardValue, forceConfiguredStart: true, releaseOriginOnComplete: false);
                return;
            }

            if (!hasCapturedOrigin)
            {
                capturedOrigin = CurrentValue;
                hasCapturedOrigin = true;
            }

            PlayDirection(capturedOrigin, forwardValue, forceConfiguredStart: false, releaseOriginOnComplete: false);
        }

        public override void PlayBackward()
        {
            if (!CanStartPlayback(PlaybackDirection.Backward))
                return;

            if (endpointMode == EndpointMode.ConfiguredEndpoints)
            {
                PlayDirection(forwardValue, backwardValue, forceConfiguredStart: true, releaseOriginOnComplete: false);
                return;
            }

            if (!hasCapturedOrigin)
            {
                CompleteWithoutPlayback();
                return;
            }

            PlayDirection(forwardValue, capturedOrigin, forceConfiguredStart: false, releaseOriginOnComplete: true);
        }

        public override void Snap(bool forward)
        {
            if (forward)
                SnapForward();
            else
                SnapBackward();
        }

        public override void SnapForward()
        {
            KillSilently();

            if (endpointMode == EndpointMode.CaptureOnForward && !hasCapturedOrigin)
            {
                capturedOrigin = CurrentValue;
                hasCapturedOrigin = true;
            }

            CurrentValue = forwardValue;
        }

        public override void SnapBackward()
        {
            KillSilently();

            if (endpointMode == EndpointMode.ConfiguredEndpoints)
            {
                CurrentValue = backwardValue;
                return;
            }

            if (!hasCapturedOrigin)
                return;

            CurrentValue = capturedOrigin;
            ClearCapturedOrigin();
        }

        public override void Kill()
        {
            if (activeHandle == null)
                return;

            activeHandle.Kill();
            activeHandle = null;
        }

        public override void SetPlayOnEnable(bool playOnEnable)
        {
            this.playOnEnable = playOnEnable;
        }

        private bool CanStartPlayback(PlaybackDirection direction)
        {
            if (!isActiveAndEnabled)
                return false;

            if (isInjected)
                return true;

            QueuePendingPlay(direction);
            LogMissingServiceWarning();
            return false;
        }

        private void PlayDirection(
            T configuredStart,
            T destination,
            bool forceConfiguredStart,
            bool releaseOriginOnComplete)
        {
            KillSilently();

            startValue = forceConfiguredStart
                ? configuredStart
                : CurrentValue;
            endValue = destination;
            loopIndex = 0;
            releaseCapturedOriginOnComplete = releaseOriginOnComplete;

            if (forceConfiguredStart)
                CurrentValue = startValue;

            PlayLoop(delay);
        }

        protected virtual void PlayLoop(float loopDelay)
        {
            if (tweenService == null)
            {
                LogMissingServiceWarning();
                return;
            }

            KillSilently();

            loopIndex++;

            TweenCase tweenCase = CreateTweenCase(endValue)
                .SetDelay(loopDelay)
                .SetDuration(duration)
                .SetUnscaledMode(unscaledTime)
                .SetUpdateMethod(updateMethod);

            ApplyEasing(tweenCase);

            ITweenHandle handle = tweenService.Play(tweenCase);
            activeHandle = handle;
            NotifyPlayed();
            played?.Invoke();

            if (ShouldContinueLoop())
            {
                handle.Completed += OnLoopCompleted;
            }
            else
            {
                handle.Completed += OnCompleted;
            }

            handle.Killed += OnKilled;
        }

        protected virtual bool ShouldContinueLoop()
        {
            if (loopAmount <= 0)
                return true; // 0 이하를 infinite loop로 사용할 경우

            return loopIndex < loopAmount;
        }

        protected virtual void OnLoopCompleted()
        {
            activeHandle = null;

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

        protected virtual void OnCompleted()
        {
            activeHandle = null;

            if (releaseCapturedOriginOnComplete)
                ClearCapturedOrigin();

            releaseCapturedOriginOnComplete = false;
            NotifyCompleted();
            completed?.Invoke();
        }

        protected virtual void OnKilled()
        {
            activeHandle = null;
            releaseCapturedOriginOnComplete = false;

            if (suppressKilledEvent)
                return;

            NotifyKilled();
            killed?.Invoke();
        }

        private void KillSilently()
        {
            if (activeHandle == null)
                return;

            suppressKilledEvent = true;
            activeHandle.Kill();
            suppressKilledEvent = false;
            activeHandle = null;
        }

        private void CompleteWithoutPlayback()
        {
            KillSilently();
            NotifyPlayed();
            played?.Invoke();
            NotifyCompleted();
            completed?.Invoke();
        }

        private void QueuePendingPlay(PlaybackDirection direction)
        {
            pendingPlay = true;
            pendingPlayDirection = direction;
        }

        private void ClearCapturedOrigin()
        {
            hasCapturedOrigin = false;
            capturedOrigin = default;
        }

        private void LogMissingServiceWarning()
        {
            if (missingServiceWarningLogged)
                return;

            missingServiceWarningLogged = true;

            Debug.LogWarning(
                $"[{GetType().Name}]: Tween service is not injected. Call {nameof(Initialise)} before playing this tween.",
                this);
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

        public enum PlaybackDirection
        {
            Backward = 0,
            Forward = 1
        }

        public enum EndpointMode
        {
            CaptureOnForward = 0,
            ConfiguredEndpoints = 1,
        }
    }
}
