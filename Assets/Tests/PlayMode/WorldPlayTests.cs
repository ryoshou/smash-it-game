using System.Collections;
using Game.Core;
using Game.Levels;
using Game.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public sealed class WorldPlayTests
    {
        private const string LevelListPath = "Assets/_Game/Levels/PrototypeLevelList.asset";
        private const string CatalogPath = "Assets/_Game/Config/PropCatalog.asset";
        private const string TuningPath = "Assets/_Game/Config/ShotTuning.asset";

        private LevelSpawner spawner;
        private BallPool pool;
        private Transform pouch;
        private ShotTuning tuning;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
            spawner = Object.FindAnyObjectByType<LevelSpawner>();
            pool = Object.FindAnyObjectByType<BallPool>();
            var pouchObject = GameObject.Find("Pouch");
            Assert.IsNotNull(spawner, "scene has no LevelSpawner");
            Assert.IsNotNull(pool, "scene has no BallPool");
            Assert.IsNotNull(pouchObject, "scene has no Pouch");
            pouch = pouchObject.transform;
            tuning = AssetDatabase.LoadAssetAtPath<ShotTuning>(TuningPath).Clone();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(tuning);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PrototypeLevels_AreStableAtSpawn()
        {
            var levels = AssetDatabase.LoadAssetAtPath<LevelList>(LevelListPath);
            var catalog = AssetDatabase.LoadAssetAtPath<PropCatalog>(CatalogPath);
            Assert.AreEqual(3, levels.Count);

            for (int i = 0; i < levels.Count; i++)
            {
                var level = levels.At(i);
                int targets = System.Array.FindAll(level.props, p => p.isTarget).Length;
                spawner.Load(level, catalog, tuning);

                yield return FixedSteps(Seconds(2f));
                Assert.IsFalse(spawner.AnyMoving(), $"{level.id} still moving after 2 s");
                yield return FixedSteps(Seconds(1f));

                Assert.AreEqual(targets, spawner.TargetsRemaining, $"{level.id} lost targets at spawn");
                Assert.AreEqual(level.props.Length, spawner.Spawned.Count);
            }
        }

        [UnityTest]
        public IEnumerator PreviewMatchesFlight()
        {
            var launch = SlingshotMath.Compute(new Vector2(0f, -0.2f), tuning);
            var origin = pouch.position + new Vector3(3f, 1f, -1f);
            var dots = new Vector3[8];
            TrajectoryMath.Sample(origin, launch.Velocity, Physics.gravity, Time.fixedDeltaTime, 1, dots);

            var ball = pool.Get();
            ball.Launch(origin, launch.Velocity, tuning, null);
            yield return FixedSteps(8);

            var body = ball.GetComponent<Rigidbody>();
            Assert.IsTrue(ball.IsActive);
            Assert.Less(Vector3.Distance(dots[7], body.position), 0.01f,
                $"preview {dots[7]} vs flight {body.position}");
        }

        private static int Seconds(float s) => Mathf.RoundToInt(s / Time.fixedDeltaTime);

        private static IEnumerator FixedSteps(int steps)
        {
            for (int i = 0; i < steps; i++) yield return new WaitForFixedUpdate();
        }
    }
}
