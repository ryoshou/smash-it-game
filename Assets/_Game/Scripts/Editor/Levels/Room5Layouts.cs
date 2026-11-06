using System.Collections.Generic;
using System.Linq;
using Game.Levels;
using static Game.Editor.LayoutKit;

namespace Game.Editor
{
    /// <summary>
    /// Room 5 (Gaming Den) layouts, levels 101–112: the soda can arrives at 101 and the game controller at 111. Every prop
    /// is a target (owner, checkpoint 2); every 5th level is Hard.
    /// </summary>
    internal static partial class Room5Layouts
    {
        private const float CanPitch = 0.2f;
        private static readonly float[] Thirds = { -0.6f, 0f, 0.6f };

        internal static PropPlacement Can(float x, float y, float z, int i) => Prop(PropKind.SodaCan, x, y, z, i % 3);

        /// <summary>A can pyramid, rows of count, count-1, ... 1, centred on x.</summary>
        internal static IEnumerable<PropPlacement> CanPyramid(int count, float x, float z, float y0 = 0f)
        {
            for (int row = 0; row < count; row++)
                for (int i = 0; i < count - row; i++)
                    yield return Can(x + (i - (count - 1 - row) / 2f) * CanPitch, y0 + row * SodaCan, z, i + row);
        }

        internal static IEnumerable<PropPlacement> CanTower(int count, float x, float z, float y0 = 0f) =>
            Enumerable.Range(0, count).Select(i => Can(x, y0 + i * SodaCan, z, i));

        /// <summary>New: the soda can. A 4-3-2-1 can pyramid set back from the edge.</summary>
        public static PropPlacement[] CanPyramid() => CanPyramid(4, 0f, 0.3f).ToArray();

        /// <summary>Two tall stacked can towers.</summary>
        public static PropPlacement[] CanTowers() => CanTower(4, -0.3f, 0.45f).Concat(CanTower(4, 0.3f, 0.45f)).ToArray();

        /// <summary>Cans standing on top of two keycap walls.</summary>
        public static PropPlacement[] CansOnWalls() =>
            new[] { -0.5f, 0.5f }.SelectMany(x =>
                    Wall(4, 2, x, 0.4f).Append(Can(x - 0.12f, 2f * Keycap, 0.4f, 0)).Append(Can(x + 0.12f, 2f * Keycap, 0.4f, 1)))
                .ToArray();

        /// <summary>Can pyramids on both desks.</summary>
        public static PropPlacement[] PyramidsTwoDesks() =>
            CanPyramid(3, 0f, 0.1f).Select(OnBack).Concat(CanPyramid(3, 0f, 0.45f)).ToArray();

        /// <summary>Three desk lamps shield two can pyramids at the back, few shots (Hard).</summary>
        public static PropPlacement[] LampShieldCans() =>
            new[] { -0.55f, 0f, 0.55f }.Select(x => Prop(PropKind.DeskLamp, x, 0f, -0.1f))
                .Concat(CanPyramid(3, -0.5f, 0.5f)).Concat(CanPyramid(3, 0.5f, 0.5f))
                .ToArray();

        /// <summary>The keyboard: a wide, low block of keycaps, two walls deep.</summary>
        public static PropPlacement[] Keyboard() => Wall(8, 2, 0f, 0.3f).Concat(Wall(8, 2, 0f, 0.45f)).ToArray();

        /// <summary>The snack desk: cans, paper cups and donut stacks.</summary>
        public static PropPlacement[] SnackDesk() => new[]
            {
                Can(-0.75f, 0f, 0.45f, 0),
                Can(-0.75f, SodaCan, 0.45f, 1),
                Prop(PropKind.PaperCup, -0.35f, 0f, 0.45f),
                Can(0.33f, 0f, 0.45f, 2),
                Prop(PropKind.PaperCup, 0.75f, 0f, 0.45f),
                Can(-0.2f, 0f, 0.05f, 1),
            }
            .Concat(DonutStack(2, 0.05f, 0.45f))
            .Concat(DonutStack(2, 0.45f, 0.05f))
            .ToArray();

