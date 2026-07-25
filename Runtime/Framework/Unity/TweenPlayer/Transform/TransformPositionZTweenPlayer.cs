using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformPositionZTweenPlayer
        : TransformVector3AxisTweenPlayerBase
    {
        protected override Axis TargetAxis => Axis.Z;

        protected override Vector3 CurrentVectorValue
        {
            get => TargetComponent.position;
            set => TargetComponent.position = value;
        }
    }
}
