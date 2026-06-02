using System;

namespace SWeetJelly.Tween.Contract
{
    public interface ITweenHandle
    {
        bool IsActive { get; }
        bool IsCompleted { get; }
        bool IsKilled { get; }

        event Action Completed;
        event Action Killed;

        void Pause();
        void Resume();
        void Complete();
        void Kill();
    }
}