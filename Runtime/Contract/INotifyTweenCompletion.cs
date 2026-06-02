using System;

namespace SWeetJelly.Tween.Contract
{
    public interface INotifyTweenCompletion
    {
        event Action Completed;
        event Action Killed;
    }
}