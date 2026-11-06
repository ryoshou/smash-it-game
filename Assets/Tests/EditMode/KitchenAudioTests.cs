using System.IO;
using Game.Audio;
using Game.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>Kitchen spec §6–7: the Kitchen's own music, and food that splats instead of popping.</summary>
    public sealed class KitchenAudioTests
    {
        public const string KitchenMusic = "Assets/_Game/Audio/Music/good_morning_cakeflaps.ogg";
        private const long MaxMusicBytes = 3 * 1024 * 1024;

        [Test]
        public void MusicClip_Imported()
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(KitchenMusic);

            Assert.IsNotNull(clip, KitchenMusic);
            Assert.Greater(clip.length, 20f, "a loop long enough not to grate");
            Assert.LessOrEqual(new FileInfo(KitchenMusic).Length, MaxMusicBytes);
        }

        [Test]
        public void Break_Donut_IsASplatSet()
        {
            var donut = AssetDatabase.LoadAssetAtPath<SoundSet>(SoundSetBuilder.SetsFolder + "/Break_Donut.asset");
            var pop = AssetDatabase.LoadAssetAtPath<SoundSet>(SoundSetBuilder.SetsFolder + "/Break_Pop.asset");

            Assert.GreaterOrEqual(donut.clips.Length, 3);
            foreach (var clip in donut.clips)
            {
                StringAssert.Contains("/Sfx/Splats/", AssetDatabase.GetAssetPath(clip), "a donut splats");
                CollectionAssert.DoesNotContain(pop.clips, clip, $"{clip.name} is also a pop");
            }
        }
    }
}
