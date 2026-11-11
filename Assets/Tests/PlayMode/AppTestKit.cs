using System.Collections;
using System.Linq;
using Game.Meta;
using Game.Monetization;
using Game.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>Launches the real Game scene on a chosen save and drives its UI like a player would.</summary>
    internal static class AppTestKit
    {
        private const float WaitSeconds = 8f;

        public static void ClearSave()
        {
            Time.timeScale = 1f;
            PlayerPrefs.DeleteKey(PlayerPrefsSaveStore.Key);
            PlayerPrefs.DeleteKey(SimulatedPurchaseService.OwnershipKey);
        }

        public static SaveData Saved => new Profile(new PlayerPrefsSaveStore()).Data;

        /// <summary>Writes the save, loads the scene and waits for the home screen.</summary>
        public static IEnumerator Launch(int savedLevel = 0, bool removeAds = false)
        {
            var profile = new Profile(new PlayerPrefsSaveStore());
            profile.RecordWin(savedLevel - 1);
            profile.SetRemoveAds(removeAds);
            yield return SceneManager.LoadSceneAsync("Game");
            yield return WaitFor(() => App.Screen == AppScreen.Home);
        }

        public static IEnumerator LaunchAndPlay(int savedLevel = 0, bool removeAds = false)
        {
            yield return Launch(savedLevel, removeAds);
            ButtonNamed<Game.UI.HomeScreen>("Play").onClick.Invoke();
            yield return null;
        }

        public static AppController App => Object.FindAnyObjectByType<AppController>();
        public static GameBootstrap Bootstrap => Object.FindAnyObjectByType<GameBootstrap>();
        public static SimulatedAdService Ads => Object.FindAnyObjectByType<SimulatedAdService>();

        public static Button ButtonNamed<T>(string name) where T : Component =>
            Object.FindAnyObjectByType<T>(FindObjectsInactive.Include).GetComponentsInChildren<Button>(true).First(b => b.name == name);

        public static bool ShowsText<T>(string text) where T : Component =>
            Object.FindAnyObjectByType<T>(FindObjectsInactive.Include).GetComponentsInChildren<Text>().Any(t => t.text == text);

        public static bool IsShown<T>() where T : Component =>
            Object.FindAnyObjectByType<T>(FindObjectsInactive.Include).gameObject.activeInHierarchy;

        /// <summary>Drops every prop below the desk, then waits for the round to end Won and the win panel to open.</summary>
        public static IEnumerator Win()
        {
            foreach (var prop in Object.FindAnyObjectByType<LevelSpawner>().Spawned)
            {
                prop.Body.isKinematic = true;
                prop.Body.position = new Vector3(prop.Body.position.x, -1f, prop.Body.position.z);
            }
            yield return WaitFor(() => Bootstrap.State == Game.Core.RoundState.Won);
            yield return WaitFor(IsShown<Game.UI.ResultPanel>);
            yield return null;
        }

        /// <summary>Spends every shot without firing, then waits for the round to end Lost.</summary>
        public static IEnumerator Lose()
        {
            while (Bootstrap.Budget.TryConsume()) { }
            yield return WaitFor(() => Bootstrap.State == Game.Core.RoundState.Lost);
            yield return null;
        }

        public static IEnumerator WaitFor(System.Func<bool> condition)
        {
            float deadline = Time.realtimeSinceStartup + WaitSeconds;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue(condition(), "timed out waiting");
        }
    }
}
