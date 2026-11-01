using UnityEngine;

namespace Game.Levels
{
    /// <summary>A room's look and sound (spec §6.1: each room has its own backdrop, desk style and music).</summary>
    [CreateAssetMenu(menuName = "Smash/Room Theme")]
    public sealed class RoomTheme : ScriptableObject
    {
        public GameObject backdropPrefab;
        public AudioClip music;
        public Color background = new Color(0.98f, 0.89f, 0.84f);
    }
}
