using Game.Juice;
using Game.Meta;
using Game.Monetization;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Editor
{
    /// <summary>Splash, home and pause/settings screens on the main canvas, the AppController, and the debug meta buttons.</summary>
    internal static class MetaSceneParts
    {
        private static readonly Color SplashColor = new Color(0.98f, 0.89f, 0.84f, 1f);
        private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color PanelColor = new Color(1f, 0.97f, 0.95f, 1f);
        private static readonly Color BoxColor = new Color(1f, 0.72f, 0.62f);
        private static readonly Color Ink = new Color(0.3f, 0.22f, 0.3f);

        public static AppController Build(UiSceneParts.Parts ui, MonetizationSceneParts.Parts money, GameBootstrap bootstrap,
            SfxPlayer sfx, MusicPlayer music, AppConfig config, UiStrings strings)
        {
            var home = BuildHome(ui.Canvas);
            var pause = BuildPause(ui.Canvas);
            var splash = BuildSplash(ui.Canvas);

            var app = bootstrap.gameObject.AddComponent<AppController>();
            PrototypeSceneBuilder.SetReference(app, "bootstrap", bootstrap);
            PrototypeSceneBuilder.SetReference(app, "config", config);
            PrototypeSceneBuilder.SetReference(app, "strings", strings);
            PrototypeSceneBuilder.SetReference(app, "hud", ui.Hud);
            PrototypeSceneBuilder.SetReference(app, "resultPanel", ui.Result);
            PrototypeSceneBuilder.SetReference(app, "splash", splash);
            PrototypeSceneBuilder.SetReference(app, "home", home);
            PrototypeSceneBuilder.SetReference(app, "pause", pause);
            PrototypeSceneBuilder.SetReference(app, "sfx", sfx);
            PrototypeSceneBuilder.SetReference(app, "music", music);
            PrototypeSceneBuilder.SetReference(app, "adService", money.Ads);
            PrototypeSceneBuilder.SetReference(app, "purchaseService", money.Store);
            return app;
        }

        /// <summary>Three buttons under the hidden tuning panel, shown and hidden with it.</summary>
        public static void BuildDebugButtons(Transform debugPanel, AppController app, SimulatedAdService ads)
        {
            var (reset, _) = UiSceneParts.Button("Reset save", debugPanel, new Vector2(0f, -0.16f), new Vector2(0.48f, -0.03f), 40);
            var (offline, offlineLabel) = UiSceneParts.Button("Ads offline", debugPanel, new Vector2(0.52f, -0.16f), new Vector2(1f, -0.03f), 40);
            var (nextRoom, _) = UiSceneParts.Button("Next room", debugPanel, new Vector2(0f, -0.30f), new Vector2(0.48f, -0.17f), 40);
            var component = debugPanel.gameObject.AddComponent<DebugMetaButtons>();
            PrototypeSceneBuilder.SetReference(component, "app", app);
            PrototypeSceneBuilder.SetReference(component, "ads", ads);
            PrototypeSceneBuilder.SetReference(component, "resetSaveButton", reset);
            PrototypeSceneBuilder.SetReference(component, "adsOfflineButton", offline);
            PrototypeSceneBuilder.SetReference(component, "adsOfflineLabel", offlineLabel);
            PrototypeSceneBuilder.SetReference(component, "nextRoomButton", nextRoom);
        }

        private static SplashScreen BuildSplash(Transform canvas)
        {
            var root = UiSceneParts.Rect("Splash", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            root.gameObject.AddComponent<Image>().color = SplashColor;
            var title = UiSceneParts.Label("Title", root, "Smash It", 140, TextAnchor.MiddleCenter);
            UiSceneParts.Place(title.rectTransform, new Vector2(0f, 0.45f), new Vector2(1f, 0.6f), Vector2.zero, Vector2.zero);
            var component = root.gameObject.AddComponent<SplashScreen>();
            PrototypeSceneBuilder.SetReference(component, "titleText", title);
            return component;
        }

        /// <summary>No backdrop: the desk and the cat show through. Play sits low, where the thumb is.</summary>
        private static HomeScreen BuildHome(Transform canvas)
        {
            var root = UiSceneParts.Rect("Home", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var title = UiSceneParts.Label("Title", root, "Smash It", 130, TextAnchor.MiddleCenter);
            UiSceneParts.Place(title.rectTransform, new Vector2(0f, 0.8f), new Vector2(1f, 0.9f), Vector2.zero, Vector2.zero);
            var (play, playLabel) = UiSceneParts.Button("Play", root, new Vector2(0.15f, 0.2f), new Vector2(0.85f, 0.29f), 72);
            var (removeAds, removeAdsLabel) = UiSceneParts.Button("Remove Ads", root, new Vector2(0.25f, 0.11f), new Vector2(0.75f, 0.17f), 44);
            var (settings, settingsLabel) = UiSceneParts.Button("Settings", root, new Vector2(0.62f, 0.93f), new Vector2(0.96f, 0.98f), 44);

            var component = root.gameObject.AddComponent<HomeScreen>();
            PrototypeSceneBuilder.SetReference(component, "titleText", title);
            PrototypeSceneBuilder.SetReference(component, "playButton", play);
            PrototypeSceneBuilder.SetReference(component, "playLabel", playLabel);
            PrototypeSceneBuilder.SetReference(component, "removeAdsButton", removeAds);
            PrototypeSceneBuilder.SetReference(component, "removeAdsLabel", removeAdsLabel);
            PrototypeSceneBuilder.SetReference(component, "settingsButton", settings);
            PrototypeSceneBuilder.SetReference(component, "settingsLabel", settingsLabel);
            root.gameObject.SetActive(false);
            return component;
        }

        private static PausePanel BuildPause(Transform canvas)
        {
            var dim = UiSceneParts.Rect("Pause", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            dim.gameObject.AddComponent<Image>().color = DimColor; // blocks taps on the game below
            var panel = UiSceneParts.Rect("Panel", dim, new Vector2(0.08f, 0.17f), new Vector2(0.92f, 0.83f), Vector2.zero, Vector2.zero);
            panel.gameObject.AddComponent<Image>().color = PanelColor;
            var title = UiSceneParts.Label("Title", panel, "Paused", 84, TextAnchor.MiddleCenter);
            UiSceneParts.Place(title.rectTransform, new Vector2(0f, 0.89f), new Vector2(1f, 0.98f), Vector2.zero, Vector2.zero);

            var (sound, soundLabel) = Toggle("Sound", panel, 0.86f);
            var (music, musicLabel) = Toggle("Music", panel, 0.77f);
            var (haptics, hapticsLabel) = Toggle("Vibration", panel, 0.68f);
            var (resume, resumeLabel) = UiSceneParts.Button("Resume", panel, new Vector2(0.1f, 0.47f), new Vector2(0.9f, 0.57f));
            var (restart, restartLabel) = UiSceneParts.Button("Restart", panel, new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.44f), 56);
            var (restore, restoreLabel) = UiSceneParts.Button("Restore", panel, new Vector2(0.1f, 0.23f), new Vector2(0.9f, 0.32f), 48);
            var (privacy, privacyLabel) = UiSceneParts.Button("Privacy", panel, new Vector2(0.1f, 0.12f), new Vector2(0.9f, 0.2f), 44);
            var message = UiSceneParts.Label("Message", panel, "", 40, TextAnchor.MiddleCenter);
            UiSceneParts.Place(message.rectTransform, new Vector2(0f, 0.02f), new Vector2(1f, 0.1f), Vector2.zero, Vector2.zero);

            var component = dim.gameObject.AddComponent<PausePanel>();
            PrototypeSceneBuilder.SetReference(component, "titleText", title);
            PrototypeSceneBuilder.SetReference(component, "soundToggle", sound);
            PrototypeSceneBuilder.SetReference(component, "soundLabel", soundLabel);
            PrototypeSceneBuilder.SetReference(component, "musicToggle", music);
            PrototypeSceneBuilder.SetReference(component, "musicLabel", musicLabel);
            PrototypeSceneBuilder.SetReference(component, "hapticsToggle", haptics);
            PrototypeSceneBuilder.SetReference(component, "hapticsLabel", hapticsLabel);
            PrototypeSceneBuilder.SetReference(component, "resumeButton", resume);
            PrototypeSceneBuilder.SetReference(component, "resumeLabel", resumeLabel);
            PrototypeSceneBuilder.SetReference(component, "restartButton", restart);
            PrototypeSceneBuilder.SetReference(component, "restartLabel", restartLabel);
            PrototypeSceneBuilder.SetReference(component, "restoreButton", restore);
            PrototypeSceneBuilder.SetReference(component, "restoreLabel", restoreLabel);
            PrototypeSceneBuilder.SetReference(component, "privacyButton", privacy);
            PrototypeSceneBuilder.SetReference(component, "privacyLabel", privacyLabel);
            PrototypeSceneBuilder.SetReference(component, "messageText", message);
            dim.gameObject.SetActive(false);
            return component;
        }

        /// <summary>A label on the left and a big check box on the right; the whole row is the tap target.</summary>
        private static (Toggle, Text) Toggle(string name, Transform panel, float top)
        {
            var row = UiSceneParts.Rect(name, panel, new Vector2(0.1f, top - 0.08f), new Vector2(0.9f, top), Vector2.zero, Vector2.zero);
            row.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0f); // invisible hit area
            var label = UiSceneParts.Label("Label", row, name, 60, TextAnchor.MiddleLeft);
            UiSceneParts.Place(label.rectTransform, Vector2.zero, new Vector2(0.75f, 1f), Vector2.zero, Vector2.zero);
            var box = UiSceneParts.Rect("Box", row, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-100f, -45f), new Vector2(-10f, 45f));
            var boxImage = box.gameObject.AddComponent<Image>();
            boxImage.color = BoxColor;
            var check = UiSceneParts.Rect("Check", box, Vector2.zero, Vector2.one, new Vector2(18f, 18f), new Vector2(-18f, -18f));
            var checkImage = check.gameObject.AddComponent<Image>();
            checkImage.color = Ink;
            var toggle = row.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = boxImage;
            toggle.graphic = checkImage;
            toggle.isOn = true;
            return (toggle, label);
        }
    }
}
