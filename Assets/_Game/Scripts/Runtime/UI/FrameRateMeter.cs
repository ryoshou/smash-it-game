namespace Game.UI
{
    /// <summary>Average frame rate over the last N frames (a ring of frame times; no allocation per frame).</summary>
    public sealed class FrameRateMeter
    {
        private readonly float[] frameTimes;
        private int next;
        private int count;
        private float sum;

        public FrameRateMeter(int window) => frameTimes = new float[System.Math.Max(1, window)];

        public float FramesPerSecond => sum > 0f ? count / sum : 0f;

        public void Add(float deltaTime)
        {
            if (count == frameTimes.Length) sum -= frameTimes[next];
            else count++;
            frameTimes[next] = deltaTime;
            sum += deltaTime;
            next = (next + 1) % frameTimes.Length;
        }
    }
}
