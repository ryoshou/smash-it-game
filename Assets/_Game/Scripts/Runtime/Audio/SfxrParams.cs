using System;
using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// One sound's sfxr sliders (DrPetter's sfxr, the same names and ranges). Defaults are sfxr's reset values. Sliders
    /// marked -1..1 slide or sweep down when negative.
    /// </summary>
    [Serializable]
    public sealed class SfxrParams
    {
        public SfxrWave wave = SfxrWave.Square;

        [Header("Frequency")]
        [Range(0f, 1f)] public float baseFrequency = 0.3f;
        [Tooltip("The sound stops when a downward slide reaches this frequency (0 = never).")]
        [Range(0f, 1f)] public float frequencyLimit;
        [Range(-1f, 1f)] public float frequencySlide;
        [Range(-1f, 1f)] public float frequencyDeltaSlide;
        [Range(0f, 1f)] public float vibratoDepth;
        [Range(0f, 1f)] public float vibratoSpeed;
        [Tooltip("Frequency jump once, partway in (positive = up).")]
        [Range(-1f, 1f)] public float arpeggioMod;
        [Range(0f, 1f)] public float arpeggioSpeed;
        [Tooltip("Restarts the frequency slide and arpeggio this often (0 = never).")]
        [Range(0f, 1f)] public float repeatSpeed;

        [Header("Square wave")]
        [Range(0f, 1f)] public float squareDuty;
        [Range(-1f, 1f)] public float dutySweep;

        [Header("Envelope")]
        [Range(0f, 1f)] public float attack;
        [Range(0f, 1f)] public float sustain = 0.3f;
        [Range(0f, 1f)] public float sustainPunch;
        [Range(0f, 1f)] public float decay = 0.4f;

        [Header("Phaser")]
        [Range(-1f, 1f)] public float phaserOffset;
        [Range(-1f, 1f)] public float phaserSweep;

        [Header("Filters")]
        [Tooltip("1 = low-pass off.")]
        [Range(0f, 1f)] public float lowPassCutoff = 1f;
        [Range(-1f, 1f)] public float lowPassSweep;
        [Range(0f, 1f)] public float lowPassResonance;
        [Range(0f, 1f)] public float highPassCutoff;
        [Range(-1f, 1f)] public float highPassSweep;
    }
}
