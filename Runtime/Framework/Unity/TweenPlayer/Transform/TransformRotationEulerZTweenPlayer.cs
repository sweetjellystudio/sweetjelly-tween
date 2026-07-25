using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformRotationEulerZTweenPlayer
        : TransformVector3AxisTweenPlayerBase
    {
        protected override Axis TargetAxis => Axis.Z;

        protected override Vector3 CurrentVectorValue
        {
            get => TargetComponent.eulerAngles;
            set => TargetComponent.eulerAngles = value;
        }
    }
}
