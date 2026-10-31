using Game.UI;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class TripleTapDetectorTests
    {
        private TripleTapDetector detector;

        [SetUp]
        public void SetUp() => detector = new TripleTapDetector(1f);

        [Test]
        public void ThreeTapsInWindow_Triggers()
        {
            Assert.IsFalse(detector.Tap(0f));
            Assert.IsFalse(detector.Tap(0.3f));
            Assert.IsTrue(detector.Tap(0.6f));
        }

        [Test]
        public void SlowTaps_DoNotTrigger()
        {
            Assert.IsFalse(detector.Tap(0f));
            Assert.IsFalse(detector.Tap(0.6f));
            Assert.IsFalse(detector.Tap(1.2f));
        }

        [Test]
        public void ResetsAfterTrigger()
        {
            detector.Tap(0f);
            detector.Tap(0.3f);
            detector.Tap(0.6f);

            Assert.IsFalse(detector.Tap(0.7f));
        }
    }
}
