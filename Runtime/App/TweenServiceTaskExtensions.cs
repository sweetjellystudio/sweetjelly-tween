using System;
using System.Threading;
using System.Threading.Tasks;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.App
{
    public static class TweenRunnerTaskExtensions
    {
        public static Task PlayAsync(
            this ITweenService service,
            ITweenPlayable playable,
            CancellationToken ct = default)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            if (playable == null)
                throw new ArgumentNullException(nameof(playable));

            if (ct.IsCancellationRequested)
                return Task.FromCanceled(ct);

            ITweenHandle handle = service.Play(playable);

            return handle.ToTask(ct);
        }

        public static Task PlayAsync(
            this ITweenService service,
            ITweenPlayable playable,
            UpdateMethod updateMethod,
            CancellationToken ct = default)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            if (playable == null)
                throw new ArgumentNullException(nameof(playable));

            if (ct.IsCancellationRequested)
                return Task.FromCanceled(ct);

            ITweenHandle handle = service.Play(playable, updateMethod);

            return handle.ToTask(ct);
        }
    }
}