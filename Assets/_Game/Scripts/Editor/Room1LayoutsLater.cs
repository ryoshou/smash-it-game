using System.Collections.Generic;
using System.Linq;
using Game.Levels;
using static Game.Editor.LayoutKit;

namespace Game.Editor
{
    /// <summary>Room 1 levels 11–25: the paper cup arrives at 12, the desk lamp at 21; every 5th level is Hard.</summary>
    internal static partial class Room1Layouts
    {
        /// <summary>A pyramid of 2u keycaps, 5 rows.</summary>
        public static PropPlacement[] WidePyramid()
        {
            var props = new List<PropPlacement>();
            for (int row = 0; row < 5; row++)
                props.AddRange(Row(PropKind.Keycap2u, 5 - row, 0f, WidePitch, row * Keycap, 0.2f, row));
            return props.ToArray();
        }

        /// <summary>New: the paper cup. A pyramid of light cups that scatter on a hit.</summary>
        public static PropPlacement[] CupTower()
        {
            var props = new List<PropPlacement>();
            const float cupPitch = 0.36f;
            for (int row = 0; row < 4; row++)
                for (int i = 0; i < 4 - row; i++)
                    props.Add(Prop(PropKind.PaperCup, (i - (3 - row) / 2f) * cupPitch, row * Cup, 0.2f));
            return props.ToArray();
        }

        /// <summary>Two spacebar decks on 1u keycap legs, a cup standing on each.</summary>
        public static PropPlacement[] CupBridges() => BridgeWithCup(-0.42f, 0.1f).Concat(BridgeWithCup(0.42f, 0.1f)).ToArray();

        private static IEnumerable<PropPlacement> BridgeWithCup(float x, float z)
        {
            foreach (float legX in new[] { x - 0.24f, x + 0.24f })
                foreach (float legZ in new[] { z - DeckDepth, z + DeckDepth })
                    yield return Cap(PropKind.Keycap1u, legX, 0f, legZ, Look.Pink);
            foreach (var deck in Bridge(x, Keycap, z)) yield return deck;
            yield return Prop(PropKind.PaperCup, x, 2f * Keycap, z);
        }

        /// <summary>Books on both desks: keycaps on a book up on the back desk, a book at each side of the main desk.</summary>
        public static PropPlacement[] BackDeskBooks() =>
            new[]
            {
                Prop(PropKind.Book, 0.1f, 0f, 0f),
                Cap(PropKind.Keycap1u, -0.06f, Book, 0f, Look.Pink),
                Cap(PropKind.Keycap1u, 0.1f, Book, 0f, Look.Sky),
                Cap(PropKind.Keycap1u, 0.26f, Book, 0f, Look.Cream),
            }
            .Select(OnBack)
            .Append(Prop(PropKind.Book, -0.7f, 0f, 0f))
            .Append(Prop(PropKind.Book, 0.7f, 0f, 0.2f))
            .ToArray();

        /// <summary>A book tower with a cup on top, a heavy cactus at the back corner, keycap towers either side (Hard).</summary>
        public static PropPlacement[] TowerBehindCactus() =>
            new[] { Prop(PropKind.CactusPot, -0.6f, 0f, 0.45f) }
                .Concat(Tower(PropKind.Book, 2, Book, 0f, 0.35f))
                .Append(Prop(PropKind.PaperCup, 0f, 2f * Book, 0.35f))
                .Concat(Tower(PropKind.Keycap1u, 2, Keycap, 0.45f, 0.35f))
                .Concat(Tower(PropKind.Keycap1u, 2, Keycap, 0.65f, 0.1f))
                .ToArray();

        /// <summary>Café: mugs and cups in two staggered rows, two cups stacked.</summary>
        public static PropPlacement[] Cafe() => new[]
        {
            Prop(PropKind.Mug, -0.55f, 0f, -0.15f, 0),
            Prop(PropKind.PaperCup, 0f, 0f, -0.15f),
            Prop(PropKind.PaperCup, 0f, Cup, -0.15f),
            Prop(PropKind.Mug, 0.55f, 0f, -0.15f, 2),
            Prop(PropKind.PaperCup, -0.55f, 0f, 0.4f),
            Prop(PropKind.PaperCup, -0.55f, Cup, 0.4f),
            Prop(PropKind.Mug, 0f, 0f, 0.4f, 1),
            Prop(PropKind.PaperCup, 0.55f, 0f, 0.4f),
            Prop(PropKind.PaperCup, 0.55f, Cup, 0.4f),
            Prop(PropKind.PaperCup, 0.55f, 2f * Cup, 0.4f),
        };

        /// <summary>A spacebar deck across two book stacks, two cups on it.</summary>
        public static PropPlacement[] BookBridge() =>
            Tower(PropKind.Book, 2, Book, -0.3f, 0.2f)
                .Concat(Tower(PropKind.Book, 2, Book, 0.3f, 0.2f))
                .Concat(Bridge(0f, 2f * Book, 0.2f))
                .Append(Prop(PropKind.PaperCup, -0.18f, 2f * Book + Keycap, 0.2f))
                .Append(Prop(PropKind.PaperCup, 0.18f, 2f * Book + Keycap, 0.2f))
                .ToArray();

