using System.Collections.Generic;
using Game.Editor;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class ArtCheckTests
    {
        private static readonly Vector2Int Grid = new Vector2Int(8, 4);

        private readonly List<string> errors = new List<string>();

        [SetUp]
        public void SetUp() => errors.Clear();

        [Test]
        public void Check_UnderBudget_Passes()
        {
            var stats = Stats(1500, 1, InCellUvs(), Vector3.one * 0.3f);

            Assert.IsTrue(ArtCheck.Check(stats, Budget(1500, true), errors), string.Join("; ", errors));
        }

        [Test]
        public void Check_OverTriangleBudget_Fails()
        {
            var stats = Stats(1501, 1, InCellUvs(), Vector3.one * 0.3f);

            Assert.IsFalse(ArtCheck.Check(stats, Budget(1500, true), errors));
            StringAssert.Contains("1501", errors[0]);
        }

        [Test]
        public void Check_TwoMaterials_Fails()
        {
            var stats = Stats(100, 2, InCellUvs(), Vector3.one * 0.3f);

            Assert.IsFalse(ArtCheck.Check(stats, Budget(1500, true), errors));
        }

        [Test]
        public void Check_TriangleStraddlingTwoCells_Fails()
        {
            var uvs = new[] { new Vector2(0.10f, 0.5f), new Vector2(0.13f, 0.5f), new Vector2(0.11f, 0.6f) };
            var stats = Stats(1, 1, uvs, Vector3.one * 0.3f);

            Assert.IsFalse(ArtCheck.Check(stats, Budget(1500, true), errors));
        }

        [Test]
        public void Check_TriangleInsideOneCell_Passes()
        {
            var stats = Stats(1, 1, InCellUvs(), Vector3.one * 0.3f);

            Assert.IsTrue(ArtCheck.Check(stats, Budget(1500, true), errors), string.Join("; ", errors));
        }

        [Test]
        public void Check_StraddlingCells_IgnoredWhenAtlasCellsNotRequired()
        {
            var uvs = new[] { new Vector2(0.10f, 0.5f), new Vector2(0.13f, 0.5f), new Vector2(0.11f, 0.6f) };
            var stats = Stats(1, 1, uvs, Vector3.one * 0.3f);

            Assert.IsTrue(ArtCheck.Check(stats, Budget(1500, false), errors), string.Join("; ", errors));
        }

        [Test]
        public void Check_ScaleOutsideRange_Fails()
        {
            Assert.IsFalse(ArtCheck.Check(Stats(1, 1, InCellUvs(), Vector3.one * 0.005f), Budget(1500, true), errors));
            errors.Clear();
            Assert.IsFalse(ArtCheck.Check(Stats(1, 1, InCellUvs(), Vector3.one * 40f), Budget(1500, true), errors));
        }

        [Test]
        public void For_KeyboardPath_Uses2500()
        {
            Assert.AreEqual(2500, AssetBudget.For("Assets/_Game/Art/Generated/Keyboard.fbx").maxTriangles);
        }

        [Test]
        public void For_CatPath_Uses8000()
        {
            Assert.AreEqual(8000, AssetBudget.For("Assets/_Game/Art/Generated/Cat.fbx").maxTriangles);
        }

        [Test]
        public void For_GeneratedPath_RequiresTinyTreatsCells()
        {
            var budget = AssetBudget.For("Assets/_Game/Art/Generated/Keycap1u_mint.fbx");

            Assert.AreEqual(1500, budget.maxTriangles);
            Assert.IsTrue(budget.requireAtlasCells);
            Assert.AreEqual(Grid, budget.atlasGrid);
        }

        [Test]
        public void For_PackPath_SkipsAtlasCells()
        {
            Assert.IsFalse(AssetBudget.For("Assets/_Game/Art/Packs/TinyTreats/mug_A_pink.fbx").requireAtlasCells);
        }

        [Test]
        public void For_BrokenSetFromPack_SkipsAtlasCells_KeepsPropBudget()
        {
            var budget = AssetBudget.For("Assets/_Game/Art/Generated/CactusPot_broken.fbx");

            Assert.IsFalse(budget.requireAtlasCells);
            Assert.AreEqual(1500, budget.maxTriangles);
        }

        [Test]
        public void For_RoomShell_AllowsRoomSizedBounds()
        {
            Assert.AreEqual(8f, AssetBudget.For("Assets/_Game/Art/Generated/RoomShell.fbx").maxSize);
        }

        private static Vector2[] InCellUvs() =>
            new[] { new Vector2(0.38f, 0.30f), new Vector2(0.40f, 0.30f), new Vector2(0.39f, 0.45f) };

        private static ArtMeshStats Stats(int triangles, int materials, Vector2[] uvs, Vector3 size) =>
            new ArtMeshStats(triangles, materials, uvs, new[] { 0, 1, 2 }, size);

        private static AssetBudget Budget(int maxTriangles, bool requireCells) =>
            new AssetBudget { maxTriangles = maxTriangles, requireAtlasCells = requireCells, atlasGrid = Grid };
    }
}
