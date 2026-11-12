using System.Collections;
using System.Linq;
using Game.Juice;
using Game.Meta;
using Game.Monetization;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
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
        public void SetUp() => AppTestKit.ClearSave();

        [TearDown]
        public void TearDown() => AppTestKit.ClearSave();

        private IEnumerator Launch(int savedLevel = 0)
        {
            yield return AppTestKit.Launch(savedLevel);
            app = AppTestKit.App;
            bootstrap = AppTestKit.Bootstrap;
        }

        private static Button ButtonNamed<T>(string name) where T : Component => AppTestKit.ButtonNamed<T>(name);

        private static bool ShowsText<T>(string text) where T : Component => AppTestKit.ShowsText<T>(text);

        [UnityTest]
        public IEnumerator Launch_ShowsHomeWithSavedLevel()
        {
            yield return Launch(savedLevel: 3);

            Assert.IsTrue(ShowsText<HomeScreen>("Level 4"));
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

            Assert.IsTrue(ShowsText<Hud>("Level 27 · Feast!"), "level 27 is the Kitchen's first feast");
            Assert.AreEqual(bootstrap.Levels.At(26), bootstrap.Level);
        }

        [UnityTest]
        public IEnumerator RemoveAds_Purchase_HidesButtonAndSetsFlag()
        {
            yield return Launch();
            var removeAds = ButtonNamed<HomeScreen>("Remove Ads");
            Assert.IsTrue(removeAds.gameObject.activeSelf);
            Assert.IsTrue(ShowsText<HomeScreen>("No Ads"));

            removeAds.onClick.Invoke();
            yield return null;
            Object.FindAnyObjectByType<SimulatedPurchaseService>().Confirm();

            Assert.IsFalse(removeAds.gameObject.activeSelf);
            Assert.IsTrue(new Profile(new PlayerPrefsSaveStore()).Data.removeAds);
        }

        [UnityTest]
        public IEnumerator Pause_WhileHoldingPull_CancelsTheShot()
        {
            yield return Launch();
            ButtonNamed<HomeScreen>("Play").onClick.Invoke();
            yield return new WaitForSeconds(0.5f);
            int shots = bootstrap.Budget.Remaining;
            bootstrap.HoldForTest(new Vector2(0f, -0.3f));
            yield return null;
            Assert.IsTrue(bootstrap.IsHoldingPull);
            Assert.IsTrue(AnyDotVisible(), "a held pull shows the preview");

            ButtonNamed<Hud>("Pause").onClick.Invoke();
            yield return null;
            ButtonNamed<PausePanel>("Resume").onClick.Invoke();
            yield return null;
            bootstrap.ReleaseHoldForTest();
            yield return null;

            Assert.IsFalse(bootstrap.IsHoldingPull);
            Assert.AreEqual(shots, bootstrap.Budget.Remaining, "a cancelled pull must not fire");
            Assert.IsFalse(AnyDotVisible(), "no preview dots left behind");
        }

        [UnityTest]
        public IEnumerator JumpToNextRoom_LoadsFirstLevelOfNextRoom()
        {
            yield return Launch();
            var real = bootstrap.Levels;
            var other = ScriptableObject.CreateInstance<Game.Levels.RoomTheme>();
            var list = ScriptableObject.CreateInstance<Game.Levels.LevelList>();
            list.levels = new[] { Copy(real.At(0), real.At(0).room), Copy(real.At(1), real.At(0).room), Copy(real.At(2), other) };
            var so = new UnityEditor.SerializedObject(bootstrap);
            so.FindProperty("levels").objectReferenceValue = list;
            so.ApplyModifiedPropertiesWithoutUndo();
            bootstrap.LoadLevel(0);
            ButtonNamed<HomeScreen>("Play").onClick.Invoke();
            yield return null;

            app.JumpToNextRoom();
            yield return null;
            Assert.AreEqual(2, bootstrap.LevelIndex);
            Assert.AreEqual(other, Object.FindAnyObjectByType<Game.View.RoomPresenter>().Current);
            Assert.AreEqual(2, AppTestKit.Saved.levelIndex);

            app.JumpToNextRoom();
            yield return null;
            Assert.AreEqual(0, bootstrap.LevelIndex, "after the last room it wraps to level 1");
            Assert.AreEqual(0, AppTestKit.Saved.levelIndex);

            foreach (var level in list.levels) Object.Destroy(level);
            Object.Destroy(list);
            Object.Destroy(other);
        }

        private static Game.Levels.LevelDef Copy(Game.Levels.LevelDef level, Game.Levels.RoomTheme room)
        {
            var copy = Object.Instantiate(level);
            copy.room = room;
            return copy;
        }

        private static bool AnyDotVisible()
        {
            var view = Object.FindAnyObjectByType<Game.View.SlingshotView>();
            var dots = new UnityEditor.SerializedObject(view).FindProperty("dots");
            for (int i = 0; i < dots.arraySize; i++)
                if (((Transform)dots.GetArrayElementAtIndex(i).objectReferenceValue).gameObject.activeSelf) return true;
            return false;
        }
    }
}
