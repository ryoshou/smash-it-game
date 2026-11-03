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
    /// shot from a 9 yaw × 4 power grid, each on a freshly loaded level. A support is only reachable once what rests on
    /// it is gone, so a target no fresh-level shot clears gets a second pass with the props above it removed.
    /// </summary>
    public sealed class Room1LevelsPlayTests
    {
        private const int YawSteps = 9;
        private static readonly float[] Powers = { 0.4f, 0.6f, 0.8f, 1f };
        private const float SimulatedSeconds = 3f;
        private const float FastForward = 10f;

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
            for (int level = 0; level < levels.Count; level++)
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
                    yield return SweepWithLoadRemoved(level, i, result => reached = result);
                    if (!reached) unreachable.Add($"level {level + 1} target {i} ({probe.Kind(i)})");
                }
            }

            Assert.IsEmpty(unreachable, "No sampled shot knocks these off:\n" + string.Join("\n", unreachable));
        }

        private IEnumerator SweepWithLoadRemoved(int level, int target, System.Action<bool> done)
        {
            for (int y = 0; y < YawSteps; y++)
                foreach (float power in Powers)
                {
                    bootstrap.LoadLevel(level);
                    probe.RemoveLoadOn(target);
                    yield return null;
                    bootstrap.FireForTest(Pull(Mathf.Lerp(-1f, 1f, y / (YawSteps - 1f)), power));
                    yield return new WaitForSeconds(SimulatedSeconds);
                    if (probe.IsCleared(target))
                    {
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

            /// <summary>Deactivates every prop resting higher than the target over its footprint.</summary>
            public void RemoveLoadOn(int target)
            {
                var below = spawner.Spawned[target];
                var bounds = Bounds(below);
                for (int i = 0; i < spawner.Spawned.Count; i++)
                {
                    if (i == target) continue;
                    var other = Bounds(spawner.Spawned[i]);
                    bool above = other.center.y > bounds.center.y;
                    bool over = Mathf.Abs(other.center.x - bounds.center.x) < (other.extents.x + bounds.extents.x)
                                && Mathf.Abs(other.center.z - bounds.center.z) < (other.extents.z + bounds.extents.z);
                    if (above && over) spawner.Spawned[i].gameObject.SetActive(false);
                }
            }

            private static Bounds Bounds(World.DeskObject prop)
            {
                var colliders = prop.GetComponentsInChildren<Collider>();
                var bounds = colliders[0].bounds;
                foreach (var c in colliders) bounds.Encapsulate(c.bounds);
                return bounds;
            }
        }
    }
}
