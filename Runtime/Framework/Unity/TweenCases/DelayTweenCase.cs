namespace SWeetJelly.Tween.Framework.Unity
{
    public sealed class DelayTweenCase : TweenCase
    {
        public override bool Validate()
        {
            return true;
        }

        public override void Invoke(float deltaTime)
        {
        }

        public override void DefaultComplete()
        {
        }
    }
}