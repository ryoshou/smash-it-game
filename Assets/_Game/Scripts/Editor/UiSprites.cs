using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// The UI's sprites: white on transparent, at most 128 px, tinted per use. Rounded shapes are drawn here once and
    /// 9-sliced; icons come from Kenney's CC0 icon packs (copied by import-packs.sh), never emoji.
    /// </summary>
    public static class UiSprites
    {
        public const int RimPixels = 3;
        private const string Folder = "Assets/_Game/Art/Generated/Ui";
        public const string PackFolder = "Assets/_Game/Art/Packs/KenneyIcons";
        private const int Size = 128;
        public const int CornerRadius = 40;
        private const int Radius = CornerRadius;
        private const int RibbonRadius = 24;
        private const float FaceBottom = 0.8f;  // the face's brightness at its bottom edge (top is 1)
        private const float Highlight = 0.08f;  // extra brightness in the top third

        public static Sprite Rounded => Ensure("Rounded", () => Shape(RoundedBox(Radius)), Radius);
        public static Sprite Circle => Ensure("Circle", () => Shape(p => Disc(p, Centre, Size / 2f)), 0);
        public static Sprite CandyFace => Ensure("CandyFace", () => Face(Radius), Radius);
        public static Sprite CandyRim => Ensure("CandyRim", Rim, Radius);
        public static Sprite RibbonFace => Ensure("RibbonFace", () => Face(RibbonRadius), RibbonRadius);
        public static Sprite IconGear => Pack("gear");
        public static Sprite IconPause => Pack("pause");
        public static Sprite IconMap => Pack("structure_house");  // the Rooms button; no CC0 pack has a map
        public static Sprite IconHome => Pack("home");
        public static Sprite IconReplay => Pack("return");
        public static Sprite IconNext => Pack("arrowRight");
        /// <summary>Still drawn: no Kenney pack has a no-ads sign.</summary>
        public static Sprite IconNoAds => Ensure("IconNoAds", () => Shape(NoAds), 0);

        private static readonly Vector2 Centre = new Vector2(Size / 2f, Size / 2f);

        /// <summary>A pack icon, imported as an unmipped sprite the first time it is asked for.</summary>
        private static Sprite Pack(string name)
        {
            string path = $"{PackFolder}/{name}.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path)
                ?? throw new FileNotFoundException($"{path} is missing: run tools/assets/import-packs.sh");
            if (importer.textureType != TextureImporterType.Sprite || importer.mipmapEnabled)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite Ensure(string name, Func<Texture2D> draw, int border)
        {
            string path = $"{Folder}/{name}.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;
            Directory.CreateDirectory(Folder);
            File.WriteAllBytes(path, draw().EncodeToPNG());
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.spriteBorder = Vector4.one * border;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        /// <summary>White where the distance is negative, with a one-pixel anti-aliased edge.</summary>
        private static Texture2D Shape(Func<Vector2, float> distance) => Paint((p, _) => (1f, Coverage(distance(p))));

        /// <summary>A rounded face lit from the top: tinted, it shades from the colour down to a darker bottom.</summary>
        private static Texture2D Face(int radius) => Paint((p, v) =>
        {
            float light = Mathf.Lerp(FaceBottom, 1f, v) + (v > 0.66f ? Highlight * Mathf.InverseLerp(0.66f, 0.8f, v) : 0f);
            return (Mathf.Min(1f, light), Coverage(RoundedBox(radius)(p)));
        });

        /// <summary>The outline: the rounded shape minus itself inset by RimPixels.</summary>
        private static Texture2D Rim() => Paint((p, _) =>
        {
            float outer = Coverage(RoundedBox(Radius)(p));
            float inner = Coverage(Box(p, Centre, Vector2.one * (Size / 2f - RimPixels), Radius - RimPixels));
            return (1f, Mathf.Clamp01(outer - inner));
        });

        private static Texture2D Paint(Func<Vector2, float, (float light, float alpha)> pixel)
        {
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            var pixels = new Color32[Size * Size];
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                var (light, alpha) = pixel(new Vector2(x + 0.5f, y + 0.5f), (y + 0.5f) / Size);
                byte grey = (byte)(light * 255f);
                pixels[y * Size + x] = new Color32(grey, grey, grey, (byte)(alpha * 255f));
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        private static float Coverage(float distance) => Mathf.Clamp01(0.5f - distance);

        private static Func<Vector2, float> RoundedBox(int radius) => p => Box(p, Centre, Vector2.one * (Size / 2f), radius);

        // The one drawn icon, in a 128 box with a clear margin.

        /// <summary>A prohibition sign: a ring with a diagonal bar.</summary>
        private static float NoAds(Vector2 p)
        {
            float ring = Mathf.Max(Disc(p, Centre, 46f), -Disc(p, Centre, 34f));
            var d = p - Centre;
            float bar = Mathf.Abs(d.x + d.y) / Mathf.Sqrt(2f) - 6f;
            return Mathf.Min(ring, Mathf.Max(bar, Disc(p, Centre, 36f)));
        }

        // Signed distances: negative inside.

        private static float Disc(Vector2 p, Vector2 centre, float radius) => Vector2.Distance(p, centre) - radius;

        private static float Box(Vector2 p, Vector2 centre, Vector2 half, float radius)
        {
            var q = new Vector2(Mathf.Abs(p.x - centre.x), Mathf.Abs(p.y - centre.y)) - half + Vector2.one * radius;
            return new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f)).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - radius;
        }
    }
}
