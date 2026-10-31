using UnityEngine;

namespace Game.Core
{
    /// <summary>Aim-preview dots integrated exactly like PhysX (semi-implicit Euler), so the preview equals the flight.</summary>
    public static class TrajectoryMath
    {
        public static int StepsPerDot(float previewSeconds, int dots, float fixedDt) =>
            Mathf.Max(1, Mathf.RoundToInt(previewSeconds / dots / fixedDt));

        /// <summary>Fills every slot of <paramref name="dots"/>; the caller owns the buffer.</summary>
        public static void Sample(Vector3 origin, Vector3 velocity, Vector3 gravity, float fixedDt, int stepsPerDot,
            Vector3[] dots)
        {
            var p = origin;
            var v = velocity;
            for (int i = 0; i < dots.Length; i++)
            {
                for (int s = 0; s < stepsPerDot; s++)
                {
                    v += gravity * fixedDt;
                    p += v * fixedDt;
                }
                dots[i] = p;
            }
        }
    }
}
