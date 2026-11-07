using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace SWeetJelly.Tween.Framework.Unity
{
    public abstract partial class TweenPlayerBehaviour
    {
        private PlaybackRun currentPlayback;
        private PlaybackCapture playbackCapture;

        protected virtual bool SupportsAsyncPlayback => false;
        internal bool TracksPlayback => SupportsAsyncPlayback;
        protected PlaybackRun CurrentPlayback => currentPlayback;

        public Task PlayAsync(CancellationToken ct = default) => PlayAsyncCore(null, ct);
        public Task PlayAsync(bool forward, CancellationToken ct = default) => PlayAsyncCore(forward, ct);

        private Task PlayAsyncCore(bool? forward, CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return Task.FromCanceled(ct);
            ValidateAsyncPlayback(forward);
            var run = ObservePlayback(() => { if (forward.HasValue) Play(forward.Value); else Play(); });
            return run.Wait(ct);
        }

        protected virtual void ValidateAsyncPlayback(bool? forward)
        {
            if (!SupportsAsyncPlayback)
                throw new NotSupportedException($"{GetType().Name} does not implement execution-aware playback.");
            if (!isActiveAndEnabled)
                throw new InvalidOperationException($"{GetType().Name} is not active and enabled.");
        }

        internal void CheckAsyncPlayback(bool? forward) => ValidateAsyncPlayback(forward);

        internal PlaybackRun ObservePlayback(Action start, Action<PlaybackRun> observe = null)
        {
            var previous = playbackCapture;
            var capture = new PlaybackCapture { Observe = observe };
            playbackCapture = capture;
            try
            {
                start();
                return capture.Run ?? throw new InvalidOperationException($"{GetType().Name} did not start playback.");
            }
            catch
            {
                if (capture.Run != null) { try { capture.Run.Kill(); } catch { } }
                throw;
            }
            finally { playbackCapture = previous; }
        }

        // Call once per command, not once per loop. Retain this token in delayed callbacks.
        protected PlaybackRun BeginPlayback()
        {
            if (currentPlayback != null)
                throw new InvalidOperationException("End the owned playback before starting another run.");
            var run = new PlaybackRun(this);
            currentPlayback = run;
            if (playbackCapture != null && playbackCapture.Run == null)
            {
                playbackCapture.Run = run;
                playbackCapture.Observe?.Invoke(run);
            }
            return run;
        }

        protected bool IsCurrentPlayback(PlaybackRun run) =>
            run != null && ReferenceEquals(currentPlayback, run) && run.IsActive;

        protected void CompletePlayback(PlaybackRun run) => EndPlayback(run, true);
        protected void CancelPlayback(PlaybackRun run) => EndPlayback(run, false);

        private void EndPlayback(PlaybackRun run, bool completed)
        {
            if (run == null || !ReferenceEquals(currentPlayback, run)) return;
            currentPlayback = null;
            run.Finish(completed);
        }

        protected virtual void OnDestroy()
        {
            var run = currentPlayback;
            if (run != null) run.Kill();
        }

        // Opaque protected token for subclass hooks; it is not a public playback handle.
        protected internal sealed class PlaybackRun
        {
            private readonly TweenPlayerBehaviour owner;
            private int outcome;
            internal bool IsActive => outcome == 0;
            internal bool IsCompleted => outcome == 1;
            internal event Action<bool> Settled;
            internal PlaybackRun(TweenPlayerBehaviour owner) { this.owner = owner; }

            internal void Kill()
            {
                if (!IsActive) return;
                try { if (owner != null && owner.IsCurrentPlayback(this)) owner.Kill(); }
                finally { owner.EndPlayback(this, false); }
            }

            internal void Finish(bool completed)
            {
                if (!IsActive) return;
                outcome = completed ? 1 : 2;
                var callbacks = Settled;
                Settled = null;
                ExceptionDispatchInfo failure = null;
                if (callbacks != null)
                    foreach (Action<bool> callback in callbacks.GetInvocationList())
                        try { callback(completed); }
                        catch (Exception error) { if (failure == null) failure = ExceptionDispatchInfo.Capture(error); }
                failure?.Throw();
            }

            internal Task Wait(CancellationToken ct)
            {
                if (!IsActive) return IsCompleted ? Task.CompletedTask : Task.FromCanceled(new CancellationToken(true));
                var completion = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                CancellationTokenRegistration registration = default;
                bool finished = false;
                void Settle(bool completed)
                {
                    if (finished) return;
                    finished = true;
                    Settled -= Settle;
                    registration.Dispose();
                    if (completed) completion.TrySetResult(null);
                    else if (ct.IsCancellationRequested) completion.TrySetCanceled(ct);
                    else completion.TrySetCanceled();
                }
                void Cancel()
                {
                    try { Kill(); }
                    finally { Settle(false); }
                }
                Settled += Settle;
                if (ct.CanBeCanceled)
                {
                    registration = ct.Register(Cancel);
                    if (finished) registration.Dispose();
                }
                return completion.Task;
            }
        }

        private sealed class PlaybackCapture
        {
            internal PlaybackRun Run;
            internal Action<PlaybackRun> Observe;
        }
    }
}
