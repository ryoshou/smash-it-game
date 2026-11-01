using UnityEngine;

namespace Game.Core
{
    /// <summary>Debris holds full size, then shrinks smoothly to nothing.</summary>
    public static class DebrisFade
    {
        /// <returns>1 while age ≤ hold, then a smoothstep down to 0 at hold + fade.</returns>
        public static float Scale01(float age, float holdSeconds, float fadeSeconds)
        {
            if (age <= holdSeconds) return 1f;
            if (fadeSeconds <= 0f) return 0f;
            float t = Mathf.Clamp01((age - holdSeconds) / fadeSeconds);
            return 1f - t * t * (3f - 2f * t);
        }

        public static bool IsDone(float age, float holdSeconds, float fadeSeconds) =>
            age >= holdSeconds + Mathf.Max(0f, fadeSeconds);
    }
}
