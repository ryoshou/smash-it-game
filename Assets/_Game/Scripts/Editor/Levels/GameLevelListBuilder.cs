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

        private static readonly (string ThemePath, Func<LevelList> Build)[] Rooms =
        {
            (HomeOfficeBuilder.ThemePath, Room1LevelBuilder.BuildAll),
        };

        public static string RoomFolder(int room) => $"Assets/_Game/Levels/Room{room}";

        public static string RoomListPath(int room) => $"{RoomFolder(room)}/Room{room}LevelList.asset";

        public static string ThemePath(int room) => Rooms[room - 1].ThemePath;

        [MenuItem("Smash/Build All Levels")]
        public static LevelList BuildAll()
        {
            var levels = new List<LevelDef>();
            foreach (var room in Rooms) levels.AddRange(room.Build().levels);
            var list = LevelAssetWriter.LoadOrCreate<LevelList>(ListPath);
            list.levels = levels.ToArray();
            EditorUtility.SetDirty(list);
            AssetDatabase.SaveAssets();
            return list;
        }
    }
}
