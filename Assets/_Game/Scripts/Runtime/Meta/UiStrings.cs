using Game.Levels;
using UnityEngine;

namespace Game.Meta
{
    /// <summary>Every player-visible string (spec §10: externalized, English at launch). One asset per language.</summary>
    [CreateAssetMenu(menuName = "Smash/UI Strings")]
    public sealed class UiStrings : ScriptableObject
    {
        [Header("Formats ({0} is the value)")]
        public string shotsFormat = "Shots: {0}";
        public string levelFormat = "Level {0}";
        public string levelHardFormat = "Level {0} · Hard";
        public string levelFeastFormat = "Level {0} · Feast!";
        public string playFormat = "Play · Level {0}";
        public string removeAdsFormat = "Remove Ads {0}";
        public string revealFormat = "New thing to knock over: {0}!";
        public string testPurchaseFormat = "Test purchase: Remove Ads for {0}";
        public string roomLevelsFormat = "Levels {0}–{1}";
        public string roomProgressFormat = "{0} / {1}";

        [Header("Labels")]
        public string winTitle = "Desk cleared!";
        public string next = "Next";
        public string loseTitle = "Out of shots";
        public string continueAd = "+3 shots";
        public string retry = "Retry";
        public string paused = "Paused";
        public string settings = "Settings";
        public string resume = "Resume";
        public string restart = "Restart level";
        public string sound = "Sound";
        public string music = "Music";
        public string haptics = "Vibration";
        public string restore = "Restore purchases";
        public string restored = "Purchases restored";
        public string nothingToRestore = "Nothing to restore";
        public string privacy = "Privacy policy";
        public string testAd = "Test ad";
        public string close = "Close";
        public string buy = "Buy";
        public string cancel = "Cancel";
        public string title = "Smash It";
        public string rooms = "Rooms";
        public string home = "Home";
        public string back = "Back";
        public string roomPlay = "Play";
        public string roomReplay = "Replay";
        public string roomLocked = "Locked";

        [Tooltip("Room map names, in play order.")]
        public string[] roomNames = { "Home Office", "Kitchen", "Café", "Library", "Gaming Den" };

        [Tooltip("Indexed by (int)PropKind.")]
        public string[] propNames = new string[0];

        /// <param name="number">1-based room number.</param>
        public string RoomName(int number) =>
            roomNames != null && number >= 1 && number <= roomNames.Length && !string.IsNullOrEmpty(roomNames[number - 1])
                ? roomNames[number - 1]
                : $"Room {number}";

        public string PropName(PropKind kind)
        {
            int i = (int)kind;
            return propNames != null && i < propNames.Length && !string.IsNullOrEmpty(propNames[i])
                ? propNames[i]
                : kind.ToString();
        }
    }
}
