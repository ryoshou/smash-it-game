using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Food squashes flat instead of breaking (the owner's call: food is not glass or china). Scale multipliers over its
    /// life, x across both flat axes and y up: it flattens over squashSeconds keeping its volume, holds, then shrinks away
    /// over fadeSeconds.
    /// </summary>
    public static class SquashMath
    {
        public static Vector2 Scale(float age, float flatten, float squashSeconds, float holdSeconds, float fadeSeconds)
        {
            float squashed = squashSeconds > 0f ? Mathf.Clamp01(age / squashSeconds) : 1f;
            float height = Mathf.Lerp(1f, flatten, squashed);
            var scale = new Vector2(1f / Mathf.Sqrt(height), height);
            float fadeAge = age - squashSeconds - holdSeconds;
            if (fadeAge <= 0f) return scale;
            float left = fadeSeconds > 0f ? 1f - Mathf.Clamp01(fadeAge / fadeSeconds) : 0f;
            return scale * left;
        }

        public static bool IsDone(float age, float squashSeconds, float holdSeconds, float fadeSeconds) =>
            age >= squashSeconds + holdSeconds + fadeSeconds;
    }
}
