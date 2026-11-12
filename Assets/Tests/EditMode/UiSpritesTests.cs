using Game.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>The candy UI's generated sprites: small, thin-rimmed, lit from the top, icons clear of their corners.</summary>
    public sealed class UiSpritesTests
    {
        private static Sprite[] All => new[]
        {
            UiSprites.Rounded, UiSprites.Circle, UiSprites.CandyFace, UiSprites.CandyRim, UiSprites.RibbonFace,
            UiSprites.IconGear, UiSprites.IconPause, UiSprites.IconMap, UiSprites.IconHome, UiSprites.IconReplay,
            UiSprites.IconNext, UiSprites.IconNoAds,
        };

        private static Sprite[] Icons => new[]
        {
            UiSprites.IconGear, UiSprites.IconPause, UiSprites.IconMap, UiSprites.IconHome, UiSprites.IconReplay,
            UiSprites.IconNext, UiSprites.IconNoAds,
        };

        [Test]
        public void EverySprite_IsGeneratedAndSmall()
        {
            foreach (var sprite in All)
            {
                Assert.IsNotNull(sprite);
                Assert.LessOrEqual(sprite.texture.width, 128, sprite.name);
                Assert.LessOrEqual(sprite.texture.height, 128, sprite.name);
                Assert.AreEqual(1, sprite.texture.mipmapCount, $"{sprite.name} has mipmaps");
            }
        }

        [Test]
        public void Rim_IsThin()
        {
            var rim = Readable(UiSprites.CandyRim);
            int mid = rim.height / 2;

            Assert.AreEqual(0f, rim.GetPixel(rim.width / 2, mid).a, 0.01f, "the rim's middle should be clear");
            Assert.AreEqual(0f, rim.GetPixel(UiSprites.RimPixels + 1, mid).a, 0.01f, "the rim is wider than RimPixels");
            Assert.Greater(rim.GetPixel(1, mid).a, 0.5f, "the rim's edge should be solid");
        }

        [Test]
        public void Face_IsLighterAtTheTop()
        {
            var face = Readable(UiSprites.CandyFace);
            int x = face.width / 2;

            Assert.Greater(face.GetPixel(x, face.height - 4).grayscale, face.GetPixel(x, 3).grayscale);
        }

        [Test]
        public void Icons_HaveInkAndClearCorners()
        {
            foreach (var icon in Icons)
            {
                var texture = Readable(icon);
                var pixels = texture.GetPixels();
                Assert.That(System.Array.Exists(pixels, p => p.a > 0.5f), Is.True, $"{icon.name} is empty");
                Assert.AreEqual(0f, texture.GetPixel(0, 0).a, 0.01f, $"{icon.name} corner is not clear");
            }
        }

        /// <summary>Icons a CC0 pack has come from that pack (owner, 2026-11-12); only No Ads is still drawn.</summary>
        [Test]
        public void PackIcons_ComeFromKenney()
        {
            foreach (var icon in new[] { UiSprites.IconGear, UiSprites.IconPause, UiSprites.IconMap, UiSprites.IconHome, UiSprites.IconReplay, UiSprites.IconNext })
                StringAssert.StartsWith("Assets/_Game/Art/Packs/KenneyIcons/", AssetDatabase.GetAssetPath(icon), icon.name);
        }

        /// <summary>The icon folder sits under Packs/ but holds UI sprites, not a model atlas.</summary>
        [Test]
        public void AtlasMaterials_SkipTheIconPack()
        {
            AtlasMaterialBuilder.BuildAll();

            Assert.IsNull(AtlasMaterialBuilder.Load("KenneyIcons"));
            Assert.IsNotNull(AtlasMaterialBuilder.Load("TinyTreats"));
        }

        /// <summary>A CPU copy of the sprite's texture (imported sprites aren't readable).</summary>
        private static Texture2D Readable(Sprite sprite)
        {
            var source = sprite.texture;
            var target = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(source, target);
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            var copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, true);
            copy.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            copy.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            return copy;
        }
    }
}
