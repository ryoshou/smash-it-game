using Game.Editor;
using Game.Levels;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Tests
{
    public sealed class LevelEditingTests
    {
        private LevelDef level;
        private PropCatalog catalog;
        private Scene scene;

        [SetUp]
        public void SetUp()
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            catalog = AssetDatabase.LoadAssetAtPath<PropCatalog>(PrototypeAssetBuilder.CatalogPath);
            level = ScriptableObject.CreateInstance<LevelDef>();
            level.hideFlags = HideFlags.DontUnloadUnusedAsset;
            level.id = "EditTest";
            level.shots = 5;
            level.props = new[]
            {
                new PropPlacement { kind = PropKind.Keycap1u, basePosition = new Vector3(-0.3f, 0f, 0.1f), isTarget = true, variant = 2 },
                new PropPlacement { kind = PropKind.Mug, basePosition = new Vector3(0.4f, 0f, -0.2f), yawDegrees = 30f, isTarget = true, variant = 1 },
                new PropPlacement { kind = PropKind.Book, basePosition = new Vector3(0f, 0f, 0.3f), isTarget = false },
            };
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(level);

        [Test]
        public void SpawnThenCapture_RoundTripsPlacements()
        {
            var root = LevelEditing.Spawn(level, catalog, scene);

            var captured = LevelEditing.Capture(root, catalog);

            Assert.AreEqual(level.props.Length, captured.Length);
            for (int i = 0; i < captured.Length; i++)
            {
                var expected = level.props[i];
                Assert.AreEqual(expected.kind, captured[i].kind, $"prop {i} kind");
                Assert.AreEqual(expected.variant, captured[i].variant, $"prop {i} variant");
                Assert.AreEqual(expected.isTarget, captured[i].isTarget, $"prop {i} target");
                Assert.AreEqual(expected.yawDegrees, captured[i].yawDegrees, 1e-3f, $"prop {i} yaw");
                Assert.Less(Vector3.Distance(expected.basePosition, captured[i].basePosition), 1e-4f, $"prop {i} position");
            }
        }

        [Test]
        public void Spawn_HasNoLiveSimulation()
        {
            var root = LevelEditing.Spawn(level, catalog, scene);

            foreach (var body in root.GetComponentsInChildren<Rigidbody>()) Assert.IsTrue(body.isKinematic);
        }

        [Test]
        public void Save_InvalidLevel_RefusesAndKeepsAsset()
        {
            var offDesk = new[] { new PropPlacement { kind = PropKind.Keycap1u, basePosition = new Vector3(5f, 0f, 0f), isTarget = true } };

            string error = LevelEditing.Save(level, offDesk, 7, Difficulty.Hard);

            Assert.IsNotNull(error);
            Assert.AreEqual(5, level.shots);
            Assert.AreEqual(3, level.props.Length);
        }

        [Test]
        public void Save_Valid_WritesShotsAndDifficulty()
        {
            string error = LevelEditing.Save(level, level.props, 7, Difficulty.Hard);

            Assert.IsNull(error);
            Assert.AreEqual(7, level.shots);
            Assert.AreEqual(Difficulty.Hard, level.difficulty);
        }
    }
}
