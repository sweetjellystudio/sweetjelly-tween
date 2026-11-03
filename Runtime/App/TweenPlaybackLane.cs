using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.App
{
    public sealed class TweenPlaybackLane
    {
        private readonly UpdateMethod updateMethod;
        private readonly List<ITweenPlayable> active = new();
        private readonly List<ITweenPlayable> pendingAdd = new();

        private bool isTicking;

        internal void PrepareToPlay(ITweenPlayable playable)
        {
            bool registered = active.Exists(item => ReferenceEquals(item, playable)) ||
                pendingAdd.Exists(item => ReferenceEquals(item, playable));
            if (!registered) return;
            if (isTicking || playable.IsActive)
                throw new InvalidOperationException("The previous playback must leave its lane before reuse.");
            active.RemoveAll(item => ReferenceEquals(item, playable));
            pendingAdd.RemoveAll(item => ReferenceEquals(item, playable));
            playable.Deactivate();
        }

        public int Count => active.Count + pendingAdd.Count;
        public int MaxUsedCount { get; private set; }

        public TweenPlaybackLane(UpdateMethod updateMethod)
        {
            this.updateMethod = updateMethod;
        }

        public void Add(ITweenPlayable playable)
        {
            if (playable == null)
                return;

            if (playable.IsActive)
                return;

            playable.Activate(updateMethod);
            AddInternal(playable);
        }

        public void AddAlreadyActivated(ITweenPlayable playable)
        {
            if (playable == null)
                return;

            AddInternal(playable);
        }

        private void AddInternal(ITweenPlayable playable)
        {
            if (isTicking)
                pendingAdd.Add(playable);
            else
                active.Add(playable);

            if (Count > MaxUsedCount)
                MaxUsedCount = Count;
        }

        public void Tick(
            float scaledDeltaTime,
            float unscaledDeltaTime,
            bool skipScaledWhenTimeScaleIsZero)
        {
            if (isTicking) throw new InvalidOperationException("Tick cannot reenter the same playback lane.");
            if (active.Count == 0 && pendingAdd.Count == 0)
                return;

            isTicking = true;
            ExceptionDispatchInfo failure = null;

            try
            {
                int count = active.Count;
                for (int i = 0; i < count; i++)
                {
                    ITweenPlayable playable = active[i];
                    if (playable == null || playable.IsKilling) continue;
                    try
                    {
                        if (!playable.Validate())
                        {
                            playable.Kill();
                            continue;
                        }
                        if (!playable.IsActive || playable.IsPaused) continue;
                        if (skipScaledWhenTimeScaleIsZero && !playable.IsUnscaled && scaledDeltaTime == 0f)
                            continue;

                        float deltaTime = playable.IsUnscaled ? unscaledDeltaTime : scaledDeltaTime;
                        if (playable.Delay > 0f && playable.CurrentDelay < playable.Delay)
                        {
                            float remainingDelay = playable.Delay - playable.CurrentDelay;
                            if (deltaTime < remainingDelay)
                            {
                                playable.UpdateDelay(deltaTime);
                                continue;
                            }
                            playable.UpdateDelay(remainingDelay);
                            deltaTime -= remainingDelay;
                        }
                        playable.UpdateState(deltaTime);
                        if (!playable.IsActive || playable.IsKilled || playable.IsKilling) continue;
                        playable.Invoke(deltaTime);
                        if (playable.IsActive && !playable.IsKilled && playable.IsCompleted)
                            playable.CompleteImmediately();
                    }
                    catch
                    {
                        // Preserve the execution failure if a Kill callback also fails.
                        try { playable.Kill(); } catch { }
                        throw;
                    }
                }
            }
            catch (Exception error) { failure = ExceptionDispatchInfo.Capture(error); }
            finally
            {
                isTicking = false;
                try { RemoveInactive(); }
                catch (Exception error) { if (failure == null) failure = ExceptionDispatchInfo.Capture(error); }
                finally { FlushPendingAdd(); }
            }
            failure?.Throw();
        }

        public void PauseAll()
        {
            var targets = new List<ITweenPlayable>(active);
            targets.AddRange(pendingAdd);
            foreach (var playable in targets) playable?.Pause();
        }

        public void ResumeAll()
        {
            var targets = new List<ITweenPlayable>(active);
            targets.AddRange(pendingAdd);
            foreach (var playable in targets) playable?.Resume();
        }

        internal ITweenPlayable[] Snapshot()
        {
            var targets = new List<ITweenPlayable>(active);
            targets.AddRange(pendingAdd);
            return targets.ToArray();
        }

        public void KillAll() => KillSnapshot(Snapshot());

        internal void KillSnapshot(ITweenPlayable[] targets)
        {
            ExceptionDispatchInfo failure = null;
            for (int i = 0; i < targets.Length; i++)
            {
                try { targets[i]?.Kill(); }
                catch (Exception error) { if (failure == null) failure = ExceptionDispatchInfo.Capture(error); }
            }

            if (!isTicking)
            {
                try { RemoveInactive(); }
                catch (Exception error) { if (failure == null) failure = ExceptionDispatchInfo.Capture(error); }
            }
            failure?.Throw();
        }

        private void RemoveInactive()
        {
            ExceptionDispatchInfo failure = null;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                ITweenPlayable playable = active[i];

                if (playable == null)
                {
                    active.RemoveAt(i);
                    continue;
                }

                if (!playable.IsActive || playable.IsKilling)
                {
                    active.RemoveAt(i);
                    try { playable.Deactivate(); }
                    catch (Exception error) { if (failure == null) failure = ExceptionDispatchInfo.Capture(error); }
                }
            }
            failure?.Throw();
        }

        private void FlushPendingAdd()
        {
            if (pendingAdd.Count == 0)
                return;

            foreach (var playable in pendingAdd)
                if (playable != null && playable.IsActive && !playable.IsKilling) active.Add(playable);
            pendingAdd.Clear();
        }
    }
}
