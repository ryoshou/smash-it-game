using Game.Core;
using NUnit.Framework;

namespace Game.Tests
{
    public sealed class BreakRulesTests
    {
        private const float Hold = 1.2f;
        private const float Fade = 0.6f;

        [Test]
        public void ShouldBreak_UnbrokenShatterOnFloor_True() =>
            Assert.IsTrue(BreakRule.ShouldBreak(BreakMode.Shatter, isBroken: false, touchedFloor: true));

        [Test]
        public void ShouldBreak_UnbrokenPopOnFloor_True() =>
            Assert.IsTrue(BreakRule.ShouldBreak(BreakMode.Pop, isBroken: false, touchedFloor: true));

        [Test]
        public void ShouldBreak_None_False() =>
            Assert.IsFalse(BreakRule.ShouldBreak(BreakMode.None, isBroken: false, touchedFloor: true));

        [Test]
        public void ShouldBreak_AlreadyBroken_False() =>
            Assert.IsFalse(BreakRule.ShouldBreak(BreakMode.Shatter, isBroken: true, touchedFloor: true));

        [Test]
        public void ShouldBreak_NotFloor_False() =>
            Assert.IsFalse(BreakRule.ShouldBreak(BreakMode.Shatter, isBroken: false, touchedFloor: false));

        [Test]
        public void Scale01_BeforeHold_One()
        {
            Assert.AreEqual(1f, DebrisFade.Scale01(0f, Hold, Fade));
            Assert.AreEqual(1f, DebrisFade.Scale01(Hold, Hold, Fade));
        }

        [Test]
        public void Scale01_MidFade_Between()
        {
            float mid = DebrisFade.Scale01(Hold + Fade / 2f, Hold, Fade);

            Assert.AreEqual(0.5f, mid, 1e-4f);
            Assert.Greater(DebrisFade.Scale01(Hold + Fade * 0.25f, Hold, Fade), mid);
        }

        [Test]
        public void Scale01_AfterFade_Zero()
        {
            Assert.AreEqual(0f, DebrisFade.Scale01(Hold + Fade, Hold, Fade));
            Assert.AreEqual(0f, DebrisFade.Scale01(99f, Hold, Fade));
        }

        [Test]
        public void Scale01_ZeroFade_StepsToZero()
        {
            Assert.AreEqual(1f, DebrisFade.Scale01(Hold, Hold, 0f));
            Assert.AreEqual(0f, DebrisFade.Scale01(Hold + 0.001f, Hold, 0f));
        }

        [Test]
        public void IsDone_OnlyAfterHoldAndFade()
        {
            Assert.IsFalse(DebrisFade.IsDone(Hold + Fade - 0.01f, Hold, Fade));
            Assert.IsTrue(DebrisFade.IsDone(Hold + Fade, Hold, Fade));
        }

        [Test]
        public void Budget_Full_EvictsOldest()
        {
            var budget = new DebrisBudget(2);

            Assert.AreEqual(0, budget.Acquire(out int evicted0));
            Assert.AreEqual(1, budget.Acquire(out int evicted1));
            Assert.AreEqual(0, budget.Acquire(out int evicted2));

            Assert.AreEqual(DebrisBudget.None, evicted0);
            Assert.AreEqual(DebrisBudget.None, evicted1);
            Assert.AreEqual(0, evicted2);
            Assert.AreEqual(2, budget.Active);
        }

        [Test]
        public void Budget_Release_FreesSlot()
        {
            var budget = new DebrisBudget(2);
            budget.Acquire(out _);
            budget.Acquire(out _);

            budget.Release(0);

            Assert.AreEqual(1, budget.Active);
            Assert.AreEqual(0, budget.Acquire(out int evicted));
            Assert.AreEqual(DebrisBudget.None, evicted);
        }

        [Test]
        public void Budget_EvictsOldestStillActive()
        {
            var budget = new DebrisBudget(2);
            budget.Acquire(out _);  // 0
            budget.Acquire(out _);  // 1
            budget.Release(0);
            budget.Acquire(out _);  // 0 again, now newest

            Assert.AreEqual(1, budget.Acquire(out int evicted));
            Assert.AreEqual(1, evicted);
        }

        [Test]
        public void Budget_ReleaseAll_Empties()
        {
            var budget = new DebrisBudget(3);
            budget.Acquire(out _);
            budget.Acquire(out _);

            budget.ReleaseAll();

            Assert.AreEqual(0, budget.Active);
        }
    }
}
