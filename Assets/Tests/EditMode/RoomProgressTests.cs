using System.Linq;
using Game.Levels;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public sealed class RoomProgressTests
    {
        private RoomTheme office, kitchen, cafe;
        private LevelList list;

        [SetUp]
        public void SetUp()
        {
            office = ScriptableObject.CreateInstance<RoomTheme>();
            kitchen = ScriptableObject.CreateInstance<RoomTheme>();
            cafe = ScriptableObject.CreateInstance<RoomTheme>();
            list = ScriptableObject.CreateInstance<LevelList>();
            // 3 office levels, 2 kitchen, 4 café.
            list.levels = new[] { office, office, office, kitchen, kitchen, cafe, cafe, cafe, cafe }.Select(room =>
            {
                var level = ScriptableObject.CreateInstance<LevelDef>();
                level.room = room;
                return level;
            }).ToArray();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var level in list.levels) Object.DestroyImmediate(level);
            Object.DestroyImmediate(list);
            Object.DestroyImmediate(office);
            Object.DestroyImmediate(kitchen);
            Object.DestroyImmediate(cafe);
        }

        [Test]
        public void Rooms_SplitByTheme_InPlayOrder()
        {
            var rooms = RoomProgress.Rooms(list, 0);

            Assert.AreEqual(3, rooms.Count);
            Assert.AreEqual(new[] { 1, 2, 3 }, rooms.Select(r => r.Number).ToArray());
            Assert.AreEqual(new[] { 0, 3, 5 }, rooms.Select(r => r.FirstIndex).ToArray());
            Assert.AreEqual(new[] { 3, 2, 4 }, rooms.Select(r => r.Count).ToArray());
            Assert.AreSame(kitchen, rooms[1].Theme);
            Assert.AreEqual(8, rooms[2].LastIndex);
        }

        [Test]
        public void Rooms_NewSave_FirstCurrentRestLocked()
        {
            var rooms = RoomProgress.Rooms(list, 0);

            Assert.AreEqual(new[] { RoomState.Current, RoomState.Locked, RoomState.Locked }, rooms.Select(r => r.State).ToArray());
            Assert.AreEqual(new[] { 0, 0, 0 }, rooms.Select(r => r.Cleared).ToArray());
            Assert.IsFalse(rooms[1].CanPlay);
        }

        [Test]
        public void Rooms_MidSecondRoom_FirstDoneSecondCurrent()
        {
            var rooms = RoomProgress.Rooms(list, 4);

            Assert.AreEqual(new[] { RoomState.Done, RoomState.Current, RoomState.Locked }, rooms.Select(r => r.State).ToArray());
            Assert.AreEqual(new[] { 3, 1, 0 }, rooms.Select(r => r.Cleared).ToArray());
        }

        [Test]
        public void Rooms_SavedAtARoomStart_PreviousDone()
        {
            var rooms = RoomProgress.Rooms(list, 5);

            Assert.AreEqual(RoomState.Done, rooms[1].State);
            Assert.AreEqual(RoomState.Current, rooms[2].State);
            Assert.AreEqual(0, rooms[2].Cleared);
        }

        [Test]
        public void Rooms_EveryLevelWon_AllDone()
        {
            var rooms = RoomProgress.Rooms(list, 9);

            Assert.IsTrue(rooms.All(r => r.State == RoomState.Done));
            Assert.AreEqual(4, rooms[2].Cleared);
        }

        [Test]
        public void PlayIndex_CurrentRoomContinues_DoneRoomStartsOver()
        {
            var rooms = RoomProgress.Rooms(list, 4);

            Assert.AreEqual(0, RoomProgress.PlayIndex(rooms[0], 4));
            Assert.AreEqual(4, RoomProgress.PlayIndex(rooms[1], 4));
        }
    }
}
