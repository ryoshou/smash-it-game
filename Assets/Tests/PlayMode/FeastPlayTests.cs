using System.Collections;
using Game.Levels;
using Game.Meta;
using Game.UI;
using Game.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>Kitchen spec §4: feast levels hold more debris and say so on the HUD.</summary>
    public sealed class FeastPlayTests
    {
        private const string CatalogPath = "Assets/_Game/Config/PropCatalog.asset";
        private const string StringsPath = "Assets/_Game/Config/UiStrings_en.asset";

        private LevelDef level;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (level != null) Object.Destroy(level);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FeastLevel_PrewarmsFeastDebrisSets()
        {
            var bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            var debris = Object.FindAnyObjectByType<DebrisPool>();
            var catalog = AssetDatabase.LoadAssetAtPath<PropCatalog>(CatalogPath);

            debris.Prewarm(catalog, MugLevel(Difficulty.Feast), bootstrap.Tuning);
            Assert.AreEqual(12, debris.Capacity, "feast");

            debris.Prewarm(catalog, MugLevel(Difficulty.Normal), bootstrap.Tuning);
            Assert.AreEqual(6, debris.Capacity, "normal");
            yield return null;
        }

        [UnityTest]
        public IEnumerator Hud_FeastLevel_ShowsFeastLabel()
        {
            var strings = AssetDatabase.LoadAssetAtPath<UiStrings>(StringsPath);
            var hud = Object.FindAnyObjectByType<Hud>(FindObjectsInactive.Include);
            hud.Bind(strings, null);

            hud.SetLevel(27, Difficulty.Feast);
            yield return null;

            string expected = string.Format(strings.levelFeastFormat, 27);
            Assert.IsTrue(System.Array.Exists(hud.GetComponentsInChildren<Text>(true), t => t.text == expected), expected);
        }

        private LevelDef MugLevel(Difficulty difficulty)
        {
            if (level != null) Object.Destroy(level);
            level = ScriptableObject.CreateInstance<LevelDef>();
            level.id = "Feast_" + difficulty;
            level.difficulty = difficulty;
            level.props = new[] { new PropPlacement { kind = PropKind.Mug, basePosition = Vector3.zero, isTarget = true } };
            return level;
        }
    }
}
