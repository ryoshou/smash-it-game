using System.Collections;
using Game.Core;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Game.Tests.AppTestKit;

namespace Game.Tests
{
    /// <summary>Spec §8 against the real scene: save on win, paced interstitials on Next, the optional +3 shots.</summary>
    public sealed class ResultFlowPlayTests
    {
        private const int Level6 = 5;
        private float clock;

        [SetUp]
        public void SetUp() => ClearSave();

        [TearDown]
        public void TearDown() => ClearSave();

        private IEnumerator PlayLevel6At(float seconds, bool removeAds = false)
        {
            yield return LaunchAndPlay(Level6, removeAds);
            clock = 0f;
            App.SetClockForTest(() => clock);
            clock = seconds;
        }

        private static void Tap(string button) => ButtonNamed<ResultPanel>(button).onClick.Invoke();

        [UnityTest]
        public IEnumerator Win_SavesProgressBeforeNext()
        {
            yield return LaunchAndPlay();

            yield return Win();

            Assert.IsTrue(IsShown<ResultPanel>());
            Assert.AreEqual(1, Saved.levelIndex);
        }

        [UnityTest]
        public IEnumerator Win_PanelWaitsForTheCelebration_ProgressSavedAtOnce()
        {
            yield return LaunchAndPlay();
            foreach (var prop in Object.FindAnyObjectByType<Game.World.LevelSpawner>().Spawned)
            {
                prop.Body.isKinematic = true;
                prop.Body.position = new Vector3(prop.Body.position.x, -1f, prop.Body.position.z);
            }

            yield return WaitFor(() => Bootstrap.State == RoundState.Won);
            yield return null;

            Assert.IsFalse(IsShown<ResultPanel>(), "the confetti plays first");
            Assert.AreEqual(1, Saved.levelIndex);
            yield return WaitFor(IsShown<ResultPanel>);
        }

        [UnityTest]
        public IEnumerator Next_OnLevel6After90s_ShowsInterstitialThenLoadsNext()
        {
            yield return PlayLevel6At(100f);
            yield return Win();

            Tap("Primary");

            Assert.IsTrue(Ads.IsShowing);
            Assert.AreEqual(Level6, Bootstrap.LevelIndex);
            yield return new WaitForSecondsRealtime(Ads.AdSeconds + 0.2f);
            Ads.Close();
            Assert.AreEqual(Level6 + 1, Bootstrap.LevelIndex);
            Assert.IsFalse(IsShown<ResultPanel>());
        }

        [UnityTest]
        public IEnumerator Next_WithInterstitialNotReady_LoadsNextLevelImmediately()
        {
            yield return PlayLevel6At(100f);
            Ads.Offline = true;
            yield return Win();

            Tap("Primary");

            Assert.IsFalse(Ads.IsShowing);
            Assert.AreEqual(Level6 + 1, Bootstrap.LevelIndex);
        }

        [UnityTest]
        public IEnumerator Next_WithRemoveAds_NeverShowsInterstitial()
        {
            yield return PlayLevel6At(100f, removeAds: true);
            yield return Win();

            Tap("Primary");

            Assert.IsFalse(Ads.IsShowing);
            Assert.AreEqual(Level6 + 1, Bootstrap.LevelIndex);
        }

        [UnityTest]
        public IEnumerator Lose_Offline_HidesContinue()
        {
            yield return LaunchAndPlay();
            Ads.Offline = true;

            yield return Lose();

            Assert.IsFalse(ButtonNamed<ResultPanel>("Primary").gameObject.activeSelf);
            Assert.IsTrue(ButtonNamed<ResultPanel>("Secondary").gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator Lose_RewardedClosedEarly_StaysLost()
        {
            yield return LaunchAndPlay();
            yield return Lose();

            Tap("Primary");
            Assert.IsTrue(Ads.IsShowing);
            Ads.Close();

            Assert.AreEqual(RoundState.Lost, Bootstrap.State);
            Assert.AreEqual(0, Bootstrap.Budget.Remaining);
            Assert.IsTrue(IsShown<ResultPanel>());
        }

        /// <summary>Review: the ad went away between the lose panel opening and the tap; the panel must not freeze.</summary>
        [UnityTest]
        public IEnumerator Lose_AdGoneBeforeContinueTap_ReShowsLoseWithRetry()
        {
            yield return LaunchAndPlay();
            yield return Lose();
            Assert.IsTrue(ButtonNamed<ResultPanel>("Primary").gameObject.activeSelf);

            Ads.Offline = true;
            Tap("Primary");

            Assert.IsTrue(IsShown<ResultPanel>());
            Assert.IsFalse(ButtonNamed<ResultPanel>("Primary").gameObject.activeSelf, "+3 shots should hide once no ad is ready");
            Tap("Secondary");
            Assert.AreEqual(RoundState.Playing, Bootstrap.State, "Retry must still work");
        }

        [UnityTest]
        public IEnumerator Lose_RewardedWatched_AddsThreeShots()
        {
            yield return LaunchAndPlay();
            yield return Lose();

            Tap("Primary");
            yield return new WaitForSecondsRealtime(Ads.AdSeconds + 0.2f);
            Ads.Close();

            Assert.AreEqual(RoundState.Playing, Bootstrap.State);
            Assert.AreEqual(3, Bootstrap.Budget.Remaining);
            Assert.IsFalse(IsShown<ResultPanel>());
        }

        [UnityTest]
        public IEnumerator ResultPanel_SecondTap_IsIgnored()
        {
            yield return LaunchAndPlay();
            yield return Win();

            Tap("Primary");
            Tap("Primary");

            Assert.AreEqual(1, Bootstrap.LevelIndex);
        }

        [UnityTest]
        public IEnumerator Retry_NeverShowsInterstitial()
        {
            yield return PlayLevel6At(100f);
            yield return Lose();

            Tap("Secondary");

            Assert.IsFalse(Ads.IsShowing);
            Assert.AreEqual(Level6, Bootstrap.LevelIndex);
            Assert.AreEqual(RoundState.Playing, Bootstrap.State);
        }

        [UnityTest]
        public IEnumerator Win_ShowsRevealOfNextNewThing()
        {
            yield return LaunchAndPlay();
            yield return Win();

            Assert.IsTrue(ShowsText<ResultPanel>("New thing to knock over: Coffee Mug!"));
        }
    }
}
