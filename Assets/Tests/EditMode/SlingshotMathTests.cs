using Game.Core;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class SlingshotMathTests
    {
        private const float Tolerance = 1e-3f;

        private ShotTuning tuning;

        [SetUp]
        public void SetUp() => tuning = ShotTuning.CreateDefault();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(tuning);

        [Test]
        public void Compute_FullStraightPull_GivesMaxSpeedAndElevation()
        {
            var result = SlingshotMath.Compute(new Vector2(0f, -0.25f), tuning);

            Assert.IsTrue(result.Fired);
            Assert.AreEqual(1f, result.Power01, Tolerance);
            AssertVector(new Vector3(0f, 3.371f, 8.345f), result.Velocity);
        }

        [Test]
        public void Compute_OverPull_IsClampedToMax()
        {
            var full = SlingshotMath.Compute(new Vector2(0f, -0.25f), tuning);

            var over = SlingshotMath.Compute(new Vector2(0f, -0.6f), tuning);

            AssertVector(full.Velocity, over.Velocity);
        }

        [Test]
        public void Compute_HalfPower_InterpolatesSpeedAndElevation()
        {
            var result = SlingshotMath.Compute(new Vector2(0f, -0.14f), tuning);

            Assert.AreEqual(0.5f, result.Power01, Tolerance);
            Assert.AreEqual(6.5f, result.Velocity.magnitude, Tolerance);
            Assert.AreEqual(15f, ElevationDeg(result.Velocity), Tolerance);
        }

        [Test]
        public void Compute_DragLeft_AimsRight()
        {
            var result = SlingshotMath.Compute(new Vector2(-0.05f, -0.2f), tuning);

            Assert.Greater(result.Velocity.x, 0f);
            Assert.AreEqual(14.036f, YawDeg(result.Velocity), Tolerance);
        }

        [Test]
        public void Compute_WideAngle_IsClampedToMaxYaw()
        {
            var result = SlingshotMath.Compute(new Vector2(-0.2f, -0.1f), tuning);

            Assert.AreEqual(25f, YawDeg(result.Velocity), Tolerance);
        }

        [Test]
        public void Compute_NearlySidewaysDrag_Cancels()
        {
            Assert.IsFalse(SlingshotMath.Compute(new Vector2(-0.2f, -0.001f), tuning).Fired);
            Assert.IsFalse(SlingshotMath.Compute(new Vector2(0.2f, -0.001f), tuning).Fired);
        }

        [Test]
        public void Compute_SteepSideDrag_InsideCone_StillFires()
        {
            Assert.IsTrue(SlingshotMath.Compute(new Vector2(0.2f, -0.05f), tuning).Fired);
        }

        [Test]
        public void Compute_UpwardPull_Cancels()
        {
            var result = SlingshotMath.Compute(new Vector2(0.05f, 0.1f), tuning);

            Assert.IsFalse(result.Fired);
        }

        [Test]
        public void Compute_TinyPull_Cancels()
        {
            var result = SlingshotMath.Compute(new Vector2(0f, -0.02f), tuning);

            Assert.IsFalse(result.Fired);
        }

        [Test]
        public void Compute_SamePull_SameVelocity()
        {
            var pull = new Vector2(0.03f, -0.18f);

            var first = SlingshotMath.Compute(pull, tuning);
            var second = SlingshotMath.Compute(pull, tuning);

            Assert.IsTrue(first.Velocity.Equals(second.Velocity));
        }

        private static float ElevationDeg(Vector3 v) =>
            Mathf.Atan2(v.y, new Vector2(v.x, v.z).magnitude) * Mathf.Rad2Deg;

        private static float YawDeg(Vector3 v) => Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg;

        private static void AssertVector(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance, "x");
            Assert.AreEqual(expected.y, actual.y, Tolerance, "y");
            Assert.AreEqual(expected.z, actual.z, Tolerance, "z");
        }

        [Test]
        public void PullFor_RoundTripsThroughCompute()
        {
            var result = SlingshotMath.Compute(SlingshotMath.PullFor(10f * Mathf.Deg2Rad, 0.5f, tuning), tuning);

            Assert.IsTrue(result.Fired);
            float yawDeg = Mathf.Atan2(result.Velocity.x, result.Velocity.z) * Mathf.Rad2Deg;
            Assert.AreEqual(10f, yawDeg, 0.1f);
            Assert.AreEqual(0.5f, result.Power01, 1e-3f);
        }

        [Test]
        public void PullFor_ClampsYaw()
        {
            var pull = SlingshotMath.PullFor(80f * Mathf.Deg2Rad, 1f, tuning);
            var result = SlingshotMath.Compute(pull, tuning);

            float yawDeg = Mathf.Atan2(result.Velocity.x, result.Velocity.z) * Mathf.Rad2Deg;
            Assert.AreEqual(tuning.maxYawDeg, yawDeg, 0.1f);
            Assert.AreEqual(tuning.maxPull, pull.magnitude, 1e-5f);
        }
    }
}
