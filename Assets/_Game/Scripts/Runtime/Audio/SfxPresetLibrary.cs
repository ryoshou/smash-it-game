namespace Game.Audio
{
    /// <summary>The starting sound design: one sfxr preset per SfxId. A new SfxPresets asset starts with these.</summary>
    public static class SfxPresetLibrary
    {
        public static SfxPreset[] Defaults() => new[]
        {
            Preset(SfxId.Pull, 11, 0.2f, new SfxrParams
            {
                wave = SfxrWave.Square, baseFrequency = 0.35f, squareDuty = 0.3f, sustain = 0.02f, decay = 0.05f,
                highPassCutoff = 0.1f,
            }),
            Preset(SfxId.Twang, 23, 0.45f, new SfxrParams
            {
                wave = SfxrWave.Sawtooth, baseFrequency = 0.28f, frequencySlide = 0.2f, sustain = 0.04f,
                sustainPunch = 0.4f, decay = 0.22f, lowPassCutoff = 0.6f,
            }),
            Preset(SfxId.HitSoft, 31, 0.35f, new SfxrParams
            {
                wave = SfxrWave.Noise, baseFrequency = 0.45f, frequencySlide = -0.3f, sustain = 0.02f, decay = 0.08f,
                lowPassCutoff = 0.5f,
            }),
            Preset(SfxId.HitHard, 37, 0.5f, new SfxrParams
            {
                wave = SfxrWave.Noise, baseFrequency = 0.3f, frequencySlide = -0.4f, sustain = 0.05f,
                sustainPunch = 0.5f, decay = 0.18f, lowPassCutoff = 0.45f,
            }),
            Preset(SfxId.Cleared, 43, 0.3f, new SfxrParams
            {
                wave = SfxrWave.Sine, baseFrequency = 0.6f, frequencySlide = 0.25f, sustain = 0.03f, decay = 0.1f,
            }),
            Preset(SfxId.Win, 83, 0.55f, new SfxrParams
            {
                wave = SfxrWave.Sine, baseFrequency = 0.55f, sustain = 0.12f, sustainPunch = 0.35f, decay = 0.5f,
                arpeggioMod = 0.5f, arpeggioSpeed = 0.6f,
            }),
            Preset(SfxId.Lose, 61, 0.4f, new SfxrParams
            {
                wave = SfxrWave.Square, baseFrequency = 0.4f, frequencySlide = -0.25f, squareDuty = 0.4f,
                sustain = 0.1f, decay = 0.3f,
            }),
            Preset(SfxId.Ui, 97, 0.3f, new SfxrParams
            {
                wave = SfxrWave.Square, baseFrequency = 0.62f, squareDuty = 0.2f, sustain = 0.02f, decay = 0.05f,
                highPassCutoff = 0.15f,
            }),
        };

        private static SfxPreset Preset(SfxId id, int seed, float volume, SfxrParams synth) =>
            new SfxPreset { id = id, seed = seed, volume = volume, synth = synth };
    }
}
