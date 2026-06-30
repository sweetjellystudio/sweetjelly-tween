using UnityEngine;
using UnityEngine.UI;
using SWeetJelly.Tween.Contract;
using SWeetJelly.Tween.App;

namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class GraphicColorTweenCase : TweenValueCase<Graphic, Color>
    {
        private readonly GameObject parentObject;

        public GraphicColorTweenCase(Graphic graphic, Color resultValue)
            : base(graphic, resultValue)
        {
            parentObject = graphic.gameObject;
            startValue = graphic.color;
        }

        public override bool Validate()
        {
            return parentObject != null && target != null;
        }

        public override void Invoke(float deltaTime)
        {
            target.color = Color.LerpUnclamped(
                startValue,
                resultValue,
                EvaluateEase(State));
        }

        public override void DefaultComplete()
        {
            if (target == null)
                return;

            target.color = resultValue;
        }
    }
}
