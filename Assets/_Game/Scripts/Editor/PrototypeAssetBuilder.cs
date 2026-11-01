using System.IO;
using Game.Levels;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>Writes the three prototype levels and their play-order list. Safe to re-run. (Props: PropPrefabBuilder.)</summary>
    public static class PrototypeAssetBuilder
    {
        public const string LevelsFolder = "Assets/_Game/Levels";
        public const string ConfigFolder = "Assets/_Game/Config";
        public const string LevelListPath = LevelsFolder + "/PrototypeLevelList.asset";
        public const string CatalogPath = ConfigFolder + "/PropCatalog.asset";

        [MenuItem("Smash/Rebuild Prototype Assets")]
        public static void RebuildAll()
        {
            Directory.CreateDirectory(LevelsFolder);
            Directory.CreateDirectory(ConfigFolder);

            var list = LoadOrCreate<LevelList>(LevelListPath);
            list.levels = new[]
            {
                WriteLevel("L01_Wall", 6, PrototypeLevels.Wall(5, 4)),
                WriteLevel("L02_Pyramid", 6, PrototypeLevels.Pyramid(5)),
                WriteLevel("L03_TwoTowers", 5, PrototypeLevels.TwoTowers(5)),
            };
            EditorUtility.SetDirty(list);

            AssetDatabase.SaveAssets();
        }

        private static LevelDef WriteLevel(string id, int shots, PropPlacement[] props)
        {
            var level = LoadOrCreate<LevelDef>($"{LevelsFolder}/{id}.asset");
            level.id = id;
            level.shots = shots;
            level.props = props;
            if (!level.Validate(out string error)) throw new System.InvalidOperationException(error);
            EditorUtility.SetDirty(level);
            return level;
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
