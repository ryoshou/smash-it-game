using Game.Audio;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class ScaffoldTests
    {
        [Test]
        public void SfxrSynth_RendersNonEmptyClip()
        {
            Assert.Greater(SfxrSynth.Render(new SfxrParams(), 1).Length, 0);
        }
    }
}
