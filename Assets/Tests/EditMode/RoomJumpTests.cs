using Game.Levels;
using Game.Meta;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    /// <summary>The debug panel's "Next room" jump: the first later level in another room, wrapping to level 1.</summary>
    public sealed class RoomJumpTests
    {
        private RoomTheme office;
        private RoomTheme cafe;
        private LevelList list;

        [SetUp]
        public void SetUp()
        {
            office = ScriptableObject.CreateInstance<RoomTheme>();
            cafe = ScriptableObject.CreateInstance<RoomTheme>();
            list = ScriptableObject.CreateInstance<LevelList>();
            list.levels = new[] { Level(office), Level(office), Level(cafe) };
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var level in list.levels) Object.DestroyImmediate(level);
            Object.DestroyImmediate(list);
            Object.DestroyImmediate(office);
            Object.DestroyImmediate(cafe);
        }

        private static LevelDef Level(RoomTheme room)
        {
            var level = ScriptableObject.CreateInstance<LevelDef>();
            level.room = room;
            return level;
        }

        [TestCase(0, 2)]
        [TestCase(1, 2)]
        [TestCase(2, 0)]
        public void NextRoomStart_FindsTheNextRoomOrWraps(int from, int expected) =>
            Assert.AreEqual(expected, list.NextRoomStart(from));

        [Test]
        public void NextRoomStart_IndexPastTheList_WrapsFirst() => Assert.AreEqual(2, list.NextRoomStart(3));

        [Test]
        public void NextRoomStart_OneRoom_GoesToLevelOne()
        {
            list.levels[2].room = office;

            Assert.AreEqual(0, list.NextRoomStart(1));
        }

        [Test]
        public void Profile_JumpTo_SetsTheLevelEvenBackwards()
        {
            var profile = new Profile(new MemoryStore());
            profile.RecordWin(9);

            profile.JumpTo(2);

            Assert.AreEqual(2, profile.Data.levelIndex);
        }

        private sealed class MemoryStore : ISaveStore
        {
            private string json;
            public string Load() => json;
            public void Save(string value) => json = value;
        }
    }
}
