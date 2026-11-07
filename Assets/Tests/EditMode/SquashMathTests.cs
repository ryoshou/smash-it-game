using Game.Core;
using NUnit.Framework;

namespace Game.Tests
{
    /// <summary>Food squashes flat instead of breaking: flattens, keeps its volume, holds, then shrinks away.</summary>
    public sealed class SquashMathTests
    {
        private const float Flatten = 0.25f, Squash = 0.1f, Hold = 1f, Fade = 0.5f;

        [Test]
        public void Scale_AtImpact_IsFullSize()
        {
            var scale = SquashMath.Scale(0f, Flatten, Squash, Hold, Fade);

            Assert.AreEqual(1f, scale.x, 1e-4f);
            Assert.AreEqual(1f, scale.y, 1e-4f);
        }

        [Test]
        public void Scale_OnceSquashed_IsFlatAndWideKeepingVolume()
        {
            var scale = SquashMath.Scale(Squash + Hold / 2f, Flatten, Squash, Hold, Fade);

            Assert.AreEqual(Flatten, scale.y, 1e-4f);
            Assert.AreEqual(2f, scale.x, 1e-4f, "1 / sqrt(0.25): as wide as it lost in height");
            Assert.AreEqual(1f, scale.x * scale.x * scale.y, 1e-4f);
        }

        [Test]
        public void Scale_HalfwayThroughTheFade_IsHalfTheSquashedSize()
        {
            var scale = SquashMath.Scale(Squash + Hold + Fade / 2f, Flatten, Squash, Hold, Fade);

            Assert.AreEqual(Flatten / 2f, scale.y, 1e-4f);
            Assert.AreEqual(1f, scale.x, 1e-4f);
        }

        [TestCase(1.5f, false)]
        [TestCase(1.6f, true)]
        public void IsDone_AfterSquashHoldAndFade(float age, bool expected) =>
            Assert.AreEqual(expected, SquashMath.IsDone(age, Squash, Hold, Fade));
    }
}
