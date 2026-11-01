using Game.Core;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class ShotTallyTests
    {
        private const float Hold = 0.6f;
        private const float MaxWait = 5f;

        private ShotTally tally;

        [SetUp]
        public void SetUp() => tally = new ShotTally(Hold, MaxWait);

        [Test]
        public void NoFire_NeverResolves()
        {
            tally.TargetCleared();

            Assert.IsFalse(Quiet(10f, out _));
        }

        [Test]
        public void Resolve_AfterBallGoneAndDeskQuiet_ReportsClearsSinceFire()
        {
            tally.TargetCleared();
            tally.Fired();
            tally.TargetCleared();
            tally.TargetCleared();

            Assert.IsFalse(tally.TryResolve(1f, anyBallActive: true, anyMoving: false, out _));
            Assert.IsFalse(Quiet(1.1f, out _));
            Assert.IsTrue(Quiet(1.1f + Hold, out int cleared));
            Assert.AreEqual(2, cleared);
        }

        [Test]
        public void Resolve_WaitsForPropsStillFallingAfterTheBallIsGone()
        {
            // The ball came to rest at t=1 but a mug is still sliding; it falls off at t=1.4 and clears.
            tally.Fired();
            Assert.IsFalse(tally.TryResolve(1f, false, anyMoving: true, out _));
            Assert.IsFalse(tally.TryResolve(1.3f, false, anyMoving: true, out _));
            tally.TargetCleared();
            Assert.IsFalse(Quiet(1.4f, out _));

            Assert.IsTrue(Quiet(1.4f + Hold, out int cleared));
            Assert.AreEqual(1, cleared, "the slow topple counts for this shot");
        }

        [Test]
        public void Resolve_AfterMaxWait_EvenIfThePileKeepsJittering()
        {
            tally.Fired();
            Assert.IsFalse(tally.TryResolve(1f, false, anyMoving: true, out _));

            Assert.IsTrue(tally.TryResolve(1f + MaxWait, false, anyMoving: true, out _));
        }

        [Test]
        public void ResolvesOncePerVolley()
        {
            tally.Fired();
            Quiet(0f, out _);
            Assert.IsTrue(Quiet(Hold, out _));

            Assert.IsFalse(Quiet(2f * Hold, out _));
        }

        [Test]
        public void SecondShotWhileFlying_JoinsVolley()
        {
            tally.Fired();
            tally.TargetCleared();
            tally.Fired();
            tally.TargetCleared();
            Quiet(0f, out _);

            Assert.IsTrue(Quiet(Hold, out int cleared));
            Assert.AreEqual(2, cleared);
        }

        [Test]
        public void BallBackInPlay_RestartsTheWait()
        {
            tally.Fired();
            Quiet(0f, out _);
            tally.Fired();
            Assert.IsFalse(tally.TryResolve(0.5f, anyBallActive: true, anyMoving: false, out _));

            Assert.IsFalse(Quiet(0.6f, out _));
            Assert.IsTrue(Quiet(0.6f + Hold, out _));
        }

        [Test]
        public void Reset_DropsPendingVolley()
        {
            tally.Fired();
            tally.TargetCleared();

            tally.Reset();

            Assert.IsFalse(Quiet(0f, out _));
            Assert.IsFalse(Quiet(10f, out _));
        }

        [Test]
        public void NextVolley_StartsFromZero()
        {
            tally.Fired();
            tally.TargetCleared();
            Quiet(0f, out _);
            Quiet(Hold, out _);

            tally.Fired();
            Quiet(5f, out _);

            Assert.IsTrue(Quiet(5f + Hold + 0.01f, out int cleared)); // past the hold: 5.6f - 5f rounds below 0.6f
            Assert.AreEqual(0, cleared);
        }

        private bool Quiet(float now, out int cleared) => tally.TryResolve(now, false, false, out cleared);
    }
}
