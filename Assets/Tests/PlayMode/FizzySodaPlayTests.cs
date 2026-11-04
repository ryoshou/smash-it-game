using System.Collections;
using Game.Levels;
using Game.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    /// <summary>The fizzy soda (spec §6.2): knocked over, it bursts, pushes its neighbours and sets other fizzies off.</summary>
    public sealed class FizzySodaPlayTests
    {
        private const string CatalogPath = "Assets/_Game/Config/PropCatalog.asset";

        private GameBootstrap bootstrap;
        private LevelSpawner spawner;
        private PropCatalog catalog;
        private LevelDef level;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
            bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            spawner = Object.FindAnyObjectByType<LevelSpawner>();
            catalog = AssetDatabase.LoadAssetAtPath<PropCatalog>(CatalogPath);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (level != null) Object.Destroy(level);
            yield return null;
        }

        private void Load(params PropPlacement[] props)
        {
            level = ScriptableObject.CreateInstance<LevelDef>();
            level.id = "Fizzy";
            level.props = props;
            spawner.Load(level, catalog, bootstrap.Tuning, null, bootstrap.Events);
        }

        private static PropPlacement At(PropKind kind, float x, float z) =>
            new PropPlacement { kind = kind, basePosition = new Vector3(x, 0f, z), isTarget = true };

        private static void TipOver(DeskObject prop)
        {
            var rotation = Quaternion.Euler(0f, 0f, 90f);
            prop.transform.rotation = rotation;
            prop.Body.rotation = rotation;
        }

        private float FuseSeconds => bootstrap.Tuning.fizzFuseSeconds;

        private static IEnumerator Seconds(float seconds)
        {
            int steps = Mathf.RoundToInt(seconds / Time.fixedDeltaTime);
            for (int i = 0; i < steps; i++) yield return new WaitForFixedUpdate();
        }

        [UnityTest]
        public IEnumerator FizzySoda_SpawnedUpright_DoesNotBurst()
        {
            Load(At(PropKind.FizzySoda, 0f, 0f), At(PropKind.SodaCan, 0.3f, 0f));
            var fizzy = spawner.Spawned[0];

            yield return Seconds(3f);

            Assert.IsFalse(fizzy.IsBroken, "an untouched fizzy must never burst");
            Assert.IsTrue(fizzy.gameObject.activeSelf);
            Assert.AreEqual(2, spawner.TargetsRemaining);
        }

        [UnityTest]
        public IEnumerator FizzySoda_KnockedOver_BurstsAndClears()
        {
            Load(At(PropKind.FizzySoda, 0f, 0f));
            var fizzy = spawner.Spawned[0];
            yield return new WaitForFixedUpdate();

            TipOver(fizzy);
            yield return Seconds(FuseSeconds + 0.2f);

            Assert.IsTrue(fizzy.IsBroken, "it should burst once its fuse burns");
            Assert.IsTrue(fizzy.IsCleared, "a burst counts as cleared");
            Assert.AreEqual(0, spawner.TargetsRemaining);
        }

        [UnityTest]
        public IEnumerator FizzySoda_Burst_PushesANeighbour()
        {
            Load(At(PropKind.FizzySoda, 0f, 0f), At(PropKind.Keycap1u, 0.3f, 0f));
            var keycap = spawner.Spawned[1];
            yield return Seconds(0.5f);
            Assert.Less(keycap.Body.linearVelocity.magnitude, 0.05f, "the keycap should be resting first");

            TipOver(spawner.Spawned[0]);
            float fastest = 0f;
            for (int i = 0; i < Mathf.RoundToInt((FuseSeconds + 0.3f) / Time.fixedDeltaTime); i++)
            {
                yield return new WaitForFixedUpdate();
                fastest = Mathf.Max(fastest, keycap.Body.linearVelocity.magnitude);
            }

            Assert.Greater(fastest, 1f, "the burst should shove the keycap");
        }

        [UnityTest]
        public IEnumerator FizzySoda_Burst_ChainsToAnother()
        {
            Load(At(PropKind.FizzySoda, 0f, 0f), At(PropKind.FizzySoda, 0.4f, 0f));
            var second = spawner.Spawned[1];
            yield return Seconds(0.5f);

            TipOver(spawner.Spawned[0]);
            yield return Seconds(2f * FuseSeconds + 1f);

            Assert.IsTrue(second.IsBroken, "a fizzy in range should be set off too");
            Assert.AreEqual(0, spawner.TargetsRemaining);
        }

        /// <summary>A crowded fortress (many colliders in range) must not crowd the second fizzy out of the burst.</summary>
        [UnityTest]
        public IEnumerator FizzySoda_Burst_ChainsThroughACrowd()
        {
            var props = new System.Collections.Generic.List<PropPlacement> { At(PropKind.FizzySoda, 0f, 0f), At(PropKind.FizzySoda, 0.45f, 0f) };
            for (int i = 0; i < 12; i++)
                for (int level = 0; level < 3; level++)
                {
                    var spot = Quaternion.Euler(0f, i * 30f, 0f) * new Vector3(0f, 0f, 0.3f);
                    if (Mathf.Abs(spot.x - 0.45f) < 0.2f && Mathf.Abs(spot.z) < 0.2f) continue;
                    props.Add(new PropPlacement { kind = PropKind.Keycap1u, basePosition = new Vector3(spot.x, level * 0.15f, spot.z), isTarget = true });
                }
            Load(props.ToArray());
            var second = spawner.Spawned[1];
            yield return Seconds(1f);

            TipOver(spawner.Spawned[0]);
            yield return Seconds(2f * FuseSeconds + 1f);

            Assert.IsTrue(second.IsBroken, $"the second fizzy was crowded out ({props.Count} props in range)");
        }

        [UnityTest]
        public IEnumerator Restart_DuringFuse_NoBurstInNewRound()
        {
            Load(At(PropKind.FizzySoda, 0f, 0f), At(PropKind.Keycap1u, 0.3f, 0f));
            yield return new WaitForFixedUpdate();
            TipOver(spawner.Spawned[0]);
            yield return new WaitForFixedUpdate();
            Assert.IsTrue(spawner.Spawned[0].GetComponent<FizzFuse>().IsArmed, "the fuse should be burning");

            var restart = level;
            level = null;
            Load(At(PropKind.FizzySoda, 0f, 0f), At(PropKind.Keycap1u, 0.3f, 0f));
            Object.Destroy(restart);
            var keycap = spawner.Spawned[1];
            yield return Seconds(FuseSeconds + 1f);

            Assert.IsFalse(spawner.Spawned[0].IsBroken, "the new round's fizzy was never touched");
            Assert.IsFalse(keycap.IsCleared, "nothing from the old fuse may reach the new round");
            Assert.Less(keycap.Body.linearVelocity.magnitude, 0.05f);
            Assert.AreEqual(2, spawner.TargetsRemaining);
        }
    }
}
