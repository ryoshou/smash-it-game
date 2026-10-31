using Game.Core;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class SlingshotGestureTests
    {
        private const float ScreenHeight = 1000f;
        private const float Tolerance = 1e-4f;
        private static readonly Vector2 Pouch = new Vector2(500f, 200f);

        private ShotTuning tuning;
        private SlingshotGesture gesture;

        [SetUp]
        public void SetUp()
        {
            tuning = ShotTuning.CreateDefault();
            gesture = new SlingshotGesture(tuning);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(tuning);

        private bool Begin(int id, Vector2 pos, bool canGrab = true) =>
            gesture.TryBegin(id, pos, Pouch, ScreenHeight, canGrab);

        [Test]
        public void Begin_NearPouch_Grabs()
        {
            Assert.IsTrue(Begin(1, new Vector2(530f, 190f)));
            Assert.IsTrue(gesture.IsHolding);
            Assert.AreEqual(1, gesture.ActiveTouchId);
        }

        [Test]
        public void Begin_FarFromPouch_DoesNotGrab()
        {
            Assert.IsFalse(Begin(1, new Vector2(500f, 400f)));
            Assert.IsFalse(gesture.IsHolding);
        }

        [Test]
        public void Begin_WhenCannotGrab_DoesNotGrab()
        {
            Assert.IsFalse(Begin(1, new Vector2(530f, 190f), canGrab: false));
            Assert.IsFalse(gesture.IsHolding);
        }

        [Test]
        public void Release_AfterFullPull_Fires()
        {
            Begin(1, new Vector2(530f, 190f));

            gesture.Move(1, new Vector2(500f, -50f));
            Assert.AreEqual(0f, gesture.Pull.x, Tolerance);
            Assert.AreEqual(-0.25f, gesture.Pull.y, Tolerance);
            var result = gesture.Release(1);

            Assert.IsTrue(result.Fired);
            Assert.AreEqual(1f, result.Power01, Tolerance);
            Assert.IsFalse(gesture.IsHolding);
        }

        [Test]
        public void Release_BackInPouch_Cancels()
        {
            Begin(1, new Vector2(530f, 190f));

            gesture.Move(1, new Vector2(505f, 195f));
            var result = gesture.Release(1);

            Assert.IsFalse(result.Fired);
            Assert.IsFalse(gesture.IsHolding);
        }

        [Test]
        public void SecondTouch_IsIgnoredWhileHolding()
        {
            Begin(1, new Vector2(530f, 190f));
            gesture.Move(1, new Vector2(500f, 50f));
            var pullBefore = gesture.Pull;

            Assert.IsFalse(Begin(2, new Vector2(510f, 200f)));
            gesture.Move(2, new Vector2(800f, -300f));
            Assert.AreEqual(pullBefore, gesture.Pull);
            var result = gesture.Release(2);

            Assert.IsFalse(result.Fired);
            Assert.IsTrue(gesture.IsHolding);
            Assert.AreEqual(1, gesture.ActiveTouchId);
        }

        [Test]
        public void Cancel_WhileHolding_ReturnsToIdleWithoutFiring()
        {
            Begin(1, new Vector2(530f, 190f));
            gesture.Move(1, new Vector2(500f, -50f));

            gesture.Cancel();

            Assert.IsFalse(gesture.IsHolding);
            Assert.AreEqual(0f, gesture.Power01);
            Assert.IsFalse(gesture.Release(1).Fired);
        }
    }
}
