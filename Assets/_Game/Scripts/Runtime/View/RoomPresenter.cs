using Game.Juice;
using Game.Levels;
using UnityEngine;

namespace Game.View
{
    /// <summary>Shows the current level's room: backdrop, camera background and music. Changes only when the room does.</summary>
    public sealed class RoomPresenter : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private MusicPlayer music;
        [SerializeField] private JuiceConfig juice;
        [Tooltip("Used for levels that name no room (e.g. the prototype levels).")]
        [SerializeField] private RoomTheme fallback;

        private RoomTheme current;
        private GameObject backdrop;

        public RoomTheme Current => current;

        public void Apply(RoomTheme theme)
        {
            theme = theme != null ? theme : fallback;
            if (theme == null || theme == current) return;
            current = theme;
            if (backdrop != null) Destroy(backdrop);
            backdrop = theme.backdropPrefab != null ? Instantiate(theme.backdropPrefab, transform) : null;
            cam.backgroundColor = theme.background;
            music.Play(theme.music, juice.musicFadeSeconds);
        }
    }
}
