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
    /// shot from a 9 yaw × 4 power grid (each shot on a freshly loaded level).
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

        [UnityTest]
        public IEnumerator EveryTarget_IsKnockedOffBySomeSampledShot()
        {
            var levels = (LevelList)new UnityEditor.SerializedObject(bootstrap).FindProperty("levels").objectReferenceValue;
            Assert.AreEqual("Room1LevelList", levels.name, "the game should play Room 1");
            var unreachable = new List<string>();
            for (int level = 0; level < 5; level++)
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
                    if (!knocked.Contains(i)) unreachable.Add($"level {level + 1} target {i} ({probe.Kind(i)})");
            }

            Assert.IsEmpty(unreachable, "No sampled shot knocks these off:\n" + string.Join("\n", unreachable));
        }

        /// <param name="yaw01">-1 (full left aim) .. 1 (full right aim).</param>
        private Vector2 Pull(float yaw01, float power)
        {
            var t = bootstrap.Tuning;
            float yaw = yaw01 * t.maxYawDeg * Mathf.Deg2Rad;
            float length = Mathf.Lerp(t.minPull, t.maxPull, power);
            return new Vector2(-Mathf.Sin(yaw), -Mathf.Cos(yaw)) * length;
        }

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
        }
    }
}
