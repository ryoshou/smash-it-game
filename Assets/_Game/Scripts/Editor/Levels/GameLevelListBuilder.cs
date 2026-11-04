using System;
using System.Collections.Generic;
using Game.Levels;
using UnityEditor;

namespace Game.Editor
{
    /// <summary>
    /// The list the game plays: every room's 25 levels in room order (spec §6.1: 4 rooms, 100 levels). Save indices
    /// 0–24 stay Room 1, so saves from before the later rooms carry over. A room joins once its levels builder exists.
    /// </summary>
    public static class GameLevelListBuilder
    {
        public const string ListPath = "Assets/_Game/Levels/GameLevelList.asset";
        public const int RoomCount = 4;

        /// <summary>Each room's theme and backdrop builder, in room order (room N at index N-1).</summary>
        private static readonly (string ThemePath, Func<RoomTheme> Build)[] Rooms =
        {
            (HomeOfficeBuilder.ThemePath, HomeOfficeBuilder.Build),
            (CafeBuilder.ThemePath, CafeBuilder.Build),
            (LibraryBuilder.ThemePath, LibraryBuilder.Build),
        };

        /// <summary>Each room's levels builder; a room joins the game list once its levels exist.</summary>
        private static readonly Func<LevelList>[] RoomLevels =
        {
            Room1LevelBuilder.BuildAll,
            Room2LevelBuilder.BuildAll,
        };

        public static string RoomFolder(int room) => $"Assets/_Game/Levels/Room{room}";

        public static string RoomListPath(int room) => $"{RoomFolder(room)}/Room{room}LevelList.asset";

        public static string ThemePath(int room) => Rooms[room - 1].ThemePath;

        public static bool HasRoom(int room) => room >= 1 && room <= Rooms.Length;

        /// <summary>Rebuilds every room's backdrop prefab and theme.</summary>
        [MenuItem("Smash/Build All Rooms")]
        public static void BuildRooms()
        {
            foreach (var room in Rooms) room.Build();
        }

        [MenuItem("Smash/Build All Levels")]
        public static LevelList BuildAll()
        {
            var levels = new List<LevelDef>();
            foreach (var build in RoomLevels) levels.AddRange(build().levels);
            var list = LevelAssetWriter.LoadOrCreate<LevelList>(ListPath);
            list.levels = levels.ToArray();
            EditorUtility.SetDirty(list);
            AssetDatabase.SaveAssets();
            return list;
        }
    }
}
