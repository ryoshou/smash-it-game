using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>The win and lose screens: a title and up to two buttons.</summary>
    public sealed class ResultPanel : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Button primary;
        [SerializeField] private Text primaryLabel;
        [SerializeField] private Button secondary;
        [SerializeField] private Text secondaryLabel;

        public void ShowWin(Action onNext)
        {
            title.text = "Desk cleared!";
            SetButton(primary, primaryLabel, "Next", onNext);
            SetButton(secondary, secondaryLabel, null, null);
            gameObject.SetActive(true);
        }

        public void ShowLose(bool canContinue, Action onContinue, Action onRetry)
        {
            title.text = "Out of shots";
            SetButton(primary, primaryLabel, canContinue ? "+3 shots" : null, canContinue ? onContinue : null);
            SetButton(secondary, secondaryLabel, "Retry", onRetry);
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);

        /// <summary>A null label hides the button.</summary>
        private static void SetButton(Button button, Text label, string text, Action onClick)
        {
            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(text != null);
            if (text == null) return;
            label.text = text;
            button.onClick.AddListener(() => onClick());
        }
    }
}
