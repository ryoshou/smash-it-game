using Game.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Tests
{
    public sealed class BackdropTests
    {
        private const float AboveEverything = 5f;

        [OneTimeSetUp]
        public void Load()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(HomeOfficeBuilder.BackdropPath));
            Physics.SyncTransforms();
        }

        // Props knocked off the back of the desk can land here: they must not fall through the furniture.
        [TestCase(-1.45f, 2.1f, "side table")]
        [TestCase(1.4f, 3.2f, "shelf")]
        public void FallingProps_LandOnFurniture(float x, float z, string what)
        {
            bool hit = Physics.Raycast(new Vector3(x, AboveEverything, z), Vector3.down, out var info, 10f);

            Assert.IsTrue(hit, $"nothing under the {what}");
            Assert.Greater(info.point.y, 0.3f, $"a prop would fall through the {what} to the floor");
        }
    }
}
