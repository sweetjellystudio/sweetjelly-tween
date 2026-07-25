using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformScaleXTweenPlayer
        : TransformVector3AxisTweenPlayerBase
    {
        protected override Axis TargetAxis => Axis.X;

        protected override Vector3 CurrentVectorValue
        {
            get => TargetComponent.localScale;
            set => TargetComponent.localScale = value;
        }
    }
}
