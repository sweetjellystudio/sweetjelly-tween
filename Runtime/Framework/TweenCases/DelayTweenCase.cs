namespace SWeetJelly.Tween.Framework
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