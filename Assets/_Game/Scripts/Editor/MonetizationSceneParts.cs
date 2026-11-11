using Game.Meta;
using Game.Monetization;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Editor
{
    /// <summary>The simulated ad overlay and purchase dialog, on their own canvas above every other UI.</summary>
    internal static class MonetizationSceneParts
    {
        private const int OverlaySortingOrder = 100;
        private static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);
        private static readonly Color AdBackdrop = new Color(0.12f, 0.1f, 0.14f, 1f);
        private static readonly Color AdInk = new Color(1f, 0.97f, 0.95f);
        private static readonly Color DimColor = new Color(0f, 0f, 0f, 0.6f);

        public readonly struct Parts
        {
            public readonly SimulatedAdService Ads;
            public readonly SimulatedPurchaseService Store;

            public Parts(SimulatedAdService ads, SimulatedPurchaseService store)
            {
                Ads = ads;
                Store = store;
            }
        }

        public static Parts Build(AppConfig config, UiStrings strings)
        {
            var root = new GameObject("Monetization (simulated)", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = OverlaySortingOrder;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();
            return new Parts(BuildAds(root.transform, config, strings), BuildStore(root.transform, config, strings));
        }

        private static SimulatedAdService BuildAds(Transform canvas, AppConfig config, UiStrings strings)
        {
            var overlay = UiSceneParts.Rect("Ad Overlay", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            overlay.gameObject.AddComponent<Image>().color = AdBackdrop; // raycast target: blocks the game below
            var title = UiSceneParts.Label("Title", overlay, strings.testAd, 110, TextAnchor.MiddleCenter);
            UiSceneParts.Place(title.rectTransform, new Vector2(0f, 0.55f), new Vector2(1f, 0.7f), Vector2.zero, Vector2.zero);
            title.color = AdInk;
            var countdown = UiSceneParts.Label("Countdown", overlay, "3", 160, TextAnchor.MiddleCenter);
            UiSceneParts.Place(countdown.rectTransform, new Vector2(0f, 0.38f), new Vector2(1f, 0.55f), Vector2.zero, Vector2.zero);
            countdown.color = AdInk;
            var (close, closeLabel) = UiSceneParts.Button("Close", overlay, new Vector2(0.62f, 0.88f), new Vector2(0.95f, 0.95f), 56);

            var service = canvas.gameObject.AddComponent<SimulatedAdService>();
            PrototypeSceneBuilder.SetReference(service, "config", config);
            PrototypeSceneBuilder.SetReference(service, "strings", strings);
            PrototypeSceneBuilder.SetReference(service, "overlay", overlay.gameObject);
            PrototypeSceneBuilder.SetReference(service, "titleText", title);
            PrototypeSceneBuilder.SetReference(service, "countdownText", countdown);
            PrototypeSceneBuilder.SetReference(service, "closeButton", close);
            PrototypeSceneBuilder.SetReference(service, "closeLabel", closeLabel);
            overlay.gameObject.SetActive(false);
            return service;
        }

        private static SimulatedPurchaseService BuildStore(Transform canvas, AppConfig config, UiStrings strings)
        {
            var dim = UiSceneParts.Rect("Purchase Dialog", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            dim.gameObject.AddComponent<Image>().color = DimColor;
            var panel = UiSceneParts.Rect("Panel", dim, new Vector2(0.1f, 0.38f), new Vector2(0.9f, 0.62f), Vector2.zero, Vector2.zero);
            UiStyle.Panel(panel, UiStyle.Paper);
            var message = UiSceneParts.Label("Message", panel, "", 56, TextAnchor.MiddleCenter);
            UiSceneParts.Place(message.rectTransform, new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero);
            var (buy, buyLabel) = UiSceneParts.Button("Buy", panel, new Vector2(0.53f, 0.08f), new Vector2(0.93f, 0.38f));
            var (cancel, cancelLabel) = UiSceneParts.Button("Cancel", panel, new Vector2(0.07f, 0.08f), new Vector2(0.47f, 0.38f), 64,
                UiStyle.ButtonLook.Secondary);

            var service = canvas.gameObject.AddComponent<SimulatedPurchaseService>();
            PrototypeSceneBuilder.SetReference(service, "config", config);
            PrototypeSceneBuilder.SetReference(service, "strings", strings);
            PrototypeSceneBuilder.SetReference(service, "dialog", dim.gameObject);
            PrototypeSceneBuilder.SetReference(service, "messageText", message);
            PrototypeSceneBuilder.SetReference(service, "buyButton", buy);
            PrototypeSceneBuilder.SetReference(service, "buyLabel", buyLabel);
            PrototypeSceneBuilder.SetReference(service, "cancelButton", cancel);
            PrototypeSceneBuilder.SetReference(service, "cancelLabel", cancelLabel);
            dim.gameObject.SetActive(false);
            return service;
        }
    }
}
