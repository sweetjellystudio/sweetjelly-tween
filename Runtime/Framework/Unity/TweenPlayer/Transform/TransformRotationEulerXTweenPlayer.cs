using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformRotationEulerXTweenPlayer
        : TransformVector3AxisTweenPlayerBase
    {
        protected override Axis TargetAxis => Axis.X;

        protected override Vector3 CurrentVectorValue
        {
            get => TargetComponent.eulerAngles;
            set => TargetComponent.eulerAngles = value;
        }
    }
}
