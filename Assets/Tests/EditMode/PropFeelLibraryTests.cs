using Game.Juice;
using Game.Levels;
using NUnit.Framework;
using UnityEngine;

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
        public void TryGet_NullFeels_False()
        {
            library.feels = null;

            Assert.IsFalse(library.TryGet(PropKind.Mug, out _));
        }
    }
}
