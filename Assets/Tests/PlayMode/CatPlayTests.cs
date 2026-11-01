using System.Collections;
using Game.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public sealed class CatPlayTests
    {
        private const float ReactSeconds = 0.5f;

        private GameBootstrap bootstrap;
        private Animator cat;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
            bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            cat = GameObject.Find("Cat").GetComponentInChildren<Animator>();
            Assert.IsNotNull(cat, "scene has no cat");
        }

        [UnityTest]
        public IEnumerator RoundWon_CatEntersCelebrate()
        {
            bootstrap.Events.RaiseRoundEnded(RoundState.Won);

            yield return WaitForState("Celebrate");
        }

        [UnityTest]
        public IEnumerator Fired_CatEntersRelease()
        {
            bootstrap.FireForTest(new Vector2(0f, -0.2f));

            yield return WaitForState("Release");
        }

        [UnityTest]
        public IEnumerator MissedShot_CatEntersAnnoyed()
        {
            // Straight up and as far sideways as the cone allows: nothing on the desk is in the way.
            bootstrap.FireForTest(new Vector2(0.24f, -0.05f));

            float deadline = Time.time + bootstrap.Tuning.ballLifetimeSeconds + 1f;
            while (Time.time < deadline && !cat.GetCurrentAnimatorStateInfo(0).IsName("Annoyed")) yield return null;

            Assert.IsTrue(cat.GetCurrentAnimatorStateInfo(0).IsName("Annoyed"), "the cat should be annoyed by a miss");
        }

        [UnityTest]
        public IEnumerator Pulling_CatEntersAim()
        {
            bootstrap.Events.RaisePullChanged(0.6f);

            yield return WaitForState("Aim");
        }

        private IEnumerator WaitForState(string state)
        {
            float deadline = Time.time + ReactSeconds;
            while (Time.time < deadline && !cat.GetCurrentAnimatorStateInfo(0).IsName(state)) yield return null;
            Assert.IsTrue(cat.GetCurrentAnimatorStateInfo(0).IsName(state), $"the cat should be in {state}");
        }
    }
}
