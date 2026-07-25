using System.Collections.Generic;
using SWeetJelly.Tween.Contract;
using UnityEngine;
using UnityEngine.Events;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TweenPlayerGroupBehaviour : TweenPlayerBehaviour
    {
        [SerializeField] private bool playOnEnable;
        [SerializeField] private TweenPlayerBehaviour[] players;

        [Header("Events")]
        [SerializeField] private UnityEvent played;
        [SerializeField] private UnityEvent completed;
        [SerializeField] private UnityEvent killed;

        private readonly List<TweenPlayerBehaviour> trackedPlayers = new();
        private int completedCount;
        private int killedCount;
        private bool completedEventInvoked;
        private bool killedEventInvoked;
        private bool suppressKilledEvent;

        public override bool IsPlaying
        {
            get
            {
                if (players == null)
                    return false;

                for (int i = 0; i < players.Length; i++)
                {
                    TweenPlayerBehaviour player = players[i];

                    if (player != null && player.IsPlaying)
                        return true;
                }

                return false;
            }
        }

        private void OnEnable()
        {
            if (playOnEnable)
                Play();
        }

        private void OnDisable()
        {
            Kill();
            ClearTracking();
        }

        public override void Play()
        {
            PlayEach(player => player.Play());
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
            PlayEach(player => player.PlayForward());
        }

        public override void PlayBackward()
        {
            PlayEach(player => player.PlayBackward());
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
            ClearTracking();

            if (players != null)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    TweenPlayerBehaviour player = players[i];

                    if (player != null)
                        player.SnapForward();
                }
            }

            InvokePlayed();
            InvokeCompleted();
        }

        public override void SnapBackward()
        {
            ClearTracking();

            if (players != null)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    TweenPlayerBehaviour player = players[i];

                    if (player != null)
                        player.SnapBackward();
                }
            }

            InvokePlayed();
            InvokeCompleted();
        }

        public override void Kill()
        {
            bool hasPlayingPlayer = IsPlaying;
            bool isTrackingPlayers = trackedPlayers.Count > 0;

            if (players != null)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    TweenPlayerBehaviour player = players[i];

                    if (player != null)
                        player.Kill();
                }
            }

            if (!suppressKilledEvent && !isTrackingPlayers && hasPlayingPlayer)
                InvokeKilled();
        }

        public override void SetPlayOnEnable(bool playOnEnable)
        {
            this.playOnEnable = playOnEnable;
        }

        public override void Initialise(ITweenService tweenService)
        {
            if (players == null)
                return;

            for (int i = 0; i < players.Length; i++)
            {
                TweenPlayerBehaviour player = players[i];

                if (player != null && player != this)
                    player.Initialise(tweenService);
            }
        }

        private void PlayEach(PlayerAction action)
        {
            if (!isActiveAndEnabled)
                return;

            ClearTracking();
            BeginTracking();

            InvokePlayed();

            if (trackedPlayers.Count == 0)
            {
                InvokeCompleted();
                return;
            }

            suppressKilledEvent = true;

            for (int i = 0; i < trackedPlayers.Count; i++)
                action(trackedPlayers[i]);

            suppressKilledEvent = false;

            TryInvokeCompleted();
            TryInvokeKilled();
        }

        private void BeginTracking()
        {
            completedCount = 0;
            killedCount = 0;
            completedEventInvoked = false;
            killedEventInvoked = false;

            if (players == null)
                return;

            for (int i = 0; i < players.Length; i++)
            {
                TweenPlayerBehaviour player = players[i];

                if (player == null || player == this || !player.isActiveAndEnabled)
                    continue;

                trackedPlayers.Add(player);
                player.Completed += OnPlayerCompleted;
                player.Killed += OnPlayerKilled;
            }
        }

        private void ClearTracking()
        {
            for (int i = 0; i < trackedPlayers.Count; i++)
            {
                TweenPlayerBehaviour player = trackedPlayers[i];

                if (player == null)
                    continue;

                player.Completed -= OnPlayerCompleted;
                player.Killed -= OnPlayerKilled;
            }

            trackedPlayers.Clear();
            completedCount = 0;
            killedCount = 0;
            completedEventInvoked = false;
            killedEventInvoked = false;
            suppressKilledEvent = false;
        }

        private void OnPlayerCompleted()
        {
            completedCount++;
            TryInvokeCompleted();
        }

        private void OnPlayerKilled()
        {
            killedCount++;

            if (!suppressKilledEvent)
                TryInvokeKilled();
        }

        private void TryInvokeCompleted()
        {
            if (completedEventInvoked)
                return;

            if (trackedPlayers.Count == 0)
                return;

            if (completedCount < trackedPlayers.Count)
                return;

            InvokeCompleted();
            ClearTracking();
        }

        private void TryInvokeKilled()
        {
            if (killedEventInvoked)
                return;

            if (trackedPlayers.Count == 0)
                return;

            if (killedCount < trackedPlayers.Count)
                return;

            InvokeKilled();
            ClearTracking();
        }

        private void InvokePlayed()
        {
            NotifyPlayed();
            played?.Invoke();
        }

        private void InvokeCompleted()
        {
            completedEventInvoked = true;
            NotifyCompleted();
            completed?.Invoke();
        }

        private void InvokeKilled()
        {
            killedEventInvoked = true;
            NotifyKilled();
            killed?.Invoke();
        }

        private delegate void PlayerAction(TweenPlayerBehaviour player);
    }
}
