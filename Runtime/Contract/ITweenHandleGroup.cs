using System.Collections.Generic;

namespace SWeetJelly.Tween.Contract
{
    public interface ITweenHandleGroup : ITweenHandle
    {
        IReadOnlyList<ITweenHandle> Handles { get; }
    }
}