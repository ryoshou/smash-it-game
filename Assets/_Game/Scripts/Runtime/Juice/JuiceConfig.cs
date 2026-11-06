using UnityEngine;

namespace Game.Juice
{
    /// <summary>Feedback tuning (sound, shake, haptics, slingshot visuals). Gameplay never reads this. Tune by playing.</summary>
    [CreateAssetMenu(menuName = "Smash/Juice Config")]
    public sealed class JuiceConfig : ScriptableObject
    {
        [Header("Audio")]
        [Min(1)] public int sfxVoices = 8;
        [Min(1)] public int maxHitCopies = 4;
        [Min(1)] public int maxOtherCopies = 2;
        [Tooltip("Impact speed (m/s) at or above which a hit uses the hard sound.")]
        [Min(0f)] public float hardHitSpeed = 3f;
        [Tooltip("Real seconds between impact sounds.")]
        [Min(0f)] public float impactMinInterval = 0.03f;

        [Header("Music")]
        [Range(0f, 1f)] public float musicVolume = 0.35f;
        [Min(0f)] public float musicFadeSeconds = 1.5f;

        [Header("Pull tension")]
        [Tooltip("A tick plays each time pull power crosses a multiple of this.")]
        [Range(0.05f, 1f)] public float pullTickStep = 0.2f;
        public float pullPitchMin = 0.9f;
        public float pullPitchMax = 1.5f;

        [Header("Camera shake")]
        [Tooltip("Impact speed (m/s) above which the camera kicks.")]
        [Min(0f)] public float shakeSpeed = 5f;
        [Min(0f)] public float shakePerSpeed = 0.02f;
        public float shakeMax = 0.15f;
        public float shakeDecaySeconds = 0.25f;

        [Header("Haptics (milliseconds, strength 0–1)")]
        [Min(0)] public int firedHapticMilliseconds = 10;
        [Range(0f, 1f)] public float firedHapticAmount = 0.35f;
        [Min(0)] public int firstContactHapticMilliseconds = 20;
        [Range(0f, 1f)] public float firstContactHapticAmount = 0.6f;
        [Min(0)] public int winHapticMilliseconds = 40;
        [Range(0f, 1f)] public float winHapticAmount = 0.9f;

        [Min(0)] public int shatterHapticMilliseconds = 12;
        [Range(0f, 1f)] public float shatterHapticAmount = 0.4f;

        [Header("Break effects")]
        [Tooltip("Pooled copies of each break effect.")]
        [Min(1)] public int vfxPerKind = 3;
        [Tooltip("Pooled copies of each splash effect (Kitchen spec §6): its splat lingers, and feasts land many at once.")]
        [Min(1)] public int splashVfxPerKind = 8;
        [Tooltip("Metres above the contact point a splat lies: clear of the floor and of the rugs on it (RoomKit.RugThickness).")]
        [Min(0f)] public float splatLift = 0.015f;

        [Header("Slingshot view")]
        [Tooltip("World metres the pouch moves per screen height of pull.")]
        public float pouchTravel = 2f;

        public HapticPulse FiredHaptic => new HapticPulse(firedHapticMilliseconds, firedHapticAmount);
        public HapticPulse FirstContactHaptic => new HapticPulse(firstContactHapticMilliseconds, firstContactHapticAmount);
        public HapticPulse WinHaptic => new HapticPulse(winHapticMilliseconds, winHapticAmount);
        public HapticPulse ShatterHaptic => new HapticPulse(shatterHapticMilliseconds, shatterHapticAmount);
    }
}
