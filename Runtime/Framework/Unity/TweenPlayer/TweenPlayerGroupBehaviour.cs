using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using SWeetJelly.Tween.Contract;
using UnityEngine;
using UnityEngine.Events;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed partial class TweenPlayerGroupBehaviour : TweenPlayerBehaviour
    {
        [SerializeField] private bool playOnEnable;
        [SerializeField] private TweenPlayerBehaviour[] players;

        [Header("Events")]
        [SerializeField] private UnityEvent played;
        [SerializeField] private UnityEvent completed;
        [SerializeField] private UnityEvent killed;

        private readonly List<ChildRun> children = new();
        private int generation;
        private int completedCount;
        private bool isPlaying;
        private bool starting;
        private bool initialising;

        public override bool IsPlaying => HasManualControl || isPlaying;
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

        private void OnEnable() { if (playOnEnable) Play(); }
        private void OnDisable() => Kill();

        public override void Play() => PlayEach(player => player.Play(), null);
        public override void Play(bool forward) { if (forward) PlayForward(); else PlayBackward(); }
        public override void PlayForward() => PlayEach(player => player.PlayForward(), true);
        public override void PlayBackward() => PlayEach(player => player.PlayBackward(), false);
        public override void Snap(bool forward) { if (forward) SnapForward(); else SnapBackward(); }
        public override void SnapForward() => SnapEach(player => player.SnapForward());
        public override void SnapBackward() => SnapEach(player => player.SnapBackward());
        public override void Kill() { if (StopManualControl()) return; if (isPlaying) Finish(true); }
        public override void SetPlayOnEnable(bool playOnEnable) => this.playOnEnable = playOnEnable;

        public override void Initialise(ITweenService tweenService)
        {
            if (initialising) throw new InvalidOperationException("TweenPlayer composition contains a cycle.");
            initialising = true;
            try { foreach (var player in Members(false)) player.Initialise(tweenService); }
            finally { initialising = false; }
        }

        private IEnumerable<TweenPlayerBehaviour> Members(bool activeOnly)
        {
            if (players == null) yield break;
            var seen = new HashSet<TweenPlayerBehaviour>();
            foreach (var player in players)
                if (player != null && player != this && (!activeOnly || player.isActiveAndEnabled) && seen.Add(player))
                    yield return player;
        }

        private bool StopForReplacement()
        {
            if (StopManualControl() && CurrentPlayback != null) return false;
            int expected = generation + (isPlaying ? 1 : 0);
            if (isPlaying) Finish(true, false);
            return generation == expected;
        }

        private void PlayEach(Action<TweenPlayerBehaviour> action, bool? forward)
        {
            if (!isActiveAndEnabled) return;
            if (starting && isPlaying) throw new InvalidOperationException("TweenPlayer composition contains a playback cycle.");
            if (!StopForReplacement()) return;
            int run = ++generation;
            completedCount = 0;
            isPlaying = true;
            BeginPlayback();
            RecordTimeline(forward);
            foreach (var player in Members(true))
            {
                var child = new ChildRun { Player = player };
                child.Completed = () => OnChildFinished(child, run, false);
                child.Killed = () => OnChildFinished(child, run, true);
                children.Add(child);
                if (!player.TracksPlayback)
                {
                    player.Completed += child.Completed;
                    player.Killed += child.Killed;
                }
            }
            var snapshot = children.ToArray();
            starting = true;
            try
            {
                NotifyPlayed();
                played?.Invoke();
                if (run != generation || !isPlaying) return;
                if (snapshot.Length == 0) { Finish(false); return; }
                foreach (var child in snapshot)
                {
                    if (run != generation || !isPlaying) return;
                    if (child.Player == null || !child.Player.isActiveAndEnabled) { Finish(true); return; }
                    child.Started = true;
                    if (child.Player.TracksPlayback)
                        child.Player.ObservePlayback(() => action(child.Player), operation =>
                        {
                            child.Operation = operation;
                            child.Settled = completed => OnChildFinished(child, run, !completed);
                            operation.Settled += child.Settled;
                        });
                    else action(child.Player);
                }
            }
            catch
            {
                if (run == generation && isPlaying) { try { Finish(true); } catch { } }
                throw;
            }
            finally { starting = false; }
        }

        private void OnChildFinished(ChildRun child, int run, bool wasKilled)
        {
            if (run != generation || !isPlaying || !child.Started || child.Finished) return;
            child.Finished = true;
            if (wasKilled) { Finish(true); return; }
            completedCount++;
            if (completedCount == children.Count) Finish(false);
        }

        private void Finish(bool wasKilled, bool notify = true)
        {
            if (!isPlaying) return;
            var operation = CurrentPlayback;
            var snapshot = children.ToArray();
            children.Clear();
            isPlaying = false;
            generation++;
            ExceptionDispatchInfo failure = null;
            foreach (var child in snapshot)
            {
                if (child.Operation != null) child.Operation.Settled -= child.Settled;
                if (child.Player == null) continue;
                TryCleanup(() => child.Player.Completed -= child.Completed, ref failure);
                TryCleanup(() => child.Player.Killed -= child.Killed, ref failure);
            }
            TryCleanup(() =>
            {
                if (wasKilled) CancelPlayback(operation);
                else CompletePlayback(operation);
            }, ref failure);
            if (wasKilled)
                foreach (var child in snapshot)
                    if (child.Started && !child.Finished && child.Player != null)
                        TryCleanup(() =>
                        {
                            if (child.Operation != null) child.Operation.Kill();
                            else child.Player.Kill();
                        }, ref failure);

            if (notify)
            {
                if (wasKilled)
                {
                    TryCleanup(NotifyKilled, ref failure);
                    TryCleanup(() => killed?.Invoke(), ref failure);
                }
                else
                {
                    TryCleanup(NotifyCompleted, ref failure);
                    TryCleanup(() => completed?.Invoke(), ref failure);
                }
            }
            failure?.Throw();
        }

        private void SnapEach(Action<TweenPlayerBehaviour> action)
        {
            if (!StopForReplacement()) return;
            int run = ++generation;
            isPlaying = true;
            BeginPlayback();
            try
            {
                NotifyPlayed();
                played?.Invoke();
                foreach (var player in Members(false))
                {
                    if (run != generation || !isPlaying) return;
                    action(player);
                }
                if (run == generation && isPlaying) Finish(false);
            }
            catch
            {
                if (run == generation && isPlaying) { try { Finish(true); } catch { } }
                throw;
            }
        }

        private static void TryCleanup(Action action, ref ExceptionDispatchInfo failure)
        {
            try { action(); }
            catch (Exception error) { if (failure == null) failure = ExceptionDispatchInfo.Capture(error); }
        }

        private sealed class ChildRun
        {
            public TweenPlayerBehaviour Player;
            public Action Completed;
            public Action Killed;
            public PlaybackRun Operation;
            public Action<bool> Settled;
            public bool Started;
            public bool Finished;
        }
    }
}
