using UnityEngine;

namespace Game.Core
{
    /// <summary>Pull to launch velocity: pull further = faster and higher; drag left aims right.</summary>
    public static class SlingshotMath
    {
        /// <param name="pull">(finger − pouch) / screen height; a downward drag has negative y.</param>
        public static LaunchResult Compute(Vector2 pull, ShotTuning t)
        {
            float magnitude = pull.magnitude;
            if (pull.y >= 0f || magnitude < t.minPull) return LaunchResult.None;

            float length = Mathf.Min(magnitude, t.maxPull);
            float power = Mathf.Clamp01((length - t.minPull) / (t.maxPull - t.minPull));
            float yawDeg = Mathf.Clamp(Mathf.Atan2(-pull.x, -pull.y) * Mathf.Rad2Deg, -t.maxYawDeg, t.maxYawDeg);
            float speed = Mathf.Lerp(t.minSpeed, t.maxSpeed, power);
            float elevation = Mathf.Lerp(t.minElevationDeg, t.maxElevationDeg, power) * Mathf.Deg2Rad;
            float yaw = yawDeg * Mathf.Deg2Rad;

            var direction = new Vector3(
                Mathf.Cos(elevation) * Mathf.Sin(yaw),
                Mathf.Sin(elevation),
                Mathf.Cos(elevation) * Mathf.Cos(yaw));
            return new LaunchResult(true, speed * direction, power);
        }
    }
}
