using System.Collections.Generic;

namespace Game.Levels
{
    public enum RoomState { Done, Current, Locked }

    /// <summary>One room on the room map: where its levels sit in the list and how far the save has got through them.</summary>
    public readonly struct RoomEntry
    {
        public readonly int Number;
        public readonly int FirstIndex;
        public readonly int Count;
        public readonly int Cleared;
        public readonly RoomState State;
        public readonly RoomTheme Theme;

        public RoomEntry(int number, int firstIndex, int count, int cleared, RoomState state, RoomTheme theme)
        {
            Number = number;
            FirstIndex = firstIndex;
            Count = count;
            Cleared = cleared;
            State = state;
            Theme = theme;
        }

        public int LastIndex => FirstIndex + Count - 1;
        public bool CanPlay => State != RoomState.Locked;
    }

    /// <summary>
    /// The room map's rule: a room is a run of consecutive levels with the same RoomTheme. Rooms before the saved level
    /// are Done, the one holding it is Current, later ones are Locked. Replaying a Done room starts at its first level;
    /// the save keeps the highest level reached (Profile.RecordWin).
    /// </summary>
    public static class RoomProgress
    {
        public static List<RoomEntry> Rooms(LevelList list, int savedIndex)
        {
            var rooms = new List<RoomEntry>();
            int first = 0;
            while (first < list.Count)
            {
                int next = list.NextRoomStart(first);
                int end = next > first ? next : list.Count; // NextRoomStart wraps to 0 after the last room
                int count = end - first;
                int cleared = System.Math.Max(0, System.Math.Min(count, savedIndex - first));
                var state = savedIndex >= end ? RoomState.Done : savedIndex >= first ? RoomState.Current : RoomState.Locked;
                rooms.Add(new RoomEntry(rooms.Count + 1, first, count, cleared, state, list.levels[first].room));
                first = end;
            }
            return rooms;
        }

        /// <summary>The level a tap on the room plays: the saved level in the current room, else the room's first.</summary>
        public static int PlayIndex(RoomEntry room, int savedIndex) =>
            room.State == RoomState.Current ? savedIndex : room.FirstIndex;
    }
}
