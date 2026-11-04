using System.Collections.Generic;
using System.Linq;
using Game.Levels;
using static Game.Editor.LayoutKit;

namespace Game.Editor
{
    /// <summary>
    /// Room 2 (Café) layouts, levels 26–37: the plate arrives at 26 and the donut at 36. Every prop is a target
    /// (owner, checkpoint 2); every 5th level is Hard.
    /// </summary>
    internal static partial class Room2Layouts
    {
        private static readonly float[] Thirds = { -0.6f, 0f, 0.6f };

        /// <summary>New: the plate. Short plate stacks on three keycap plinths.</summary>
        public static PropPlacement[] PlatePlinths()
        {
            var props = new List<PropPlacement>();
            var spots = new[] { new UnityEngine.Vector2(-0.6f, 0.1f), new UnityEngine.Vector2(0f, 0.4f), new UnityEngine.Vector2(0.6f, 0f) };
            foreach (var spot in spots)
            {
                props.Add(Cap(PropKind.Keycap2u, spot.x, 0f, spot.y - 0.08f, Look.Cream));
                props.Add(Cap(PropKind.Keycap2u, spot.x, 0f, spot.y + 0.08f, Look.Sky));
                props.AddRange(Tower(PropKind.Plate, 2, Plate, spot.x, spot.y, Keycap));
            }
            return props.ToArray();
        }

        /// <summary>Two plate stacks near the back edge that topple off it.</summary>
        public static PropPlacement[] PlateStacks() =>
            Tower(PropKind.Plate, 5, Plate, -0.45f, 0.5f).Concat(Tower(PropKind.Plate, 5, Plate, 0.45f, 0.5f)).ToArray();

        /// <summary>A row of mugs, each standing on a plate, near the back edge.</summary>
        public static PropPlacement[] MugsOnPlates() =>
            Thirds.SelectMany((x, i) => MugOnPlate(x, 0.45f, i)).ToArray();

        /// <summary>Plate stacks on the back desk, a row of paper cups on the main desk.</summary>
        public static PropPlacement[] BackPlatesFrontCups() =>
            Tower(PropKind.Plate, 2, Plate, -0.3f, 0f).Concat(Tower(PropKind.Plate, 2, Plate, 0.3f, 0f)).Select(OnBack)
                .Concat(new[] { -0.6f, -0.2f, 0.2f, 0.6f }.Select(x => Prop(PropKind.PaperCup, x, 0f, -0.2f)))
                .ToArray();

        /// <summary>Three plate-capped keycap towers (2u keycaps, two deep), a heavy cactus shielding the middle one (Hard).</summary>
        public static PropPlacement[] CappedTowers() =>
            new[] { Prop(PropKind.CactusPot, 0f, 0f, -0.05f) }
                .Concat(new[] { -0.62f, 0f, 0.62f }.SelectMany(x => PlinthTower(3, x, 0.45f)
                    .Append(Prop(PropKind.Plate, x, 3f * Keycap, 0.45f))))
                .ToArray();

        /// <summary>rows of 2u keycap pairs, two deep (0.31 x 0.27): steady enough to stand a plate or a mug on.</summary>
        internal static IEnumerable<PropPlacement> PlinthTower(int rows, float x, float z, float y0 = 0f)
        {
            for (int row = 0; row < rows; row++)
            {
                yield return Cap(PropKind.Keycap2u, x, y0 + row * Keycap, z - 0.08f, (Look)(row % 3));
                yield return Cap(PropKind.Keycap2u, x, y0 + row * Keycap, z + 0.08f, (Look)((row + 1) % 3));
            }
        }

        /// <summary>A paper-cup pyramid, 3-2-1, near the back edge.</summary>
        public static PropPlacement[] CupPyramid()
        {
            var props = new List<PropPlacement>();
            const float cupPitch = 0.36f;
            for (int row = 0; row < 3; row++)
                for (int i = 0; i < 3 - row; i++)
                    props.Add(Prop(PropKind.PaperCup, (i - (2 - row) / 2f) * cupPitch, row * Cup, 0.45f));
            props.Add(Prop(PropKind.Mug, -0.72f, 0f, -0.35f, 0));
            props.Add(Prop(PropKind.Mug, 0.72f, 0f, -0.35f, 2));
            return props.ToArray();
        }

        /// <summary>Lob: a low wall of books shields three mugs at the back edge.</summary>
        public static PropPlacement[] BookWallLob() =>
            Thirds.Select(x => Prop(PropKind.Book, x * 0.92f, 0f, 0.12f))
                .Concat(Thirds.Select((x, i) => Prop(PropKind.Mug, x, 0f, 0.53f, i)))
                .ToArray();

        /// <summary>A lamp and a plate stack on the back desk, three plate stacks in front.</summary>
        public static PropPlacement[] LampBehindPlates() =>
            new[] { Prop(PropKind.DeskLamp, 0.3f, 0f, 0f) }.Concat(Tower(PropKind.Plate, 2, Plate, -0.3f, 0f)).Select(OnBack)
                .Concat(new[] { -0.5f, 0.5f }.SelectMany(x => Tower(PropKind.Plate, 2, Plate, x, 0.5f)))
                .ToArray();

        /// <summary>Two spacebar decks on 1u keycap legs, a plate stack on each.</summary>
        public static PropPlacement[] PlateBridges() => BridgeWithPlates(-0.42f, 0.15f).Concat(BridgeWithPlates(0.42f, 0.15f)).ToArray();

        private static IEnumerable<PropPlacement> BridgeWithPlates(float x, float z)
        {
            foreach (float legX in new[] { x - 0.24f, x + 0.24f })
                foreach (float legZ in new[] { z - DeckDepth, z + DeckDepth })
                    yield return Cap(PropKind.Keycap1u, legX, 0f, legZ, Look.Pink);
            foreach (var deck in Bridge(x, Keycap, z)) yield return deck;
            foreach (var plate in Tower(PropKind.Plate, 2, Plate, x, z, 2f * Keycap)) yield return plate;
        }

        /// <summary>A wide spread of mugs on plates and plate stacks, few shots (Hard).</summary>
        public static PropPlacement[] WideCafeSpread() =>
            new[] { (x: -0.7f, z: -0.4f), (x: 0.7f, z: -0.4f), (x: 0f, z: 0.5f) }
                .SelectMany((spot, i) => MugOnPlate(spot.x, spot.z, i))
                .ToArray();

        /// <summary>New: the donut. Two donut stacks.</summary>
        public static PropPlacement[] DonutStacks() =>
            DonutStack(5, -0.35f, 0.25f).Concat(DonutStack(5, 0.35f, 0.25f)).ToArray();

        /// <summary>Donuts on plates, a row of three near the back edge.</summary>
        public static PropPlacement[] DonutsOnPlates() =>
            Thirds.SelectMany(x => DonutStack(2, x, 0.45f, Plate).Prepend(Prop(PropKind.Plate, x, 0f, 0.45f))).ToArray();

        internal static IEnumerable<PropPlacement> MugOnPlate(float x, float z, int look)
        {
            yield return Prop(PropKind.Plate, x, 0f, z);
            yield return Prop(PropKind.Mug, x, Plate, z, look % 3);
        }
    }
}
