using Game.Editor;
using Game.Levels;
using Game.World;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Tests
{
    /// <summary>The raised back desk must be fully on screen on tall and taller phones (portrait).</summary>
    public sealed class BackDeskViewTests
    {
        private const float Margin = 0.02f;
        private Scene scene;

        [TearDown]
        public void TearDown()
        {
            if (scene.IsValid()) EditorSceneManager.CloseScene(scene, true);
        }

        [TestCase(1080, 1920)]
        [TestCase(1080, 2340)]
        public void BackDesk_IsInsideCameraViewAtPhoneAspects(int width, int height)
        {
            scene = EditorSceneManager.OpenScene(PrototypeSceneBuilder.ScenePath, OpenSceneMode.Additive);
            Camera camera = null;
            foreach (var root in scene.GetRootGameObjects())
                if (root.TryGetComponent(out Camera found)) camera = found;
            Assert.IsNotNull(camera, "Game scene has no camera");
            camera.aspect = (float)width / height;
            var level = ScriptableObject.CreateInstance<LevelDef>();
            var size = level.backDeskSize;
            Object.DestroyImmediate(level);

            foreach (float x in new[] { -0.5f, 0.5f })
            foreach (float z in new[] { -0.5f, 0.5f })
            {
                var corner = LevelSpawner.BackDeskTop + new Vector3(x * size.x, 0f, z * size.y);
                var view = camera.WorldToViewportPoint(corner);
                Assert.That(view.x, Is.InRange(Margin, 1f - Margin), $"corner {corner} x");
                Assert.That(view.y, Is.InRange(Margin, 1f - Margin), $"corner {corner} y");
            }
        }

        [Test]
        public void DeskTop_MapsDeskIndex()
        {
            Assert.AreEqual(new Vector3(0f, LevelSpawner.DeskTopY, 0f), LevelSpawner.DeskTop(0));
            Assert.AreEqual(LevelSpawner.BackDeskTop, LevelSpawner.DeskTop(1));
        }
    }
}
