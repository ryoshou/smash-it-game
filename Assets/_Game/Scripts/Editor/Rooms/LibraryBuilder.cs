using Game.Levels;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// Room 4 (Library, levels 76–100): the sage shell, the walnut desk with its green inlay, bookshelves, a reading
    /// armchair and lamp, candles, under a dim warm light. All static, clear of the desks and the shot corridor.
    /// </summary>
    public static class LibraryBuilder
    {
        public const string ThemePath = PrototypeAssetBuilder.ConfigFolder + "/Rooms/Library.asset";
        public const string BackdropPath = "Assets/_Game/Prefabs/LibraryBackdrop.prefab";
        private const string TinyTreats = RoomKit.TinyTreats;
        private const string KayKit = RoomKit.KayKit;
        private const string Furniture = "Packs/KayKitFurniture/";
        private const string Halloween = "KayKitHalloween";
        private const string Music = "Assets/_Game/Audio/Music/cozy_puzzle_stage_select_bpm100.ogg";
        private static readonly Color Background = new Color(0.86f, 0.9f, 0.8f);
        private static readonly Vector3 PendantBulb = new Vector3(0f, 2.4f, 1.2f);
        private static readonly Color LampColor = new Color(1f, 0.74f, 0.45f);
        private const float LampIntensity = 1.9f; // dimmer than the other rooms: a reading room
        private const float LampRange = 6f;
        private const float ArmchairScale = 0.6f;
        private const float WallShelfScale = 0.7f;
        private const float BookSetScale = 0.6f;
        private const float CandleScale = 0.4f;
        private const float PictureScale = 0.4f;
        private const float RugScale = 0.8f;
        private const float ShelfX = 0.75f;
        private const float LowShelfY = 1.45f;
        private const float HighShelfY = 1.95f;
        private const float PictureY = 2.5f;

        [MenuItem("Smash/Build Library")]
        public static RoomTheme Build()
        {
            var root = new GameObject("LibraryBackdrop");
            RoomKit.Place(root, "Generated/RoomShell_Library.fbx", TinyTreats, Vector3.zero, 0f);
            RoomKit.PlaceDesk(root, "Generated/LibraryDesk.fbx", TinyTreats);
            RoomKit.PlaceRug(root, Furniture + "rug_oval_A.fbx", KayKit, new Vector3(0f, 0.002f, 0.2f), RugScale);

            // A tall decorated bookcase along the right wall, an armchair and standing lamp in the left corner.
            RoomKit.Solid(RoomKit.FlushSideWall(
                RoomKit.Place(root, Furniture + "shelf_B_large_decorated.fbx", KayKit, Vector3.zero, -90f), 1f, 2.4f));
            RoomKit.Solid(RoomKit.FlushSideWall(
                RoomKit.Place(root, Furniture + "armchair_pillows.fbx", KayKit, Vector3.zero, 120f, ArmchairScale), -1f, 2.75f));
            RoomKit.FlushSideWall(RoomKit.Place(root, Furniture + "lamp_standing.fbx", KayKit, Vector3.zero, 0f, 0.8f), -1f, 1.85f);

            // Two wall shelves of books beside the window, a triple candle and a picture above.
            foreach (float y in new[] { LowShelfY, HighShelfY })
            {
                var shelf = RoomKit.FlushBackWall(RoomKit.Place(root, Furniture + "shelf_A_big.fbx", KayKit,
                    new Vector3(0f, y, 0f), 180f, WallShelfScale), ShelfX);
                RoomKit.OnTopOf(root, shelf, Furniture + "book_set.fbx", KayKit, -0.3f, 0f, 180f, BookSetScale);
                if (y == LowShelfY)
                    RoomKit.OnTopOf(root, shelf, $"Packs/{Halloween}/candle_triple.fbx", Halloween, 0.35f, 0f, 180f, CandleScale);
                else
                    RoomKit.OnTopOf(root, shelf, Furniture + "book_set.fbx", KayKit, 0.3f, 0f, 160f, BookSetScale);
            }
            RoomKit.FlushBackWall(RoomKit.Place(root, Furniture + "pictureframe_large_B.fbx", KayKit,
                new Vector3(0f, PictureY, 0f), 180f, PictureScale), ShelfX);

            RoomKit.AddLight(root, "Pendant Light", PendantBulb, LampColor, LampIntensity, LampRange);
            var prefab = RoomKit.SaveBackdrop(root, BackdropPath);
            return RoomKit.WriteTheme(ThemePath, prefab, Music, Background);
        }
    }
}
