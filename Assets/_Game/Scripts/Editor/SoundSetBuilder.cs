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
            var donutBreak = Set("Break_Donut", 0.6f, Kenney + "minimize_001.ogg", Kenney + "minimize_002.ogg", Kenney + "minimize_003.ogg");
            var milkHit = Set("Hit_MilkBottle", 0.7f, Kenney + "glass_003.ogg", Kenney + "glass_004.ogg", Kenney + "glass_005.ogg", Kenney + "glass_006.ogg");
            var milkBreak = Set("Break_MilkBottle", 0.85f, Breaking + "glass_breaking_04.ogg", Breaking + "glass_breaking_05.ogg", Breaking + "glass_breaking_06.ogg");

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
                    default: feels[i].hit = keycapHit; feels[i].breakSound = pop; break;
                }
            }
            library.feels = feels;
            EditorUtility.SetDirty(library);
            OverridePresets();
            AssetDatabase.SaveAssets();
        }

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
