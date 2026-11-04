using Game.Levels;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>Writes one level asset through LevelEditing.Save, so it is validated; a bad layout stops the build.</summary>
    public static class LevelAssetWriter
    {
        public static LevelDef Write(string folder, string id, RoomTheme room, int shots, Difficulty difficulty,
            PropPlacement[] props, bool backDesk = false)
        {
            var level = LoadOrCreate<LevelDef>($"{folder}/{id}.asset");
            level.id = id;
            level.room = room;
            level.hasBackDesk = backDesk;
            string error = LevelEditing.Save(level, props, shots, difficulty);
            if (error != null) throw new System.InvalidOperationException(error);
            return level;
        }

        public static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
