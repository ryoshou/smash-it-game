namespace Game.Core
{
    /// <summary>Proud on a clear, annoyed on a miss or a loss (spec §5). A win always beats a miss.</summary>
    public static class CatReactionRule
    {
        public static CatReaction For(int clearedThisShot, RoundState state)
        {
            if (state == RoundState.Won) return CatReaction.Celebrate;
            if (state == RoundState.Lost) return CatReaction.Annoyed;
            return clearedThisShot == 0 ? CatReaction.Annoyed : CatReaction.None;
        }
    }
}
