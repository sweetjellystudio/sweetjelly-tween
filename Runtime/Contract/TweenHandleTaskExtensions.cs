using System;
using System.Threading;
using System.Threading.Tasks;

namespace SWeetJelly.Tween.Contract
{
    public static class TweenHandleTaskExtensions
    {
        public static Task ToTask(this ITweenHandle handle, CancellationToken ct = default)
        {
            if (handle == null)
                throw new ArgumentNullException(nameof(handle));

            if (handle.IsCompleted)
                return Task.CompletedTask;

            if (ct.IsCancellationRequested)
            {
                handle.Kill();
                return Task.FromCanceled(ct);
            }

            var tcs = new TaskCompletionSource<object>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            int finished = 0;
            CancellationTokenRegistration ctr = default;

            void DisposeRegistration()
            {
                if (ct.CanBeCanceled)
                    ctr.Dispose();
            }

            void Complete()
            {
                if (Interlocked.Exchange(ref finished, 1) != 0)
                    return;

                DisposeRegistration();
                tcs.TrySetResult(null);
            }

            void Kill()
            {
                if (Interlocked.Exchange(ref finished, 1) != 0)
                    return;

                DisposeRegistration();

                if (ct.IsCancellationRequested)
                    tcs.TrySetCanceled(ct);
                else
                    tcs.TrySetCanceled();
            }

            void Cancel()
            {
                if (Interlocked.Exchange(ref finished, 1) != 0)
                    return;

                handle.Kill();
                DisposeRegistration();
                tcs.TrySetCanceled(ct);
            }

            handle.Completed += Complete;
            handle.Killed += Kill;

            if (ct.CanBeCanceled)
                ctr = ct.Register(Cancel);

            return tcs.Task;
        }
    }
}