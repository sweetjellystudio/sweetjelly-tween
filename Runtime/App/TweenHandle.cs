using System;
using System.Runtime.ExceptionServices;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.App
{
    public sealed class TweenHandle : ITweenHandle
    {
        private readonly ITweenPlayable playable;
        private readonly INotifyTweenCompletion notifier;
        private bool terminal;
        private bool completed;
        private bool killed;

        public bool IsActive => !terminal && playable.IsActive;
        public bool IsCompleted => terminal ? completed : notifier == null && playable.IsCompleted;
        public bool IsKilled => terminal ? killed : notifier == null && playable.IsKilled;

        public event Action Completed;
        public event Action Killed;

        public TweenHandle(ITweenPlayable playable)
        {
            this.playable = playable ?? throw new ArgumentNullException(nameof(playable));

            notifier = playable as INotifyTweenCompletion;
            if (!playable.IsActive && (playable.IsCompleted || playable.IsKilled))
            {
                terminal = true;
                killed = playable.IsKilled;
                completed = playable.IsCompleted && !killed;
                return;
            }
            if (notifier != null)
            {
                notifier.Completed += OnCompleted;
                notifier.Killed += OnKilled;
            }
        }

        public void Pause() { if (!terminal) playable.Pause(); }
        public void Resume() { if (!terminal) playable.Resume(); }
        public void Complete()
        {
            if (terminal) return;
            try { playable.CompleteImmediately(); }
            catch
            {
                if (!terminal) { try { playable.Kill(); } catch { } }
                throw;
            }
        }
        public void Kill() { if (!terminal) playable.Kill(); }

        private void OnCompleted()
        {
            if (terminal) return;
            terminal = true;
            completed = true;
            Detach();
            var callbacks = Completed;
            Completed = null;
            Killed = null;
            NotifyTerminal(callbacks);
        }

        private void OnKilled()
        {
            if (terminal) return;
            terminal = true;
            killed = true;
            Detach();
            var callbacks = Killed;
            Completed = null;
            Killed = null;
            NotifyTerminal(callbacks);
        }

        private static void NotifyTerminal(Action callbacks)
        {
            if (callbacks == null) return;
            ExceptionDispatchInfo failure = null;
            foreach (Action callback in callbacks.GetInvocationList())
            {
                try { callback(); }
                catch (Exception error) { if (failure == null) failure = ExceptionDispatchInfo.Capture(error); }
            }
            failure?.Throw();
        }

        private void Detach()
        {
            if (notifier == null) return;
            notifier.Completed -= OnCompleted;
            notifier.Killed -= OnKilled;
        }
    }
}
