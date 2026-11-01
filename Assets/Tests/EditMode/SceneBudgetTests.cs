using Game.Editor;
using Game.Levels;
using Game.World;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Tests
{
    public sealed class SceneBudgetTests
    {
        private const int VisibleTriangleBudget = 100000;

        private Scene scene;

        [TearDown]
        public void TearDown()
        {
            if (scene.IsValid()) EditorSceneManager.CloseScene(scene, true);
        }

        [Test]
        public void GameScene_VisibleTrianglesUnder100k()
        {
            var theme = AssetDatabase.LoadAssetAtPath<RoomTheme>(HomeOfficeBuilder.ThemePath);
            Assert.IsNotNull(theme, "Home Office theme asset is missing");
            scene = EditorSceneManager.OpenScene(PrototypeSceneBuilder.ScenePath, OpenSceneMode.Additive);
            var root = new GameObject("BudgetProbe");
            SceneManager.MoveGameObjectToScene(root, scene);
            Object.Instantiate(theme.backdropPrefab, root.transform);
            var bootstrap = new SerializedObject(Object.FindAnyObjectByType<GameBootstrap>());
            var levels = (LevelList)bootstrap.FindProperty("levels").objectReferenceValue;
            var catalog = (PropCatalog)bootstrap.FindProperty("catalog").objectReferenceValue;
            foreach (var placement in levels.At(0).props)
                Object.Instantiate(catalog.Get(placement.kind).Variant(0).prefab, root.transform);

            int triangles = 0;
            foreach (var go in scene.GetRootGameObjects())
                foreach (var renderer in go.GetComponentsInChildren<Renderer>())
                    triangles += Triangles(renderer);

            Assert.LessOrEqual(triangles, VisibleTriangleBudget);
            Assert.Greater(triangles, 1000, "suspiciously few triangles: did the room load?");
        }

        private static int Triangles(Renderer renderer)
        {
            Mesh mesh = renderer is SkinnedMeshRenderer skinned ? skinned.sharedMesh
                : renderer.TryGetComponent(out MeshFilter filter) ? filter.sharedMesh : null;
            return mesh == null ? 0 : mesh.triangles.Length / 3;
        }
    }
}
