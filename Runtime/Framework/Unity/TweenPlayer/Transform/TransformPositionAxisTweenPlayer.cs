using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformPositionAxisTweenPlayer
        : TransformVector3AxisTweenPlayerBase
    {
        [SerializeField] private Axis axis = Axis.Y;

        protected override Axis TargetAxis => axis;

        protected override Vector3 CurrentVectorValue
        {
            get => TargetComponent.position;
            set => TargetComponent.position = value;
        }
    }
}
