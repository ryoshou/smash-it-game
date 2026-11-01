using Game.Levels;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class LayoutSnapTests
    {
        private const float Grid = 0.05f;
        private static readonly Vector2 KeycapFootprint = new Vector2(0.15f, 0.15f);

        private PropCatalog catalog;

        [SetUp]
        public void SetUp()
        {
            catalog = ScriptableObject.CreateInstance<PropCatalog>();
            catalog.specs = new[]
            {
                new PropSpec { kind = PropKind.Keycap1u, size = new Vector3(0.15f, 0.11f, 0.15f) },
                new PropSpec { kind = PropKind.KeycapSpace, size = new Vector3(0.63f, 0.11f, 0.15f) },
            };
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(catalog);

        [Test]
        public void SnapXZ_RoundsToGrid_KeepsY()
        {
            var snapped = LayoutSnap.SnapXZ(new Vector3(0.123f, 0.37f, -0.276f), Grid);

            Assert.AreEqual(0.10f, snapped.x, 1e-5f);
            Assert.AreEqual(0.37f, snapped.y, 1e-5f);
            Assert.AreEqual(-0.30f, snapped.z, 1e-5f);
        }

        [Test]
        public void StackHeight_BareDesk_Zero()
        {
            Assert.AreEqual(0f, LayoutSnap.StackHeight(Vector2.zero, KeycapFootprint, 0f, new PropPlacement[0], catalog));
        }

        [Test]
        public void StackHeight_OnKeycap_IsKeycapTop()
        {
            var others = new[] { Keycap(0f, 0f, 0f) };

            Assert.AreEqual(0.11f, LayoutSnap.StackHeight(new Vector2(0.05f, 0f), KeycapFootprint, 0f, others, catalog), 1e-5f);
        }

        [Test]
        public void StackHeight_OnTwoStacked_IsTopOfUpper()
        {
            var others = new[] { Keycap(0f, 0f, 0f), Keycap(0f, 0.11f, 0f) };

            Assert.AreEqual(0.22f, LayoutSnap.StackHeight(Vector2.zero, KeycapFootprint, 0f, others, catalog), 1e-5f);
        }

        [Test]
        public void StackHeight_NonOverlapping_Ignored()
        {
            var others = new[] { Keycap(0.15f, 0f, 0f) };

            Assert.AreEqual(0f, LayoutSnap.StackHeight(Vector2.zero, KeycapFootprint, 0f, others, catalog));
        }

        [Test]
        public void StackHeight_RotatedFootprintOverlap_Counts()
        {
            // A spacebar at x = 0.5 reaches 0.185 when straight; turned 90° it only covers x 0.425..0.575 …
            var turned = new PropPlacement { kind = PropKind.KeycapSpace, basePosition = new Vector3(0.5f, 0f, 0f), yawDegrees = 90f };
            Assert.AreEqual(0f, LayoutSnap.StackHeight(new Vector2(0.3f, 0f), KeycapFootprint, 0f, new[] { turned }, catalog));

            // … but it now reaches 0.315 along z.
            Assert.AreEqual(0.11f, LayoutSnap.StackHeight(new Vector2(0.5f, 0.25f), KeycapFootprint, 0f, new[] { turned }, catalog), 1e-5f);
        }

        private static PropPlacement Keycap(float x, float y, float z) =>
            new PropPlacement { kind = PropKind.Keycap1u, basePosition = new Vector3(x, y, z) };
    }
}
