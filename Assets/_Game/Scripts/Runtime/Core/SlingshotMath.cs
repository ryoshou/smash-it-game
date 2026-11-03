using UnityEngine;

namespace Game.Core
{
    /// <summary>Pull to launch velocity: pull further = faster and higher; drag left aims right.</summary>
    public static class SlingshotMath
    {
        /// <param name="pull">(finger − touch-down point) / screen height; a downward drag has negative y.</param>
        public static LaunchResult Compute(Vector2 pull, ShotTuning t)
        {
            float magnitude = pull.magnitude;
            if (pull.y >= 0f || magnitude < t.minPull) return LaunchResult.None;

            float dragAngleDeg = Mathf.Atan2(-pull.x, -pull.y) * Mathf.Rad2Deg;
            if (Mathf.Abs(dragAngleDeg) > t.maxDragAngleDeg) return LaunchResult.None;

            float length = Mathf.Min(magnitude, t.maxPull);
            float power = Mathf.Clamp01((length - t.minPull) / (t.maxPull - t.minPull));
            float yawDeg = Mathf.Clamp(dragAngleDeg, -t.maxYawDeg, t.maxYawDeg);
            float speed = Mathf.Lerp(t.minSpeed, t.maxSpeed, power);
            float elevation = Mathf.Lerp(t.minElevationDeg, t.maxElevationDeg, power) * Mathf.Deg2Rad;
            float yaw = yawDeg * Mathf.Deg2Rad;

            var direction = new Vector3(
                Mathf.Cos(elevation) * Mathf.Sin(yaw),
                Mathf.Sin(elevation),
                Mathf.Cos(elevation) * Mathf.Cos(yaw));
            return new LaunchResult(true, speed * direction, power);
        }

        /// <summary>The inverse of Compute's aim: the pull that launches at this yaw and power (bots and tests).</summary>
        /// <param name="yawRadians">Clamped to ±maxYawDeg; positive aims right.</param>
        public static Vector2 PullFor(float yawRadians, float power01, ShotTuning t)
        {
            float maxYaw = t.maxYawDeg * Mathf.Deg2Rad;
            float yaw = Mathf.Clamp(yawRadians, -maxYaw, maxYaw);
            float length = Mathf.Lerp(t.minPull, t.maxPull, Mathf.Clamp01(power01));
            return new Vector2(-Mathf.Sin(yaw), -Mathf.Cos(yaw)) * length;
        }
    }
}
