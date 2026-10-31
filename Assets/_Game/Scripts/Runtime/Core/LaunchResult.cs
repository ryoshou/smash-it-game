using UnityEngine;

namespace Game.Core
{
    /// <summary>What a released pull does: fire with a velocity, or cancel.</summary>
    public readonly struct LaunchResult
    {
        public readonly bool Fired;
        public readonly Vector3 Velocity;
        public readonly float Power01;

        public LaunchResult(bool fired, Vector3 velocity, float power01)
        {
            Fired = fired;
            Velocity = velocity;
            Power01 = power01;
        }

        public static LaunchResult None => default;
    }
}
