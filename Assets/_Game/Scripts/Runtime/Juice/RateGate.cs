namespace Game.Juice
{
    /// <summary>Lets a call through at most once per minInterval (real seconds); blocked calls don't push the next pass back.</summary>
    public sealed class RateGate
    {
        private float lastPass = float.NegativeInfinity;

        public bool TryPass(float now, float minInterval)
        {
            if (now - lastPass < minInterval) return false;
            lastPass = now;
            return true;
        }

        public void Reset() => lastPass = float.NegativeInfinity;
    }
}
