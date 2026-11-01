namespace Game.Core
{
    /// <summary>Props break (pop or shatter) the first time they touch the floor (spec §4.3).</summary>
    public static class BreakRule
    {
        public static bool ShouldBreak(BreakMode mode, bool isBroken, bool touchedFloor) =>
            mode != BreakMode.None && !isBroken && touchedFloor;
    }
}
