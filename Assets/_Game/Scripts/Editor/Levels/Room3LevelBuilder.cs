using System.IO;
using Game.Levels;
using UnityEditor;

namespace Game.Editor
{
    /// <summary>
    /// Writes Room 3 (Café, levels 51–75): the level assets and their list (layouts in Room3Layouts). Written through
    /// LevelAssetWriter, so each is validated. Safe to re-run. After changing a layout, re-run
    /// SMASH_BOT_ROOMS=3 tools/unity/playtest-bot.sh.
    /// </summary>
    public static class Room3LevelBuilder
    {
        private const int Room = 3;
        private const Difficulty N = Difficulty.Normal;
        private const Difficulty H = Difficulty.Hard;

        [MenuItem("Smash/Build Room 3 Levels")]
        public static LevelList BuildAll()
        {
            Directory.CreateDirectory(GameLevelListBuilder.RoomFolder(Room));
            var room = AssetDatabase.LoadAssetAtPath<RoomTheme>(CafeBuilder.ThemePath) ?? CafeBuilder.Build();
            var list = LevelAssetWriter.LoadOrCreate<LevelList>(GameLevelListBuilder.RoomListPath(Room));
            list.levels = new[]
            {
                Write("R3_L51", room, 5, N, Room3Layouts.PlatePlinths()),
                Write("R3_L52", room, 5, N, Room3Layouts.PlateStacks()),
                Write("R3_L53", room, 5, N, Room3Layouts.MugsOnPlates()),
                Write("R3_L54", room, 7, N, Room3Layouts.BackPlatesFrontCups(), backDesk: true),
                Write("R3_L55", room, 8, H, Room3Layouts.CappedTowers()),
                Write("R3_L56", room, 5, N, Room3Layouts.CupPyramid()),
                Write("R3_L57", room, 6, N, Room3Layouts.BookWallLob()),
                Write("R3_L58", room, 7, N, Room3Layouts.LampBehindPlates(), backDesk: true),
                Write("R3_L59", room, 7, N, Room3Layouts.PlateBridges()),
                Write("R3_L60", room, 6, H, Room3Layouts.WideCafeSpread()),
                Write("R3_L61", room, 5, N, Room3Layouts.DonutStacks()),
                Write("R3_L62", room, 5, N, Room3Layouts.DonutsOnPlates()),
                Write("R3_L63", room, 7, N, Room3Layouts.DonutTowersTwoDesks(), backDesk: true),
                Write("R3_L64", room, 6, N, Room3Layouts.DonutRow()),
                Write("R3_L65", room, 9, H, Room3Layouts.DonutPyramidTwoDesks(), backDesk: true),
                Write("R3_L66", room, 5, N, Room3Layouts.CupMugTowers()),
                Write("R3_L67", room, 7, N, Room3Layouts.DonutsBehindCactus()),
                Write("R3_L68", room, 9, N, Room3Layouts.PlateTopWalls(), backDesk: true),
                Write("R3_L69", room, 6, N, Room3Layouts.CafeCounter()),
                Write("R3_L70", room, 6, H, Room3Layouts.CafeTower()),
                Write("R3_L71", room, 5, N, Room3Layouts.MilkRow()),
                Write("R3_L72", room, 5, N, Room3Layouts.MilkDominoes()),
                Write("R3_L73", room, 7, N, Room3Layouts.MilkBehindPlates(), backDesk: true),
                Write("R3_L74", room, 7, N, Room3Layouts.MilkOnBooks()),
                Write("R3_L75", room, 10, H, Room3Layouts.CafeFinale(), backDesk: true),
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
