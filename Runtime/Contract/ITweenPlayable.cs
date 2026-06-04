namespace SWeetJelly.Tween.Contract
{
    public interface ITweenPlayable
    {
        UpdateMethod UpdateMethod { get; }
        bool IsActive { get; }
        bool IsCompleted { get; }
        bool IsKilling { get; }
        bool IsKilled { get; }
        bool IsPaused { get; }
        bool IsUnscaled { get; }

        float Delay { get; }
        float CurrentDelay { get; }

        void Activate(UpdateMethod updateMethod);
        void Deactivate();

        bool Validate();
        void UpdateDelay(float deltaTime);
        void UpdateState(float deltaTime);
        void Invoke(float deltaTime);

        void Pause();
        void Resume();
        void Kill();
        void CompleteImmediately();
    }
}
