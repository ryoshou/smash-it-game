using UnityEngine;

namespace Game.Levels
{
    /// <summary>One level: shot count, desk size(s) and the props on them.</summary>
    [CreateAssetMenu(menuName = "Smash/Level")]
    public sealed class LevelDef : ScriptableObject
    {
        public const int MinShots = 1;
        public const int MaxShots = 30;
        /// <summary>Spec §3 performance budget.</summary>
        public const int MaxProps = 40;
        public const int MainDesk = 0;
        public const int BackDesk = 1;
        /// <summary>The back desk prefab is built at this size.</summary>
        public static readonly Vector2 DefaultBackDeskSize = new Vector2(1.2f, 0.5f);

        public string id;
        public int shots = 6;
        public Difficulty difficulty;
        public Vector2 deskSize = new Vector2(2.0f, 1.5f);
        [Tooltip("A second, raised desk behind the main one (LevelSpawner.BackDeskTop).")]
        public bool hasBackDesk;
        public Vector2 backDeskSize = DefaultBackDeskSize;
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
            else if (props.Length > MaxProps)
                error = $"{id}: {props.Length} props, more than {MaxProps}";
            else
                error = FirstOffDesk();
            return error == null;
        }

        private string FirstOffDesk()
        {
            for (int i = 0; i < props.Length; i++)
            {
                int desk = props[i].desk;
                if (desk != MainDesk && desk != BackDesk) return $"{id}: prop {i} is on unknown desk {desk}";
                if (desk == BackDesk && !hasBackDesk) return $"{id}: prop {i} is on the back desk, but the level has no back desk";
                var size = desk == BackDesk ? backDeskSize : deskSize;
                var p = props[i].basePosition;
                if (Mathf.Abs(p.x) > size.x / 2f || Mathf.Abs(p.z) > size.y / 2f || p.y < 0f)
                    return $"{id}: prop {i} at {p} is off its desk";
            }
            return null;
        }
    }
}
