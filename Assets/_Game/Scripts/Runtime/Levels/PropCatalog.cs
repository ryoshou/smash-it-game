using System.Collections.Generic;
using UnityEngine;

namespace Game.Levels
{
    [CreateAssetMenu(menuName = "Smash/Prop Catalog")]
    public sealed class PropCatalog : ScriptableObject
    {
        public PropSpec[] specs = new PropSpec[0];

        public PropSpec Get(PropKind kind)
        {
            foreach (var spec in specs)
                if (spec.kind == kind) return spec;
            throw new KeyNotFoundException($"PropCatalog '{name}' has no spec for {kind}.");
        }
    }
}
