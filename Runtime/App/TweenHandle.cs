using System;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.App
{
    public sealed class TweenHandle : ITweenHandle
    {
        private readonly ITweenPlayable playable;

        public bool IsActive => playable.IsActive;
        public bool IsCompleted => playable.IsCompleted;
        public bool IsKilled => playable.IsKilled;

        public event Action Completed;
        public event Action Killed;

        public TweenHandle(ITweenPlayable playable)
        {
            this.playable = playable ?? throw new ArgumentNullException(nameof(playable));

            if (playable is INotifyTweenCompletion notifier)
            {
                notifier.Completed += OnCompleted;
                notifier.Killed += OnKilled;
            }
        }

        public void Pause() => playable.Pause();
        public void Resume() => playable.Resume();
        public void Complete() => playable.CompleteImmediately();
        public void Kill() => playable.Kill();

        private void OnCompleted()
        {
            Completed?.Invoke();
        }

        private void OnKilled()
        {
            Killed?.Invoke();
        }
    }
}
