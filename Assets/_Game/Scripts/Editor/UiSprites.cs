using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// The UI's sprites, drawn here once (no art pack has UI shapes, and icons are our own, never emoji): white shapes
    /// on transparent, 128 px, tinted per use. Rounded shapes are 9-sliced; icons are drawn from signed distances.
    /// </summary>
    public static class UiSprites
    {
        public const int RimPixels = 3;
        private const string Folder = "Assets/_Game/Art/Generated/Ui";
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
        public static Sprite IconGear => Ensure("IconGear", () => Shape(Gear), 0);
        public static Sprite IconPause => Ensure("IconPause", () => Shape(Pause), 0);
        public static Sprite IconMap => Ensure("IconMap", () => Shape(Map), 0);
        public static Sprite IconHome => Ensure("IconHome", () => Shape(House), 0);
        public static Sprite IconReplay => Ensure("IconReplay", () => Shape(Replay), 0);
        public static Sprite IconNext => Ensure("IconNext", () => Shape(Next), 0);
        public static Sprite IconNoAds => Ensure("IconNoAds", () => Shape(NoAds), 0);

        private static readonly Vector2 Centre = new Vector2(Size / 2f, Size / 2f);

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

        // Icons, drawn in a 128 box with a clear margin.

        private static float Gear(Vector2 p)
        {
            float body = Disc(p, Centre, 34f);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4f;
                var along = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var local = new Vector2(Vector2.Dot(p - Centre, along), Vector2.Dot(p - Centre, new Vector2(-along.y, along.x)));
                body = Mathf.Min(body, Box(local, new Vector2(40f, 0f), new Vector2(10f, 9f), 3f));
            }
            return Mathf.Max(body, -Disc(p, Centre, 14f));
        }

        private static float Pause(Vector2 p) =>
            Mathf.Min(Box(p, new Vector2(46f, 64f), new Vector2(11f, 34f), 5f), Box(p, new Vector2(82f, 64f), new Vector2(11f, 34f), 5f));

        /// <summary>A folded map: three panels, zig-zagging at the top and bottom, with gaps at the folds.</summary>
        private static float Map(Vector2 p) =>
            Mathf.Min(Mathf.Min(
                Convex(p, new Vector2(22f, 30f), new Vector2(49f, 22f), new Vector2(49f, 98f), new Vector2(22f, 106f)),
                Convex(p, new Vector2(52f, 22f), new Vector2(76f, 30f), new Vector2(76f, 106f), new Vector2(52f, 98f))),
                Convex(p, new Vector2(79f, 30f), new Vector2(106f, 22f), new Vector2(106f, 98f), new Vector2(79f, 106f)));

        private static float House(Vector2 p)
        {
            float roof = Convex(p, new Vector2(20f, 66f), new Vector2(108f, 66f), new Vector2(64f, 104f));
            float body = Box(p, new Vector2(64f, 46f), new Vector2(30f, 24f), 3f);
            float door = Box(p, new Vector2(64f, 36f), new Vector2(9f, 14f), 2f);
            return Mathf.Max(Mathf.Min(roof, body), -door);
        }

        /// <summary>A ring open at the top right with an arrowhead at the gap.</summary>
        private static float Replay(Vector2 p)
        {
            float ring = Mathf.Max(Disc(p, Centre, 38f), -Disc(p, Centre, 25f));
            var d = p - Centre;
            bool inGap = d.x > 0f && d.y > 0f && d.y > d.x * 0.3f;
            if (inGap) ring = Mathf.Max(ring, 1f);
            float head = Convex(p, new Vector2(80f, 116f), new Vector2(80f, 76f), new Vector2(108f, 96f));
            return Mathf.Min(ring, head);
        }

        private static float Next(Vector2 p) =>
            Mathf.Min(Box(p, new Vector2(48f, 64f), new Vector2(24f, 11f), 4f),
                Convex(p, new Vector2(66f, 98f), new Vector2(66f, 30f), new Vector2(104f, 64f)));

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

        /// <summary>A convex polygon, corners counter-clockwise; distance from its nearest edge line (good enough to anti-alias).</summary>
        private static float Convex(Vector2 p, params Vector2[] corners)
        {
            float distance = float.NegativeInfinity;
            for (int i = 0; i < corners.Length; i++)
            {
                var a = corners[i];
                var edge = (corners[(i + 1) % corners.Length] - a).normalized;
                float leftOfEdge = edge.x * (p.y - a.y) - edge.y * (p.x - a.x);
                distance = Mathf.Max(distance, -leftOfEdge);
            }
            return distance;
        }
    }
}
