using UnityEngine;

namespace Game.Juice
{
    /// <summary>One vibration: its length (Android) and strength 0–1 (Android amplitude, iOS intensity).</summary>
    public readonly struct HapticPulse
    {
        private const int MaxAndroidAmplitude = 255;

        public readonly int Milliseconds;
        public readonly float Amount01;

        public HapticPulse(int milliseconds, float amount01)
        {
            Milliseconds = milliseconds;
            Amount01 = amount01;
        }

        public bool IsNone => Milliseconds <= 0 || !(Amount01 > 0f);

        /// <summary>Android's 1–255 amplitude (0 for no pulse).</summary>
        public int AndroidAmplitude =>
            IsNone ? 0 : Mathf.Clamp(Mathf.RoundToInt(Amount01 * MaxAndroidAmplitude), 1, MaxAndroidAmplitude);
    }
}
