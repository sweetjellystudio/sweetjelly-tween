using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformPositionXTweenPlayer
        : TransformVector3AxisTweenPlayerBase
    {
        protected override Axis TargetAxis => Axis.X;

        protected override Vector3 CurrentVectorValue
        {
            get => TargetComponent.position;
            set => TargetComponent.position = value;
        }
    }
}
