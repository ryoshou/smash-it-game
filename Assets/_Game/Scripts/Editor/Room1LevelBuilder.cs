using System.IO;
using Game.Levels;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// Writes Room 1 (Home Office): the 25 level assets and their play list (layouts in Room1Layouts). Written through
    /// LevelEditing.Save, so each is validated; fine-tune later in the Level Editor (it saves to the same assets). Safe
    /// to re-run: it rewrites the layouts. After changing a layout, re-run tools/unity/playtest-bot.sh.
    /// </summary>
    public static class Room1LevelBuilder
    {
        public const string Folder = "Assets/_Game/Levels/Room1";
        public const string ListPath = Folder + "/Room1LevelList.asset";
        private const Difficulty N = Difficulty.Normal;
        private const Difficulty H = Difficulty.Hard;

        [MenuItem("Smash/Build Room 1 Levels")]
        public static LevelList BuildAll()
        {
            Directory.CreateDirectory(Folder);
            var room = AssetDatabase.LoadAssetAtPath<RoomTheme>(HomeOfficeBuilder.ThemePath) ?? HomeOfficeBuilder.Build();
            var list = LoadOrCreate<LevelList>(ListPath);
            list.levels = new[]
            {
                Write("R1_L01", room, 5, N, Room1Layouts.FirstPull()),
                Write("R1_L02", room, 5, N, Room1Layouts.Power()),
                Write("R1_L03", room, 6, N, Room1Layouts.AimAndBreak()),
                Write("R1_L04", room, 6, N, Room1Layouts.TallStack()),
                Write("R1_L05", room, 5, H, Room1Layouts.Heavy()),
                Write("R1_L06", room, 7, N, Room1Layouts.TwoDesks(), backDesk: true),
                Write("R1_L07", room, 5, N, Room1Layouts.Dominoes()),
                Write("R1_L08", room, 6, N, Room1Layouts.Lob()),
                Write("R1_L09", room, 7, N, Room1Layouts.WideSpread()),
                Write("R1_L10", room, 9, H, Room1Layouts.MixedTwoDesks(), backDesk: true),
                Write("R1_L11", room, 5, N, Room1Layouts.WidePyramid()),
                Write("R1_L12", room, 5, N, Room1Layouts.CupTower()),
                Write("R1_L13", room, 7, N, Room1Layouts.CupBridges()),
                Write("R1_L14", room, 7, N, Room1Layouts.BackDeskBooks(), backDesk: true),
                Write("R1_L15", room, 5, H, Room1Layouts.TowerBehindCactus()),
                Write("R1_L16", room, 5, N, Room1Layouts.Cafe()),
                Write("R1_L17", room, 7, N, Room1Layouts.BookBridge()),
                Write("R1_L18", room, 8, N, Room1Layouts.BackWallFrontCups(), backDesk: true),
                Write("R1_L19", room, 6, N, Room1Layouts.Fortress()),
                Write("R1_L20", room, 10, H, Room1Layouts.EverythingTwoDesks(), backDesk: true),
                Write("R1_L21", room, 6, N, Room1Layouts.LampRing()),
                Write("R1_L22", room, 7, N, Room1Layouts.LampsOnBooks()),
                Write("R1_L23", room, 8, N, Room1Layouts.LampShield()),
                Write("R1_L24", room, 8, N, Room1Layouts.LampAcrossDesks(), backDesk: true),
                Write("R1_L25", room, 10, H, Room1Layouts.Finale(), backDesk: true),
            };
            EditorUtility.SetDirty(list);
            AssetDatabase.SaveAssets();
            return list;
        }

        private static LevelDef Write(string id, RoomTheme room, int shots, Difficulty difficulty, PropPlacement[] props,
            bool backDesk = false)
        {
            var level = LoadOrCreate<LevelDef>($"{Folder}/{id}.asset");
            level.id = id;
            level.room = room;
            level.hasBackDesk = backDesk;
            string error = LevelEditing.Save(level, props, shots, difficulty);
            if (error != null) throw new System.InvalidOperationException(error);
            return level;
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
