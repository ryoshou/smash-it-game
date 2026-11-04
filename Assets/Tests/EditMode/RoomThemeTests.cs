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
