using System.IO;
using Game.Levels;
using UnityEditor;

namespace Game.Editor
{
    /// <summary>
    /// Writes Room 2 (Kitchen, levels 26–50; Kitchen spec §4): the level assets and their list (layouts in Room2Layouts).
    /// Eight feasts (F) heap 25–40 props and get extra shots (12–18); 30, 35, 40 and 45 are Hard. Written through
    /// LevelAssetWriter, so each is validated. Safe to re-run. After changing a layout, re-run
    /// SMASH_BOT_ROOMS=2 tools/unity/playtest-bot.sh.
    /// </summary>
    public static class Room2LevelBuilder
    {
        private const int Room = 2;
        private const Difficulty N = Difficulty.Normal;
        private const Difficulty H = Difficulty.Hard;
        private const Difficulty F = Difficulty.Feast;

        [MenuItem("Smash/Build Room 2 Levels")]
        public static LevelList BuildAll()
        {
            Directory.CreateDirectory(GameLevelListBuilder.RoomFolder(Room));
            var room = AssetDatabase.LoadAssetAtPath<RoomTheme>(KitchenBuilder.ThemePath) ?? KitchenBuilder.Build();
            var list = LevelAssetWriter.LoadOrCreate<LevelList>(GameLevelListBuilder.RoomListPath(Room));
            list.levels = new[]
            {
                Write("R2_L26", room, 5, N, Room2Layouts.ApplePlates()),
                Write("R2_L27", room, 12, F, Room2Layouts.FruitSpread()),
                Write("R2_L28", room, 6, N, Room2Layouts.TomatoBowls()),
                Write("R2_L29", room, 9, N, Room2Layouts.ApplesOnTheBackDesk(), backDesk: true),
                Write("R2_L30", room, 6, H, Room2Layouts.CrownedTowers()),
                Write("R2_L31", room, 13, F, Room2Layouts.SaladBar()),
                Write("R2_L32", room, 8, N, Room2Layouts.TeaTable()),
                Write("R2_L33", room, 10, N, Room2Layouts.CheeseOnTheBackDesk(), backDesk: true),
                Write("R2_L34", room, 13, F, Room2Layouts.AfternoonTea()),
                Write("R2_L35", room, 7, H, Room2Layouts.WideSpread()),
                Write("R2_L36", room, 6, N, Room2Layouts.CupcakesAndJam()),
                Write("R2_L37", room, 13, F, Room2Layouts.BakeSale()),
                Write("R2_L38", room, 10, N, Room2Layouts.BurgersAndGlasses(), backDesk: true),
                Write("R2_L39", room, 8, N, Room2Layouts.BurgerPlinths()),
                Write("R2_L40", room, 5, H, Room2Layouts.GlassGuard()),
                Write("R2_L41", room, 14, F, Room2Layouts.Diner()),
                Write("R2_L42", room, 7, N, Room2Layouts.KetchupAndCroissants()),
                Write("R2_L43", room, 9, N, Room2Layouts.CroissantsOnTheBackDesk(), backDesk: true),
                Write("R2_L44", room, 14, F, Room2Layouts.Breakfast()),
                Write("R2_L45", room, 5, H, Room2Layouts.CondimentTowers()),
                Write("R2_L46", room, 8, N, Room2Layouts.Picnic()),
                Write("R2_L47", room, 14, F, Room2Layouts.PicnicFeast()),
                Write("R2_L48", room, 10, N, Room2Layouts.CansOnTheBackDesk(), backDesk: true),
                Write("R2_L49", room, 9, N, Room2Layouts.DishTowers()),
                Write("R2_L50", room, 18, F, Room2Layouts.GrandFeast(), backDesk: true),
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
