using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformLocalRotationEulerZTweenPlayer
        : TransformVector3AxisTweenPlayerBase
    {
        protected override Axis TargetAxis => Axis.Z;

        protected override Vector3 CurrentVectorValue
        {
            get => TargetComponent.localEulerAngles;
            set => TargetComponent.localEulerAngles = value;
        }
    }
}
