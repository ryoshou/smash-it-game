using System.Collections;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using static Game.Tests.AppTestKit;

namespace Game.Tests
{
    /// <summary>The owner's home layout (2026-11-12): gear top right with No Ads under it, Rooms centred under Play.</summary>
    public sealed class HomeLayoutPlayTests
    {
        [SetUp]
        public void SetUp() => ClearSave();

        [TearDown]
        public void TearDown() => ClearSave();

        [UnityTest]
        public IEnumerator Home_GearTopRight_NoAdsUnderIt()
        {
            yield return Launch(savedLevel: 3);
            var settings = Bounds("Settings");
            var noAds = Bounds("Remove Ads");

            Assert.Greater(settings.center.x, Screen.width * 0.8f, "the gear sits on the right");
            Assert.IsEmpty(ButtonNamed<HomeScreen>("Settings").GetComponentsInChildren<Text>(true), "settings is an icon");
            Assert.Less(noAds.yMax, settings.yMin, "No Ads sits under the gear");
            Assert.AreEqual(settings.xMax, noAds.xMax, Screen.width * 0.02f, "their right edges line up");
        }

        [UnityTest]
        public IEnumerator Home_RoomsCentredUnderPlay()
        {
            yield return Launch(savedLevel: 3);
            var play = Bounds("Play");
            var rooms = Bounds("Rooms");

            Assert.AreEqual(play.center.x, rooms.center.x, 5f, "Rooms is centred under Play");
            Assert.Less(rooms.yMax, play.yMin, "Rooms is under Play");
        }

        [UnityTest]
        public IEnumerator Home_LevelOnTheBadge_PlaySaysPlay()
        {
            yield return Launch(savedLevel: 3);

            Assert.IsTrue(ShowsText<HomeScreen>("Level 4"));
            Assert.AreEqual("Play", ButtonNamed<HomeScreen>("Play").GetComponentInChildren<Text>().text);
        }

        [UnityTest]
        public IEnumerator Home_NoAdsBought_GearStaysPut()
        {
            yield return Launch(savedLevel: 3);
            var before = Bounds("Settings");

            Object.FindAnyObjectByType<HomeScreen>().SetRemoveAdsVisible(false);
            Canvas.ForceUpdateCanvases();

            Assert.AreEqual(before, Bounds("Settings"));
        }

        /// <summary>A home button's screen rectangle (the overlay canvas maps world corners to pixels).</summary>
        private static Rect Bounds(string button)
        {
            var corners = new Vector3[4];
            ((RectTransform)ButtonNamed<HomeScreen>(button).transform).GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }
    }
}
