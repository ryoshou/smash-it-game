using System;
using Game.Core;
using Game.Editor;
using Game.Levels;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    public sealed class CatalogTests
    {
        private PropCatalog catalog;

        [SetUp]
        public void SetUp() => catalog = AssetDatabase.LoadAssetAtPath<PropCatalog>(PrototypeAssetBuilder.CatalogPath);

        [Test]
        public void EveryKind_HasSpecWithPrefab()
        {
            foreach (PropKind kind in Enum.GetValues(typeof(PropKind)))
            {
                var spec = catalog.Get(kind);
                Assert.IsNotNull(spec.Variant(0).prefab, $"{kind} has no prefab");
            }
        }

        [Test]
        public void ShatterSpecs_HaveBrokenPrefabForEveryVariant()
        {
            foreach (var spec in catalog.specs)
            {
                if (spec.breakMode != BreakMode.Shatter) continue;
                foreach (var variant in spec.variants)
                    Assert.IsNotNull(variant.brokenPrefab, $"{spec.kind} variant {variant.prefab?.name} has no broken set");
            }
        }

        [Test]
        public void Variant_OutOfRange_FallsBackToFirst()
        {
            var spec = catalog.Get(PropKind.Mug);

            Assert.AreSame(spec.variants[0].prefab, spec.Variant(99).prefab);
            Assert.AreSame(spec.variants[0].prefab, spec.Variant(-1).prefab);
        }

        // Keycaps stand up with the letter towards the camera (owner, checkpoint 2): width x 0.15 tall x 0.11 deep, the
        // collider matching the new shape. Masses and the physics material stay M1's.
        [TestCase(PropKind.Keycap1u, 0.15f, 0.15f, 0.11f, 0.04f)]
        [TestCase(PropKind.Keycap2u, 0.31f, 0.15f, 0.11f, 0.07f)]
        [TestCase(PropKind.KeycapSpace, 0.63f, 0.15f, 0.11f, 0.12f)]
        [TestCase(PropKind.KeycapEnter, 0.225f, 0.15f, 0.11f, 0.06f)]
        public void KeycapSpecs_StandUpright_KeepM1Masses(PropKind kind, float x, float y, float z, float mass)
        {
            var spec = catalog.Get(kind);

            Assert.AreEqual(new Vector3(x, y, z), spec.size);
            Assert.AreEqual(mass, spec.mass, 1e-6f);
            Assert.AreEqual(PropColliderShape.Box, spec.collider);
            Assert.AreEqual(0.6f, spec.material.dynamicFriction, 1e-6f);
            Assert.AreEqual(0.8f, spec.material.staticFriction, 1e-6f);
            Assert.AreEqual(0.05f, spec.material.bounciness, 1e-6f);
        }

        [Test]
        public void KeycapPrefab_ColliderMatchesStandingShape()
        {
            var prefab = catalog.Get(PropKind.Keycap1u).Variant(0).prefab;
            var box = prefab.GetComponent<BoxCollider>();

            Assert.IsNotNull(box);
            Assert.AreEqual(new Vector3(0.15f, 0.15f, 0.11f), box.size);
            Assert.AreEqual(Vector3.zero, box.center);
        }

        [TestCase(PropKind.Mug, BreakMode.Shatter, PropColliderShape.ConvexMesh)]
        [TestCase(PropKind.Book, BreakMode.Pop, PropColliderShape.Box)]
        [TestCase(PropKind.CactusPot, BreakMode.Shatter, PropColliderShape.ConvexMesh)]
        [TestCase(PropKind.KeycapEnter, BreakMode.Pop, PropColliderShape.Box)]
        [TestCase(PropKind.PaperCup, BreakMode.Pop, PropColliderShape.ConvexMesh)]
        [TestCase(PropKind.DeskLamp, BreakMode.Pop, PropColliderShape.ConvexMesh)]
        [TestCase(PropKind.Plate, BreakMode.Shatter, PropColliderShape.ConvexMesh)]
        [TestCase(PropKind.Donut, BreakMode.Pop, PropColliderShape.Box)]
        [TestCase(PropKind.MilkBottle, BreakMode.Shatter, PropColliderShape.ConvexMesh)]
        public void NewKinds_HaveTheirPersonality(PropKind kind, BreakMode mode, PropColliderShape shape)
        {
            var spec = catalog.Get(kind);

            Assert.AreEqual(mode, spec.breakMode);
            Assert.AreEqual(shape, spec.collider);
            Assert.Greater(spec.mass, 0f);
            Assert.Greater(spec.size.y, 0f);
        }

        [Test]
        public void PaperCup_IsLighterThanEveryKeycap()
        {
            float cup = catalog.Get(PropKind.PaperCup).mass;
            foreach (var kind in new[] { PropKind.Keycap1u, PropKind.Keycap2u, PropKind.KeycapSpace, PropKind.KeycapEnter })
                Assert.Less(cup, catalog.Get(kind).mass, kind.ToString());
        }

        [Test]
        public void PaperCup_IsAboutAFifthTallerThanAMug()
        {
            float ratio = catalog.Get(PropKind.PaperCup).size.y / catalog.Get(PropKind.Mug).size.y;
            Assert.That(ratio, Is.InRange(1.1f, 1.3f));
        }

        [Test]
        public void DeskLamp_IsAboutFortyFiveCentimetresTall() =>
            Assert.AreEqual(0.45f, catalog.Get(PropKind.DeskLamp).size.y, 0.01f);

        /// <summary>The plate is wide enough to stand a mug on (Tiny Treats mugs are 0.43 deep), and thin.</summary>
        [Test]
        public void Plate_IsFlatterThanItIsWide()
        {
            var plate = catalog.Get(PropKind.Plate).size;
            var mug = catalog.Get(PropKind.Mug).size;

            Assert.Less(plate.y * 5f, plate.x);
            Assert.Greater(plate.x, mug.z, "a mug's base fits on a plate");
            Assert.AreEqual(0.45f, plate.x, 0.02f);
        }

        [Test]
        public void Donut_LiesFlat_ForStacking()
        {
            var size = catalog.Get(PropKind.Donut).size;

            Assert.Less(size.y * 2f, size.x);
            Assert.Less(catalog.Get(PropKind.Donut).mass, catalog.Get(PropKind.Keycap1u).mass * 2f, "a donut is light");
        }

        [Test]
        public void MilkBottle_IsTallerThanAMug()
        {
            float milk = catalog.Get(PropKind.MilkBottle).size.y;

            Assert.Greater(milk, catalog.Get(PropKind.Mug).size.y);
            Assert.AreEqual(0.42f, milk, 0.02f);
        }

        [Test]
        public void Book_LiesFlat_ForStacking()
        {
            var size = catalog.Get(PropKind.Book).size;

            Assert.Less(size.y, size.x);
            Assert.Less(size.y, size.z);
        }

        [Test]
        public void Mug_IsUprightAtPackScale()
        {
            Assert.AreEqual(0.35f, catalog.Get(PropKind.Mug).size.y, 0.02f);
        }

        [Test]
        public void CactusPot_IsTheHeaviest()
        {
            float pot = catalog.Get(PropKind.CactusPot).mass;
            foreach (var spec in catalog.specs)
                if (spec.kind != PropKind.CactusPot) Assert.Less(spec.mass, pot, spec.kind.ToString());
        }
    }
}
