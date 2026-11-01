using System.Collections.Generic;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>The art rules every imported model must meet: triangle budget, one material, sane size, atlas-cell UVs.</summary>
    public static class ArtCheck
    {
        /// <returns>True when clean; otherwise appends one message per broken rule.</returns>
        public static bool Check(in ArtMeshStats stats, AssetBudget budget, List<string> errors)
        {
            int before = errors.Count;
            if (stats.triangles > budget.maxTriangles)
                errors.Add($"triangles {stats.triangles} > budget {budget.maxTriangles}");
            if (stats.materialCount != 1)
                errors.Add($"materials {stats.materialCount}, expected 1");
            float largest = Mathf.Max(stats.boundsSize.x, Mathf.Max(stats.boundsSize.y, stats.boundsSize.z));
            if (largest < budget.minSize || largest > budget.maxSize)
                errors.Add($"size {largest:0.###} outside {budget.minSize}..{budget.maxSize} (wrong import scale?)");
            if (budget.requireAtlasCells)
            {
                int straddling = CountStraddling(stats, budget.atlasGrid);
                if (straddling > 0)
                    errors.Add($"{straddling} triangles have UVs across atlas cells ({budget.atlasGrid.x}x{budget.atlasGrid.y})");
            }
            return errors.Count == before;
        }

        private static int CountStraddling(in ArtMeshStats stats, Vector2Int grid)
        {
            int count = 0;
            var indices = stats.triangleIndices;
            for (int i = 0; i + 2 < indices.Length; i += 3)
            {
                var cell = Cell(stats.uvs[indices[i]], grid);
                if (Cell(stats.uvs[indices[i + 1]], grid) != cell || Cell(stats.uvs[indices[i + 2]], grid) != cell) count++;
            }
            return count;
        }

        private static Vector2Int Cell(Vector2 uv, Vector2Int grid) =>
            new Vector2Int(CellIndex(uv.x, grid.x), CellIndex(uv.y, grid.y));

        // Generators inset UVs from cell edges, so a plain floor is unambiguous; the clamp keeps u = 1 in the last cell.
        private static int CellIndex(float t, int cells) => Mathf.Clamp(Mathf.FloorToInt(t * cells), 0, cells - 1);
    }
}
