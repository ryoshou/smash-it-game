namespace Game.Core
{
    /// <summary>
    /// A fixed number of debris slots. Acquiring when all are busy recycles the oldest active one, so a whole stack
    /// breaking at once never grows the pool.
    /// </summary>
    public sealed class DebrisBudget
    {
        public const int None = -1;

        private readonly bool[] active;
        private readonly long[] acquiredAt;
        private long clock;

        public DebrisBudget(int capacity)
        {
            active = new bool[System.Math.Max(1, capacity)];
            acquiredAt = new long[active.Length];
        }

        public int Capacity => active.Length;
        public int Active { get; private set; }

        /// <param name="evicted">The slot that was still active and is being recycled, or <see cref="None"/>.</param>
        public int Acquire(out int evicted)
        {
            evicted = None;
            int slot = FirstFree();
            if (slot == None)
            {
                slot = Oldest();
                evicted = slot;
            }
            else
            {
                active[slot] = true;
                Active++;
            }
            acquiredAt[slot] = clock++;
            return slot;
        }

        public void Release(int slot)
        {
            if (slot < 0 || slot >= active.Length || !active[slot]) return;
            active[slot] = false;
            Active--;
        }

        public void ReleaseAll()
        {
            for (int i = 0; i < active.Length; i++) active[i] = false;
            Active = 0;
        }

        private int FirstFree()
        {
            for (int i = 0; i < active.Length; i++)
                if (!active[i]) return i;
            return None;
        }

        private int Oldest()
        {
            int oldest = 0;
            for (int i = 1; i < active.Length; i++)
                if (acquiredAt[i] < acquiredAt[oldest]) oldest = i;
            return oldest;
        }
    }
}
