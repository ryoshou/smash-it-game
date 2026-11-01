using Game.Audio;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class SoundSetTests
    {
        private SoundSet set;
        private AudioClip a;
        private AudioClip b;

        [SetUp]
        public void SetUp()
        {
            a = AudioClip.Create("a", 10, 1, 44100, false);
            b = AudioClip.Create("b", 10, 1, 44100, false);
            set = ScriptableObject.CreateInstance<SoundSet>();
            set.clips = new[] { a, b };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(set);
            Object.DestroyImmediate(a);
            Object.DestroyImmediate(b);
        }

        [Test]
        public void Next_CyclesInOrder()
        {
            Assert.AreSame(a, set.Next());
            Assert.AreSame(b, set.Next());
            Assert.AreSame(a, set.Next());
        }

        [Test]
        public void Next_EmptyClips_ReturnsNull()
        {
            set.clips = new AudioClip[0];

            Assert.IsNull(set.Next());
        }

        [Test]
        public void Pitch_ClampsBelowSoftAndAboveHard()
        {
            Assert.AreEqual(set.pitchAtSoft, set.Pitch(0f), 1e-5f);
            Assert.AreEqual(set.pitchAtHard, set.Pitch(100f), 1e-5f);
            float mid = set.Pitch((set.softSpeed + set.hardSpeed) / 2f);
            Assert.AreEqual((set.pitchAtSoft + set.pitchAtHard) / 2f, mid, 1e-5f);
        }

        [Test]
        public void Volume_RisesWithSpeed()
        {
            Assert.AreEqual(set.volume * SoundSet.SoftVolumeShare, set.Volume(0f), 1e-5f);
            Assert.AreEqual(set.volume, set.Volume(100f), 1e-5f);
            Assert.Less(set.Volume(1f), set.Volume(3f));
        }
    }
}
