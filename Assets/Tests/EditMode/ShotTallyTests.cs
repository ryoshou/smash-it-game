using Game.Core;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class ShotTallyTests
    {
        private ShotTally tally;

        [SetUp]
        public void SetUp() => tally = new ShotTally();

        [Test]
        public void NoFire_NeverResolves()
        {
            tally.TargetCleared();

            Assert.IsFalse(tally.TryResolve(anyBallActive: false, out _));
        }

        [Test]
        public void Resolve_AfterBallGone_ReportsClearsSinceFire()
        {
            tally.TargetCleared();
            tally.Fired();
            tally.TargetCleared();
            tally.TargetCleared();

            Assert.IsFalse(tally.TryResolve(anyBallActive: true, out _));
            Assert.IsTrue(tally.TryResolve(anyBallActive: false, out int cleared));
            Assert.AreEqual(2, cleared);
        }

        [Test]
        public void ResolvesOncePerVolley()
        {
            tally.Fired();
            Assert.IsTrue(tally.TryResolve(false, out _));

            Assert.IsFalse(tally.TryResolve(false, out _));
        }

        [Test]
        public void SecondShotWhileFlying_JoinsVolley()
        {
            tally.Fired();
            tally.TargetCleared();
            tally.Fired();
            tally.TargetCleared();

            Assert.IsTrue(tally.TryResolve(false, out int cleared));
            Assert.AreEqual(2, cleared);
            Assert.IsFalse(tally.TryResolve(false, out _));
        }

        [Test]
        public void Reset_DropsPendingVolley()
        {
            tally.Fired();
            tally.TargetCleared();

            tally.Reset();

            Assert.IsFalse(tally.TryResolve(false, out _));
        }

        [Test]
        public void NextVolley_StartsFromZero()
        {
            tally.Fired();
            tally.TargetCleared();
            tally.TryResolve(false, out _);

            tally.Fired();

            Assert.IsTrue(tally.TryResolve(false, out int cleared));
            Assert.AreEqual(0, cleared);
        }
    }
}
