using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.App
{
    public sealed class TweenHandleGroup : ITweenHandleGroup
    {
        private readonly List<ITweenHandle> handles = new();
        private bool terminal;
        public IReadOnlyList<ITweenHandle> Handles => handles;
        public bool IsActive => !terminal;
        public bool IsCompleted { get; private set; }
        public bool IsKilled { get; private set; }
        public event Action Completed;
        public event Action Killed;

        public TweenHandleGroup(IReadOnlyList<ITweenHandle> handles)
        {
            if (handles == null) throw new ArgumentNullException(nameof(handles));
            for (int i = 0; i < handles.Count; i++)
            {
                var handle = handles[i];
                if (handle != null && !this.handles.Contains(handle)) this.handles.Add(handle);
            }
            foreach (var handle in this.handles)
            {
                handle.Completed += Evaluate;
                handle.Killed += Evaluate;
            }
            Evaluate();
        }

        public void Pause() { if (!terminal) foreach (var handle in handles) handle.Pause(); }
        public void Resume() { if (!terminal) foreach (var handle in handles) handle.Resume(); }
        public void Complete()
        {
            if (terminal) return;
            ExceptionDispatchInfo failure = null;
            foreach (var handle in handles)
            {
                if (terminal) break;
                try { handle.Complete(); }
                catch (Exception error) { if (failure == null) failure = ExceptionDispatchInfo.Capture(error); }
            }
            failure?.Throw();
        }
        public void Kill() { if (!terminal) Finish(true); }

        private void Evaluate()
        {
            if (terminal) return;
            bool allCompleted = true;
            foreach (var handle in handles)
            {
                if (handle.IsKilled) { Finish(true); return; }
                if (!handle.IsCompleted) allCompleted = false;
            }
            if (allCompleted) Finish(false);
        }

        private void Finish(bool killed)
        {
            terminal = true;
            IsKilled = killed;
            IsCompleted = !killed;
            var callbacks = killed ? Killed : Completed;
            Completed = null;
            Killed = null;
            ExceptionDispatchInfo failure = null;
            foreach (var handle in handles)
            {
                handle.Completed -= Evaluate;
                handle.Killed -= Evaluate;
            }
            if (killed)
            {
                foreach (var handle in handles)
                {
                    if (!handle.IsActive) continue;
                    try { handle.Kill(); }
                    catch (Exception error) { if (failure == null) failure = ExceptionDispatchInfo.Capture(error); }
                }
            }
            if (callbacks != null)
            {
                foreach (Action callback in callbacks.GetInvocationList())
                {
                    try { callback(); }
                    catch (Exception error) { if (failure == null) failure = ExceptionDispatchInfo.Capture(error); }
                }
            }
            failure?.Throw();
        }
    }
}
