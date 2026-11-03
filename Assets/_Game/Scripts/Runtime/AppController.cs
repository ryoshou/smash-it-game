using Game.Core;
using Game.Juice;
using Game.Meta;
using Game.Monetization;
using Game.UI;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Owns the app around the round (spec §7, §8, §10): screen flow, the save, settings, the result panel, ads and
    /// Remove Ads. GameBootstrap owns the round itself and only reports how it ended.
    /// </summary>
    public sealed class AppController : MonoBehaviour
    {
        [SerializeField] private GameBootstrap bootstrap;
        [SerializeField] private AppConfig config;
        [SerializeField] private UiStrings strings;
        [SerializeField] private Hud hud;
        [SerializeField] private ResultPanel resultPanel;
        [SerializeField] private SplashScreen splash;
        [SerializeField] private HomeScreen home;
        [SerializeField] private PausePanel pause;
        [SerializeField] private SfxPlayer sfx;
        [SerializeField] private MusicPlayer music;
        [Tooltip("An IAdService.")]
        [SerializeField] private MonoBehaviour adService;
        [Tooltip("An IPurchaseService.")]
        [SerializeField] private MonoBehaviour purchaseService;

        private IAdService ads;
        private IPurchaseService store;
        private Profile profile;
        private AppFlow flow;
        private AdPacing pacing;
        private float splashEndsAt;

        public AppScreen Screen => flow.Screen;

        /// <summary>Debug panel: wipe the save and go back to a fresh home screen.</summary>
        public void ResetSave()
        {
            profile.ResetAll();
            ApplySettings();
            bootstrap.LoadLevel(profile.Data.levelIndex);
            flow.GoHome();
            if (flow.Screen == AppScreen.Home) ShowHome();
        }

        private void Awake()
        {
            ads = adService as IAdService;
            store = purchaseService as IPurchaseService;
            if (ads == null) Debug.LogError($"{nameof(AppController)}.{nameof(adService)} is not an {nameof(IAdService)}.", this);
            if (store == null) Debug.LogError($"{nameof(AppController)}.{nameof(purchaseService)} is not an {nameof(IPurchaseService)}.", this);

            profile = new Profile(new PlayerPrefsSaveStore());
            flow = new AppFlow(!profile.Data.firstRunDone);
            pacing = new AdPacing(config, Time.unscaledTime);
            flow.Changed += OnScreenChanged;

            hud.Bind(strings, () => flow.Pause());
            resultPanel.Bind(strings);
            home.Bind(strings);
            pause.Bind(strings, SetSound, SetMusic, SetHaptics, OnResumeTapped, Restart, Restore,
                string.IsNullOrEmpty(config.privacyPolicyUrl) ? null : OpenPrivacyPolicy);
            bootstrap.RoundFinished += OnRoundFinished;
            ApplySettings();
        }

        private void Start()
        {
            int editorLevel = EditorStartLevel.Consume(-1);
            bootstrap.InputEnabled = false;
            hud.gameObject.SetActive(false);
            bootstrap.LoadLevel(editorLevel >= 0 ? editorLevel : profile.Data.levelIndex);
            splash.Show(strings.title);
            splashEndsAt = Time.unscaledTime + config.splashSeconds;
            if (editorLevel < 0) return;
            // The level editor's Preview: straight into play.
            flow.SplashDone();
            flow.Play();
        }

        private void OnDestroy()
        {
            if (bootstrap != null) bootstrap.RoundFinished -= OnRoundFinished;
            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (flow.Screen == AppScreen.Splash && Time.unscaledTime >= splashEndsAt) flow.SplashDone();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) flow.Pause();
        }

        private void OnScreenChanged(AppScreen screen)
        {
            switch (screen)
            {
                case AppScreen.Consent:
                    // M4 plugs the consent SDK in here; M3 tracks nothing, so there is nothing to ask.
                    profile.MarkFirstRunDone();
                    flow.ConsentDone();
                    break;
                case AppScreen.Home:
                    splash.Hide();
                    pause.Hide();
                    resultPanel.Hide();
                    hud.gameObject.SetActive(false);
                    SetPlaying(false);
                    if (bootstrap.LevelIndex != profile.Data.levelIndex) bootstrap.LoadLevel(profile.Data.levelIndex);
                    ShowHome();
                    break;
                case AppScreen.Playing:
                    splash.Hide();
                    home.Hide();
                    pause.Hide();
                    hud.gameObject.SetActive(true);
                    SetPlaying(true);
                    break;
                case AppScreen.Paused:
                    Time.timeScale = 0f;
                    bootstrap.InputEnabled = false;
                    pause.Show(true, profile.Data);
                    break;
            }
        }

        private void SetPlaying(bool playing)
        {
            Time.timeScale = 1f;
            bootstrap.InputEnabled = playing;
        }

        private void ShowHome() =>
            home.Show(profile.Data.levelIndex + 1, !profile.Data.removeAds, store?.RemoveAdsPrice,
                () => flow.Play(), BuyRemoveAds, () => pause.Show(false, profile.Data));

        private void OnResumeTapped()
        {
            if (!flow.Resume()) pause.Hide(); // settings opened from home
        }

        private void Restart()
        {
            flow.Resume();
            resultPanel.Hide();
            bootstrap.Retry();
        }

        private void OnRoundFinished(RoundState state)
        {
            if (state == RoundState.Won)
            {
                resultPanel.ShowWin(() => { resultPanel.Hide(); bootstrap.Next(); });
                return;
            }
            resultPanel.ShowLose(bootstrap.Budget.CanContinue,
                () => { resultPanel.Hide(); bootstrap.Continue(); },
                () => { resultPanel.Hide(); bootstrap.Retry(); });
        }

        private void BuyRemoveAds() => store?.BuyRemoveAds(purchased =>
        {
            if (!purchased) return;
            profile.SetRemoveAds(true);
            home.SetRemoveAdsVisible(false);
        });

        private void Restore() => store?.RestoreRemoveAds(owned =>
        {
            if (owned)
            {
                profile.SetRemoveAds(true);
                home.SetRemoveAdsVisible(false);
            }
            pause.ShowMessage(owned ? strings.restored : strings.nothingToRestore);
        });

        private void OpenPrivacyPolicy() => Application.OpenURL(config.privacyPolicyUrl);

        private void SetSound(bool on)
        {
            profile.SetSound(on);
            ApplySettings();
        }

        private void SetMusic(bool on)
        {
            profile.SetMusic(on);
            ApplySettings();
        }

        private void SetHaptics(bool on)
        {
            profile.SetHaptics(on);
            ApplySettings();
        }

        private void ApplySettings()
        {
            sfx.Muted = !profile.Data.sound;
            music.Muted = !profile.Data.music;
            HapticsBridge.Enabled = profile.Data.haptics;
        }
    }
}
