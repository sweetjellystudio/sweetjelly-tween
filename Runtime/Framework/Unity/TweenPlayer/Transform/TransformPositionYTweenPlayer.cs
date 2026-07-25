using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformPositionYTweenPlayer
        : TransformVector3AxisTweenPlayerBase
    {
        protected override Axis TargetAxis => Axis.Y;

        protected override Vector3 CurrentVectorValue
        {
            get => TargetComponent.position;
            set => TargetComponent.position = value;
        }
    }
}
