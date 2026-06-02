using System.Collections.Generic;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.App
{
    public sealed class TweenPlaybackLane
    {
        private readonly UpdateMethod updateMethod;
        private readonly List<ITweenPlayable> active = new();
        private readonly List<ITweenPlayable> pendingAdd = new();

        private bool isTicking;

        public int Count => active.Count + pendingAdd.Count;
        public int MaxUsedCount { get; private set; }

        public TweenPlaybackLane(UpdateMethod updateMethod)
        {
            this.updateMethod = updateMethod;
        }

        public void Add(ITweenPlayable playable)
        {
            if (playable == null)
                return;

            if (playable.IsActive)
                return;

            playable.Activate(updateMethod);
            AddInternal(playable);
        }

        public void AddAlreadyActivated(ITweenPlayable playable)
        {
            if (playable == null)
                return;

            AddInternal(playable);
        }

        private void AddInternal(ITweenPlayable playable)
        {
            if (isTicking)
                pendingAdd.Add(playable);
            else
                active.Add(playable);

            if (Count > MaxUsedCount)
                MaxUsedCount = Count;
        }

        public void Tick(
            float scaledDeltaTime,
            float unscaledDeltaTime,
            bool skipScaledWhenTimeScaleIsZero)
        {
            if (active.Count == 0 && pendingAdd.Count == 0)
                return;

            isTicking = true;

            int count = active.Count;

            for (int i = 0; i < count; i++)
            {
                ITweenPlayable playable = active[i];

                if (playable == null)
                    continue;

                if (playable.IsKilling)
                    continue;

                if (!playable.Validate())
                {
                    playable.Kill();
                    continue;
                }

                if (!playable.IsActive || playable.IsPaused)
                    continue;

                float deltaTime = playable.IsUnscaled
                    ? unscaledDeltaTime
                    : scaledDeltaTime;

                if (playable.Delay > 0f && playable.CurrentDelay < playable.Delay)
                {
                    float remainingDelay = playable.Delay - playable.CurrentDelay;

                    if (deltaTime < remainingDelay)
                    {
                        playable.UpdateDelay(deltaTime);
                        continue;
                    }

                    playable.UpdateDelay(remainingDelay);
                    deltaTime -= remainingDelay;
                }

                playable.UpdateState(deltaTime);
                playable.Invoke(deltaTime);

                if (playable.IsCompleted)
                    playable.CompleteImmediately();
            }

            isTicking = false;

            RemoveInactive();
            FlushPendingAdd();
        }

        public void PauseAll()
        {
            for (int i = 0; i < active.Count; i++)
                active[i]?.Pause();
        }

        public void ResumeAll()
        {
            for (int i = 0; i < active.Count; i++)
                active[i]?.Resume();
        }

        public void KillAll()
        {
            for (int i = 0; i < active.Count; i++)
                active[i]?.Kill();

            RemoveInactive();
        }

        private void RemoveInactive()
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                ITweenPlayable playable = active[i];

                if (playable == null)
                {
                    active.RemoveAt(i);
                    continue;
                }

                if (!playable.IsActive || playable.IsKilling)
                {
                    playable.Deactivate();
                    active.RemoveAt(i);
                }
            }
        }

        private void FlushPendingAdd()
        {
            if (pendingAdd.Count == 0)
                return;

            active.AddRange(pendingAdd);
            pendingAdd.Clear();
        }
    }
}