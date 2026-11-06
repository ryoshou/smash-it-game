using System.Linq;
using Game.Levels;
using static Game.Editor.LayoutKit;

namespace Game.Editor
{
    /// <summary>
    /// Room 5 (Gaming Den) levels 113–125: the fizzy soda arrives at 116 (it bursts when knocked over, spec §6.2); level
    /// 124 revisits every room, and the grand finale at 125 mixes everything.
    /// </summary>
    internal static partial class Room5Layouts
    {
        internal static PropPlacement Fizzy(float x, float y, float z) => Prop(PropKind.FizzySoda, x, y, z);

        /// <summary>Controllers carrying cans on both desks.</summary>
        public static PropPlacement[] ControllersTwoDesks() =>
            new[] { Prop(PropKind.GameController, 0f, 0f, 0.1f, 0), Can(0f, Controller, 0.1f, 0) }.Select(OnBack)
                .Concat(new[] { -0.55f, 0.55f }.SelectMany((x, i) => new[]
                    {
                        Prop(PropKind.GameController, x, 0f, 0.45f, i + 1),
                        Can(x - 0.1f, Controller, 0.45f, i),
                        Can(x + 0.1f, Controller, 0.45f, i + 1),
                    }))
                .ToArray();

        /// <summary>Controllers on a plate and on books, cans between.</summary>
        public static PropPlacement[] ControllersOnPlatesAndBooks() => new[]
        {
            Prop(PropKind.Book, -0.6f, 0f, 0.45f),
            Prop(PropKind.GameController, -0.6f, Book, 0.45f, 0),
            Prop(PropKind.Plate, 0f, 0f, 0.45f),
            Prop(PropKind.GameController, 0f, Plate, 0.45f, 1),
            Prop(PropKind.Book, 0.6f, 0f, 0.45f),
            Prop(PropKind.GameController, 0.6f, Book, 0.45f, 2),
            Can(-0.3f, 0f, 0f, 0),
            Can(0.3f, 0f, 0f, 1),
        };

        /// <summary>A wide Den spread: controllers, cans and keycaps across the desk (Hard).</summary>
        public static PropPlacement[] WideDenSpread() =>
            new[]
                {
                    Prop(PropKind.GameController, -0.7f, 0f, 0.5f, 0),
                    Prop(PropKind.GameController, 0.7f, 0f, 0.5f, 2),
                    Prop(PropKind.GameController, 0f, 0f, -0.4f, 1),
                    Can(-0.7f, 0f, -0.35f, 0),
                    Can(0.7f, 0f, -0.35f, 1),
                }
                .Concat(CanTower(3, 0f, 0.5f))
                .Concat(Wall(3, 2, -0.35f, 0.1f))
                .Concat(Wall(3, 2, 0.35f, 0.1f))
                .ToArray();

        /// <summary>New: the fizzy soda. A fizzy crowning a can pyramid (its domed lid can't carry a can).</summary>
        public static PropPlacement[] FizzyInPyramid() =>
            CanPyramid(3, 0f, 0.45f).Where(p => p.basePosition.y < 1.5f * SodaCan)
                .Append(Fizzy(0f, 2f * SodaCan, 0.45f))
                .Concat(CanTower(2, -0.75f, 0.45f))
                .Concat(CanTower(2, 0.75f, 0.45f))
                .ToArray();

        /// <summary>A fizzy chain: a row of fizzies close enough to set each other off, keycaps between.</summary>
        public static PropPlacement[] FizzyChain() =>
            new[] { -0.7f, -0.35f, 0f, 0.35f, 0.7f }.Select(x => Fizzy(x, 0f, 0.45f))
                .Concat(Row(PropKind.Keycap2u, 3, 0f, 0.6f, 0f, 0.1f))
                .ToArray();

        /// <summary>A fizzy inside a keycap fortress.</summary>
        public static PropPlacement[] FizzyFortress()
        {
            var props = new System.Collections.Generic.List<PropPlacement> { Fizzy(0f, 0f, 0.3f) };
            for (int row = 0; row < 2; row++)
            {
                props.AddRange(Row(PropKind.Keycap2u, 3, 0f, WidePitch, row * Keycap, 0.05f, row));
                foreach (float x in new[] { -0.42f, 0.42f })
                    for (int i = 0; i < 3; i++)
                        props.Add(Cap(PropKind.Keycap1u, x, row * Keycap, 0.2f + i * Pitch, (Look)((i + row) % 3), 90f));
            }
            return props.ToArray();
        }

        /// <summary>A fizzy among cans on the back desk, a can row up front.</summary>
        public static PropPlacement[] BackDeskFizzy() =>
            new[] { Fizzy(0f, 0f, 0.1f), Can(-0.25f, 0f, 0.1f, 0), Can(0.25f, 0f, 0.1f, 1), Can(-0.45f, 0f, 0.1f, 2), Can(0.45f, 0f, 0.1f, 0) }
                .Select(OnBack)
                .Concat(new[] { -0.6f, -0.2f, 0.2f, 0.6f }.Select((x, i) => Can(x, 0f, 0.4f, i)))
                .ToArray();

