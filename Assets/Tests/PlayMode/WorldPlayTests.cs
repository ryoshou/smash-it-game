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
        private const string GameListPath = "Assets/_Game/Levels/GameLevelList.asset";
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
            Assert.AreEqual(3, levels.Count);
            yield return AssertStable(levels);
        }

        [UnityTest]
        [Timeout(3600000)]
        public IEnumerator GameLevels_AreStableAtSpawn()
        {
            var levels = AssetDatabase.LoadAssetAtPath<LevelList>(GameListPath);
            Assert.IsNotNull(levels, "game level list is missing");
            Assert.GreaterOrEqual(levels.Count, 25);
            yield return AssertStable(levels);
        }

        private IEnumerator AssertStable(LevelList levels)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<PropCatalog>(CatalogPath);

            for (int i = 0; i < levels.Count; i++)
            {
                var level = levels.At(i);
                int targets = System.Array.FindAll(level.props, p => p.isTarget).Length;
                spawner.Load(level, catalog, tuning, null, null);

                yield return FixedSteps(Seconds(2f));
                Assert.IsFalse(spawner.AnyMoving(), $"{level.id} still moving after 2 s");
                yield return FixedSteps(Seconds(1f));

                Assert.AreEqual(targets, spawner.TargetsRemaining, $"{level.id} lost targets at spawn");
                Assert.AreEqual(level.props.Length, spawner.Spawned.Count);
                int awake = 0;
                foreach (var prop in spawner.Spawned)
                    if (!prop.Body.IsSleeping()) awake++;
                Assert.AreEqual(0, awake, $"{level.id}: settled props should be asleep after 3 s");
            }
        }

        [UnityTest]
        public IEnumerator AllPropKinds_AreStableAtSpawn()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<PropCatalog>(CatalogPath);
            var level = ScriptableObject.CreateInstance<LevelDef>();
            level.id = "AllKinds";
            level.props = new[]
            {
                Place(PropKind.Keycap1u, -0.85f, -0.55f),
                Place(PropKind.Keycap2u, -0.45f, -0.55f),
                Place(PropKind.KeycapEnter, 0f, -0.55f),
                Place(PropKind.KeycapSpace, 0.5f, -0.55f),
                Place(PropKind.Mug, -0.6f, 0.3f),
                Place(PropKind.Book, 0.05f, 0.3f),
                Place(PropKind.CactusPot, 0.65f, 0.35f),
            };
            Assert.IsTrue(level.Validate(out string error), error);
            spawner.Load(level, catalog, tuning, null, null);

            yield return FixedSteps(Seconds(3f));

            Assert.AreEqual(level.props.Length, spawner.TargetsRemaining, "a prop fell off at spawn");
            foreach (var prop in spawner.Spawned)
            {
                Assert.IsTrue(prop.Body.IsSleeping(), $"{prop.name} still awake after 3 s");
                Assert.Greater(Vector3.Dot(prop.transform.up, Vector3.up), 0.99f, $"{prop.name} is not upright");
            }
            Object.Destroy(level);
        }

        /// <summary>Each M4 kind, every look, stands still where it spawns.</summary>
        [UnityTest]
        public IEnumerator RoomKinds_AreStableAtSpawn()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<PropCatalog>(CatalogPath);
            var level = ScriptableObject.CreateInstance<LevelDef>();
            level.id = "RoomKinds";
            level.props = new[]
            {
                Place(PropKind.Plate, -0.7f, -0.4f),
                Place(PropKind.MilkBottle, 0f, -0.4f),
                Look(Place(PropKind.Donut, 0.7f, -0.4f), 0),
                Look(Place(PropKind.Donut, -0.5f, 0.35f), 1),
                Look(Place(PropKind.Donut, 0.4f, 0.35f), 2),
                Look(Place(PropKind.Candle, -0.85f, 0.1f), 0),
                Look(Place(PropKind.Candle, -0.2f, 0.6f), 1),
                Place(PropKind.Hourglass, 0.15f, 0.1f),
                Look(Place(PropKind.PencilCup, 0.85f, 0.1f), 0),
                Look(Place(PropKind.SodaCan, -0.45f, 0.6f), 2),
                Look(Place(PropKind.GameController, 0.5f, 0.6f), 1),
            };
            Assert.IsTrue(level.Validate(out string error), error);
            spawner.Load(level, catalog, tuning, null, null);

            yield return FixedSteps(Seconds(3f));

            Assert.AreEqual(level.props.Length, spawner.TargetsRemaining, "a prop fell off at spawn");
            foreach (var prop in spawner.Spawned)
            {
                Assert.IsTrue(prop.Body.IsSleeping(), $"{prop.name} still awake after 3 s");
                Assert.Greater(Vector3.Dot(prop.transform.up, Vector3.up), 0.99f, $"{prop.name} is not upright");
            }
            Object.Destroy(level);
        }

        /// <summary>
        /// A new level spawns into an empty desk: the last level's props leave physics at once, not at the end of the
        /// frame, so they never shove the new layout (several physics steps can run in one frame).
        /// </summary>
        [UnityTest]
        public IEnumerator Load_ReplacingALevel_RemovesTheOldPropsFromPhysicsAtOnce()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<PropCatalog>(CatalogPath);
            var first = ScriptableObject.CreateInstance<LevelDef>();
            first.id = "Old";
            first.props = new[] { Place(PropKind.Mug, -0.5f, -0.3f) };
            var second = ScriptableObject.CreateInstance<LevelDef>();
            second.id = "New";
            second.props = new[] { Place(PropKind.Mug, 0.5f, 0.3f) };
            spawner.Load(first, catalog, tuning, null, null);
            yield return FixedSteps(2);

            spawner.Load(second, catalog, tuning, null, null);

            var old = Physics.OverlapBox(new Vector3(-0.5f, LevelSpawner.DeskTopY + 0.15f, -0.3f), Vector3.one * 0.1f);
            Assert.IsEmpty(old, "the old level's mug still collides");
            Object.Destroy(first);
            Object.Destroy(second);
        }

        /// <summary>Kitchen spec §5: every kitchen kind stands still where it spawns (the big ones, then the small).</summary>
        [UnityTest]
        public IEnumerator KitchenKinds_AreStableAtSpawn()
        {
            yield return AssertStableAtSpawn("KitchenBig", new[]
            {
                Place(PropKind.DinnerPlate, -0.5f, -0.35f),
                Place(PropKind.Teapot, 0.55f, -0.45f),
                Place(PropKind.Bowl, -0.6f, 0.45f),
                Place(PropKind.WineGlass, -0.1f, 0.4f),
                Place(PropKind.KetchupBottle, 0.55f, 0.05f),
                Place(PropKind.Burger, 0.38f, 0.5f),
                Place(PropKind.Sandwich, 0.8f, 0.45f),
            });
            yield return AssertStableAtSpawn("KitchenSmall", new[]
            {
                Place(PropKind.JamJar, -0.8f, 0f),
                Place(PropKind.DrinkCan, -0.45f, 0f),
                Place(PropKind.Apple, -0.12f, 0f),
                Place(PropKind.Tomato, 0.18f, 0f),
                Place(PropKind.Cheese, 0.5f, 0f),
                Place(PropKind.Cupcake, 0.82f, 0f),
                Place(PropKind.Croissant, 0f, 0.45f),
            });
        }

        private IEnumerator AssertStableAtSpawn(string id, PropPlacement[] props)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<PropCatalog>(CatalogPath);
            var level = ScriptableObject.CreateInstance<LevelDef>();
            level.id = id;
            level.props = props;
            Assert.IsTrue(level.Validate(out string error), error);
            spawner.Load(level, catalog, tuning, null, null);

            yield return FixedSteps(Seconds(3f));

            Assert.AreEqual(level.props.Length, spawner.TargetsRemaining, $"{id}: a prop fell off at spawn");
            foreach (var prop in spawner.Spawned)
            {
                Assert.IsTrue(prop.Body.IsSleeping(), $"{prop.name} still awake after 3 s");
                Assert.Greater(Vector3.Dot(prop.transform.up, Vector3.up), 0.99f, $"{prop.name} is not upright");
            }
            Object.Destroy(level);
        }

        private static PropPlacement Look(PropPlacement placement, int variant)
        {
            placement.variant = variant;
            return placement;
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

        private static PropPlacement Place(PropKind kind, float x, float z) =>
            new PropPlacement { kind = kind, basePosition = new Vector3(x, 0f, z), isTarget = true };

        private static int Seconds(float s) => Mathf.RoundToInt(s / Time.fixedDeltaTime);

        private static IEnumerator FixedSteps(int steps)
        {
            for (int i = 0; i < steps; i++) yield return new WaitForFixedUpdate();
        }
    }
}
