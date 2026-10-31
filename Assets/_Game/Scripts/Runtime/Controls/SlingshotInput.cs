using System;
using Game.Core;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Game.Controls
{
    /// <summary>Routes finger down/move/up to the slingshot gesture. Mouse acts as touch in the editor.</summary>
    public sealed class SlingshotInput : MonoBehaviour
    {
        private SlingshotGesture gesture;
        private Func<bool> canGrab;
        private Transform pouch;
        private Camera cam;

        /// <summary>Raised when the holding finger lifts; Fired is false for a cancelled pull.</summary>
        public event Action<LaunchResult> Released;

        public void Bind(SlingshotGesture gesture, Func<bool> canGrab, Transform pouch, Camera cam)
        {
            this.gesture = gesture;
            this.canGrab = canGrab;
            this.pouch = pouch;
            this.cam = cam;
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
#if UNITY_EDITOR
            TouchSimulation.Enable();
#endif
            ETouch.onFingerDown += OnFingerDown;
            ETouch.onFingerMove += OnFingerMove;
            ETouch.onFingerUp += OnFingerUp;
        }

        private void OnDisable()
        {
            ETouch.onFingerDown -= OnFingerDown;
            ETouch.onFingerMove -= OnFingerMove;
            ETouch.onFingerUp -= OnFingerUp;
#if UNITY_EDITOR
            TouchSimulation.Disable();
#endif
            EnhancedTouchSupport.Disable();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) gesture?.Cancel();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) gesture?.Cancel();
        }

        private void OnFingerDown(Finger finger)
        {
            if (gesture == null) return;
            Vector2 pouchScreen = cam.WorldToScreenPoint(pouch.position);
            gesture.TryBegin(finger.index, finger.screenPosition, pouchScreen, Screen.height, canGrab());
        }

        private void OnFingerMove(Finger finger) => gesture?.Move(finger.index, finger.screenPosition);

        private void OnFingerUp(Finger finger)
        {
            if (gesture == null || !gesture.IsHolding || gesture.ActiveTouchId != finger.index) return;
            // onFingerUp also reports touches the OS cancelled; those must not fire.
            bool isCanceled = finger.lastTouch.phase == UnityEngine.InputSystem.TouchPhase.Canceled;
            Released?.Invoke(gesture.End(finger.index, isCanceled));
        }
    }
}
