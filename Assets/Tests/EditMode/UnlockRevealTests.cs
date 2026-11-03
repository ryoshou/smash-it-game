using System.Collections.Generic;
using Game.Levels;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>Spec §4.5: the win panel's "new thing to knock over" bar fills towards the next level that brings a new kind.</summary>
    public sealed class UnlockRevealTests
    {
        private const float Tolerance = 1e-4f;

        private readonly List<Object> created = new List<Object>();
        private LevelList list;

        /// <summary>12 levels; kinds first appear at 0 (keycap), 2 (mug), 4 (book) and 11 (cactus).</summary>
        [SetUp]
        public void SetUp()
        {
            var intros = new Dictionary<int, PropKind> { { 2, PropKind.Mug }, { 4, PropKind.Book }, { 11, PropKind.CactusPot } };
            var levels = new LevelDef[12];
            for (int i = 0; i < levels.Length; i++)
            {
                var props = new List<PropPlacement> { new PropPlacement { kind = PropKind.Keycap1u, isTarget = true } };
                if (intros.TryGetValue(i, out var kind)) props.Add(new PropPlacement { kind = kind, isTarget = true });
                if (i > 4) props.Add(new PropPlacement { kind = PropKind.Mug, isTarget = true }); // old kinds don't count
                levels[i] = ScriptableObject.CreateInstance<LevelDef>();
                levels[i].props = props.ToArray();
                created.Add(levels[i]);
            }
            list = ScriptableObject.CreateInstance<LevelList>();
            list.levels = levels;
            created.Add(list);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var o in created) Object.DestroyImmediate(o);
            created.Clear();
        }

        [Test]
        public void After_LevelOne_PointsToFirstNewKind()
        {
            var reveal = UnlockReveal.After(list, 0);

            Assert.IsTrue(reveal.HasNext);
            Assert.AreEqual(PropKind.Mug, reveal.Next);
            Assert.AreEqual(0.5f, reveal.Progress01, Tolerance);
        }

        [Test]
        public void After_IntroLevel_StartsNewSpan()
        {
            var reveal = UnlockReveal.After(list, 4);

            Assert.AreEqual(PropKind.CactusPot, reveal.Next);
            Assert.AreEqual(1f / 7f, reveal.Progress01, Tolerance);
        }

        [Test]
        public void After_LastLevelBeforeIntro_IsFull() => Assert.AreEqual(1f, UnlockReveal.After(list, 10).Progress01, Tolerance);

        [Test]
        public void After_NoLaterIntro_HasNoNext() => Assert.IsFalse(UnlockReveal.After(list, 11).HasNext);

        [Test]
        public void After_WrappedIndex_UsesLayoutPosition()
        {
            var wrapped = UnlockReveal.After(list, list.Count + 4);
            var plain = UnlockReveal.After(list, 4);

            Assert.AreEqual(plain.HasNext, wrapped.HasNext);
            Assert.AreEqual(plain.Next, wrapped.Next);
            Assert.AreEqual(plain.Progress01, wrapped.Progress01, Tolerance);
        }

        [Test]
        public void After_NewKeycapSize_IsNotANewThing()
        {
            list.levels[1].props = new[] { new PropPlacement { kind = PropKind.KeycapSpace, isTarget = true } };

            var reveal = UnlockReveal.After(list, 0);

            Assert.AreEqual(PropKind.Mug, reveal.Next);
            Assert.AreEqual(0.5f, reveal.Progress01, Tolerance);
        }

        [Test]
        public void After_TwoNewKindsOnOneLevel_NamesTheFirstInKindOrder()
        {
            list.levels[2].props = new[]
            {
                new PropPlacement { kind = PropKind.CactusPot, isTarget = true },
                new PropPlacement { kind = PropKind.Mug, isTarget = true },
            };

            Assert.AreEqual(PropKind.Mug, UnlockReveal.After(list, 0).Next);
        }
    }
}
