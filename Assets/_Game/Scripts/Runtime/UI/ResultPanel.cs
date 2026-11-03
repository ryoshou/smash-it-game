using System;
using Game.Levels;
using Game.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// The win and lose screens: a title, up to two buttons and, on a win, the "new thing to knock over" bar (spec §4.5).
    /// The first tap on either button locks both until the panel is shown again, so a double tap acts once.
    /// </summary>
    public sealed class ResultPanel : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Button primary;
        [SerializeField] private Text primaryLabel;
        [SerializeField] private Button secondary;
        [SerializeField] private Text secondaryLabel;
        [SerializeField] private GameObject revealRoot;
        [SerializeField] private Text revealText;
        [SerializeField] private RectTransform revealFill;

        private UiStrings strings;
        private bool isLocked;

        public void Bind(UiStrings uiStrings) => strings = uiStrings;

        public void ShowWin(Reveal reveal, string nextName, Action onNext)
        {
            title.text = strings.winTitle;
            SetButton(primary, primaryLabel, strings.next, onNext);
            SetButton(secondary, secondaryLabel, null, null);
            revealRoot.SetActive(reveal.HasNext);
            if (reveal.HasNext)
            {
                revealText.text = string.Format(strings.revealFormat, nextName);
                revealFill.anchorMax = new Vector2(Mathf.Clamp01(reveal.Progress01), 1f);
            }
            Open();
        }

        public void ShowLose(bool offerContinue, Action onContinue, Action onRetry)
        {
            title.text = strings.loseTitle;
            SetButton(primary, primaryLabel, offerContinue ? strings.continueAd : null, offerContinue ? onContinue : null);
            SetButton(secondary, secondaryLabel, strings.retry, onRetry);
            revealRoot.SetActive(false);
            Open();
        }

        public void Hide() => gameObject.SetActive(false);

        private void Open()
        {
            isLocked = false;
            gameObject.SetActive(true);
        }

        /// <summary>A null label hides the button.</summary>
        private void SetButton(Button button, Text label, string text, Action onClick)
        {
            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(text != null);
            if (text == null) return;
            label.text = text;
            button.onClick.AddListener(() =>
            {
                if (isLocked) return;
                isLocked = true;
                onClick();
            });
        }
    }
}
