namespace Game.Core
{
    /// <summary>
    /// Props break (pop, shatter or splash) the first time they touch the floor (spec §4.3). A Burst prop leaves it to its
    /// fuse, which the floor arms.
    /// </summary>
    public static class BreakRule
    {
        public static bool ShouldBreak(BreakMode mode, bool isBroken, bool touchedFloor) =>
            mode != BreakMode.None && mode != BreakMode.Burst && !isBroken && touchedFloor;

        /// <summary>Whether the prop swaps to its pre-cut pieces (a debris set) when it breaks.</summary>
        public static bool HasPieces(BreakMode mode) => mode == BreakMode.Shatter || mode == BreakMode.Splash;
    }
}
