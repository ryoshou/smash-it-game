using System.Collections;
using Game.Monetization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    /// <summary>The test-mode ad and purchase services that stand in for MAX and IAP until M4.</summary>
    public sealed class SimulatedServicesPlayTests
    {
        private SimulatedAdService ads;
        private SimulatedPurchaseService store;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            PlayerPrefs.DeleteKey(SimulatedPurchaseService.OwnershipKey);
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
            ads = Object.FindAnyObjectByType<SimulatedAdService>();
            store = Object.FindAnyObjectByType<SimulatedPurchaseService>();
            Assert.IsNotNull(ads, "scene has no SimulatedAdService");
            Assert.IsNotNull(store, "scene has no SimulatedPurchaseService");
        }

        [TearDown]
        public void TearDown() => PlayerPrefs.DeleteKey(SimulatedPurchaseService.OwnershipKey);

        [UnityTest]
        public IEnumerator ShowRewarded_ClosedEarly_ReportsNotRewarded()
        {
            int calls = 0;
            bool rewarded = true;
            ads.ShowRewarded(r => { calls++; rewarded = r; });
            yield return null;

            Assert.IsTrue(ads.IsShowing);
            ads.Close();

            Assert.AreEqual(1, calls);
            Assert.IsFalse(rewarded);
            Assert.IsFalse(ads.IsShowing);
        }

        [UnityTest]
        public IEnumerator ShowRewarded_WatchedToEnd_ReportsRewarded()
        {
            bool rewarded = false;
            ads.ShowRewarded(r => rewarded = r);
            yield return new WaitForSecondsRealtime(ads.AdSeconds + 0.2f);

            ads.Close();

            Assert.IsTrue(rewarded);
        }

        [UnityTest]
        public IEnumerator ShowInterstitial_CannotCloseBeforeCountdown()
        {
            int closed = 0;
            ads.ShowInterstitial(() => closed++);
            yield return null;

            ads.Close();
            Assert.AreEqual(0, closed);
            yield return new WaitForSecondsRealtime(ads.AdSeconds + 0.2f);
            ads.Close();

            Assert.AreEqual(1, closed);
        }

        [UnityTest]
        public IEnumerator ShowRewarded_WhileShowing_IsIgnored()
        {
            int first = 0, second = 0;
            ads.ShowRewarded(_ => first++);
            ads.ShowRewarded(_ => second++);
            yield return null;

            ads.Close();
            ads.Close();

            Assert.AreEqual(1, first);
            Assert.AreEqual(0, second);
        }

        [UnityTest]
        public IEnumerator Offline_NothingReady()
        {
            Assert.IsTrue(ads.InterstitialReady);
            Assert.IsTrue(ads.RewardedReady);

            ads.Offline = true;
            yield return null;

            Assert.IsFalse(ads.InterstitialReady);
            Assert.IsFalse(ads.RewardedReady);
        }

        [UnityTest]
        public IEnumerator Buy_ThenRestoreAfterClearingSave_ReportsOwned()
        {
            bool purchased = false, owned = false;
            store.BuyRemoveAds(p => purchased = p);
            yield return null;
            store.Confirm();
            PlayerPrefs.DeleteKey("smash.save");

            store.RestoreRemoveAds(o => owned = o);

            Assert.IsTrue(purchased);
            Assert.IsTrue(owned);
        }

        [UnityTest]
        public IEnumerator Buy_Cancel_ReportsNotPurchased()
        {
            bool? purchased = null, owned = null;
            store.BuyRemoveAds(p => purchased = p);
            yield return null;

            store.Cancel();
            store.RestoreRemoveAds(o => owned = o);

            Assert.AreEqual(false, purchased);
            Assert.AreEqual(false, owned);
            Assert.IsFalse(store.IsShowing);
        }

        [UnityTest]
        public IEnumerator Buy_WhileDialogOpen_IsIgnored()
        {
            int first = 0, second = 0;
            store.BuyRemoveAds(_ => first++);
            store.BuyRemoveAds(_ => second++);
            yield return null;

            store.Confirm();
            store.Confirm();

            Assert.AreEqual(1, first);
            Assert.AreEqual(0, second);
        }

        [Test]
        public void Price_ComesFromConfig() => Assert.AreEqual("$3.99", store.RemoveAdsPrice);
    }
}
