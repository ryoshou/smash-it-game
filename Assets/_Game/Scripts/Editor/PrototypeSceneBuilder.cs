using System.IO;
using Game.Audio;
using Game.Controls;
using Game.Core;
using Game.Juice;
using Game.Levels;
using Game.View;
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
        public const string DeskPrefabPath = PrefabsFolder + "/Desk.prefab";
        private const string MaterialsFolder = "Assets/_Game/Materials";
        private const string ConfigFolder = PrototypeAssetBuilder.ConfigFolder;

        private static readonly string[] BootstrapFields =
        {
            "tuningAsset", "levels", "catalog", "spawner", "pool", "pouch", "cam", "input", "view", "hud",
            "juice", "sfx", "shake", "shotFeedback", "impactFeedback", "debris",
            "propFeel", "vfx", "propFeedback", "winFeedback", "confetti", "confettiAnchor", "roomPresenter",
            "catReactions", "catAnimator",
        };
        private const string LitShader = "Universal Render Pipeline/Lit";

        private static readonly Vector3 DeskSize = new Vector3(2.0f, 0.08f, 1.5f);
        private const float LegSize = 0.07f;
        private const float BackDeskThickness = 0.06f;
        private const float YarnModelDiameter = 0.18f;
        private const float SunIntensity = 0.85f;
        private static readonly Color SunColor = new Color(1f, 0.93f, 0.84f);
        private static readonly Color AmbientCeiling = new Color(0.93f, 0.85f, 0.76f);
        private static readonly Color AmbientWalls = new Color(0.78f, 0.64f, 0.55f);
        private static readonly Color AmbientFloor = new Color(0.5f, 0.38f, 0.3f);
        private static readonly Vector3 CatPosition = new Vector3(-0.34f, 0f, -1.95f);
        private const float CatYaw = 155f;
        private static readonly Vector3 CameraPosition = new Vector3(0f, 2.2f, -4.2f);
        private static readonly Vector3 CameraLookAt = new Vector3(0f, 0.7f, 0f);
        private static readonly Vector3 PouchPosition = new Vector3(0f, 1.1f, -2.4f);
        private static readonly Color Background = new Color(0.98f, 0.89f, 0.84f);

        [MenuItem("Smash/Rebuild Prototype Scene")]
        public static void Rebuild()
        {
            PrototypeAssetBuilder.RebuildAll();
            MetaAssetBuilder.Build();
            Directory.CreateDirectory(PrefabsFolder);
            Directory.CreateDirectory(MaterialsFolder);
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));

            PropPrefabBuilder.BuildAll();
            VfxBuilder.BuildAll();
            SoundSetBuilder.BuildAll();
            HomeOfficeBuilder.Build();
            var desk = WriteDeskPrefab();
            var backDesk = WriteBackDeskPrefab();
            var ball = WriteBallPrefab();
            BuildScene(desk, backDesk, ball);
        }

        /// <summary>
        /// M1's collider boxes (top surface at DeskTopY), so play is unchanged. No renderer: each room's backdrop carries
        /// its own desk look over them (RoomKit.PlaceDesk).
        /// </summary>
        private static GameObject WriteDeskPrefab()
        {
            var root = new GameObject("Desk");
            ColliderBox(root.transform, "Top Collider", new Vector3(0f, LevelSpawner.DeskTopY - DeskSize.y / 2f, 0f), DeskSize);
            float legHeight = LevelSpawner.DeskTopY - DeskSize.y;
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -1f : 1f) * (DeskSize.x / 2f - LegSize);
                float z = (i < 2 ? -1f : 1f) * (DeskSize.z / 2f - LegSize);
                ColliderBox(root.transform, $"Leg {i} Collider", new Vector3(x, legHeight / 2f, z), new Vector3(LegSize, legHeight, LegSize));
            }
            return SavePrefab(root, "Desk");
        }

        /// <summary>
        /// The raised back desk: a collider top at LevelSpawner.BackDeskTop with a KayKit small table stretched to fit
        /// under it. No FloorSurface: props landing on it are still on a desk.
        /// </summary>
        private static GameObject WriteBackDeskPrefab()
        {
            var root = new GameObject("Back Desk");
            var top = LevelSpawner.BackDeskTop;
            var size = LevelDef.DefaultBackDeskSize;
            var visual = ArtVisual(root.transform, "Packs/KayKitFurniture/table_small.fbx", "KayKitFurniture", Vector3.zero, 1f);
            var bounds = RendererBounds(visual);
            visual.localScale = new Vector3(size.x / bounds.size.x, top.y / bounds.size.y, size.y / bounds.size.z);
            visual.localPosition = new Vector3(top.x - bounds.center.x * visual.localScale.x, -bounds.min.y * visual.localScale.y,
                top.z - bounds.center.z * visual.localScale.z);
            ColliderBox(root.transform, "Top Collider", new Vector3(top.x, top.y - BackDeskThickness / 2f, top.z),
                new Vector3(size.x, BackDeskThickness, size.y));
            return SavePrefab(root, "BackDesk");
        }

        private static Bounds RendererBounds(Transform visual)
        {
            var renderers = visual.GetComponentsInChildren<Renderer>();
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            return bounds;
        }

        private static void ColliderBox(Transform parent, string name, Vector3 center, Vector3 size)
        {
            var box = new GameObject(name).AddComponent<BoxCollider>();
            box.transform.SetParent(parent, false);
            box.transform.localPosition = center;
            box.size = size;
        }

        internal static Transform ArtVisual(Transform parent, string model, string atlas, Vector3 localPosition, float scale)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/" + model)
                        ?? throw new FileNotFoundException($"Model missing: {model}");
            var visual = ((GameObject)PrefabUtility.InstantiatePrefab(asset, parent)).transform;
            visual.name = "Visual";
            visual.localPosition = localPosition;
            visual.localScale = Vector3.one * scale;
            var material = AtlasMaterialBuilder.Load(atlas);
            foreach (var renderer in visual.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = material;
            return visual;
        }

        /// <summary>Unit sphere collider (YarnBall scales it to 2 x ballRadius at launch) with the yarn mesh inside.</summary>
        private static GameObject WriteBallPrefab()
        {
            var ball = new GameObject("YarnBall");
            ball.AddComponent<SphereCollider>().radius = 0.5f;
            var body = ball.AddComponent<Rigidbody>();
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.linearDamping = 0f;
            body.angularDamping = 0f;
            ball.AddComponent<YarnBall>();
            ArtVisual(ball.transform, "Generated/YarnBall.fbx", "TinyTreats", Vector3.zero, 1f / YarnModelDiameter);
            return SavePrefab(ball, "YarnBall");
        }

        private static void BuildScene(GameObject deskPrefab, GameObject backDeskPrefab, GameObject ballPrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            PrefabUtility.InstantiatePrefab(deskPrefab);
            var backDesk = (GameObject)PrefabUtility.InstantiatePrefab(backDeskPrefab);
            backDesk.name = "Back Desk";
            backDesk.SetActive(false);
            // The room backdrop draws the floor; this collider makes breaking work in every room.
            var floor = new GameObject("Floor");
            var floorBox = floor.AddComponent<BoxCollider>();
            floorBox.center = new Vector3(0f, -0.05f, -0.5f);
            floorBox.size = new Vector3(8f, 0.1f, 8f);
            floor.AddComponent<FloorSurface>();

            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = SunIntensity;
            light.color = SunColor;
            // Warm indoor fill instead of flat daylight (owner, checkpoint 2: feel like being in a room).
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = AmbientCeiling;
            RenderSettings.ambientEquatorColor = AmbientWalls;
            RenderSettings.ambientGroundColor = AmbientFloor;
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
            SetReference(spawner, "backDesk", backDesk);
            var debris = new GameObject("Debris Pool").AddComponent<DebrisPool>();
            var ui = UiSceneParts.Build();
            var appConfig = MetaAssetBuilder.LoadAppConfig();
            var strings = MetaAssetBuilder.LoadStrings();
            var money = MonetizationSceneParts.Build(appConfig, strings);

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
            var debugPanel = UiSceneParts.BuildDebugPanel(ui, game, tuning);
            WireBootstrap(game, new Object[]
            {
                tuning,
                GameLevelListBuilder.BuildAll(),
                AssetDatabase.LoadAssetAtPath<PropCatalog>(PrototypeAssetBuilder.CatalogPath),
                spawner, pool, pouch, camera, input, view, ui.Hud,
                juice, sfx, shake, juiceObject.AddComponent<ShotFeedback>(), juiceObject.AddComponent<ImpactFeedback>(),
                debris, VfxBuilder.LoadOrCreateLibrary(), juiceObject.AddComponent<VfxPool>(),
                juiceObject.AddComponent<PropFeedback>(), juiceObject.AddComponent<WinFeedback>(),
                VfxBuilder.LoadConfetti(), ConfettiAnchor(), Room(camera, juice, juiceObject, out var music),
                juiceObject.AddComponent<CatReactions>(), PlaceCat(),
            });
            var app = MetaSceneParts.Build(ui, money, game, sfx, music, appConfig, strings);
            MetaSceneParts.BuildDebugButtons(debugPanel, app, money.Ads);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        private static RoomPresenter Room(Camera camera, JuiceConfig juice, GameObject juiceObject, out MusicPlayer music)
        {
            music = juiceObject.AddComponent<MusicPlayer>();
            SetReference(music, "config", juice);
            var room = new GameObject("Room").AddComponent<RoomPresenter>();
            SetReference(room, "cam", camera);
            SetReference(room, "music", music);
            SetReference(room, "juice", juice);
            SetReference(room, "fallback", AssetDatabase.LoadAssetAtPath<RoomTheme>(HomeOfficeBuilder.ThemePath));
            return room;
        }

        /// <summary>
        /// The cat sits on the floor just left of the slingshot, turned to face the camera in three-quarter view (its
        /// model faces +z), leaning towards the slingshot. It stays below the desk and away from the preview dots.
        /// </summary>
        private static Animator PlaceCat()
        {
            var prefab = CatAnimatorBuilder.BuildAll();
            var cat = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            cat.transform.SetPositionAndRotation(CatPosition, Quaternion.Euler(0f, CatYaw, 0f));
            return cat.GetComponentInChildren<Animator>();
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
