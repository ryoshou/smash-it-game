using System.Collections.Generic;
using Game.Meta;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class AppFlowTests
    {
        [Test]
        public void FirstRun_GoesSplashConsentHome()
        {
            var flow = new AppFlow(firstRun: true);

            Assert.AreEqual(AppScreen.Splash, flow.Screen);
            Assert.IsTrue(flow.SplashDone());
            Assert.AreEqual(AppScreen.Consent, flow.Screen);
            Assert.IsTrue(flow.ConsentDone());
            Assert.AreEqual(AppScreen.Home, flow.Screen);
        }

        [Test]
        public void LaterRun_SkipsConsent()
        {
            var flow = new AppFlow(firstRun: false);

            flow.SplashDone();

            Assert.AreEqual(AppScreen.Home, flow.Screen);
            Assert.IsFalse(flow.ConsentDone());
        }

        [Test]
        public void Pause_OnlyFromPlaying()
        {
            var flow = new AppFlow(false);
            Assert.IsFalse(flow.Pause());
            flow.SplashDone();
            Assert.IsFalse(flow.Pause());

            flow.Play();

            Assert.IsTrue(flow.Pause());
            Assert.AreEqual(AppScreen.Paused, flow.Screen);
            Assert.IsFalse(flow.Pause());
        }

        [Test]
        public void Resume_ReturnsToPlaying()
        {
            var flow = Playing();
            Assert.IsFalse(flow.Resume());
            flow.Pause();

            Assert.IsTrue(flow.Resume());
            Assert.AreEqual(AppScreen.Playing, flow.Screen);
        }

        [Test]
        public void Play_FromSplash_IsRejected()
        {
            var flow = new AppFlow(false);

            Assert.IsFalse(flow.Play());
            Assert.AreEqual(AppScreen.Splash, flow.Screen);
        }

        [Test]
        public void GoHome_FromPlayingOrPaused()
        {
            var flow = Playing();
            Assert.IsTrue(flow.GoHome());
            Assert.AreEqual(AppScreen.Home, flow.Screen);
            Assert.IsFalse(flow.GoHome());

            flow.Play();
            flow.Pause();
            Assert.IsTrue(flow.GoHome());
        }

        [Test]
        public void Changed_FiresOncePerMove()
        {
            var flow = new AppFlow(true);
            var seen = new List<AppScreen>();
            flow.Changed += seen.Add;

            flow.SplashDone();
            flow.SplashDone();
            flow.ConsentDone();
            flow.Play();
            flow.Pause();
            flow.Resume();

            CollectionAssert.AreEqual(
                new[] { AppScreen.Consent, AppScreen.Home, AppScreen.Playing, AppScreen.Paused, AppScreen.Playing }, seen);
        }

        private static AppFlow Playing()
        {
            var flow = new AppFlow(false);
            flow.SplashDone();
            flow.Play();
            return flow;
        }
    }
}
