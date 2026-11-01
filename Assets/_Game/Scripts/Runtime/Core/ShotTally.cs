namespace Game.Core
{
    /// <summary>
    /// Counts targets cleared by a volley: from a shot being fired until no ball is in play. A shot fired while an
    /// earlier ball still flies joins the same volley.
    /// </summary>
    public sealed class ShotTally
    {
        private bool isPending;
        private int cleared;

        public void Fired() => isPending = true;

        public void TargetCleared()
        {
            if (isPending) cleared++;
        }

        /// <returns>True once per volley, when no ball is active any more.</returns>
        public bool TryResolve(bool anyBallActive, out int clearedThisShot)
        {
            clearedThisShot = cleared;
            if (!isPending || anyBallActive) return false;
            Reset();
            return true;
        }

        public void Reset()
        {
            isPending = false;
            cleared = 0;
        }
    }
}
