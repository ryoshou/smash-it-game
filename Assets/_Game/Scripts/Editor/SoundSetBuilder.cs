using System.Collections.Generic;
using System.IO;
using Game.Audio;
using Game.Juice;
using Game.Levels;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// Writes the per-material sound sets from the imported CC0 clips, puts them in PropFeelLibrary, and points the
    /// twang/win/lose/ui presets at recorded clips (pull and target-cleared keep the synth). Safe to re-run.
    /// </summary>
    public static class SoundSetBuilder
    {
        public const string SetsFolder = PrototypeAssetBuilder.ConfigFolder + "/SoundSets";
        private const string Sfx = "Assets/_Game/Audio/Sfx/";
        private const string Kenney = Sfx + "KenneyInterface/";
        private const string Breaking = Sfx + "BreakingFalling/bfh1_";
        private const string Pops = Sfx + "Pops/";
        private const string Splats = Sfx + "Splats/"; // food (Kitchen spec §6)

        [MenuItem("Smash/Build Sound Sets")]
        public static void BuildAll()
        {
            Directory.CreateDirectory(SetsFolder);
            var keycapHit = Set("Hit_Keycap", 0.6f, Kenney + "click_001.ogg", Kenney + "click_002.ogg", Kenney + "click_003.ogg", Kenney + "click_004.ogg");
            var mugHit = Set("Hit_Mug", 0.7f, Breaking + "glass_hit_01.ogg", Kenney + "glass_001.ogg", Breaking + "glass_hit_02.ogg", Kenney + "glass_002.ogg");
            var bookHit = Set("Hit_Book", 0.8f, Breaking + "wood_hit_01.ogg", Breaking + "wood_hit_02.ogg", Breaking + "wood_hit_03.ogg");
            var potHit = Set("Hit_CactusPot", 0.85f, Breaking + "rock_hit_01.ogg", Breaking + "rock_falling_01.ogg", Breaking + "rock_falling_02.ogg");
            var mugBreak = Set("Break_Mug", 0.85f, Breaking + "glass_breaking_01.ogg", Breaking + "glass_breaking_02.ogg", Breaking + "glass_breaking_03.ogg");
            var potBreak = Set("Break_CactusPot", 0.9f, Breaking + "rock_breaking_01.ogg", Breaking + "rock_breaking_02.ogg", Breaking + "rock_breaking_03.ogg");
            var pop = Set("Break_Pop", 0.6f, Pops + "pop1.ogg", Pops + "pop2.ogg", Pops + "pop3.ogg");
            var cupHit = Set("Hit_PaperCup", 0.6f, Kenney + "drop_001.ogg", Kenney + "drop_002.ogg", Kenney + "drop_003.ogg");
            var cupBreak = Set("Break_PaperCup", 0.65f, Kenney + "scratch_001.ogg", Kenney + "scratch_002.ogg", Kenney + "scratch_003.ogg");
            var lampHit = Set("Hit_DeskLamp", 0.75f, Breaking + "metal_hit_01.ogg", Breaking + "metal_hit_02.ogg", Breaking + "metal_hit_03.ogg");
            var lampBreak = Set("Break_DeskLamp", 0.8f, Breaking + "metal_falling_01.ogg", Breaking + "metal_falling_02.ogg");
            var plateHit = Set("Hit_Plate", 0.7f, Breaking + "glass_falling_01.ogg", Breaking + "glass_falling_02.ogg", Breaking + "glass_falling_03.ogg");
            var plateBreak = Set("Break_Plate", 0.85f, Breaking + "breaking_01.ogg", Breaking + "breaking_02.ogg", Breaking + "breaking_03.ogg");
            var donutHit = Set("Hit_Donut", 0.55f, Kenney + "close_001.ogg", Kenney + "close_002.ogg", Kenney + "close_003.ogg");
            var donutBreak = Set("Break_Donut", 0.6f, Splats + "squish_01.mp3", Splats + "squish_02.mp3", Splats + "squishsplat_impact.mp3");
            var milkHit = Set("Hit_MilkBottle", 0.7f, Kenney + "glass_003.ogg", Kenney + "glass_004.ogg", Kenney + "glass_005.ogg", Kenney + "glass_006.ogg");
            var candleHit = Set("Hit_Candle", 0.7f, Breaking + "wood_falling_01.ogg", Breaking + "wood_falling_02.ogg");
            var candleBreak = Set("Break_Candle", 0.7f, Breaking + "misc_01.ogg", Breaking + "misc_02.ogg", Breaking + "misc_03.ogg");
            var hourglassHit = Set("Hit_Hourglass", 0.65f, Kenney + "tick_001.ogg", Kenney + "tick_002.ogg", Kenney + "tick_004.ogg");
            var hourglassBreak = Set("Break_Hourglass", 0.85f, Breaking + "misc_04.ogg", Breaking + "misc_05.ogg", Breaking + "misc_06.ogg", Breaking + "misc_07.ogg");
            var pencilHit = Set("Hit_PencilCup", 0.7f, Breaking + "hit_01.ogg", Breaking + "hit_02.ogg", Breaking + "hit_03.ogg", Breaking + "hit_04.ogg");
            var pencilBreak = Set("Break_PencilCup", 0.75f, Breaking + "wood_breaking_01.ogg", Breaking + "wood_breaking_02.ogg", Breaking + "wood_breaking_03.ogg", Breaking + "wood_breaking_04.ogg");
            var canHit = Set("Hit_SodaCan", 0.7f, Breaking + "metal_hit_04.ogg", Breaking + "metal_hit_05.ogg", Breaking + "metal_hit_06.ogg");
            var canBreak = Set("Break_SodaCan", 0.75f, Breaking + "metal_falling_03.ogg", Breaking + "metal_falling_04.ogg", Breaking + "metal_falling_05.ogg");
            var padHit = Set("Hit_GameController", 0.6f, Kenney + "switch_001.ogg", Kenney + "switch_002.ogg", Kenney + "switch_003.ogg", Kenney + "switch_004.ogg");
            var padBreak = Set("Break_GameController", 0.65f, Kenney + "glitch_001.ogg", Kenney + "glitch_002.ogg", Kenney + "glitch_003.ogg", Kenney + "glitch_004.ogg");
            var fizzHit = Set("Hit_FizzySoda", 0.65f, Kenney + "toggle_001.ogg", Kenney + "toggle_002.ogg", Kenney + "toggle_003.ogg");
            var fizzBurst = Set("Break_FizzySoda", 0.95f, Pops + "pop1.ogg", Kenney + "maximize_001.ogg", Pops + "pop2.ogg",
                Kenney + "maximize_002.ogg", Pops + "pop3.ogg", Kenney + "maximize_003.ogg");
            var milkBreak = Set("Break_MilkBottle", 0.85f, Breaking + "glass_breaking_04.ogg", Breaking + "glass_breaking_05.ogg", Breaking + "glass_breaking_06.ogg");

            var kitchen = KitchenSets();
            var library = VfxBuilder.LoadOrCreateLibrary();
            var feels = library.feels;
            for (int i = 0; i < feels.Length; i++)
            {
                switch (feels[i].kind)
                {
                    case PropKind.Mug: feels[i].hit = mugHit; feels[i].breakSound = mugBreak; break;
                    case PropKind.Book: feels[i].hit = bookHit; feels[i].breakSound = pop; break;
                    case PropKind.CactusPot: feels[i].hit = potHit; feels[i].breakSound = potBreak; break;
                    case PropKind.PaperCup: feels[i].hit = cupHit; feels[i].breakSound = cupBreak; break;
                    case PropKind.DeskLamp: feels[i].hit = lampHit; feels[i].breakSound = lampBreak; break;
                    case PropKind.Plate: feels[i].hit = plateHit; feels[i].breakSound = plateBreak; break;
                    case PropKind.Donut: feels[i].hit = donutHit; feels[i].breakSound = donutBreak; break;
                    case PropKind.MilkBottle: feels[i].hit = milkHit; feels[i].breakSound = milkBreak; break;
                    case PropKind.Candle: feels[i].hit = candleHit; feels[i].breakSound = candleBreak; break;
                    case PropKind.Hourglass: feels[i].hit = hourglassHit; feels[i].breakSound = hourglassBreak; break;
                    case PropKind.PencilCup: feels[i].hit = pencilHit; feels[i].breakSound = pencilBreak; break;
                    case PropKind.SodaCan: feels[i].hit = canHit; feels[i].breakSound = canBreak; break;
                    case PropKind.GameController: feels[i].hit = padHit; feels[i].breakSound = padBreak; break;
                    case PropKind.FizzySoda: feels[i].hit = fizzHit; feels[i].breakSound = fizzBurst; break;
                    default:
                        if (kitchen.TryGetValue(feels[i].kind, out var sets)) (feels[i].hit, feels[i].breakSound) = sets;
                        else (feels[i].hit, feels[i].breakSound) = (keycapHit, pop);
                        break;
                }
            }
            library.feels = feels;
            EditorUtility.SetDirty(library);
            OverridePresets();
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Kitchen kinds (Kitchen spec §6): each its own hit and break set. Dishes clink and shatter; food thuds softly
        /// and splats (Task 3's squish clips); the drink can rattles and clatters.
        /// </summary>
        private static Dictionary<PropKind, (SoundSet hit, SoundSet breakSound)> KitchenSets() => new Dictionary<PropKind, (SoundSet, SoundSet)>
        {
            [PropKind.DinnerPlate] = (Set("Hit_DinnerPlate", 0.7f, Breaking + "glass_falling_04.ogg", Breaking + "hit_05.ogg", Breaking + "hit_06.ogg"),
                Set("Break_DinnerPlate", 0.85f, Breaking + "glass_breaking_01.ogg", Breaking + "breaking_02.ogg", Breaking + "rock_breaking_01.ogg")),
            [PropKind.Bowl] = (Set("Hit_Bowl", 0.7f, Breaking + "hit_07.ogg", Breaking + "hit_08.ogg", Kenney + "drop_004.ogg"),
                Set("Break_Bowl", 0.85f, Breaking + "glass_breaking_02.ogg", Breaking + "breaking_03.ogg", Breaking + "rock_breaking_02.ogg")),
            [PropKind.Teapot] = (Set("Hit_Teapot", 0.75f, Kenney + "bong_001.ogg", Breaking + "hit_09.ogg", Breaking + "glass_hit_01.ogg"),
                Set("Break_Teapot", 0.9f, Breaking + "breaking_01.ogg", Breaking + "rock_breaking_03.ogg", Breaking + "glass_breaking_03.ogg")),
            [PropKind.WineGlass] = (Set("Hit_WineGlass", 0.6f, Kenney + "glass_001.ogg", Kenney + "glass_003.ogg", Kenney + "glass_005.ogg"),
                Set("Break_WineGlass", 0.85f, Breaking + "glass_breaking_04.ogg", Breaking + "glass_breaking_05.ogg", Breaking + "glass_falling_03.ogg")),
            [PropKind.JamJar] = (Set("Hit_JamJar", 0.65f, Breaking + "hit_10.ogg", Breaking + "hit_11.ogg", Kenney + "close_004.ogg"),
                Set("Break_JamJar", 0.8f, Splats + "impactsplat01.wav", Splats + "impactsplat02.wav", Splats + "squish_03.mp3")),
            [PropKind.KetchupBottle] = (Set("Hit_KetchupBottle", 0.65f, Kenney + "open_001.ogg", Kenney + "open_002.ogg", Breaking + "hit_12.ogg"),
                Set("Break_KetchupBottle", 0.8f, Splats + "impactsplat03.wav", Splats + "impactsplat04.wav", Splats + "squish_04.mp3")),
            [PropKind.DrinkCan] = (Set("Hit_DrinkCan", 0.7f, Breaking + "falling_01.ogg", Breaking + "falling_02.ogg", Breaking + "metal_hit_06.ogg"),
                Set("Break_DrinkCan", 0.75f, Breaking + "metal_falling_01.ogg", Breaking + "metal_falling_03.ogg", Breaking + "metal_falling_05.ogg")),
            [PropKind.Apple] = (Set("Hit_Apple", 0.6f, Kenney + "back_001.ogg", Kenney + "back_002.ogg", Kenney + "back_003.ogg"),
                Set("Break_Apple", 0.75f, Splats + "squish_05.mp3", Splats + "squish_06.mp3", Splats + "impactsplat07.wav")),
            [PropKind.Tomato] = (Set("Hit_Tomato", 0.6f, Kenney + "back_004.ogg", Kenney + "scroll_001.ogg", Kenney + "scroll_002.ogg"),
                Set("Break_Tomato", 0.75f, Splats + "impactsplat05.wav", Splats + "impactsplat06.wav", Splats + "squishpop.mp3")),
            [PropKind.Cheese] = (Set("Hit_Cheese", 0.6f, Breaking + "falling_03.ogg", Breaking + "falling_04.ogg", Kenney + "scroll_003.ogg"),
                Set("Break_Cheese", 0.7f, Splats + "squish_01.mp3", Splats + "impactsplat08.wav", Splats + "squish_05.mp3")),
            [PropKind.Burger] = (Set("Hit_Burger", 0.6f, Breaking + "falling_05.ogg", Breaking + "falling_06.ogg", Kenney + "scroll_004.ogg"),
                Set("Break_Burger", 0.75f, Splats + "impactsplat02.wav", Splats + "impactsplat06.wav", Splats + "squish_06.mp3")),
            [PropKind.Cupcake] = (Set("Hit_Cupcake", 0.55f, Kenney + "select_002.ogg", Kenney + "select_003.ogg", Kenney + "select_004.ogg"),
                Set("Break_Cupcake", 0.7f, Splats + "squishpop.mp3", Splats + "squish_02.mp3", Splats + "impactsplat07.wav")),
            [PropKind.Croissant] = (Set("Hit_Croissant", 0.55f, Kenney + "scratch_004.ogg", Kenney + "scratch_005.ogg", Kenney + "scroll_005.ogg"),
                Set("Break_Croissant", 0.7f, Splats + "squish_03.mp3", Splats + "squish_04.mp3", Splats + "impactsplat08.wav")),
            [PropKind.Sandwich] = (Set("Hit_Sandwich", 0.6f, Breaking + "falling_07.ogg", Breaking + "hit_13.ogg", Kenney + "select_005.ogg"),
                Set("Break_Sandwich", 0.75f, Splats + "impactsplat04.wav", Splats + "impactsplat05.wav", Splats + "squish_01.mp3")),
        };

        private static SoundSet Set(string name, float volume, params string[] clipPaths)
        {
            string path = $"{SetsFolder}/{name}.asset";
            var set = AssetDatabase.LoadAssetAtPath<SoundSet>(path);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<SoundSet>();
                set.volume = volume;
                AssetDatabase.CreateAsset(set, path);
            }
            set.clips = new AudioClip[clipPaths.Length];
            for (int i = 0; i < clipPaths.Length; i++)
                set.clips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPaths[i])
                               ?? throw new FileNotFoundException($"Sound missing: {clipPaths[i]} (run tools/assets/import-packs.sh)");
            EditorUtility.SetDirty(set);
            return set;
        }

        private static void OverridePresets()
        {
            var presets = AssetDatabase.LoadAssetAtPath<SfxPresets>(PrototypeAssetBuilder.ConfigFolder + "/SfxPresets.asset");
            if (presets == null) return;
            foreach (var preset in presets.Presets)
            {
                preset.clipOverride = preset.id switch
                {
                    SfxId.Twang => Clip(Kenney + "pluck_001.ogg"),
                    SfxId.Win => Clip(Kenney + "confirmation_002.ogg"),
                    SfxId.Lose => Clip(Kenney + "question_001.ogg"),
                    SfxId.Ui => Clip(Kenney + "select_001.ogg"),
                    _ => preset.clipOverride,
                };
            }
            EditorUtility.SetDirty(presets);
        }

        private static AudioClip Clip(string path) =>
            AssetDatabase.LoadAssetAtPath<AudioClip>(path) ?? throw new FileNotFoundException($"Sound missing: {path}");
    }
}
