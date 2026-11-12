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
    /// <summary>
    /// The owner (2026-11-12): no food squashes until a new mechanism is chosen. Every food keeps its shape: the ball
    /// knocks it like any other prop, and on the floor it pops whole like a book, with no pieces and no stain.
    /// </summary>
    public sealed class FoodPlayTests
    {
        private const string CatalogPath = "Assets/_Game/Config/PropCatalog.asset";
        private static readonly PropKind[] Foods =
        {
            PropKind.Sandwich, PropKind.Tomato, PropKind.Cupcake, PropKind.Apple, PropKind.Cheese, PropKind.Burger,
            PropKind.Croissant, PropKind.JamJar, PropKind.KetchupBottle, PropKind.Donut,
        };
        private static readonly PropKind[] OnceSoft = { PropKind.Sandwich, PropKind.Tomato, PropKind.Cupcake };
        private static readonly Vector3 BesideDesk = new Vector3(1.6f, 1f, -1.2f);

        private GameBootstrap bootstrap;
        private LevelSpawner spawner;
        private DebrisPool debris;
        private VfxPool vfx;
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
            vfx = Object.FindAnyObjectByType<VfxPool>();
            catalog = AssetDatabase.LoadAssetAtPath<PropCatalog>(CatalogPath);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (level != null) Object.Destroy(level);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Food_HitsFloor_PopsWholeWithoutAStain([ValueSource(nameof(Foods))] PropKind kind)
        {
            var food = Load(kind);
            var start = food.transform.localScale;
            Teleport(food, BesideDesk);
            float mostSquashed = 0f;
            int splats = 0, peak = 0;

            for (int i = 0; i < Steps(3f); i++)
            {
                yield return new WaitForFixedUpdate();
                splats = Mathf.Max(splats, LiveSplats());
                peak = Mathf.Max(peak, debris.ActiveSets);
                var scale = food.transform.localScale;
                if (food.gameObject.activeSelf && scale.x > 0f)
                    mostSquashed = Mathf.Max(mostSquashed, Mathf.Abs(scale.y / scale.x - start.y / start.x));
            }

            Assert.IsTrue(food.IsBroken && food.IsCleared, "it should pop on the floor");
            Assert.IsFalse(food.gameObject.activeSelf, "it should be gone");
            Assert.AreEqual(0, peak, "it should not break into pieces");
            Assert.AreEqual(0, splats, "it should leave no stain");
            Assert.Less(mostSquashed, 0.01f, "it should keep its shape as it shrinks");
        }

        /// <summary>The ball no longer squashes the sandwich, tomato or cupcake on the desk: only the floor pops them.</summary>
        [UnityTest]
        public IEnumerator OnceSoftFood_HitByTheBall_StaysWholeOnTheDesk([ValueSource(nameof(OnceSoft))] PropKind kind)
        {
            var food = Load(kind);
            yield return WaitSeconds(0.5f);
            Assert.IsTrue(food.gameObject.activeSelf, "precondition: it rests whole on the desk");
            var centre = new Vector3(0f, LevelSpawner.DeskTopY + catalog.Get(kind).size.y * 0.6f, 0f);
            Object.FindAnyObjectByType<BallPool>().Get().Launch(centre + new Vector3(0f, 0.02f, -0.6f),
                new Vector3(0f, 0f, bootstrap.Tuning.minSpeed), bootstrap.Tuning, bootstrap.Events);

            for (int i = 0; i < Steps(0.3f); i++)
            {
                yield return new WaitForFixedUpdate();
                if (food.HasTouchedFloor) break;
            }

            if (!food.HasTouchedFloor) Assert.IsFalse(food.IsBroken, "the ball's touch alone should not break it");
            Assert.AreEqual(0, LiveSplats(), "it should leave no stain");
        }

        private int LiveSplats()
        {
            int live = 0;
            foreach (Transform effect in vfx.transform)
            {
                var splat = effect.Find("Splat");
                if (splat != null && splat.GetComponent<ParticleSystem>().IsAlive()) live++;
            }
            return live;
        }

        private DeskObject Load(PropKind kind)
        {
            level = ScriptableObject.CreateInstance<LevelDef>();
            level.id = "Food_" + kind;
            level.props = new[] { new PropPlacement { kind = kind, basePosition = Vector3.zero, isTarget = true } };
            debris.Prewarm(catalog, level, bootstrap.Tuning);
            spawner.Load(level, catalog, bootstrap.Tuning, debris, bootstrap.Events);
            return spawner.Spawned[0];
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
