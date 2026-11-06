using Game.Levels;
using NUnit.Framework;

namespace Game.Tests
{
    /// <summary>Spec §6.3: each level's bot clear rate must sit in the band for its role.</summary>
    public sealed class ClearRateBandTests
    {
        [TestCase(Difficulty.Normal, 1, 0.60f, 1.00f)]
        [TestCase(Difficulty.Normal, 10, 0.60f, 1.00f)]
        [TestCase(Difficulty.Normal, 11, 0.35f, 0.95f)]
        [TestCase(Difficulty.Hard, 5, 0.15f, 0.65f)]
        [TestCase(Difficulty.Hard, 25, 0.15f, 0.65f)]
        public void Band_TeachingNormalHard_Values(Difficulty difficulty, int levelNumber, float min, float max)
        {
            var band = ClearRateBand.For(difficulty, levelNumber);

            Assert.AreEqual(min, band.Min, 1e-6f);
            Assert.AreEqual(max, band.Max, 1e-6f);
        }

        /// <summary>Kitchen spec §4: feasts are a reward, checked against the easy band wherever they sit.</summary>
        [TestCase(27)]
        [TestCase(50)]
        [TestCase(5)]
        public void Feast_UsesTheFeastBand_AtAnyLevel(int levelNumber)
        {
            var band = ClearRateBand.For(Difficulty.Feast, levelNumber);

            Assert.AreEqual("feast", band.Name);
            Assert.AreEqual(0.60f, band.Min, 1e-6f);
            Assert.AreEqual(1.00f, band.Max, 1e-6f);
        }

        [Test]
        public void Band_Accepts_IsInclusive()
        {
            var band = ClearRateBand.For(Difficulty.Hard, 5);

            Assert.IsTrue(band.Accepts(0.15f));
            Assert.IsTrue(band.Accepts(0.65f));
            Assert.IsFalse(band.Accepts(0.149f));
            Assert.IsFalse(band.Accepts(0.651f));
        }

        [Test]
        public void Band_Names()
        {
            Assert.AreEqual("teaching", ClearRateBand.For(Difficulty.Normal, 3).Name);
            Assert.AreEqual("normal", ClearRateBand.For(Difficulty.Normal, 13).Name);
            Assert.AreEqual("hard", ClearRateBand.For(Difficulty.Hard, 15).Name);
        }
    }
}