        /// <summary>Fizzies buried behind a book wall, with cans (Hard).</summary>
        public static PropPlacement[] FizzyBookFortress() =>
            new[] { -0.55f, 0f, 0.55f }.Select(x => Prop(PropKind.Book, x, 0f, 0.1f))
                .Concat(new[] { -0.35f, 0.35f }.Select(x => Fizzy(x, 0f, 0.5f)))
                .Concat(new[] { -0.75f, 0f, 0.75f }.Select((x, i) => Can(x, 0f, 0.55f, i)))
                .ToArray();

        /// <summary>A fizzy beside a desk lamp, mugs around them.</summary>
        public static PropPlacement[] FizzyByTheLamp() => new[]
        {
            Prop(PropKind.DeskLamp, -0.15f, 0f, 0.4f),
            Fizzy(0.25f, 0f, 0.4f),
            Prop(PropKind.Mug, -0.7f, 0f, 0.3f, 0),
            Prop(PropKind.Mug, 0.7f, 0f, 0.3f, 1),
            Prop(PropKind.Mug, 0f, 0f, -0.25f, 2),
            Can(-0.85f, 0f, 0.62f, 0),
            Can(0.85f, 0f, 0.62f, 1),
            Prop(PropKind.GameController, -0.78f, 0f, 0.0f, 0),
            Prop(PropKind.GameController, 0.78f, 0f, 0.0f, 2),
        };

        /// <summary>Everything Den: cans, controllers, a fizzy and keycaps.</summary>
        public static PropPlacement[] EverythingDen() =>
            new[]
                {
                    Prop(PropKind.GameController, -0.6f, 0f, 0.45f, 0),
                    Can(-0.6f, Controller, 0.45f, 0),
                    Fizzy(0f, 0f, 0.5f),
                    Prop(PropKind.GameController, 0.6f, 0f, 0.45f, 2),
                    Can(0.6f, Controller, 0.45f, 1),
                }
                .Concat(CanPyramid(2, 0.3f, 0.05f))
                .Concat(Wall(3, 2, -0.4f, 0.05f))
                .ToArray();

        /// <summary>A fizzy chain across both desks, cans around each fizzy.</summary>
        public static PropPlacement[] FizzyChainTwoDesks() =>
            new[] { Fizzy(-0.25f, 0f, 0.1f), Fizzy(0.25f, 0f, 0.1f), Can(-0.5f, 0f, 0.1f, 0), Can(0.5f, 0f, 0.1f, 1) }.Select(OnBack)
                .Concat(new[] { Fizzy(-0.3f, 0f, 0.5f), Fizzy(0.3f, 0f, 0.5f) })
                .Concat(new[] { -0.75f, 0f, 0.75f }.Select((x, i) => Can(x, 0f, 0.5f, i)))
                .ToArray();

        /// <summary>Greatest hits: one cluster from each room.</summary>
        public static PropPlacement[] GreatestHits() =>
            Wall(3, 2, -0.65f, 0.4f)                                     // Home Office: keycaps
                .Append(Prop(PropKind.Plate, -0.15f, 0f, 0.45f))        // Café: a donut on a plate
                .Concat(DonutStack(1, -0.15f, 0.45f, Plate))
                .Append(Prop(PropKind.Book, 0.35f, 0f, 0.45f))          // Library: a candle on a book
                .Append(Prop(PropKind.Candle, 0.35f, Book, 0.45f, 0))
                .Concat(CanTower(2, 0.8f, 0.45f))                       // Gaming Den: cans
                .Append(Prop(PropKind.MilkBottle, -0.5f, 0f, 0.05f))
                .Append(Prop(PropKind.Hourglass, 0.4f, 0f, 0.05f))
                .ToArray();

        /// <summary>The grand finale (Hard): everything, across both desks.</summary>
        public static PropPlacement[] GrandFinale() =>
            new[]
                {
                    OnBack(Fizzy(0.2f, 0f, 0.15f)),
                    OnBack(Prop(PropKind.PencilCup, -0.2f, 0f, 0.15f, 0)),
                    Prop(PropKind.Book, -0.74f, 0f, 0.45f),
                    Prop(PropKind.Candle, -0.74f, Book, 0.45f, 1),
                    // Under 0.24 tall, so pushed straight back it can't jam under the back desk.
                    Prop(PropKind.Plate, 0f, 0f, 0.35f),
                    Prop(PropKind.Donut, 0f, Plate, 0.35f, 1),
                    Prop(PropKind.GameController, 0.6f, 0f, 0.45f, 2),
                    Can(0.6f, Controller, 0.45f, 0),
                    Prop(PropKind.MilkBottle, -0.75f, 0f, -0.25f),
                    Fizzy(0.75f, 0f, -0.25f),
                }
                .Concat(DonutStack(2, -0.3f, -0.25f))
                .Concat(Wall(2, 1, 0.3f, -0.25f))
                .ToArray();
    }
}
