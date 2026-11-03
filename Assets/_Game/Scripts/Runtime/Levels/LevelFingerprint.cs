using System.Globalization;
using System.Text;
using UnityEngine;

namespace Game.Levels
{
    /// <summary>
    /// A 64-bit FNV-1a hash of everything that changes how a level plays: shots, difficulty, desks, and each prop's
    /// kind, desk, target flag, yaw (0.1°) and position (mm). The variant (colour) is left out: it is only the look.
    /// </summary>
    public static class LevelFingerprint
    {
        private const ulong OffsetBasis = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        public static string Of(LevelDef level)
        {
            var text = new StringBuilder();
            text.Append(level.shots).Append('|').Append((int)level.difficulty).Append('|').Append(level.hasBackDesk ? 1 : 0)
                .Append('|').Append(Mm(level.backDeskSize.x)).Append(',').Append(Mm(level.backDeskSize.y));
            foreach (var prop in level.props)
            {
                text.Append('|').Append((int)prop.kind).Append(',').Append(prop.desk).Append(',').Append(prop.isTarget ? 1 : 0)
                    .Append(',').Append(Mathf.RoundToInt(prop.yawDegrees * 10f).ToString(CultureInfo.InvariantCulture))
                    .Append(',').Append(Mm(prop.basePosition.x)).Append(',').Append(Mm(prop.basePosition.y))
                    .Append(',').Append(Mm(prop.basePosition.z));
            }
            ulong hash = OffsetBasis;
            foreach (byte b in Encoding.UTF8.GetBytes(text.ToString()))
            {
                hash ^= b;
                hash *= Prime;
            }
            return hash.ToString("x16", CultureInfo.InvariantCulture);
        }

        private static string Mm(float metres) => Mathf.RoundToInt(metres * 1000f).ToString(CultureInfo.InvariantCulture);
    }
}
