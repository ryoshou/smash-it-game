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
        private const float SplatGoneBy = 4.5f;
        private static readonly PropKind[] SoftFoods = { PropKind.Tomato, PropKind.Cupcake, PropKind.Cheese };
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

            yield return WaitSeconds(SplatGoneBy);
            Assert.AreEqual(0, LiveSplats(out _), "the splat should have faded");
        }

        /// <summary>The owner wants stains to linger (2–5 s): still there after 3 s, gone a little after 4.</summary>
        [UnityTest]
        public IEnumerator Splat_LingersAboutFourSecondsThenFades()
        {
            var donut = LoadDonuts(1, Difficulty.Normal)[0];
            Teleport(donut, BesideDesk);
            for (int i = 0; i < Steps(1.2f) && LiveSplats(out _) == 0; i++) yield return new WaitForFixedUpdate();
            Assert.Greater(LiveSplats(out _), 0, "precondition: the donut splatted");

            yield return WaitSeconds(3f);
            Assert.Greater(LiveSplats(out _), 0, "the stain faded too soon");

            yield return WaitSeconds(SplatGoneBy - 3f);
            Assert.AreEqual(0, LiveSplats(out _), "the stain should have faded");
        }

        /// <summary>Tomato, cupcake and cheese splash the moment the ball touches them, staining the desk under them.</summary>
        [UnityTest]
        public IEnumerator SoftFood_HitByTheBall_SplashesAtOnceOnTheDesk([ValueSource(nameof(SoftFoods))] PropKind kind)
        {
            var food = LoadKinds(kind)[0];
            yield return WaitSeconds(0.5f);
            Assert.IsTrue(food.gameObject.activeSelf, "precondition: it rests whole on the desk");
            float height = catalog.Get(kind).size.y;
            var centre = new Vector3(0f, LevelSpawner.DeskTopY + height * 0.6f, 0f);
            Object.FindAnyObjectByType<BallPool>().Get().Launch(centre + new Vector3(0f, 0.02f, -0.6f),
                new Vector3(0f, 0f, bootstrap.Tuning.minSpeed), bootstrap.Tuning, bootstrap.Events);
            var splat = new Vector3(float.NaN, float.NaN, float.NaN);

            for (int i = 0; i < Steps(0.5f) && float.IsNaN(splat.y); i++)
            {
                yield return new WaitForFixedUpdate();
                if (LiveSplatsAt(out Vector3 at) > 0) splat = at;
            }

            Assert.IsFalse(food.gameObject.activeSelf, "it should splash when the ball touches it");
            Assert.IsTrue(food.IsCleared, "a splashed target counts as cleared");
            Assert.AreEqual(LevelSpawner.DeskTopY, splat.y, NearSplat, "the stain should lie on the desk under it");
            Assert.Less(new Vector2(splat.x, splat.z).magnitude, 0.15f, "the stain should be where the food stood, not where it was knocked to");
        }

        [UnityTest]
        public IEnumerator SoftFood_RestingSideBySide_StaysWhole()
        {
            var foods = LoadKinds(PropKind.Tomato, PropKind.Cupcake, PropKind.Cheese, PropKind.Tomato);
            yield return WaitSeconds(2f);

            foreach (var food in foods) Assert.IsTrue(food.gameObject.activeSelf && !food.IsBroken, $"{food.Kind} splashed at rest");
            Assert.AreEqual(0, LiveSplats(out _));
        }

        /// <summary>Other food still splashes only where it lands: a ball hit on the desk leaves no stain up there.</summary>
        [UnityTest]
        public IEnumerator Donut_HitByTheBall_SplatsOnlyWhereItLands()
        {
            var donut = LoadKinds(PropKind.Donut)[0];
            yield return WaitSeconds(0.5f);
            float height = catalog.Get(PropKind.Donut).size.y;
            Object.FindAnyObjectByType<BallPool>().Get().Launch(new Vector3(0f, LevelSpawner.DeskTopY + height * 0.6f + 0.02f, -0.6f),
                new Vector3(0f, 0f, bootstrap.Tuning.minSpeed), bootstrap.Tuning, bootstrap.Events);
            float highest = float.NegativeInfinity;

            for (int i = 0; i < Steps(0.3f); i++)
            {
                yield return new WaitForFixedUpdate();
                if (LiveSplats(out var y) > 0) highest = Mathf.Max(highest, y);
            }

            Assert.Less(highest, LevelSpawner.DeskTopY - 0.5f, "a donut should not splash on the desk where it was hit");
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

        /// <summary>Food thrown into the side of furniture splats where it lands, never on the side face in mid-air.</summary>
        [UnityTest]
        public IEnumerator Food_HitsTheSideOfFurniture_SplatsWhereItLands()
        {
            shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelf.transform.position = new Vector3(1.9f, 0.6f, -1.2f);
            shelf.transform.localScale = new Vector3(0.4f, 1.2f, 0.8f); // side face at x 1.7, top at y 1.2
            shelf.AddComponent<FloorSurface>();
            var donut = LoadDonuts(1, Difficulty.Normal)[0];
            Teleport(donut, new Vector3(1.3f, 0.7f, -1.2f));
            donut.Body.linearVelocity = new Vector3(4f, 0f, 0f);
            float highest = float.NegativeInfinity;
            int seen = 0;

            for (int i = 0; i < Steps(2f); i++)
            {
                yield return new WaitForFixedUpdate();
                if (LiveSplats(out var y) > 0) { seen++; highest = Mathf.Max(highest, y); }
            }

            Assert.Greater(seen, 0, "the donut never splatted");
            Assert.AreEqual(0f, highest, NearSplat, "a splat should lie on the floor, not on the shelf's side");
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
            int live = LiveSplatsAt(out Vector3 at);
            y = at.y;
            return live;
        }

        /// <summary>Live splats under the effect pool; at is the last one's position.</summary>
        private int LiveSplatsAt(out Vector3 at)
        {
            int live = 0;
            at = new Vector3(float.NaN, float.NaN, float.NaN);
            foreach (Transform effect in vfx.transform)
            {
                var splat = effect.Find("Splat");
                if (splat == null || !splat.GetComponent<ParticleSystem>().IsAlive()) continue;
                live++;
                at = splat.position;
            }
            return live;
        }

        /// <summary>One of each kind on the main desk, side by side and 0.3 apart.</summary>
        private DeskObject[] LoadKinds(params PropKind[] kinds)
        {
            level = ScriptableObject.CreateInstance<LevelDef>();
            level.id = "Splash_Kinds";
            level.props = new PropPlacement[kinds.Length];
            for (int i = 0; i < kinds.Length; i++)
                level.props[i] = new PropPlacement
                {
                    kind = kinds[i], basePosition = new Vector3((i - (kinds.Length - 1) / 2f) * 0.3f, 0f, 0f), isTarget = true,
                };
            debris.Prewarm(catalog, level, bootstrap.Tuning);
            spawner.Load(level, catalog, bootstrap.Tuning, debris, bootstrap.Events);
            var props = new DeskObject[kinds.Length];
            for (int i = 0; i < kinds.Length; i++) props[i] = spawner.Spawned[i];
            return props;
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
