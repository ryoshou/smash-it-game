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
                Assert.AreEqual(r * 0.152f, wall[r * 5].basePosition.y, Tolerance, $"row {r}"); // standing keycaps: 0.15 + 2 mm gap
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

        private static PropPlacement BackTarget(Vector3 basePosition) =>
            new PropPlacement { kind = PropKind.Keycap1u, basePosition = basePosition, isTarget = true, desk = 1 };

        [Test]
        public void Validate_BackDeskProp_WithoutBackDesk_Fails()
        {
            Assert.IsFalse(Level(new[] { BackTarget(Vector3.zero) }).Validate(out string error));
            StringAssert.Contains("back desk", error);
        }

        [Test]
        public void Validate_BackDeskProp_UsesBackDeskBounds()
        {
            var inside = Level(new[] { BackTarget(new Vector3(0.5f, 0f, 0.2f)) });
            var outside = Level(new[] { BackTarget(new Vector3(0.7f, 0f, 0f)) });
            inside.hasBackDesk = outside.hasBackDesk = true;

            Assert.IsTrue(inside.Validate(out string error), error);
            Assert.IsFalse(outside.Validate(out _));
        }

        [Test]
        public void Validate_UnknownDesk_Fails()
        {
            var level = Level(new[] { new PropPlacement { kind = PropKind.Mug, isTarget = true, desk = 2 } });
            level.hasBackDesk = true;

            Assert.IsFalse(level.Validate(out _));
        }

        [Test]
        public void Validate_MoreThanMaxProps_Fails()
        {
            var props = new PropPlacement[LevelDef.MaxProps + 1];
            for (int i = 0; i < props.Length; i++) props[i] = Target(Vector3.zero);

            Assert.IsFalse(Level(props).Validate(out string error));
            StringAssert.Contains("props", error);
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
        public void PlaceholderMass_GrowsWithKeycapWidth()
        {
            float oneU = PrototypeLevels.Mass(PropKind.Keycap1u);
            float twoU = PrototypeLevels.Mass(PropKind.Keycap2u);
            float space = PrototypeLevels.Mass(PropKind.KeycapSpace);

            Assert.AreEqual(0.04f, oneU, Tolerance);
            Assert.Less(oneU, twoU);
            Assert.Less(twoU, space);
        }

        [Test]
        public void PlaceholderSizeAndMass_UnknownKind_Throws()
        {
            var unknown = (PropKind)99;

            Assert.Throws<KeyNotFoundException>(() => PrototypeLevels.Size(unknown));
            Assert.Throws<KeyNotFoundException>(() => PrototypeLevels.Mass(unknown));
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

        private static LevelList Room1() =>
            UnityEditor.AssetDatabase.LoadAssetAtPath<LevelList>("Assets/_Game/Levels/Room1/Room1LevelList.asset");

        [Test]
        public void Room1_Has25Levels_WithHardEveryFifth()
        {
            var list = Room1();

            Assert.AreEqual(25, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                var expected = (i + 1) % 5 == 0 ? Difficulty.Hard : Difficulty.Normal;
                Assert.AreEqual(expected, list.At(i).difficulty, $"level {i + 1}");
                Assert.AreEqual($"R1_L{i + 1:00}", list.At(i).id);
                Assert.IsTrue(list.At(i).Validate(out string error), error);
            }
        }

        [Test]
        public void Room1_ShotsWithinFiveToTen()
        {
            var list = Room1();
            for (int i = 0; i < list.Count; i++)
                Assert.That(list.At(i).shots, Is.InRange(5, 10), list.At(i).id);
        }

        /// <summary>Spec §6.1: a new kind every ~10 levels; Room 1 brings the paper cup at 12 and the lamp at 21.</summary>
        [Test]
        public void Room1_NewKindsArriveAtLevels12And21()
        {
            var list = Room1();
            var seen = new HashSet<PropKind>();
            var intros = new List<string>();
            for (int i = 0; i < list.Count; i++)
                foreach (var prop in list.At(i).props)
                    if (seen.Add(UnlockReveal.Family(prop.kind))) intros.Add($"{i + 1}:{UnlockReveal.Family(prop.kind)}");

            CollectionAssert.AreEqual(new[] { "1:Keycap1u", "3:Mug", "4:Book", "5:CactusPot", "12:PaperCup", "21:DeskLamp" }, intros);
        }

        [Test]
        public void Room1_TwoDeskLevels_UseTheBackDesk()
        {
            var list = Room1();
            foreach (int number in new[] { 6, 10, 14, 18, 20, 24, 25 })
            {
                var level = list.At(number - 1);
                Assert.IsTrue(level.hasBackDesk, level.id);
                Assert.IsTrue(System.Array.Exists(level.props, p => p.desk == LevelDef.BackDesk), $"{level.id} has nothing on the back desk");
            }
        }

        [Test]
        public void Room1Levels_EveryPropIsATarget()
        {
            // Owner, checkpoint 2: props holding others up must be cleared too.
            var list = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelList>("Assets/_Game/Levels/Room1/Room1LevelList.asset");
            for (int i = 0; i < list.Count; i++)
                foreach (var prop in list.At(i).props)
                    Assert.IsTrue(prop.isTarget, $"{list.At(i).id}: {prop.kind} at {prop.basePosition} is not a target");
        }
    }
}
