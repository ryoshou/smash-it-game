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

        [TestCase(1f, "right")]
        [TestCase(-1f, "left")]
        public void SideWalls_StopFlyingProps(float direction, string side)
        {
            bool hit = Physics.Raycast(new Vector3(0f, 1.5f, 0f), new Vector3(direction, 0f, 0f), out var info, 4f);

            Assert.IsTrue(hit, $"nothing stops a prop flying {side}");
            Assert.Less(Mathf.Abs(info.point.x), 2f, $"the {side} wall should be close");
        }

        [Test]
        public void Room_HasAWarmLampLight()
        {
            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);

            Assert.IsTrue(System.Array.Exists(lights, l => l.type == LightType.Point && l.color.r > l.color.b), "no warm lamp");
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
