using System;
using System.Collections.Generic;
using System.Threading;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.App
{
    public sealed class TweenHandleGroup : ITweenHandleGroup
    {
        private readonly List<ITweenHandle> handles;
        private int completedCount;
        private int killedCount;
        private bool completedEventInvoked;
        private bool killedEventInvoked;

        public IReadOnlyList<ITweenHandle> Handles => handles;

        public bool IsActive
        {
            get
            {
                for (int i = 0; i < handles.Count; i++)
                {
                    if (handles[i].IsActive)
                        return true;
                }

                return false;
            }
        }

        public bool IsCompleted
        {
            get
            {
                if (handles.Count == 0)
                    return false;

                for (int i = 0; i < handles.Count; i++)
                {
                    if (!handles[i].IsCompleted)
                        return false;
                }

                return true;
            }
        }

        public bool IsKilled
        {
            get
            {
                if (handles.Count == 0)
                    return false;

                for (int i = 0; i < handles.Count; i++)
                {
                    if (!handles[i].IsKilled)
                        return false;
                }

                return true;
            }
        }

        public event Action Completed;
        public event Action Killed;

        public TweenHandleGroup(IReadOnlyList<ITweenHandle> handles)
        {
            if (handles == null)
                throw new ArgumentNullException(nameof(handles));

            this.handles = new List<ITweenHandle>(handles.Count);

            for (int i = 0; i < handles.Count; i++)
            {
                ITweenHandle handle = handles[i];

                if (handle == null)
                    continue;

                this.handles.Add(handle);

                if (handle.IsCompleted)
                    completedCount++;

                if (handle.IsKilled)
                    killedCount++;

                handle.Completed += OnHandleCompleted;
                handle.Killed += OnHandleKilled;
            }

            TryInvokeCompleted();
            TryInvokeKilled();
        }

        public void Pause()
        {
            for (int i = 0; i < handles.Count; i++)
                handles[i].Pause();
        }

        public void Resume()
        {
            for (int i = 0; i < handles.Count; i++)
                handles[i].Resume();
        }

        public void Complete()
        {
            for (int i = 0; i < handles.Count; i++)
                handles[i].Complete();
        }

        public void Kill()
        {
            for (int i = 0; i < handles.Count; i++)
                handles[i].Kill();
        }

        private void OnHandleCompleted()
        {
            Interlocked.Increment(ref completedCount);
            TryInvokeCompleted();
        }

        private void OnHandleKilled()
        {
            Interlocked.Increment(ref killedCount);
            TryInvokeKilled();
        }

        private void TryInvokeCompleted()
        {
            if (completedEventInvoked)
                return;

            if (handles.Count == 0)
                return;

            if (completedCount < handles.Count)
                return;

            completedEventInvoked = true;
            Completed?.Invoke();
        }

        private void TryInvokeKilled()
        {
            if (killedEventInvoked)
                return;

            if (handles.Count == 0)
                return;

            if (killedCount < handles.Count)
                return;

            killedEventInvoked = true;
            Killed?.Invoke();
        }
    }
}