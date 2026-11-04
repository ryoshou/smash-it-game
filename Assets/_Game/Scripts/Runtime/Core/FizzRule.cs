using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// The fizzy soda (spec §6.2): knocked over or dropped, it arms, and bursts once its short fuse has burned. A can
    /// that only wobbles never arms, so spawning and settling are safe.
    /// </summary>
    public static class FizzRule
    {
        public static bool ShouldArm(Vector3 up, bool touchedFloor, float tiltDegrees) =>
            touchedFloor || Vector3.Angle(up, Vector3.up) >= tiltDegrees;

        public static bool FuseDone(float armedAt, float now, float fuseSeconds) => now - armedAt >= fuseSeconds;
    }
}
