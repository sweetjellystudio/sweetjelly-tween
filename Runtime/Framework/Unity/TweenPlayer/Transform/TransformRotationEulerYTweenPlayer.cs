using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformRotationEulerYTweenPlayer
        : TransformVector3AxisTweenPlayerBase
    {
        protected override Axis TargetAxis => Axis.Y;

        protected override Vector3 CurrentVectorValue
        {
            get => TargetComponent.eulerAngles;
            set => TargetComponent.eulerAngles = value;
        }
    }
}
