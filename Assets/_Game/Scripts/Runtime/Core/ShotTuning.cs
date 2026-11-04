using UnityEngine;

namespace Game.Core
{
    /// <summary>Every gameplay number. GameBootstrap clones the asset at runtime so the debug panel never edits it.</summary>
    [CreateAssetMenu(menuName = "Smash/Shot Tuning")]
    public sealed class ShotTuning : ScriptableObject
    {
        [Header("Pull and aim (pull lengths in screen heights)")]
        public float maxPull = 0.25f;
        public float minPull = 0.03f;
        public float grabRadius = 0.08f;
        public float minSpeed = 4f;
        public float maxSpeed = 9f;
        public float minElevationDeg = 8f;
        public float maxElevationDeg = 22f;
        public float maxYawDeg = 25f;
        [Tooltip("Drags further than this from straight down (e.g. almost sideways) cancel instead of firing.")]
        public float maxDragAngleDeg = 80f;
        public float reloadSeconds = 0.5f;
        public float gravityY = -9.81f;

        [Header("Preview")]
        public float previewSeconds = 0.2f;
        public int previewDots = 8;

        [Header("Ball")]
        public float ballRadius = 0.09f;
        public float ballMass = 0.25f;

        [Header("Impact")]
        public float impactSpeedMultiplier = 1.6f;
        public float burstStrength = 0.6f;
        public float burstRadius = 0.35f;
        public float objectMaxLinearSpeed = 6f;
        public float objectMaxAngularSpeed = 20f;

        [Header("Settling and clearing")]
        public float settleLinearThreshold = 0.05f;
        public float settleAngularThreshold = 0.25f;
        public float settleHoldSeconds = 0.6f;
        public float settleMaxWaitSeconds = 5f;
        public float clearMargin = 0.15f;

        [Header("Ball despawn")]
        public float ballLifetimeSeconds = 4f;
        public float ballRestSpeed = 0.15f;
        public float ballRestMinAge = 1f;
        public float ballKillY = -1f;

        [Header("Breaking and debris")]
        [Tooltip("Seconds broken pieces (or a popped prop) stay full size before shrinking away.")]
        public float debrisHoldSeconds = 1.2f;
        public float debrisFadeSeconds = 0.6f;
        [Tooltip("Most shattered sets on screen at once; the oldest is recycled first. Lower on weak devices.")]
        public int maxDebrisSets = 6;
        [Tooltip("Outward impulse (N·s) on each piece when a prop shatters.")]
        public float breakScatterImpulse = 0.6f;
        [Tooltip("Relative speed (m/s) below which prop collisions raise no impact event.")]
        public float propImpactMinSpeed = 0.4f;

        [Header("Continue")]
        public int continueShots = 3;

        [Header("Fizzy soda (spec §6.2)")]
        [Tooltip("Tilt from upright (degrees) that arms the fuse; reaching the floor arms it too.")]
        public float fizzTiltDegrees = 55f;
        public float fizzFuseSeconds = 0.25f;
        public float fizzBurstRadius = 0.7f;
        [Tooltip("Impulse at the burst centre, falling off to zero at the radius.")]
        public float fizzBurstStrength = 2.5f;

        public static ShotTuning CreateDefault() => CreateInstance<ShotTuning>();

        public ShotTuning Clone() => Instantiate(this);
    }
}
