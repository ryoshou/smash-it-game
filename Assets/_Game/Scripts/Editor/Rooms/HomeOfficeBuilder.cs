using Game.Levels;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// Room 1 (Home Office): the generated room shell, the office desk look, plus pack decor, all static and clear of the play desk
    /// (2.0 x 1.5 at the origin) and the shot corridor. Writes the backdrop prefab and the RoomTheme. Safe to re-run.
    /// </summary>
    public static class HomeOfficeBuilder
    {
        public const string ThemePath = PrototypeAssetBuilder.ConfigFolder + "/Rooms/HomeOffice.asset";
        public const string BackdropPath = "Assets/_Game/Prefabs/HomeOfficeBackdrop.prefab";
        private const string KayKit = RoomKit.KayKit;
        private const string TinyTreats = RoomKit.TinyTreats;
        private const string Music = "Assets/_Game/Audio/Music/cozy_puzzle_in-game_2_bpm90.ogg";
        private static readonly Color Background = new Color(0.98f, 0.9f, 0.82f);
        private static readonly Vector3 PendantBulb = new Vector3(0f, 2.4f, 1.2f);
        private static readonly Color LampColor = new Color(1f, 0.8f, 0.55f);
        private const float LampIntensity = 2.5f;
        private const float LampRange = 6f;

        [MenuItem("Smash/Build Home Office")]
        public static RoomTheme Build()
        {
            var root = new GameObject("HomeOfficeBackdrop");
            RoomKit.Place(root, "Generated/RoomShell.fbx", TinyTreats, Vector3.zero, 0f);
            RoomKit.PlaceDesk(root, "Generated/OfficeDesk.fbx", TinyTreats);
            RoomKit.PlaceRug(root, "Packs/KayKitFurniture/rug_rectangle_stripes_A.fbx", KayKit, new Vector3(0f, 0.002f, 0.2f));

            // Furniture stands against the walls (the room is only 3.5 m wide), clear of the play desk and shot corridor.
            var table = RoomKit.Solid(RoomKit.AgainstSideWall(
                RoomKit.Place(root, "Packs/KayKitFurniture/table_medium.fbx", KayKit, Vector3.zero, 0f), -1f, 2.1f));
            var tableSpot = table.transform.position;
            float tableTop = ArtMeshStats.ModelBounds(table).max.y;
            RoomKit.Solid(RoomKit.Place(root, "Generated/Monitor.fbx", TinyTreats, tableSpot + new Vector3(0f, tableTop, 0.15f), 0f));
            RoomKit.Place(root, "Generated/Keyboard.fbx", TinyTreats, tableSpot + new Vector3(0f, tableTop, -0.25f), 0f, 0.6f);
            RoomKit.Place(root, "Packs/KayKitFurniture/pictureframe_standing_A.fbx", KayKit, tableSpot + new Vector3(0.55f, tableTop, -0.05f), 160f, 0.5f);
            RoomKit.Solid(RoomKit.AgainstBackWall(
                RoomKit.Place(root, "Packs/KayKitFurniture/shelf_B_large_decorated.fbx", KayKit, Vector3.zero, 0f), 1f));
            RoomKit.AgainstSideWall(RoomKit.Place(root, "Packs/KayKitFurniture/lamp_standing.fbx", KayKit, Vector3.zero, 0f, 0.8f), 1f, 2.45f);
            RoomKit.AgainstBackWall(RoomKit.Place(root, "Packs/KayKitFurniture/cactus_medium_A.fbx", KayKit, Vector3.zero, 0f), -1f);
            // Pictures hang on the back wall, facing into the room.
            RoomKit.Place(root, "Packs/KayKitFurniture/pictureframe_large_A.fbx", KayKit, new Vector3(0.55f, 1.7f, RoomKit.BackWallZ - 0.04f), 180f);
            RoomKit.Place(root, "Packs/KayKitFurniture/pictureframe_medium.fbx", KayKit, new Vector3(1.2f, 2.1f, RoomKit.BackWallZ - 0.04f), 180f);
            // A warm pendant light under the shade (the room shell's Pendant).
            RoomKit.AddLight(root, "Pendant Light", PendantBulb, LampColor, LampIntensity, LampRange);

            var prefab = RoomKit.SaveBackdrop(root, BackdropPath);
            return RoomKit.WriteTheme(ThemePath, prefab, Music, Background);
        }
    }
}
