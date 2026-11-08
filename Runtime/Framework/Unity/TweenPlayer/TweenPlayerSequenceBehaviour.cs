using System;
using System.Collections.Generic;
using SWeetJelly.Tween.Contract;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed partial class TweenPlayerSequenceBehaviour : TweenPlayerBehaviour
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
        private PlaybackRun currentChildRun;
        private Action<bool> currentChildSettled;
        private int generation;
        private PlaybackMode playbackMode;
        private int currentIndex;
        private int direction;
        private bool isSequencePlaying;
        private bool killedEventInvoked;
        private bool initialising;
        private bool starting;

        public override bool IsPlaying => HasManualControl || isSequencePlaying;
        protected override bool SupportsAsyncPlayback => true;
        private bool validatingPlayback;

        protected override void ValidateAsyncPlayback(bool? forward)
        {
            base.ValidateAsyncPlayback(forward);
            if (validatingPlayback) throw new InvalidOperationException("TweenPlayer composition contains a cycle.");
            validatingPlayback = true;
            try
            {
                if (players == null) return;
                foreach (var player in players)
                    if (player != null && player != this && player.isActiveAndEnabled)
                        player.CheckAsyncPlayback(forward);
            }
            finally { validatingPlayback = false; }
        }

        private void OnEnable()
        {
            if (playOnEnable)
                Play();
        }

        private void OnDisable()
        {
            Kill();
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
            int expected = generation + 1;
            StopCurrentSilently();
            if (generation != expected) return;
            int snapGeneration = generation;

            if (players != null)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    TweenPlayerBehaviour player = players[i];

                    if (player != null && player != this)
                    {
                        player.SnapForward();
                        if (generation != snapGeneration) return;
                    }
                }
            }

            InvokePlayed();
            if (generation == snapGeneration) InvokeCompleted();
        }

        public override void SnapBackward()
        {
            int expected = generation + 1;
            StopCurrentSilently();
            if (generation != expected) return;
            int snapGeneration = generation;

            if (players != null)
            {
                for (int i = 0; i < players.Length; i++)
                {
                    TweenPlayerBehaviour player = players[i];

                    if (player != null && player != this)
                    {
                        player.SnapBackward();
                        if (generation != snapGeneration) return;
                    }
                }
            }

            InvokePlayed();
            if (generation == snapGeneration) InvokeCompleted();
        }

        public override void Kill()
        {
            if (StopManualControl()) return;
            if (!isSequencePlaying)
                return;

            var operation = CurrentPlayback;
            var childRun = currentChildRun;
            TweenPlayerBehaviour player = currentPlayer;

            ClearCurrentPlayer();
            isSequencePlaying = false;
            generation++;
            System.Runtime.ExceptionServices.ExceptionDispatchInfo failure = null;
            try { CancelPlayback(operation); }
            catch (Exception error) { if (failure == null) failure = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error); }
            try
            {
                if (childRun != null) childRun.Kill();
                else if (player != null) player.Kill();
            }
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
            BeginPlayback();
            RecordTimeline(mode == PlaybackMode.Default ? null : mode == PlaybackMode.Forward);

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
            if (!player.TracksPlayback)
            {
                currentPlayer.Completed += currentCompleted;
                currentPlayer.Killed += currentKilled;
            }

            try
            {
                void StartChild()
                {
                    switch (playbackMode)
                    {
                        case PlaybackMode.Default: player.Play(); break;
                        case PlaybackMode.Forward: player.PlayForward(); break;
                        case PlaybackMode.Backward: player.PlayBackward(); break;
                    }
                }
                if (player.TracksPlayback)
                    player.ObservePlayback(StartChild, operation =>
                    {
                        currentChildRun = operation;
                        currentChildSettled = completed =>
                        {
                            if (run != generation || !ReferenceEquals(currentChildRun, operation)) return;
                            if (completed) OnCurrentPlayerCompleted();
                            else OnCurrentPlayerKilled();
                        };
                        operation.Settled += currentChildSettled;
                    });
                else StartChild();
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
            generation++;
            var operation = CurrentPlayback;
            NotifyAllTerminals(() => CancelPlayback(operation), InvokeKilled);
        }

        private void StopCurrentSilently()
        {
            if (StopManualControl() && CurrentPlayback != null) return;
            generation++;
            var operation = CurrentPlayback;
            var childRun = currentChildRun;
            TweenPlayerBehaviour player = currentPlayer;

            ClearCurrentPlayer();
            isSequencePlaying = false;
            System.Runtime.ExceptionServices.ExceptionDispatchInfo failure = null;
            try { CancelPlayback(operation); }
            catch (Exception error) { failure = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error); }
            try
            {
                if (childRun != null) childRun.Kill();
                else if (player != null) player.Kill();
            }
            catch (Exception error) { if (failure == null) failure = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error); }
            failure?.Throw();
        }

        private void ClearCurrentPlayer()
        {
            if (currentChildRun != null) currentChildRun.Settled -= currentChildSettled;
            currentChildRun = null;
            currentChildSettled = null;
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
            var operation = CurrentPlayback;
            NotifyAllTerminals(() => CompletePlayback(operation), NotifyCompleted, () => completed?.Invoke());
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
