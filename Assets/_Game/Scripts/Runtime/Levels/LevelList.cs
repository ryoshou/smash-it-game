using UnityEngine;

namespace Game.Levels
{
    /// <summary>Play order of the levels.</summary>
    [CreateAssetMenu(menuName = "Smash/Level List")]
    public sealed class LevelList : ScriptableObject
    {
        public LevelDef[] levels = new LevelDef[0];

        public int Count => levels.Length;

        /// <summary>Wraps around, so play loops back to the first level after the last.</summary>
        public LevelDef At(int index) => levels[((index % Count) + Count) % Count];
    }
}
