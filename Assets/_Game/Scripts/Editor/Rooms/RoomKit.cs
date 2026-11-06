using System.IO;
using Game.Levels;
using Game.World;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// What every room builder shares: the room shell's walls, placing pack or generated models, furniture against the
    /// walls, solid furniture, lights, the desk look, and writing the backdrop prefab and RoomTheme. Room geometry is
    /// the same in every room (the shell differs only in colour), so the wall constants hold everywhere.
    /// </summary>
    public static class RoomKit
    {
        public const string DeskLookName = "Desk Look";
        public const string Art = "Assets/_Game/Art/";
        public const string KayKit = "KayKitFurniture";
        public const string TinyTreats = "TinyTreats";
        public const float WallInnerX = 1.65f;   // room shell side walls (1.75) minus their thickness
        public const float BackWallZ = 3.4f;     // room shell back wall's inner face
        public const float RugThickness = 0.01f; // under JuiceConfig.splatLift, so splats show on rugs
        private const float WallGap = 0.05f;
        private const float WallHeight = 3.1f;

        public static GameObject Place(GameObject root, string model, string atlas, Vector3 position, float yaw, float scale = 1f)
        {
            var instance = Instantiate(root, model, atlas, position, yaw, scale);
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return instance;
        }

        /// <summary>
        /// A rug flattened to <see cref="RugThickness"/>: the floor collider is at y 0, so a thick rug would swallow the
        /// pieces and splats that land on it (Kitchen spec §6).
        /// </summary>
        public static GameObject PlaceRug(GameObject root, string model, string atlas, Vector3 position, float scale = 1f)
        {
            var rug = Place(root, model, atlas, position, 0f, scale);
            float height = ArtMeshStats.ModelBounds(rug).size.y;
            if (height > RugThickness)
            {
                var size = rug.transform.localScale;
                rug.transform.localScale = new Vector3(size.x, size.y * RugThickness / height, size.z);
            }
            return rug;
        }

        /// <summary>
        /// The room's desk look at the origin (top at LevelSpawner.DeskTopY), over the scene's collider-only play desk.
        /// It keeps its shadow, as the M2 desk did.
        /// </summary>
        public static GameObject PlaceDesk(GameObject root, string model, string atlas)
        {
            var desk = Instantiate(root, model, atlas, Vector3.zero, 0f, 1f);
            desk.name = DeskLookName;
            return desk;
        }

        /// <summary>Puts furniture against the left (-1) or right (+1) wall at depth z.</summary>
        public static GameObject AgainstSideWall(GameObject furniture, float side, float z)
        {
            var bounds = ArtMeshStats.ModelBounds(furniture);
            float x = side * (WallInnerX - WallGap) - (side > 0 ? bounds.max.x : bounds.min.x);
            furniture.transform.position = new Vector3(x, furniture.transform.position.y, z - bounds.center.z);
            return furniture;
        }

        /// <summary>Puts furniture against the back wall, towards the left (-1) or right (+1) corner.</summary>
        public static GameObject AgainstBackWall(GameObject furniture, float side)
        {
            var bounds = ArtMeshStats.ModelBounds(furniture);
            float x = side * (WallInnerX - WallGap) - (side > 0 ? bounds.max.x : bounds.min.x);
            furniture.transform.position = new Vector3(x, furniture.transform.position.y, BackWallZ - WallGap - bounds.max.z);
            return furniture;
        }

        /// <summary>
        /// Turned or scaled furniture flat against the left (-1) or right (+1) wall, its world footprint centred at depth
        /// z (AgainstSideWall measures unturned, unscaled models; Room 1 keeps using it so its look does not move).
        /// </summary>
        public static GameObject FlushSideWall(GameObject furniture, float side, float z)
        {
            var bounds = WorldBounds(furniture);
            float x = side * (WallInnerX - WallGap) - (side > 0 ? bounds.max.x : bounds.min.x);
            furniture.transform.position += new Vector3(x, 0f, z - bounds.center.z);
            return furniture;
        }

        /// <summary>Turned or scaled furniture flat against the back wall, its world footprint centred at x.</summary>
        public static GameObject FlushBackWall(GameObject furniture, float x)
        {
            var bounds = WorldBounds(furniture);
            furniture.transform.position += new Vector3(x - bounds.center.x, 0f, BackWallZ - WallGap - bounds.max.z);
            return furniture;
        }

        /// <summary>Stands a model on top of another placed one, centred on it, offset in x and z.</summary>
        public static GameObject OnTopOf(GameObject root, GameObject below, string model, string atlas, float dx, float dz,
            float yaw, float scale)
        {
            var bounds = WorldBounds(below);
            return Place(root, model, atlas, new Vector3(bounds.center.x + dx, bounds.max.y, bounds.center.z + dz), yaw, scale);
        }

        /// <summary>The scene-space bounds of a placed model's renderers (rotation and scale included).</summary>
        public static Bounds WorldBounds(GameObject placed)
        {
            var renderers = placed.GetComponentsInChildren<Renderer>();
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            return bounds;
        }

        /// <summary>
        /// A box collider around furniture that falling props can reach, so they land on it, not through it. It counts
        /// as floor: a prop that lands on other furniture is off the desk, so it breaks and is cleared (spec §4.3).
        /// </summary>
        public static GameObject Solid(GameObject furniture)
        {
            var bounds = ArtMeshStats.ModelBounds(furniture);
            var box = furniture.AddComponent<BoxCollider>(); // scaled and turned with the furniture
            box.center = bounds.center; // ModelBounds is in the furniture's own space
            box.size = bounds.size;
            furniture.AddComponent<FloorSurface>();
            return furniture;
        }

        /// <summary>A point light with no shadows (mobile).</summary>
        public static Light AddLight(GameObject root, string name, Vector3 position, Color colour, float intensity, float range)
        {
            var light = new GameObject(name).AddComponent<Light>();
            light.transform.SetParent(root.transform, false);
            light.transform.position = position;
            light.type = LightType.Point;
            light.color = colour;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
            return light;
        }

        /// <summary>Walls stop stray balls and props; the floor collider lives in the scene so breaking works in any room.</summary>
        public static void AddRoomColliders(GameObject root)
        {
            Wall(root, "Back Wall Collider", new Vector3(0f, WallHeight / 2f, BackWallZ + 0.05f), new Vector3(3.5f, WallHeight, 0.1f));
            Wall(root, "Left Wall Collider", new Vector3(-WallInnerX - 0.05f, WallHeight / 2f, -0.5f), new Vector3(0.1f, WallHeight, 8f));
            Wall(root, "Right Wall Collider", new Vector3(WallInnerX + 0.05f, WallHeight / 2f, -0.5f), new Vector3(0.1f, WallHeight, 8f));
        }

        /// <summary>Marks everything static, saves the backdrop prefab and destroys the scene copy.</summary>
        public static GameObject SaveBackdrop(GameObject root, string path)
        {
            AddRoomColliders(root);
            foreach (var t in root.GetComponentsInChildren<Transform>()) t.gameObject.isStatic = true;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        public static RoomTheme WriteTheme(string path, GameObject backdrop, string musicPath, Color background)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var theme = AssetDatabase.LoadAssetAtPath<RoomTheme>(path);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<RoomTheme>();
                AssetDatabase.CreateAsset(theme, path);
            }
            theme.backdropPrefab = backdrop;
            theme.music = AssetDatabase.LoadAssetAtPath<AudioClip>(musicPath)
                          ?? throw new FileNotFoundException($"Room music missing: {musicPath}");
            theme.background = background;
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            return theme;
        }

        private static GameObject Instantiate(GameObject root, string model, string atlas, Vector3 position, float yaw, float scale)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Art + model)
                        ?? throw new FileNotFoundException($"Backdrop model missing: {model}");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, root.transform);
            instance.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            instance.transform.localScale = Vector3.one * scale;
            var material = AtlasMaterialBuilder.Load(atlas);
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = material;
            return instance;
        }

        private static void Wall(GameObject root, string name, Vector3 center, Vector3 size)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(root.transform, false);
            wall.transform.position = center;
            wall.AddComponent<BoxCollider>().size = size;
        }
    }
}
