using Game.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Editor
{
    /// <summary>
    /// The UI's look: the Lilita One font, sprites from UiSprites, the palette, and the
    /// pieces every screen shares (rounded panels, buttons with a lip, switches, pills).
    /// </summary>
    internal static class UiStyle
    {
        public const string FontPath = "Assets/_Game/Art/Fonts/LilitaOne-Regular.ttf";
        public const float ButtonLip = 14f;

        public static readonly Color Ink = new Color(0.3f, 0.22f, 0.3f);
        public static readonly Color Paper = new Color(1f, 0.97f, 0.94f);
        public static readonly Color Coral = new Color(0.97f, 0.5f, 0.37f);
        public static readonly Color CoralLip = new Color(0.76f, 0.33f, 0.26f);
        public static readonly Color Cream = new Color(1f, 0.92f, 0.85f);
        public static readonly Color CreamLip = new Color(0.86f, 0.72f, 0.64f);
        public static readonly Color SwitchOn = new Color(0.45f, 0.77f, 0.5f);
        public static readonly Color PillBack = new Color(0.3f, 0.22f, 0.3f, 0.45f);
        private static readonly Color DropShadow = new Color(0.2f, 0.12f, 0.15f, 0.3f);

        public enum ButtonLook { Primary, Secondary }

        public static Font Font => AssetDatabase.LoadAssetAtPath<Font>(FontPath) ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        public static Sprite Rounded => UiSprites.Rounded;
        public static Sprite Circle => UiSprites.Circle;

        /// <summary>A rounded card with a soft drop shadow.</summary>
        public static Image Panel(RectTransform rect, Color color)
        {
            var image = Fill(rect, color);
            var shadow = rect.gameObject.AddComponent<Shadow>();
            shadow.effectColor = DropShadow;
            shadow.effectDistance = new Vector2(0f, -16f);
            return image;
        }

        public static Image Fill(RectTransform rect, Color color)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = Rounded;
            image.type = Image.Type.Sliced;
            image.color = color;
            return image;
        }

        /// <summary>
        /// A rounded face over a darker lip; pressing sinks the face onto the lip. The root keeps the name and the Button,
        /// so callers and tests find it as before.
        /// </summary>
        public static (Button, Text) Button(RectTransform root, int fontSize, ButtonLook look)
        {
            bool primary = look == ButtonLook.Primary;
            var face = UiSceneParts.Rect("Face", root, Vector2.zero, Vector2.one, new Vector2(0f, ButtonLip), Vector2.zero);
            var faceImage = Fill(face, primary ? Coral : Cream);
            var lip = face.gameObject.AddComponent<Shadow>();
            lip.effectColor = primary ? CoralLip : CreamLip;
            lip.effectDistance = new Vector2(0f, -ButtonLip);

            var label = UiSceneParts.Label("Label", face, root.name, fontSize, TextAnchor.MiddleCenter);
            UiSceneParts.Place(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(24f, 6f), new Vector2(-24f, -6f));
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = Mathf.Min(24, fontSize);
            label.resizeTextMaxSize = fontSize;
            if (primary)
            {
                label.color = Color.white;
                Outline(label, CoralLip, 3f);
            }

            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = faceImage;
            var press = root.gameObject.AddComponent<ButtonPress>();
            PrototypeSceneBuilder.SetReference(press, "face", face);
            PrototypeSceneBuilder.SetReference(press, "lip", lip);
            return (button, label);
        }

        /// <summary>A switch on the right of a settings row: the knob slides right and the track turns green when on.</summary>
        public static Toggle Switch(RectTransform row)
        {
            var track = UiSceneParts.Rect("Track", row, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-170f, -42f), new Vector2(-10f, 42f));
            var trackImage = Fill(track, SwitchOn);
            var knob = UiSceneParts.Rect("Knob", track, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-78f, -34f), new Vector2(-8f, 34f));
            var knobImage = knob.gameObject.AddComponent<Image>();
            knobImage.sprite = Circle;
            knobImage.color = Color.white;
            var knobShadow = knob.gameObject.AddComponent<Shadow>();
            knobShadow.effectColor = DropShadow;
            knobShadow.effectDistance = new Vector2(0f, -4f);

            var toggle = row.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = trackImage;
            toggle.transition = Selectable.Transition.None;
            toggle.isOn = true;
            var view = row.gameObject.AddComponent<SwitchView>();
            PrototypeSceneBuilder.SetReference(view, "toggle", toggle);
            PrototypeSceneBuilder.SetReference(view, "track", trackImage);
            PrototypeSceneBuilder.SetReference(view, "knob", knob);
            return toggle;
        }

        /// <summary>A translucent rounded backing behind HUD text, so it reads over any room.</summary>
        public static void Pill(RectTransform rect) => Fill(rect, PillBack);

        public static void Outline(Text text, Color color, float width)
        {
            var outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(width, -width);
        }

        /// <summary>A white title with an ink outline and a drop, readable over the room.</summary>
        public static void Title(Text text)
        {
            text.color = Color.white;
            Outline(text, Ink, 5f);
            var drop = text.gameObject.AddComponent<Shadow>();
            drop.effectColor = DropShadow;
            drop.effectDistance = new Vector2(0f, -10f);
        }
    }
}
