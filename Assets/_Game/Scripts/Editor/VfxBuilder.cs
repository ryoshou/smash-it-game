using System.Collections.Generic;
using System.IO;
using Game.Core;
using Game.Juice;
using Game.Levels;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// Writes the break and win effects (dust puff, shard sparkle, confetti) and fills PropFeelLibrary's effect per
    /// kind: pops puff, shatters sparkle. Colours come from the Tiny Treats atlas palette. Safe to re-run.
    /// </summary>
    public static class VfxBuilder
    {
        public const string VfxFolder = "Assets/_Game/Art/Vfx";
        public const string LibraryPath = PrototypeAssetBuilder.ConfigFolder + "/PropFeelLibrary.asset";
        private const string ParticleShader = "Universal Render Pipeline/Particles/Unlit";
        private const float DefaultHitMinSpeed = 0.5f;

        private static readonly Color Cream = new Color(0.98f, 0.92f, 0.86f);
        private static readonly Color Pink = new Color(0.96f, 0.6f, 0.72f);
        private static readonly Color Butter = new Color(1f, 0.85f, 0.4f);
        private static readonly Color Sky = new Color(0.55f, 0.8f, 0.95f);
        private static readonly Color Mint = new Color(0.45f, 0.8f, 0.72f);
        private static readonly Color Milk = new Color(1f, 0.99f, 0.95f);
        private static readonly Color Rose = new Color(0.93f, 0.45f, 0.6f);
        private static readonly Color Wax = new Color(0.98f, 0.94f, 0.8f);
        private static readonly Color Smoke = new Color(0.7f, 0.7f, 0.72f);
        private static readonly Color Sand = new Color(0.86f, 0.72f, 0.48f);
        private static readonly Color Wood = new Color(0.72f, 0.5f, 0.32f);
        private static readonly Color Silver = new Color(0.85f, 0.87f, 0.92f);
        private static readonly Color Violet = new Color(0.7f, 0.45f, 1f);

        [MenuItem("Smash/Build Vfx")]
        public static void BuildAll()
        {
            Directory.CreateDirectory(VfxFolder);
            var material = ParticleMaterial();
            var puff = Save(Burst("DustPuff", material, 24, 0.5f, new Vector2(0.06f, 0.14f), new Vector2(0.4f, 1.2f), 0.1f, Cream, Cream));
            var sparkle = Save(Burst("ShardSparkle", material, 16, 0.4f, new Vector2(0.03f, 0.07f), new Vector2(1f, 2.2f), 0.6f, Color.white, Butter));
            var paperPuff = Save(Burst("PaperPuff", material, 12, 0.35f, new Vector2(0.03f, 0.08f), new Vector2(0.3f, 0.9f), 0.05f, Color.white, Cream));
            var sparks = Save(Burst("SparkPuff", material, 20, 0.3f, new Vector2(0.02f, 0.05f), new Vector2(1.4f, 2.8f), 0.8f, Butter, Color.white));
            Save(Confetti(material));
            var kindBursts = new Dictionary<PropKind, ParticleSystem>
            {
                // From M4 every new kind breaks with its own effect.
                [PropKind.Plate] = Save(Burst("ChinaChips", material, 18, 0.45f, new Vector2(0.03f, 0.06f), new Vector2(1.2f, 2.4f), 0.9f, Color.white, Sky)),
                [PropKind.Donut] = Save(Burst("Sprinkles", material, 22, 0.45f, new Vector2(0.015f, 0.035f), new Vector2(0.8f, 1.8f), 0.7f, Pink, Rose)),
                [PropKind.MilkBottle] = Save(Burst("MilkSplash", material, 26, 0.5f, new Vector2(0.05f, 0.12f), new Vector2(0.6f, 1.6f), 0.5f, Milk, Cream)),
                // Wax flecks with a grey wisp: slow, light particles that drift up a little.
                [PropKind.Candle] = Save(Burst("WaxAndSmoke", material, 18, 0.7f, new Vector2(0.03f, 0.1f), new Vector2(0.3f, 0.9f), -0.05f, Wax, Smoke)),
                [PropKind.Hourglass] = Save(Burst("SandBurst", material, 30, 0.5f, new Vector2(0.015f, 0.04f), new Vector2(0.8f, 2f), 1f, Sand, Butter)),
                [PropKind.SodaCan] = Save(Burst("SilverFlecks", material, 20, 0.4f, new Vector2(0.02f, 0.045f), new Vector2(1.2f, 2.6f), 0.8f, Silver, Color.white)),
                // Square-ish pixel puff: few, larger, quick particles in game colours.
                [PropKind.GameController] = Save(Burst("PixelPuff", material, 12, 0.35f, new Vector2(0.05f, 0.09f), new Vector2(0.6f, 1.4f), 0.2f, Violet, Sky)),
                [PropKind.PencilCup] = Save(Burst("WoodChips", material, 16, 0.45f, new Vector2(0.025f, 0.05f), new Vector2(1f, 2f), 0.9f, Wood, Butter)),
            };
            FillLibrary(puff, sparkle, paperPuff, sparks, kindBursts);
            AssetDatabase.SaveAssets();
        }

        public static ParticleSystem LoadConfetti() =>
            AssetDatabase.LoadAssetAtPath<GameObject>($"{VfxFolder}/Confetti.prefab").GetComponent<ParticleSystem>();

        public static PropFeelLibrary LoadOrCreateLibrary()
        {
            var library = AssetDatabase.LoadAssetAtPath<PropFeelLibrary>(LibraryPath);
            if (library != null) return library;
            library = ScriptableObject.CreateInstance<PropFeelLibrary>();
            AssetDatabase.CreateAsset(library, LibraryPath);
            return library;
        }

        /// <summary>Sets each kind's break effect; keeps any sounds already assigned.</summary>
        private static void FillLibrary(ParticleSystem puff, ParticleSystem sparkle, ParticleSystem paperPuff, ParticleSystem sparks,
            Dictionary<PropKind, ParticleSystem> kindBursts)
        {
            var library = LoadOrCreateLibrary();
            var catalog = AssetDatabase.LoadAssetAtPath<PropCatalog>(PrototypeAssetBuilder.CatalogPath);
            var kinds = (PropKind[])System.Enum.GetValues(typeof(PropKind));
            var feels = new PropFeel[kinds.Length];
            for (int i = 0; i < kinds.Length; i++)
            {
                library.TryGet(kinds[i], out var feel);
                feel.kind = kinds[i];
                if (feel.hitMinSpeed <= 0f) feel.hitMinSpeed = DefaultHitMinSpeed;
                feel.breakVfx = kindBursts.TryGetValue(kinds[i], out var own) ? own : kinds[i] switch
                {
                    PropKind.PaperCup => paperPuff,
                    PropKind.DeskLamp => sparks,
                    _ => catalog.Get(kinds[i]).breakMode == BreakMode.Shatter ? sparkle : puff,
                };
                feels[i] = feel;
            }
            library.feels = feels;
            EditorUtility.SetDirty(library);
        }

        private static ParticleSystem Burst(string name, Material material, int count, float lifetime, Vector2 size,
            Vector2 speed, float gravity, Color colorA, Color colorB)
        {
            var system = NewSystem(name, material, count);
            var main = system.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.7f, lifetime);
            main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed.x, speed.y);
            main.gravityModifier = gravity;
            main.startColor = new ParticleSystem.MinMaxGradient(colorA, colorB);
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.08f;
            FadeAndShrink(system);
            return system;
        }

        private static ParticleSystem Confetti(Material material)
        {
            var system = NewSystem("Confetti", material, 60);
            var main = system.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 1.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.08f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 4f);
            main.gravityModifier = 0.5f;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            var colors = new Gradient();
            colors.SetKeys(
                new[] { new GradientColorKey(Pink, 0f), new GradientColorKey(Butter, 0.33f), new GradientColorKey(Sky, 0.66f), new GradientColorKey(Mint, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            colors.mode = GradientMode.Fixed;
            main.startColor = new ParticleSystem.MinMaxGradient(colors) { mode = ParticleSystemGradientMode.RandomColor };
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 35f;
            shape.radius = 0.3f;
            shape.rotation = new Vector3(-90f, 0f, 0f);
            var rotation = system.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-4f, 4f);
            FadeAndShrink(system);
            return system;
        }

        private static ParticleSystem NewSystem(string name, Material material, int count)
        {
            var go = new GameObject(name);
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.2f;
            main.maxParticles = count;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });
            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
            return system;
        }

        private static void FadeAndShrink(ParticleSystem system)
        {
            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
        }

        private static Material ParticleMaterial()
        {
            string path = AtlasMaterialBuilder.MaterialsRoot + "/Particles.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            Directory.CreateDirectory(AtlasMaterialBuilder.MaterialsRoot);
            material = new Material(Shader.Find(ParticleShader));
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static ParticleSystem Save(ParticleSystem system)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(system.gameObject, $"{VfxFolder}/{system.name}.prefab");
            Object.DestroyImmediate(system.gameObject);
            return prefab.GetComponent<ParticleSystem>();
        }
    }
}
