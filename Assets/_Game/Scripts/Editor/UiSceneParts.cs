using Game.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Game.Editor
{
    /// <summary>Portrait canvas with the HUD (top) and the result panel (centre), plus the EventSystem.</summary>
    internal static class UiSceneParts
    {
        private static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);
        private static readonly Color Ink = new Color(0.3f, 0.22f, 0.3f);
        private static readonly Color PanelColor = new Color(1f, 0.97f, 0.95f, 0.95f);
        private static readonly Color ButtonColor = new Color(1f, 0.72f, 0.62f);

        public readonly struct Parts
        {
            public readonly Hud Hud;
            public readonly ResultPanel Result;

            public Parts(Hud hud, ResultPanel result)
            {
                Hud = hud;
                Result = result;
            }
        }

        public static Parts Build()
        {
            var canvasGo = new GameObject("UI", typeof(RectTransform));
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var events = new GameObject("EventSystem");
            events.AddComponent<EventSystem>();
            events.AddComponent<InputSystemUIInputModule>();

            return new Parts(BuildHud(canvasGo.transform), BuildResult(canvasGo.transform));
        }

        private static Hud BuildHud(Transform canvas)
        {
            var hud = Rect("HUD", canvas, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -160f), Vector2.zero);
            var level = Label("Level", hud, "Level 1", 56, TextAnchor.MiddleLeft);
            Place(level.rectTransform, new Vector2(0f, 0f), new Vector2(0.5f, 1f), new Vector2(60f, 0f), Vector2.zero);
            var shots = Label("Shots", hud, "Shots: 6", 56, TextAnchor.MiddleRight);
            Place(shots.rectTransform, new Vector2(0.5f, 0f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-60f, 0f));

            var component = hud.gameObject.AddComponent<Hud>();
            PrototypeSceneBuilder.SetReference(component, "shotsText", shots);
            PrototypeSceneBuilder.SetReference(component, "levelText", level);
            return component;
        }

        private static ResultPanel BuildResult(Transform canvas)
        {
            var panel = Rect("Result Panel", canvas, new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.65f), Vector2.zero, Vector2.zero);
            panel.gameObject.AddComponent<Image>().color = PanelColor;
            var title = Label("Title", panel, "Desk cleared!", 84, TextAnchor.MiddleCenter);
            Place(title.rectTransform, new Vector2(0f, 0.6f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            var (primary, primaryLabel) = Button("Primary", panel, new Vector2(0.1f, 0.33f), new Vector2(0.9f, 0.55f));
            var (secondary, secondaryLabel) = Button("Secondary", panel, new Vector2(0.1f, 0.06f), new Vector2(0.9f, 0.28f));

            var component = panel.gameObject.AddComponent<ResultPanel>();
            PrototypeSceneBuilder.SetReference(component, "title", title);
            PrototypeSceneBuilder.SetReference(component, "primary", primary);
            PrototypeSceneBuilder.SetReference(component, "primaryLabel", primaryLabel);
            PrototypeSceneBuilder.SetReference(component, "secondary", secondary);
            PrototypeSceneBuilder.SetReference(component, "secondaryLabel", secondaryLabel);
            panel.gameObject.SetActive(false);
            return component;
        }

        private static (Button, Text) Button(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var rect = Rect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            rect.gameObject.AddComponent<Image>().color = ButtonColor;
            var button = rect.gameObject.AddComponent<Button>();
            var label = Label("Label", rect, name, 64, TextAnchor.MiddleCenter);
            Place(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return (button, label);
        }

        private static Text Label(string name, Transform parent, string text, int size, TextAnchor anchor)
        {
            var label = new GameObject(name, typeof(RectTransform)).AddComponent<Text>();
            label.transform.SetParent(parent, false);
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = text;
            label.fontSize = size;
            label.alignment = anchor;
            label.color = Ink;
            label.raycastTarget = false;
            return label;
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            Place(rect, anchorMin, anchorMax, offsetMin, offsetMax);
            return rect;
        }

        private static void Place(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
