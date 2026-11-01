using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// A few clips for one kind of sound, played round-robin (no randomness: the same hits give the same sounds).
    /// Harder hits play louder and slightly lower.
    /// </summary>
    [CreateAssetMenu(menuName = "Smash/Sound Set")]
    public sealed class SoundSet : ScriptableObject
    {
        public const float SoftVolumeShare = 0.4f;

        public AudioClip[] clips = new AudioClip[0];
        [Range(0f, 1f)] public float volume = 0.8f;
        public float pitchAtSoft = 1.05f;
        public float pitchAtHard = 0.9f;
        [Tooltip("Impact speed (m/s) treated as the softest hit.")]
        public float softSpeed = 0.5f;
        [Tooltip("Impact speed (m/s) treated as the hardest hit.")]
        public float hardSpeed = 4f;

        private int next;

        public AudioClip Next()
        {
            if (clips == null || clips.Length == 0) return null;
            var clip = clips[next % clips.Length];
            next = (next + 1) % clips.Length;
            return clip;
        }

        public float Pitch(float speed) => Mathf.Lerp(pitchAtSoft, pitchAtHard, Hardness(speed));

        public float Volume(float speed) => volume * Mathf.Lerp(SoftVolumeShare, 1f, Hardness(speed));

        private float Hardness(float speed) => Mathf.InverseLerp(softSpeed, hardSpeed, speed);
    }
}
