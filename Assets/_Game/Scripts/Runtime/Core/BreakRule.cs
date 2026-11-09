namespace Game.Core
{
    /// <summary>
    /// Props break (pop, shatter or splash) the first time they touch the floor (spec §4.3); soft food splashes the
    /// moment it is touched. A Burst prop leaves it to its fuse, which the floor arms.
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

        /// <summary>
        /// Whether a contact breaks soft food that splashes on touch (the owner's sandwich, tomato and cupcake): the cat's
        /// ball does at any speed, anything else must hit it at least minSpeed, so neighbours resting against it don't.
        /// </summary>
        public static bool TouchBreaks(bool splashesOnTouch, bool byBall, float relativeSpeed, float minSpeed) =>
            splashesOnTouch && (byBall || relativeSpeed >= minSpeed);

        /// <summary>Whether the prop swaps to its pre-cut pieces (a debris set) when it breaks: glass and china only, food squashes whole.</summary>
        public static bool HasPieces(BreakMode mode) => mode == BreakMode.Shatter;
    }
}
