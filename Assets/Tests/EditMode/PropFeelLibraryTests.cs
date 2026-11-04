using System;
using Game.Core;
using Game.Editor;
using Game.Juice;
using Game.Levels;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Tests
{
    public sealed class PropFeelLibraryTests
    {
        private PropFeelLibrary library;

        [SetUp]
        public void SetUp()
        {
            library = ScriptableObject.CreateInstance<PropFeelLibrary>();
            library.feels = new[] { new PropFeel { kind = PropKind.Mug, hitMinSpeed = 0.5f } };
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(library);

        [Test]
        public void TryGet_KnownKind_True()
        {
            Assert.IsTrue(library.TryGet(PropKind.Mug, out var feel));
            Assert.AreEqual(0.5f, feel.hitMinSpeed);
        }

        [Test]
        public void TryGet_MissingKind_FalseWithoutThrowing()
        {
            Assert.IsFalse(library.TryGet(PropKind.Book, out _));
        }

        [Test]
        public void ShippedLibrary_EveryKindHasHitSound_AndShatterKindsHaveBreakSound()
        {
            var shipped = AssetDatabase.LoadAssetAtPath<PropFeelLibrary>(VfxBuilder.LibraryPath);
            var catalog = AssetDatabase.LoadAssetAtPath<PropCatalog>(PrototypeAssetBuilder.CatalogPath);
            Assert.IsTrue(shipped.TryGet(PropKind.Keycap1u, out var keycap));

            foreach (PropKind kind in Enum.GetValues(typeof(PropKind)))
            {
                Assert.IsTrue(shipped.TryGet(kind, out var feel), $"{kind} has no feel");
                AssertSet(feel.hit, $"{kind} hit");
                if (catalog.Get(kind).breakMode != BreakMode.None) AssertSet(feel.breakSound, $"{kind} break");
                if (catalog.Get(kind).breakMode == BreakMode.Shatter)
                    Assert.AreNotSame(keycap.hit, feel.hit, $"{kind} should not sound like a keycap");
            }
        }

        /// <summary>Spec §9: the new kinds sound like themselves, not like any other kind.</summary>
        [TestCase(PropKind.PaperCup)]
        [TestCase(PropKind.DeskLamp)]
        [TestCase(PropKind.Plate)]
        [TestCase(PropKind.Donut)]
        [TestCase(PropKind.MilkBottle)]
        public void NewKind_HasItsOwnSounds(PropKind kind)
        {
            var shipped = AssetDatabase.LoadAssetAtPath<PropFeelLibrary>(VfxBuilder.LibraryPath);
            Assert.IsTrue(shipped.TryGet(kind, out var mine), $"{kind} has no feel");
            foreach (var other in shipped.feels)
            {
                if (other.kind == kind) continue;
                Assert.AreNotSame(mine.hit, other.hit, $"{kind} hit is shared with {other.kind}");
                Assert.AreNotSame(mine.breakSound, other.breakSound, $"{kind} break is shared with {other.kind}");
            }
        }

        /// <summary>From M4 each new kind breaks with its own effect (M3's kinds share puff and sparkle).</summary>
        [TestCase(PropKind.Plate)]
        [TestCase(PropKind.Donut)]
        [TestCase(PropKind.MilkBottle)]
        public void NewKind_HasItsOwnBreakEffect(PropKind kind)
        {
            var shipped = AssetDatabase.LoadAssetAtPath<PropFeelLibrary>(VfxBuilder.LibraryPath);
            Assert.IsTrue(shipped.TryGet(kind, out var mine), $"{kind} has no feel");
            Assert.IsNotNull(mine.breakVfx, $"{kind} has no break effect");
            foreach (var other in shipped.feels)
                if (other.kind != kind) Assert.AreNotSame(mine.breakVfx, other.breakVfx, $"{kind} effect is shared with {other.kind}");
        }

        private static void AssertSet(Game.Audio.SoundSet set, string what)
        {
            Assert.IsNotNull(set, what);
            Assert.GreaterOrEqual(set.clips.Length, 2, what);
            foreach (var clip in set.clips) Assert.IsNotNull(clip, what);
        }

        [Test]
        public void TryGet_NullFeels_False()
        {
            library.feels = null;

            Assert.IsFalse(library.TryGet(PropKind.Mug, out _));
        }
    }
}
