using System.Collections.Generic;
using System.Linq;
using Game.Levels;
using static Game.Editor.LayoutKit;

namespace Game.Editor
{
    /// <summary>Room 3 (Café) levels 63–75: the milk bottle arrives at 71; the finale at 75 mixes every Café kind.</summary>
    internal static partial class Room3Layouts
    {
        /// <summary>Donut towers on the back desk; donut towers on keycap plinths on the main desk.</summary>
        public static PropPlacement[] DonutTowersTwoDesks() =>
            DonutStack(4, -0.3f, 0f).Concat(DonutStack(4, 0.3f, 0f)).Select(OnBack)
                .Concat(new[] { -0.5f, 0.5f }.SelectMany(x => new[]
                    {
                        Cap(PropKind.Keycap2u, x, 0f, 0.12f, Look.Pink),
                        Cap(PropKind.Keycap2u, x, 0f, 0.28f, Look.Cream),
                    }.Concat(DonutStack(3, x, 0.2f, Keycap))))
                .ToArray();

        /// <summary>A long row of donut stacks near the back edge.</summary>
        public static PropPlacement[] DonutRow() =>
            new[] { -0.8f, -0.4f, 0f, 0.4f, 0.8f }.SelectMany(x => DonutStack(3, x, 0.35f)).ToArray();

        /// <summary>Plates and donuts on a book and on a keycap plinth, more on the back desk (Hard).</summary>
        public static PropPlacement[] DonutPyramidTwoDesks() =>
            new[] { Prop(PropKind.Book, -0.45f, 0f, 0.4f), Prop(PropKind.Plate, -0.45f, Book, 0.4f) }
                .Concat(DonutStack(2, -0.45f, 0.4f, Book + Plate))
                .Concat(Room3Layouts.PlinthTower(2, 0.45f, 0.4f))
                .Append(Prop(PropKind.Plate, 0.45f, 2f * Keycap, 0.4f))
                .Concat(DonutStack(2, 0.45f, 0.4f, 2f * Keycap + Plate))
                .Concat(DonutStack(2, 0f, 0f, Plate).Prepend(Prop(PropKind.Plate, 0f, 0f, 0f)).Select(OnBack))
                .ToArray();

        /// <summary>Three towers: a mug, a plate, a paper cup on top (a mug won't stand on a lid; taller towers never settled).</summary>
        public static PropPlacement[] CupMugTowers() =>
            new[] { -0.62f, 0f, 0.62f }.SelectMany((x, i) => new[]
                {
                    Prop(PropKind.Mug, x, 0f, 0.25f, i),
                    Prop(PropKind.Plate, x, Mug, 0.25f),
                    Prop(PropKind.PaperCup, x, Mug + Plate, 0.25f),
                })
                .ToArray();

        /// <summary>A cactus and a book shield three donut stacks at the back edge.</summary>
        public static PropPlacement[] DonutsBehindCactus() =>
            new[] { Prop(PropKind.CactusPot, -0.3f, 0f, 0.15f), Prop(PropKind.Book, 0.4f, 0f, 0.15f) }
                .Concat(new[] { -0.6f, 0f, 0.6f }.SelectMany(x => DonutStack(2, x, 0.55f)))
                .ToArray();

        /// <summary>Keycap walls on both desks, plates on top.</summary>
        public static PropPlacement[] PlateTopWalls() =>
            Wall(5, 2, 0f, 0f).Concat(new[] { -0.2f, 0.2f }.Select(x => Prop(PropKind.Plate, x, 2f * Keycap, 0f))).Select(OnBack)
                .Concat(new[] { -0.5f, 0.5f }.SelectMany(cx =>
                    Wall(3, 2, cx, 0.35f).Append(Prop(PropKind.Plate, cx, 2f * Keycap, 0.35f))))
                .ToArray();

