namespace Game.UI
{
    /// <summary>True on the third tap within the window; then starts over.</summary>
    public sealed class TripleTapDetector
    {
        private const int TapsNeeded = 3;

        private readonly float windowSeconds;
        private readonly float[] taps = new float[TapsNeeded];
        private int count;

        public TripleTapDetector(float windowSeconds) => this.windowSeconds = windowSeconds;

        public bool Tap(float now)
        {
            DropTapsOlderThan(now - windowSeconds);
            taps[count++] = now;
            if (count < TapsNeeded) return false;
            count = 0;
            return true;
        }

        private void DropTapsOlderThan(float cutoff)
        {
            int kept = 0;
            for (int i = 0; i < count; i++)
                if (taps[i] >= cutoff) taps[kept++] = taps[i];
            count = kept;
        }
    }
}
