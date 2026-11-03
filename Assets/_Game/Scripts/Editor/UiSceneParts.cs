using Game.Core;
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
        private static readonly Color BarBackColor = new Color(0.3f, 0.22f, 0.3f, 0.18f);
        private static readonly Color BarFillColor = new Color(0.98f, 0.55f, 0.42f);
        private const int DebugSliderCount = 7;

        public readonly struct Parts
        {
            public readonly Transform Canvas;
            public readonly Hud Hud;
            public readonly Text LevelLabel;
            public readonly ResultPanel Result;

            public Parts(Transform canvas, Hud hud, Text levelLabel, ResultPanel result)
            {
                Canvas = canvas;
                Hud = hud;
                LevelLabel = levelLabel;
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

            var hud = BuildHud(canvasGo.transform, out Text levelLabel);
            return new Parts(canvasGo.transform, hud, levelLabel, BuildResult(canvasGo.transform));
        }

        private static Hud BuildHud(Transform canvas, out Text levelLabel)
        {
            var hud = Rect("HUD", canvas, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -160f), Vector2.zero);
            var level = Label("Level", hud, "Level 1", 56, TextAnchor.MiddleLeft);
            Place(level.rectTransform, new Vector2(0f, 0f), new Vector2(0.5f, 1f), new Vector2(60f, 0f), Vector2.zero);
            var shots = Label("Shots", hud, "Shots: 6", 56, TextAnchor.MiddleRight);
            Place(shots.rectTransform, new Vector2(0.5f, 0f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-200f, 0f));
            // Top-right, clear of the pouch at the bottom.
            var (pause, pauseLabel) = Button("Pause", hud, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), 56);
            Place((RectTransform)pause.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-170f, -55f), new Vector2(-50f, 55f));
            pauseLabel.text = "II";

            var component = hud.gameObject.AddComponent<Hud>();
            PrototypeSceneBuilder.SetReference(component, "shotsText", shots);
            PrototypeSceneBuilder.SetReference(component, "levelText", level);
            PrototypeSceneBuilder.SetReference(component, "pauseButton", pause);
            levelLabel = level;
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

            // The reveal bar sits where the second button would be; a win shows only Next.
            var reveal = Rect("Reveal", panel, new Vector2(0.06f, 0.04f), new Vector2(0.94f, 0.3f), Vector2.zero, Vector2.zero);
            var revealText = Label("Text", reveal, "", 44, TextAnchor.MiddleCenter);
            Place(revealText.rectTransform, new Vector2(0f, 0.45f), Vector2.one, Vector2.zero, Vector2.zero);
            var bar = Rect("Bar", reveal, new Vector2(0.04f, 0.1f), new Vector2(0.96f, 0.38f), Vector2.zero, Vector2.zero);
            bar.gameObject.AddComponent<Image>().color = BarBackColor;
            var fill = Rect("Fill", bar, Vector2.zero, new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            fill.gameObject.AddComponent<Image>().color = BarFillColor;

            var component = panel.gameObject.AddComponent<ResultPanel>();
            PrototypeSceneBuilder.SetReference(component, "revealRoot", reveal.gameObject);
            PrototypeSceneBuilder.SetReference(component, "revealText", revealText);
            PrototypeSceneBuilder.SetReference(component, "revealFill", fill);
            PrototypeSceneBuilder.SetReference(component, "title", title);
            PrototypeSceneBuilder.SetReference(component, "primary", primary);
            PrototypeSceneBuilder.SetReference(component, "primaryLabel", primaryLabel);
            PrototypeSceneBuilder.SetReference(component, "secondary", secondary);
            PrototypeSceneBuilder.SetReference(component, "secondaryLabel", secondaryLabel);
            panel.gameObject.SetActive(false);
            return component;
        }

        /// <summary>Hidden tuning panel in the top half (away from the pouch); the level label becomes its toggle.</summary>
        public static Transform BuildDebugPanel(Parts parts, GameBootstrap bootstrap, ShotTuning tuningAsset)
        {
            parts.LevelLabel.raycastTarget = true;
            var toggle = parts.LevelLabel.gameObject.AddComponent<Button>();
            toggle.targetGraphic = parts.LevelLabel;
            toggle.transition = Selectable.Transition.None;

            var host = Rect("Debug Tuning", parts.Canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var panel = Rect("Panel", host, new Vector2(0.03f, 0.5f), new Vector2(0.97f, 0.9f), Vector2.zero, Vector2.zero);
            panel.gameObject.AddComponent<Image>().color = PanelColor;

            var sliders = new Slider[DebugSliderCount];
            var labels = new Text[DebugSliderCount];
            float rowHeight = 0.8f / DebugSliderCount;
            for (int i = 0; i < DebugSliderCount; i++)
            {
                float top = 1f - i * rowHeight - 0.01f;
                labels[i] = Label($"Label {i}", panel, "", 34, TextAnchor.MiddleLeft);
                Place(labels[i].rectTransform, new Vector2(0.04f, top - rowHeight), new Vector2(0.5f, top), Vector2.zero, Vector2.zero);
                var slider = DefaultControls.CreateSlider(new DefaultControls.Resources());
                slider.name = $"Slider {i}";
                slider.transform.SetParent(panel, false);
                Place((RectTransform)slider.transform, new Vector2(0.52f, top - rowHeight * 0.75f),
                    new Vector2(0.96f, top - rowHeight * 0.25f), Vector2.zero, Vector2.zero);
                sliders[i] = slider.GetComponent<Slider>();
            }
            var readout = Label("Readout", panel, "", 22, TextAnchor.UpperLeft);
            readout.verticalOverflow = VerticalWrapMode.Overflow;
            Place(readout.rectTransform, new Vector2(0.04f, 0.02f), new Vector2(0.76f, 0.19f), Vector2.zero, Vector2.zero);
            var (reset, _) = Button("Reset", panel, new Vector2(0.78f, 0.04f), new Vector2(0.96f, 0.17f), 40);

            var component = host.gameObject.AddComponent<DebugTuningPanel>();
            PrototypeSceneBuilder.SetReference(component, "bootstrap", bootstrap);
            PrototypeSceneBuilder.SetReference(component, "tuningAsset", tuningAsset);
            PrototypeSceneBuilder.SetReference(component, "toggleTarget", toggle);
            PrototypeSceneBuilder.SetReference(component, "panelRoot", panel.gameObject);
            PrototypeSceneBuilder.SetReference(component, "readout", readout);
            PrototypeSceneBuilder.SetReference(component, "resetButton", reset);
            SetArray(component, "sliders", sliders);
            SetArray(component, "sliderLabels", labels);
            return panel;
        }

        private static void SetArray(Object target, string field, Object[] values)
        {
            var so = new UnityEditor.SerializedObject(target);
            var array = so.FindProperty(field);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static (Button, Text) Button(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, int fontSize = 64)
        {
            var rect = Rect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            rect.gameObject.AddComponent<Image>().color = ButtonColor;
            var button = rect.gameObject.AddComponent<Button>();
            var label = Label("Label", rect, name, fontSize, TextAnchor.MiddleCenter);
            Place(label.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return (button, label);
        }

        internal static Text Label(string name, Transform parent, string text, int size, TextAnchor anchor)
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

        internal static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            Place(rect, anchorMin, anchorMax, offsetMin, offsetMax);
            return rect;
        }

        internal static void Place(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
