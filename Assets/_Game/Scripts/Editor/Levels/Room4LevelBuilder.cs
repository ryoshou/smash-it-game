using System.IO;
using Game.Levels;
using UnityEditor;

namespace Game.Editor
{
    /// <summary>
    /// Writes Room 4 (Gaming Den, levels 76–100): the level assets and their list (layouts in Room4Layouts). Written through
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
            var room = AssetDatabase.LoadAssetAtPath<RoomTheme>(GamingDenBuilder.ThemePath) ?? GamingDenBuilder.Build();
            var list = LevelAssetWriter.LoadOrCreate<LevelList>(GameLevelListBuilder.RoomListPath(Room));
            list.levels = new[]
            {
                Write("R4_L76", room, 5, N, Room4Layouts.CanPyramid()),
                Write("R4_L77", room, 5, N, Room4Layouts.CanTowers()),
                Write("R4_L78", room, 6, N, Room4Layouts.CansOnWalls()),
                Write("R4_L79", room, 7, N, Room4Layouts.PyramidsTwoDesks(), backDesk: true),
                Write("R4_L80", room, 6, H, Room4Layouts.LampShieldCans()),
                Write("R4_L81", room, 7, N, Room4Layouts.Keyboard()),
                Write("R4_L82", room, 5, N, Room4Layouts.SnackDesk()),
                Write("R4_L83", room, 8, N, Room4Layouts.BackWallFrontCans(), backDesk: true),
                Write("R4_L84", room, 7, N, Room4Layouts.CanBridges()),
                Write("R4_L85", room, 9, H, Room4Layouts.TowersOnBooks(), backDesk: true),
                Write("R4_L86", room, 5, N, Room4Layouts.ControllersOnPlinths()),
                Write("R4_L87", room, 5, N, Room4Layouts.ControllerShield()),
                Write("R4_L88", room, 8, N, Room4Layouts.ControllersTwoDesks(), backDesk: true),
                Write("R4_L89", room, 5, N, Room4Layouts.ControllersOnPlatesAndBooks()),
                Write("R4_L90", room, 7, H, Room4Layouts.WideDenSpread()),
                Write("R4_L91", room, 5, N, Room4Layouts.FizzyInPyramid()),
                Write("R4_L92", room, 5, N, Room4Layouts.FizzyChain()),
                Write("R4_L93", room, 5, N, Room4Layouts.FizzyFortress()),
                Write("R4_L94", room, 8, N, Room4Layouts.BackDeskFizzy(), backDesk: true),
                Write("R4_L95", room, 5, H, Room4Layouts.FizzyBookFortress()),
                Write("R4_L96", room, 5, N, Room4Layouts.FizzyByTheLamp()),
                Write("R4_L97", room, 6, N, Room4Layouts.EverythingDen()),
                Write("R4_L98", room, 8, N, Room4Layouts.FizzyChainTwoDesks(), backDesk: true),
                Write("R4_L99", room, 7, N, Room4Layouts.GreatestHits()),
                Write("R4_L100", room, 10, H, Room4Layouts.GrandFinale(), backDesk: true),
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
