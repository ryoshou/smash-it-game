using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Editor;
using Game.Levels;
using NUnit.Framework;
using UnityEditor;

namespace Game.Tests
{
    /// <summary>
    /// The level rules every room keeps (spec §4.4, §6.1), and the game list that joins the rooms in order. The Kitchen
    /// (room 2) has its own feast-and-hard schedule and bands (Kitchen spec §4), checked by the Kitchen_ tests.
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

        private const int KitchenRoom = 2;
        private static readonly HashSet<int> KitchenFeasts = new HashSet<int> { 27, 31, 34, 37, 41, 44, 47, 50 };
        private static readonly HashSet<int> KitchenHard = new HashSet<int> { 30, 35, 40, 45 };

        private static int FirstNumber(int room) => (room - 1) * LevelsPerRoom + 1;

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
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
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
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
        [TestCase(5)]
        public void Room_EveryPropIsATarget(int room)
        {
            var list = Room(room);
            for (int i = 0; i < list.Count; i++)
                foreach (var prop in list.At(i).props)
                    Assert.IsTrue(prop.isTarget, $"{list.At(i).id}: {prop.kind} at {prop.basePosition} is not a target");
        }

        [TestCase(1, new[] { 6, 10, 14, 18, 20, 24, 25 })]
        [TestCase(2, new[] { 29, 33, 38, 43, 48, 50 })]
        [TestCase(3, new[] { 54, 58, 63, 65, 68, 73, 75 })]
        [TestCase(4, new[] { 79, 83, 85, 89, 93, 95, 99, 100 })]
        [TestCase(5, new[] { 104, 108, 110, 113, 119, 123, 125 })]
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
        [TestCase(5)]
        public void Room_LevelsUseItsOwnTheme(int room)
        {
            var theme = AssetDatabase.LoadAssetAtPath<RoomTheme>(GameLevelListBuilder.ThemePath(room));
            Assert.IsNotNull(theme, $"room {room} has no theme");
            var list = Room(room);
            for (int i = 0; i < list.Count; i++) Assert.AreEqual(theme, list.At(i).room, list.At(i).id);
        }

        /// <summary>Kitchen spec §4: 8 feasts spread through the room (the first within three levels), Hard every 5th.</summary>
        [Test]
        public void Kitchen_FeastsAndHardOnSchedule()
        {
            var list = Room(KitchenRoom);

            Assert.AreEqual(LevelsPerRoom, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                int number = FirstNumber(KitchenRoom) + i;
                var expected = KitchenFeasts.Contains(number) ? Difficulty.Feast
                    : KitchenHard.Contains(number) ? Difficulty.Hard : Difficulty.Normal;
                Assert.AreEqual(expected, list.At(i).difficulty, $"level {number}");
                Assert.AreEqual($"R{KitchenRoom}_L{number:00}", list.At(i).id);
                Assert.IsTrue(list.At(i).Validate(out string error), error);
            }
        }

        /// <summary>Feasts get extra shots: they are a reward, never a puzzle.</summary>
        [Test]
        public void Kitchen_ShotsWithinBands()
        {
            var list = Room(KitchenRoom);
            for (int i = 0; i < list.Count; i++)
            {
                var level = list.At(i);
                if (level.difficulty == Difficulty.Feast) Assert.That(level.shots, Is.InRange(12, 18), level.id);
                else Assert.That(level.shots, Is.InRange(5, 10), level.id);
            }
        }

        /// <summary>Normal and Hard levels hold 10–18 props, feasts 25–40, and level 50 is the biggest feast.</summary>
        [Test]
        public void Kitchen_PropCountsWithinBands()
        {
            var list = Room(KitchenRoom);
            int most = 0;
            for (int i = 0; i < list.Count; i++)
            {
                var level = list.At(i);
                if (level.difficulty == Difficulty.Feast) Assert.That(level.props.Length, Is.InRange(25, 40), level.id);
                else Assert.That(level.props.Length, Is.InRange(10, 18), level.id);
                if (i < list.Count - 1) most = System.Math.Max(most, level.props.Length);
            }
            Assert.Greater(list.At(list.Count - 1).props.Length, most, "level 50 should be the biggest feast");
        }

        /// <summary>Next room (debug panel) and the reveal bar step 1 → 26 → 51 → 76 → 101 → back to 1.</summary>
        [Test]
        public void GameList_RoomStartsAt_1_26_51_76_101()
        {
            var game = GameList();
            var starts = new List<int> { 0 };
            for (int i = 0; i < GameLevelListBuilder.RoomCount; i++) starts.Add(game.NextRoomStart(starts[starts.Count - 1]));

            CollectionAssert.AreEqual(new[] { 0, 25, 50, 75, 100, 0 }, starts);
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
            Assert.AreEqual(GameLevelListBuilder.RoomCount * LevelsPerRoom, game.Count, "Kitchen spec §3: 125 levels");
            for (int i = 0; i < game.Count; i++)
            {
                var match = Regex.Match(game.At(i).id, @"^R(\d)_L(\d{2,})$");
                Assert.IsTrue(match.Success, game.At(i).id);
                Assert.AreEqual(i + 1, int.Parse(match.Groups[2].Value), game.At(i).id);
                Assert.AreEqual(i / LevelsPerRoom + 1, int.Parse(match.Groups[1].Value), game.At(i).id);
            }
        }

        /// <summary>
        /// Spec §6.1: a new kind about every 8–10 levels, introduced by its level's idea; the Kitchen brings its kinds
        /// two at a time (Kitchen spec §4). Each entry is a level and the kinds it introduces, in PropKind order.
        /// </summary>
        [Test]
        public void GameList_NewKindsArriveOnSchedule()
        {
            var expected = new List<string> { "1:Keycap1u", "3:Mug", "4:Book", "5:CactusPot", "12:PaperCup", "21:DeskLamp",
                "26:DinnerPlate,Apple", "28:Bowl,Tomato", "32:Teapot,Cheese", "36:JamJar,Cupcake", "38:WineGlass,Burger",
                "42:KetchupBottle,Croissant", "46:DrinkCan,Sandwich",
                "51:Plate", "61:Donut", "71:MilkBottle",
                "76:Candle", "86:Hourglass", "96:PencilCup",
                "101:SodaCan", "111:GameController", "116:FizzySoda" };
            var game = GameList();
            var seen = new HashSet<PropKind>();
            var intros = new List<string>();
            for (int i = 0; i < game.Count; i++)
            {
                var fresh = new SortedSet<PropKind>();
                foreach (var prop in game.At(i).props)
                    if (seen.Add(UnlockReveal.Family(prop.kind))) fresh.Add(UnlockReveal.Family(prop.kind));
                if (fresh.Count > 0) intros.Add($"{i + 1}:{string.Join(",", fresh)}");
            }

            CollectionAssert.AreEqual(expected, intros);
        }
    }
}
