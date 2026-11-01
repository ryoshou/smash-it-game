using System;
using UnityEngine;

namespace Game.Levels
{
    /// <summary>One look of a prop kind (e.g. a colour) and its pre-fractured set, if it shatters.</summary>
    [Serializable]
    public struct PropVariant
    {
        public GameObject prefab;
        public GameObject brokenPrefab;
    }
}
