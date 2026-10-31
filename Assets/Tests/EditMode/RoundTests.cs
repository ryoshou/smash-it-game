using Game.Core;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class RoundTests
    {
        private const float Hold = 0.6f;
        private const float MaxWait = 5f;

        private RoundJudge judge;

        [SetUp]
        public void SetUp() => judge = new RoundJudge(Hold, MaxWait);

        private RoundState OutOfShots(float now, bool moving, int targets = 3) =>
            judge.Evaluate(now, targets, 0, false, moving);

        [Test]
        public void ShotBudget_ConsumesUntilEmpty()
        {
            var budget = new ShotBudget(2);

            Assert.IsTrue(budget.TryConsume());
            Assert.IsTrue(budget.TryConsume());
            Assert.IsFalse(budget.TryConsume());
            Assert.AreEqual(0, budget.Remaining);
        }

        [Test]
        public void ShotBudget_ContinueOncePerAttempt()
        {
            var budget = new ShotBudget(1);
            budget.TryConsume();

            Assert.IsTrue(budget.TryContinue(3));
            Assert.AreEqual(3, budget.Remaining);
            Assert.IsFalse(budget.CanContinue);
            Assert.IsFalse(budget.TryContinue(3));

            budget.ResetAttempt(5);
            Assert.IsTrue(budget.CanContinue);
            Assert.AreEqual(5, budget.Remaining);
        }

        [Test]
        public void AllTargetsCleared_WinsImmediately()
        {
            Assert.AreEqual(RoundState.Won, judge.Evaluate(0f, 0, 4, true, true));
        }

        [Test]
        public void ShotsLeft_KeepsPlaying()
        {
            Assert.AreEqual(RoundState.Playing, judge.Evaluate(10f, 3, 1, false, false));
        }

        [Test]
        public void OutOfShots_SettledForHold_Lost()
        {
            for (float t = 0f; t < 0.59f; t += 0.05f)
                Assert.AreEqual(RoundState.Playing, OutOfShots(t, false), $"t={t}");
            Assert.AreEqual(RoundState.Playing, OutOfShots(0.59f, false));

            Assert.AreEqual(RoundState.Lost, OutOfShots(0.6f, false));
        }

        [Test]
        public void SlowToppleDuringSettle_Wins()
        {
            Assert.AreEqual(RoundState.Playing, OutOfShots(0f, true));
            Assert.AreEqual(RoundState.Playing, OutOfShots(1f, true));

            Assert.AreEqual(RoundState.Won, OutOfShots(2f, true, targets: 0));
        }

        [Test]
        public void NeverSettles_JudgedLostAtMaxWait()
        {
            Assert.AreEqual(RoundState.Playing, OutOfShots(0f, true));
            Assert.AreEqual(RoundState.Playing, OutOfShots(4.99f, true));

            Assert.AreEqual(RoundState.Lost, OutOfShots(5f, true));
        }

        [Test]
        public void MovementResetsQuietTimer()
        {
            for (float t = 0f; t <= 0.5f; t += 0.05f) OutOfShots(t, false);
            Assert.AreEqual(RoundState.Playing, OutOfShots(0.55f, true));

            Assert.AreEqual(RoundState.Playing, OutOfShots(1f, false));
            Assert.AreEqual(RoundState.Playing, OutOfShots(1.59f, false));
            Assert.AreEqual(RoundState.Lost, OutOfShots(1.6f, false));
        }

        [Test]
        public void Reset_AfterContinue_StartsFresh()
        {
            for (float t = 0f; t <= 0.5f; t += 0.05f) OutOfShots(t, false);

            judge.Reset();

            Assert.AreEqual(RoundState.Playing, OutOfShots(0.6f, false));
            Assert.AreEqual(RoundState.Playing, OutOfShots(1.19f, false));
            Assert.AreEqual(RoundState.Lost, OutOfShots(1.2f, false));
        }

        [Test]
        public void GameEvents_RaiseNotifiesSubscribers()
        {
            var events = new GameEvents();
            float power = -1f;
            Vector3 fired = Vector3.zero;
            bool firstContact = false;
            int remaining = -1;
            RoundState ended = RoundState.Playing;
            events.PullChanged += p => power = p;
            events.Fired += v => fired = v;
            events.Impact += (_, _, first) => firstContact = first;
            events.TargetCleared += r => remaining = r;
            events.RoundEnded += s => ended = s;

            events.RaisePullChanged(0.5f);
            events.RaiseFired(Vector3.forward);
            events.RaiseImpact(Vector3.zero, 3f, true);
            events.RaiseTargetCleared(2);
            events.RaiseRoundEnded(RoundState.Won);

            Assert.AreEqual(0.5f, power);
            Assert.AreEqual(Vector3.forward, fired);
            Assert.IsTrue(firstContact);
            Assert.AreEqual(2, remaining);
            Assert.AreEqual(RoundState.Won, ended);
        }

        [Test]
        public void GameEvents_RaiseWithoutSubscribers_DoesNotThrow()
        {
            var events = new GameEvents();

            Assert.DoesNotThrow(() =>
            {
                events.RaisePullChanged(1f);
                events.RaiseFired(Vector3.zero);
                events.RaiseImpact(Vector3.zero, 0f, false);
                events.RaiseTargetCleared(0);
                events.RaiseRoundEnded(RoundState.Lost);
            });
        }
    }
}
