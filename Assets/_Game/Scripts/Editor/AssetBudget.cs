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
        public const float RoomMaxSize = 8f;
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
            var budget = new AssetBudget { requireAtlasCells = isGenerated };
            if (isGenerated && name.StartsWith("Keyboard")) budget.maxTriangles = KeyboardTriangles;
            else if (isGenerated && name.StartsWith("Cat")) budget.maxTriangles = CatTriangles;
            if (name.StartsWith("RoomShell")) budget.maxSize = RoomMaxSize;
            return budget;
        }
    }
}
