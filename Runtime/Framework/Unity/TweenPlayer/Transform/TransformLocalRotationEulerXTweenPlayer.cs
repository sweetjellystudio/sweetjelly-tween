using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformLocalRotationEulerXTweenPlayer
        : TransformVector3AxisTweenPlayerBase
    {
        protected override Axis TargetAxis => Axis.X;

        protected override Vector3 CurrentVectorValue
        {
            get => TargetComponent.localEulerAngles;
            set => TargetComponent.localEulerAngles = value;
        }
    }
}
