using System.Collections.Generic;
using System.IO;
using Game.Core;
using Game.Levels;
using Game.World;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// Builds every prop prefab (one per colour variant), its pre-fractured set and the PropCatalog from the art in
    /// Assets/_Game/Art. Keycap sizes, masses and physics stay M1's (checkpoint 1 approved them). Safe to re-run.
    /// </summary>
    public static class PropPrefabBuilder
    {
        public const string PropsFolder = "Assets/_Game/Prefabs/Props";
        public const string PhysicsFolder = "Assets/_Game/Materials";
        private const string Art = "Assets/_Game/Art/";
        private const string TinyTreats = "TinyTreats";
        private const string KayKit = "KayKitFurniture";
        private static readonly string[] KeycapColours = { "cream", "pink", "sky" };
        private static readonly string[] MugColours = { "pink", "blue", "yellow" };
        private static readonly Vector3 KeycapEnterSize = new Vector3(0.225f, 0.11f, 0.15f);
        private const float KeycapEnterMass = 0.06f;
        private static readonly Vector3 LayFlat = new Vector3(0f, 0f, 90f);

        private sealed class Source
        {
            public PropKind kind;
            public string[] variants;
            public string[] broken;
            public string atlas;
            public PropColliderShape collider;
            public float mass;
            public PhysicsMaterial physics;
            public BreakMode breakMode;
            public Vector3? size;
            public Vector3 visualEuler;
        }

        [MenuItem("Smash/Build Props")]
        public static void BuildAll()
        {
            AtlasMaterialBuilder.BuildAll();
            Directory.CreateDirectory(PropsFolder);
            var catalog = LoadOrCreateCatalog();
            var specs = new List<PropSpec>();
            foreach (var source in Sources()) specs.Add(Build(source));
            catalog.specs = specs.ToArray();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }

        public static PhysicsMaterial WritePhysicsMaterial(string name, float dynamicFriction, float staticFriction, float bounce)
        {
            string path = $"{PhysicsFolder}/{name}.physicMaterial";
            var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);
            if (material == null)
            {
                material = new PhysicsMaterial(name);
                AssetDatabase.CreateAsset(material, path);
            }
            material.dynamicFriction = dynamicFriction;
            material.staticFriction = staticFriction;
            material.bounciness = bounce;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static IEnumerable<Source> Sources()
        {
            var keycap = WritePhysicsMaterial("Keycap", 0.6f, 0.8f, 0.05f);
            foreach (var kind in new[] { PropKind.Keycap1u, PropKind.Keycap2u, PropKind.KeycapSpace })
                yield return Keycap(kind, PrototypeLevels.Size(kind), PrototypeLevels.Mass(kind), keycap);
            yield return Keycap(PropKind.KeycapEnter, KeycapEnterSize, KeycapEnterMass, keycap);

            yield return new Source
            {
                kind = PropKind.Mug, atlas = TinyTreats, collider = PropColliderShape.ConvexMesh, mass = 0.3f,
                physics = WritePhysicsMaterial("Mug", 0.5f, 0.5f, 0.1f), breakMode = BreakMode.Shatter,
                variants = Paths("Packs/TinyTreats/mug_A_{0}.fbx", MugColours),
                broken = Paths("Generated/Mug_{0}_broken.fbx", MugColours),
            };
            yield return new Source
            {
                kind = PropKind.Book, atlas = KayKit, collider = PropColliderShape.Box, mass = 0.5f,
                physics = WritePhysicsMaterial("Book", 0.45f, 0.45f, 0f), breakMode = BreakMode.Pop,
                variants = new[] { Art + "Packs/KayKitFurniture/book_single.fbx" }, visualEuler = LayFlat,
            };
            yield return new Source
            {
                kind = PropKind.CactusPot, atlas = KayKit, collider = PropColliderShape.ConvexMesh, mass = 1.2f,
                physics = WritePhysicsMaterial("CactusPot", 0.7f, 0.7f, 0.05f), breakMode = BreakMode.Shatter,
                variants = new[] { Art + "Packs/KayKitFurniture/cactus_small_A.fbx" },
                broken = new[] { Art + "Generated/CactusPot_broken.fbx" },
            };
        }

        private static Source Keycap(PropKind kind, Vector3 size, float mass, PhysicsMaterial physics) => new Source
        {
            kind = kind, atlas = TinyTreats, collider = PropColliderShape.Box, mass = mass, physics = physics,
            breakMode = BreakMode.Pop, size = size, variants = Paths($"Generated/{kind}_{{0}}.fbx", KeycapColours),
        };

        private static string[] Paths(string pattern, string[] colours)
        {
            var paths = new string[colours.Length];
            for (int i = 0; i < colours.Length; i++) paths[i] = Art + string.Format(pattern, colours[i]);
            return paths;
        }

        private static PropSpec Build(Source source)
        {
            var atlas = AtlasMaterialBuilder.Load(source.atlas);
            var spec = new PropSpec
            {
                kind = source.kind, mass = source.mass, material = source.physics, collider = source.collider,
                breakMode = source.breakMode, variants = new PropVariant[source.variants.Length],
            };
            for (int i = 0; i < source.variants.Length; i++)
            {
                string name = $"{source.kind}_{i}";
                var prefab = BuildIntact(source, source.variants[i], name, atlas, out var bounds);
                spec.size = source.size ?? bounds.size;
                spec.variants[i].prefab = prefab;
                if (source.broken != null)
                    spec.variants[i].brokenPrefab = BuildBroken(source, source.broken[i], name + "_Broken", atlas, bounds.center);
            }
            return spec;
        }

        /// <summary>Root at the visual's bounds centre (the spawner places props by their centre), collider, body.</summary>
        private static GameObject BuildIntact(Source source, string modelPath, string name, Material atlas, out Bounds bounds)
        {
            var root = new GameObject(name);
            var visual = AddVisual(root, modelPath, source.visualEuler, atlas);
            bounds = ArtMeshStats.ModelBounds(root);
            visual.transform.localPosition = -bounds.center;

            if (source.collider == PropColliderShape.Box)
                root.AddComponent<BoxCollider>().size = source.size ?? bounds.size;
            else
                foreach (var filter in visual.GetComponentsInChildren<MeshFilter>())
                    AddConvex(filter, source.physics);
            if (root.TryGetComponent(out Collider box)) box.sharedMaterial = source.physics;

            var body = root.AddComponent<Rigidbody>();
            body.mass = source.mass;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            root.AddComponent<DeskObject>();
            return Save(root, name);
        }

        /// <summary>Pieces placed exactly like the intact visual, each its own convex body; inactive until a break.</summary>
        private static GameObject BuildBroken(Source source, string modelPath, string name, Material atlas, Vector3 intactCenter)
        {
            var root = new GameObject(name);
            var visual = AddVisual(root, modelPath, source.visualEuler, atlas);
            visual.transform.localPosition = -intactCenter;
            var pieces = visual.GetComponentsInChildren<MeshFilter>();
            foreach (var filter in pieces)
            {
                AddConvex(filter, source.physics);
                var body = filter.gameObject.AddComponent<Rigidbody>();
                body.mass = source.mass / pieces.Length;
                body.interpolation = RigidbodyInterpolation.Interpolate;
            }
            root.SetActive(false);
            return Save(root, name);
        }

        private static GameObject AddVisual(GameObject root, string modelPath, Vector3 euler, Material atlas)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null) throw new FileNotFoundException($"Prop model missing: {modelPath} (run tools/assets/import-packs.sh or tools/blender/build.sh)");
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
            visual.name = "Visual";
            visual.transform.localRotation = Quaternion.Euler(euler);
            foreach (var renderer in visual.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = atlas;
            return visual;
        }

        private static void AddConvex(MeshFilter filter, PhysicsMaterial physics)
        {
            var collider = filter.gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = filter.sharedMesh;
            collider.convex = true;
            collider.sharedMaterial = physics;
        }

        private static GameObject Save(GameObject root, string name)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{PropsFolder}/{name}.prefab");
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static PropCatalog LoadOrCreateCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<PropCatalog>(PrototypeAssetBuilder.CatalogPath);
            if (catalog != null) return catalog;
            catalog = ScriptableObject.CreateInstance<PropCatalog>();
            AssetDatabase.CreateAsset(catalog, PrototypeAssetBuilder.CatalogPath);
            return catalog;
        }
    }
}
