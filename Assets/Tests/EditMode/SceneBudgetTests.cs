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

        /// <summary>Spec §3: every Room 1 level's visible scene (room, desks, props) stays under 100k triangles.</summary>
        [Test]
        public void SceneBudget_EveryRoom1Level()
        {
            var theme = AssetDatabase.LoadAssetAtPath<RoomTheme>(HomeOfficeBuilder.ThemePath);
            Assert.IsNotNull(theme, "Home Office theme asset is missing");
            scene = EditorSceneManager.OpenScene(PrototypeSceneBuilder.ScenePath, OpenSceneMode.Additive);
            var bootstrap = new SerializedObject(Object.FindAnyObjectByType<GameBootstrap>());
            var levels = (LevelList)bootstrap.FindProperty("levels").objectReferenceValue;
            var catalog = (PropCatalog)bootstrap.FindProperty("catalog").objectReferenceValue;
            var backdrop = Object.Instantiate(theme.backdropPrefab);
            SceneManager.MoveGameObjectToScene(backdrop, scene);
            int baseline = 0;
            foreach (var go in scene.GetRootGameObjects())
                foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
                    baseline += Triangles(renderer); // includes the back desk, shown or not

            for (int i = 0; i < levels.Count; i++)
            {
                var level = levels.At(i);
                int triangles = baseline;
                foreach (var placement in level.props)
                    foreach (var renderer in catalog.Get(placement.kind).Variant(placement.variant).prefab.GetComponentsInChildren<Renderer>())
                        triangles += Triangles(renderer);

                Assert.LessOrEqual(level.props.Length, LevelDef.MaxProps, level.id);
                Assert.LessOrEqual(triangles, VisibleTriangleBudget, level.id);
            }
            Assert.Greater(baseline, 1000, "suspiciously few triangles: did the room load?");
        }

        private static int Triangles(Renderer renderer)
        {
            Mesh mesh = renderer is SkinnedMeshRenderer skinned ? skinned.sharedMesh
                : renderer.TryGetComponent(out MeshFilter filter) ? filter.sharedMesh : null;
            return mesh == null ? 0 : mesh.triangles.Length / 3;
        }
    }
}
