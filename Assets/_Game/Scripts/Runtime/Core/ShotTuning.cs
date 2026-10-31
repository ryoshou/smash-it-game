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

        [Header("Continue")]
        public int continueShots = 3;

        public static ShotTuning CreateDefault() => CreateInstance<ShotTuning>();

        public ShotTuning Clone() => Instantiate(this);
    }
}
