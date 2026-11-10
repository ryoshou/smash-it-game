using Game.Core;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>The owner's call: nothing stays stuck between the desks.</summary>
    public sealed class WedgeRuleTests
    {
        private const float MainBackZ = 0.75f;
        private const float BackFrontZ = 0.95f;

        [TestCase(0.76f, true)]   // just past the main desk's back edge
        [TestCase(0.85f, true)]   // over the middle of the gap
        [TestCase(0.94f, true)]   // just short of the back desk's front edge
        [TestCase(0.75f, false)]  // on the edge: still standing on the main desk
        [TestCase(0.5f, false)]   // on the main desk
        [TestCase(1.2f, false)]   // on the back desk
        public void IsOverGap_OnlyBetweenTheDesks(float z, bool expected) =>
            Assert.AreEqual(expected, WedgeRule.IsOverGap(new Vector3(0.3f, 1.2f, z), MainBackZ, BackFrontZ));

        [Test]
        public void IsOverGap_AnyHeight_NothingThereCanHoldAProp() =>
            Assert.IsTrue(WedgeRule.IsOverGap(new Vector3(0f, 1.6f, 0.82f), MainBackZ, BackFrontZ));

        [TestCase(0.59f, false)]
        [TestCase(0.6f, true)]
        public void IsStuck_AfterRestingOverTheGapLongEnough(float restedSeconds, bool expected) =>
            Assert.AreEqual(expected, WedgeRule.IsStuck(restedSeconds, 0.6f));
    }
}
