using System;
using Game.Core;
using UnityEngine;

namespace Game.World
{
    /// <summary>A prop on the desk: caps its speeds and latches "cleared" once it falls off or leaves the play volume.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class DeskObject : MonoBehaviour
    {
        private float deskTopY;
        private Bounds playVolume;
        private ShotTuning tuning;

        public event Action<DeskObject> Cleared;

        public bool IsTarget { get; private set; }
        public bool IsCleared { get; private set; }
        public Rigidbody Body { get; private set; }

        public void Init(bool isTarget, float deskTopY, Bounds playVolume, ShotTuning t)
        {
            IsTarget = isTarget;
            IsCleared = false;
            this.deskTopY = deskTopY;
            this.playVolume = playVolume;
            tuning = t;
        }

        public bool IsMoving() =>
            SettleMath.IsMoving(Body.linearVelocity, Body.angularVelocity,
                tuning.settleLinearThreshold, tuning.settleAngularThreshold);

        private void Awake() => Body = GetComponent<Rigidbody>();

        private void FixedUpdate()
        {
            if (tuning == null) return;
            Body.linearVelocity = ImpactMath.CapMagnitude(Body.linearVelocity, tuning.objectMaxLinearSpeed);
            Body.angularVelocity = ImpactMath.CapMagnitude(Body.angularVelocity, tuning.objectMaxAngularSpeed);

            if (IsCleared || !ClearRule.IsCleared(Body.position, deskTopY, tuning.clearMargin, playVolume)) return;
            IsCleared = true;
            Cleared?.Invoke(this);
        }
    }
}
