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
    public sealed class BreakPlayTests
    {
        private const string CatalogPath = "Assets/_Game/Config/PropCatalog.asset";
        private static readonly Vector3 BesideDesk = new Vector3(1.6f, 0.6f, -1.2f);

        private GameBootstrap bootstrap;
        private LevelSpawner spawner;
        private DebrisPool debris;
        private PropCatalog catalog;
        private LevelDef level;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
            bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            spawner = Object.FindAnyObjectByType<LevelSpawner>();
            debris = Object.FindAnyObjectByType<DebrisPool>();
            Assert.IsNotNull(debris, "scene has no DebrisPool");
            catalog = AssetDatabase.LoadAssetAtPath<PropCatalog>(CatalogPath);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (level != null) Object.Destroy(level);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MugDroppedOnFloor_ShattersAndPiecesFadeBackToPool()
        {
            var mug = LoadOne(PropKind.Mug);
            Teleport(mug, BesideDesk);
            int peak = 0;

            for (int i = 0; i < Steps(1f); i++)
            {
                yield return new WaitForFixedUpdate();
                peak = Mathf.Max(peak, debris.ActiveSets);
            }
            Assert.AreEqual(1, peak, "the mug never shattered");
            Assert.IsFalse(mug.gameObject.activeSelf, "the intact mug should be swapped out");

            yield return WaitSeconds(FadeTotal() + 0.5f);
            Assert.AreEqual(0, debris.ActiveSets);
        }

        [UnityTest]
        public IEnumerator KeycapDroppedOnFloor_PopsAndDeactivates()
        {
            var keycap = LoadOne(PropKind.Keycap1u);
            Teleport(keycap, BesideDesk);

            yield return WaitSeconds(1f + FadeTotal() + 0.5f);

            Assert.IsTrue(keycap.IsCleared);
            Assert.IsFalse(keycap.gameObject.activeSelf);
            Assert.AreEqual(0, debris.ActiveSets, "a pop never uses debris sets");
        }

        [UnityTest]
        public IEnumerator BreakBeforeClearCheck_CountsTargetOnce()
        {
            var mug = LoadOne(PropKind.Mug);
            int cleared = 0;
            spawner.TargetCleared += _ => cleared++;
            Teleport(mug, new Vector3(1.6f, mug.transform.position.y - LevelSpawner.DeskTopY + 0.05f, -1.2f));

            yield return WaitSeconds(1f);

            Assert.AreEqual(1, cleared);
            Assert.AreEqual(0, spawner.TargetsRemaining);
        }

        [UnityTest]
        public IEnumerator ManyBreaks_ActiveDebrisNeverExceedsMax()
        {
            var props = LoadMany(PropKind.Mug, 10);
            int pooledObjects = debris.transform.childCount;
            for (int i = 0; i < props.Length; i++)
                Teleport(props[i], new Vector3(-3.4f + i * 0.75f, 0.6f, -1.6f));
            int max = bootstrap.Tuning.maxDebrisSets;
            int peak = 0;

            for (int i = 0; i < Steps(2f); i++)
            {
                yield return new WaitForFixedUpdate();
                Assert.LessOrEqual(debris.ActiveSets, max);
                peak = Mathf.Max(peak, debris.ActiveSets);
            }

            Assert.AreEqual(max, peak, "expected the budget to fill");
            Assert.AreEqual(pooledObjects, debris.transform.childCount, "debris was instantiated during play");
        }

        [UnityTest]
        public IEnumerator LoadLevel_DuringDebrisFade_ReturnsAllDebris()
        {
            var mug = LoadOne(PropKind.Mug);
            Teleport(mug, BesideDesk);
            yield return WaitSeconds(0.8f);
            Assert.AreEqual(1, debris.ActiveSets, "precondition: the mug shattered");

            bootstrap.LoadLevel(0);
            yield return null;

            Assert.AreEqual(0, debris.ActiveSets);
            foreach (Transform set in debris.transform)
                Assert.IsFalse(set.gameObject.activeSelf, $"{set.name} still active after a level load");
        }

        [UnityTest]
        public IEnumerator PropLeavingPlayVolume_IsClearedAndFades()
        {
            var keycap = LoadOne(PropKind.Keycap1u);
            Teleport(keycap, new Vector3(0f, 2f, LevelSpawner.PlayVolume.max.z + 1f));

            yield return WaitSeconds(FadeTotal() + 0.5f);

            Assert.IsTrue(keycap.IsCleared);
            Assert.IsFalse(keycap.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator MugKnockedOntoTheSideTable_BreaksAndCountsAsCleared()
        {
            var mug = LoadOne(PropKind.Mug);
            Teleport(mug, new Vector3(-1.45f, 1.8f, 2.1f)); // above the Home Office side table

            yield return WaitSeconds(1.5f);

            Assert.IsTrue(mug.IsCleared, "a prop knocked onto other furniture is off the desk");
            Assert.AreEqual(0, spawner.TargetsRemaining);
        }

        [UnityTest]
        public IEnumerator Win_PopsPropsThatDidNotCount()
        {
            level = ScriptableObject.CreateInstance<LevelDef>();
            level.id = "WinPop";
            level.props = new[]
            {
                new PropPlacement { kind = PropKind.Keycap1u, basePosition = new Vector3(-0.5f, 0f, 0f), isTarget = true },
                new PropPlacement { kind = PropKind.Book, basePosition = new Vector3(0.4f, 0f, 0.2f), isTarget = false },
            };
            debris.Prewarm(catalog, level, bootstrap.Tuning);
            spawner.Load(level, catalog, bootstrap.Tuning, debris, bootstrap.Events);
            var support = spawner.Spawned[1];
            yield return WaitSeconds(0.5f);

            Teleport(spawner.Spawned[0], BesideDesk);
            yield return WaitSeconds(1f + FadeTotal() + 0.5f);

            Assert.AreEqual(RoundState.Won, bootstrap.State);
            Assert.IsFalse(support.gameObject.activeSelf, "a desk shown as cleared should be empty");
        }

        [UnityTest]
        public IEnumerator FadeOut_NeverShrinksAPhysicsBodyBelowMinimumScale()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.AddComponent<Rigidbody>();
            var fade = cube.AddComponent<FadeOut>();
            float smallest = 1f;

            fade.Begin(0f, 0.3f);
            while (cube.activeSelf)
            {
                smallest = Mathf.Min(smallest, cube.transform.localScale.x);
                yield return null;
            }

            Assert.GreaterOrEqual(smallest, FadeOut.MinScale, "a near-zero convex collider gives the body degenerate inertia");
            Assert.AreEqual(1f, cube.transform.localScale.x, 1e-5f, "scale is restored for reuse");
            Object.Destroy(cube);
        }

        private DeskObject LoadOne(PropKind kind) => LoadMany(kind, 1)[0];

        private DeskObject[] LoadMany(PropKind kind, int count)
        {
            level = ScriptableObject.CreateInstance<LevelDef>();
            level.id = "Break_" + kind;
            level.props = new PropPlacement[count];
            for (int i = 0; i < count; i++)
                level.props[i] = new PropPlacement { kind = kind, basePosition = Vector3.zero, isTarget = true };
            debris.Prewarm(catalog, level, bootstrap.Tuning);
            spawner.Load(level, catalog, bootstrap.Tuning, debris, bootstrap.Events);
            var props = new DeskObject[count];
            for (int i = 0; i < count; i++) props[i] = spawner.Spawned[i];
            return props;
        }

        private static void Teleport(DeskObject prop, Vector3 position)
        {
            prop.transform.position = position;
            prop.Body.position = position;
            prop.Body.linearVelocity = Vector3.zero;
            prop.Body.angularVelocity = Vector3.zero;
        }

        private float FadeTotal() => bootstrap.Tuning.debrisHoldSeconds + bootstrap.Tuning.debrisFadeSeconds;

        private static int Steps(float seconds) => Mathf.RoundToInt(seconds / Time.fixedDeltaTime);

        private static IEnumerator WaitSeconds(float seconds)
        {
            for (int i = 0; i < Steps(seconds); i++) yield return new WaitForFixedUpdate();
        }
    }
}
