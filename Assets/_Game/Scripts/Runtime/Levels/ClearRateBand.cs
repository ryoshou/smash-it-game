namespace Game.Levels
{
    /// <summary>
    /// The playtest bot's acceptable clear rate for a level (spec §6.3). Teaching levels (1–10, Normal) should almost
    /// always clear, Hard ones rarely, and Feasts (Kitchen spec §4) clear easily wherever they sit. Claude's starting values; recalibrate with soft-launch data.
    /// </summary>
    public readonly struct ClearRateBand
    {
        public const int LastTeachingLevel = 10;

        public readonly string Name;
        public readonly float Min;
        public readonly float Max;

        private ClearRateBand(string name, float min, float max)
        {
            Name = name;
            Min = min;
            Max = max;
        }

        public bool Accepts(float rate) => rate >= Min && rate <= Max;

        public static ClearRateBand For(Difficulty difficulty, int levelNumber)
        {
            if (difficulty == Difficulty.Feast) return new ClearRateBand("feast", 0.60f, 1.00f);
            if (difficulty == Difficulty.Hard) return new ClearRateBand("hard", 0.15f, 0.65f);
            return levelNumber <= LastTeachingLevel
                ? new ClearRateBand("teaching", 0.60f, 1.00f)
                : new ClearRateBand("normal", 0.35f, 0.95f);
        }
    }
}
