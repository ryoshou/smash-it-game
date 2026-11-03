using System;
using UnityEngine;

namespace Game.Meta
{
    /// <summary>SaveData ⇄ JSON. Never throws: a missing or corrupt save reads as defaults.</summary>
    public static class SaveCodec
    {
        public const int Version = 1;

        public static string Write(SaveData data) => JsonUtility.ToJson(data);

        public static SaveData Read(string json)
        {
            if (string.IsNullOrEmpty(json)) return new SaveData();
            SaveData data;
            try
            {
                data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Unreadable save, starting fresh: {e.Message}");
                return new SaveData();
            }
            data.levelIndex = Mathf.Max(0, data.levelIndex);
            return data;
        }
    }
}
