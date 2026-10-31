namespace Game.Core
{
    /// <summary>Shots left in this attempt, plus the one rewarded continue each attempt allows.</summary>
    public sealed class ShotBudget
    {
        public ShotBudget(int shots) => ResetAttempt(shots);

        public int Remaining { get; private set; }

        /// <summary>True until a continue is used in this attempt.</summary>
        public bool CanContinue { get; private set; }

        public bool TryConsume()
        {
            if (Remaining <= 0) return false;
            Remaining--;
            return true;
        }

        public bool TryContinue(int extraShots)
        {
            if (!CanContinue) return false;
            CanContinue = false;
            Remaining += extraShots;
            return true;
        }

        public void ResetAttempt(int shots)
        {
            Remaining = shots;
            CanContinue = true;
        }
    }
}
