using UnityEngine;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class TransformLocalRotationEulerAxisTweenPlayer
        : TransformVector3AxisTweenPlayerBase
    {
        [SerializeField] private Axis axis = Axis.Z;

        protected override Axis TargetAxis => axis;

        protected override Vector3 CurrentVectorValue
        {
            get => TargetComponent.localEulerAngles;
            set => TargetComponent.localEulerAngles = value;
        }
    }
}
