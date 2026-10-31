using System;
using UnityEngine;

namespace Game.Levels
{
    /// <summary>One prop in a layout.</summary>
    [Serializable]
    public struct PropPlacement
    {
        public PropKind kind;

        /// <summary>Bottom-face center, relative to the desk-top center; +z is away from the player.</summary>
        public Vector3 basePosition;

        public float yawDegrees;
        public bool isTarget;
    }
}
