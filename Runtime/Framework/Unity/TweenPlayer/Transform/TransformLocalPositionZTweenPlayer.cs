using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformLocalPositionZTweenPlayer
        : TransformVector3AxisTweenPlayerBase
    {
        protected override Axis TargetAxis => Axis.Z;

        protected override Vector3 CurrentVectorValue
        {
            get => TargetComponent.localPosition;
            set => TargetComponent.localPosition = value;
        }
    }
}
