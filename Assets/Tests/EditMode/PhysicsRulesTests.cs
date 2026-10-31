using Game.Core;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class PhysicsRulesTests
    {
        private const float Tolerance = 1e-5f;
        private const float DeskTopY = 1f;
        private const float ClearMargin = 0.15f;
        private static readonly Bounds PlayVolume = new Bounds(new Vector3(0f, 2f, 0f), new Vector3(8f, 6f, 8f));

        private ShotTuning tuning;

        [SetUp]
        public void SetUp() => tuning = ShotTuning.CreateDefault();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(tuning);

        [Test]
        public void BoostImpulse_AddsExtraMomentum()
        {
            AssertVector(new Vector3(0f, 0f, 1.2f), ImpactMath.BoostImpulse(new Vector3(0f, 0f, 8f), 0.25f, 1.6f));
            AssertVector(Vector3.zero, ImpactMath.BoostImpulse(new Vector3(0f, 0f, 8f), 0.25f, 1f));
        }

        [Test]
        public void BurstImpulse_FallsOffLinearly()
        {
            var impulse = ImpactMath.BurstImpulse(Vector3.zero, new Vector3(0.175f, 0f, 0f), 0.6f, 0.35f);

            AssertVector(new Vector3(0.3f, 0f, 0f), impulse);
        }

        [Test]
        public void BurstImpulse_OutsideRadius_IsZero()
        {
            var impulse = ImpactMath.BurstImpulse(Vector3.zero, new Vector3(0.5f, 0f, 0f), 0.6f, 0.35f);

            AssertVector(Vector3.zero, impulse);
        }

        [Test]
        public void BurstImpulse_AtCenter_PushesUp()
        {
            var impulse = ImpactMath.BurstImpulse(Vector3.zero, Vector3.zero, 0.6f, 0.35f);

            AssertVector(new Vector3(0f, 0.6f, 0f), impulse);
        }

        [Test]
        public void CapMagnitude_LimitsLongVectors_KeepsShortOnes()
        {
            AssertVector(new Vector3(0f, 0f, 6f), ImpactMath.CapMagnitude(new Vector3(0f, 0f, 10f), 6f));
            AssertVector(new Vector3(1f, 0f, 0f), ImpactMath.CapMagnitude(new Vector3(1f, 0f, 0f), 6f));
        }

        [Test]
        public void IsMoving_UsesBothThresholds()
        {
            Assert.IsFalse(SettleMath.IsMoving(new Vector3(0.04f, 0f, 0f), Vector3.zero, 0.05f, 0.25f));
            Assert.IsTrue(SettleMath.IsMoving(new Vector3(0.06f, 0f, 0f), Vector3.zero, 0.05f, 0.25f));
            Assert.IsTrue(SettleMath.IsMoving(Vector3.zero, new Vector3(0f, 0.3f, 0f), 0.05f, 0.25f));
        }

        [Test]
        public void IsCleared_BelowDeskTopMinusMargin()
        {
            Assert.IsTrue(ClearRule.IsCleared(new Vector3(0f, 0.84f, 0f), DeskTopY, ClearMargin, PlayVolume));
            Assert.IsFalse(ClearRule.IsCleared(new Vector3(0f, 0.86f, 0f), DeskTopY, ClearMargin, PlayVolume));
        }

        [Test]
        public void IsCleared_OutsidePlayVolume()
        {
            Assert.IsTrue(ClearRule.IsCleared(new Vector3(5f, 1.5f, 0f), DeskTopY, ClearMargin, PlayVolume));
        }

        [Test]
        public void ShouldDespawn_AfterLifetime()
        {
            Assert.IsTrue(BallLife.ShouldDespawn(4f, 5f, 1.5f, tuning));
        }

        [Test]
        public void ShouldDespawn_WhenBelowKillY()
        {
            Assert.IsTrue(BallLife.ShouldDespawn(0.2f, 5f, -1.1f, tuning));
        }

        [Test]
        public void ShouldDespawn_WhenRestingAfterMinAge()
        {
            Assert.IsTrue(BallLife.ShouldDespawn(1.2f, 0.1f, 1.1f, tuning));
        }

        [Test]
        public void ShouldNotDespawn_SlowRightAfterLaunch()
        {
            Assert.IsFalse(BallLife.ShouldDespawn(0.5f, 0.1f, 1.1f, tuning));
        }

        private static void AssertVector(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance, "x");
            Assert.AreEqual(expected.y, actual.y, Tolerance, "y");
            Assert.AreEqual(expected.z, actual.z, Tolerance, "z");
        }
    }
}
