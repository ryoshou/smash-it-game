using Game.Meta;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>Spec §8: interstitials only after a win, none after levels 1–5, at most one per 2 levels and per 90 s.</summary>
    public sealed class AdPacingTests
    {
        private AppConfig config;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<AppConfig>();
            config.firstInterstitialLevel = 6;
            config.minLevelsBetweenInterstitials = 2;
            config.minSecondsBetweenInterstitials = 90f;
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(config);

        private static bool Win(AdPacing pacing, int levelNumber, float now, bool removeAds = false)
        {
            pacing.LevelWon();
            return pacing.ShouldShowInterstitial(levelNumber, now, removeAds);
        }

        [Test]
        public void FirstFiveLevels_NeverShow()
        {
            var pacing = new AdPacing(config, 0f);

            for (int level = 1; level <= 5; level++)
                Assert.IsFalse(Win(pacing, level, 1000f), $"level {level}");
        }

        [Test]
        public void Level6_AfterNinetySeconds_Shows() => Assert.IsTrue(Win(new AdPacing(config, 0f), 6, 91f));

        [Test]
        public void WithinNinetySecondsOfSessionStart_DoesNotShow() => Assert.IsFalse(Win(new AdPacing(config, 0f), 6, 60f));

        [Test]
        public void AfterAnAd_NeedsTwoMoreWins()
        {
            var pacing = new AdPacing(config, 0f);
            Assert.IsTrue(Win(pacing, 6, 100f));
            pacing.InterstitialShown(100f);

            Assert.IsFalse(Win(pacing, 7, 300f));
            Assert.IsTrue(Win(pacing, 8, 400f));
        }

        [Test]
        public void AfterAnAd_NeedsNinetySeconds()
        {
            var pacing = new AdPacing(config, 0f);
            pacing.InterstitialShown(100f);

            Win(pacing, 7, 120f);
            Assert.IsFalse(Win(pacing, 8, 150f));
        }

        [Test]
        public void RemoveAds_NeverShows() => Assert.IsFalse(Win(new AdPacing(config, 0f), 6, 1000f, removeAds: true));
    }
}
