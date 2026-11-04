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
        public LevelDef At(int index) => levels[Wrap(index)];

        /// <summary>The first later level whose room differs from this one's; level 1 (index 0) after the last room.</summary>
        public int NextRoomStart(int index)
        {
            int from = Wrap(index);
            var room = levels[from].room;
            for (int i = from + 1; i < Count; i++)
                if (levels[i].room != room) return i;
            return 0;
        }

        private int Wrap(int index) => ((index % Count) + Count) % Count;
    }
}
