namespace Game.Meta
{
    /// <summary>Where the save JSON lives. PlayerPrefs on device, in-memory in tests.</summary>
    public interface ISaveStore
    {
        string Load();
        void Save(string json);
    }
}
