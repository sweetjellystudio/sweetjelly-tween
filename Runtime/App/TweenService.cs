using System;
using System.Collections.Generic;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.App
{
    public sealed class TweenService : ITweenService
    {
        private readonly TweenPlaybackLane updateLane = new(UpdateMethod.Update);
        private readonly TweenPlaybackLane fixedUpdateLane = new(UpdateMethod.FixedUpdate);
        private readonly TweenPlaybackLane lateUpdateLane = new(UpdateMethod.LateUpdate);

        public ITweenHandle Play(ITweenPlayable playable)
        {
            if (playable == null)
                throw new ArgumentNullException(nameof(playable));

            var handle = new TweenHandle(playable);

            GetLane(playable.UpdateMethod).Add(playable);

            return handle;
        }

        public ITweenHandle Play(ITweenPlayable playable, UpdateMethod updateMethod)
        {
            if (playable == null)
                throw new ArgumentNullException(nameof(playable));

            playable.Activate(updateMethod);

            var handle = new TweenHandle(playable);

            GetLane(updateMethod).AddAlreadyActivated(playable);

            return handle;
        }

        public ITweenHandleGroup PlayGroup(params ITweenPlayable[] playables)
        {
            if (playables == null)
                throw new ArgumentNullException(nameof(playables));

            var handles = new List<ITweenHandle>();

            for (int i = 0; i < playables.Length; i++)
            {
                if (playables[i] == null)
                    continue;

                handles.Add(Play(playables[i]));
            }

            return new TweenHandleGroup(handles);
        }

        public void Tick(
            UpdateMethod updateMethod,
            float scaledDeltaTime,
            float unscaledDeltaTime,
            bool skipScaledWhenTimeScaleIsZero)
        {
            GetLane(updateMethod).Tick(
                scaledDeltaTime,
                unscaledDeltaTime,
                skipScaledWhenTimeScaleIsZero);
        }

        public void Pause(UpdateMethod updateMethod) => GetLane(updateMethod).PauseAll();

        public void PauseAll()
        {
            updateLane.PauseAll();
            fixedUpdateLane.PauseAll();
            lateUpdateLane.PauseAll();
        }

        public void Resume(UpdateMethod updateMethod) => GetLane(updateMethod).ResumeAll();

        public void ResumeAll()
        {
            updateLane.ResumeAll();
            fixedUpdateLane.ResumeAll();
            lateUpdateLane.ResumeAll();
        }

        public void Kill(UpdateMethod updateMethod) => GetLane(updateMethod).KillAll();

        public void KillAll()
        {
            updateLane.KillAll();
            fixedUpdateLane.KillAll();
            lateUpdateLane.KillAll();
        }

        private TweenPlaybackLane GetLane(UpdateMethod updateMethod)
        {
            return updateMethod switch
            {
                UpdateMethod.Update => updateLane,
                UpdateMethod.FixedUpdate => fixedUpdateLane,
                UpdateMethod.LateUpdate => lateUpdateLane,
                _ => throw new ArgumentOutOfRangeException(nameof(updateMethod), updateMethod, null)
            };
        }
    }
}