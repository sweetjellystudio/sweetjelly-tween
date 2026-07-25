using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformLocalPositionXTweenPlayer
        : TransformVector3AxisTweenPlayerBase
    {
        protected override Axis TargetAxis => Axis.X;

        protected override Vector3 CurrentVectorValue
        {
            get => TargetComponent.localPosition;
            set => TargetComponent.localPosition = value;
        }
    }
}
