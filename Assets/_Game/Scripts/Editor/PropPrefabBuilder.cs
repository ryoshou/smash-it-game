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
        private static readonly Vector3 KeycapEnterSize = new Vector3(0.225f, 0.15f, 0.11f);
        private const float KeycapEnterMass = 0.06f;
        private static readonly Vector3 LayFlat = new Vector3(0f, 0f, 90f);
        private const float DeskLampScale = 0.44f; // the KayKit lamp is 1 m tall; a desk lamp is ~0.45 m
        private const string BakedGoods = "TinyTreatsBakedGoods";
        private static readonly string[] DonutLooks = { "donut", "donut_pink", "donut_chocolate" };
        private static readonly string[] DonutBrokenSets = { "Donut", "Donut_pink", "Donut_chocolate" };
        // Tiny Treats models share the mug's toy scale (a mug is 0.585 wide); these keep the new kinds in proportion.
        private const float PlateScale = 0.6f;      // 0.45 wide: a mug's 0.43-deep base fits on it
        private const float DonutScale = 0.8f;      // 0.34 wide
        private const float MilkBottleScale = 0.9f; // 0.42 tall, taller than a mug
        private const string Halloween = "KayKitHalloween";
        private const string BoardGame = "KayKitBoardGame";
        private static readonly string[] CandleLooks = { "candle", "candle_thin" };
        private static readonly string[] PencilCupColours = { "teal", "rose", "butter" };
        private const float CandleScale = 0.5f;     // the Halloween candle is 0.79 tall; 0.39 sits beside a mug
        private const float HourglassScale = 0.16f; // the board-game hourglass is 2.5 tall; 0.4
        private static readonly string[] CanColours = { "red", "blue", "grass" };
        private static readonly string[] ControllerColours = { "charcoal", "red", "sky" };

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
            public float visualScale = 1f;
            public bool fizzFuse;
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
            // Room 1 new kinds (spec §6.1): a featherweight that flies, and a top-heavy one that topples.
            yield return new Source
            {
                kind = PropKind.PaperCup, atlas = TinyTreats, collider = PropColliderShape.ConvexMesh, mass = 0.03f,
                physics = WritePhysicsMaterial("PaperCup", 0.4f, 0.4f, 0.3f), breakMode = BreakMode.Pop,
                variants = new[] { Art + "Packs/TinyTreats/coffee_cup_takeaway.fbx" },
            };
            yield return new Source
            {
                kind = PropKind.DeskLamp, atlas = KayKit, collider = PropColliderShape.ConvexMesh, mass = 0.6f,
                physics = WritePhysicsMaterial("DeskLamp", 0.5f, 0.6f, 0.05f), breakMode = BreakMode.Pop,
                variants = new[] { Art + "Packs/KayKitFurniture/lamp_table.fbx" }, visualScale = DeskLampScale,
            };
            foreach (var source in CafeSources()) yield return source;
            foreach (var source in LibrarySources()) yield return source;
            foreach (var source in DenSources()) yield return source;
        }

        /// <summary>Room 2 new kinds (spec §6.1): a thin plate that shatters, a featherweight ring, a top-heavy bottle.</summary>
        private static IEnumerable<Source> CafeSources()
        {
            yield return new Source
            {
                kind = PropKind.Plate, atlas = TinyTreats, collider = PropColliderShape.ConvexMesh, mass = 0.25f,
                physics = WritePhysicsMaterial("Plate", 0.4f, 0.5f, 0.05f), breakMode = BreakMode.Shatter,
                variants = new[] { Art + "Packs/TinyTreats/plate.fbx" },
                broken = new[] { Art + "Generated/Plate_broken.fbx" }, visualScale = PlateScale,
            };
            yield return new Source
            {
                // A box, not its hull: the icing domes the hull's top, and stacks of domes never settle.
                kind = PropKind.Donut, atlas = BakedGoods, collider = PropColliderShape.Box, mass = 0.06f,
                physics = WritePhysicsMaterial("Donut", 0.6f, 0.7f, 0.2f), breakMode = BreakMode.Splash,
                variants = Paths($"Packs/{BakedGoods}/{{0}}.fbx", DonutLooks),
                broken = Paths("Generated/{0}_broken.fbx", DonutBrokenSets), visualScale = DonutScale,
            };
            yield return new Source
            {
                kind = PropKind.MilkBottle, atlas = TinyTreats, collider = PropColliderShape.ConvexMesh, mass = 0.35f,
                physics = WritePhysicsMaterial("MilkBottle", 0.4f, 0.5f, 0.05f), breakMode = BreakMode.Shatter,
                variants = new[] { Art + "Packs/TinyTreats/milk.fbx" },
                broken = new[] { Art + "Generated/MilkBottle_broken.fbx" }, visualScale = MilkBottleScale,
            };
        }

        /// <summary>Room 3 new kinds: a tall candle that topples, a heavy glass hourglass, a pencil cup.</summary>
        private static IEnumerable<Source> LibrarySources()
        {
            yield return new Source
            {
                kind = PropKind.Candle, atlas = Halloween, collider = PropColliderShape.ConvexMesh, mass = 0.15f,
                physics = WritePhysicsMaterial("Candle", 0.6f, 0.7f, 0.05f), breakMode = BreakMode.Pop,
                variants = Paths($"Packs/{Halloween}/{{0}}.fbx", CandleLooks), visualScale = CandleScale,
            };
            yield return new Source
            {
                kind = PropKind.Hourglass, atlas = BoardGame, collider = PropColliderShape.ConvexMesh, mass = 0.4f,
                physics = WritePhysicsMaterial("Hourglass", 0.4f, 0.5f, 0.05f), breakMode = BreakMode.Shatter,
                variants = new[] { Art + $"Packs/{BoardGame}/hourglass.fbx" },
                broken = new[] { Art + "Generated/Hourglass_broken.fbx" }, visualScale = HourglassScale,
            };
            yield return new Source
            {
                kind = PropKind.PencilCup, atlas = TinyTreats, collider = PropColliderShape.ConvexMesh, mass = 0.25f,
                physics = WritePhysicsMaterial("PencilCup", 0.5f, 0.6f, 0.05f), breakMode = BreakMode.Pop,
                variants = Paths("Generated/PencilCup_{0}.fbx", PencilCupColours),
            };
        }

        /// <summary>Room 4 new kinds: a light can that flies and stacks, a flat controller that slides.</summary>
        private static IEnumerable<Source> DenSources()
        {
            yield return new Source
            {
                kind = PropKind.SodaCan, atlas = TinyTreats, collider = PropColliderShape.ConvexMesh, mass = 0.12f,
                physics = WritePhysicsMaterial("SodaCan", 0.4f, 0.5f, 0.15f), breakMode = BreakMode.Pop,
                variants = Paths("Generated/SodaCan_{0}.fbx", CanColours),
            };
            yield return new Source
            {
                kind = PropKind.GameController, atlas = TinyTreats, collider = PropColliderShape.ConvexMesh, mass = 0.2f,
                physics = WritePhysicsMaterial("GameController", 0.6f, 0.7f, 0.1f), breakMode = BreakMode.Pop,
                variants = Paths("Generated/GameController_{0}.fbx", ControllerColours),
            };
            // The special (spec §6.2): a can that bursts when knocked over.
            yield return new Source
            {
                kind = PropKind.FizzySoda, atlas = TinyTreats, collider = PropColliderShape.ConvexMesh, mass = 0.12f,
                physics = WritePhysicsMaterial("SodaCan", 0.4f, 0.5f, 0.15f), breakMode = BreakMode.Burst,
                variants = new[] { Art + "Generated/FizzySoda.fbx" }, fizzFuse = true,
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
            visual.transform.localScale = Vector3.one * source.visualScale;
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
            root.AddComponent<FadeOut>();
            if (source.fizzFuse) root.AddComponent<FizzFuse>();
            return Save(root, name);
        }

        /// <summary>Pieces placed exactly like the intact visual, each its own convex body; inactive until a break.</summary>
        private static GameObject BuildBroken(Source source, string modelPath, string name, Material atlas, Vector3 intactCenter)
        {
            var root = new GameObject(name);
            var visual = AddVisual(root, modelPath, source.visualEuler, atlas);
            visual.transform.localScale = Vector3.one * source.visualScale;
            visual.transform.localPosition = -intactCenter;
            var pieces = visual.GetComponentsInChildren<MeshFilter>();
            var targets = new Transform[pieces.Length];
            for (int i = 0; i < pieces.Length; i++)
            {
                AddConvex(pieces[i], source.physics);
                var body = pieces[i].gameObject.AddComponent<Rigidbody>();
                body.mass = source.mass / pieces.Length;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                targets[i] = pieces[i].transform;
            }
            SetTargets(root.AddComponent<FadeOut>(), targets);
            root.SetActive(false);
            return Save(root, name);
        }

        private static void SetTargets(FadeOut fade, Transform[] targets)
        {
            var so = new SerializedObject(fade);
            var array = so.FindProperty("targets");
            array.arraySize = targets.Length;
            for (int i = 0; i < targets.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = targets[i];
            so.ApplyModifiedPropertiesWithoutUndo();
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