        /// <summary>A 3-row keycap wall on the back desk, a row of cups on the main desk.</summary>
        public static PropPlacement[] BackWallFrontCups() =>
            Wall(5, 3, 0f, 0f).Select(OnBack)
                .Concat(new[] { -0.6f, -0.2f, 0.2f, 0.6f }.Select(x => Prop(PropKind.PaperCup, x, 0f, -0.3f)))
                .ToArray();

        /// <summary>Fortress: 2u and 1u keycap walls around a cactus.</summary>
        public static PropPlacement[] Fortress()
        {
            var props = new List<PropPlacement> { Prop(PropKind.CactusPot, 0f, 0f, 0.2f) };
            for (int row = 0; row < 2; row++)
            {
                props.AddRange(Row(PropKind.Keycap2u, 3, 0f, WidePitch, row * Keycap, -0.15f, row));
                foreach (float x in new[] { -0.42f, 0.42f })
                    for (int i = 0; i < 3; i++)
                        props.Add(Cap(PropKind.Keycap1u, x, row * Keycap, 0.05f + i * Pitch, (Look)((i + row) % 3), 90f));
            }
            return props.ToArray();
        }

        /// <summary>Everything so far, across two desks (Hard).</summary>
        public static PropPlacement[] EverythingTwoDesks() => new[]
            {
                OnBack(Prop(PropKind.Mug, 0.3f, 0f, 0f, 0)),
                OnBack(Prop(PropKind.PaperCup, -0.2f, 0f, 0f)),
                Prop(PropKind.Book, -0.55f, 0f, 0.4f),
                Prop(PropKind.Mug, -0.55f, Book, 0.4f, 1),
                Prop(PropKind.CactusPot, 0.6f, 0f, 0.45f),
                Prop(PropKind.PaperCup, 0.6f, 0f, -0.3f),
            }
            .Concat(Wall(3, 2, 0f, 0.1f))
            .ToArray();

        /// <summary>New: the desk lamp, ringed by keycaps.</summary>
        public static PropPlacement[] LampRing() =>
            new[] { Prop(PropKind.DeskLamp, 0f, 0f, 0.2f) }
                .Concat(Row(PropKind.Keycap1u, 5, 0f, Pitch, 0f, -0.15f))
                .Concat(new[] { -0.35f, 0.35f }.SelectMany(x => new[]
                {
                    Cap(PropKind.Keycap1u, x, 0f, 0.1f, Look.Sky, 90f),
                    Cap(PropKind.Keycap1u, x, 0f, 0.3f, Look.Pink, 90f),
                }))
                .ToArray();

        /// <summary>Lamps standing on books: top-heavy.</summary>
        public static PropPlacement[] LampsOnBooks() =>
            Tower(PropKind.Book, 2, Book, -0.4f, 0.2f)
                .Append(Prop(PropKind.DeskLamp, -0.4f, 2f * Book, 0.2f))
                .Append(Prop(PropKind.Book, 0.4f, 0f, 0.2f))
                .Append(Prop(PropKind.DeskLamp, 0.4f, Book, 0.2f))
                .ToArray();

        /// <summary>Three lamps in front shield three mugs; two cups at the back corners.</summary>
        public static PropPlacement[] LampShield() =>
            new[] { -0.5f, 0f, 0.5f }.Select(x => Prop(PropKind.DeskLamp, x, 0f, -0.3f))
                .Concat(new[] { -0.6f, 0f, 0.6f }.Select((x, i) => Prop(PropKind.Mug, x, 0f, 0.35f, i)))
                .Append(Prop(PropKind.PaperCup, -0.8f, 0f, 0.6f))
                .Append(Prop(PropKind.PaperCup, 0.8f, 0f, 0.6f))
                .ToArray();

        /// <summary>A lamp and stacked cups on the back desk, cups and a keycap block up front.</summary>
        public static PropPlacement[] LampAcrossDesks() =>
            new[]
            {
                OnBack(Prop(PropKind.DeskLamp, 0.3f, 0f, 0f)),
                OnBack(Prop(PropKind.PaperCup, -0.3f, 0f, 0f)),
                OnBack(Prop(PropKind.PaperCup, -0.3f, Cup, 0f)),
            }
            .Concat(new[] { -0.5f, 0f, 0.5f }.Select(x => Prop(PropKind.PaperCup, x, 0f, -0.2f)))
            .Concat(Wall(2, 2, 0f, 0.35f))
            .ToArray();

        /// <summary>The finale (Hard): a mixed pile across both desks.</summary>
        public static PropPlacement[] Finale() =>
            new[]
            {
                OnBack(Prop(PropKind.Book, -0.3f, 0f, 0f)),
                OnBack(Cap(PropKind.Keycap1u, -0.38f, Book, 0f, Look.Cream)),
                OnBack(Cap(PropKind.Keycap1u, -0.22f, Book, 0f, Look.Pink)),
                OnBack(Prop(PropKind.DeskLamp, 0.3f, 0f, 0f)),
                Prop(PropKind.Book, -0.6f, 0f, 0.45f),
                Prop(PropKind.Mug, -0.6f, Book, 0.45f, 2),
                Prop(PropKind.CactusPot, 0.6f, 0f, 0.45f),
                Prop(PropKind.PaperCup, -0.4f, 0f, -0.3f),
                Prop(PropKind.PaperCup, 0.45f, 0f, -0.3f),
            }
            .Concat(Wall(3, 1, 0f, 0.1f))
            .ToArray();
    }
}
