using System;
using Game.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>Home (spec §7): title, "Play · Level N", Remove Ads (until bought) and settings, over the live desk.</summary>
    public sealed class HomeScreen : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Button playButton;
        [SerializeField] private Text playLabel;
        [SerializeField] private Button removeAdsButton;
        [SerializeField] private Text removeAdsLabel;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Text settingsLabel;

        private UiStrings strings;

        public void Bind(UiStrings uiStrings) => strings = uiStrings;

        public void Show(int levelNumber, bool showRemoveAds, string price, Action onPlay, Action onRemoveAds, Action onSettings)
        {
            titleText.text = strings.title;
            playLabel.text = string.Format(strings.playFormat, levelNumber);
            removeAdsLabel.text = string.Format(strings.removeAdsFormat, price);
            settingsLabel.text = strings.settings;
            Listen(playButton, onPlay);
            Listen(removeAdsButton, onRemoveAds);
            Listen(settingsButton, onSettings);
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
