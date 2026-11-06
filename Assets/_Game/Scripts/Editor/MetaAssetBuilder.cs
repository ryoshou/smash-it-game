using System;
using System.Collections.Generic;
using Game.Levels;
using Game.Meta;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>Creates AppConfig and the English UiStrings; fills a display name for every PropKind.</summary>
    public static class MetaAssetBuilder
    {
        public const string AppConfigPath = PrototypeAssetBuilder.ConfigFolder + "/AppConfig.asset";
        public const string StringsPath = PrototypeAssetBuilder.ConfigFolder + "/UiStrings_en.asset";

        private static readonly Dictionary<PropKind, string> PropNames = new Dictionary<PropKind, string>
        {
            { PropKind.Keycap1u, "Keycap" },
            { PropKind.Keycap2u, "Wide Keycap" },
            { PropKind.KeycapSpace, "Spacebar" },
            { PropKind.KeycapEnter, "Enter Key" },
            { PropKind.Mug, "Coffee Mug" },
            { PropKind.Book, "Book" },
            { PropKind.CactusPot, "Cactus" },
            { PropKind.PaperCup, "Paper Cup" },
            { PropKind.DeskLamp, "Desk Lamp" },
            { PropKind.Plate, "Plate" },
            { PropKind.Donut, "Donut" },
            { PropKind.MilkBottle, "Milk Bottle" },
            { PropKind.Candle, "Candle" },
            { PropKind.Hourglass, "Hourglass" },
            { PropKind.PencilCup, "Pencil Cup" },
            { PropKind.SodaCan, "Soda Can" },
            { PropKind.GameController, "Game Controller" },
            { PropKind.FizzySoda, "Fizzy Soda" },
            { PropKind.DinnerPlate, "Dinner Plate" },
            { PropKind.Bowl, "Bowl" },
            { PropKind.Teapot, "Teapot" },
            { PropKind.WineGlass, "Wine Glass" },
            { PropKind.JamJar, "Jam Jar" },
            { PropKind.KetchupBottle, "Ketchup" },
            { PropKind.DrinkCan, "Drink Can" },
            { PropKind.Apple, "Apple" },
            { PropKind.Tomato, "Tomato" },
            { PropKind.Cheese, "Cheese" },
            { PropKind.Burger, "Burger" },
            { PropKind.Cupcake, "Cupcake" },
            { PropKind.Croissant, "Croissant" },
            { PropKind.Sandwich, "Sandwich" },
        };

        [MenuItem("Smash/Build Meta Assets")]
        public static void Build()
        {
            LoadOrCreate<AppConfig>(AppConfigPath);
            var strings = LoadOrCreate<UiStrings>(StringsPath);
            var kinds = (PropKind[])Enum.GetValues(typeof(PropKind));
            var names = new string[kinds.Length];
            foreach (var kind in kinds)
            {
                string existing = strings.propNames != null && (int)kind < strings.propNames.Length ? strings.propNames[(int)kind] : null;
                names[(int)kind] = !string.IsNullOrEmpty(existing) ? existing
                    : PropNames.TryGetValue(kind, out string name) ? name
                    : throw new InvalidOperationException($"No display name for {kind}; add one to MetaAssetBuilder.");
            }
            strings.propNames = names;
            EditorUtility.SetDirty(strings);
            AssetDatabase.SaveAssets();
        }

        public static AppConfig LoadAppConfig() => AssetDatabase.LoadAssetAtPath<AppConfig>(AppConfigPath);
        public static UiStrings LoadStrings() => AssetDatabase.LoadAssetAtPath<UiStrings>(StringsPath);

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
