using System;
using SWeetJelly.Tween.Contract;

namespace SWeetJelly.Tween.App
{
    public static class TweenPlayablePlayExtensions
    {
        public static ITweenHandle Play(
            this ITweenPlayable playable,
            ITweenService service)
        {
            if (playable == null)
                throw new ArgumentNullException(nameof(playable));

            if (service == null)
                throw new ArgumentNullException(nameof(service));

            return service.Play(playable);
        }

        public static ITweenHandle Play(
            this ITweenPlayable playable,
            ITweenService service,
            UpdateMethod updateMethod)
        {
            if (playable == null)
                throw new ArgumentNullException(nameof(playable));

            if (service == null)
                throw new ArgumentNullException(nameof(service));

            return service.Play(playable, updateMethod);
        }
    }
}