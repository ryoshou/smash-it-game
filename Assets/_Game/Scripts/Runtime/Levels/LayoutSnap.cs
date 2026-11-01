using System.Collections.Generic;
using UnityEngine;

namespace Game.Levels
{
    /// <summary>Level-editor placement rules: snap to a grid on the desk and stack on whatever is underneath.</summary>
    public static class LayoutSnap
    {
        public const float DefaultGrid = 0.05f;
        private const float OverlapEpsilon = 1e-4f;

        public static Vector3 SnapXZ(Vector3 basePosition, float grid) =>
            new Vector3(Mathf.Round(basePosition.x / grid) * grid, basePosition.y, Mathf.Round(basePosition.z / grid) * grid);

        /// <summary>Top of the highest prop whose (yaw-rotated) footprint overlaps this one; 0 on the bare desk.</summary>
        public static float StackHeight(Vector2 centerXZ, Vector2 footprint, float yawDegrees,
            IReadOnlyList<PropPlacement> others, PropCatalog catalog)
        {
            var half = HalfExtents(footprint, yawDegrees);
            float top = 0f;
            for (int i = 0; i < others.Count; i++)
            {
                var other = others[i];
                var size = catalog.Get(other.kind).size;
                var otherHalf = HalfExtents(new Vector2(size.x, size.z), other.yawDegrees);
                bool overlapsX = Mathf.Abs(other.basePosition.x - centerXZ.x) < half.x + otherHalf.x - OverlapEpsilon;
                bool overlapsZ = Mathf.Abs(other.basePosition.z - centerXZ.y) < half.y + otherHalf.y - OverlapEpsilon;
                if (overlapsX && overlapsZ) top = Mathf.Max(top, other.basePosition.y + size.y);
            }
            return top;
        }

        /// <summary>Half extents of the axis-aligned box around a footprint turned by yaw.</summary>
        private static Vector2 HalfExtents(Vector2 footprint, float yawDegrees)
        {
            float rad = yawDegrees * Mathf.Deg2Rad;
            float c = Mathf.Abs(Mathf.Cos(rad));
            float s = Mathf.Abs(Mathf.Sin(rad));
            return new Vector2(c * footprint.x + s * footprint.y, s * footprint.x + c * footprint.y) / 2f;
        }
    }
}
