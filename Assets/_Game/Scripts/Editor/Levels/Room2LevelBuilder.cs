using System.IO;
using Game.Levels;
using UnityEditor;

namespace Game.Editor
{
    /// <summary>
    /// Writes Room 2 (Café, levels 26–50): the level assets and their list (layouts in Room2Layouts). Written through
    /// LevelAssetWriter, so each is validated. Safe to re-run. After changing a layout, re-run
    /// SMASH_BOT_ROOMS=2 tools/unity/playtest-bot.sh.
    /// </summary>
    public static class Room2LevelBuilder
    {
        private const int Room = 2;
        private const Difficulty N = Difficulty.Normal;
        private const Difficulty H = Difficulty.Hard;

        [MenuItem("Smash/Build Room 2 Levels")]
        public static LevelList BuildAll()
        {
            Directory.CreateDirectory(GameLevelListBuilder.RoomFolder(Room));
            var room = AssetDatabase.LoadAssetAtPath<RoomTheme>(CafeBuilder.ThemePath) ?? CafeBuilder.Build();
            var list = LevelAssetWriter.LoadOrCreate<LevelList>(GameLevelListBuilder.RoomListPath(Room));
            list.levels = new[]
            {
                Write("R2_L26", room, 5, N, Room2Layouts.PlatePlinths()),
                Write("R2_L27", room, 5, N, Room2Layouts.PlateStacks()),
                Write("R2_L28", room, 6, N, Room2Layouts.MugsOnPlates()),
                Write("R2_L29", room, 7, N, Room2Layouts.BackPlatesFrontCups(), backDesk: true),
                Write("R2_L30", room, 6, H, Room2Layouts.CappedTowers()),
                Write("R2_L31", room, 5, N, Room2Layouts.CupPyramid()),
                Write("R2_L32", room, 7, N, Room2Layouts.BookWallLob()),
                Write("R2_L33", room, 7, N, Room2Layouts.LampBehindPlates(), backDesk: true),
                Write("R2_L34", room, 7, N, Room2Layouts.PlateBridges()),
                Write("R2_L35", room, 6, H, Room2Layouts.WideCafeSpread()),
                Write("R2_L36", room, 5, N, Room2Layouts.DonutStacks()),
                Write("R2_L37", room, 6, N, Room2Layouts.DonutsOnPlates()),
                Write("R2_L38", room, 7, N, Room2Layouts.DonutTowersTwoDesks(), backDesk: true),
                Write("R2_L39", room, 6, N, Room2Layouts.DonutRow()),
                Write("R2_L40", room, 9, H, Room2Layouts.DonutPyramidTwoDesks(), backDesk: true),
                Write("R2_L41", room, 6, N, Room2Layouts.CupMugTowers()),
                Write("R2_L42", room, 7, N, Room2Layouts.DonutsBehindCactus()),
                Write("R2_L43", room, 8, N, Room2Layouts.PlateTopWalls(), backDesk: true),
                Write("R2_L44", room, 6, N, Room2Layouts.CafeCounter()),
                Write("R2_L45", room, 6, H, Room2Layouts.CafeTower()),
                Write("R2_L46", room, 5, N, Room2Layouts.MilkRow()),
                Write("R2_L47", room, 6, N, Room2Layouts.MilkDominoes()),
                Write("R2_L48", room, 8, N, Room2Layouts.MilkBehindPlates(), backDesk: true),
                Write("R2_L49", room, 7, N, Room2Layouts.MilkOnBooks()),
                Write("R2_L50", room, 10, H, Room2Layouts.CafeFinale(), backDesk: true),
            };
            EditorUtility.SetDirty(list);
            AssetDatabase.SaveAssets();
            return list;
        }

        private static LevelDef Write(string id, RoomTheme room, int shots, Difficulty difficulty, PropPlacement[] props,
            bool backDesk = false) =>
            LevelAssetWriter.Write(GameLevelListBuilder.RoomFolder(Room), id, room, shots, difficulty, props, backDesk);
    }
}
