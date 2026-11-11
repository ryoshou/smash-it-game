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
    /// kind: pops puff, shatters sparkle, food splashes in its own colour (Kitchen spec §6). Colours come from the
    /// Tiny Treats atlas palette. Safe to re-run.
    /// </summary>
    public static class VfxBuilder
    {
        public const string VfxFolder = "Assets/_Game/Art/Vfx";
        public const string LibraryPath = PrototypeAssetBuilder.ConfigFolder + "/PropFeelLibrary.asset";
        private const string ParticleShader = "Universal Render Pipeline/Particles/Unlit";
        private const float DefaultHitMinSpeed = 0.5f;
        private const float SplatSeconds = 4f;      // the owner's lingering stain (2–5 s)
        private const float SplatHoldUntil = 0.75f; // of its life, full size (3 s); then it shrinks to nothing
        private const float DefaultSplatSize = 0.35f;
        private const int SplatTextureSize = 64;

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
        private static readonly Color Coral = new Color(0.97f, 0.5f, 0.37f);

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
                [PropKind.Donut] = Save(Crumbs("DonutCrumbs", material, Rose)),
                [PropKind.MilkBottle] = Save(Burst("MilkSplash", material, 26, 0.5f, new Vector2(0.05f, 0.12f), new Vector2(0.6f, 1.6f), 0.5f, Milk, Cream)),
                // Wax flecks with a grey wisp: slow, light particles that drift up a little.
                [PropKind.Candle] = Save(Burst("WaxAndSmoke", material, 18, 0.7f, new Vector2(0.03f, 0.1f), new Vector2(0.3f, 0.9f), -0.05f, Wax, Smoke)),
                [PropKind.Hourglass] = Save(Burst("SandBurst", material, 30, 0.5f, new Vector2(0.015f, 0.04f), new Vector2(0.8f, 2f), 1f, Sand, Butter)),
                // The fizzy burst: a big cyan bubble spray, wide and slow to fall.
                [PropKind.FizzySoda] = Save(Burst("BubbleSpray", material, 48, 0.8f, new Vector2(0.04f, 0.11f), new Vector2(1.5f, 3.2f), 0.3f, Sky, Color.white)),
                [PropKind.SodaCan] = Save(Burst("SilverFlecks", material, 20, 0.4f, new Vector2(0.02f, 0.045f), new Vector2(1.2f, 2.6f), 0.8f, Silver, Color.white)),
                // Square-ish pixel puff: few, larger, quick particles in game colours.
                [PropKind.GameController] = Save(Burst("PixelPuff", material, 12, 0.35f, new Vector2(0.05f, 0.09f), new Vector2(0.6f, 1.4f), 0.2f, Violet, Sky)),
                [PropKind.PencilCup] = Save(Burst("WoodChips", material, 16, 0.45f, new Vector2(0.025f, 0.05f), new Vector2(1f, 2f), 0.9f, Wood, Butter)),
            };
            AddKitchen(kindBursts, material);
            FillLibrary(puff, sparkle, paperPuff, sparks, kindBursts);
            AssetDatabase.SaveAssets();
        }

        /// <summary>Kitchen kinds (Kitchen spec §6): dishes throw chips; the soft foods splash and the rest puff crumbs, each in its own colour.</summary>
        private static void AddKitchen(Dictionary<PropKind, ParticleSystem> bursts, Material material)
        {
            bursts[PropKind.DinnerPlate] = Save(Burst("WhiteChips", material, 18, 0.45f, new Vector2(0.03f, 0.06f), new Vector2(1.2f, 2.4f), 0.9f, Color.white, Cream));
            bursts[PropKind.Bowl] = Save(Burst("SkyChips", material, 16, 0.45f, new Vector2(0.03f, 0.06f), new Vector2(1.1f, 2.2f), 0.9f, Sky, Color.white));
            bursts[PropKind.Teapot] = Save(Burst("CreamChips", material, 22, 0.5f, new Vector2(0.035f, 0.07f), new Vector2(1.2f, 2.4f), 0.9f, Cream, Butter));
            bursts[PropKind.WineGlass] = Save(Burst("GlassSparkle", material, 20, 0.4f, new Vector2(0.015f, 0.04f), new Vector2(1.4f, 2.8f), 0.7f, Color.white, Sky));
            bursts[PropKind.DrinkCan] = Save(Burst("SilverFizz", material, 24, 0.45f, new Vector2(0.02f, 0.05f), new Vector2(1.2f, 2.6f), 0.4f, Silver, Sky));
            bursts[PropKind.JamJar] = Save(Crumbs("JamCrumbs", material, new Color(0.72f, 0.12f, 0.3f)));
            bursts[PropKind.KetchupBottle] = Save(Crumbs("KetchupCrumbs", material, new Color(0.85f, 0.1f, 0.06f)));
            bursts[PropKind.Apple] = Save(Crumbs("AppleCrumbs", material, new Color(0.86f, 0.25f, 0.2f)));
            bursts[PropKind.Tomato] = Save(Splash("TomatoSplash", new Color(0.95f, 0.3f, 0.12f), 0.32f));
            bursts[PropKind.Cheese] = Save(Crumbs("CheeseCrumbs", material, new Color(1f, 0.82f, 0.3f)));
            bursts[PropKind.Burger] = Save(Crumbs("BurgerCrumbs", material, new Color(0.6f, 0.34f, 0.18f)));
            bursts[PropKind.Cupcake] = Save(Splash("CupcakeSplash", new Color(0.97f, 0.6f, 0.75f), 0.28f));
            bursts[PropKind.Croissant] = Save(Crumbs("CroissantCrumbs", material, new Color(0.92f, 0.7f, 0.35f)));
            bursts[PropKind.Sandwich] = Save(Splash("SandwichSplash", new Color(0.55f, 0.8f, 0.35f), 0.32f));
        }

        /// <summary>Food that keeps its shape: a small puff of crumbs in its colour, no splat.</summary>
        private static ParticleSystem Crumbs(string name, Material material, Color colour) =>
            Burst(name, material, 16, 0.45f, new Vector2(0.025f, 0.055f), new Vector2(0.8f, 1.8f), 0.8f, colour, Cream);

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
                    _ => BreakRule.HasPieces(catalog.Get(kinds[i]).breakMode) ? sparkle : puff,
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

        /// <summary>
        /// A juicy droplet burst in the food's colour plus a child <see cref="PropFeedback.SplatName"/>: one flat
        /// particle lying where the food landed, full size for a moment, then shrinking away.
        /// </summary>
        public static ParticleSystem Splash(string name, Color colour, float splatSize = DefaultSplatSize)
        {
            var droplets = Burst(name, ParticleMaterial(), 26, 0.5f, new Vector2(0.025f, 0.06f), new Vector2(1f, 2.2f), 1f,
                colour, Color.Lerp(colour, Color.white, 0.35f));
            var splat = NewSystem(PropFeedback.SplatName, SplatMaterial(), 1);
            splat.transform.SetParent(droplets.transform, false);
            var main = splat.main;
            main.startLifetime = SplatSeconds;
            main.startSpeed = 0f;
            main.startSize = splatSize;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = colour;
            var shape = splat.shape;
            shape.enabled = false;
            var size = splat.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 1f), new Keyframe(SplatHoldUntil, 1f), new Keyframe(1f, 0f)));
            splat.GetComponent<ParticleSystemRenderer>().renderMode = ParticleSystemRenderMode.HorizontalBillboard;
            return droplets;
        }

        private const int ConfettiPerCannon = 90;

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
                // Fixed mode: each key owns the stretch up to its time, so a key at 0 would never be picked.
                new[]
                {
                    new GradientColorKey(Pink, 0.2f), new GradientColorKey(Coral, 0.4f), new GradientColorKey(Butter, 0.6f),
                    new GradientColorKey(Sky, 0.8f), new GradientColorKey(Mint, 1f),
                },
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
            Cannon(system.transform, material, colors, -1f);
            Cannon(system.transform, material, colors, 1f);
            return system;
        }

        /// <summary>
        /// A confetti cannon just off the bottom corner of the view, firing paper strips up and in over the desk; they
        /// flutter down slowly, so the celebration fills the screen before the result panel opens.
        /// </summary>
        private static void Cannon(Transform confetti, Material material, Gradient colors, float side)
        {
            var system = NewSystem(side < 0f ? "Cannon L" : "Cannon R", material, ConfettiPerCannon);
            system.transform.SetParent(confetti, false);
            system.transform.localPosition = new Vector3(side * 1.1f, -0.7f, -0.8f);
            system.transform.localRotation = Quaternion.Euler(-62f, -side * 28f, 0f); // up, and in towards the desk
            var main = system.main;
            main.duration = 0.35f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.2f, 3.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(4.5f, 6.5f);
            main.gravityModifier = 0.35f;
            main.startSize3D = true;
            main.startSizeX = new ParticleSystem.MinMaxCurve(0.05f, 0.08f);
            main.startSizeY = new ParticleSystem.MinMaxCurve(0.09f, 0.14f);
            main.startSizeZ = 1f;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new ParticleSystem.MinMaxGradient(colors) { mode = ParticleSystemGradientMode.RandomColor };
            var emission = system.emission;
            emission.SetBursts(new[]
            {
                new ParticleSystem.Burst(0f, (short)(ConfettiPerCannon / 2)),
                new ParticleSystem.Burst(0.15f, (short)(ConfettiPerCannon / 2)),
            });
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 18f;
            shape.radius = 0.05f;
            // Air drag: strips shoot out fast, then hang and drift down.
            var drag = system.limitVelocityOverLifetime;
            drag.enabled = true;
            drag.limit = 1.2f;
            drag.dampen = 0.08f;
            var flutter = system.noise;
            flutter.enabled = true;
            flutter.strength = 0.6f;
            flutter.frequency = 0.8f;
            var rotation = system.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-6f, 6f);
            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.8f, 1f), new Keyframe(1f, 0f)));
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

        /// <summary>An alpha-clipped particle material whose texture is a generated splat shape (no art file).</summary>
        private static Material SplatMaterial()
        {
            string path = AtlasMaterialBuilder.MaterialsRoot + "/Splat.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Directory.CreateDirectory(AtlasMaterialBuilder.MaterialsRoot);
                material = new Material(Shader.Find(ParticleShader));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetTexture("_BaseMap", SplatTexture());
            material.SetFloat("_AlphaClip", 1f);
            material.SetFloat("_Cutoff", 0.5f);
            material.EnableKeyword("_ALPHATEST_ON");
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>A white blob with a wobbly rim and a few droplets round it; the particle colour tints it.</summary>
        private static Texture2D SplatTexture()
        {
            string path = VfxFolder + "/SplatShape.asset";
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture != null) return texture;
            texture = new Texture2D(SplatTextureSize, SplatTextureSize, TextureFormat.RGBA32, false) { name = "SplatShape" };
            var droplets = new[] { new Vector3(0.8f, 0.25f, 0.07f), new Vector3(0.22f, 0.82f, 0.06f), new Vector3(0.15f, 0.3f, 0.05f) };
            for (int y = 0; y < SplatTextureSize; y++)
                for (int x = 0; x < SplatTextureSize; x++)
                {
                    var uv = new Vector2((x + 0.5f) / SplatTextureSize, (y + 0.5f) / SplatTextureSize);
                    var offset = uv - new Vector2(0.5f, 0.5f);
                    float angle = Mathf.Atan2(offset.y, offset.x);
                    float rim = 0.3f + 0.04f * Mathf.Sin(angle * 5f) + 0.025f * Mathf.Sin(angle * 9f + 1.3f);
                    bool inside = offset.magnitude < rim;
                    foreach (var drop in droplets) inside |= Vector2.Distance(uv, drop) < drop.z;
                    texture.SetPixel(x, y, inside ? Color.white : new Color(1f, 1f, 1f, 0f));
                }
            texture.Apply();
            AssetDatabase.CreateAsset(texture, path);
            return texture;
        }

        private static ParticleSystem Save(ParticleSystem system)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(system.gameObject, $"{VfxFolder}/{system.name}.prefab");
            Object.DestroyImmediate(system.gameObject);
            return prefab.GetComponent<ParticleSystem>();
        }
    }
}
