using System;
using Game.Core;
using UnityEngine;

namespace Game.Levels
{
    /// <summary>How a prop kind spawns and behaves: its looks, collider size and shape, mass, physics material, break.</summary>
    [Serializable]
    public struct PropSpec
    {
        public PropKind kind;
        public PropVariant[] variants;
        public Vector3 size;
        public float mass;
        public PhysicsMaterial material;
        public PropColliderShape collider;
        public BreakMode breakMode;

        /// <summary>The variant at index, or the first one when the index is out of range.</summary>
        public PropVariant Variant(int index) => variants[VariantIndex(index)];

        public int VariantIndex(int index) => index >= 0 && index < variants.Length ? index : 0;
    }
}
