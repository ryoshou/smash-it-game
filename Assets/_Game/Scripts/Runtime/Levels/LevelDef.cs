using UnityEngine;

namespace Game.Levels
{
    /// <summary>One level: shot count, desk size and the props on it.</summary>
    [CreateAssetMenu(menuName = "Smash/Level")]
    public sealed class LevelDef : ScriptableObject
    {
        public const int MinShots = 1;
        public const int MaxShots = 30;

        public string id;
        public int shots = 6;
        public Difficulty difficulty;
        public Vector2 deskSize = new Vector2(2.0f, 1.5f);
        public PropPlacement[] props = new PropPlacement[0];
        [Tooltip("Backdrop and music; empty uses the scene's fallback room.")]
        public RoomTheme room;

        public bool Validate(out string error)
        {
            error = null;
            if (shots < MinShots || shots > MaxShots)
                error = $"{id}: shots {shots} outside {MinShots}..{MaxShots}";
            else if (props == null || System.Array.FindIndex(props, p => p.isTarget) < 0)
                error = $"{id}: needs at least one target";
            else
                error = FirstOffDesk();
            return error == null;
        }

        private string FirstOffDesk()
        {
            for (int i = 0; i < props.Length; i++)
            {
                var p = props[i].basePosition;
                if (Mathf.Abs(p.x) > deskSize.x / 2f || Mathf.Abs(p.z) > deskSize.y / 2f || p.y < 0f)
                    return $"{id}: prop {i} at {p} is off the desk";
            }
            return null;
        }
    }
}
