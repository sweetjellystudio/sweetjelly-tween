using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformScaleZTweenPlayer
        : TransformVector3AxisTweenPlayerBase
    {
        protected override Axis TargetAxis => Axis.Z;

        protected override Vector3 CurrentVectorValue
        {
            get => TargetComponent.localScale;
            set => TargetComponent.localScale = value;
        }
    }
}
