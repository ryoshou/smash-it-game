using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Levels;
using Game.Meta;
using Game.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Game.Tests
{
    public sealed class MetaAssetsTests
    {
        private const string StringsPath = "Assets/_Game/Config/UiStrings_en.asset";
        private const string AppConfigPath = "Assets/_Game/Config/AppConfig.asset";

        private readonly List<Object> created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in created) Object.DestroyImmediate(o);
            created.Clear();
        }

        private static UiStrings Strings() =>
            AssetDatabase.LoadAssetAtPath<UiStrings>(StringsPath) ?? throw new AssertionException($"missing {StringsPath}");

        [Test]
        public void UiStrings_HasANameForEveryPropKind()
        {
            var strings = Strings();

            Assert.AreEqual(Enum.GetValues(typeof(PropKind)).Length, strings.propNames.Length);
            foreach (PropKind kind in Enum.GetValues(typeof(PropKind)))
                Assert.IsFalse(string.IsNullOrWhiteSpace(strings.propNames[(int)kind]), kind.ToString());
        }

        [Test]
        public void UiStrings_PropName_MissingEntry_FallsBackToKindName()
        {
            var strings = ScriptableObject.CreateInstance<UiStrings>();
            created.Add(strings);
            strings.propNames = new[] { "Keycap" };

            Assert.AreEqual("Keycap", strings.PropName(PropKind.Keycap1u));
            Assert.AreEqual("Mug", strings.PropName(PropKind.Mug));
        }

        [Test]
        public void UiStrings_FormatsHaveOnePlaceholder()
        {
            var strings = Strings();
            int formats = 0;
            foreach (var field in typeof(UiStrings).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!field.Name.EndsWith("Format")) continue;
                formats++;
                string format = (string)field.GetValue(strings);
                StringAssert.Contains("{0}", format, field.Name);
                Assert.DoesNotThrow(() => string.Format(format, "x"), field.Name);
            }
            Assert.AreEqual(7, formats);
        }

        [Test]
        public void AppConfig_DefaultsMatchSpecAdRules()
        {
            var config = AssetDatabase.LoadAssetAtPath<AppConfig>(AppConfigPath);

            Assert.IsNotNull(config, AppConfigPath);
            Assert.AreEqual(6, config.firstInterstitialLevel);
            Assert.AreEqual(2, config.minLevelsBetweenInterstitials);
            Assert.AreEqual(90f, config.minSecondsBetweenInterstitials);
            Assert.AreEqual("$3.99", config.simulatedPrice);
        }

        [Test]
        public void Hud_HardLevel_UsesHardFormat()
        {
            var go = new GameObject("HUD");
            created.Add(go);
            var shots = new GameObject("Shots").AddComponent<Text>();
            var level = new GameObject("Level").AddComponent<Text>();
            created.Add(shots.gameObject);
            created.Add(level.gameObject);
            var hud = go.AddComponent<Hud>();
            var so = new SerializedObject(hud);
            so.FindProperty("shotsText").objectReferenceValue = shots;
            so.FindProperty("levelText").objectReferenceValue = level;
            so.ApplyModifiedPropertiesWithoutUndo();
            var strings = ScriptableObject.CreateInstance<UiStrings>();
            created.Add(strings);
            strings.levelHardFormat = "L{0}!";

            hud.Bind(strings, null);
            hud.SetLevel(5, Difficulty.Hard);
            hud.SetShots(4);

            Assert.AreEqual("L5!", level.text);
            Assert.AreEqual("Shots: 4", shots.text);
        }
    }
}
