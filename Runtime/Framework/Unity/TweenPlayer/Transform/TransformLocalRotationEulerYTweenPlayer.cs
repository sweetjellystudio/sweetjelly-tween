using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformLocalRotationEulerYTweenPlayer
        : TransformVector3AxisTweenPlayerBase
    {
        protected override Axis TargetAxis => Axis.Y;

        protected override Vector3 CurrentVectorValue
        {
            get => TargetComponent.localEulerAngles;
            set => TargetComponent.localEulerAngles = value;
        }
    }
}
