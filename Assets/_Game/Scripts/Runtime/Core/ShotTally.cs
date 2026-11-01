namespace Game.Core
{
    /// <summary>
    /// Counts targets cleared by a volley: from a shot being fired until no ball is in play AND the desk has been quiet
    /// for the hold time (capped by the max wait, for piles that never settle), so a slow topple still counts for the
    /// shot that caused it (spec §4.3). A shot fired while an earlier ball still flies joins the same volley.
    /// </summary>
    public sealed class ShotTally
    {
        private readonly float holdSeconds;
        private readonly float maxWaitSeconds;
        private bool isPending;
        private int cleared;
        private float? waitStartedAt;
        private float? quietSince;

        public ShotTally(float holdSeconds, float maxWaitSeconds)
        {
            this.holdSeconds = holdSeconds;
            this.maxWaitSeconds = maxWaitSeconds;
        }

        public void Fired() => isPending = true;

        public void TargetCleared()
        {
            if (isPending) cleared++;
        }

        /// <returns>True once per volley, when it is over; then <paramref name="clearedThisShot"/> is its count.</returns>
        public bool TryResolve(float now, bool anyBallActive, bool anyMoving, out int clearedThisShot)
        {
            clearedThisShot = cleared;
            if (!isPending) return false;
            if (anyBallActive)
            {
                waitStartedAt = null;
                quietSince = null;
                return false;
            }

            waitStartedAt ??= now;
            if (anyMoving) quietSince = null;
            else quietSince ??= now;
            bool settled = quietSince.HasValue && now - quietSince.Value >= holdSeconds;
            bool timedOut = now - waitStartedAt.Value >= maxWaitSeconds;
            if (!settled && !timedOut) return false;
            Reset();
            return true;
        }

        public void Reset()
        {
            isPending = false;
            cleared = 0;
            waitStartedAt = null;
            quietSince = null;
        }
    }
}
