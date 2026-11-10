using System.IO;
using Game.Levels;
using UnityEditor;

namespace Game.Editor
{
    /// <summary>
    /// Writes Room 4 (Library, levels 76–100): the level assets and their list (layouts in Room4Layouts). Written through
    /// LevelAssetWriter, so each is validated. Safe to re-run. After changing a layout, re-run
    /// SMASH_BOT_ROOMS=4 tools/unity/playtest-bot.sh.
    /// </summary>
    public static class Room4LevelBuilder
    {
        private const int Room = 4;
        private const Difficulty N = Difficulty.Normal;
        private const Difficulty H = Difficulty.Hard;

        [MenuItem("Smash/Build Room 4 Levels")]
        public static LevelList BuildAll()
        {
            Directory.CreateDirectory(GameLevelListBuilder.RoomFolder(Room));
            var room = AssetDatabase.LoadAssetAtPath<RoomTheme>(LibraryBuilder.ThemePath) ?? LibraryBuilder.Build();
            var list = LevelAssetWriter.LoadOrCreate<LevelList>(GameLevelListBuilder.RoomListPath(Room));
            list.levels = new[]
            {
                Write("R4_L76", room, 5, N, Room4Layouts.CandleRow()),
                Write("R4_L77", room, 6, N, Room4Layouts.CandlesOnBooks()),
                Write("R4_L78", room, 6, N, Room4Layouts.BookStaircase()),
                Write("R4_L79", room, 8, N, Room4Layouts.BookWalls(), backDesk: true),
                Write("R4_L80", room, 5, H, Room4Layouts.HeavyLob()),
                Write("R4_L81", room, 5, N, Room4Layouts.Bookends()),
                Write("R4_L82", room, 7, N, Room4Layouts.ReadingNook()),
                Write("R4_L83", room, 8, N, Room4Layouts.CandleRowsTwoDesks(), backDesk: true),
                Write("R4_L84", room, 6, N, Room4Layouts.FramedBooks()),
                Write("R4_L85", room, 8, H, Room4Layouts.TowersTwoDesks(), backDesk: true),
                Write("R4_L86", room, 5, N, Room4Layouts.HourglassesOnBooks()),
                Write("R4_L87", room, 6, N, Room4Layouts.HourglassBridges()),
                Write("R4_L88", room, 5, N, Room4Layouts.LampShield()),
                Write("R4_L89", room, 8, N, Room4Layouts.BackHourglasses(), backDesk: true),
                Write("R4_L90", room, 5, H, Room4Layouts.HourglassTower()),
                Write("R4_L91", room, 7, N, Room4Layouts.StudyDesk()),
                Write("R4_L92", room, 6, N, Room4Layouts.HourglassCandleRow()),
                Write("R4_L93", room, 8, N, Room4Layouts.LampsAndHourglasses(), backDesk: true),
                Write("R4_L94", room, 6, N, Room4Layouts.LibraryWidth()),
                Write("R4_L95", room, 6, H, Room4Layouts.BookFortress(), backDesk: true),
                Write("R4_L96", room, 5, N, Room4Layouts.PencilCupRow()),
                Write("R4_L97", room, 5, N, Room4Layouts.PencilCupsOnBooks()),
                Write("R4_L98", room, 5, N, Room4Layouts.CupsAndCandles()),
                Write("R4_L99", room, 8, N, Room4Layouts.BackCupsFrontWall(), backDesk: true),
                Write("R4_L100", room, 9, H, Room4Layouts.LibraryFinale(), backDesk: true),
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
