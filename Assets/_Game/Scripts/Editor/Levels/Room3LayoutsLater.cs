using System.Linq;
using Game.Levels;
using static Game.Editor.LayoutKit;

namespace Game.Editor
{
    /// <summary>Room 3 (Library) levels 63–75: the pencil cup arrives at 71; the finale at 75 mixes every Library kind.</summary>
    internal static partial class Room3Layouts
    {
        /// <summary>A pair of desk lamps shields three hourglasses at the back edge.</summary>
        public static PropPlacement[] LampShield() =>
            new[] { -0.35f, 0.35f }.Select(x => Prop(PropKind.DeskLamp, x, 0f, 0f))
                .Concat(Thirds.Select(x => Prop(PropKind.Hourglass, x, 0f, 0.5f)))
                .Append(Candle(-0.85f, 0f, 0.6f, 0))
                .Append(Candle(0.85f, 0f, 0.6f, 1))
                .ToArray();

        /// <summary>Candles on the back desk; hourglasses and a book in front (back-desk hourglasses never came off).</summary>
        public static PropPlacement[] BackHourglasses() =>
            new[] { -0.2f, 0.2f }.Select((x, i) => OnBack(Candle(x, 0f, 0.15f, i)))
                .Concat(new[] { -0.8f, 0.8f }.Select(x => Prop(PropKind.Hourglass, x, 0f, 0.4f)))
                .Append(Candle(-0.3f, 0f, 0.3f, 0))
                .Append(Candle(0.3f, 0f, 0.3f, 1))
                .ToArray();

        /// <summary>An hourglass on a tall book tower, two more at the back corners (Hard).</summary>
        public static PropPlacement[] HourglassTower() =>
            Tower(PropKind.Book, 3, Book, 0f, 0.45f)
                .Append(Prop(PropKind.Hourglass, 0f, 3f * Book, 0.45f))
                .Append(Prop(PropKind.Hourglass, -0.65f, 0f, 0.5f))
                .Append(Prop(PropKind.Hourglass, 0.65f, 0f, 0.5f))
                .Append(Candle(-0.8f, 0f, -0.35f, 0))
                .Append(Candle(0.8f, 0f, -0.35f, 1))
                .ToArray();

        /// <summary>The study desk: books with candles, a mug, candles at the front.</summary>
        public static PropPlacement[] StudyDesk() => new[]
        {
            Prop(PropKind.Book, -0.55f, 0f, 0.45f),
            Candle(-0.55f, Book, 0.45f, 0),
            Prop(PropKind.Mug, 0f, 0f, 0.45f, 1),
            Prop(PropKind.Book, 0.55f, 0f, 0.45f),
            Candle(0.55f, Book, 0.45f, 1),
            Candle(-0.3f, 0f, -0.2f, 1),
            Candle(0.3f, 0f, -0.2f, 0),
        };

        /// <summary>A long row of hourglasses and candles, alternating.</summary>
        public static PropPlacement[] HourglassCandleRow() =>
            Enumerable.Range(0, 7).Select(i => i % 2 == 0
                    ? Prop(PropKind.Hourglass, -0.8f + i * 0.27f, 0f, 0.45f)
                    : Candle(-0.8f + i * 0.27f, 0f, 0.45f, i / 2))
                .ToArray();

        /// <summary>A desk lamp and an hourglass on each desk, two more hourglasses up front.</summary>
        public static PropPlacement[] LampsAndHourglasses() =>
            new[] { OnBack(Prop(PropKind.DeskLamp, 0.3f, 0f, 0.1f)) }
                .Append(Prop(PropKind.DeskLamp, -0.55f, 0f, 0.45f))
                .Append(Prop(PropKind.Hourglass, 0.05f, 0f, 0.5f))
                .Append(Prop(PropKind.Hourglass, 0.6f, 0f, 0.5f))
                .Append(Candle(-0.88f, 0f, 0.6f, 0))
                .Append(Candle(0.88f, 0f, 0.6f, 1))
                .ToArray();

        /// <summary>Everything Library across the full width.</summary>
        public static PropPlacement[] LibraryWidth() => new[]
        {
            Prop(PropKind.DeskLamp, -0.72f, 0f, 0.45f),
            Prop(PropKind.Book, -0.2f, 0f, 0.45f),
            Candle(-0.2f, Book, 0.45f, 0),
            Prop(PropKind.Hourglass, 0.25f, 0f, 0.5f),
            Candle(0.55f, 0f, 0.5f, 1),
            Candle(0.8f, 0f, 0.3f, 0),
            Candle(-0.45f, 0f, -0.15f, 1),
        };

        /// <summary>Books shield hourglasses at both sides; a candle on the back desk (Hard).</summary>
        public static PropPlacement[] BookFortress() =>
            new[] { -0.72f, 0.72f }.Select(x => Prop(PropKind.Book, x, 0f, 0.05f))
                .Concat(new[] { -0.78f, 0.78f }.Select(x => Prop(PropKind.Hourglass, x, 0f, 0.5f)))
                .Append(OnBack(Candle(0f, 0f, 0.15f, 0)))
                .ToArray();

        /// <summary>New: the pencil cup. A row of pencil cups near the back edge.</summary>
        public static PropPlacement[] PencilCupRow() =>
            Enumerable.Range(0, 5).Select(i => Prop(PropKind.PencilCup, -0.7f + i * 0.35f, 0f, 0.45f, i % 3)).ToArray();

        /// <summary>Pencil cups on books.</summary>
        public static PropPlacement[] PencilCupsOnBooks() =>
            Thirds.SelectMany((x, i) => new[] { Prop(PropKind.Book, x, 0f, 0.45f), Prop(PropKind.PencilCup, x, Book, 0.45f, i) })
                .Append(Candle(-0.85f, 0f, 0.6f, 0))
                .Append(Candle(0.85f, 0f, 0.6f, 1))
                .ToArray();

        /// <summary>Pencil cups and candles, staggered in two rows.</summary>
        public static PropPlacement[] CupsAndCandles() =>
            Thirds.Select((x, i) => Prop(PropKind.PencilCup, x, 0f, 0.05f, i))
                .Concat(new[] { -0.75f, -0.3f, 0.3f, 0.75f }.Select((x, i) => Candle(x, 0f, 0.5f, i)))
                .ToArray();

        /// <summary>Pencil cups on the back desk, a keycap wall in front.</summary>
        public static PropPlacement[] BackCupsFrontWall() =>
            new[] { -0.4f, 0f, 0.4f }.Select((x, i) => OnBack(Prop(PropKind.PencilCup, x, 0f, 0f, i)))
                .Concat(Wall(5, 2, 0f, 0.1f))
                .ToArray();

        /// <summary>The Library finale (Hard): every Library kind with earlier ones, across both desks.</summary>
        public static PropPlacement[] LibraryFinale() =>
            new[]
                {
                    OnBack(Prop(PropKind.PencilCup, -0.2f, 0f, 0.15f, 1)),
                    OnBack(Candle(0.2f, 0f, 0.15f, 0)),
                    Prop(PropKind.Book, -0.6f, 0f, 0.45f),
                    Candle(-0.6f, Book, 0.45f, 1),
                    Prop(PropKind.Hourglass, 0.6f, 0f, 0.5f),
                    Prop(PropKind.PencilCup, -0.3f, 0f, -0.25f, 0),
                    Prop(PropKind.PencilCup, 0.75f, 0f, -0.25f, 2),
                }
                .Concat(Wall(3, 1, 0.25f, -0.25f))
                .ToArray();
    }
}
