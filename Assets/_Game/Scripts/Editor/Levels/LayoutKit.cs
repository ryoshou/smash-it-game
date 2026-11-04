using System.Collections.Generic;
using Game.Levels;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>Building blocks for hand-written layouts: prop heights, keycap looks, rows, towers and bridges.</summary>
    internal static class LayoutKit
    {
        public const float Keycap = 0.15f;   // standing keycap height
        public const float Book = 0.26f;     // book height, lying flat
        public const float Mug = 0.35f;
        public const float Cup = 0.41f;      // paper cup with its lid
        public const float Plate = 0.06f;
        public const float Donut = 0.164f;   // the frosted looks (1, 2); the plain one (0) is 0.144, so it goes on top
        public const float MilkBottle = 0.422f;
        public const float Candle = 0.393f;
        public const float Hourglass = 0.4f;
        public const float PencilCup = 0.403f;
        public const float SodaCan = 0.3f;
        public const float Controller = 0.092f; // lying flat
        public const float Pitch = 0.16f;    // 1u keycap spacing (0.15 + a gap)
        public const float WidePitch = 0.32f; // 2u keycap spacing (0.31 + a gap)
        public const float DeckDepth = 0.065f; // half the spacing of a two-spacebar deck

        public enum Look { Cream, Pink, Sky }

        public static PropPlacement Cap(PropKind kind, float x, float y, float z, Look look, float yaw = 0f) =>
            Prop(kind, x, y, z, (int)look, yaw);

        public static PropPlacement Prop(PropKind kind, float x, float y, float z, int variant = 0, float yaw = 0f) =>
            new PropPlacement { kind = kind, basePosition = new Vector3(x, y, z), variant = variant, yawDegrees = yaw, isTarget = true };

        /// <summary>The same placement on the raised back desk (position relative to its top centre).</summary>
        public static PropPlacement OnBack(PropPlacement placement)
        {
            placement.desk = LevelDef.BackDesk;
            return placement;
        }

        /// <summary>count keycaps side by side along x, centred on centreX, looks cycling from the row's index.</summary>
        public static IEnumerable<PropPlacement> Row(PropKind kind, int count, float centreX, float pitch, float y, float z, int lookOffset = 0)
        {
            for (int i = 0; i < count; i++)
                yield return Cap(kind, centreX + (i - (count - 1) / 2f) * pitch, y, z, (Look)((i + lookOffset) % 3));
        }

        /// <summary>A rows × columns keycap wall (1u), bottom row on y0.</summary>
        public static IEnumerable<PropPlacement> Wall(int columns, int rows, float centreX, float z, float y0 = 0f)
        {
            for (int row = 0; row < rows; row++)
                foreach (var cap in Row(PropKind.Keycap1u, columns, centreX, Pitch, y0 + row * Keycap, z, row))
                    yield return cap;
        }

        /// <summary>A stack of the same kind, each resting on the one below.</summary>
        public static IEnumerable<PropPlacement> Tower(PropKind kind, int count, float height, float x, float z, float y0 = 0f)
        {
            for (int i = 0; i < count; i++) yield return Prop(kind, x, y0 + i * height, z, i % 3);
        }

        /// <summary>A donut stack: frosted looks alternating, the shorter plain one on top.</summary>
        public static IEnumerable<PropPlacement> DonutStack(int count, float x, float z, float y0 = 0f)
        {
            for (int i = 0; i < count; i++)
                yield return Prop(PropKind.Donut, x, y0 + i * Donut, z, i == count - 1 ? 0 : 1 + i % 2);
        }

        /// <summary>Two spacebars side by side (a deck deep enough to stand a cup on), resting at height y.</summary>
        public static IEnumerable<PropPlacement> Bridge(float x, float y, float z)
        {
            yield return Cap(PropKind.KeycapSpace, x, y, z - DeckDepth, Look.Cream);
            yield return Cap(PropKind.KeycapSpace, x, y, z + DeckDepth, Look.Sky);
        }
    }
}
