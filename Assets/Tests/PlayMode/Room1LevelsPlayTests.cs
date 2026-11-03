using System.Collections;
using System.Collections.Generic;
using Game.Core;
using Game.Levels;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    /// <summary>
    /// A smoke test, not the M3 playtest bot: for each Room 1 level, every target must be knocked off by at least one
    /// shot from a 9 yaw × 4 power grid, each on a freshly loaded level. Some targets are only reachable once others are
    /// gone (a support, or a prop shielded by another), so a target no fresh-level shot clears gets a second pass alone
    /// on its desk, aimed at, a few shots per power: that proves nothing is stuck. Level difficulty is the bot's job.
    /// </summary>
    public sealed class Room1LevelsPlayTests
    {
        private const int YawSteps = 9;
        private static readonly float[] Powers = { 0.4f, 0.6f, 0.8f, 1f };
        private const float SimulatedSeconds = 3f;
        private const float FastForward = 10f;
        private const int AloneShots = 3;
        // M2's levels; from M3 the playtest bot gates every level (a win clears every target, and every band needs wins).
        private const int SmokeLevels = 5;

        private GameBootstrap bootstrap;
        private LevelSpawnerProbe probe;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
            bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            probe = new LevelSpawnerProbe(Object.FindAnyObjectByType<World.LevelSpawner>());
            Time.timeScale = FastForward;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1f;
            yield return null;
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator EveryTarget_IsKnockedOffBySomeSampledShot()
        {
            var levels = (LevelList)new UnityEditor.SerializedObject(bootstrap).FindProperty("levels").objectReferenceValue;
            Assert.AreEqual("Room1LevelList", levels.name, "the game should play Room 1");
            var unreachable = new List<string>();
            for (int level = 0; level < SmokeLevels; level++)
            {
                bootstrap.LoadLevel(level);
                var targets = probe.TargetIndices();
                var knocked = new HashSet<int>();
                for (int y = 0; y < YawSteps && knocked.Count < targets.Count; y++)
                    foreach (float power in Powers)
                    {
                        bootstrap.LoadLevel(level);
                        yield return null;
                        bootstrap.FireForTest(Pull(Mathf.Lerp(-1f, 1f, y / (YawSteps - 1f)), power));
                        yield return new WaitForSeconds(SimulatedSeconds);
                        foreach (int i in targets)
                            if (probe.IsCleared(i)) knocked.Add(i);
                    }
                foreach (int i in targets)
                {
                    if (knocked.Contains(i)) continue;
                    bool reached = false;
                    yield return SweepAlone(level, i, result => reached = result);
                    if (!reached) unreachable.Add($"level {level + 1} target {i} ({probe.Kind(i)})");
                }
            }

            Assert.IsEmpty(unreachable, "No sampled shot knocks these off:\n" + string.Join("\n", unreachable));
        }

        /// <summary>The target alone on its desk, aimed at directly; up to AloneShots shots per power, like a player would.</summary>
        private IEnumerator SweepAlone(int level, int target, System.Action<bool> done)
        {
            var pouch = GameObject.Find("Pouch").transform.position;
            foreach (float power in Powers)
            {
                bootstrap.LoadLevel(level);
                probe.RemoveAllBut(target);
                yield return null;
                for (int shot = 0; shot < AloneShots; shot++)
                {
                    var at = probe.Position(target);
                    float yaw = Mathf.Atan2(at.x - pouch.x, at.z - pouch.z);
                    bootstrap.FireForTest(SlingshotMath.PullFor(yaw, power, bootstrap.Tuning));
                    yield return new WaitForSeconds(SimulatedSeconds);
                    if (!probe.IsCleared(target)) continue;
                    done(true);
                    yield break;
                }
            }
            done(false);
        }

        /// <param name="yaw01">-1 (full left aim) .. 1 (full right aim).</param>
        private Vector2 Pull(float yaw01, float power) =>
            SlingshotMath.PullFor(yaw01 * bootstrap.Tuning.maxYawDeg * Mathf.Deg2Rad, power, bootstrap.Tuning);

        private sealed class LevelSpawnerProbe
        {
            private readonly World.LevelSpawner spawner;

            public LevelSpawnerProbe(World.LevelSpawner spawner) => this.spawner = spawner;

            public List<int> TargetIndices()
            {
                var indices = new List<int>();
                for (int i = 0; i < spawner.Spawned.Count; i++)
                    if (spawner.Spawned[i].IsTarget) indices.Add(i);
                return indices;
            }

            public bool IsCleared(int i) => spawner.Spawned[i].IsCleared;

            public PropKind Kind(int i) => spawner.Spawned[i].Kind;

            public Vector3 Position(int i) => spawner.Spawned[i].Body.position;

            /// <summary>Deactivates every prop except the target.</summary>
            public void RemoveAllBut(int target)
            {
                for (int i = 0; i < spawner.Spawned.Count; i++)
                    if (i != target) spawner.Spawned[i].gameObject.SetActive(false);
            }
        }
    }
}
