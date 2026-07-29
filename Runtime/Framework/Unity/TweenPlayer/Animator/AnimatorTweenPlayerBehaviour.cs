using UnityEngine;
using UnityEngine.Events;

namespace SWeetJelly.Tween.Framework.Unity
{
    /// <summary>
    /// Bridges Animator trigger-driven playback to the TweenPlayer lifecycle.
    /// The Animator Controller owns transitions and blending; this component only
    /// issues commands and observes the configured state until it finishes.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class AnimatorTweenPlayerBehaviour : TweenPlayerBehaviour
    {
        [Header("Target")]
        [SerializeField] private Animator targetAnimator;
        [SerializeField, Min(0)] private int layer;

        [Header("Forward")]
        [SerializeField] private string forwardTrigger;
        [Tooltip("State entered by the forward trigger. A full path such as 'Base Layer.Show' is recommended.")]
        [SerializeField] private string forwardState;
        [Tooltip("State sampled by SnapForward. When empty, Forward State is used.")]
        [SerializeField] private string forwardSnapState;

        [Header("Backward")]
        [SerializeField] private string backwardTrigger;
        [Tooltip("State entered by the backward trigger. A full path such as 'Base Layer.Hide' is recommended.")]
        [SerializeField] private string backwardState;
        [Tooltip("State sampled by SnapBackward. When empty, Backward State is used.")]
        [SerializeField] private string backwardSnapState;

        [Header("Playback")]
        [SerializeField] private bool playOnEnable;
        [SerializeField] private PlaybackDirection playOnEnableDirection = PlaybackDirection.Forward;
        [Tooltip("Maximum unscaled seconds to wait for the configured state. Set to 0 to disable the timeout.")]
        [SerializeField, Min(0f)] private float stateEntryTimeout = 5f;

        [Header("Events")]
        [SerializeField] private UnityEvent played;
        [SerializeField] private UnityEvent completed;
        [SerializeField] private UnityEvent killed;

        private Animator activeAnimator;
        private string activeState;
        private float stateEntryStartedAt;
        private bool isPlaying;
        private bool enteredActiveState;
        private bool loopWarningLogged;
        private bool suppressKilledEvent;

        public override bool IsPlaying => isPlaying;

        private Animator TargetAnimator
        {
            get
            {
                if (targetAnimator != null)
                    return targetAnimator;

                targetAnimator = GetComponent<Animator>();
                return targetAnimator;
            }
        }

        private void Awake()
        {
            if (TargetAnimator != null)
                return;

            Debug.LogError($"[{nameof(AnimatorTweenPlayerBehaviour)}]: Target Animator is missing.", this);
            enabled = false;
        }

        private void OnEnable()
        {
            if (!playOnEnable || TargetAnimator == null)
                return;

            Play(playOnEnableDirection == PlaybackDirection.Forward);
        }

        private void OnDisable()
        {
            Kill();
        }

        private void Update()
        {
            if (!isPlaying)
                return;

            if (activeAnimator == null || !activeAnimator.isActiveAndEnabled)
            {
                Kill();
                return;
            }

            if (layer < 0 || layer >= activeAnimator.layerCount)
            {
                Debug.LogWarning(
                    $"[{nameof(AnimatorTweenPlayerBehaviour)}]: Layer {layer} is outside the Animator layer range.",
                    this);
                Kill();
                return;
            }

            AnimatorStateInfo state = activeAnimator.GetCurrentAnimatorStateInfo(layer);
            bool isTargetState = state.IsName(activeState);

            if (!enteredActiveState)
            {
                if (isTargetState)
                {
                    enteredActiveState = true;

                    if (state.loop && !loopWarningLogged)
                    {
                        loopWarningLogged = true;
                        Debug.LogWarning(
                            $"[{nameof(AnimatorTweenPlayerBehaviour)}]: State '{activeState}' loops and will not " +
                            "complete until the Animator leaves that state or the player is killed.",
                            this);
                    }
                }
                else if (stateEntryTimeout > 0f &&
                         Time.unscaledTime - stateEntryStartedAt >= stateEntryTimeout)
                {
                    Debug.LogWarning(
                        $"[{nameof(AnimatorTweenPlayerBehaviour)}]: Animator did not enter state '{activeState}' " +
                        $"on layer {layer} within {stateEntryTimeout:0.###} seconds.",
                        this);
                    Kill();
                }

                return;
            }

            if (!isTargetState)
            {
                CompletePlayback();
                return;
            }

            if (!state.loop && !activeAnimator.IsInTransition(layer) && state.normalizedTime >= 1f)
                CompletePlayback();
        }

        public override void Play()
        {
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
            StartPlayback(forwardTrigger, backwardTrigger, forwardState);
        }

        public override void PlayBackward()
        {
            StartPlayback(backwardTrigger, forwardTrigger, backwardState);
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
            SnapToState(string.IsNullOrWhiteSpace(forwardSnapState) ? forwardState : forwardSnapState);
        }

        public override void SnapBackward()
        {
            SnapToState(string.IsNullOrWhiteSpace(backwardSnapState) ? backwardState : backwardSnapState);
        }

        public override void Kill()
        {
            if (!isPlaying)
                return;

            ClearPlayback();

            if (suppressKilledEvent)
                return;

            NotifyKilled();
            killed?.Invoke();
        }

        public override void SetPlayOnEnable(bool value)
        {
            playOnEnable = value;
        }

        private void StartPlayback(string trigger, string oppositeTrigger, string stateName)
        {
            if (!isActiveAndEnabled)
                return;

            Animator animator = TargetAnimator;

            if (!CanPlay(animator, trigger, stateName))
                return;

            string resolvedState = ResolveStateName(animator, stateName);

            if (resolvedState == null)
            {
                Debug.LogWarning(
                    $"[{nameof(AnimatorTweenPlayerBehaviour)}]: State '{stateName}' does not exist on layer {layer}.",
                    this);
                return;
            }

            KillSilently();

            if (!string.IsNullOrWhiteSpace(oppositeTrigger))
                animator.ResetTrigger(oppositeTrigger);

            activeAnimator = animator;
            activeState = resolvedState;
            stateEntryStartedAt = Time.unscaledTime;
            enteredActiveState = false;
            loopWarningLogged = false;
            isPlaying = true;

            animator.SetTrigger(trigger);

            NotifyPlayed();
            played?.Invoke();
        }

        private bool CanPlay(Animator animator, string trigger, string stateName)
        {
            if (animator == null || !animator.isActiveAndEnabled)
                return false;

            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogWarning(
                    $"[{nameof(AnimatorTweenPlayerBehaviour)}]: Runtime Animator Controller is missing.",
                    this);
                return false;
            }

            if (layer < 0 || layer >= animator.layerCount)
            {
                Debug.LogWarning(
                    $"[{nameof(AnimatorTweenPlayerBehaviour)}]: Layer {layer} is outside the Animator layer range.",
                    this);
                return false;
            }

            if (string.IsNullOrWhiteSpace(trigger))
            {
                Debug.LogWarning($"[{nameof(AnimatorTweenPlayerBehaviour)}]: Trigger is not configured.", this);
                return false;
            }

            if (string.IsNullOrWhiteSpace(stateName))
            {
                Debug.LogWarning($"[{nameof(AnimatorTweenPlayerBehaviour)}]: Completion state is not configured.", this);
                return false;
            }

            if (!HasTrigger(animator, trigger))
            {
                Debug.LogWarning(
                    $"[{nameof(AnimatorTweenPlayerBehaviour)}]: Trigger parameter '{trigger}' does not exist.",
                    this);
                return false;
            }

            return true;
        }