        /// <summary>The café counter: a long mixed row of mugs, cups, plates and donuts, two deep.</summary>
        public static PropPlacement[] CafeCounter() =>
            new[]
                {
                    Prop(PropKind.Mug, -0.68f, 0f, -0.2f, 0),
                    Prop(PropKind.PaperCup, -0.2f, 0f, -0.2f),
                    Prop(PropKind.Mug, 0.28f, 0f, -0.2f, 1),
                    Prop(PropKind.PaperCup, 0.76f, 0f, -0.2f),
                    Prop(PropKind.PaperCup, -0.78f, 0f, 0.4f),
                    Prop(PropKind.Mug, 0.66f, 0f, 0.4f, 2),
                }
                .Concat(DonutStack(2, -0.36f, 0.4f, Plate).Prepend(Prop(PropKind.Plate, -0.36f, 0f, 0.4f)))
                .Concat(DonutStack(2, 0.1f, 0.4f, Plate).Prepend(Prop(PropKind.Plate, 0.1f, 0f, 0.4f)))
                .ToArray();

        /// <summary>A tall mixed café tower at the back (books, a plate, donuts), mugs on plates either side (Hard).</summary>
        public static PropPlacement[] CafeTower() =>
            Tower(PropKind.Book, 2, Book, 0f, 0.4f)
                .Append(Prop(PropKind.Plate, 0f, 2f * Book, 0.4f))
                .Concat(DonutStack(3, 0f, 0.4f, 2f * Book + Plate))
                .Concat(Room3Layouts.MugOnPlate(-0.65f, 0.1f, 0))
                .Concat(Room3Layouts.MugOnPlate(0.65f, 0.1f, 2))
                .ToArray();

        /// <summary>New: the milk bottle. Two staggered rows of bottles.</summary>
        public static PropPlacement[] MilkRow() =>
            new[] { -0.6f, -0.2f, 0.2f, 0.6f }.Select(x => Prop(PropKind.MilkBottle, x, 0f, 0.1f))
                .Concat(new[] { -0.4f, 0f, 0.4f }.Select(x => Prop(PropKind.MilkBottle, x, 0f, 0.5f)))
                .ToArray();

        /// <summary>Bottle dominoes: two lines of bottles running away from the player.</summary>
        public static PropPlacement[] MilkDominoes() =>
            new[] { -0.35f, 0.35f }.SelectMany(x =>
                    new[] { 0f, 0.3f, 0.6f }.Select(z => Prop(PropKind.MilkBottle, x, 0f, z)))
                .ToArray();

        /// <summary>Bottles on the back desk behind plate stacks on the main desk.</summary>
        public static PropPlacement[] MilkBehindPlates() =>
            new[] { -0.4f, 0f, 0.4f }.Select(x => OnBack(Prop(PropKind.MilkBottle, x, 0f, 0f)))
                .Concat(new[] { -0.5f, 0.5f }.SelectMany(x => Tower(PropKind.Plate, 2, Plate, x, 0.5f)))
                .ToArray();

        /// <summary>Top-heavy: milk bottles standing on book stacks.</summary>
        public static PropPlacement[] MilkOnBooks() =>
            new[] { -0.6f, 0f, 0.6f }.SelectMany((x, i) =>
                    Tower(PropKind.Book, i == 1 ? 2 : 1, Book, x, 0.25f)
                        .Append(Prop(PropKind.MilkBottle, x, (i == 1 ? 2 : 1) * Book, 0.25f)))
                .ToArray();

        /// <summary>The Café finale (Hard): every Café kind with earlier ones, across both desks.</summary>
        public static PropPlacement[] CafeFinale() =>
            new[]
                {
                    OnBack(Prop(PropKind.MilkBottle, -0.4f, 0f, 0f)),
                    OnBack(Prop(PropKind.PaperCup, 0.4f, 0f, 0f)),
                    OnBack(Prop(PropKind.Plate, 0f, 0f, 0f)),
                    OnBack(Prop(PropKind.Donut, 0f, Plate, 0f, 1)),
                    Prop(PropKind.MilkBottle, -0.6f, 0f, 0.5f),
                    Prop(PropKind.PaperCup, 0.6f, 0f, 0.5f),
                }
                .Concat(Room3Layouts.MugOnPlate(0f, 0.45f, 1))
                .Concat(DonutStack(2, -0.55f, -0.3f))
                .Concat(Wall(3, 1, 0.35f, -0.3f))
                .ToArray();
    }
}
