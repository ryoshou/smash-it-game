using UnityEngine;

namespace Game.Meta
{
    /// <summary>The player's save. Every change writes through to the store at once, so a kill never loses it.</summary>
    public sealed class Profile
    {
        private readonly ISaveStore store;

        public Profile(ISaveStore store)
        {
            this.store = store;
            Data = SaveCodec.Read(store.Load());
        }

        /// <summary>Read-only use: change it through the setters.</summary>
        public SaveData Data { get; private set; }

        public void RecordWin(int levelIndex)
        {
            Data.levelIndex = Mathf.Max(Data.levelIndex, levelIndex + 1);
            Flush();
        }

        public void SetSound(bool on) { Data.sound = on; Flush(); }
        public void SetMusic(bool on) { Data.music = on; Flush(); }
        public void SetHaptics(bool on) { Data.haptics = on; Flush(); }
        public void SetRemoveAds(bool owned) { Data.removeAds = owned; Flush(); }
        public void MarkFirstRunDone() { Data.firstRunDone = true; Flush(); }

        public void ResetAll()
        {
            Data = new SaveData();
            Flush();
        }

        private void Flush() => store.Save(SaveCodec.Write(Data));
    }
}
