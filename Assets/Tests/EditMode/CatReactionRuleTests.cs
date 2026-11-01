using Game.Core;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class CatReactionRuleTests
    {
        [Test]
        public void For_WinningShot_Celebrates() =>
            Assert.AreEqual(CatReaction.Celebrate, CatReactionRule.For(0, RoundState.Won));

        [Test]
        public void For_WinWithClears_Celebrates() =>
            Assert.AreEqual(CatReaction.Celebrate, CatReactionRule.For(3, RoundState.Won));

        [Test]
        public void For_Miss_Annoyed() =>
            Assert.AreEqual(CatReaction.Annoyed, CatReactionRule.For(0, RoundState.Playing));

        [Test]
        public void For_HitWhilePlaying_None() =>
            Assert.AreEqual(CatReaction.None, CatReactionRule.For(1, RoundState.Playing));

        [Test]
        public void For_Lost_Annoyed() =>
            Assert.AreEqual(CatReaction.Annoyed, CatReactionRule.For(0, RoundState.Lost));
    }
}
