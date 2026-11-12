using System.Collections;
using Game.Levels;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>The HUD and home sit inside the device's safe area (clear of the notch, Dynamic Island and home bar).</summary>
    public sealed class UiLayoutPlayTests
    {
        private const SafeArea.SafeAreaMode AllEdges =
            SafeArea.SafeAreaMode.Top | SafeArea.SafeAreaMode.Bottom | SafeArea.SafeAreaMode.Left | SafeArea.SafeAreaMode.Right;

        [TearDown]
        public void TearDown() => AppTestKit.ClearSave();

        [UnityTest]
        public IEnumerator HudAndHome_InsideASafeAreaThatInsetsEveryEdge()
        {
            yield return SceneManager.LoadSceneAsync("Game");

            var hud = Object.FindAnyObjectByType<Hud>(FindObjectsInactive.Include);
            var home = Object.FindAnyObjectByType<HomeScreen>(FindObjectsInactive.Include);
            var hudArea = hud.GetComponentInParent<SafeArea>(true);
            var homeArea = home.GetComponentInParent<SafeArea>(true);

            Assert.IsNotNull(hudArea, "the HUD is inside a SafeArea");
            Assert.AreSame(hudArea, homeArea, "home shares it");
            Assert.AreEqual(AllEdges, hudArea.Edges, "no edges means no inset at all");
        }

        /// <summary>The longest level name ("Level N · Feast!") shrinks to fit its badge instead of being cut.</summary>
        [UnityTest]
        public IEnumerator Hud_LongLevelNameFits()
        {
            yield return AppTestKit.LaunchAndPlay();
            var hud = Object.FindAnyObjectByType<Hud>();
            hud.SetLevel(37, Difficulty.Feast);
            Canvas.ForceUpdateCanvases();

            var label = hud.transform.Find("Level/Face/Label").GetComponent<Text>();
            Assert.AreEqual(label.text.Length, label.cachedTextGenerator.characterCountVisible, label.text);
        }

        [UnityTest]
        public IEnumerator Hud_PauseIsAnIcon()
        {
            yield return AppTestKit.LaunchAndPlay();

            var pause = AppTestKit.ButtonNamed<Hud>("Pause");
            Assert.IsEmpty(pause.GetComponentsInChildren<Text>(true));
        }
    }
}
