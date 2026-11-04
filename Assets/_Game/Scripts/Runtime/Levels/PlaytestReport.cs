using System;

namespace Game.Levels
{
    /// <summary>The playtest bot's results (docs/playtest/*.json), read back by the report gate test.</summary>
    [Serializable]
    public sealed class PlaytestReport
    {
        /// <summary>One report per room: docs/playtest/room&lt;N&gt;-report.json (Room 1's path is unchanged since M3).</summary>
        public static string PathFor(int room) => $"docs/playtest/room{room}-report.json";

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
