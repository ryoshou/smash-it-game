using Game.Juice;
using Game.Meta;
using Game.Monetization;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Editor
{
    /// <summary>
    /// Splash, home, room map and pause/settings screens on the main canvas, the AppController, and the debug meta buttons.
    /// </summary>
    internal static class MetaSceneParts
    {
        private static readonly Color SplashColor = new Color(0.98f, 0.89f, 0.84f, 1f);
        private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color BarBack = new Color(0.3f, 0.22f, 0.3f, 0.18f);
        private const float HomeBadgeWidth = 440f;
        private const float HomeBadgeHeight = 110f;
        private const float HomeGap = 24f;
        private const float NoAdsWidth = 300f;
        private const float NoAdsHeight = 96f;
        private const float TitleTilt = 4f;
        private const int RoomCards = 5;

        public static AppController Build(UiSceneParts.Parts ui, MonetizationSceneParts.Parts money, GameBootstrap bootstrap,
            SfxPlayer sfx, MusicPlayer music, AppConfig config, UiStrings strings)
        {
            var home = BuildHome(ui.Safe);
            var roomMap = BuildRoomMap(ui.Canvas);
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
            PrototypeSceneBuilder.SetReference(app, "roomMap", roomMap);
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
            var title = UiSceneParts.Label("Title", root, "Smash It", 170, TextAnchor.MiddleCenter);
            UiSceneParts.Place(title.rectTransform, new Vector2(0f, 0.45f), new Vector2(1f, 0.6f), Vector2.zero, Vector2.zero);
            UiStyle.Title(title);
            var component = root.gameObject.AddComponent<SplashScreen>();
            PrototypeSceneBuilder.SetReference(component, "titleText", title);
            return component;
        }

        /// <summary>
        /// No backdrop: the desk and the cat show through. The owner's layout (2026-11-12): level badge top left, the
        /// settings gear top right with No Ads under it, the title, Play and Rooms (centred under it) on the floor
        /// between the desk and the slingshot, where the thumb is, so the cat and the slingshot stay in view.
        /// </summary>
        private static HomeScreen BuildHome(Transform safe)
        {
            // Inset from the safe area so the corner buttons clear the screen's rounded corners.
            var root = UiSceneParts.Rect("Home", safe, Vector2.zero, Vector2.one, new Vector2(12f, 12f), new Vector2(-12f, -16f));
            var topLeft = new Vector2(0f, 1f);
            var topRight = Vector2.one;
            var level = UiStyle.Badge(UiSceneParts.Rect("Level", root, topLeft, topLeft, new Vector2(0f, -HomeBadgeHeight),
                new Vector2(HomeBadgeWidth, 0f)), CandyLook.Purple, 54);
            var settings = UiStyle.IconButton(UiSceneParts.Rect("Settings", root, topRight, topRight,
                -UiStyle.IconButtonSize, Vector2.zero), CandyLook.Yellow, UiSprites.IconGear);
            float noAdsTop = -UiStyle.IconButtonSize.y - HomeGap;
            var (removeAds, removeAdsLabel) = UiStyle.Candy(UiSceneParts.Rect("Remove Ads", root, topRight, topRight,
                new Vector2(-NoAdsWidth, noAdsTop - NoAdsHeight), new Vector2(0f, noAdsTop)), CandyLook.Red, 40);

            var title = UiSceneParts.Label("Title", root, "Smash It", 170, TextAnchor.MiddleCenter);
            UiSceneParts.Place(title.rectTransform, new Vector2(0f, 0.74f), new Vector2(1f, 0.86f), Vector2.zero, Vector2.zero);
            title.rectTransform.localEulerAngles = new Vector3(0f, 0f, TitleTilt);
            UiStyle.Title(title);
            var subtitle = UiSceneParts.Label("Subtitle", root, "", 72, TextAnchor.MiddleCenter);
            UiSceneParts.Place(subtitle.rectTransform, new Vector2(0f, 0.7f), new Vector2(1f, 0.75f), Vector2.zero, Vector2.zero);
            subtitle.rectTransform.localEulerAngles = new Vector3(0f, 0f, TitleTilt);
            UiStyle.Title(subtitle);
            subtitle.color = Color.white;

            var (play, playLabel) = UiSceneParts.Button("Play", root, new Vector2(0.14f, 0.43f), new Vector2(0.86f, 0.51f), 84);
            var (rooms, roomsLabel) = UiSceneParts.Button("Rooms", root, new Vector2(0.3f, 0.355f), new Vector2(0.7f, 0.415f), 56,
                CandyLook.Blue);
            UiStyle.LeadingIcon(roomsLabel, UiSprites.IconMap);

            var component = root.gameObject.AddComponent<HomeScreen>();
            PrototypeSceneBuilder.SetReference(component, "titleText", title);
            PrototypeSceneBuilder.SetReference(component, "subtitleText", subtitle);
            PrototypeSceneBuilder.SetReference(component, "levelText", level);
            PrototypeSceneBuilder.SetReference(component, "playButton", play);
            PrototypeSceneBuilder.SetReference(component, "playLabel", playLabel);
            PrototypeSceneBuilder.SetReference(component, "removeAdsButton", removeAds);
            PrototypeSceneBuilder.SetReference(component, "removeAdsLabel", removeAdsLabel);
            PrototypeSceneBuilder.SetReference(component, "settingsButton", settings);
            PrototypeSceneBuilder.SetReference(component, "roomsButton", rooms);
            PrototypeSceneBuilder.SetReference(component, "roomsLabel", roomsLabel);
            root.gameObject.SetActive(false);
            return component;
        }

        private static PausePanel BuildPause(Transform canvas)
        {
            var dim = UiSceneParts.Rect("Pause", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            dim.gameObject.AddComponent<Image>().color = DimColor; // blocks taps on the game below
            var panel = UiSceneParts.Rect("Panel", dim, new Vector2(0.08f, 0.17f), new Vector2(0.92f, 0.83f), Vector2.zero, Vector2.zero);
            var title = UiStyle.RibbonPanel(panel, "Paused");
            var pop = dim.gameObject.AddComponent<PopIn>();
            PrototypeSceneBuilder.SetReference(pop, "target", panel);

            var (sound, soundLabel) = Toggle("Sound", panel, 0.86f);
            var (music, musicLabel) = Toggle("Music", panel, 0.77f);
            var (haptics, hapticsLabel) = Toggle("Vibration", panel, 0.68f);
            var (resume, resumeLabel) = UiSceneParts.Button("Resume", panel, new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.59f));
            var (restart, restartLabel) = UiSceneParts.Button("Restart", panel, new Vector2(0.1f, 0.39f), new Vector2(0.9f, 0.47f), 56,
                CandyLook.Blue);
            var (home, homeLabel) = UiSceneParts.Button("Home", panel, new Vector2(0.1f, 0.28f), new Vector2(0.9f, 0.36f), 56,
                CandyLook.Blue);
            var (restore, restoreLabel) = UiSceneParts.Button("Restore", panel, new Vector2(0.1f, 0.18f), new Vector2(0.9f, 0.25f), 44,
                CandyLook.Blue);
            var (privacy, privacyLabel) = UiSceneParts.Button("Privacy", panel, new Vector2(0.1f, 0.09f), new Vector2(0.9f, 0.16f), 40,
                CandyLook.Blue);
            var message = UiSceneParts.Label("Message", panel, "", 40, TextAnchor.MiddleCenter);
            UiSceneParts.Place(message.rectTransform, new Vector2(0f, 0.01f), new Vector2(1f, 0.08f), Vector2.zero, Vector2.zero);

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
            PrototypeSceneBuilder.SetReference(component, "homeButton", home);
            PrototypeSceneBuilder.SetReference(component, "homeLabel", homeLabel);
            PrototypeSceneBuilder.SetReference(component, "restoreButton", restore);
            PrototypeSceneBuilder.SetReference(component, "restoreLabel", restoreLabel);
            PrototypeSceneBuilder.SetReference(component, "privacyButton", privacy);
            PrototypeSceneBuilder.SetReference(component, "privacyLabel", privacyLabel);
            PrototypeSceneBuilder.SetReference(component, "messageText", message);
            dim.gameObject.SetActive(false);
            return component;
        }

        /// <summary>A dimmed full-screen card list: one card per room (name, levels, progress bar, status) and Back.</summary>
        private static RoomMapPanel BuildRoomMap(Transform canvas)
        {
            var dim = UiSceneParts.Rect("Room Map", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            dim.gameObject.AddComponent<Image>().color = DimColor; // blocks taps on the game below
            var panel = UiSceneParts.Rect("Panel", dim, new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.9f), Vector2.zero, Vector2.zero);
            var title = UiStyle.RibbonPanel(panel, "Rooms");
            var pop = dim.gameObject.AddComponent<PopIn>();
            PrototypeSceneBuilder.SetReference(pop, "target", panel);

            var cards = new Button[RoomCards];
            var images = new Image[RoomCards];
            var names = new Text[RoomCards];
            var details = new Text[RoomCards];
            var statuses = new Text[RoomCards];
            var fills = new RectTransform[RoomCards];
            var groups = new CanvasGroup[RoomCards];
            const float top = 0.87f, height = 0.135f, gap = 0.012f;
            for (int i = 0; i < RoomCards; i++)
            {
                float cardTop = top - i * (height + gap);
                var card = UiSceneParts.Rect($"Room {i + 1}", panel, new Vector2(0.05f, cardTop - height), new Vector2(0.95f, cardTop),
                    Vector2.zero, Vector2.zero);
                var face = UiStyle.Card(card);
                // A stripe down the card's left edge in the room's own colour (RoomMapPanel tints it).
                var stripe = UiSceneParts.Rect("Stripe", card, Vector2.zero, new Vector2(0f, 1f), new Vector2(14f, 18f), new Vector2(34f, -14f));
                images[i] = UiStyle.Fill(stripe, Color.white);
                images[i].pixelsPerUnitMultiplier = 4f;
                images[i].raycastTarget = false;
                groups[i] = card.gameObject.AddComponent<CanvasGroup>();
                cards[i] = card.gameObject.AddComponent<Button>();
                cards[i].targetGraphic = face;

                names[i] = UiSceneParts.Label("Name", card, "Room", 58, TextAnchor.MiddleLeft);
                UiSceneParts.Place(names[i].rectTransform, new Vector2(0.05f, 0.52f), new Vector2(0.68f, 0.95f), Vector2.zero, Vector2.zero);
                details[i] = UiSceneParts.Label("Detail", card, "", 34, TextAnchor.MiddleLeft);
                UiSceneParts.Place(details[i].rectTransform, new Vector2(0.05f, 0.3f), new Vector2(0.68f, 0.54f), Vector2.zero, Vector2.zero);
                details[i].resizeTextForBestFit = true;
                details[i].resizeTextMinSize = 22;
                details[i].resizeTextMaxSize = 34;
                var bar = UiSceneParts.Rect("Bar", card, new Vector2(0.05f, 0.1f), new Vector2(0.66f, 0.24f), Vector2.zero, Vector2.zero);
                UiStyle.Fill(bar, BarBack).pixelsPerUnitMultiplier = 3f;
                fills[i] = UiSceneParts.Rect("Fill", bar, Vector2.zero, new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
                UiStyle.Fill(fills[i], CandyLook.Orange.Top).pixelsPerUnitMultiplier = 3f;

                var status = UiSceneParts.Rect("Status", card, new Vector2(0.71f, 0.24f), new Vector2(0.95f, 0.76f), Vector2.zero, Vector2.zero);
                statuses[i] = UiStyle.Badge(status, CandyLook.Green, 44);
            }
            var (back, backLabel) = UiSceneParts.Button("Back", panel, new Vector2(0.25f, 0.03f), new Vector2(0.75f, 0.11f), 56,
                CandyLook.Blue);

            var component = dim.gameObject.AddComponent<RoomMapPanel>();
            PrototypeSceneBuilder.SetReference(component, "titleText", title);
            PrototypeSceneBuilder.SetReference(component, "backButton", back);
            PrototypeSceneBuilder.SetReference(component, "backLabel", backLabel);
            UiSceneParts.SetArray(component, "cards", cards);
            UiSceneParts.SetArray(component, "cardImages", images);
            UiSceneParts.SetArray(component, "nameTexts", names);
            UiSceneParts.SetArray(component, "detailTexts", details);
            UiSceneParts.SetArray(component, "statusTexts", statuses);
            UiSceneParts.SetArray(component, "fills", fills);
            UiSceneParts.SetArray(component, "cardGroups", groups);
            dim.gameObject.SetActive(false);
            return component;
        }

        /// <summary>A label on the left and an on/off switch on the right; the whole row is the tap target.</summary>
        private static (Toggle, Text) Toggle(string name, Transform panel, float top)
        {
            var row = UiSceneParts.Rect(name, panel, new Vector2(0.1f, top - 0.08f), new Vector2(0.9f, top), Vector2.zero, Vector2.zero);
            row.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0f); // invisible hit area
            var label = UiSceneParts.Label("Label", row, name, 60, TextAnchor.MiddleLeft);
            UiSceneParts.Place(label.rectTransform, Vector2.zero, new Vector2(0.7f, 1f), Vector2.zero, Vector2.zero);
            return (UiStyle.Switch(row), label);
        }
    }
}
