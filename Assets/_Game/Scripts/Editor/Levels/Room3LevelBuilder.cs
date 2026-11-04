using System.IO;
using Game.Levels;
using UnityEditor;

namespace Game.Editor
{
    /// <summary>
    /// Writes Room 3 (Library, levels 51–75): the level assets and their list (layouts in Room3Layouts). Written through
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
            var room = AssetDatabase.LoadAssetAtPath<RoomTheme>(LibraryBuilder.ThemePath) ?? LibraryBuilder.Build();
            var list = LevelAssetWriter.LoadOrCreate<LevelList>(GameLevelListBuilder.RoomListPath(Room));
            list.levels = new[]
            {
                Write("R3_L51", room, 5, N, Room3Layouts.CandleRow()),
                Write("R3_L52", room, 6, N, Room3Layouts.CandlesOnBooks()),
                Write("R3_L53", room, 6, N, Room3Layouts.BookStaircase()),
                Write("R3_L54", room, 8, N, Room3Layouts.BookWalls(), backDesk: true),
                Write("R3_L55", room, 5, H, Room3Layouts.HeavyLob()),
                Write("R3_L56", room, 5, N, Room3Layouts.Bookends()),
                Write("R3_L57", room, 7, N, Room3Layouts.ReadingNook()),
                Write("R3_L58", room, 8, N, Room3Layouts.CandleRowsTwoDesks(), backDesk: true),
                Write("R3_L59", room, 6, N, Room3Layouts.FramedBooks()),
                Write("R3_L60", room, 10, H, Room3Layouts.TowersTwoDesks(), backDesk: true),
                Write("R3_L61", room, 5, N, Room3Layouts.HourglassesOnBooks()),
                Write("R3_L62", room, 6, N, Room3Layouts.HourglassBridges()),
                Write("R3_L63", room, 5, N, Room3Layouts.LampShield()),
                Write("R3_L64", room, 8, N, Room3Layouts.BackHourglasses(), backDesk: true),
                Write("R3_L65", room, 5, H, Room3Layouts.HourglassTower()),
                Write("R3_L66", room, 7, N, Room3Layouts.StudyDesk()),
                Write("R3_L67", room, 6, N, Room3Layouts.HourglassCandleRow()),
                Write("R3_L68", room, 8, N, Room3Layouts.LampsAndHourglasses(), backDesk: true),
                Write("R3_L69", room, 6, N, Room3Layouts.LibraryWidth()),
                Write("R3_L70", room, 6, H, Room3Layouts.BookFortress(), backDesk: true),
                Write("R3_L71", room, 5, N, Room3Layouts.PencilCupRow()),
                Write("R3_L72", room, 5, N, Room3Layouts.PencilCupsOnBooks()),
                Write("R3_L73", room, 5, N, Room3Layouts.CupsAndCandles()),
                Write("R3_L74", room, 8, N, Room3Layouts.BackCupsFrontWall(), backDesk: true),
                Write("R3_L75", room, 10, H, Room3Layouts.LibraryFinale(), backDesk: true),
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
