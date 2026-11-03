using Game.Levels;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>A layout's fingerprint ties a playtest result to the exact layout it was measured on.</summary>
    public sealed class LevelFingerprintTests
    {
        private LevelDef a;
        private LevelDef b;

        [SetUp]
        public void SetUp()
        {
            a = Make();
            b = Make();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(a);
            Object.DestroyImmediate(b);
        }

        private static LevelDef Make()
        {
            var level = ScriptableObject.CreateInstance<LevelDef>();
            level.id = "fp";
            level.shots = 6;
            level.props = new[]
            {
                new PropPlacement { kind = PropKind.Mug, basePosition = new Vector3(0.1f, 0f, 0.2f), yawDegrees = 15f, isTarget = true },
                new PropPlacement { kind = PropKind.Keycap1u, basePosition = new Vector3(-0.3f, 0.15f, 0f), isTarget = true, variant = 1 },
            };
            return level;
        }

        [Test]
        public void Fingerprint_SameLayout_SameHash()
        {
            Assert.AreEqual(LevelFingerprint.Of(a), LevelFingerprint.Of(b));
            Assert.AreEqual(16, LevelFingerprint.Of(a).Length);
        }

        [Test]
        public void Fingerprint_PropMovedOneMillimetre_Changes()
        {
            b.props[0].basePosition += new Vector3(0.001f, 0f, 0f);

            Assert.AreNotEqual(LevelFingerprint.Of(a), LevelFingerprint.Of(b));
        }

        [Test]
        public void Fingerprint_VariantOnly_Unchanged()
        {
            b.props[1].variant = 2;

            Assert.AreEqual(LevelFingerprint.Of(a), LevelFingerprint.Of(b));
        }

        [Test]
        public void Fingerprint_ShotsDeskOrTargetFlag_Changes()
        {
            b.shots = 7;
            Assert.AreNotEqual(LevelFingerprint.Of(a), LevelFingerprint.Of(b));
            b.shots = 6;
            b.props[0].desk = 1;
            b.hasBackDesk = true;
            Assert.AreNotEqual(LevelFingerprint.Of(a), LevelFingerprint.Of(b));
            b.props[0].desk = 0;
            b.hasBackDesk = false;
            b.props[1].isTarget = false;
            Assert.AreNotEqual(LevelFingerprint.Of(a), LevelFingerprint.Of(b));
        }
    }
}
