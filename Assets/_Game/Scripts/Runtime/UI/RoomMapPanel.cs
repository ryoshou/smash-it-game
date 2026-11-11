using System;
using System.Collections.Generic;
using Game.Levels;
using Game.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// The room map, opened from home: one card per room with its levels, a progress bar and Play, Replay or Locked.
    /// Tapping a room that isn't locked plays it (RoomProgress.PlayIndex).
    /// </summary>
    public sealed class RoomMapPanel : MonoBehaviour
    {
        private const float LockedAlpha = 0.45f;

        [SerializeField] private Text titleText;
        [SerializeField] private Button backButton;
        [SerializeField] private Text backLabel;
        [SerializeField] private Button[] cards;
        [SerializeField] private Image[] cardImages;
        [SerializeField] private Text[] nameTexts;
        [SerializeField] private Text[] detailTexts;
        [SerializeField] private Text[] statusTexts;
        [SerializeField] private RectTransform[] fills;
        [SerializeField] private CanvasGroup[] cardGroups;

        public void Show(IReadOnlyList<RoomEntry> rooms, UiStrings strings, Action<RoomEntry> onPick, Action onBack)
        {
            titleText.text = strings.rooms;
            backLabel.text = strings.back;
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => onBack());
            for (int i = 0; i < cards.Length; i++)
            {
                bool hasRoom = i < rooms.Count;
                cards[i].gameObject.SetActive(hasRoom);
                if (hasRoom) ShowCard(i, rooms[i], strings, onPick);
            }
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        private void ShowCard(int i, RoomEntry room, UiStrings strings, Action<RoomEntry> onPick)
        {
            nameTexts[i].text = strings.RoomName(room.Number);
            detailTexts[i].text = string.Format(strings.roomLevelsFormat, room.FirstIndex + 1, room.LastIndex + 1) + "  ·  " +
                                  string.Format(strings.roomProgressFormat, room.Cleared, room.Count);
            statusTexts[i].text = room.State == RoomState.Locked ? strings.roomLocked
                : room.State == RoomState.Done ? strings.roomReplay
                : strings.roomPlay;
            fills[i].anchorMax = new Vector2(room.Count > 0 ? (float)room.Cleared / room.Count : 0f, 1f);
            if (room.Theme != null) cardImages[i].color = room.Theme.background;
            cardGroups[i].alpha = room.CanPlay ? 1f : LockedAlpha;
            cards[i].interactable = room.CanPlay;
            cards[i].onClick.RemoveAllListeners();
            if (room.CanPlay) cards[i].onClick.AddListener(() => onPick(room));
        }
    }
}
