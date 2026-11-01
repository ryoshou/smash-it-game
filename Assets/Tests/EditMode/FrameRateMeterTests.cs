using Game.UI;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class FrameRateMeterTests
    {
        [Test]
        public void Average_Of_SteadyFrames()
        {
            var meter = new FrameRateMeter(30);
            for (int i = 0; i < 30; i++) meter.Add(1f / 60f);

            Assert.AreEqual(60f, meter.FramesPerSecond, 0.01f);
        }

        [Test]
        public void Window_ForgetsOldFrames()
        {
            var meter = new FrameRateMeter(4);
            for (int i = 0; i < 4; i++) meter.Add(1f / 10f);
            for (int i = 0; i < 4; i++) meter.Add(1f / 50f);

            Assert.AreEqual(50f, meter.FramesPerSecond, 0.01f);
        }

        [Test]
        public void PartialWindow_AveragesWhatItHas()
        {
            var meter = new FrameRateMeter(30);
            meter.Add(1f / 20f);
            meter.Add(1f / 40f);

            Assert.AreEqual(2f / (1f / 20f + 1f / 40f), meter.FramesPerSecond, 0.01f);
        }

        [Test]
        public void Empty_IsZero() => Assert.AreEqual(0f, new FrameRateMeter(30).FramesPerSecond);
    }
}
