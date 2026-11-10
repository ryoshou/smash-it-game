using System.IO;
using Game.Levels;
using UnityEditor;

namespace Game.Editor
{
    /// <summary>
    /// Writes Room 5 (Gaming Den, levels 101–125): the level assets and their list (layouts in Room5Layouts). Written through
    /// LevelAssetWriter, so each is validated. Safe to re-run. After changing a layout, re-run
    /// SMASH_BOT_ROOMS=5 tools/unity/playtest-bot.sh.
    /// </summary>
    public static class Room5LevelBuilder
    {
        private const int Room = 5;
        private const Difficulty N = Difficulty.Normal;
        private const Difficulty H = Difficulty.Hard;

        [MenuItem("Smash/Build Room 5 Levels")]
        public static LevelList BuildAll()
        {
            Directory.CreateDirectory(GameLevelListBuilder.RoomFolder(Room));
            var room = AssetDatabase.LoadAssetAtPath<RoomTheme>(GamingDenBuilder.ThemePath) ?? GamingDenBuilder.Build();
            var list = LevelAssetWriter.LoadOrCreate<LevelList>(GameLevelListBuilder.RoomListPath(Room));
            list.levels = new[]
            {
                Write("R5_L101", room, 5, N, Room5Layouts.CanPyramid()),
                Write("R5_L102", room, 5, N, Room5Layouts.CanTowers()),
                Write("R5_L103", room, 6, N, Room5Layouts.CansOnWalls()),
                Write("R5_L104", room, 7, N, Room5Layouts.PyramidsTwoDesks(), backDesk: true),
                Write("R5_L105", room, 6, H, Room5Layouts.LampShieldCans()),
                Write("R5_L106", room, 7, N, Room5Layouts.Keyboard()),
                Write("R5_L107", room, 5, N, Room5Layouts.SnackDesk()),
                Write("R5_L108", room, 8, N, Room5Layouts.BackWallFrontCans(), backDesk: true),
                Write("R5_L109", room, 7, N, Room5Layouts.CanBridges()),
                Write("R5_L110", room, 7, H, Room5Layouts.TowersOnBooks(), backDesk: true),
                Write("R5_L111", room, 5, N, Room5Layouts.ControllersOnPlinths()),
                Write("R5_L112", room, 5, N, Room5Layouts.ControllerShield()),
                Write("R5_L113", room, 8, N, Room5Layouts.ControllersTwoDesks(), backDesk: true),
                Write("R5_L114", room, 5, N, Room5Layouts.ControllersOnPlatesAndBooks()),
                Write("R5_L115", room, 7, H, Room5Layouts.WideDenSpread()),
                Write("R5_L116", room, 5, N, Room5Layouts.FizzyInPyramid()),
                Write("R5_L117", room, 5, N, Room5Layouts.FizzyChain()),
                Write("R5_L118", room, 5, N, Room5Layouts.FizzyFortress()),
                Write("R5_L119", room, 8, N, Room5Layouts.BackDeskFizzy(), backDesk: true),
                Write("R5_L120", room, 5, H, Room5Layouts.FizzyBookFortress()),
                Write("R5_L121", room, 6, N, Room5Layouts.FizzyByTheLamp()),
                Write("R5_L122", room, 6, N, Room5Layouts.EverythingDen()),
                Write("R5_L123", room, 8, N, Room5Layouts.FizzyChainTwoDesks(), backDesk: true),
                Write("R5_L124", room, 7, N, Room5Layouts.GreatestHits()),
                Write("R5_L125", room, 10, H, Room5Layouts.GrandFinale(), backDesk: true),
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
