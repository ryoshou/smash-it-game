using System.Collections;
using Game.Juice;
using Game.Levels;
using Game.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    /// <summary>Crossing a room boundary swaps the backdrop and music once; after the last level (100 in the game list) play wraps to level 1.</summary>
    public sealed class RoomFlowPlayTests
    {
        private GameBootstrap bootstrap;
        private RoomPresenter presenter;
        private LevelList list;
        private RoomTheme office;
        private RoomTheme other;
        private AudioClip otherMusic;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Game");
            yield return null;
            bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            presenter = Object.FindAnyObjectByType<RoomPresenter>();
            var real = bootstrap.Levels.At(0);
            office = real.room;
            otherMusic = AudioClip.Create("other room", 4410, 1, 44100, false);
            other = Object.Instantiate(office);
            other.music = otherMusic;
            other.background = Color.blue;
            list = ScriptableObject.CreateInstance<LevelList>();
            list.levels = new[] { Copy(real, office), Copy(real, other) };
            var so = new UnityEditor.SerializedObject(bootstrap);
            so.FindProperty("levels").objectReferenceValue = list;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var level in list.levels) Object.Destroy(level);
            Object.Destroy(list);
            Object.Destroy(other);
            Object.Destroy(otherMusic);
        }

        private static LevelDef Copy(LevelDef level, RoomTheme room)
        {
            var copy = Object.Instantiate(level);
            copy.room = room;
            return copy;
        }

        [UnityTest]
        public IEnumerator NextLevel_AcrossRoomBoundary_SwapsRoomOnce()
        {
            bootstrap.LoadLevel(0);
            yield return null;
            Assert.AreEqual(office, presenter.Current);

            bootstrap.Next();
            yield return null;

            Assert.AreEqual(other, presenter.Current);
            Assert.AreEqual(1, presenter.transform.childCount, "exactly one backdrop after the swap");
            var music = Object.FindAnyObjectByType<MusicPlayer>();
            Assert.AreEqual(otherMusic, music.Current, "the new room's music plays");
            Assert.AreEqual(1, PlayingMusicSources(), "no doubled music");
            Assert.AreEqual(Color.blue, Camera.main.backgroundColor);
        }

        [UnityTest]
        public IEnumerator NextAfterLastLevel_PlaysLevel1InHomeOffice()
        {
            bootstrap.LoadLevel(1);
            yield return null;

            bootstrap.Next();
            yield return null;

            Assert.AreEqual(list.At(0), bootstrap.Level);
            Assert.AreEqual(office, presenter.Current);
            Assert.AreEqual(office.music, Object.FindAnyObjectByType<MusicPlayer>().Current);
            Assert.AreEqual(1, presenter.transform.childCount);
        }

        private int PlayingMusicSources()
        {
            int count = 0;
            foreach (var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
                if (source.isPlaying && (source.clip == office.music || source.clip == otherMusic)) count++;
            return count;
        }
    }
}
