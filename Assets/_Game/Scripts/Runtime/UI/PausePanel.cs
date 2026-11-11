using System;
using Game.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Pause and settings (spec §7): sound, music and vibration toggles, resume, restart, home, restore purchases and
    /// the privacy policy. Opened from home it is plain settings: no restart or home, and Resume reads Close.
    /// </summary>
    public sealed class PausePanel : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Toggle soundToggle;
        [SerializeField] private Text soundLabel;
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Text musicLabel;
        [SerializeField] private Toggle hapticsToggle;
        [SerializeField] private Text hapticsLabel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Text resumeLabel;
        [SerializeField] private Button restartButton;
        [SerializeField] private Text restartLabel;
        [SerializeField] private Button homeButton;
        [SerializeField] private Text homeLabel;
        [SerializeField] private Button restoreButton;
        [SerializeField] private Text restoreLabel;
        [SerializeField] private Button privacyButton;
        [SerializeField] private Text privacyLabel;
        [SerializeField] private Text messageText;

        private UiStrings strings;

        /// <summary>A null onPrivacy hides the privacy button (no policy URL yet).</summary>
        public void Bind(UiStrings uiStrings, Action<bool> onSound, Action<bool> onMusic, Action<bool> onHaptics,
            Action onResume, Action onRestart, Action onHome, Action onRestore, Action onPrivacy)
        {
            strings = uiStrings;
            soundLabel.text = strings.sound;
            musicLabel.text = strings.music;
            hapticsLabel.text = strings.haptics;
            restartLabel.text = strings.restart;
            homeLabel.text = strings.home;
            restoreLabel.text = strings.restore;
            privacyLabel.text = strings.privacy;
            Listen(soundToggle, onSound);
            Listen(musicToggle, onMusic);
            Listen(hapticsToggle, onHaptics);
            Listen(resumeButton, onResume);
            Listen(restartButton, onRestart);
            Listen(homeButton, onHome);
            Listen(restoreButton, onRestore);
            privacyButton.gameObject.SetActive(onPrivacy != null);
            if (onPrivacy != null) Listen(privacyButton, onPrivacy);
        }

        public void Show(bool inGame, SaveData settings)
        {
            titleText.text = inGame ? strings.paused : strings.settings;
            resumeLabel.text = inGame ? strings.resume : strings.close;
            restartButton.gameObject.SetActive(inGame);
            homeButton.gameObject.SetActive(inGame);
            soundToggle.SetIsOnWithoutNotify(settings.sound);
            musicToggle.SetIsOnWithoutNotify(settings.music);
            hapticsToggle.SetIsOnWithoutNotify(settings.haptics);
            messageText.text = "";
            gameObject.SetActive(true);
        }

        public void ShowMessage(string message) => messageText.text = message;

        public void Hide() => gameObject.SetActive(false);

        private static void Listen(Button button, Action onClick)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick());
        }

        private static void Listen(Toggle toggle, Action<bool> onChange)
        {
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(on => onChange(on));
        }
    }
}
