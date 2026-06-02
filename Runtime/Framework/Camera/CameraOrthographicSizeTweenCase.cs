using UnityEngine;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework
{
    public sealed class CameraOrthographicSizeTweenCase : TweenValueCase<Camera, float>
    {
        private readonly GameObject parentObject;

        public CameraOrthographicSizeTweenCase(Camera camera, float resultValue)
            : base(camera, resultValue)
        {
            parentObject = camera.gameObject;
            startValue = camera.orthographicSize;
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            target.orthographicSize = Mathf.LerpUnclamped(
                startValue,
                resultValue,
                EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.orthographicSize = resultValue;
        }
    }
}