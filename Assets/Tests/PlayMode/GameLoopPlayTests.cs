using System.Collections;
using Game.Core;
using Game.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    /// <summary>The round loop, on a fixed fixture: the prototype wall level (20 keycaps, 6 shots), whatever the game ships.</summary>
    public sealed class GameLoopPlayTests
    {
        private const string FixtureListPath = "Assets/_Game/Levels/PrototypeLevelList.asset";
        private const int LevelOneTargets = 20;
        private const int LevelOneShots = 6;

        private GameBootstrap bootstrap;
        private LevelSpawner spawner;
        private BallPool pool;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
            bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            spawner = Object.FindAnyObjectByType<LevelSpawner>();
            pool = Object.FindAnyObjectByType<BallPool>();
            Assert.IsNotNull(bootstrap, "scene has no GameBootstrap");
            var so = new UnityEditor.SerializedObject(bootstrap);
            so.FindProperty("levels").objectReferenceValue = UnityEditor.AssetDatabase.LoadAssetAtPath<Game.Levels.LevelList>(FixtureListPath);
            so.ApplyModifiedPropertiesWithoutUndo();
            bootstrap.LoadLevel(0);
            yield return null;
        }

        [UnityTest]
        public IEnumerator GameScene_LoadsLevelOne_WithoutErrors()
        {
            yield return new WaitForSeconds(1f);

            Assert.AreEqual(LevelOneTargets, spawner.TargetsRemaining);
            Assert.AreEqual(LevelOneShots, bootstrap.Budget.Remaining);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator StrongCenterShot_ClearsAtLeastOneTarget()
        {
            bootstrap.FireForTest(new Vector2(0f, -0.2f));
            yield return new WaitForSeconds(4f);

            Assert.Less(spawner.TargetsRemaining, LevelOneTargets);
            Assert.AreEqual(LevelOneShots - 1, bootstrap.Budget.Remaining);
        }

        [UnityTest]
        public IEnumerator Release_BackInPouch_CancelsAndDoesNotSpend()
        {
            bootstrap.FireForTest(new Vector2(0f, -0.01f));
            yield return null;

            Assert.AreEqual(LevelOneShots, bootstrap.Budget.Remaining);
            Assert.IsFalse(pool.AnyActive);
        }

        [UnityTest]
        public IEnumerator OutOfShots_ShowsLose_AndContinueGrantsThree()
        {
            bootstrap.Budget.ResetAttempt(1);
            bootstrap.FireForTest(new Vector2(0.2f, -0.05f));

            float deadline = Time.time + 15f;
            while (bootstrap.State != RoundState.Lost && Time.time < deadline) yield return null;
            Assert.AreEqual(RoundState.Lost, bootstrap.State);
            Assert.AreEqual(LevelOneTargets, spawner.TargetsRemaining, "the wide shot should miss the wall");

            bootstrap.Continue();
            yield return null;

            Assert.AreEqual(3, bootstrap.Budget.Remaining);
            Assert.AreEqual(RoundState.Playing, bootstrap.State);
            Assert.IsFalse(bootstrap.Budget.CanContinue);
        }
    }
}
