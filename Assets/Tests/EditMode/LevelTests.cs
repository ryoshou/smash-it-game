using System.Collections.Generic;
using Game.Levels;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class LevelTests
    {
        private const float Tolerance = 1e-4f;

        private readonly List<Object> created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in created) Object.DestroyImmediate(o);
            created.Clear();
        }

        private LevelDef Level(PropPlacement[] props, int shots = 6, string id = "test")
        {
            var level = ScriptableObject.CreateInstance<LevelDef>();
            level.id = id;
            level.shots = shots;
            level.props = props;
            created.Add(level);
            return level;
        }

        private static PropPlacement Target(Vector3 basePosition) =>
            new PropPlacement { kind = PropKind.Keycap1u, basePosition = basePosition, isTarget = true };

        [Test]
        public void Wall_HasColumnsTimesRows_AllTargets()
        {
            var wall = PrototypeLevels.Wall(5, 4);

            Assert.AreEqual(20, wall.Length);
            foreach (var p in wall)
            {
                Assert.IsTrue(p.isTarget);
                Assert.AreEqual(PropKind.Keycap1u, p.kind);
            }
            for (int r = 0; r < 4; r++)
                Assert.AreEqual(r * 0.112f, wall[r * 5].basePosition.y, Tolerance, $"row {r}");
        }

        [Test]
        public void Pyramid_RowCounts()
        {
            var pyramid = PrototypeLevels.Pyramid(5);

            Assert.AreEqual(15, pyramid.Length);
            for (int r = 0; r < 5; r++)
            {
                float y = r * (PrototypeLevels.Size(PropKind.Keycap2u).y + 0.002f);
                int inRow = System.Array.FindAll(pyramid, p => Mathf.Abs(p.basePosition.y - y) < Tolerance).Length;
                Assert.AreEqual(5 - r, inRow, $"row {r}");
            }
        }

        [Test]
        public void TwoTowers_StaysOnDesk()
        {
            var towers = Level(PrototypeLevels.TwoTowers(5));

            Assert.AreEqual(11, towers.props.Length);
            Assert.IsTrue(towers.Validate(out string error), error);
            Assert.AreEqual(1, System.Array.FindAll(towers.props, p => p.kind == PropKind.KeycapSpace).Length);
        }

        [Test]
        public void PrototypeLayouts_PassValidation()
        {
            var layouts = new[] { PrototypeLevels.Wall(5, 4), PrototypeLevels.Pyramid(5), PrototypeLevels.TwoTowers(5) };

            foreach (var props in layouts)
                Assert.IsTrue(Level(props).Validate(out string error), error);
        }

        [Test]
        public void Validate_RejectsNoTargets()
        {
            var props = new[] { new PropPlacement { kind = PropKind.Keycap1u, isTarget = false } };

            Assert.IsFalse(Level(props).Validate(out string error));
            StringAssert.Contains("target", error);
        }

        [Test]
        public void Validate_RejectsZeroShots()
        {
            Assert.IsFalse(Level(new[] { Target(Vector3.zero) }, shots: 0).Validate(out _));
            Assert.IsFalse(Level(new[] { Target(Vector3.zero) }, shots: 31).Validate(out _));
        }

        [Test]
        public void Validate_RejectsOffDesk()
        {
            Assert.IsFalse(Level(new[] { Target(new Vector3(1.2f, 0f, 0f)) }).Validate(out _));
            Assert.IsFalse(Level(new[] { Target(new Vector3(0f, 0f, 0.8f)) }).Validate(out _));
            Assert.IsFalse(Level(new[] { Target(new Vector3(0f, -0.01f, 0f)) }).Validate(out _));
        }

        [Test]
        public void LevelList_At_WrapsAround()
        {
            var list = ScriptableObject.CreateInstance<LevelList>();
            created.Add(list);
            var second = Level(new[] { Target(Vector3.zero) }, id: "b");
            list.levels = new[] { Level(new[] { Target(Vector3.zero) }, id: "a"), second, Level(new[] { Target(Vector3.zero) }, id: "c") };

            Assert.AreEqual(3, list.Count);
            Assert.AreSame(second, list.At(4));
        }

        [Test]
        public void PropCatalog_Get_MissingKind_Throws()
        {
            var catalog = ScriptableObject.CreateInstance<PropCatalog>();
            created.Add(catalog);
            catalog.specs = new[] { new PropSpec { kind = PropKind.Keycap1u, size = Vector3.one } };

            Assert.AreEqual(Vector3.one, catalog.Get(PropKind.Keycap1u).size);
            Assert.Throws<KeyNotFoundException>(() => catalog.Get(PropKind.KeycapSpace));
        }
    }
}
