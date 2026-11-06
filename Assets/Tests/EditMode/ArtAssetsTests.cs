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
        public void GeneratedKeycap1u_StandsUpInUnityAxes()
        {
            var stats = Measure("Generated/Keycap1u_cream.fbx");

            Assert.Greater(stats.triangles, 0);
            Assert.AreEqual(1, stats.materialCount);
            AssertSize(new Vector3(0.15f, 0.15f, 0.11f), stats.boundsSize, 0.005f);
        }

        [TestCase("Generated/Keycap1u_cream.fbx")]
        [TestCase("Generated/Keycap2u_pink.fbx")]
        [TestCase("Generated/KeycapEnter_sky.fbx")]
        public void GeneratedKeycap_LetterFacesTheCamera(string model)
        {
            // The frontmost surface (-z, towards the camera) is the raised legend, in a darker cell than the body.
            var root = Load(model);
            var filter = root.GetComponentInChildren<MeshFilter>();
            var toModel = root.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            var vertices = filter.sharedMesh.vertices;
            var uvs = filter.sharedMesh.uv;
            int front = 0;
            float frontZ = float.MaxValue;
            for (int i = 0; i < vertices.Length; i++)
            {
                float z = toModel.MultiplyPoint3x4(vertices[i]).z;
                if (z < frontZ) { frontZ = z; front = i; }
            }
            int bodyRow = CellOf(uvs[Highest(vertices, toModel)]).y;
            Assert.AreNotEqual(CellOf(uvs[Highest(vertices, toModel)]), CellOf(uvs[front]),
                $"frontmost point (z {frontZ}) should be the legend, not the body (body row {bodyRow})");
        }

        private static int Highest(Vector3[] vertices, Matrix4x4 toModel)
        {
            int top = 0;
            for (int i = 1; i < vertices.Length; i++)
                if (toModel.MultiplyPoint3x4(vertices[i]).y > toModel.MultiplyPoint3x4(vertices[top]).y) top = i;
            return top;
        }

        private static Vector2Int CellOf(Vector2 uv) => new Vector2Int(Mathf.FloorToInt(uv.x * 8f), Mathf.FloorToInt(uv.y * 4f));

        // Upright sizes in Unity axes (y = height). A model imported on its side (e.g. the plain "fbx" instead of the
        // "fbx (unity)" variant) puts its height on another axis and fails here; the validator only sees the largest side.
        [TestCase("Packs/TinyTreats/mug_A_pink.fbx", 0.585f, 0.35f, 0.432f)]
        [TestCase("Packs/TinyTreats/mug_A_blue.fbx", 0.585f, 0.35f, 0.432f)]
        [TestCase("Packs/TinyTreats/mug_A_yellow.fbx", 0.585f, 0.35f, 0.432f)]
        [TestCase("Packs/KayKitFurniture/book_single.fbx", 0.26f, 0.5f, 0.365f)]
        [TestCase("Packs/KayKitFurniture/cactus_small_A.fbx", 0.5f, 0.552f, 0.5f)]
        [TestCase("Packs/TinyTreats/coffee_cup_takeaway.fbx", 0.342f, 0.41f, 0.342f)]
        [TestCase("Packs/KayKitFurniture/lamp_table.fbx", 1f, 1.022f, 1f)]
        [TestCase("Packs/TinyTreats/plate.fbx", 0.75f, 0.1f, 0.75f)]
        [TestCase("Packs/TinyTreats/milk.fbx", 0.363f, 0.468f, 0.332f)]
        [TestCase("Packs/TinyTreatsBakedGoods/donut.fbx", 0.42f, 0.18f, 0.42f)]
        [TestCase("Packs/TinyTreatsBakedGoods/donut_pink.fbx", 0.417f, 0.205f, 0.417f)]
        [TestCase("Packs/TinyTreatsBakedGoods/donut_chocolate.fbx", 0.417f, 0.205f, 0.417f)]
        [TestCase("Packs/KayKitHalloween/candle.fbx", 0.301f, 0.786f, 0.296f)]
        [TestCase("Packs/KayKitHalloween/candle_thin.fbx", 0.174f, 0.786f, 0.171f)]
        [TestCase("Packs/KayKitBoardGame/hourglass.fbx", 1f, 2.5f, 1f)]
        [TestCase("Packs/TinyTreats/countertop_sink.fbx", 2.0f, 1.758f, 1.501f)]
        [TestCase("Packs/TinyTreats/countertop_single.fbx", 1.0f, 1.0f, 1.501f)]
        [TestCase("Packs/TinyTreats/stove.fbx", 2.0f, 1.3f, 1.676f)]
        [TestCase("Packs/TinyTreats/fridge.fbx", 2.04f, 3.0f, 1.616f)]
        [TestCase("Packs/TinyTreats/kettle.fbx", 0.813f, 0.595f, 0.6f)]
        [TestCase("Packs/TinyTreats/toaster.fbx", 0.916f, 0.6f, 0.5f)]
        [TestCase("Packs/TinyTreats/blinds_kitchen.fbx", 1.6f, 1.95f, 0.3f)]
        [TestCase("Packs/TinyTreats/dishrack_plates.fbx", 0.75f, 0.784f, 0.8f)]
        [TestCase("Packs/TinyTreats/plate_A.fbx", 0.75f, 0.1f, 0.75f)]
        [TestCase("Packs/TinyTreats/bowl.fbx", 0.4f, 0.206f, 0.4f)]
        [TestCase("Packs/TinyTreats/teapot.fbx", 0.919f, 0.614f, 0.62f)]
        [TestCase("Packs/TinyTreats/wine_glass.fbx", 0.4f, 0.45f, 0.4f)]
        [TestCase("Packs/TinyTreats/jam.fbx", 0.39f, 0.482f, 0.39f)]
        [TestCase("Packs/KayKitRestaurant/ketchup.fbx", 0.3f, 0.779f, 0.3f)]
        [TestCase("Packs/TinyTreats/drink_can.fbx", 0.325f, 0.506f, 0.325f)]
        [TestCase("Packs/TinyTreats/apple.fbx", 0.439f, 0.356f, 0.439f)]
        [TestCase("Packs/KayKitRestaurant/food_ingredient_tomato.fbx", 0.75f, 0.7f, 0.75f)]
        [TestCase("Packs/TinyTreats/cheese_A.fbx", 0.243f, 0.28f, 0.375f)]
        [TestCase("Packs/TinyTreatsBakedGoods/cupcake.fbx", 0.362f, 0.51f, 0.362f)]
        [TestCase("Packs/TinyTreatsBakedGoods/croissant.fbx", 0.737f, 0.307f, 0.389f)]
        [TestCase("Packs/TinyTreats/sandwich.fbx", 0.622f, 0.436f, 0.624f)]
        public void PackProp_ImportsUpright(string model, float x, float y, float z)
        {
            AssertSize(new Vector3(x, y, z), Measure(model).boundsSize, 0.01f);
        }

        [TestCase("OfficeDesk")]
        [TestCase("CafeDesk")]
        [TestCase("LibraryDesk")]
        [TestCase("DenDesk")]
        [TestCase("KitchenDesk")]
        public void GeneratedDesk_IsTwoByOneAndAHalf_OneMetreTall(string model)
        {
            AssertSize(new Vector3(2.0f, 1.0f, 1.5f), Measure($"Generated/{model}.fbx").boundsSize, 0.01f);
        }

        [TestCase("Generated/PencilCup_teal.fbx", 600)]
        [TestCase("Generated/PencilCup_rose.fbx", 600)]
        [TestCase("Generated/PencilCup_butter.fbx", 600)]
        public void GeneratedPencilCup_IsUprightAndInBudget(string model, int maxTriangles)
        {
            var stats = Measure(model);

            Assert.AreEqual(0.4f, stats.boundsSize.y, 0.02f, "pencil tips reach about 0.4");
            Assert.Less(stats.boundsSize.x, 0.25f);
            Assert.Less(stats.boundsSize.z, 0.25f);
            Assert.LessOrEqual(stats.triangles, maxTriangles);
        }

        [TestCase("Generated/SodaCan_red.fbx")]
        [TestCase("Generated/SodaCan_blue.fbx")]
        [TestCase("Generated/SodaCan_grass.fbx")]
        [TestCase("Generated/FizzySoda.fbx")]
        public void GeneratedSodaCan_IsUprightAndInBudget(string model)
        {
            var stats = Measure(model);

            Assert.AreEqual(0.3f, stats.boundsSize.y, 0.02f);
            Assert.Less(stats.boundsSize.x, 0.23f);
            Assert.LessOrEqual(stats.triangles, model.Contains("Fizzy") ? 1000 : 500);
        }

        [TestCase("Generated/GameController_charcoal.fbx")]
        [TestCase("Generated/GameController_red.fbx")]
        [TestCase("Generated/GameController_sky.fbx")]
        public void GeneratedController_LiesFlatAndInBudget(string model)
        {
            var stats = Measure(model);

            Assert.AreEqual(0.4f, stats.boundsSize.x, 0.02f);
            Assert.Less(stats.boundsSize.y, 0.11f);
            Assert.AreEqual(0.26f, stats.boundsSize.z, 0.02f);
            Assert.LessOrEqual(stats.triangles, 1200);
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
        [TestCase("Packs/TinyTreats/plate.fbx", "Generated/Plate_broken.fbx", 8)]
        [TestCase("Packs/TinyTreats/milk.fbx", "Generated/MilkBottle_broken.fbx", 8)]
        [TestCase("Packs/KayKitBoardGame/hourglass.fbx", "Generated/Hourglass_broken.fbx", 8)]
        [TestCase("Packs/TinyTreatsBakedGoods/donut.fbx", "Generated/Donut_broken.fbx", 4)]
        [TestCase("Packs/TinyTreatsBakedGoods/donut_pink.fbx", "Generated/Donut_pink_broken.fbx", 4)]
        [TestCase("Packs/TinyTreatsBakedGoods/donut_chocolate.fbx", "Generated/Donut_chocolate_broken.fbx", 4)]
        [TestCase("Packs/TinyTreats/plate_A.fbx", "Generated/DinnerPlate_broken.fbx", 8)]
        [TestCase("Packs/TinyTreats/bowl.fbx", "Generated/Bowl_broken.fbx", 8)]
        [TestCase("Packs/TinyTreats/teapot.fbx", "Generated/Teapot_broken.fbx", 8)]
        [TestCase("Packs/TinyTreats/wine_glass.fbx", "Generated/WineGlass_broken.fbx", 8)]
        [TestCase("Packs/TinyTreats/jam.fbx", "Generated/JamJar_broken.fbx", 6)]
        [TestCase("Packs/KayKitRestaurant/ketchup.fbx", "Generated/KetchupBottle_broken.fbx", 6)]
        [TestCase("Packs/TinyTreats/apple.fbx", "Generated/Apple_broken.fbx", 4)]
        [TestCase("Packs/KayKitRestaurant/food_ingredient_tomato.fbx", "Generated/Tomato_broken.fbx", 4)]
        [TestCase("Packs/TinyTreats/cheese_A.fbx", "Generated/Cheese_broken.fbx", 4)]
        [TestCase("Generated/Burger.fbx", "Generated/Burger_broken.fbx", 6)]
        [TestCase("Packs/TinyTreatsBakedGoods/cupcake.fbx", "Generated/Cupcake_broken.fbx", 4)]
        [TestCase("Packs/TinyTreatsBakedGoods/croissant.fbx", "Generated/Croissant_broken.fbx", 4)]
        [TestCase("Packs/TinyTreats/sandwich.fbx", "Generated/Sandwich_broken.fbx", 4)]
        public void BrokenSet_LinesUpWithIntactModel(string intact, string broken, int pieces)
        {
            var whole = ArtMeshStats.ModelBounds(Load(intact));
            var brokenModel = Load(broken);
            var shards = ArtMeshStats.ModelBounds(brokenModel);

            Assert.AreEqual(pieces, brokenModel.GetComponentsInChildren<MeshFilter>().Length, "pieces");
            AssertSize(whole.center, shards.center, 0.01f);
            AssertSize(whole.size, shards.size, 0.01f);
        }

        [TestCase("RoomShell")]
        [TestCase("RoomShell_Cafe")]
        [TestCase("RoomShell_Library")]
        [TestCase("RoomShell_Den")]
        [TestCase("RoomShell_Kitchen")]
        public void GeneratedRoomShell_EnclosesTheDesk(string model)
        {
            // Owner, checkpoint 2: it should feel like being inside a room. The portrait camera only sees about
            // +/-2 m at the back wall, so the side walls sit close enough to show as corners and the ceiling edge shows
            // along the top.
            var room = Load($"Generated/{model}.fbx").transform;

            Assert.Greater(Center(room, "BackWall").z, 3f, "back wall away from the player");
            Assert.Less(Center(room, "LeftWall").x, -1.5f, "left wall on the left");
            Assert.Greater(Center(room, "LeftWall").x, -2f, "left wall close enough to see");
            Assert.Greater(Center(room, "RightWall").x, 1.5f, "right wall on the right");
            Assert.Less(Center(room, "RightWall").x, 2f, "right wall close enough to see");
            Assert.AreEqual(3.1f, Center(room, "Ceiling").y, 0.1f, "ceiling");
            Assert.IsNotNull(room.Find("Pendant"), "a hanging lamp");
        }

        private static Vector3 Center(Transform room, string part)
        {
            var child = room.Find(part);
            Assert.IsNotNull(child, $"room shell has no {part}");
            return child.TransformPoint(ArtMeshStats.ModelBounds(child.gameObject).center);
        }

        [Test]
        public void GeneratedMonitor_ScreenFacesThePlayer()
        {
            // The frontmost surface (towards the camera, -z) must be the coloured desktop, not the charcoal bezel back.
            var model = Load("Generated/Monitor.fbx");
            var filter = model.GetComponentInChildren<MeshFilter>();
            var toModel = model.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
            var vertices = filter.sharedMesh.vertices;
            var uvs = filter.sharedMesh.uv;
            const float aboveBase = 0.3f;
            int front = -1;
            float frontZ = float.MaxValue;
            for (int i = 0; i < vertices.Length; i++)
            {
                var p = toModel.MultiplyPoint3x4(vertices[i]);
                if (p.y <= aboveBase || p.z >= frontZ) continue;
                frontZ = p.z;
                front = i;
            }
            var charcoal = new Vector2Int(0, 3);
            var cell = new Vector2Int(Mathf.FloorToInt(uvs[front].x * 8f), Mathf.FloorToInt(uvs[front].y * 4f));

            Assert.AreNotEqual(charcoal, cell, $"frontmost surface at z {frontZ} is the bezel");
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
