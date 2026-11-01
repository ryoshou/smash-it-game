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
        private static readonly Vector3 SideTable = new Vector3(-1.45f, 0f, 2.1f);

        [MenuItem("Smash/Build Home Office")]
        public static RoomTheme Build()
        {
            var root = new GameObject("HomeOfficeBackdrop");
            Place(root, "Generated/RoomShell.fbx", TinyTreats, Vector3.zero, 0f);
            Place(root, "Packs/KayKitFurniture/rug_rectangle_stripes_A.fbx", KayKit, new Vector3(0f, 0.002f, 0.2f), 0f);
            var table = Place(root, "Packs/KayKitFurniture/table_medium.fbx", KayKit, SideTable, 0f);
            float tableTop = ArtMeshStats.ModelBounds(table).max.y;
            Place(root, "Generated/Monitor.fbx", TinyTreats, SideTable + new Vector3(0f, tableTop, 0.15f), 0f);
            Place(root, "Generated/Keyboard.fbx", TinyTreats, SideTable + new Vector3(0f, tableTop, -0.25f), 0f, 0.6f);
            Place(root, "Packs/KayKitFurniture/pictureframe_standing_A.fbx", KayKit, SideTable + new Vector3(0.62f, tableTop, -0.05f), 160f, 0.5f);
            Place(root, "Packs/KayKitFurniture/shelf_B_large_decorated.fbx", KayKit, new Vector3(1.4f, 0f, 3.2f), 0f);
            Place(root, "Packs/KayKitFurniture/lamp_standing.fbx", KayKit, new Vector3(1.9f, 0f, 2.2f), 0f);
            Place(root, "Packs/KayKitFurniture/cactus_medium_A.fbx", KayKit, new Vector3(-2.6f, 0f, 2.9f), 0f);
            AddRoomColliders(root);
            foreach (var t in root.GetComponentsInChildren<Transform>()) t.gameObject.isStatic = true;

            Directory.CreateDirectory(Path.GetDirectoryName(BackdropPath));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, BackdropPath);
            Object.DestroyImmediate(root);
            return WriteTheme(prefab);
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

        /// <summary>Walls stop stray balls and props; the floor collider lives in the scene so breaking works in any room.</summary>
        private static void AddRoomColliders(GameObject root)
        {
            var bounds = LevelSpawner.PlayVolume;
            Wall(root, "Back Wall Collider", new Vector3(0f, 2f, 3.45f), new Vector3(8f, 4f, 0.1f));
            Wall(root, "Left Wall Collider", new Vector3(bounds.min.x + 0.05f, 2f, -0.5f), new Vector3(0.1f, 4f, 8f));
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
