using System.IO;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>The art budget for one model, chosen by its path (spec §12.2).</summary>
    public sealed class AssetBudget
    {
        public const int PropTriangles = 1500;
        public const int KeyboardTriangles = 2500;
        public const int CatTriangles = 8000;
        public const float RoomMaxSize = 8.5f; // an 8 m room plus wall thickness
        public static readonly Vector2Int TinyTreatsGrid = new Vector2Int(8, 4);

        public int maxTriangles = PropTriangles;
        public bool requireAtlasCells;
        public Vector2Int atlasGrid = TinyTreatsGrid;
        public float minSize = 0.02f;
        public float maxSize = 3f;

        public static AssetBudget For(string assetPath)
        {
            string path = assetPath.Replace('\\', '/');
            string name = Path.GetFileNameWithoutExtension(path);
            bool isGenerated = path.Contains("/Art/Generated/");
            // Broken sets cut from pack models keep the pack's own UVs (pack atlases merge cells), so only the
            // budget applies to them; fracture tests check the cut faces' cells.
            bool isPackDerived = name.EndsWith("_broken");
            var budget = new AssetBudget { requireAtlasCells = isGenerated && !isPackDerived };
            if (isGenerated && name.StartsWith("Keyboard")) budget.maxTriangles = KeyboardTriangles;
            else if (isGenerated && name.StartsWith("Cat")) budget.maxTriangles = CatTriangles;
            if (name.StartsWith("RoomShell")) budget.maxSize = RoomMaxSize;
            return budget;
        }
    }
}
