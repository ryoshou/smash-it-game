namespace Game.Core
{
    /// <summary>
    /// Win as soon as every target is cleared. Out of shots with no ball in flight, lose once the desk has been quiet
    /// for the hold time, or once the wait hits its cap (for piles that never fully settle).
    /// </summary>
    public sealed class RoundJudge
    {
        private readonly float holdSeconds;
        private readonly float maxWaitSeconds;
        private float? waitStartedAt;
        private float? quietSince;

        public RoundJudge(float holdSeconds, float maxWaitSeconds)
        {
            this.holdSeconds = holdSeconds;
            this.maxWaitSeconds = maxWaitSeconds;
        }

        public void Reset()
        {
            waitStartedAt = null;
            quietSince = null;
        }

        public RoundState Evaluate(float now, int targetsRemaining, int shotsRemaining, bool ballActive, bool anyMoving)
        {
            if (targetsRemaining == 0) return RoundState.Won;
            if (shotsRemaining > 0 || ballActive)
            {
                Reset();
                return RoundState.Playing;
            }

            waitStartedAt ??= now;
            if (anyMoving) quietSince = null;
            else quietSince ??= now;

            bool settled = quietSince.HasValue && now - quietSince.Value >= holdSeconds;
            bool timedOut = now - waitStartedAt.Value >= maxWaitSeconds;
            return settled || timedOut ? RoundState.Lost : RoundState.Playing;
        }
    }
}
