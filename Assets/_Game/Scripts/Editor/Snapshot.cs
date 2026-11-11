using System;
using System.IO;
using Game.Levels;
using Game.Meta;
using Game.UI;
using Game.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Editor
{
    /// <summary>
    /// Headless visual checks (the GUI editor is not needed): renders the Game scene's camera to a PNG with a level's
    /// props placed, optionally staging a break (shards spread, effects mid-burst). Run via tools/unity/snapshot.sh.
    /// </summary>
    public static class Snapshot
    {
        private const int Width = 1170;
        private const int Height = 2532;
        private const float ShardSpread = 0.18f;

        /// <summary>Command line: -snapshotOut path.png [-snapshotLevel n] [-snapshotBreak] [-snapshotCatPose Take:seconds]
        /// [-snapshotScreen home|pause|rooms] [-snapshotRoom n] [-snapshotTheme path] (show the level in room n's backdrop, or that RoomTheme's, instead of its own).</summary>
        public static void FromCommandLine()
        {
            string[] args = Environment.GetCommandLineArgs();
            string output = Arg(args, "-snapshotOut") ?? throw new ArgumentException("-snapshotOut is required");
            int level = int.TryParse(Arg(args, "-snapshotLevel"), out int n) ? n : 0;
            bool stageBreak = Array.IndexOf(args, "-snapshotBreak") >= 0;
            bool stageWin = Array.IndexOf(args, "-snapshotWin") >= 0;
            int room = int.TryParse(Arg(args, "-snapshotRoom"), out int r) ? r : 0;
            string theme = Arg(args, "-snapshotTheme") ?? (room > 0 ? GameLevelListBuilder.ThemePath(room) : null);
            Capture(output, level, null, stageBreak, Arg(args, "-snapshotCatPose"), Arg(args, "-snapshotScreen"), theme, stageWin);
        }

        public static void Capture(string output, int levelIndex, bool stageBreak, string catPose = null, string screen = null) =>
            Capture(output, levelIndex, null, stageBreak, catPose, screen);

        /// <summary>Renders an unsaved layout (a level being designed) instead of one from the list.</summary>
        public static void Capture(string output, LevelDef layout) => Capture(output, 0, layout, false, null, null);

        private static void Capture(string output, int levelIndex, LevelDef layout, bool stageBreak, string catPose, string screen,
            string themeOverride = null, bool stageWin = false)
        {
            // Opening the scene destroys an unsaved level, so carry it across as JSON.
            string layoutJson = layout != null ? JsonUtility.ToJson(layout) : null;
            EditorSceneManager.OpenScene(PrototypeSceneBuilder.ScenePath, OpenSceneMode.Single);
            if (layoutJson != null)
            {
                layout = ScriptableObject.CreateInstance<LevelDef>();
                JsonUtility.FromJsonOverwrite(layoutJson, layout);
            }
            var bootstrap = UnityEngine.Object.FindAnyObjectByType<GameBootstrap>();
            var so = new SerializedObject(bootstrap);
            var levels = (LevelList)so.FindProperty("levels").objectReferenceValue;
            var catalog = (PropCatalog)so.FindProperty("catalog").objectReferenceValue;
            var level = layout != null ? layout : levels.At(levelIndex);
            ApplyRoom(themeOverride != null ? AssetDatabase.LoadAssetAtPath<RoomTheme>(themeOverride) : level.room);
            PlaceProps(level, catalog);
            if (stageBreak) StageBreak(catalog);
            if (stageWin) Burst(VfxBuilder.LoadConfetti().gameObject, WinConfettiAt, WinConfettiSeconds);
            if (catPose != null) PoseCat(catPose);
            if (screen != null) ShowScreen(screen, levelIndex, levels);
            DrawBands();
            Render(Camera.main, output);
            Debug.Log($"[Snapshot] wrote {output}");
        }

        /// <summary>What RoomPresenter does at runtime: the level's room (or the scene fallback) and its background.</summary>
        private static void ApplyRoom(RoomTheme room)
        {
            var presenter = UnityEngine.Object.FindAnyObjectByType<Game.View.RoomPresenter>();
            var theme = room != null ? room : (RoomTheme)new SerializedObject(presenter).FindProperty("fallback").objectReferenceValue;
            if (theme == null) return;
            if (theme.backdropPrefab != null) UnityEngine.Object.Instantiate(theme.backdropPrefab);
            Camera.main.backgroundColor = theme.background;
        }

        private static void PlaceProps(LevelDef level, PropCatalog catalog)
        {
            var spawner = UnityEngine.Object.FindAnyObjectByType<LevelSpawner>();
            var backDesk = (GameObject)new SerializedObject(spawner).FindProperty("backDesk").objectReferenceValue;
            if (backDesk != null) backDesk.SetActive(level.hasBackDesk);
            foreach (var placement in level.props)
            {
                var spec = catalog.Get(placement.kind);
                var position = LevelSpawner.DeskTop(placement.desk) + placement.basePosition + Vector3.up * (spec.size.y / 2f);
                UnityEngine.Object.Instantiate(spec.Variant(placement.variant).prefab, position, Quaternion.Euler(0f, placement.yawDegrees, 0f));
            }
        }

        /// <summary>
        /// A mug mid-shatter on the floor beside the desk, with the shard sparkle and a dust puff, and a donut splashing
        /// beside it (chunks, droplets and its splat on the floor).
        /// </summary>
        /// <summary>The bands are line renderers SlingshotView places every frame; edit mode runs no frames, so place them once.</summary>
        private static void DrawBands()
        {
            var view = UnityEngine.Object.FindAnyObjectByType<Game.View.SlingshotView>();
            typeof(Game.View.SlingshotView)
                .GetMethod("LateUpdate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(view, null);
        }

        /// <summary>Where the scene's Confetti Anchor sits, and the moment the win snapshot freezes the cannons.</summary>
        private static readonly Vector3 WinConfettiAt = new Vector3(0f, LevelSpawner.DeskTopY + 0.2f, 0f);
        private const float WinConfettiSeconds = 0.9f;

        private static void StageBreak(PropCatalog catalog)
        {
            var at = new Vector3(0.5f, 0.2f, -1.5f);
            Spread(catalog.Get(PropKind.Mug).Variant(0).brokenPrefab, at);
            Burst(AssetDatabase.LoadAssetAtPath<GameObject>(VfxBuilder.VfxFolder + "/ShardSparkle.prefab"), at, 0.12f);
            var splashAt = new Vector3(0.3f, 0.015f, -0.95f);
            Spread(catalog.Get(PropKind.Donut).Variant(1).brokenPrefab, splashAt + Vector3.up * 0.1f);
            Burst(AssetDatabase.LoadAssetAtPath<GameObject>(VfxBuilder.VfxFolder + "/DonutSplash.prefab"), splashAt, 0.15f);
            Burst(AssetDatabase.LoadAssetAtPath<GameObject>(VfxBuilder.VfxFolder + "/DustPuff.prefab"), at + new Vector3(-1.1f, 0f, 0f), 0.15f);
        }

        private static void Spread(GameObject brokenPrefab, Vector3 at)
        {
            var broken = UnityEngine.Object.Instantiate(brokenPrefab, at, Quaternion.identity);
            broken.SetActive(true);
            foreach (var body in broken.GetComponentsInChildren<Rigidbody>())
            {
                var outward = body.transform.position - at;
                body.transform.position += outward.normalized * ShardSpread;
            }
        }

        /// <summary>"Take:seconds" — samples one of the cat's takes onto the scene cat (edit mode cannot run the Animator).</summary>
        private static void PoseCat(string pose)
        {
            string[] parts = pose.Split(':');
            float seconds = parts.Length > 1 && float.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float s) ? s : 0f;
            var animator = GameObject.Find("Cat").GetComponentInChildren<Animator>();
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(CatAnimatorBuilder.ModelPath))
                if (asset is AnimationClip clip && clip.name == parts[0])
                    clip.SampleAnimation(animator.gameObject, seconds);
        }

        /// <summary>Shows a UI screen as AppController would, and moves the canvases onto the camera so the render has them.</summary>
        private static void ShowScreen(string screen, int levelIndex, LevelList levels)
        {
            var strings = MetaAssetBuilder.LoadStrings();
            UnityEngine.Object.FindAnyObjectByType<Game.UI.SplashScreen>(FindObjectsInactive.Include).Hide();
            UnityEngine.Object.FindAnyObjectByType<DebugTuningPanel>().gameObject.SetActive(false); // hidden at runtime
            var hud = UnityEngine.Object.FindAnyObjectByType<Hud>(FindObjectsInactive.Include);
            hud.Bind(strings, () => { });
            hud.SetLevel(levelIndex + 1, Difficulty.Normal);
            hud.SetShots(6);
            switch (screen)
            {
                case "home":
                    hud.gameObject.SetActive(false);
                    var home = UnityEngine.Object.FindAnyObjectByType<HomeScreen>(FindObjectsInactive.Include);
                    home.Bind(strings);
                    home.Show(levelIndex + 1, true, MetaAssetBuilder.LoadAppConfig().simulatedPrice, () => { }, () => { }, () => { }, () => { });
                    break;
                case "pause":
                    var pause = UnityEngine.Object.FindAnyObjectByType<PausePanel>(FindObjectsInactive.Include);
                    pause.Bind(strings, _ => { }, _ => { }, _ => { }, () => { }, () => { }, () => { }, () => { }, () => { });
                    pause.Show(true, new SaveData { music = false });
                    break;
                case "rooms":
                    hud.gameObject.SetActive(false);
                    var map = UnityEngine.Object.FindAnyObjectByType<RoomMapPanel>(FindObjectsInactive.Include);
                    map.Show(RoomProgress.Rooms(levels, levelIndex), strings, _ => { }, () => { });
                    break;
                default: throw new ArgumentException($"unknown screen '{screen}' (home|pause|rooms)");
            }
            foreach (var canvas in UnityEngine.Object.FindObjectsByType<Canvas>())
            {
                if (!canvas.isRootCanvas) continue;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = Camera.main;
                canvas.planeDistance = 0.5f;
            }
            Canvas.ForceUpdateCanvases();
        }

        private static void Burst(GameObject prefab, Vector3 at, float seconds)
        {
            var system = UnityEngine.Object.Instantiate(prefab, at, Quaternion.identity).GetComponent<ParticleSystem>();
            system.Simulate(seconds, true, true);
        }

        private static void Render(Camera camera, string output)
        {
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 4 };
            camera.aspect = (float)Width / Height;
            // A one-off edit-mode render with the SRP Batcher on draws every lit object with the first material's
            // properties; turn it off for the snapshot only (not saved).
            var pipeline = GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
            if (pipeline != null) pipeline.useSRPBatcher = false;
            var request = new RenderPipeline.StandardRequest { destination = target };
            if (RenderPipeline.SupportsRenderRequest(camera, request)) RenderPipeline.SubmitRenderRequest(camera, request);
            else
            {
                camera.targetTexture = target;
                camera.Render();
            }
            RenderTexture.active = target;
            var image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            image.Apply();
            camera.targetTexture = null;
            RenderTexture.active = null;
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllBytes(output, image.EncodeToPNG());
        }

        private static string Arg(string[] args, string name)
        {
            int i = Array.IndexOf(args, name);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
        }
    }
}
