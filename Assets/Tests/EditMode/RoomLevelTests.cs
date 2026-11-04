using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Editor;
using Game.Levels;
using NUnit.Framework;
using UnityEditor;

namespace Game.Tests
{
    /// <summary>
    /// The level rules every room keeps (spec §4.4, §6.1), and the game list that joins the rooms in order. Each room's
    /// levels task adds its room to the [TestCase]s.
    /// </summary>
    public sealed class RoomLevelTests
    {
        private const int LevelsPerRoom = 25;

        private static LevelList Room(int room) =>
            AssetDatabase.LoadAssetAtPath<LevelList>(GameLevelListBuilder.RoomListPath(room))
            ?? throw new AssertionException($"room {room} has no level list at {GameLevelListBuilder.RoomListPath(room)}");

        private static LevelList GameList() =>
            AssetDatabase.LoadAssetAtPath<LevelList>(GameLevelListBuilder.ListPath)
            ?? throw new AssertionException($"no game level list at {GameLevelListBuilder.ListPath}");

        private static int FirstNumber(int room) => (room - 1) * LevelsPerRoom + 1;

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Room_Has25Levels_WithHardEveryFifth(int room)
        {
            var list = Room(room);

            Assert.AreEqual(LevelsPerRoom, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                int number = FirstNumber(room) + i;
                var expected = number % 5 == 0 ? Difficulty.Hard : Difficulty.Normal;
                Assert.AreEqual(expected, list.At(i).difficulty, $"level {number}");
                Assert.AreEqual($"R{room}_L{number:00}", list.At(i).id);
                Assert.IsTrue(list.At(i).Validate(out string error), error);
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Room_ShotsWithinFiveToTen(int room)
        {
            var list = Room(room);
            for (int i = 0; i < list.Count; i++)
                Assert.That(list.At(i).shots, Is.InRange(5, 10), list.At(i).id);
        }

        /// <summary>Owner, checkpoint 2: props holding others up must be cleared too.</summary>
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Room_EveryPropIsATarget(int room)
        {
            var list = Room(room);
            for (int i = 0; i < list.Count; i++)
                foreach (var prop in list.At(i).props)
                    Assert.IsTrue(prop.isTarget, $"{list.At(i).id}: {prop.kind} at {prop.basePosition} is not a target");
        }

        [TestCase(1, new[] { 6, 10, 14, 18, 20, 24, 25 })]
        [TestCase(2, new[] { 29, 33, 38, 40, 43, 48, 50 })]
        [TestCase(3, new[] { 54, 58, 60, 64, 68, 70, 74, 75 })]
        [TestCase(4, new[] { 79, 83, 85, 88, 94, 98, 100 })]
        public void Room_TwoDeskLevels_UseTheBackDesk(int room, int[] twoDeskNumbers)
        {
            var list = Room(room);
            var expected = new HashSet<int>(twoDeskNumbers);
            for (int i = 0; i < list.Count; i++)
            {
                var level = list.At(i);
                bool twoDesks = expected.Contains(FirstNumber(room) + i);
                Assert.AreEqual(twoDesks, level.hasBackDesk, level.id);
                bool usesBackDesk = System.Array.Exists(level.props, p => p.desk == LevelDef.BackDesk);
                Assert.AreEqual(twoDesks, usesBackDesk, $"{level.id}: back-desk props");
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void Room_LevelsUseItsOwnTheme(int room)
        {
            var theme = AssetDatabase.LoadAssetAtPath<RoomTheme>(GameLevelListBuilder.ThemePath(room));
            Assert.IsNotNull(theme, $"room {room} has no theme");
            var list = Room(room);
            for (int i = 0; i < list.Count; i++) Assert.AreEqual(theme, list.At(i).room, list.At(i).id);
        }

        [Test]
        public void GameList_IsTheRoomsInOrder()
        {
            var game = GameList();
            var expected = new List<LevelDef>();
            for (int room = 1; room <= GameLevelListBuilder.RoomCount; room++)
            {
                var list = AssetDatabase.LoadAssetAtPath<LevelList>(GameLevelListBuilder.RoomListPath(room));
                if (list != null) expected.AddRange(list.levels);
            }

            CollectionAssert.AreEqual(expected, game.levels);
            Assert.AreEqual(GameLevelListBuilder.RoomCount * LevelsPerRoom, game.Count, "spec §6.1: 100 levels");
            for (int i = 0; i < game.Count; i++)
            {
                var match = Regex.Match(game.At(i).id, @"^R(\d)_L(\d{2,})$");
                Assert.IsTrue(match.Success, game.At(i).id);
                Assert.AreEqual(i + 1, int.Parse(match.Groups[2].Value), game.At(i).id);
                Assert.AreEqual(i / LevelsPerRoom + 1, int.Parse(match.Groups[1].Value), game.At(i).id);
            }
        }

        /// <summary>Spec §6.1: a new kind about every 8–10 levels, each introduced alone by its level's idea.</summary>
        [Test]
        public void GameList_NewKindsArriveOnSchedule()
        {
            var expected = new List<string> { "1:Keycap1u", "3:Mug", "4:Book", "5:CactusPot", "12:PaperCup", "21:DeskLamp",
                "26:Plate", "36:Donut", "46:MilkBottle",
                "51:Candle", "61:Hourglass", "71:PencilCup",
                "76:SodaCan", "86:GameController", "91:FizzySoda" };
            var game = GameList();
            var seen = new HashSet<PropKind>();
            var intros = new List<string>();
            for (int i = 0; i < game.Count; i++)
                foreach (var prop in game.At(i).props)
                    if (seen.Add(UnlockReveal.Family(prop.kind))) intros.Add($"{i + 1}:{UnlockReveal.Family(prop.kind)}");

            CollectionAssert.AreEqual(expected, intros);
        }
    }
}
