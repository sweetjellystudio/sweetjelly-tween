using UnityEngine;
using UnityEngine.UI;

namespace SWeetJelly.Tween.Framework
{
    public sealed class GraphicFadeTweenCase : TweenValueCase<Graphic, float>
    {
        private readonly GameObject parentObject;

        public GraphicFadeTweenCase(Graphic graphic, float resultValue)
            : base(graphic, resultValue)
        {
            parentObject = graphic.gameObject;
            startValue = graphic.color.a;
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            Color color = target.color;

            color.a = Mathf.LerpUnclamped(
                startValue,
                resultValue,
                EvaluateEase(State));

            target.color = color;
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            Color color = target.color;
            color.a = resultValue;
            target.color = color;
        }
    }
}