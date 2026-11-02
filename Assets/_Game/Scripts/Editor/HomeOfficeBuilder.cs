using System.IO;
using Game.Levels;
using Game.World;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// Room 1 (Home Office): the generated room shell plus pack decor, all static and clear of the play desk
    /// (2.0 x 1.5 at the origin) and the shot corridor. Writes the backdrop prefab and the RoomTheme. Safe to re-run.
    /// </summary>
    public static class HomeOfficeBuilder
    {
        public const string ThemePath = PrototypeAssetBuilder.ConfigFolder + "/Rooms/HomeOffice.asset";
        public const string BackdropPath = "Assets/_Game/Prefabs/HomeOfficeBackdrop.prefab";
        private const string Art = "Assets/_Game/Art/";
        private const string KayKit = "KayKitFurniture";
        private const string TinyTreats = "TinyTreats";
        private const string Music = "Assets/_Game/Audio/Music/cozy_puzzle_in-game_2_bpm90.ogg";
        private static readonly Color Background = new Color(0.98f, 0.9f, 0.82f);
        private const float WallInnerX = 1.65f;   // room shell side walls (1.75) minus their thickness
        private const float BackWallZ = 3.4f;     // room shell back wall's inner face
        private const float WallGap = 0.05f;
        private static readonly Vector3 PendantBulb = new Vector3(0f, 2.4f, 1.2f);
        private static readonly Color LampColor = new Color(1f, 0.8f, 0.55f);
        private const float LampIntensity = 2.5f;
        private const float LampRange = 6f;

        [MenuItem("Smash/Build Home Office")]
        public static RoomTheme Build()
        {
            var root = new GameObject("HomeOfficeBackdrop");
            Place(root, "Generated/RoomShell.fbx", TinyTreats, Vector3.zero, 0f);
            Place(root, "Packs/KayKitFurniture/rug_rectangle_stripes_A.fbx", KayKit, new Vector3(0f, 0.002f, 0.2f), 0f);

            // Furniture stands against the walls (the room is only 3.5 m wide), clear of the play desk and shot corridor.
            var table = Solid(AgainstSideWall(Place(root, "Packs/KayKitFurniture/table_medium.fbx", KayKit, Vector3.zero, 0f), -1f, 2.1f));
            var tableSpot = table.transform.position;
            float tableTop = ArtMeshStats.ModelBounds(table).max.y;
            Solid(Place(root, "Generated/Monitor.fbx", TinyTreats, tableSpot + new Vector3(0f, tableTop, 0.15f), 0f));
            Place(root, "Generated/Keyboard.fbx", TinyTreats, tableSpot + new Vector3(0f, tableTop, -0.25f), 0f, 0.6f);
            Place(root, "Packs/KayKitFurniture/pictureframe_standing_A.fbx", KayKit, tableSpot + new Vector3(0.55f, tableTop, -0.05f), 160f, 0.5f);
            Solid(AgainstBackWall(Place(root, "Packs/KayKitFurniture/shelf_B_large_decorated.fbx", KayKit, Vector3.zero, 0f), 1f));
            AgainstSideWall(Place(root, "Packs/KayKitFurniture/lamp_standing.fbx", KayKit, Vector3.zero, 0f, 0.8f), 1f, 2.45f);
            AgainstBackWall(Place(root, "Packs/KayKitFurniture/cactus_medium_A.fbx", KayKit, Vector3.zero, 0f), -1f);
            // Pictures hang on the back wall, facing into the room.
            Place(root, "Packs/KayKitFurniture/pictureframe_large_A.fbx", KayKit, new Vector3(0.55f, 1.7f, BackWallZ - 0.04f), 180f);
            Place(root, "Packs/KayKitFurniture/pictureframe_medium.fbx", KayKit, new Vector3(1.2f, 2.1f, BackWallZ - 0.04f), 180f);
            AddLampLight(root);
            AddRoomColliders(root);
            foreach (var t in root.GetComponentsInChildren<Transform>()) t.gameObject.isStatic = true;

            Directory.CreateDirectory(Path.GetDirectoryName(BackdropPath));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, BackdropPath);
            Object.DestroyImmediate(root);
            return WriteTheme(prefab);
        }

        /// <summary>Puts furniture against the left (-1) or right (+1) wall at depth z.</summary>
        private static GameObject AgainstSideWall(GameObject furniture, float side, float z)
        {
            var bounds = ArtMeshStats.ModelBounds(furniture);
            float x = side * (WallInnerX - WallGap) - (side > 0 ? bounds.max.x : bounds.min.x);
            furniture.transform.position = new Vector3(x, 0f, z - bounds.center.z);
            return furniture;
        }

        /// <summary>Puts furniture against the back wall, towards the left (-1) or right (+1) corner.</summary>
        private static GameObject AgainstBackWall(GameObject furniture, float side)
        {
            var bounds = ArtMeshStats.ModelBounds(furniture);
            float x = side * (WallInnerX - WallGap) - (side > 0 ? bounds.max.x : bounds.min.x);
            furniture.transform.position = new Vector3(x, 0f, BackWallZ - WallGap - bounds.max.z);
            return furniture;
        }

        /// <summary>A warm pendant light under the shade (the room shell's Pendant), no shadows (mobile).</summary>
        private static void AddLampLight(GameObject root)
        {
            var light = new GameObject("Pendant Light").AddComponent<Light>();
            light.transform.SetParent(root.transform, false);
            light.transform.position = PendantBulb;
            light.type = LightType.Point;
            light.color = LampColor;
            light.intensity = LampIntensity;
            light.range = LampRange;
            light.shadows = LightShadows.None;
        }

        private static GameObject Place(GameObject root, string model, string atlas, Vector3 position, float yaw, float scale = 1f)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Art + model)
                        ?? throw new FileNotFoundException($"Backdrop model missing: {model}");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, root.transform);
            instance.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            instance.transform.localScale = Vector3.one * scale;
            var material = AtlasMaterialBuilder.Load(atlas);
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            return instance;
        }

        /// <summary>
        /// A box collider around furniture that falling props can reach, so they land on it, not through it. It counts
        /// as floor: a prop that lands on other furniture is off the desk, so it breaks and is cleared (spec §4.3).
        /// </summary>
        private static GameObject Solid(GameObject furniture)
        {
            var bounds = ArtMeshStats.ModelBounds(furniture);
            var box = furniture.AddComponent<BoxCollider>();
            box.center = bounds.center;
            box.size = bounds.size;
            furniture.AddComponent<FloorSurface>();
            return furniture;
        }

        /// <summary>Walls stop stray balls and props; the floor collider lives in the scene so breaking works in any room.</summary>
        private static void AddRoomColliders(GameObject root)
        {
            Wall(root, "Back Wall Collider", new Vector3(0f, 1.55f, BackWallZ + 0.05f), new Vector3(3.5f, 3.1f, 0.1f));
            Wall(root, "Left Wall Collider", new Vector3(-WallInnerX - 0.05f, 1.55f, -0.5f), new Vector3(0.1f, 3.1f, 8f));
            Wall(root, "Right Wall Collider", new Vector3(WallInnerX + 0.05f, 1.55f, -0.5f), new Vector3(0.1f, 3.1f, 8f));
        }

        private static void Wall(GameObject root, string name, Vector3 center, Vector3 size)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(root.transform, false);
            wall.transform.position = center;
            wall.AddComponent<BoxCollider>().size = size;
        }

        private static RoomTheme WriteTheme(GameObject backdrop)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ThemePath));
            var theme = AssetDatabase.LoadAssetAtPath<RoomTheme>(ThemePath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<RoomTheme>();
                AssetDatabase.CreateAsset(theme, ThemePath);
            }
            theme.backdropPrefab = backdrop;
            theme.music = AssetDatabase.LoadAssetAtPath<AudioClip>(Music);
            theme.background = Background;
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            return theme;
        }
    }
}
