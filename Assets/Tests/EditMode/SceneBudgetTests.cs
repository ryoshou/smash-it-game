using System.Collections.Generic;
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

        /// <summary>
        /// Spec §3: every game level's visible scene (its own room's backdrop with the desk look, the back desk, the
        /// props) stays under 100k triangles.
        /// </summary>
        [Test]
        public void SceneBudget_EveryGameLevel()
        {
            int sceneTriangles = SceneTriangles();
            var bootstrap = new SerializedObject(Object.FindAnyObjectByType<GameBootstrap>());
            var levels = (LevelList)bootstrap.FindProperty("levels").objectReferenceValue;
            var catalog = (PropCatalog)bootstrap.FindProperty("catalog").objectReferenceValue;
            Assert.AreEqual("GameLevelList", levels.name, "the scene should play the game list");
            var backdrops = new Dictionary<RoomTheme, int>();

            for (int i = 0; i < levels.Count; i++)
            {
                var level = levels.At(i);
                Assert.IsNotNull(level.room, $"{level.id} has no room");
                if (!backdrops.TryGetValue(level.room, out int room))
                    backdrops[level.room] = room = PrefabTriangles(level.room.backdropPrefab);
                int triangles = sceneTriangles + room;
                foreach (var placement in level.props)
                    triangles += PrefabTriangles(catalog.Get(placement.kind).Variant(placement.variant).prefab);

                Assert.LessOrEqual(level.props.Length, LevelDef.MaxProps, level.id);
                Assert.LessOrEqual(triangles, VisibleTriangleBudget, level.id);
                Assert.Greater(room, 1000, $"{level.id}: suspiciously few backdrop triangles");
            }
        }

        [Test]
        public void SceneDesk_HasNoRenderer()
        {
            var desk = AssetDatabase.LoadAssetAtPath<GameObject>(PrototypeSceneBuilder.DeskPrefabPath);

            Assert.IsNotNull(desk, "desk prefab missing");
            Assert.IsEmpty(desk.GetComponentsInChildren<Renderer>(true), "the desk look belongs to each room's backdrop");
            Assert.IsNotEmpty(desk.GetComponentsInChildren<BoxCollider>(true), "the play desk keeps its colliders");
        }

        /// <summary>The desk look (top 2.0 x 1.5 at y 1.0, centred) sits where the play desk's collider is.</summary>
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void EveryRoomBackdrop_HasItsDeskAtTheOrigin(int room) =>
            AssertDeskAtTheOrigin(AssetDatabase.LoadAssetAtPath<RoomTheme>(GameLevelListBuilder.ThemePath(room)), $"room {room}");

        [Test]
        public void KitchenBackdrop_HasItsDeskAtTheOrigin() =>
            AssertDeskAtTheOrigin(AssetDatabase.LoadAssetAtPath<RoomTheme>(KitchenBuilder.ThemePath), "the Kitchen");

        /// <summary>Kitchen spec §2: feasts hold up to 40 props, so the room leaves them 40k of the 100k budget.</summary>
        [Test]
        public void KitchenBackdrop_UnderTriangleBudget()
        {
            const int KitchenRoomBudget = 60000;
            int room = PrefabTriangles(AssetDatabase.LoadAssetAtPath<RoomTheme>(KitchenBuilder.ThemePath).backdropPrefab);

            Assert.LessOrEqual(SceneTriangles() + room, KitchenRoomBudget);
        }

        private static void AssertDeskAtTheOrigin(RoomTheme theme, string room)
        {
            Assert.IsNotNull(theme, $"{room} has no theme");
            var look = theme.backdropPrefab.transform.Find(RoomKit.DeskLookName);
            Assert.IsNotNull(look, $"{room}'s backdrop has no {RoomKit.DeskLookName}");
            var instance = Object.Instantiate(look.gameObject);
            try
            {
                var renderers = instance.GetComponentsInChildren<Renderer>();
                Assert.IsNotEmpty(renderers, room);
                var bounds = renderers[0].bounds;
                foreach (var r in renderers) bounds.Encapsulate(r.bounds);
                Assert.AreEqual(LevelSpawner.DeskTopY, bounds.max.y, 0.01f, "desk top height");
                Assert.AreEqual(0f, bounds.center.x, 0.02f);
                Assert.AreEqual(0f, bounds.center.z, 0.02f);
                Assert.AreEqual(2.0f, bounds.size.x, 0.05f);
                Assert.AreEqual(1.5f, bounds.size.z, 0.05f);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        /// <summary>
        /// Kitchen spec §6: a splat lies just above the floor collider (y 0), so a rug over it must be thinner than the
        /// splat's lift or it hides the splat.
        /// </summary>
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void EveryRoomBackdrop_RugLiesUnderTheSplat(int room)
        {
            var theme = AssetDatabase.LoadAssetAtPath<RoomTheme>(GameLevelListBuilder.ThemePath(room));
            float lift = AssetDatabase.LoadAssetAtPath<Game.Juice.JuiceConfig>("Assets/_Game/Config/JuiceConfig.asset").splatLift;
            var instance = Object.Instantiate(theme.backdropPrefab);
            try
            {
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                    if (renderer.name.StartsWith("rug"))
                        Assert.Less(renderer.bounds.max.y, lift, $"room {room}: {renderer.name} hides splats");
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        /// <summary>Opens the game scene and counts its own triangles (includes the back desk, shown or not).</summary>
        private int SceneTriangles()
        {
            scene = EditorSceneManager.OpenScene(PrototypeSceneBuilder.ScenePath, OpenSceneMode.Additive);
            int triangles = 0;
            foreach (var go in scene.GetRootGameObjects())
                foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
                    triangles += Triangles(renderer);
            return triangles;
        }

        private static int PrefabTriangles(GameObject prefab)
        {
            int triangles = 0;
            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true)) triangles += Triangles(renderer);
            return triangles;
        }

        private static int Triangles(Renderer renderer)
        {
            Mesh mesh = renderer is SkinnedMeshRenderer skinned ? skinned.sharedMesh
                : renderer.TryGetComponent(out MeshFilter filter) ? filter.sharedMesh : null;
            return mesh == null ? 0 : mesh.triangles.Length / 3;
        }
    }
}
