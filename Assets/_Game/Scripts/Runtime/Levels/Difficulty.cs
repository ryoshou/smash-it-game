namespace Game.Levels
{
    /// <summary>
    /// Level difficulty label (spec §6.1: about every 5th level is Hard; Kitchen spec §4: Feast levels are heaped
    /// rewards). Stored as a number: append only.
    /// </summary>
    public enum Difficulty
    {
        Normal,
        Hard,
        Feast,
    }
}
