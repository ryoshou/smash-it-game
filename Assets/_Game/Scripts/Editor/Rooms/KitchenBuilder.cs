using Game.Levels;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// Room 2 (Kitchen, levels 26–50; Kitchen spec §7): the mint-walled shell with a cream tiled floor, a farmhouse table
    /// as the desk look, and Tiny Treats Charming Kitchen pieces against the walls, all static and clear of the play
    /// desk, the back desk and the shot corridor. Writes the backdrop prefab and theme.
    /// </summary>
    public static class KitchenBuilder
    {
        public const string ThemePath = PrototypeAssetBuilder.ConfigFolder + "/Rooms/Kitchen.asset";
        public const string BackdropPath = "Assets/_Game/Prefabs/KitchenBackdrop.prefab";
        private const string TinyTreats = RoomKit.TinyTreats;
        private const string Kitchen = "Packs/TinyTreats/";
        private const string Music = "Assets/_Game/Audio/Music/good_morning_cakeflaps.ogg";
        private static readonly Color Background = new Color(0.82f, 0.95f, 0.9f);
        private static readonly Vector3 PendantBulb = new Vector3(0f, 2.4f, 1.2f);
        private static readonly Color LampColor = new Color(1f, 0.8f, 0.55f);
        private const float LampIntensity = 2.6f;
        private const float LampRange = 6f;
        // Charming Kitchen is built at toy scale (a counter is as tall as the play desk); these bring it to room size.
        private const float CounterScale = 0.6f;
        private const float CounterTopScale = 0.42f;
        private const float BlindsScale = 0.72f;
        private const float WindowX = -0.85f;      // the room shell's back-wall window
        private const float BlindsRailY = 2.62f;
        private const float StoveZ = 2.0f;
        private const float FridgeZ = 2.75f;
        private const float SideCounterX = 0.05f;

        [MenuItem("Smash/Build Kitchen Room")]
        public static RoomTheme Build()
        {
            var root = new GameObject("KitchenBackdrop");
            RoomKit.Place(root, "Generated/RoomShell_Kitchen.fbx", TinyTreats, Vector3.zero, 0f);
            RoomKit.PlaceDesk(root, "Generated/KitchenDesk.fbx", TinyTreats);

            // The sink under the window, a dish rack beside it; a small counter to its right with the toaster.
            var sink = RoomKit.Solid(RoomKit.FlushBackWall(
                RoomKit.Place(root, Kitchen + "countertop_sink.fbx", TinyTreats, Vector3.zero, 180f, CounterScale), WindowX));
            RoomKit.OnTopOf(root, sink, Kitchen + "dishrack_plates.fbx", TinyTreats, -0.38f, 0.05f, 180f, CounterTopScale);
            var counter = RoomKit.Solid(RoomKit.FlushBackWall(
                RoomKit.Place(root, Kitchen + "countertop_single.fbx", TinyTreats, Vector3.zero, 180f, CounterScale), SideCounterX));
            RoomKit.OnTopOf(root, counter, Kitchen + "toaster.fbx", TinyTreats, 0f, 0.05f, 180f, CounterTopScale);
            RoomKit.FlushBackWall(RoomKit.Place(root, Kitchen + "blinds_kitchen.fbx", TinyTreats,
                new Vector3(0f, BlindsRailY, 0f), 180f, BlindsScale), WindowX);

            // The stove (side-on) against the left wall with the kettle on it.
            var stove = RoomKit.Solid(RoomKit.FlushSideWall(
                RoomKit.Place(root, Kitchen + "stove.fbx", TinyTreats, Vector3.zero, 90f, CounterScale), -1f, StoveZ));
            RoomKit.OnTopOf(root, stove, Kitchen + "kettle.fbx", TinyTreats, 0f, 0.15f, 90f, CounterTopScale);
            // The fridge against the right wall.
            RoomKit.Solid(RoomKit.FlushSideWall(
                RoomKit.Place(root, Kitchen + "fridge.fbx", TinyTreats, Vector3.zero, -90f, CounterScale), 1f, FridgeZ));

            RoomKit.AddLight(root, "Pendant Light", PendantBulb, LampColor, LampIntensity, LampRange);
            var prefab = RoomKit.SaveBackdrop(root, BackdropPath);
            return RoomKit.WriteTheme(ThemePath, prefab, Music, Background);
        }
    }
}
