using System;
using System.IO;
using Game.Levels;
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

        /// <summary>Command line: -snapshotOut path.png [-snapshotLevel n] [-snapshotBreak].</summary>
        public static void FromCommandLine()
        {
            string[] args = Environment.GetCommandLineArgs();
            string output = Arg(args, "-snapshotOut") ?? throw new ArgumentException("-snapshotOut is required");
            int level = int.TryParse(Arg(args, "-snapshotLevel"), out int n) ? n : 0;
            bool stageBreak = Array.IndexOf(args, "-snapshotBreak") >= 0;
            Capture(output, level, stageBreak);
        }

        public static void Capture(string output, int levelIndex, bool stageBreak)
        {
            EditorSceneManager.OpenScene(PrototypeSceneBuilder.ScenePath, OpenSceneMode.Single);
            var bootstrap = UnityEngine.Object.FindAnyObjectByType<GameBootstrap>();
            var so = new SerializedObject(bootstrap);
            var levels = (LevelList)so.FindProperty("levels").objectReferenceValue;
            var catalog = (PropCatalog)so.FindProperty("catalog").objectReferenceValue;
            PlaceProps(levels.At(levelIndex), catalog);
            if (stageBreak) StageBreak(catalog);
            Render(Camera.main, output);
            Debug.Log($"[Snapshot] wrote {output}");
        }

        private static void PlaceProps(LevelDef level, PropCatalog catalog)
        {
            var deskTop = new Vector3(0f, LevelSpawner.DeskTopY, 0f);
            foreach (var placement in level.props)
            {
                var spec = catalog.Get(placement.kind);
                var position = deskTop + placement.basePosition + Vector3.up * (spec.size.y / 2f);
                UnityEngine.Object.Instantiate(spec.Variant(0).prefab, position, Quaternion.Euler(0f, placement.yawDegrees, 0f));
            }
        }

        /// <summary>A mug mid-shatter on the floor beside the desk, with the shard sparkle and a dust puff.</summary>
        private static void StageBreak(PropCatalog catalog)
        {
            var at = new Vector3(0.5f, 0.2f, -1.5f);
            var broken = UnityEngine.Object.Instantiate(catalog.Get(PropKind.Mug).Variant(0).brokenPrefab, at, Quaternion.identity);
            broken.SetActive(true);
            foreach (var body in broken.GetComponentsInChildren<Rigidbody>())
            {
                var outward = body.transform.position - at;
                body.transform.position += outward.normalized * ShardSpread;
            }
            Burst(AssetDatabase.LoadAssetAtPath<GameObject>(VfxBuilder.VfxFolder + "/ShardSparkle.prefab"), at, 0.12f);
            Burst(AssetDatabase.LoadAssetAtPath<GameObject>(VfxBuilder.VfxFolder + "/DustPuff.prefab"), at + new Vector3(-1.1f, 0f, 0f), 0.15f);
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