        private static bool HasTrigger(Animator animator, string parameterName)
        {
            int parameterHash = Animator.StringToHash(parameterName);
            AnimatorControllerParameter[] parameters = animator.parameters;

            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];

                if (parameter.nameHash == parameterHash && parameter.type == AnimatorControllerParameterType.Trigger)
                    return true;
            }

            return false;
        }

        private void SnapToState(string stateName)
        {
            Animator animator = TargetAnimator;

            if (animator == null || !animator.isActiveAndEnabled || string.IsNullOrWhiteSpace(stateName))
                return;

            if (layer < 0 || layer >= animator.layerCount)
                return;

            string resolvedState = ResolveStateName(animator, stateName);

            if (resolvedState == null)
            {
                Debug.LogWarning(
                    $"[{nameof(AnimatorTweenPlayerBehaviour)}]: Snap state '{stateName}' does not exist on layer {layer}.",
                    this);
                return;
            }

            KillSilently();
            animator.Play(resolvedState, layer, 1f);
            animator.Update(0f);
        }

        private string ResolveStateName(Animator animator, string stateName)
        {
            if (animator.HasState(layer, Animator.StringToHash(stateName)))
                return stateName;

            string fullPath = $"{animator.GetLayerName(layer)}.{stateName}";

            return animator.HasState(layer, Animator.StringToHash(fullPath))
                ? fullPath
                : null;
        }

        private void CompletePlayback()
        {
            ClearPlayback();
            NotifyCompleted();
            completed?.Invoke();
        }

        private void KillSilently()
        {
            if (!isPlaying)
                return;

            suppressKilledEvent = true;
            Kill();
            suppressKilledEvent = false;
        }

        private void ClearPlayback()
        {
            isPlaying = false;
            enteredActiveState = false;
            loopWarningLogged = false;
            activeAnimator = null;
            activeState = null;
        }

        public enum PlaybackDirection
        {
            Backward = 0,
            Forward = 1,
        }
    }
}
