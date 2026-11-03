using System.Collections;
using Game.Levels;
using Game.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    /// <summary>Levels with the raised back desk: props spawn on it, rest there, and are cleared once they reach the floor.</summary>
    public sealed class TwoDeskPlayTests
    {
        private const float StayPutMetres = 0.01f;

        private GameBootstrap bootstrap;
        private LevelSpawner spawner;
        private LevelList list;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
            bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            spawner = Object.FindAnyObjectByType<LevelSpawner>();
            list = ScriptableObject.CreateInstance<LevelList>();
            list.levels = new[] { TwoDeskLevel(), OneDeskLevel() };
            var so = new UnityEditor.SerializedObject(bootstrap);
            so.FindProperty("levels").objectReferenceValue = list;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var level in list.levels) Object.Destroy(level);
            Object.Destroy(list);
        }

        private static LevelDef TwoDeskLevel()
        {
            var level = ScriptableObject.CreateInstance<LevelDef>();
            level.id = "two-desk";
            level.hasBackDesk = true;
            level.props = new[]
            {
                new PropPlacement { kind = PropKind.Keycap1u, basePosition = new Vector3(-0.3f, 0f, 0f), isTarget = true, desk = 1 },
                new PropPlacement { kind = PropKind.Mug, basePosition = new Vector3(0.3f, 0f, 0f), isTarget = true, desk = 1 },
                new PropPlacement { kind = PropKind.Keycap1u, basePosition = Vector3.zero, isTarget = true },
            };
            return level;
        }

        private static LevelDef OneDeskLevel()
        {
            var level = ScriptableObject.CreateInstance<LevelDef>();
            level.id = "one-desk";
            level.props = new[] { new PropPlacement { kind = PropKind.Mug, isTarget = true } };
            return level;
        }

        private static GameObject BackDesk() => GameObject.Find("Back Desk");

        [UnityTest]
        public IEnumerator BackDeskProps_SpawnOnTheBackDesk_AndStayPut()
        {
            bootstrap.LoadLevel(0);
            yield return new WaitForFixedUpdate();
            var start = new Vector3[spawner.Spawned.Count];
            for (int i = 0; i < start.Length; i++) start[i] = spawner.Spawned[i].Body.position;

            yield return new WaitForSeconds(2f);

            Assert.Greater(start[0].y, LevelSpawner.BackDeskTop.y, "back-desk keycap should stand on the back desk");
            Assert.Less(start[2].y, LevelSpawner.BackDeskTop.y, "main-desk keycap should stand on the main desk");
            for (int i = 0; i < start.Length; i++)
                Assert.Less(Vector3.Distance(start[i], spawner.Spawned[i].Body.position), StayPutMetres, spawner.Spawned[i].Kind.ToString());
            Assert.AreEqual(3, spawner.TargetsRemaining);
        }

        [UnityTest]
        public IEnumerator BackDesk_HiddenWhenLevelHasNone()
        {
            bootstrap.LoadLevel(0);
            yield return null;
            Assert.IsNotNull(BackDesk(), "back desk should show on a two-desk level");

            bootstrap.LoadLevel(1);
            yield return null;

            Assert.IsNull(BackDesk(), "back desk should hide on a one-desk level");
        }

        [UnityTest]
        public IEnumerator PropKnockedOffBackOfBackDesk_IsCleared()
        {
            bootstrap.LoadLevel(0);
            yield return new WaitForFixedUpdate();
            var mug = spawner.Spawned[1].Body;

            mug.position = LevelSpawner.BackDeskTop + new Vector3(0.3f, 0.1f, 0.5f); // behind the back desk's rear edge
            yield return new WaitForSeconds(2f);

            Assert.IsTrue(spawner.Spawned[1].IsCleared);
            Assert.AreEqual(2, spawner.TargetsRemaining);
        }

        [UnityTest]
        public IEnumerator PropFallingOntoMainDesk_IsNotCleared()
        {
            bootstrap.LoadLevel(0);
            yield return new WaitForFixedUpdate();
            var mug = spawner.Spawned[1].Body;

            mug.position = new Vector3(0.3f, LevelSpawner.DeskTopY + 0.2f, 0.4f);
            yield return new WaitForSeconds(2f);

            Assert.IsFalse(spawner.Spawned[1].IsCleared);
        }
    }
}
