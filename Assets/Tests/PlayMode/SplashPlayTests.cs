using System.Collections;
using Game.Juice;
using Game.Levels;
using Game.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    /// <summary>Kitchen spec §6: food bursts into chunks and leaves a coloured splat where it landed, which fades.</summary>
    public sealed class SplashPlayTests
    {
        private const string CatalogPath = "Assets/_Game/Config/PropCatalog.asset";
        private const string JuicePath = "Assets/_Game/Config/JuiceConfig.asset";
        private const float NearSplat = 0.02f;
        private static readonly Vector3 BesideDesk = new Vector3(1.6f, 1f, -1.2f);

        private GameBootstrap bootstrap;
        private LevelSpawner spawner;
        private DebrisPool debris;
        private VfxPool vfx;
        private PropCatalog catalog;
        private LevelDef level;
        private GameObject shelf;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
            bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            spawner = Object.FindAnyObjectByType<LevelSpawner>();
            debris = Object.FindAnyObjectByType<DebrisPool>();
            vfx = Object.FindAnyObjectByType<VfxPool>();
            catalog = AssetDatabase.LoadAssetAtPath<PropCatalog>(CatalogPath);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (level != null) Object.Destroy(level);
            if (shelf != null) Object.Destroy(shelf);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Donut_HitsFloor_SplatsAndChunksThenFades()
        {
            var donut = LoadDonuts(1, Difficulty.Normal)[0];
            Teleport(donut, BesideDesk);
            float splatY = float.NaN;
            int peak = 0;

            for (int i = 0; i < Steps(1.2f); i++)
            {
                yield return new WaitForFixedUpdate();
                peak = Mathf.Max(peak, debris.ActiveSets);
                if (float.IsNaN(splatY) && LiveSplats(out var y) > 0) splatY = y;
            }

            Assert.AreEqual(1, peak, "the donut never swapped to its pieces");
            Assert.IsFalse(donut.gameObject.activeSelf);
            Assert.IsFalse(float.IsNaN(splatY), "no splat appeared");
            Assert.AreEqual(0f, splatY, NearSplat, "the splat should lie on the floor");

            yield return WaitSeconds(2.5f);
            Assert.AreEqual(0, LiveSplats(out _), "the splat should have faded");
        }

        [UnityTest]
        public IEnumerator Food_LandsOnFurniture_SplatsOnItsTop()
        {
            shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelf.transform.position = new Vector3(1.6f, 0.3f, -1.2f);
            shelf.transform.localScale = new Vector3(0.8f, 0.6f, 0.8f); // top at y 0.6
            shelf.AddComponent<FloorSurface>();
            var donut = LoadDonuts(1, Difficulty.Normal)[0];
            Teleport(donut, BesideDesk + Vector3.up * 0.2f);
            float splatY = float.NaN;

            for (int i = 0; i < Steps(1.2f) && float.IsNaN(splatY); i++)
            {
                yield return new WaitForFixedUpdate();
                if (LiveSplats(out var y) > 0) splatY = y;
            }

            Assert.IsFalse(float.IsNaN(splatY), "no splat appeared");
            Assert.AreEqual(0.6f, splatY, NearSplat, "the splat should lie on the furniture's top");
        }

        [UnityTest]
        public IEnumerator Food_LeavesPlayVolume_NoSplat()
        {
            var donut = LoadDonuts(1, Difficulty.Normal)[0];
            Teleport(donut, new Vector3(0f, 2f, LevelSpawner.PlayVolume.max.z + 1f));
            int most = 0;

            for (int i = 0; i < Steps(1f); i++)
            {
                yield return new WaitForFixedUpdate();
                most = Mathf.Max(most, LiveSplats(out _));
            }

            Assert.IsTrue(donut.IsCleared);
            Assert.AreEqual(0, most, "a prop gone from the play volume leaves no splat");
        }

        [UnityTest]
        public IEnumerator Restart_WhileSplatsFade_ClearsThem()
        {
            var donut = LoadDonuts(1, Difficulty.Normal)[0];
            Teleport(donut, BesideDesk);
            for (int i = 0; i < Steps(1.2f) && LiveSplats(out _) == 0; i++) yield return new WaitForFixedUpdate();
            Assert.Greater(LiveSplats(out _), 0, "precondition: the donut splatted");

            bootstrap.Retry();
            yield return null;

            Assert.AreEqual(0, LiveSplats(out _), "a splat from the old round is still showing");
        }

        [UnityTest]
        public IEnumerator Feast_AllLandAtOnce_StaysWithinPools()
        {
            var donuts = LoadDonuts(30, Difficulty.Feast);
            int pooledDebris = debris.transform.childCount;
            int pooledVfx = vfx.transform.childCount;
            for (int i = 0; i < donuts.Length; i++)
                Teleport(donuts[i], new Vector3(-3.4f + (i % 10) * 0.75f, 0.6f + (i / 10) * 0.3f, -1.6f - (i / 10) * 0.5f));
            int feastSets = bootstrap.Tuning.feastDebrisSets;
            int splashes = AssetDatabase.LoadAssetAtPath<JuiceConfig>(JuicePath).splashVfxPerKind;
            int peak = 0;

            for (int i = 0; i < Steps(2f); i++)
            {
                yield return new WaitForFixedUpdate();
                Assert.LessOrEqual(debris.ActiveSets, feastSets);
                Assert.LessOrEqual(LiveSplats(out _), splashes);
                peak = Mathf.Max(peak, debris.ActiveSets);
            }

            Assert.AreEqual(feastSets, peak, "expected the feast budget to fill");
            Assert.AreEqual(pooledDebris, debris.transform.childCount, "debris was instantiated during play");
            Assert.AreEqual(pooledVfx, vfx.transform.childCount, "effects were instantiated during play");
        }

        /// <summary>Live splats under the effect pool; y is the last one's height.</summary>
        private int LiveSplats(out float y)
        {
            int live = 0;
            y = float.NaN;
            foreach (Transform effect in vfx.transform)
            {
                var splat = effect.Find("Splat");
                if (splat == null || !splat.GetComponent<ParticleSystem>().IsAlive()) continue;
                live++;
                y = splat.position.y;
            }
            return live;
        }

        private DeskObject[] LoadDonuts(int count, Difficulty difficulty)
        {
            level = ScriptableObject.CreateInstance<LevelDef>();
            level.id = "Splash_" + count;
            level.difficulty = difficulty;
            level.props = new PropPlacement[count];
            for (int i = 0; i < count; i++)
                level.props[i] = new PropPlacement { kind = PropKind.Donut, basePosition = Vector3.zero, isTarget = true };
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

        private static int Steps(float seconds) => Mathf.RoundToInt(seconds / Time.fixedDeltaTime);

        private static IEnumerator WaitSeconds(float seconds)
        {
            for (int i = 0; i < Steps(seconds); i++) yield return new WaitForFixedUpdate();
        }
    }
}
