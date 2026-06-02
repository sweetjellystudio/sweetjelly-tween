using UnityEngine;

namespace SWeetJelly.Tween.Framework
{
    internal static class BezierUtility
    {
        public static Vector3 EvaluateQuadratic(
            Vector3 p0,
            Vector3 p1,
            Vector3 p2,
            float t)
        {
            t = Mathf.Clamp01(t);

            Vector3 a = Vector3.LerpUnclamped(p0, p1, t);
            Vector3 b = Vector3.LerpUnclamped(p1, p2, t);

            return Vector3.LerpUnclamped(a, b, t);
        }

        public static Vector3 EvaluateCubic(
            Vector3 p0,
            Vector3 p1,
            Vector3 p2,
            Vector3 p3,
            float t)
        {
            t = Mathf.Clamp01(t);

            Vector3 a = Vector3.LerpUnclamped(p0, p1, t);
            Vector3 b = Vector3.LerpUnclamped(p1, p2, t);
            Vector3 c = Vector3.LerpUnclamped(p2, p3, t);

            Vector3 d = Vector3.LerpUnclamped(a, b, t);
            Vector3 e = Vector3.LerpUnclamped(b, c, t);

            return Vector3.LerpUnclamped(d, e, t);
        }
    }
}