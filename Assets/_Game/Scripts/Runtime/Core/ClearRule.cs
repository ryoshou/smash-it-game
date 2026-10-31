using UnityEngine;

namespace Game.Core
{
    /// <summary>An object is cleared once it has fallen below the desk top (with margin) or left the play volume.</summary>
    public static class ClearRule
    {
        public static bool IsCleared(Vector3 position, float deskTopY, float clearMargin, Bounds playVolume) =>
            position.y < deskTopY - clearMargin || !playVolume.Contains(position);
    }
}
