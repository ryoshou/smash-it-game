using System;
using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// Renders SfxrParams to mono samples: a port of DrPetter's sfxr (MIT) with seeded noise, guarded zero-length envelope
    /// stages and peak normalisation instead of a fixed master gain. Pure: the same params and seed give the same samples.
    /// Allocates; call it at startup, never per frame.
    /// </summary>
    public static class SfxrSynth
    {
        public const int SampleRate = 44100;
        public const int Supersampling = 8;
        public const float MaxSeconds = 4f;
        public const int MaxSamples = (int)(SampleRate * MaxSeconds);

        /// <summary>Peak level after normalising: a little headroom below full scale.</summary>
        public const float PeakLevel = 0.9f;

        private const float SilencePeak = 1e-6f;

        public static float[] Render(SfxrParams parameters, int seed)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            var scratch = new float[MaxSamples];
            int length = new Voice(parameters, seed).Fill(scratch);
            var samples = new float[Math.Max(1, length)];
            Array.Copy(scratch, samples, length);
            Normalize(samples);
            return samples;
        }

        private static void Normalize(float[] samples)
        {
            float peak = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                if (float.IsNaN(samples[i]) || float.IsInfinity(samples[i])) samples[i] = 0f;
                peak = Math.Max(peak, Math.Abs(samples[i]));
            }
            if (peak < SilencePeak) return;
            float gain = PeakLevel / peak;
            for (int i = 0; i < samples.Length; i++) samples[i] *= gain;
        }

        /// <summary>One playing sfxr voice: the state of sfxr's ResetSample / SynthSample.</summary>
        private sealed class Voice
        {
            private const int PhaserSize = 1024;
            private const int PhaserMask = PhaserSize - 1;
            private const int NoiseSize = 32;
            private const int MinPeriod = 8;
            private const int EnvelopeStages = 3;

            private readonly SfxrParams p;
            private readonly System.Random random;
            private readonly float[] phaserBuffer = new float[PhaserSize];
            private readonly float[] noiseBuffer = new float[NoiseSize];
            private readonly int[] envelopeLength = new int[EnvelopeStages];

            private bool isPlaying = true;
            private int phase;
            private double period;
            private double maxPeriod;
            private double slide;
            private double deltaSlide;
            private float squareDuty;
            private float squareSlide;
            private double arpeggioMultiplier;
            private int arpeggioTime;
            private int arpeggioLimit;
            private float lowPassPos;
            private float lowPassDelta;
            private float lowPassW;
            private float lowPassWDelta;
            private float lowPassDamping;
            private float highPassPos;
            private float highPassW;
            private float highPassWDelta;
            private float vibratoPhase;
            private float vibratoSpeed;
            private float vibratoAmplitude;
            private float envelopeVolume;
            private int envelopeStage;
            private int envelopeTime;
            private float phaserPhase;
            private float phaserDelta;
            private int phaserOffset;
            private int phaserIndex;
            private int repeatTime;
            private int repeatLimit;

            public Voice(SfxrParams parameters, int seed)
            {
                p = parameters;
                random = new System.Random(seed);
                Reset(false);
            }

            /// <returns>How many samples were written (the sound ends, or the buffer is full).</returns>
            public int Fill(float[] buffer)
            {
                int written = 0;
                while (isPlaying && written < buffer.Length) buffer[written++] = NextSample();
                return written;
            }

            private void Reset(bool restart)
            {
                if (!restart) phase = 0;
                period = 100.0 / (p.baseFrequency * p.baseFrequency + 0.001);
                maxPeriod = 100.0 / (p.frequencyLimit * p.frequencyLimit + 0.001);
                slide = 1.0 - Math.Pow(p.frequencySlide, 3.0) * 0.01;
                deltaSlide = -Math.Pow(p.frequencyDeltaSlide, 3.0) * 0.000001;
                squareDuty = 0.5f - p.squareDuty * 0.5f;
                squareSlide = -p.dutySweep * 0.00005f;
                arpeggioMultiplier = p.arpeggioMod >= 0f
                    ? 1.0 - Math.Pow(p.arpeggioMod, 2.0) * 0.9
                    : 1.0 + Math.Pow(p.arpeggioMod, 2.0) * 10.0;
                arpeggioTime = 0;
                arpeggioLimit = p.arpeggioSpeed >= 1f ? 0 : (int)(Math.Pow(1.0 - p.arpeggioSpeed, 2.0) * 20000 + 32);
                if (restart) return;

                lowPassPos = 0f;
                lowPassDelta = 0f;
                lowPassW = Mathf.Pow(p.lowPassCutoff, 3f) * 0.1f;
                lowPassWDelta = 1f + p.lowPassSweep * 0.0001f;
                lowPassDamping = Mathf.Min(0.8f, 5f / (1f + Mathf.Pow(p.lowPassResonance, 2f) * 20f) * (0.01f + lowPassW));
                highPassPos = 0f;
                highPassW = Mathf.Pow(p.highPassCutoff, 2f) * 0.1f;
                highPassWDelta = 1f + p.highPassSweep * 0.0003f;
                vibratoPhase = 0f;
                vibratoSpeed = Mathf.Pow(p.vibratoSpeed, 2f) * 0.01f;
                vibratoAmplitude = p.vibratoDepth * 0.5f;
                envelopeVolume = 0f;
                envelopeStage = 0;
                envelopeTime = 0;
                envelopeLength[0] = (int)(p.attack * p.attack * 100000f);
                envelopeLength[1] = (int)(p.sustain * p.sustain * 100000f);
                envelopeLength[2] = (int)(p.decay * p.decay * 100000f);
                phaserPhase = p.phaserOffset * Math.Abs(p.phaserOffset) * 1020f;
                phaserDelta = p.phaserSweep * Math.Abs(p.phaserSweep);
                phaserOffset = Math.Min(PhaserMask, Math.Abs((int)phaserPhase));
                phaserIndex = 0;
                Array.Clear(phaserBuffer, 0, PhaserSize);
                RefillNoise();
                repeatTime = 0;
                repeatLimit = p.repeatSpeed <= 0f ? 0 : (int)(Math.Pow(1.0 - p.repeatSpeed, 2.0) * 20000 + 32);
            }

            private float NextSample()
            {
                repeatTime++;
                if (repeatLimit != 0 && repeatTime >= repeatLimit)
                {
                    repeatTime = 0;
                    Reset(true);
                }

                arpeggioTime++;
                if (arpeggioLimit != 0 && arpeggioTime >= arpeggioLimit)
                {
                    arpeggioLimit = 0;
                    period *= arpeggioMultiplier;
                }

                slide += deltaSlide;
                period *= slide;
                if (period > maxPeriod)
                {
                    period = maxPeriod;
                    if (p.frequencyLimit > 0f) isPlaying = false;
                }

                double vibrated = period;
                if (vibratoAmplitude > 0f)
                {
                    vibratoPhase += vibratoSpeed;
                    vibrated = period * (1.0 + Math.Sin(vibratoPhase) * vibratoAmplitude);
                }
                int wavePeriod = Math.Max(MinPeriod, (int)vibrated);

                squareDuty = Mathf.Clamp(squareDuty + squareSlide, 0f, 0.5f);
                StepEnvelope();

                phaserPhase += phaserDelta;
                phaserOffset = Math.Min(PhaserMask, Math.Abs((int)phaserPhase));
                highPassW = Mathf.Clamp(highPassW * highPassWDelta, 0.00001f, 0.1f);

                float sum = 0f;
                for (int s = 0; s < Supersampling; s++) sum += SubSample(wavePeriod);
                return sum / Supersampling;
            }

            private float SubSample(int wavePeriod)
            {
                phase++;
                if (phase >= wavePeriod)
                {
                    phase %= wavePeriod;
                    if (p.wave == SfxrWave.Noise) RefillNoise();
                }

                float fraction = (float)phase / wavePeriod;
                float sample;
                switch (p.wave)
                {
                    case SfxrWave.Square: sample = fraction < squareDuty ? 0.5f : -0.5f; break;
                    case SfxrWave.Sawtooth: sample = 1f - fraction * 2f; break;
                    case SfxrWave.Sine: sample = Mathf.Sin(fraction * 2f * Mathf.PI); break;
                    default: sample = noiseBuffer[phase * NoiseSize / wavePeriod]; break;
                }

                float previous = lowPassPos;
                lowPassW = Mathf.Clamp(lowPassW * lowPassWDelta, 0f, 0.1f);
                if (p.lowPassCutoff < 1f)
                {
                    lowPassDelta += (sample - lowPassPos) * lowPassW;
                    lowPassDelta -= lowPassDelta * lowPassDamping;
                }
                else
                {
                    lowPassPos = sample;
                    lowPassDelta = 0f;
                }
                lowPassPos += lowPassDelta;

                highPassPos += lowPassPos - previous;
                highPassPos -= highPassPos * highPassW;
                sample = highPassPos;

                phaserBuffer[phaserIndex & PhaserMask] = sample;
                sample += phaserBuffer[(phaserIndex - phaserOffset + PhaserSize) & PhaserMask];
                phaserIndex = (phaserIndex + 1) & PhaserMask;

                return sample * envelopeVolume;
            }

            /// <summary>Attack, sustain (with punch), decay; the sound ends after decay. Zero-length stages never divide by zero.</summary>
            private void StepEnvelope()
            {
                envelopeTime++;
                if (envelopeTime > envelopeLength[envelopeStage])
                {
                    envelopeTime = 0;
                    envelopeStage++;
                    if (envelopeStage == EnvelopeStages)
                    {
                        isPlaying = false;
                        envelopeStage = EnvelopeStages - 1;
                        envelopeVolume = 0f;
                        return;
                    }
                }

                float t = (float)envelopeTime / Math.Max(1, envelopeLength[envelopeStage]);
                switch (envelopeStage)
                {
                    case 0: envelopeVolume = t; break;
                    case 1: envelopeVolume = 1f + (1f - t) * 2f * p.sustainPunch; break;
                    default: envelopeVolume = 1f - t; break;
                }
            }

            private void RefillNoise()
            {
                for (int i = 0; i < NoiseSize; i++) noiseBuffer[i] = (float)(random.NextDouble() * 2.0 - 1.0);
            }
        }
    }
}
