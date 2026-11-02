using System.Collections.Generic;
using UnityEngine;

namespace Game.Levels
{
    /// <summary>The three M1 layouts, built from placeholder keycap sizes so they need no assets. Every prop is a target.</summary>
    public static class PrototypeLevels
    {
        private const float RowGap = 0.002f;
        private const float ColumnGap = 0.005f;
        private const float TowerX = 0.45f;
        private const float SpaceBarZ = -0.3f;

        public static Vector3 Size(PropKind kind) => kind switch
        {
            // Standing up, letter towards the camera: width x 0.15 tall x 0.11 deep (owner, checkpoint 2).
            PropKind.Keycap1u => new Vector3(0.15f, 0.15f, 0.11f),
            PropKind.Keycap2u => new Vector3(0.31f, 0.15f, 0.11f),
            PropKind.KeycapSpace => new Vector3(0.63f, 0.15f, 0.11f),
            _ => throw new KeyNotFoundException($"No placeholder size for {kind}."),
        };

        public static float Mass(PropKind kind) => kind switch
        {
            PropKind.Keycap1u => 0.04f,
            PropKind.Keycap2u => 0.07f,
            PropKind.KeycapSpace => 0.12f,
            _ => throw new KeyNotFoundException($"No placeholder mass for {kind}."),
        };

        public static PropPlacement[] Wall(int columns, int rows)
        {
            var props = new List<PropPlacement>(columns * rows);
            for (int r = 0; r < rows; r++) AddRow(props, PropKind.Keycap1u, columns, r);
            return props.ToArray();
        }

        /// <summary>Base row of 2u keycaps; each row above has one fewer, which centers it half a key over.</summary>
        public static PropPlacement[] Pyramid(int baseCount)
        {
            var props = new List<PropPlacement>();
            for (int r = 0; r < baseCount; r++) AddRow(props, PropKind.Keycap2u, baseCount - r, r);
            return props.ToArray();
        }

        public static PropPlacement[] TwoTowers(int height)
        {
            var props = new List<PropPlacement>(height * 2 + 1);
            for (int r = 0; r < height; r++)
            {
                props.Add(Target(PropKind.Keycap1u, new Vector3(-TowerX, RowY(PropKind.Keycap1u, r), 0f)));
                props.Add(Target(PropKind.Keycap1u, new Vector3(TowerX, RowY(PropKind.Keycap1u, r), 0f)));
            }
            props.Add(Target(PropKind.KeycapSpace, new Vector3(0f, 0f, SpaceBarZ)));
            return props.ToArray();
        }

        private static void AddRow(List<PropPlacement> props, PropKind kind, int count, int row)
        {
            float pitch = Size(kind).x + ColumnGap;
            float y = RowY(kind, row);
            for (int i = 0; i < count; i++)
                props.Add(Target(kind, new Vector3((i - (count - 1) / 2f) * pitch, y, 0f)));
        }

        private static float RowY(PropKind kind, int row) => row * (Size(kind).y + RowGap);

        private static PropPlacement Target(PropKind kind, Vector3 basePosition) =>
            new PropPlacement { kind = kind, basePosition = basePosition, isTarget = true };
    }
}
