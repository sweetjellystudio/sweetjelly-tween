using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformScaleAxisTweenPlayer
        : TransformVector3AxisTweenPlayerBase
    {
        [SerializeField] private Axis axis = Axis.Y;

        protected override Axis TargetAxis => axis;

        protected override Vector3 CurrentVectorValue
        {
            get => TargetComponent.localScale;
            set => TargetComponent.localScale = value;
        }
    }
}
