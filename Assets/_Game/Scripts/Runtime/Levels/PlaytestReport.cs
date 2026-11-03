using System;

namespace Game.Levels
{
    /// <summary>The playtest bot's results (docs/playtest/*.json), read back by the report gate test.</summary>
    [Serializable]
    public sealed class PlaytestReport
    {
        public const string Room1Path = "docs/playtest/room1-report.json";

        public Row[] rows = new Row[0];

        [Serializable]
        public sealed class Row
        {
            public string id;
            public string fingerprint;
            public int attempts;
            public int wins;
            public float clearRate;
            public float medianShotsOnWin;
            public string band;
        }
    }
}
