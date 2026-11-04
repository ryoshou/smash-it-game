using System.Collections.Generic;
using System.Linq;
using Game.Levels;
using static Game.Editor.LayoutKit;

namespace Game.Editor
{
    /// <summary>
    /// Room 1 (Home Office) layouts, one idea each (spec §6.1). Every prop is a target, supports included (owner,
    /// checkpoint 2). Levels 1–10 teach: pull, power, aim and breaking, toppling, heavy objects (Hard), two desks,
    /// a chain, a lob over a wall, a wide spread, and a mixed two-desk Hard level.
    /// </summary>
    internal static partial class Room1Layouts
    {
        /// <summary>A low wall of 1u keycaps at the front centre: any pull hits.</summary>
        public static PropPlacement[] FirstPull()
        {
            var props = new List<PropPlacement>();
            for (int row = 0; row < 2; row++)
                for (int col = -2; col <= 2; col++)
                    props.Add(Cap(PropKind.Keycap1u, col * Pitch, row * Keycap, -0.25f, (Look)((col + 2 + row) % 3)));
            return props.ToArray();
        }

        /// <summary>A keycap tower at the back edge: needs a strong pull.</summary>
        public static PropPlacement[] Power()
        {
            var props = new List<PropPlacement>();
            for (int row = 0; row < 5; row++)
                for (int col = 0; col < 2; col++)
                    props.Add(Cap(PropKind.Keycap1u, (col - 0.5f) * Pitch, row * Keycap, 0.5f, (Look)((row + col) % 3)));
            return props.ToArray();
        }

        /// <summary>Three mugs on keycap plinths, left / centre-right / far right: aim, and mugs shatter.</summary>
        public static PropPlacement[] AimAndBreak()
        {
            var props = new List<PropPlacement>();
            var spots = new[] { new UnityEngine.Vector2(-0.62f, 0.05f), new UnityEngine.Vector2(0.12f, 0.35f), new UnityEngine.Vector2(0.62f, -0.2f) };
            for (int i = 0; i < spots.Length; i++)
            {
                props.Add(Cap(PropKind.Keycap2u, spots[i].x, 0f, spots[i].y - 0.08f, Look.Cream));
                props.Add(Cap(PropKind.Keycap2u, spots[i].x, 0f, spots[i].y + 0.08f, Look.Sky));
                props.Add(Prop(PropKind.Mug, spots[i].x, Keycap, spots[i].y, i));
            }
            return props.ToArray();
        }

        /// <summary>Four books stacked, three keycaps on top: topple the tower.</summary>
        public static PropPlacement[] TallStack()
        {
            var props = new List<PropPlacement>();
            float[] yaws = { 0f, 4f, -3f, 2f };
            props.Add(Prop(PropKind.Book, 0f, 0f, 0.15f, 0, yaws[0]));
            for (int i = 1; i < yaws.Length; i++)
                props.Add(Prop(PropKind.Book, 0f, i * Book, 0.15f, 0, yaws[i]));
            for (int col = -1; col <= 1; col++)
                props.Add(Cap(PropKind.Keycap1u, col * Pitch, yaws.Length * Book, 0.15f, (Look)(col + 1)));
            return props.ToArray();
        }

        /// <summary>A heavy cactus pot in the middle, mugs on books either side (Hard).</summary>
        public static PropPlacement[] Heavy() => new[]
        {
            Prop(PropKind.CactusPot, 0f, 0f, 0.3f, 0),
            // Books mid-desk (M3: moved forward from z 0.42 so the level is Hard by the bot's band).
            Prop(PropKind.Book, -0.62f, 0f, 0.1f, 0),
            Prop(PropKind.Mug, -0.62f, Book, 0.1f, 2),
            Prop(PropKind.Book, 0.62f, 0f, 0.1f, 0),
            Prop(PropKind.Mug, 0.62f, Book, 0.1f, 1),
        };

        /// <summary>Two desks: a keycap wall on the raised back desk (right of the monitor), three keycaps up front.</summary>
        public static PropPlacement[] TwoDesks() =>
            Wall(4, 2, 0.24f, 0f).Select(OnBack)
                .Concat(Row(PropKind.Keycap1u, 3, 0f, Pitch, 0f, -0.3f))
                .ToArray();

        /// <summary>Dominoes: two rows of standing keycaps running away from the player; the back ones drop off.</summary>
        public static PropPlacement[] Dominoes()
        {
            var props = new List<PropPlacement>();
            for (int i = 0; i < 6; i++)
            {
                props.Add(Cap(PropKind.Keycap1u, -0.25f, 0f, i * 0.13f, (Look)(i % 3)));
                props.Add(Cap(PropKind.Keycap1u, 0.25f, 0f, i * 0.13f, (Look)((i + 1) % 3)));
            }
            return props.ToArray();
        }

        /// <summary>Lob: a low wall of wide keycaps at the front shields three mugs near the back edge.</summary>
        public static PropPlacement[] Lob() =>
            Row(PropKind.Keycap2u, 3, 0f, WidePitch, 0f, -0.1f)
                .Concat(Row(PropKind.Keycap2u, 3, 0f, WidePitch, Keycap, -0.1f, 1))
                .Concat(new[] { -0.6f, 0f, 0.6f }.Select((x, i) => Prop(PropKind.Mug, x, 0f, 0.45f, i)))
                .ToArray();

        /// <summary>Wide spread: spacebars and Enter keys across the full width, two wide keys stacked on top.</summary>
        public static PropPlacement[] WideSpread() => new[]
        {
            Cap(PropKind.KeycapSpace, -0.6f, 0f, -0.3f, Look.Cream),
            Cap(PropKind.KeycapSpace, 0.6f, 0f, -0.3f, Look.Sky),
            Cap(PropKind.KeycapEnter, 0f, 0f, -0.3f, Look.Pink),
            Cap(PropKind.Keycap2u, -0.6f, Keycap, -0.3f, Look.Pink),
            Cap(PropKind.Keycap2u, 0.6f, Keycap, -0.3f, Look.Cream),
            Cap(PropKind.KeycapEnter, -0.75f, 0f, 0.35f, Look.Sky),
            Cap(PropKind.KeycapSpace, 0f, 0f, 0.35f, Look.Pink),
            Cap(PropKind.KeycapEnter, 0.75f, 0f, 0.35f, Look.Cream),
        };

        /// <summary>Mixed, two desks (Hard): a mug and a book with keycaps on the back desk; a mug on a book and a wall up front.</summary>
        public static PropPlacement[] MixedTwoDesks() => new[]
            {
                OnBack(Prop(PropKind.Mug, 0.3f, 0f, 0f, 1)),
                OnBack(Prop(PropKind.Book, -0.3f, 0f, 0f)),
                OnBack(Cap(PropKind.Keycap1u, -0.38f, Book, 0f, Look.Pink)),
                OnBack(Cap(PropKind.Keycap1u, -0.22f, Book, 0f, Look.Sky)),
                Prop(PropKind.Book, -0.5f, 0f, 0f),
                Prop(PropKind.Mug, -0.5f, Book, 0f, 2),
            }
            .Concat(Wall(3, 2, 0.45f, 0.1f))
            .ToArray();
    }
}
