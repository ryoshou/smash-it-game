using UnityEngine;

namespace Game.Core
{
    /// <summary>One-finger grab, pull and release on the pouch. Other touches are ignored while holding.</summary>
    public sealed class SlingshotGesture
    {
        private const int NoTouch = -1;

        private readonly ShotTuning tuning;
        private Vector2 pouchScreenPos;
        private float screenHeight;

        public SlingshotGesture(ShotTuning t) => tuning = t;

        public bool IsHolding { get; private set; }
        public int ActiveTouchId { get; private set; } = NoTouch;

        /// <summary>(finger − pouch) / screen height, length clamped to maxPull.</summary>
        public Vector2 Pull { get; private set; }

        public float Power01
        {
            get
            {
                if (!IsHolding) return 0f;
                var result = SlingshotMath.Compute(Pull, tuning);
                return result.Fired ? result.Power01 : 0f;
            }
        }

        public bool TryBegin(int touchId, Vector2 screenPos, Vector2 pouchScreenPos, float screenHeight, bool canGrab)
        {
            if (!canGrab || IsHolding || screenHeight <= 0f) return false;
            if ((screenPos - pouchScreenPos).magnitude > tuning.grabRadius * screenHeight) return false;

            this.pouchScreenPos = pouchScreenPos;
            this.screenHeight = screenHeight;
            IsHolding = true;
            ActiveTouchId = touchId;
            Pull = Vector2.zero;
            Move(touchId, screenPos);
            return true;
        }

        public void Move(int touchId, Vector2 screenPos)
        {
            if (!IsHolding || touchId != ActiveTouchId) return;
            Pull = Vector2.ClampMagnitude((screenPos - pouchScreenPos) / screenHeight, tuning.maxPull);
        }

        public LaunchResult Release(int touchId)
        {
            if (!IsHolding || touchId != ActiveTouchId) return LaunchResult.None;
            var result = SlingshotMath.Compute(Pull, tuning);
            Cancel();
            return result;
        }

        public void Cancel()
        {
            IsHolding = false;
            ActiveTouchId = NoTouch;
            Pull = Vector2.zero;
        }
    }
}
