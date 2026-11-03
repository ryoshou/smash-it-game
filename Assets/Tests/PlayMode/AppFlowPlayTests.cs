using System.Collections;
using System.Linq;
using Game.Juice;
using Game.Meta;
using Game.Monetization;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>Splash → home → play, pause and settings, against the real Game scene and a real save.</summary>
    public sealed class AppFlowPlayTests
    {
        private AppController app;
        private GameBootstrap bootstrap;

        [SetUp]
        public void ClearSave()
        {
            PlayerPrefs.DeleteKey(PlayerPrefsSaveStore.Key);
            PlayerPrefs.DeleteKey(SimulatedPurchaseService.OwnershipKey);
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            ClearSave();
        }

        private IEnumerator Launch(int savedLevel = 0)
        {
            new Profile(new PlayerPrefsSaveStore()).RecordWin(savedLevel - 1);
            yield return SceneManager.LoadSceneAsync("Game");
            app = Object.FindAnyObjectByType<AppController>();
            bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            Assert.IsNotNull(app, "scene has no AppController");
            float deadline = Time.realtimeSinceStartup + 5f;
            while (app.Screen != AppScreen.Home && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.AreEqual(AppScreen.Home, app.Screen);
        }

        private static Button ButtonNamed<T>(string name) where T : Component =>
            Object.FindAnyObjectByType<T>(FindObjectsInactive.Include).GetComponentsInChildren<Button>(true).First(b => b.name == name);

        private static bool ShowsText<T>(string text) where T : Component =>
            Object.FindAnyObjectByType<T>(FindObjectsInactive.Include).GetComponentsInChildren<Text>().Any(t => t.text == text);

        [UnityTest]
        public IEnumerator Launch_ShowsHomeWithSavedLevel()
        {
            yield return Launch(savedLevel: 3);

            Assert.IsTrue(ShowsText<HomeScreen>("Play · Level 4"));
            Assert.AreEqual("R1_L04", bootstrap.Level.id);
            Assert.IsFalse(bootstrap.InputEnabled);
        }

        [UnityTest]
        public IEnumerator Play_EnablesInput()
        {
            yield return Launch();

            ButtonNamed<HomeScreen>("Play").onClick.Invoke();
            yield return null;

            Assert.AreEqual(AppScreen.Playing, app.Screen);
            Assert.IsTrue(bootstrap.InputEnabled);
            Assert.IsFalse(Object.FindAnyObjectByType<HomeScreen>(FindObjectsInactive.Include).gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator Backgrounding_PausesPlay()
        {
            yield return Launch();
            ButtonNamed<HomeScreen>("Play").onClick.Invoke();
            yield return null;

            app.SendMessage("OnApplicationPause", true);
            yield return null;

            Assert.AreEqual(AppScreen.Paused, app.Screen);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.IsFalse(bootstrap.InputEnabled);
            ButtonNamed<PausePanel>("Resume").onClick.Invoke();
            Assert.AreEqual(1f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator SoundToggle_MutesSfxAndPersists()
        {
            yield return Launch();
            ButtonNamed<HomeScreen>("Play").onClick.Invoke();
            ButtonNamed<Hud>("Pause").onClick.Invoke();
            yield return null;

            var sound = Object.FindAnyObjectByType<PausePanel>().GetComponentsInChildren<Toggle>().First(t => t.name == "Sound");
            sound.isOn = false;

            Assert.IsTrue(Object.FindAnyObjectByType<SfxPlayer>().Muted);
            Assert.IsFalse(new Profile(new PlayerPrefsSaveStore()).Data.sound);
        }

        [UnityTest]
        public IEnumerator Begin_WithSavedLevelBeyondList_PlaysWrappedLayout()
        {
            yield return Launch(savedLevel: 26);
            ButtonNamed<HomeScreen>("Play").onClick.Invoke();
            yield return null;

            Assert.IsTrue(ShowsText<Hud>("Level 27"));
            Assert.AreEqual(bootstrap.Levels.At(26), bootstrap.Level);
        }

        [UnityTest]
        public IEnumerator RemoveAds_Purchase_HidesButtonAndSetsFlag()
        {
            yield return Launch();
            var removeAds = ButtonNamed<HomeScreen>("Remove Ads");
            Assert.IsTrue(removeAds.gameObject.activeSelf);
            Assert.IsTrue(ShowsText<HomeScreen>("Remove Ads $3.99"));

            removeAds.onClick.Invoke();
            yield return null;
            Object.FindAnyObjectByType<SimulatedPurchaseService>().Confirm();

            Assert.IsFalse(removeAds.gameObject.activeSelf);
            Assert.IsTrue(new Profile(new PlayerPrefsSaveStore()).Data.removeAds);
        }
    }
}
