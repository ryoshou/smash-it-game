using System;
using UnityEngine;

namespace Game.Juice
{
    /// <summary>
    /// Decides which pooled voice plays a sound. It caps the copies of one sound (new copies are dropped), uses a free
    /// voice when there is one, and otherwise reuses the busy voice that ends soonest. It tracks end times, so it needs
    /// no per-frame update. Times are real seconds.
    /// </summary>
    public sealed class SfxVoiceLimiter
    {
        private const int NoSound = -1;

        private readonly int[] sounds;
        private readonly float[] endTimes;

        public SfxVoiceLimiter(int voices)
        {
            if (voices < 1) throw new ArgumentOutOfRangeException(nameof(voices), voices, "A limiter needs at least one voice.");
            sounds = new int[voices];
            endTimes = new float[voices];
            Clear();
        }

        public int VoiceCount => sounds.Length;

        /// <returns>The voice to play on, or -1 when 'sound' already has maxCopies copies playing.</returns>
        public int Acquire(int sound, float now, float seconds, int maxCopies)
        {
            if (CountPlaying(sound, now) >= Mathf.Max(1, maxCopies)) return -1;
            int voice = FindFree(now);
            if (voice < 0) voice = FindEndingSoonest();
            sounds[voice] = sound;
            endTimes[voice] = now + Mathf.Max(0f, seconds);
            return voice;
        }

        public int CountPlaying(int sound, float now)
        {
            int count = 0;
            for (int i = 0; i < sounds.Length; i++)
                if (sounds[i] == sound && endTimes[i] > now) count++;
            return count;
        }

        public int CountPlaying(float now)
        {
            int count = 0;
            for (int i = 0; i < endTimes.Length; i++)
                if (endTimes[i] > now) count++;
            return count;
        }

        public void Clear()
        {
            for (int i = 0; i < sounds.Length; i++)
            {
                sounds[i] = NoSound;
                endTimes[i] = float.NegativeInfinity;
            }
        }

        private int FindFree(float now)
        {
            for (int i = 0; i < endTimes.Length; i++)
                if (endTimes[i] <= now) return i;
            return -1;
        }

        private int FindEndingSoonest()
        {
            int soonest = 0;
            for (int i = 1; i < endTimes.Length; i++)
                if (endTimes[i] < endTimes[soonest]) soonest = i;
            return soonest;
        }
    }
}
