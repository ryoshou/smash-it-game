using UnityEngine;

namespace Game.Core
{
    /// <summary>Extra punch on top of PhysX's own collision response, plus speed caps that keep piles from exploding.</summary>
    public static class ImpactMath
    {
        private const float CenterEpsilon = 1e-4f;

        /// <summary>The extra impulse that makes a hit feel like <paramref name="multiplier"/>× the ball's momentum.</summary>
        public static Vector3 BoostImpulse(Vector3 ballVelocityBeforeHit, float ballMass, float multiplier) =>
            multiplier <= 1f ? Vector3.zero : ballVelocityBeforeHit * (ballMass * (multiplier - 1f));

        /// <summary>Outward push that falls off linearly to zero at <paramref name="radius"/>.</summary>
        public static Vector3 BurstImpulse(Vector3 center, Vector3 target, float strength, float radius)
        {
            var offset = target - center;
            float dist = offset.magnitude;
            if (dist >= radius) return Vector3.zero;
            var dir = dist < CenterEpsilon ? Vector3.up : offset / dist;
            return dir * (strength * (1f - dist / radius));
        }

        public static Vector3 CapMagnitude(Vector3 v, float max) => Vector3.ClampMagnitude(v, max);
    }
}
