using System;
using Game.Core;
using Game.Levels;
using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// A prop on the desk: caps its speeds, latches "cleared" once it falls off or leaves the play volume, raises
    /// impact events, and breaks the first time it touches the floor.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class DeskObject : MonoBehaviour
    {
        private float deskTopY;
        private Bounds playVolume;
        private ShotTuning tuning;
        private GameEvents events;

        public event Action<DeskObject> Cleared;

        /// <summary>The prop broke, with how it should disappear (leaving the play volume always pops).</summary>
        public event Action<DeskObject, BreakMode> Broken;

        public bool IsTarget { get; private set; }
        public bool IsCleared { get; private set; }
        public bool IsBroken { get; private set; }
        public PropKind Kind { get; private set; }
        public int Variant { get; private set; }
        public BreakMode BreakMode { get; private set; }
        public Rigidbody Body { get; private set; }

        public void Init(bool isTarget, float deskTopY, Bounds playVolume, ShotTuning t, PropKind kind, int variant,
            BreakMode mode, GameEvents gameEvents)
        {
            IsTarget = isTarget;
            IsCleared = false;
            IsBroken = false;
            this.deskTopY = deskTopY;
            this.playVolume = playVolume;
            tuning = t;
            Kind = kind;
            Variant = variant;
            BreakMode = mode;
            events = gameEvents;
        }

        public bool IsMoving() =>
            SettleMath.IsMoving(Body.linearVelocity, Body.angularVelocity,
                tuning.settleLinearThreshold, tuning.settleAngularThreshold);

        private void Awake() => Body = GetComponent<Rigidbody>();

        private void FixedUpdate()
        {
            if (tuning == null || IsBroken) return;
            // Write only when over the cap: any velocity write wakes the body, and settled props must be able to sleep.
            var linear = Body.linearVelocity;
            if (linear.sqrMagnitude > tuning.objectMaxLinearSpeed * tuning.objectMaxLinearSpeed)
                Body.linearVelocity = ImpactMath.CapMagnitude(linear, tuning.objectMaxLinearSpeed);
            var angular = Body.angularVelocity;
            if (angular.sqrMagnitude > tuning.objectMaxAngularSpeed * tuning.objectMaxAngularSpeed)
                Body.angularVelocity = ImpactMath.CapMagnitude(angular, tuning.objectMaxAngularSpeed);

            if (!IsCleared && ClearRule.IsCleared(Body.position, deskTopY, tuning.clearMargin, playVolume)) LatchCleared();
            if (!playVolume.Contains(Body.position)) Break(BreakMode.Pop, raiseEvent: false);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (tuning == null || IsBroken) return;
            float speed = collision.relativeVelocity.magnitude;
            if (events != null && speed >= tuning.propImpactMinSpeed)
                events.RaisePropImpact(Kind, collision.GetContact(0).point, speed);
            bool touchedFloor = BreakMode != BreakMode.None && collision.collider.TryGetComponent(out FloorSurface _);
            if (BreakRule.ShouldBreak(BreakMode, IsBroken, touchedFloor)) Break(BreakMode, raiseEvent: true);
        }

        private void Break(BreakMode mode, bool raiseEvent)
        {
            IsBroken = true;
            // A prop can reach the floor in the same step it would have been marked cleared: count it exactly once.
            if (!IsCleared) LatchCleared();
            if (raiseEvent) events?.RaiseBroke(Kind, Body.position, mode);
            Broken?.Invoke(this, mode);
        }

        private void LatchCleared()
        {
            IsCleared = true;
            Cleared?.Invoke(this);
        }
    }
}
