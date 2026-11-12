using System;
using Game.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>Home (spec §7): title, the level badge, Play, the room map, No Ads (until bought) and the settings gear, over the live desk.</summary>
    public sealed class HomeScreen : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Text levelText;
        [SerializeField] private Button playButton;
        [SerializeField] private Text playLabel;
        [SerializeField] private Button removeAdsButton;
        [SerializeField] private Text removeAdsLabel;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button roomsButton;
        [SerializeField] private Text roomsLabel;

        private UiStrings strings;

        public void Bind(UiStrings uiStrings) => strings = uiStrings;

        public void Show(int levelNumber, bool showRemoveAds, Action onPlay, Action onRemoveAds, Action onSettings,
            Action onRooms)
        {
            titleText.text = strings.title;
            subtitleText.text = strings.subtitle;
            levelText.text = string.Format(strings.levelFormat, levelNumber);
            playLabel.text = strings.play;
            removeAdsLabel.text = strings.noAds;
            roomsLabel.text = strings.rooms;
            Listen(playButton, onPlay);
            Listen(removeAdsButton, onRemoveAds);
            Listen(settingsButton, onSettings);
            Listen(roomsButton, onRooms);
            SetRemoveAdsVisible(showRemoveAds);
            gameObject.SetActive(true);
        }

        public void SetRemoveAdsVisible(bool visible) => removeAdsButton.gameObject.SetActive(visible);

        public void Hide() => gameObject.SetActive(false);

        private static void Listen(Button button, Action onClick)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick());
        }
    }
}
