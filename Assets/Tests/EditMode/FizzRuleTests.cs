using Game.Core;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class FizzRuleTests
    {
        private const float Tilt = 55f;

        [Test]
        public void ShouldArm_Upright_False() => Assert.IsFalse(FizzRule.ShouldArm(Vector3.up, false, Tilt));

        [Test]
        public void ShouldArm_Wobbling_False() =>
            Assert.IsFalse(FizzRule.ShouldArm(Quaternion.Euler(30f, 0f, 0f) * Vector3.up, false, Tilt));

        [Test]
        public void ShouldArm_TiltedPastThreshold_True() =>
            Assert.IsTrue(FizzRule.ShouldArm(Quaternion.Euler(0f, 0f, 60f) * Vector3.up, false, Tilt));

        [Test]
        public void ShouldArm_TouchedFloor_True() => Assert.IsTrue(FizzRule.ShouldArm(Vector3.up, true, Tilt));

        [Test]
        public void FuseDone_BeforeAndAfter()
        {
            Assert.IsFalse(FizzRule.FuseDone(1f, 1.2f, 0.25f));
            Assert.IsTrue(FizzRule.FuseDone(1f, 1.25f, 0.25f));
        }

        [Test]
        public void BreakRule_Burst_LeavesItToTheFuse() => Assert.IsFalse(BreakRule.ShouldBreak(BreakMode.Burst, false, true));
    }
}