        /// <summary>A keycap wall on the back desk, cans in front.</summary>
        public static PropPlacement[] BackWallFrontCans() =>
            Wall(5, 2, 0f, 0.1f).Select(OnBack)
                .Concat(new[] { -0.6f, 0f, 0.6f }.Select((x, i) => Can(x, 0f, 0.45f, i)))
                .Concat(CanTower(2, -0.3f, 0.1f)).Concat(CanTower(2, 0.3f, 0.1f))
                .ToArray();

        /// <summary>Two spacebar decks on keycap legs, two cans on each.</summary>
        public static PropPlacement[] CanBridges() =>
            new[] { -0.42f, 0.42f }.SelectMany(x => DeckOnLegs(x, 0.3f)
                    .Append(Can(x - 0.15f, 2f * Keycap, 0.3f, 0)).Append(Can(x + 0.15f, 2f * Keycap, 0.3f, 1)))
                .ToArray();

        /// <summary>Can towers on books on the main desk, a book with cans on the back desk (Hard).</summary>
        public static PropPlacement[] TowersOnBooks() =>
            // Books outside the back desk's span (x ±0.6): pushed straight back, a book jams under its top.
            new[] { -0.74f, 0.74f }.SelectMany(x => CanTower(3, x, 0.45f, Book).Prepend(Prop(PropKind.Book, x, 0f, 0.45f)))
                .Concat(new[] { Can(-0.15f, 0f, 0.15f, 0), Can(0.15f, 0f, 0.15f, 1) }.Select(OnBack))
                .ToArray();

        /// <summary>New: the game controller. Controllers lying on three keycap plinths.</summary>
        public static PropPlacement[] ControllersOnPlinths() =>
            Thirds.SelectMany((x, i) => Plinth(2, x, 0.4f)
                    .Append(Prop(PropKind.GameController, x, 2f * Keycap, 0.4f, i))
                    .Append(Can(x, 2f * Keycap + Controller, 0.4f, i)))
                .ToArray();

        /// <summary>Stacked controllers in front shield a row of cans.</summary>
        public static PropPlacement[] ControllerShield() =>
            new[] { -0.4f, 0.4f }.SelectMany((x, i) => new[]
                {
                    Prop(PropKind.GameController, x, 0f, 0.05f, i),
                    Prop(PropKind.GameController, x, Controller, 0.05f, i + 1),
                })
                .Concat(new[] { -0.6f, -0.2f, 0.2f, 0.6f }.Select((x, i) => Can(x, 0f, 0.5f, i)))
                .Concat(new[] { -0.2f, 0.6f }.Select((x, i) => Can(x, SodaCan, 0.5f, i)))
                .ToArray();

        /// <summary>rows of 2u keycap pairs, two deep (0.31 x 0.27): steady enough to lay a controller on.</summary>
        internal static IEnumerable<PropPlacement> Plinth(int rows, float x, float z, float y0 = 0f)
        {
            for (int row = 0; row < rows; row++)
            {
                yield return Cap(PropKind.Keycap2u, x, y0 + row * Keycap, z - 0.08f, (Look)(row % 3));
                yield return Cap(PropKind.Keycap2u, x, y0 + row * Keycap, z + 0.08f, (Look)((row + 1) % 3));
            }
        }

        internal static IEnumerable<PropPlacement> DeckOnLegs(float x, float z)
        {
            foreach (float legX in new[] { x - 0.24f, x + 0.24f })
                foreach (float legZ in new[] { z - DeckDepth, z + DeckDepth })
                    yield return Cap(PropKind.Keycap1u, legX, 0f, legZ, Look.Pink);
            foreach (var deck in Bridge(x, Keycap, z)) yield return deck;
        }
    }
}
