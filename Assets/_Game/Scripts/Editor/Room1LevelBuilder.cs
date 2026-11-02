using System.Collections.Generic;
using System.IO;
using Game.Levels;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// Room 1 (Home Office) levels 1–5, one teaching idea each (spec §6.1): pull, power, aim and breaking, toppling a
    /// tall stack, heavy objects (Hard). Every prop is a target, supports included (owner, checkpoint 2). Written through LevelEditing.Save, so each is validated; fine-tune later in
    /// the Level Editor (it saves to the same assets). Safe to re-run: it rewrites the layouts.
    /// </summary>
    public static class Room1LevelBuilder
    {
        public const string Folder = "Assets/_Game/Levels/Room1";
        public const string ListPath = Folder + "/Room1LevelList.asset";
        private const float Keycap = 0.15f;   // standing keycap height
        private const float Book = 0.26f;     // book height, lying flat
        private const float Pitch = 0.16f;    // 1u keycap spacing (0.15 + a gap)

        private enum Look { Cream, Pink, Sky }

        [MenuItem("Smash/Build Room 1 Levels")]
        public static LevelList BuildAll()
        {
            Directory.CreateDirectory(Folder);
            var room = AssetDatabase.LoadAssetAtPath<RoomTheme>(HomeOfficeBuilder.ThemePath) ?? HomeOfficeBuilder.Build();
            var list = LoadOrCreate<LevelList>(ListPath);
            list.levels = new[]
            {
                Write("R1_L01", room, 5, Difficulty.Normal, FirstPull()),
                Write("R1_L02", room, 5, Difficulty.Normal, Power()),
                Write("R1_L03", room, 6, Difficulty.Normal, AimAndBreak()),
                Write("R1_L04", room, 6, Difficulty.Normal, TallStack()),
                Write("R1_L05", room, 7, Difficulty.Hard, Heavy()),
            };
            EditorUtility.SetDirty(list);
            AssetDatabase.SaveAssets();
            return list;
        }

        /// <summary>A low wall of 1u keycaps at the front centre: any pull hits.</summary>
        private static PropPlacement[] FirstPull()
        {
            var props = new List<PropPlacement>();
            for (int row = 0; row < 2; row++)
                for (int col = -2; col <= 2; col++)
                    props.Add(Cap(PropKind.Keycap1u, col * Pitch, row * Keycap, -0.25f, (Look)((col + 2 + row) % 3)));
            return props.ToArray();
        }

        /// <summary>A keycap tower at the back edge: needs a strong pull.</summary>
        private static PropPlacement[] Power()
        {
            var props = new List<PropPlacement>();
            for (int row = 0; row < 5; row++)
                for (int col = 0; col < 2; col++)
                    props.Add(Cap(PropKind.Keycap1u, (col - 0.5f) * Pitch, row * Keycap, 0.5f, (Look)((row + col) % 3)));
            return props.ToArray();
        }

        /// <summary>Three mugs on keycap plinths, left / centre-right / far right: aim, and mugs shatter.</summary>
        private static PropPlacement[] AimAndBreak()
        {
            var props = new List<PropPlacement>();
            var spots = new[] { new Vector2(-0.62f, 0.05f), new Vector2(0.12f, 0.35f), new Vector2(0.62f, -0.2f) };
            for (int i = 0; i < spots.Length; i++)
            {
                props.Add(Cap(PropKind.Keycap2u, spots[i].x, 0f, spots[i].y - 0.08f, Look.Cream));
                props.Add(Cap(PropKind.Keycap2u, spots[i].x, 0f, spots[i].y + 0.08f, Look.Sky));
                props.Add(Prop(PropKind.Mug, spots[i].x, Keycap, spots[i].y, i));
            }
            return props.ToArray();
        }

        /// <summary>Four books stacked, three keycaps on top: topple the tower.</summary>
        private static PropPlacement[] TallStack()
        {
            var props = new List<PropPlacement>();
            float[] yaws = { 0f, 4f, -3f, 2f };
            props.Add(Prop(PropKind.Book, 0f, 0f, 0.15f, 0, yaws[0]));
            for (int i = 1; i < yaws.Length; i++)
                props.Add(Prop(PropKind.Book, 0f, i * Book, 0.15f, 0, yaws[i]));
            for (int col = -1; col <= 1; col++)
                props.Add(Cap(PropKind.Keycap1u, col * Pitch, yaws.Length * Book, 0.15f, (Look)(col + 1)));
            return props.ToArray();
        }

        /// <summary>A heavy cactus pot in the middle, mugs on books either side (Hard).</summary>
        private static PropPlacement[] Heavy() => new[]
        {
            Prop(PropKind.CactusPot, 0f, 0f, 0.3f, 0),
            // Books near the back edge, so once its mug is gone a hit can push a book off.
            Prop(PropKind.Book, -0.62f, 0f, 0.42f, 0),
            Prop(PropKind.Mug, -0.62f, Book, 0.42f, 2),
            Prop(PropKind.Book, 0.62f, 0f, 0.42f, 0),
            Prop(PropKind.Mug, 0.62f, Book, 0.42f, 1),
        };

        private static PropPlacement Cap(PropKind kind, float x, float y, float z, Look look) => Prop(kind, x, y, z, (int)look);

        private static PropPlacement Prop(PropKind kind, float x, float y, float z, int variant, float yaw = 0f) =>
            new PropPlacement { kind = kind, basePosition = new Vector3(x, y, z), variant = variant, yawDegrees = yaw, isTarget = true };

        private static LevelDef Write(string id, RoomTheme room, int shots, Difficulty difficulty, PropPlacement[] props)
        {
            var level = LoadOrCreate<LevelDef>($"{Folder}/{id}.asset");
            level.id = id;
            level.room = room;
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
