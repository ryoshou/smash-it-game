using System.Collections;
using Game.Meta;
using Game.UI;
using NUnit.Framework;
using UnityEngine.TestTools;
using static Game.Tests.AppTestKit;

namespace Game.Tests
{
    /// <summary>The room map and Pause → Home, against the real Game scene and a real save.</summary>
    public sealed class RoomMapPlayTests
    {
        private const int SavedInKitchen = 30; // level 31, Kitchen

        [SetUp]
        public void SetUp() => ClearSave();

        [TearDown]
        public void TearDown() => ClearSave();

        private static void Tap<T>(string button) where T : UnityEngine.Component => ButtonNamed<T>(button).onClick.Invoke();

        [UnityTest]
        public IEnumerator Rooms_ShowsProgress_LaterRoomsLocked()
        {
            yield return Launch(SavedInKitchen);

            Tap<HomeScreen>("Rooms");
            yield return null;

            Assert.IsTrue(IsShown<RoomMapPanel>());
            Assert.IsTrue(ShowsText<RoomMapPanel>("Home Office"));
            Assert.IsTrue(ShowsText<RoomMapPanel>("Replay"));
            Assert.IsTrue(ShowsText<RoomMapPanel>("Levels 26–50  ·  5 / 25"));
            Assert.IsFalse(ButtonNamed<RoomMapPanel>("Room 3").interactable, "the Café is locked");
        }

        [UnityTest]
        public IEnumerator DoneRoom_PlaysItsFirstLevel_SaveKeepsHighest()
        {
            yield return Launch(SavedInKitchen);
            Tap<HomeScreen>("Rooms");
            yield return null;

            Tap<RoomMapPanel>("Room 1");
            yield return null;

            Assert.AreEqual(AppScreen.Playing, App.Screen);
            Assert.IsFalse(IsShown<RoomMapPanel>());
            Assert.AreEqual(0, Bootstrap.LevelIndex);
            Assert.AreEqual(SavedInKitchen, Saved.levelIndex);
        }

        [UnityTest]
        public IEnumerator CurrentRoom_ContinuesFromTheSavedLevel()
        {
            yield return Launch(SavedInKitchen);
            Tap<HomeScreen>("Rooms");
            yield return null;

            Tap<RoomMapPanel>("Room 2");
            yield return null;

            Assert.AreEqual(SavedInKitchen, Bootstrap.LevelIndex);
        }

        [UnityTest]
        public IEnumerator Back_ClosesTheMap_StaysHome()
        {
            yield return Launch(SavedInKitchen);
            Tap<HomeScreen>("Rooms");
            yield return null;

            Tap<RoomMapPanel>("Back");
            yield return null;

            Assert.IsFalse(IsShown<RoomMapPanel>());
            Assert.AreEqual(AppScreen.Home, App.Screen);
        }

        [UnityTest]
        public IEnumerator PauseHome_AfterAReplay_HomeOffersTheSavedLevelAgain()
        {
            yield return Launch(SavedInKitchen);
            Tap<HomeScreen>("Rooms");
            yield return null;
            Tap<RoomMapPanel>("Room 1");
            yield return null;
            Tap<Hud>("Pause");
            yield return null;

            Tap<PausePanel>("Home");
            yield return null;

            Assert.AreEqual(AppScreen.Home, App.Screen);
            Assert.AreEqual(SavedInKitchen, Bootstrap.LevelIndex);
            Assert.IsTrue(ShowsText<HomeScreen>("Level 31"));
            Assert.AreEqual(1f, UnityEngine.Time.timeScale);
        }

        [UnityTest]
        public IEnumerator SettingsFromHome_HasNoHomeButton()
        {
            yield return Launch(SavedInKitchen);

            Tap<HomeScreen>("Settings");
            yield return null;

            Assert.IsFalse(ButtonNamed<PausePanel>("Home").gameObject.activeSelf);
        }
    }
}
