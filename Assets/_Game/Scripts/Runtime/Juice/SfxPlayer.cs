using System.Collections.Generic;
using Game.Audio;
using UnityEngine;

namespace Game.Juice
{
    /// <summary>
    /// Plays sound effects on a fixed pool of 2D AudioSources. Clips are synthesised once from SfxPresets.
    /// SfxVoiceLimiter caps copies of one sound and picks the voice. Runs on real time (audio ignores timeScale).
    /// Ported from first-hyper-casual without its juice-layer settings.
    /// </summary>
    public sealed class SfxPlayer : MonoBehaviour
    {
        private const float MinPitch = 0.25f; // engine safety clamps, not tuning
        private const float MaxPitch = 3f;

        [SerializeField] private JuiceConfig config;
        [SerializeField] private SfxPresets presets;

        private readonly List<SfxId> missing = new List<SfxId>();
        private SfxBank bank;
        private AudioSource[] sources;
        private SfxVoiceLimiter limiter;

        private void Awake()
        {
            if (config == null || presets == null)
            {
                Debug.LogError($"{nameof(SfxPlayer)} needs a JuiceConfig and SfxPresets assigned; sounds are off.", this);
                return;
            }

            int voices = Mathf.Max(1, config.sfxVoices);
            sources = new AudioSource[voices];
            for (int i = 0; i < voices; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 0f;
                sources[i] = source;
            }
            limiter = new SfxVoiceLimiter(voices);
            BuildBank();
        }

        private void OnDestroy() => bank?.Destroy();

        /// <returns>False when nothing plays: no clip for the sound, or its copies are capped.</returns>
        public bool Play(SfxId id, int maxCopies, float pitch = 1f)
        {
            if (bank == null) return false;
            AudioClip clip = bank.Clip(id);
            if (clip == null) return false;
            float safePitch = Mathf.Clamp(pitch, MinPitch, MaxPitch);
            int voice = limiter.Acquire((int)id, Time.unscaledTime, clip.length / safePitch, maxCopies);
            if (voice < 0) return false;
            AudioSource source = sources[voice];
            source.Stop();
            source.clip = clip;
            source.pitch = safePitch;
            source.volume = bank.Volume(id);
            source.Play();
            return true;
        }

        public int PlayingCopies(SfxId id) => limiter == null ? 0 : limiter.CountPlaying((int)id, Time.unscaledTime);

        /// <summary>For tuning in Play mode: edit SfxPresets, then rebuild to hear the change.</summary>
        [ContextMenu("Rebuild Clips")]
        public void RebuildClips()
        {
            if (sources == null) return;
            foreach (AudioSource source in sources) source.Stop();
            limiter.Clear();
            bank?.Destroy();
            BuildBank();
        }

        private void BuildBank()
        {
            bank = SfxBank.Build(presets.Presets, missing);
            if (missing.Count > 0)
                Debug.LogError($"{nameof(SfxPlayer)}: SfxPresets has no preset for {string.Join(", ", missing)}; those sounds stay silent.", this);
        }
    }
}
