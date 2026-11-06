using System.Collections.Generic;
using Game.Editor;
using Game.Levels;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>Spec §6.1, §12.5: every room has its own backdrop, background colour and music loop.</summary>
    public sealed class RoomThemeTests
    {
        private static RoomTheme Theme(int room) =>
            AssetDatabase.LoadAssetAtPath<RoomTheme>(GameLevelListBuilder.ThemePath(room))
            ?? throw new AssertionException($"room {room} has no theme at {GameLevelListBuilder.ThemePath(room)}");

        [TestCase(1, "HomeOfficeBackdrop")]
        [TestCase(2, "CafeBackdrop")]
        [TestCase(3, "LibraryBackdrop")]
        [TestCase(4, "GamingDenBackdrop")]
        public void RoomTheme_HasBackdropMusicAndColour(int room, string backdrop)
        {
            var theme = Theme(room);

            Assert.IsNotNull(theme.backdropPrefab);
            Assert.AreEqual(backdrop, theme.backdropPrefab.name);
            Assert.IsNotNull(theme.music, "no music loop");
            Assert.AreEqual(1f, theme.background.a, 0.001f);
        }

        /// <summary>Kitchen spec §7: its own backdrop, a fifth music track no other room plays, and its own colour.</summary>
        [Test]
        public void KitchenTheme_HasBackdropMusicAndBackground()
        {
            var kitchen = AssetDatabase.LoadAssetAtPath<RoomTheme>(KitchenBuilder.ThemePath);

            Assert.IsNotNull(kitchen, KitchenBuilder.ThemePath);
            Assert.AreEqual("KitchenBackdrop", kitchen.backdropPrefab.name);
            Assert.AreEqual(AssetDatabase.LoadAssetAtPath<AudioClip>(KitchenAudioTests.KitchenMusic), kitchen.music);
            Assert.AreEqual(1f, kitchen.background.a, 0.001f);
            foreach (string path in AssetDatabase.FindAssets("t:RoomTheme"))
            {
                var other = AssetDatabase.LoadAssetAtPath<RoomTheme>(AssetDatabase.GUIDToAssetPath(path));
                if (other == kitchen) continue;
                Assert.AreNotEqual(kitchen.music, other.music, $"{other.name} plays the Kitchen's music");
                Assert.AreNotEqual(kitchen.background, other.background, $"{other.name} has the Kitchen's background");
            }
        }

        [Test]
        public void EveryRoom_HasItsOwnMusicAndBackground()
        {
            var music = new HashSet<AudioClip>();
            var backgrounds = new HashSet<Color>();
            for (int room = 1; room <= GameLevelListBuilder.RoomCount; room++)
            {
                if (!GameLevelListBuilder.HasRoom(room)) continue;
                var theme = Theme(room);
                Assert.IsTrue(music.Add(theme.music), $"room {room} reuses another room's music");
                Assert.IsTrue(backgrounds.Add(theme.background), $"room {room} reuses another room's background");
            }
        }
    }
}
