namespace Game.Core
{
    /// <summary>
    /// Props break (pop or shatter) the first time they touch the floor (spec §4.3). A Burst prop leaves it to its
    /// fuse, which the floor arms.
    /// </summary>
    public static class BreakRule
    {
        public static bool ShouldBreak(BreakMode mode, bool isBroken, bool touchedFloor) =>
            mode != BreakMode.None && mode != BreakMode.Burst && !isBroken && touchedFloor;
    }
}
