using System;

namespace Game.Meta
{
    /// <summary>Everything the game remembers between launches (spec §10). Plain fields for JsonUtility.</summary>
    [Serializable]
    public sealed class SaveData
    {
        public int version = SaveCodec.Version;
        /// <summary>0-based level the player continues from (spec §7: always the highest reached).</summary>
        public int levelIndex;
        public bool sound = true;
        public bool music = true;
        public bool haptics = true;
        public bool removeAds;
        public bool firstRunDone;
    }
}
