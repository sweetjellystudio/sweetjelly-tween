using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformScaleYTweenPlayer
        : TransformVector3AxisTweenPlayerBase
    {
        protected override Axis TargetAxis => Axis.Y;

        protected override Vector3 CurrentVectorValue
        {
            get => TargetComponent.localScale;
            set => TargetComponent.localScale = value;
        }
    }
}
