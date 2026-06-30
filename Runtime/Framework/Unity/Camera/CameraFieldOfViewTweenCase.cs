using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class CameraFieldOfViewTweenCase : TweenValueCase<Camera, float>
    {
        private readonly GameObject parentObject;

        public CameraFieldOfViewTweenCase(Camera camera, float resultValue)
            : base(camera, resultValue)
        {
            parentObject = camera.gameObject;
            startValue = camera.fieldOfView;
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            target.fieldOfView = Mathf.LerpUnclamped(
                startValue,
                resultValue,
                EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.fieldOfView = resultValue;
        }
    }
}