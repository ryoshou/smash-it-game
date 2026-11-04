using System.Collections.Generic;
using System.Linq;
using Game.Levels;
using static Game.Editor.LayoutKit;

namespace Game.Editor
{
    /// <summary>
    /// Room 3 (Library) layouts, levels 51–62: the candle arrives at 51 and the hourglass at 61. Every prop is a target
    /// (owner, checkpoint 2); every 5th level is Hard. Heavy books sit near the back edge, about one per shot.
    /// </summary>
    internal static partial class Room3Layouts
    {
        private static readonly float[] Thirds = { -0.6f, 0f, 0.6f };

        /// <summary>A candle, alternating thick and thin, standing on whatever is below.</summary>
        internal static PropPlacement Candle(float x, float y, float z, int i) => Prop(PropKind.Candle, x, y, z, i % 2);

        /// <summary>New: the candle. A row of tall candles near the back edge.</summary>
        public static PropPlacement[] CandleRow() =>
            Enumerable.Range(0, 7).Select(i => Candle(-0.75f + i * 0.25f, 0f, 0.45f, i)).ToArray();

        /// <summary>Two candles on each of three books.</summary>
        public static PropPlacement[] CandlesOnBooks() =>
            Thirds.SelectMany((x, i) => new[]
                {
                    Prop(PropKind.Book, x, 0f, 0.45f),
                    Candle(x - 0.12f, Book, 0.45f, i),
                    Candle(x + 0.12f, Book, 0.45f, i + 1),
                })
                .ToArray();

        /// <summary>A book staircase, one, two and three high, a candle on each step.</summary>
        public static PropPlacement[] BookStaircase() =>
            new[] { -0.55f, 0f, 0.55f }.SelectMany((x, i) =>
                    Tower(PropKind.Book, i + 1, Book, x, 0.5f).Append(Candle(x, (i + 1) * Book, 0.5f, i)))
                .ToArray();

        /// <summary>Walls on both desks: a keycap wall on the back desk, a wall of books near the main desk's back edge.</summary>
        public static PropPlacement[] BookWalls() =>
            Wall(4, 2, 0f, 0.1f).Select(OnBack)
                // Outside the back desk's span (x ±0.6): a book pushed straight back would jam under its top.
                .Concat(new[] { -0.74f, 0.74f }.Select(x => Prop(PropKind.Book, x, 0f, 0.4f)))
                .ToArray();

        /// <summary>Lob (Hard): two heavy books in front shield a row of candles at the back edge.</summary>
        public static PropPlacement[] HeavyLob() =>
            new[] { -0.35f, 0.35f }.Select(x => Prop(PropKind.Book, x, 0f, 0.05f))
                .Concat(Enumerable.Range(0, 7).Select(i => Candle(-0.75f + i * 0.25f, 0f, 0.55f, i)))
                .ToArray();

        /// <summary>Bookends: a desk lamp at each end of a book row.</summary>
        public static PropPlacement[] Bookends() => new[]
        {
            Prop(PropKind.DeskLamp, -0.75f, 0f, 0.45f),
            Prop(PropKind.Book, -0.25f, 0f, 0.45f),
            Candle(-0.25f, Book, 0.45f, 0),
            Prop(PropKind.Book, 0.25f, 0f, 0.45f),
            Candle(0.25f, Book, 0.45f, 1),
            Prop(PropKind.DeskLamp, 0.75f, 0f, 0.45f),
        };

        /// <summary>A reading nook: mugs and candles around a cactus.</summary>
        public static PropPlacement[] ReadingNook() => new[]
        {
            Prop(PropKind.CactusPot, 0f, 0f, 0.4f),
            Prop(PropKind.Mug, -0.65f, 0f, 0.4f, 0),
            Prop(PropKind.Mug, 0.65f, 0f, 0.4f, 1),
            Candle(-0.35f, 0f, -0.05f, 0),
            Candle(0.35f, 0f, -0.05f, 1),
            Candle(-0.75f, 0f, -0.3f, 1),
            Candle(0.75f, 0f, -0.3f, 0),
        };

        /// <summary>Candle rows on both desks.</summary>
        public static PropPlacement[] CandleRowsTwoDesks() =>
            Enumerable.Range(0, 3).Select(i => OnBack(Candle(-0.3f + i * 0.3f, 0f, 0.15f, i)))
                .Concat(Enumerable.Range(0, 5).Select(i => Candle(-0.6f + i * 0.3f, 0f, 0.45f, i + 1)))
                .ToArray();

        /// <summary>Keycap walls framing a two-book stack.</summary>
        public static PropPlacement[] FramedBooks() =>
            Wall(2, 3, -0.7f, 0.4f).Concat(Wall(2, 3, 0.7f, 0.4f)).Concat(Tower(PropKind.Book, 2, Book, 0f, 0.45f)).ToArray();

        /// <summary>Book, keycap and candle towers on the main desk; a book with candles on the back desk (Hard).</summary>
        public static PropPlacement[] TowersTwoDesks() =>
            Tower(PropKind.Book, 2, Book, 0f, 0.45f)
                .Concat(Row(PropKind.Keycap1u, 3, 0f, Pitch, 2f * Book, 0.45f))
                .Append(Candle(0f, 2f * Book + Keycap, 0.45f, 0))
                .Append(Candle(-0.6f, 0f, 0.45f, 1))
                .Append(Candle(0.6f, 0f, 0.45f, 0))
                .Concat(new[] { Candle(-0.15f, 0f, 0.15f, 0), Candle(0.15f, 0f, 0.15f, 1) }.Select(OnBack))
                .ToArray();

        /// <summary>New: the hourglass. Hourglasses on books, one on the desk.</summary>
        public static PropPlacement[] HourglassesOnBooks() => new[]
        {
            Prop(PropKind.Book, -0.55f, 0f, 0.45f),
            Prop(PropKind.Hourglass, -0.55f, Book, 0.45f),
            Prop(PropKind.Hourglass, 0f, 0f, 0.5f),
            Prop(PropKind.Book, 0.55f, 0f, 0.45f),
            Prop(PropKind.Hourglass, 0.55f, Book, 0.45f),
        };

        /// <summary>Hourglasses on two spacebar decks.</summary>
        public static PropPlacement[] HourglassBridges() =>
            new[] { -0.42f, 0.42f }.SelectMany(x => DeckOnLegs(x, 0.3f).Append(Prop(PropKind.Hourglass, x, 2f * Keycap, 0.3f))).ToArray();

        internal static IEnumerable<PropPlacement> DeckOnLegs(float x, float z)
        {
            foreach (float legX in new[] { x - 0.24f, x + 0.24f })
                foreach (float legZ in new[] { z - DeckDepth, z + DeckDepth })
                    yield return Cap(PropKind.Keycap1u, legX, 0f, legZ, Look.Pink);
            foreach (var deck in Bridge(x, Keycap, z)) yield return deck;
        }
    }
}
