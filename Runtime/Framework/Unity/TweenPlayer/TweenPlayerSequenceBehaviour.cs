using System;
using System.Collections.Generic;
using SWeetJelly.Tween.Contract;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TweenPlayerSequenceBehaviour : TweenPlayerBehaviour
    {
        [SerializeField] private bool playOnEnable;
        [FormerlySerializedAs("reverseWhenPlayingToInitial")]
        [SerializeField] private bool reverseWhenPlayingBackward = true;
        [SerializeField] private TweenPlayerBehaviour[] players;

        [Header("Events")]
        [SerializeField] private UnityEvent played;
        [SerializeField] private UnityEvent completed;
        [SerializeField] private UnityEvent killed;

        private readonly List<TweenPlayerBehaviour> sequence = new();

        private TweenPlayerBehaviour currentPlayer;
        private Action currentCompleted;
        private Action currentKilled;
        private int generation;
        private PlaybackMode playbackMode;
        private int currentIndex;
        private int direction;
        private bool isSequencePlaying;
        private bool killedEventInvoked;
        private bool initialising;
        private bool starting;

        public override bool IsPlaying => isSequencePlaying;

        private void OnEnable()
        {
            if (playOnEnable)
                Play();
        }

        private void OnDisable()
        {
            Kill();
            ClearCurrentPlayer();
            isSequencePlaying = false;
        }

        public override void Play()
        {
            StartSequence(PlaybackMode.Default, false);
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
            StartSequence(PlaybackMode.Forward, false);
        }

        public override void PlayBackward()
        {
            StartSequence(PlaybackMode.Backward, reverseWhenPlayingBackward);
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
            StopCurrentSilently();

            if (players != null)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    TweenPlayerBehaviour player = players[i];

                    if (player != null && player != this)
                        player.SnapForward();
                }
            }

            InvokePlayed();
            InvokeCompleted();
        }

        public override void SnapBackward()
        {
            StopCurrentSilently();

            if (players != null)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    TweenPlayerBehaviour player = players[i];

                    if (player != null && player != this)
                        player.SnapBackward();
                }
            }

            InvokePlayed();
            InvokeCompleted();
        }

        public override void Kill()
        {
            if (!isSequencePlaying)
                return;

            TweenPlayerBehaviour player = currentPlayer;

            ClearCurrentPlayer();
            isSequencePlaying = false;
            generation++;
            System.Runtime.ExceptionServices.ExceptionDispatchInfo failure = null;
            try { if (player != null) player.Kill(); }
            catch (Exception error) { failure = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error); }
            try { InvokeKilled(); }
            catch (Exception error) { if (failure == null) failure = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error); }
            failure?.Throw();
        }

        public override void SetPlayOnEnable(bool playOnEnable)
        {
            this.playOnEnable = playOnEnable;
        }

        public override void Initialise(ITweenService tweenService)
        {
            if (initialising) throw new InvalidOperationException("TweenPlayer composition contains a cycle.");
            if (players == null)
                return;
            initialising = true;
            try
            {
                for (int i = 0; i < players.Length; i++)
                {
                    TweenPlayerBehaviour player = players[i];
                    if (player != null && player != this) player.Initialise(tweenService);
                }
            }
            finally { initialising = false; }
        }

        private void StartSequence(PlaybackMode mode, bool reverse)
        {
            if (!isActiveAndEnabled)
                return;
            if (starting && isSequencePlaying) throw new InvalidOperationException("TweenPlayer composition contains a playback cycle.");
            int expected = generation + 1;
            StopCurrentSilently();
            if (generation != expected) return;
            BuildSequence();

            playbackMode = mode;
            direction = reverse ? -1 : 1;
            currentIndex = reverse ? sequence.Count - 1 : 0;
            isSequencePlaying = true;
            killedEventInvoked = false;
            int run = ++generation;

            starting = true;
            try
            {
                InvokePlayed();
                if (run != generation || !isSequencePlaying) return;
                if (sequence.Count == 0)
                {
                    InvokeCompleted();
                    return;
                }
                PlayCurrent();
            }
            catch
            {
                if (run == generation && isSequencePlaying) { try { Kill(); } catch { } }
                throw;
            }
            finally { starting = false; }
        }

        private void BuildSequence()
        {
            sequence.Clear();

            if (players == null)
                return;

            for (int i = 0; i < players.Length; i++)
            {
                TweenPlayerBehaviour player = players[i];

                if (player == null || player == this || !player.isActiveAndEnabled)
                    continue;

                sequence.Add(player);
            }
        }

        private void PlayCurrent()
        {
            if (!isSequencePlaying)
                return;

            if (currentIndex < 0 || currentIndex >= sequence.Count)
            {
                InvokeCompleted();
                return;
            }

            currentPlayer = sequence[currentIndex];
            var player = currentPlayer;
            int run = generation;
            currentCompleted = () =>
            {
                if (run == generation && ReferenceEquals(currentPlayer, player)) OnCurrentPlayerCompleted();
            };
            currentKilled = () =>
            {
                if (run == generation && ReferenceEquals(currentPlayer, player)) OnCurrentPlayerKilled();
            };
            currentPlayer.Completed += currentCompleted;
            currentPlayer.Killed += currentKilled;

            try
            {
                switch (playbackMode)
                {
                    case PlaybackMode.Default: player.Play(); break;
                    case PlaybackMode.Forward: player.PlayForward(); break;
                    case PlaybackMode.Backward: player.PlayBackward(); break;
                }
            }
            catch
            {
                if (run == generation && isSequencePlaying) { try { Kill(); } catch { } }
                throw;
            }
        }

        private void OnCurrentPlayerCompleted()
        {
            ClearCurrentPlayer();

            currentIndex += direction;
            PlayCurrent();
        }

        private void OnCurrentPlayerKilled()
        {
            ClearCurrentPlayer();
            isSequencePlaying = false;
            InvokeKilled();
        }

        private void StopCurrentSilently()
        {
            generation++;
            TweenPlayerBehaviour player = currentPlayer;

            ClearCurrentPlayer();
            isSequencePlaying = false;

            if (player != null)
                player.Kill();
        }

        private void ClearCurrentPlayer()
        {
            if (currentPlayer != null)
            {
                currentPlayer.Completed -= currentCompleted;
                currentPlayer.Killed -= currentKilled;
                currentPlayer = null;
            }
            currentCompleted = null;
            currentKilled = null;
        }

        private void InvokePlayed()
        {
            NotifyPlayed();
            played?.Invoke();
        }

        private void InvokeCompleted()
        {
            ClearCurrentPlayer();
            isSequencePlaying = false;
            NotifyCompleted();
            completed?.Invoke();
        }

        private void InvokeKilled()
        {
            if (killedEventInvoked)
                return;

            killedEventInvoked = true;
            NotifyKilled();
            killed?.Invoke();
        }

        private enum PlaybackMode
        {
            Default = 0,
            Forward = 1,
            Backward = 2,
        }
    }
}
