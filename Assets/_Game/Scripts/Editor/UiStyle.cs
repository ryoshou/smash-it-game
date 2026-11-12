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
    public static class UiStyle
    {
        public const string FontPath = "Assets/_Game/Art/Fonts/LilitaOne-Regular.ttf";
        public const float CandyLip = 8f;
        public const float IconSize = 120f;
        /// <summary>An icon button's root: a round IconSize face over a CandyLip lip.</summary>
        public static readonly Vector2 IconButtonSize = new Vector2(IconSize, IconSize + CandyLip);
        private const float BadgeLip = 6f;
        private const float TextStroke = 2f;
        private const float TitleStroke = 3f;
        private const float RibbonHeight = 104f;
        private const float LeadingIconWidth = 72f;
        private const float LeadingIconInset = 28f;  // clear of the rounded corner
        private const float RibbonOverhang = 0.04f;
        private static readonly Color TitleInk = CandyLook.Hex(0x5A2A00);

        public static readonly Color Ink = new Color(0.3f, 0.22f, 0.3f);
        public static readonly Color Paper = new Color(1f, 0.97f, 0.94f);
        public static readonly Color Coral = new Color(0.97f, 0.5f, 0.37f);
        public static readonly Color Cream = new Color(1f, 0.92f, 0.85f);
        public static readonly Color SwitchOn = new Color(0.45f, 0.77f, 0.5f);
        private static readonly Color DropShadow = new Color(0.2f, 0.12f, 0.15f, 0.3f);

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
        /// A candy button: a face lit from the top inside a thin rim, over a short lip in the rim's colour; pressing sinks
        /// the face onto the lip. The root keeps the name and the Button, so callers and tests find it as before.
        /// </summary>
        public static (Button, Text) Candy(RectTransform root, CandyLook look, int fontSize)
        {
            var (face, faceImage, lip) = CandyFace(root, look, CandyLip, 1f);
            var label = CandyLabel(face, root.name, look, fontSize, Mathf.Min(24, fontSize));
            return (Pressable(root, face, faceImage, lip), label);
        }

        /// <summary>A round candy button showing a drawn icon in the rim's colour. Size the root IconButtonSize.</summary>
        public static Button IconButton(RectTransform root, CandyLook look, Sprite icon)
        {
            // Shrink the sprite's units so its corners round the whole square into a circle.
            var (face, faceImage, lip) = CandyFace(root, look, CandyLip, UiSprites.CornerRadius / (IconSize / 2f));
            var glyph = UiSceneParts.Rect("Icon", face, new Vector2(0.22f, 0.22f), new Vector2(0.78f, 0.78f), Vector2.zero, Vector2.zero);
            var image = glyph.gameObject.AddComponent<Image>();
            image.sprite = icon;
            image.color = look.Rim;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return Pressable(root, face, faceImage, lip);
        }

        /// <summary>A candy pill that shows a value (level, shots left); not a button.</summary>
        public static Text Badge(RectTransform root, CandyLook look, int fontSize)
        {
            var (face, faceImage, _) = CandyFace(root, look, BadgeLip, 1f);
            faceImage.raycastTarget = false;
            return CandyLabel(face, root.name, look, fontSize, 28);
        }

        /// <summary>A cream candy panel with a yellow ribbon across its top edge holding the title; returns the title.</summary>
        public static Text RibbonPanel(RectTransform panel, string ribbon)
        {
            var image = SlicedImage(panel.gameObject, UiSprites.CandyFace, CandyLook.Cream.Top, 1f);
            Rim(panel, CandyLook.Cream, 1f);
            Lip(panel.gameObject, CandyLook.Cream, CandyLip);
            image.raycastTarget = true;

            var band = UiSceneParts.Rect("Ribbon", panel, new Vector2(-RibbonOverhang, 1f), new Vector2(1f + RibbonOverhang, 1f),
                new Vector2(0f, -RibbonHeight / 2f), new Vector2(0f, RibbonHeight / 2f));
            // RibbonFace has tighter corners; the shared rim is scaled to match them.
            SlicedImage(band.gameObject, UiSprites.RibbonFace, CandyLook.Yellow.Top, 1f);
            Rim(band, CandyLook.Yellow, UiSprites.CornerRadius / 24f);
            Lip(band.gameObject, CandyLook.Yellow, BadgeLip);
            var title = CandyLabel(band, ribbon, CandyLook.Yellow, 64, 32);
            title.text = ribbon;
            return title;
        }

        /// <summary>A cream candy card (face, rim, lip) that a list row is built on; returns the face for the button.</summary>
        public static Image Card(RectTransform card)
        {
            var image = SlicedImage(card.gameObject, UiSprites.CandyFace, CandyLook.Cream.Top, 1f);
            Rim(card, CandyLook.Cream, 1f);
            Lip(card.gameObject, CandyLook.Cream, BadgeLip);
            return image;
        }

        /// <summary>A white icon at the left of a candy button's label; the label moves right to make room.</summary>
        public static void LeadingIcon(Text label, Sprite icon)
        {
            var face = (RectTransform)label.transform.parent;
            var glyph = UiSceneParts.Rect("Icon", face, new Vector2(0f, 0.2f), new Vector2(0f, 0.8f), Vector2.zero, Vector2.zero);
            glyph.sizeDelta = new Vector2(LeadingIconWidth, glyph.sizeDelta.y);
            glyph.anchoredPosition = new Vector2(LeadingIconInset + LeadingIconWidth / 2f, 0f);
            var image = glyph.gameObject.AddComponent<Image>();
            image.sprite = icon;
            image.preserveAspect = true;
            image.raycastTarget = false;
            label.rectTransform.offsetMin += new Vector2(LeadingIconInset + LeadingIconWidth, 0f);
        }

        private static (RectTransform face, Image image, Shadow lip) CandyFace(RectTransform root, CandyLook look, float lipHeight,
            float unitsScale)
        {
            var face = UiSceneParts.Rect("Face", root, Vector2.zero, Vector2.one, new Vector2(0f, lipHeight), Vector2.zero);
            var image = SlicedImage(face.gameObject, UiSprites.CandyFace, look.Top, unitsScale);
            Rim(face, look, unitsScale);
            return (face, image, Lip(face.gameObject, look, lipHeight));
        }

        private static Image SlicedImage(GameObject target, Sprite sprite, Color color, float unitsScale)
        {
            var image = target.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            image.pixelsPerUnitMultiplier = unitsScale;
            return image;
        }

        private static void Rim(RectTransform face, CandyLook look, float unitsScale)
        {
            var rim = UiSceneParts.Rect("Rim", face, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            SlicedImage(rim.gameObject, UiSprites.CandyRim, look.Rim, unitsScale).raycastTarget = false;
        }

        private static Shadow Lip(GameObject face, CandyLook look, float height)
        {
            var lip = face.AddComponent<Shadow>();
            lip.effectColor = look.Rim;
            lip.effectDistance = new Vector2(0f, -height);
            return lip;
        }

        private static Text CandyLabel(RectTransform face, string text, CandyLook look, int fontSize, int minSize)
        {
            var label = UiSceneParts.Label("Label", face, text, fontSize, TextAnchor.MiddleCenter);
            UiSceneParts.Place(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(24f, 6f), new Vector2(-24f, -6f));
            label.color = Color.white;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = minSize;
            label.resizeTextMaxSize = fontSize;
            label.raycastTarget = false;
            Outline(label, look.Rim, TextStroke);
            return label;
        }

        private static Button Pressable(RectTransform root, RectTransform face, Image faceImage, Shadow lip)
        {
            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = faceImage;
            var press = root.gameObject.AddComponent<ButtonPress>();
            PrototypeSceneBuilder.SetReference(press, "face", face);
            PrototypeSceneBuilder.SetReference(press, "lip", lip);
            return button;
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

        public static void Outline(Text text, Color color, float width)
        {
            var outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(width, -width);
        }

        /// <summary>A yellow title with a brown stroke and a short drop, readable over the room.</summary>
        public static void Title(Text text)
        {
            text.color = CandyLook.Yellow.Top;
            Outline(text, TitleInk, TitleStroke);
            var drop = text.gameObject.AddComponent<Shadow>();
            drop.effectColor = TitleInk;
            drop.effectDistance = new Vector2(0f, -CandyLip);
        }
    }
}
