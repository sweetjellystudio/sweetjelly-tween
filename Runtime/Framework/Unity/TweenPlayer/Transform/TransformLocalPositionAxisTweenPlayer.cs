using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformLocalPositionAxisTweenPlayer
        : TransformVector3AxisTweenPlayerBase
    {
        [SerializeField] private Axis axis = Axis.Y;

        protected override Axis TargetAxis => axis;

        protected override Vector3 CurrentVectorValue
        {
            get => TargetComponent.localPosition;
            set => TargetComponent.localPosition = value;
        }
    }
}
