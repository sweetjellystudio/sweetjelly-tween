namespace SWeetJelly.Tween.Domain
{
    public static class EasingMath
    {
        public static float Clamp01(float value)
        {
            if (value < 0f)
                return 0f;

            if (value > 1f)
                return 1f;

            return value;
        }
    }

}