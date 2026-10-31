using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Audio
{
    /// <summary>
    /// One AudioClip and volume per SfxId, built once from the presets: the preset's clipOverride when set, else the
    /// synthesised preset. Sounds without a preset stay silent (null clip) and are reported to the caller.
    /// </summary>
    public sealed class SfxBank
    {
        public static readonly int Count = Enum.GetValues(typeof(SfxId)).Length;

        private readonly AudioClip[] clips = new AudioClip[Count];
        private readonly bool[] isOwned = new bool[Count];
        private readonly float[] volumes = new float[Count];

        public AudioClip Clip(SfxId id) => clips[(int)id];

        public float Volume(SfxId id) => volumes[(int)id];

        /// <param name="missing">Receives every SfxId with no preset (cleared first).</param>
        public static SfxBank Build(IReadOnlyList<SfxPreset> presets, List<SfxId> missing)
        {
            if (presets == null) throw new ArgumentNullException(nameof(presets));
            if (missing == null) throw new ArgumentNullException(nameof(missing));
            var bank = new SfxBank();
            for (int i = 0; i < presets.Count; i++)
            {
                SfxPreset preset = presets[i];
                int index = preset == null ? -1 : (int)preset.id;
                if (index < 0 || index >= Count || bank.clips[index] != null) continue;
                bank.volumes[index] = Mathf.Clamp01(preset.volume);
                if (preset.clipOverride != null)
                {
                    bank.clips[index] = preset.clipOverride;
                    continue;
                }
                bank.clips[index] = Synthesize(preset);
                bank.isOwned[index] = true;
            }
            missing.Clear();
            for (int i = 0; i < Count; i++)
                if (bank.clips[i] == null) missing.Add((SfxId)i);
            return bank;
        }

        /// <summary>Destroys the clips this bank synthesised. Override clips belong to the project and are left alone.</summary>
        public void Destroy()
        {
            for (int i = 0; i < Count; i++)
            {
                if (isOwned[i] && clips[i] != null)
                {
                    if (Application.isPlaying) Object.Destroy(clips[i]);
                    else Object.DestroyImmediate(clips[i]);
                }
                clips[i] = null;
                isOwned[i] = false;
            }
        }

        private static AudioClip Synthesize(SfxPreset preset)
        {
            float[] samples = SfxrSynth.Render(preset.synth ?? new SfxrParams(), preset.seed);
            var clip = AudioClip.Create($"Sfx_{preset.id}", samples.Length, 1, SfxrSynth.SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
