using System;
using Game.Levels;
using UnityEngine;

namespace Game.Core
{
    /// <summary>Gameplay raises these; feedback (Juice, UI) only listens.</summary>
    public sealed class GameEvents
    {
        /// <summary>Pull power 0–1.</summary>
        public event Action<float> PullChanged;

        /// <summary>Launch velocity.</summary>
        public event Action<Vector3> Fired;

        /// <summary>Contact point, impact speed, whether it is the ball's first contact.</summary>
        public event Action<Vector3, float, bool> Impact;

        /// <summary>Targets remaining.</summary>
        public event Action<int> TargetCleared;

        public event Action<RoundState> RoundEnded;

        /// <summary>A volley ended (no ball in play): how many targets it cleared.</summary>
        public event Action<int> ShotResolved;

        /// <summary>A prop hit something: its kind, contact point, relative speed.</summary>
        public event Action<PropKind, Vector3, float> PropImpact;

        /// <summary>A prop broke on the floor: its kind, position, how it broke.</summary>
        public event Action<PropKind, Vector3, BreakMode> Broke;

        public void RaisePullChanged(float power01) => PullChanged?.Invoke(power01);
        public void RaiseFired(Vector3 velocity) => Fired?.Invoke(velocity);
        public void RaiseImpact(Vector3 point, float speed, bool isFirstContact) => Impact?.Invoke(point, speed, isFirstContact);
        public void RaiseTargetCleared(int remaining) => TargetCleared?.Invoke(remaining);
        public void RaiseRoundEnded(RoundState state) => RoundEnded?.Invoke(state);
        public void RaiseShotResolved(int cleared) => ShotResolved?.Invoke(cleared);
        public void RaisePropImpact(PropKind kind, Vector3 point, float speed) => PropImpact?.Invoke(kind, point, speed);
        public void RaiseBroke(PropKind kind, Vector3 position, BreakMode mode) => Broke?.Invoke(kind, position, mode);
    }
}
