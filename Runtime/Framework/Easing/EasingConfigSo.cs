using System.Collections.Generic;
using UnityEngine;

namespace SWeetJelly.Tween.Framework
{
    [CreateAssetMenu(
        fileName = "EasingConfig",
        menuName = "SWeetJelly/Tween/Easing Config")]
    public sealed class EasingConfigSo : ScriptableObject
    {
        [SerializeField]
        private AnimationCurveEasingFunction[] customEasingFunctions = new AnimationCurveEasingFunction[0];

        public IReadOnlyList<AnimationCurveEasingFunction> CustomEasingFunctions
            => customEasingFunctions;

        public void Initialise()
        {
            if (customEasingFunctions == null)
                return;

            for (int i = 0; i < customEasingFunctions.Length; i++)
            {
                customEasingFunctions[i]?.Initialise();
            }
        }
    }
}