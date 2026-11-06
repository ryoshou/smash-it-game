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

        /// <summary>Contacts steeper than this (|normal.y| below it) are a side face, not something to land on.</summary>
        public const float MinLandingNormalY = 0.7f;

        /// <summary>
        /// Whether a floor contact counts as landing. Food only splats on something it lands on (Kitchen spec §6), so a
        /// glancing hit on a counter's side lets it fall on; everything else breaks on any floor contact.
        /// </summary>
        public static bool Lands(BreakMode mode, float contactNormalY) =>
            mode != BreakMode.Splash || System.Math.Abs(contactNormalY) >= MinLandingNormalY;

        /// <summary>Whether the prop swaps to its pre-cut pieces (a debris set) when it breaks.</summary>
        public static bool HasPieces(BreakMode mode) => mode == BreakMode.Shatter || mode == BreakMode.Splash;
    }
}
