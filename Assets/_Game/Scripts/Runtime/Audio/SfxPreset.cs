using System;
using UnityEngine;

namespace Game.Audio
{
    /// <summary>One sound: its sfxr parameters and seed, its playback volume, and an optional clip that replaces the synth.</summary>
    [Serializable]
    public sealed class SfxPreset
    {
        public SfxId id;
        [Tooltip("Seeds the noise, so the sound is the same every run.")]
        public int seed = 1;
        [Tooltip("Playback volume at layer intensity 1 (× intensity, capped at 1).")]
        [Range(0f, 1f)] public float volume = 0.5f;
        [Tooltip("Plays this clip instead of the synth (e.g. from a bought sound pack).")]
        public AudioClip clipOverride;
        public SfxrParams synth = new SfxrParams();
    }
}
