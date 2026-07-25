using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformLocalPositionYTweenPlayer
        : TransformVector3AxisTweenPlayerBase
    {
        protected override Axis TargetAxis => Axis.Y;

        protected override Vector3 CurrentVectorValue
        {
            get => TargetComponent.localPosition;
            set => TargetComponent.localPosition = value;
        }
    }
}
