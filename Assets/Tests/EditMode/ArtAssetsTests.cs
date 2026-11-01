using System.Collections.Generic;
using Game.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    public sealed class ArtAssetsTests
    {
        [Test]
        public void AllModels_PassArtCheck()
        {
            var failures = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { "Assets/_Game/Art" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var errors = new List<string>();
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!ArtCheck.Check(ArtMeshStats.FromModel(model), AssetBudget.For(path), errors))
                    failures.Add(path + ": " + string.Join("; ", errors));
            }

            Assert.IsEmpty(failures, string.Join("\n", failures));
        }

        [Test]
        public void GeneratedKeycap1u_MatchesM1ColliderInUnityAxes()
        {
            var stats = Measure("Generated/Keycap1u_cream.fbx");

            Assert.Greater(stats.triangles, 0);
            Assert.AreEqual(1, stats.materialCount);
            AssertSize(new Vector3(0.15f, 0.11f, 0.15f), stats.boundsSize, 0.005f);
        }

        [Test]
        public void GeneratedDesk_IsTwoByOneAndAHalf_OneMetreTall()
        {
            AssertSize(new Vector3(2.0f, 1.0f, 1.5f), Measure("Generated/OfficeDesk.fbx").boundsSize, 0.01f);
        }

        [Test]
        public void GeneratedSlingshot_AnchorsAtM1ForkTips()
        {
            var model = Load("Generated/Slingshot.fbx").transform;
            var left = model.Find("BandAnchorL");
            var right = model.Find("BandAnchorR");

            Assert.IsNotNull(left);
            Assert.IsNotNull(right);
            Assert.AreEqual(0.192f, left.localPosition.y, 0.002f);
            Assert.AreEqual(0.26f, Mathf.Abs(right.localPosition.x - left.localPosition.x), 0.002f);
            Assert.AreEqual(0f, left.localPosition.z, 0.002f);
        }

        [TestCase("Packs/TinyTreats/mug_A_pink.fbx", "Generated/Mug_pink_broken.fbx", 8)]
        [TestCase("Packs/TinyTreats/mug_A_blue.fbx", "Generated/Mug_blue_broken.fbx", 8)]
        [TestCase("Packs/TinyTreats/mug_A_yellow.fbx", "Generated/Mug_yellow_broken.fbx", 8)]
        [TestCase("Packs/KayKitFurniture/cactus_small_A.fbx", "Generated/CactusPot_broken.fbx", 8)]
        public void BrokenSet_LinesUpWithIntactModel(string intact, string broken, int pieces)
        {
            var whole = ArtMeshStats.ModelBounds(Load(intact));
            var brokenModel = Load(broken);
            var shards = ArtMeshStats.ModelBounds(brokenModel);

            Assert.AreEqual(pieces, brokenModel.GetComponentsInChildren<MeshFilter>().Length, "pieces");
            AssertSize(whole.center, shards.center, 0.01f);
            AssertSize(whole.size, shards.size, 0.01f);
        }

        private static GameObject Load(string relative) =>
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/" + relative);

        private static ArtMeshStats Measure(string relative) => ArtMeshStats.FromModel(Load(relative));

        private static void AssertSize(Vector3 expected, Vector3 actual, float tolerance)
        {
            Assert.AreEqual(expected.x, actual.x, tolerance, "x");
            Assert.AreEqual(expected.y, actual.y, tolerance, "y");
            Assert.AreEqual(expected.z, actual.z, tolerance, "z");
        }
    }
}
