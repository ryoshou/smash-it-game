using System;
using UnityEngine;

namespace Game.Levels
{
    /// <summary>How a prop kind spawns: prefab, collider size, mass and physics material.</summary>
    [Serializable]
    public struct PropSpec
    {
        public PropKind kind;
        public GameObject prefab;
        public Vector3 size;
        public float mass;
        public PhysicsMaterial material;
    }
}
