using Game.Levels;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// Room 4 (Gaming Den, levels 76–100): the deep-blue shell, the charcoal desk with its violet band, a gaming armchair, a
    /// low table at the back, a gaming table with monitor, keyboard and headphones, and a violet and cyan glow.
    /// </summary>
    public static class GamingDenBuilder
    {
        public const string ThemePath = PrototypeAssetBuilder.ConfigFolder + "/Rooms/GamingDen.asset";
        public const string BackdropPath = "Assets/_Game/Prefabs/GamingDenBackdrop.prefab";
        private const string TinyTreats = RoomKit.TinyTreats;
        private const string KayKit = RoomKit.KayKit;
        private const string Furniture = "Packs/KayKitFurniture/";
        private const string Music = "Assets/_Game/Audio/Music/cozy_puzzle_in-game_3_bpm108.ogg";
        private static readonly Color Background = new Color(0.78f, 0.8f, 0.95f);
        private static readonly Color Violet = new Color(0.7f, 0.45f, 1f);
        private static readonly Color Cyan = new Color(0.35f, 0.9f, 1f);
        private static readonly Color Warm = new Color(1f, 0.82f, 0.62f);
        private const float GlowIntensity = 2.2f;
        private const float GlowRange = 4.5f;
        private const float CouchScale = 0.75f;
        private const float LowTableScale = 0.5f;
        private const float GamingTableScale = 0.6f;
        private const float WallShelfScale = 0.7f;
        private const float CouchX = 0.15f;
        private const float RugScale = 0.8f;

        [MenuItem("Smash/Build Gaming Den")]
        public static RoomTheme Build()
        {
            var root = new GameObject("GamingDenBackdrop");
            RoomKit.Place(root, "Generated/RoomShell_Den.fbx", TinyTreats, Vector3.zero, 0f);
            RoomKit.PlaceDesk(root, "Generated/DenDesk.fbx", TinyTreats);

            RoomKit.Place(root, Furniture + "rug_rectangle_B.fbx", KayKit, new Vector3(0f, 0.002f, 0.2f), 0f, RugScale);
            // The gaming armchair in the right corner, turned to the room; a low table against the back wall.
            RoomKit.Solid(RoomKit.FlushSideWall(
                RoomKit.Place(root, Furniture + "armchair_pillows.fbx", KayKit, Vector3.zero, -135f, CouchScale), 1f, 2.75f));
            RoomKit.Solid(RoomKit.FlushBackWall(
                RoomKit.Place(root, Furniture + "table_low.fbx", KayKit, Vector3.zero, 0f, LowTableScale), CouchX));

            // The gaming table against the left wall: monitor, keyboard and headphones.
            var table = RoomKit.Solid(RoomKit.FlushSideWall(
                RoomKit.Place(root, Furniture + "table_medium.fbx", KayKit, Vector3.zero, 0f, GamingTableScale), -1f, 2.0f));
            RoomKit.Solid(RoomKit.OnTopOf(root, table, "Generated/Monitor.fbx", TinyTreats, 0f, 0.12f, 0f, 1f));
            RoomKit.OnTopOf(root, table, "Generated/Keyboard.fbx", TinyTreats, 0f, -0.2f, 0f, 0.5f);
            RoomKit.OnTopOf(root, table, "Generated/Headphones.fbx", TinyTreats, 0.28f, -0.1f, 30f, 0.9f);
            RoomKit.FlushSideWall(RoomKit.Place(root, Furniture + "shelf_A_small.fbx", KayKit,
                new Vector3(0f, 1.7f, 0f), -90f, WallShelfScale), 1f, 1.9f);

            // A gaming glow: violet and cyan on either side, a warm fill over the desk (all without shadows).
            RoomKit.AddLight(root, "Violet Glow", new Vector3(-1.3f, 1.8f, 2.4f), Violet, GlowIntensity, GlowRange);
            RoomKit.AddLight(root, "Cyan Glow", new Vector3(1.3f, 1.8f, 2.4f), Cyan, GlowIntensity, GlowRange);
            RoomKit.AddLight(root, "Pendant Light", new Vector3(0f, 2.4f, 1.2f), Warm, 1.6f, 6f);
            var prefab = RoomKit.SaveBackdrop(root, BackdropPath);
            return RoomKit.WriteTheme(ThemePath, prefab, Music, Background);
        }
    }
}
