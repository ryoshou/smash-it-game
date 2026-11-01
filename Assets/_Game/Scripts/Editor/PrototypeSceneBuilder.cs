using System.IO;
using Game.Audio;
using Game.Controls;
using Game.Core;
using Game.Juice;
using Game.Levels;
using Game.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>Builds the placeholder prefabs, materials and Game scene, and fills the PropCatalog prefab refs.</summary>
    public static class PrototypeSceneBuilder
    {
        public const string ScenePath = "Assets/_Game/Scenes/Game.unity";
        private const string PrefabsFolder = "Assets/_Game/Prefabs";
        private const string MaterialsFolder = "Assets/_Game/Materials";
        private const string ConfigFolder = PrototypeAssetBuilder.ConfigFolder;

        private static readonly string[] BootstrapFields =
        {
            "tuningAsset", "levels", "catalog", "spawner", "pool", "pouch", "cam", "input", "view", "hud", "resultPanel",
            "juice", "sfx", "shake", "shotFeedback", "impactFeedback", "debris",
            "propFeel", "vfx", "propFeedback", "winFeedback", "confetti", "confettiAnchor",
        };
        private const string LitShader = "Universal Render Pipeline/Lit";

        private static readonly Vector3 DeskSize = new Vector3(2.0f, 0.08f, 1.5f);
        private const float LegSize = 0.07f;
        private static readonly Vector3 CameraPosition = new Vector3(0f, 2.2f, -4.2f);
        private static readonly Vector3 CameraLookAt = new Vector3(0f, 0.7f, 0f);
        private static readonly Vector3 PouchPosition = new Vector3(0f, 1.1f, -2.4f);
        private static readonly Color Background = new Color(0.98f, 0.89f, 0.84f);

        [MenuItem("Smash/Rebuild Prototype Scene")]
        public static void Rebuild()
        {
            PrototypeAssetBuilder.RebuildAll();
            Directory.CreateDirectory(PrefabsFolder);
            Directory.CreateDirectory(MaterialsFolder);
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));

            PropPrefabBuilder.BuildAll();
            VfxBuilder.BuildAll();
            SoundSetBuilder.BuildAll();
            var desk = WriteDeskPrefab();
            var ball = WriteBallPrefab();
            BuildScene(desk, ball);
        }

        private static GameObject WriteDeskPrefab()
        {
            var wood = Material("DeskWood", new Color(0.87f, 0.7f, 0.53f));
            var root = new GameObject("Desk");
            var top = Visual(PrimitiveType.Cube, root.transform, "Top", wood);
            top.localScale = DeskSize;
            top.localPosition = new Vector3(0f, LevelSpawner.DeskTopY - DeskSize.y / 2f, 0f);
            top.gameObject.AddComponent<BoxCollider>();

            float legHeight = LevelSpawner.DeskTopY - DeskSize.y;
            for (int i = 0; i < 4; i++)
            {
                var leg = Visual(PrimitiveType.Cube, root.transform, $"Leg {i}", wood);
                leg.localScale = new Vector3(LegSize, legHeight, LegSize);
                float x = (i % 2 == 0 ? -1f : 1f) * (DeskSize.x / 2f - LegSize);
                float z = (i < 2 ? -1f : 1f) * (DeskSize.z / 2f - LegSize);
                leg.localPosition = new Vector3(x, legHeight / 2f, z);
                leg.gameObject.AddComponent<BoxCollider>();
            }
            return SavePrefab(root, "Desk");
        }

        private static GameObject WriteBallPrefab()
        {
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "YarnBall";
            ball.GetComponent<MeshRenderer>().sharedMaterial = Material("Yarn", new Color(0.95f, 0.45f, 0.5f));
            var body = ball.AddComponent<Rigidbody>();
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.linearDamping = 0f;
            body.angularDamping = 0f;
            ball.AddComponent<YarnBall>();
            return SavePrefab(ball, "YarnBall");
        }

        private static void BuildScene(GameObject deskPrefab, GameObject ballPrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            PrefabUtility.InstantiatePrefab(deskPrefab);
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(3f, 1f, 3f);
            floor.GetComponent<MeshRenderer>().sharedMaterial = Material("Floor", new Color(0.93f, 0.85f, 0.95f));
            floor.AddComponent<FloorSurface>();

            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;
            camera.fieldOfView = 60f;
            camera.transform.position = CameraPosition;
            camera.transform.LookAt(CameraLookAt);
            camera.gameObject.AddComponent<AudioListener>();

            var pouch = new GameObject("Pouch").transform;
            pouch.position = PouchPosition;
            var view = SlingshotSceneParts.BuildSlingshot(pouch, MaterialsFolder, Material);
            var pool = new GameObject("Ball Pool").AddComponent<BallPool>();
            SetReference(pool, "ballPrefab", ballPrefab.GetComponent<YarnBall>());
            var spawner = new GameObject("Level Spawner").AddComponent<LevelSpawner>();
            var debris = new GameObject("Debris Pool").AddComponent<DebrisPool>();
            var ui = UiSceneParts.Build();

            var juice = SlingshotSceneParts.LoadOrCreateJuice();
            var shake = camera.gameObject.AddComponent<CameraShake>();
            SetReference(shake, "config", juice);
            var juiceObject = new GameObject("Juice");
            var sfx = juiceObject.AddComponent<SfxPlayer>();
            SetReference(sfx, "config", juice);
            SetReference(sfx, "presets", LoadOrCreateSfxPresets());

            var bootstrap = new GameObject("Bootstrap");
            var input = bootstrap.AddComponent<SlingshotInput>();
            var game = bootstrap.AddComponent<GameBootstrap>();
            var tuning = AssetDatabase.LoadAssetAtPath<ShotTuning>(ConfigFolder + "/ShotTuning.asset");
            UiSceneParts.BuildDebugPanel(ui, game, tuning);
            WireBootstrap(game, new Object[]
            {
                tuning,
                AssetDatabase.LoadAssetAtPath<LevelList>(PrototypeAssetBuilder.LevelListPath),
                AssetDatabase.LoadAssetAtPath<PropCatalog>(PrototypeAssetBuilder.CatalogPath),
                spawner, pool, pouch, camera, input, view, ui.Hud, ui.Result,
                juice, sfx, shake, juiceObject.AddComponent<ShotFeedback>(), juiceObject.AddComponent<ImpactFeedback>(),
                debris, VfxBuilder.LoadOrCreateLibrary(), juiceObject.AddComponent<VfxPool>(),
                juiceObject.AddComponent<PropFeedback>(), juiceObject.AddComponent<WinFeedback>(),
                VfxBuilder.LoadConfetti(), ConfettiAnchor(),
            });

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        private static Transform ConfettiAnchor()
        {
            var anchor = new GameObject("Confetti Anchor").transform;
            anchor.position = new Vector3(0f, LevelSpawner.DeskTopY + 0.2f, 0f);
            return anchor;
        }

        private static SfxPresets LoadOrCreateSfxPresets()
        {
            string path = ConfigFolder + "/SfxPresets.asset";
            var presets = AssetDatabase.LoadAssetAtPath<SfxPresets>(path);
            if (presets != null) return presets;
            presets = ScriptableObject.CreateInstance<SfxPresets>();
            AssetDatabase.CreateAsset(presets, path);
            return presets;
        }

        private static void WireBootstrap(GameBootstrap bootstrap, Object[] references)
        {
            for (int i = 0; i < BootstrapFields.Length; i++)
            {
                if (references[i] == null) throw new System.InvalidOperationException($"Bootstrap {BootstrapFields[i]} is missing.");
                SetReference(bootstrap, BootstrapFields[i], references[i]);
            }
        }

        internal static Transform Visual(PrimitiveType type, Transform parent, string name, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        internal static Material Material(string name, Color color)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find(LitShader));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0.25f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject SavePrefab(GameObject root, string name)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabsFolder}/{name}.prefab");
            Object.DestroyImmediate(root);
            return prefab;
        }

        internal static void SetReference(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
