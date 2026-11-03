using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.Domain;

namespace SWeetJelly.Tween.App
{
    public sealed class TweenService : ITweenService, IDisposable
    {
        private readonly TweenPlaybackLane updateLane = new(UpdateMethod.Update);
        private readonly TweenPlaybackLane fixedUpdateLane = new(UpdateMethod.FixedUpdate);
        private readonly TweenPlaybackLane lateUpdateLane = new(UpdateMethod.LateUpdate);
        private bool disposed;

        public ITweenHandle Play(ITweenPlayable playable)
        {
            ThrowIfDisposed();
            if (playable == null)
                throw new ArgumentNullException(nameof(playable));

            if (playable.IsActive)
                throw new InvalidOperationException("The playable is already active.");

            var lane = GetLane(playable.UpdateMethod);
            PrepareToPlay(playable);
            playable.Activate(playable.UpdateMethod);
            var handle = new TweenHandle(playable);
            lane.AddAlreadyActivated(playable);

            return handle;
        }

        public ITweenHandle Play(ITweenPlayable playable, UpdateMethod updateMethod)
        {
            ThrowIfDisposed();
            if (playable == null)
                throw new ArgumentNullException(nameof(playable));

            if (playable.IsActive)
                throw new InvalidOperationException("The playable is already active.");

            var lane = GetLane(updateMethod);
            PrepareToPlay(playable);
            playable.Activate(updateMethod);

            var handle = new TweenHandle(playable);

            lane.AddAlreadyActivated(playable);

            return handle;
        }

        public ITweenHandleGroup PlayGroup(params ITweenPlayable[] playables)
        {
            ThrowIfDisposed();
            if (playables == null)
                throw new ArgumentNullException(nameof(playables));

            var handles = new List<ITweenHandle>();

            try
            {
                for (int i = 0; i < playables.Length; i++)
                {
                    if (playables[i] != null) handles.Add(Play(playables[i]));
                }
                return new TweenHandleGroup(handles);
            }
            catch
            {
                foreach (var handle in handles) { try { handle.Kill(); } catch { } }
                throw;
            }
        }

        public void Tick(
            UpdateMethod updateMethod,
            float scaledDeltaTime,
            float unscaledDeltaTime,
            bool skipScaledWhenTimeScaleIsZero)
        {
            ThrowIfDisposed();
            scaledDeltaTime = TweenTiming.Delta(scaledDeltaTime);
            unscaledDeltaTime = TweenTiming.Delta(unscaledDeltaTime);
            GetLane(updateMethod).Tick(
                scaledDeltaTime,
                unscaledDeltaTime,
                skipScaledWhenTimeScaleIsZero);
        }

        public void Pause(UpdateMethod updateMethod) => GetLane(updateMethod).PauseAll();

        public void PauseAll()
        {
            updateLane.PauseAll();
            fixedUpdateLane.PauseAll();
            lateUpdateLane.PauseAll();
        }

        public void Resume(UpdateMethod updateMethod) => GetLane(updateMethod).ResumeAll();

        public void ResumeAll()
        {
            updateLane.ResumeAll();
            fixedUpdateLane.ResumeAll();
            lateUpdateLane.ResumeAll();
        }

        public void Kill(UpdateMethod updateMethod) => GetLane(updateMethod).KillAll();

        public void KillAll()
        {
            var lanes = new[] { updateLane, fixedUpdateLane, lateUpdateLane };
            var snapshots = new[] { updateLane.Snapshot(), fixedUpdateLane.Snapshot(), lateUpdateLane.Snapshot() };
            ExceptionDispatchInfo failure = null;
            for (int i = 0; i < lanes.Length; i++)
            {
                try { lanes[i].KillSnapshot(snapshots[i]); }
                catch (Exception error) { if (failure == null) failure = ExceptionDispatchInfo.Capture(error); }
            }
            failure?.Throw();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            KillAll();
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(TweenService));
        }

        private void PrepareToPlay(ITweenPlayable playable)
        {
            updateLane.PrepareToPlay(playable);
            fixedUpdateLane.PrepareToPlay(playable);
            lateUpdateLane.PrepareToPlay(playable);
        }

        private TweenPlaybackLane GetLane(UpdateMethod updateMethod)
        {
            return updateMethod switch
            {
                UpdateMethod.Update => updateLane,
                UpdateMethod.FixedUpdate => fixedUpdateLane,
                UpdateMethod.LateUpdate => lateUpdateLane,
                _ => throw new ArgumentOutOfRangeException(nameof(updateMethod), updateMethod, null)
            };
        }
    }
}
