using UnityEngine;

namespace Game.Meta
{
    public sealed class PlayerPrefsSaveStore : ISaveStore
    {
        public const string Key = "smash.save";

        public string Load() => PlayerPrefs.GetString(Key, "");

        public void Save(string json)
        {
            PlayerPrefs.SetString(Key, json);
            PlayerPrefs.Save();
        }
    }
}
