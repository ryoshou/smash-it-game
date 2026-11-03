using System;
using UnityEngine;

namespace Game.Levels
{
    /// <summary>One prop in a layout.</summary>
    [Serializable]
    public struct PropPlacement
    {
        public PropKind kind;

        /// <summary>Bottom-face center, relative to its desk's top center; +z is away from the player.</summary>
        public Vector3 basePosition;

        public float yawDegrees;
        public bool isTarget;

        [Tooltip("Which look (e.g. colour) of the kind; out of range uses the first.")]
        public int variant;

        [Tooltip("0 = main desk, 1 = the raised back desk.")]
        public int desk;
    }
}
