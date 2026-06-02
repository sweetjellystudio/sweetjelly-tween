namespace SWeetJelly.Tween.Contract
{
    public interface ITweenService
    {
        ITweenHandle Play(ITweenPlayable tweenCase);
        ITweenHandle Play(ITweenPlayable tweenCase, UpdateMethod updateMethod);
        ITweenHandleGroup PlayGroup(params ITweenPlayable[] playables);

        void Pause(UpdateMethod updateMethod);
        void PauseAll();

        void Resume(UpdateMethod updateMethod);
        void ResumeAll();

        void Kill(UpdateMethod updateMethod);
        void KillAll();
    }
}