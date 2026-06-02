using System;
using UnityEngine;

namespace SWeetJelly.Tween.Framework
{
    public sealed class AudioVolumeTweenCase : TweenValueCase<AudioSource, float>
    {
        private readonly GameObject parentObject;

        public AudioVolumeTweenCase(AudioSource audioSource, float resultValue)
            : base(audioSource, resultValue)
        {
            parentObject = audioSource.gameObject;
            startValue = audioSource.volume;
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            target.volume = Mathf.LerpUnclamped(
                startValue,
                resultValue,
                EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.volume = resultValue;
        }
    }
}