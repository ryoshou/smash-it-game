using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Game.Core;
using Game.Levels;
using Game.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Game.Tests
{
    /// <summary>
    /// Spec §6.3 playtest bot. Plays every level of a room 30 times at 20× speed like a player using the trajectory preview:
    /// pick a random remaining target, aim at it and pick the power whose arc passes through it, both with noise. It
    /// writes docs/playtest/room&lt;N&gt;-report.{json,md}. Not part of the normal suite: run tools/unity/playtest-bot.sh
    /// (SMASH_BOT_ROOMS="2 3" picks rooms; SMASH_BOT_LEVELS="26,27" re-runs only those global level numbers and keeps
    /// the other rows). Levels play at their index in the game list, as a player meets them.
    /// </summary>
    [Explicit, Category("Bot")]
    public sealed class PlaytestBot
    {
        private const int Attempts = 30;
        private const float TimeScale = 20f;
        private const float AimNoiseDeg = 3f;
        private const float PowerNoise = 0.06f;
        private const int PowerSearchSteps = 100;
        private const float QuietCapSeconds = 3f;
        private const float RoundEndCapSeconds = 15f;
        private const string LevelsVariable = "SMASH_BOT_LEVELS";

        private GameBootstrap bootstrap;
        private LevelSpawner spawner;
        private Vector3 pouch;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
            bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            spawner = Object.FindAnyObjectByType<LevelSpawner>();
            pouch = GameObject.Find("Pouch").transform.position;
            Time.timeScale = TimeScale;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest, Timeout(3600000)]
        public IEnumerator PlayRoom([Values(1, 2, 3, 4)] int room)
        {
            // GameLevelListBuilder.RoomListPath (editor-only assembly).
            var list = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelList>($"Assets/_Game/Levels/Room{room}/Room{room}LevelList.asset");
            if (list == null) Assert.Ignore($"room {room} has no levels yet");
            int offset = System.Array.IndexOf(bootstrap.Levels.levels, list.At(0));
            Assert.GreaterOrEqual(offset, 0, $"room {room} is not in the game list");
            var only = OnlyLevels();
            string path = PlaytestReport.PathFor(room);
            var rows = ReadExistingRows(path);
            for (int i = 0; i < list.Count; i++)
            {
                int index = offset + i;
                if (only != null && !only.Contains(index + 1)) continue;
                var level = list.At(i);
                var shotsOnWin = new List<int>();
                var rng = new Random(1000 + index);
                for (int attempt = 0; attempt < Attempts; attempt++)
                    yield return PlayOnce(index, level, rng, shotsOnWin);
                rows[level.id] = Row(level, index + 1, shotsOnWin);
                Debug.Log($"[Bot] {level.id}: {shotsOnWin.Count}/{Attempts}");
            }
            Write(room, path, list, offset, rows);
        }

        private IEnumerator PlayOnce(int index, LevelDef level, Random rng, List<int> shotsOnWin)
        {
            bootstrap.LoadLevel(index);
            yield return new WaitForFixedUpdate();
            while (bootstrap.State == RoundState.Playing && bootstrap.Budget.Remaining > 0)
            {
                if (!TryPickTarget(rng, out var target)) break;
                float aim = Mathf.Atan2(target.x - pouch.x, target.z - pouch.z);
                float yaw = aim + Gaussian(rng) * AimNoiseDeg * Mathf.Deg2Rad;
                float power = Mathf.Clamp01(PowerThrough(target, aim) + Gaussian(rng) * PowerNoise);
                bootstrap.FireForTest(SlingshotMath.PullFor(yaw, power, bootstrap.Tuning));
                float fired = Time.time;
                yield return new WaitForFixedUpdate();
                while (bootstrap.State == RoundState.Playing && Time.time - fired < QuietCapSeconds
                       && (!bootstrap.IsDeskQuiet || Time.time - fired < bootstrap.Tuning.reloadSeconds))
                    yield return null;
            }
            float end = Time.time;
            while (bootstrap.State == RoundState.Playing && Time.time - end < RoundEndCapSeconds) yield return null;
            if (bootstrap.State == RoundState.Won) shotsOnWin.Add(level.shots - bootstrap.Budget.Remaining);
            else LogLeftovers(level);
        }

        /// <summary>On a loss, which targets were left and where: the first thing to read when a level won't clear.</summary>
        private void LogLeftovers(LevelDef level)
        {
            var left = spawner.Spawned.Where(p => p.IsTarget && !p.IsCleared)
                .Select(p => $"{p.Kind}@({p.Body.position.x:0.00},{p.Body.position.y:0.00},{p.Body.position.z:0.00})");
            Debug.Log($"[Bot] {level.id} lost, left: {string.Join(" ", left)}");
        }

        /// <summary>The power whose arc (as the preview draws it) passes closest to the target's centre.</summary>
        private float PowerThrough(Vector3 target, float yaw)
        {
            var t = bootstrap.Tuning;
            var flat = new Vector2(target.x - pouch.x, target.z - pouch.z);
            float distance = flat.magnitude;
            float best = 0.5f, bestError = float.MaxValue;
            for (int step = 0; step <= PowerSearchSteps; step++)
            {
                float power = (float)step / PowerSearchSteps;
                var velocity = SlingshotMath.Compute(SlingshotMath.PullFor(yaw, power, t), t).Velocity;
                float horizontal = new Vector2(velocity.x, velocity.z).magnitude;
                if (horizontal <= 0f) continue;
                float time = distance / horizontal;
                float height = pouch.y + velocity.y * time + 0.5f * t.gravityY * time * time;
                float error = Mathf.Abs(height - target.y);
                if (error < bestError)
                {
                    bestError = error;
                    best = power;
                }
            }
            return best;
        }

        private bool TryPickTarget(Random rng, out Vector3 position)
        {
            var remaining = spawner.Spawned.Where(p => p.IsTarget && !p.IsCleared && p.gameObject.activeInHierarchy).ToList();
            position = remaining.Count > 0 ? remaining[rng.Next(remaining.Count)].Body.position : Vector3.zero;
            return remaining.Count > 0;
        }

        private static float Gaussian(Random rng)
        {
            double u1 = 1.0 - rng.NextDouble();
            double u2 = rng.NextDouble();
            return (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2));
        }

        private static PlaytestReport.Row Row(LevelDef level, int number, List<int> shotsOnWin)
        {
            shotsOnWin.Sort();
            float median = shotsOnWin.Count == 0 ? 0f
                : shotsOnWin.Count % 2 == 1 ? shotsOnWin[shotsOnWin.Count / 2]
                : (shotsOnWin[shotsOnWin.Count / 2 - 1] + shotsOnWin[shotsOnWin.Count / 2]) / 2f;
            return new PlaytestReport.Row
            {
                id = level.id, fingerprint = LevelFingerprint.Of(level), attempts = Attempts, wins = shotsOnWin.Count,
                clearRate = (float)shotsOnWin.Count / Attempts, medianShotsOnWin = median,
                band = ClearRateBand.For(level.difficulty, number).Name,
            };
        }

        private static HashSet<int> OnlyLevels()
        {
            string value = Environment.GetEnvironmentVariable(LevelsVariable);
            if (string.IsNullOrWhiteSpace(value)) return null;
            return new HashSet<int>(value.Split(',').Select(v => int.Parse(v.Trim())));
        }

        private static string FullPath(string relative) => Path.Combine(Application.dataPath, "..", relative);

        private static Dictionary<string, PlaytestReport.Row> ReadExistingRows(string relative)
        {
            var rows = new Dictionary<string, PlaytestReport.Row>();
            string path = FullPath(relative);
            if (!File.Exists(path)) return rows;
            foreach (var row in JsonUtility.FromJson<PlaytestReport>(File.ReadAllText(path)).rows) rows[row.id] = row;
            return rows;
        }

        /// <summary>Rows in list order; ids no longer in the list are dropped.</summary>
        private static void Write(int room, string path, LevelList list, int offset, Dictionary<string, PlaytestReport.Row> rows)
        {
            var ordered = new List<PlaytestReport.Row>();
            var md = new StringBuilder($"# Room {room} playtest bot report\n\n");
            md.Append($"{Attempts} attempts per level; seeded policy: random remaining target, aim and preview-arc power with ")
              .Append($"noise (σ {AimNoiseDeg}° aim, σ {PowerNoise} power), ")
              .Append("no continues. Bands: teaching 0.60–1.00, normal 0.35–0.95, hard 0.15–0.65 (spec §6.3).\n\n")
              .Append("| Level | Band | Shots | Clear rate | Median shots on win | In band |\n|---|---|---|---|---|---|\n");
            for (int i = 0; i < list.Count; i++)
            {
                var level = list.At(i);
                if (!rows.TryGetValue(level.id, out var row)) continue;
                ordered.Add(row);
                bool fresh = row.fingerprint == LevelFingerprint.Of(level);
                bool ok = ClearRateBand.For(level.difficulty, offset + i + 1).Accepts(row.clearRate);
                md.Append($"| {level.id} | {row.band} | {level.shots} | {row.clearRate:0.00} | {row.medianShotsOnWin:0.#} | ")
                  .Append(!fresh ? "stale" : ok ? "yes" : "**no**").Append(" |\n");
            }
            Directory.CreateDirectory(Path.GetDirectoryName(FullPath(path)));
            File.WriteAllText(FullPath(path), JsonUtility.ToJson(new PlaytestReport { rows = ordered.ToArray() }, true));
            File.WriteAllText(FullPath(Path.ChangeExtension(path, ".md")), md.ToString());
        }
    }
}
