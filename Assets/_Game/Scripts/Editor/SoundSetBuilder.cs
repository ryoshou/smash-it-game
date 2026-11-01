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

            var library = VfxBuilder.LoadOrCreateLibrary();
            var feels = library.feels;
            for (int i = 0; i < feels.Length; i++)
            {
                switch (feels[i].kind)
                {
                    case PropKind.Mug: feels[i].hit = mugHit; feels[i].breakSound = mugBreak; break;
                    case PropKind.Book: feels[i].hit = bookHit; feels[i].breakSound = pop; break;
                    case PropKind.CactusPot: feels[i].hit = potHit; feels[i].breakSound = potBreak; break;
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
