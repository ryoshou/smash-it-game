using Game.Levels;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// Room 2 (Café, levels 26–50): the butter-walled shell, the pale café desk, and Tiny Treats Bakery decor against
    /// the walls, all static and clear of the play desk and the shot corridor. Writes the backdrop prefab and theme.
    /// </summary>
    public static class CafeBuilder
    {
        public const string ThemePath = PrototypeAssetBuilder.ConfigFolder + "/Rooms/Cafe.asset";
        public const string BackdropPath = "Assets/_Game/Prefabs/CafeBackdrop.prefab";
        private const string TinyTreats = RoomKit.TinyTreats;
        private const string Bakery = "Packs/TinyTreats/";
        private const string Music = "Assets/_Game/Audio/Music/cozy_puzzle_in-game_1_bpm118.ogg";
        private static readonly Color Background = new Color(1f, 0.93f, 0.78f);
        private static readonly Vector3 PendantBulb = new Vector3(0f, 2.4f, 1.2f);
        private static readonly Color LampColor = new Color(1f, 0.76f, 0.5f);
        private const float LampIntensity = 2.6f;
        private const float LampRange = 6f;
        // Tiny Treats furniture is built at toy scale (its counter is as big as the play desk); these bring it to room size.
        private const float CounterScale = 0.6f;
        private const float CounterTopScale = 0.42f;
        private const float CaseScale = 0.55f;
        private const float ShelfScale = 0.6f;
        private const float ShelfDecorScale = 0.55f;
        private const float CurtainScale = 0.72f;
        private const float RugScale = 0.8f;
        private const float WindowX = -0.85f;      // the room shell's back-wall window
        private const float CurtainRailY = 2.62f;
        private const float ShelfX = 0.85f;
        private const float ShelfY = 1.95f;

        [MenuItem("Smash/Build Café")]
        public static RoomTheme Build()
        {
            var root = new GameObject("CafeBackdrop");
            RoomKit.Place(root, "Generated/RoomShell_Cafe.fbx", TinyTreats, Vector3.zero, 0f);
            RoomKit.PlaceDesk(root, "Generated/CafeDesk.fbx", TinyTreats);
            RoomKit.Place(root, Bakery + "rug.fbx", TinyTreats, new Vector3(0f, 0.002f, 0.2f), 0f, RugScale);

            // The counter (side-on) against the left wall, coffee machine and cookie jar on it.
            var counter = RoomKit.Solid(RoomKit.FlushSideWall(
                RoomKit.Place(root, Bakery + "counter_table.fbx", TinyTreats, Vector3.zero, 90f, CounterScale), -1f, 2.4f));
            RoomKit.OnTopOf(root, counter, Bakery + "coffee_machine.fbx", TinyTreats, 0f, 0.25f, 90f, CounterTopScale);
            RoomKit.OnTopOf(root, counter, Bakery + "cookie_jar.fbx", TinyTreats, 0.05f, -0.3f, 0f, CounterTopScale);
            RoomKit.OnTopOf(root, counter, Bakery + "coffee_cup_takeaway_stacked.fbx", TinyTreats, 0.1f, -0.05f, 0f, CounterTopScale);
            // The pastry display case against the right wall.
            RoomKit.Solid(RoomKit.FlushSideWall(
                RoomKit.Place(root, Bakery + "display_case_long.fbx", TinyTreats, Vector3.zero, -90f, CaseScale), 1f, 2.4f));

            // A shelf on the back wall beside the window, with a decorated pastry stand and a cookie jar on it.
            var shelf = RoomKit.FlushBackWall(RoomKit.Place(root, Bakery + "wall_shelf_bakery_B.fbx", TinyTreats,
                new Vector3(0f, ShelfY, 0f), 180f, ShelfScale), ShelfX);
            RoomKit.OnTopOf(root, shelf, Bakery + "pastry_stand_A_decorated.fbx", TinyTreats, 0.25f, 0f, 0f, ShelfDecorScale);
            RoomKit.OnTopOf(root, shelf, Bakery + "cookie_jar.fbx", TinyTreats, -0.3f, 0f, 0f, ShelfDecorScale);
            RoomKit.FlushBackWall(RoomKit.Place(root, Bakery + "curtains.fbx", TinyTreats,
                new Vector3(0f, CurtainRailY, 0f), 180f, CurtainScale), WindowX);

            RoomKit.AddLight(root, "Pendant Light", PendantBulb, LampColor, LampIntensity, LampRange);
            var prefab = RoomKit.SaveBackdrop(root, BackdropPath);
            return RoomKit.WriteTheme(ThemePath, prefab, Music, Background);
        }
    }
}
