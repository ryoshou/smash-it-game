using System.Collections.Generic;
using System.IO;
using Game.Levels;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>
    /// Spec §6.3 gate: every Room 1 level has a playtest-bot result for its current layout, inside its band. Moving a
    /// prop changes the fingerprint, so this fails until tools/unity/playtest-bot.sh is re-run.
    /// </summary>
    public sealed class PlaytestReportTests
    {
        private const string ListPath = "Assets/_Game/Levels/Room1/Room1LevelList.asset";

        [Test]
        public void PlaytestReport_CoversEveryRoom1LevelWithCurrentLayout()
        {
            string path = Path.Combine(Application.dataPath, "..", PlaytestReport.Room1Path);
            Assert.IsTrue(File.Exists(path), $"no playtest report at {PlaytestReport.Room1Path}: run tools/unity/playtest-bot.sh");
            var report = JsonUtility.FromJson<PlaytestReport>(File.ReadAllText(path));
            var rows = new Dictionary<string, PlaytestReport.Row>();
            foreach (var row in report.rows) rows[row.id] = row;
            var list = AssetDatabase.LoadAssetAtPath<LevelList>(ListPath);
            var problems = new List<string>();

            for (int i = 0; i < list.Count; i++)
            {
                var level = list.At(i);
                var band = ClearRateBand.For(level.difficulty, i + 1);
                if (!rows.TryGetValue(level.id, out var found)) problems.Add($"{level.id}: not in the report");
                else if (found.fingerprint != LevelFingerprint.Of(level)) problems.Add($"{level.id}: layout changed since the bot ran");
                else if (!band.Accepts(found.clearRate))
                    problems.Add($"{level.id}: clear rate {found.clearRate:0.00} outside {band.Name} band {band.Min:0.00}–{band.Max:0.00}");
            }

            Assert.IsEmpty(problems, string.Join("\n", problems));
        }
    }
}
