using System;
using Game.Levels;
using Game.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>Shot counter, level number (labelled when Hard or a Feast, spec §6.1) and the pause button.</summary>
    public sealed class Hud : MonoBehaviour
    {
        [SerializeField] private Text shotsText;
        [SerializeField] private Text levelText;
        [SerializeField] private Button pauseButton;

        private UiStrings strings;

        /// <summary>A null onPause hides the pause button.</summary>
        public void Bind(UiStrings uiStrings, Action onPause)
        {
            strings = uiStrings;
            if (pauseButton == null) return;
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.gameObject.SetActive(onPause != null);
            if (onPause != null) pauseButton.onClick.AddListener(() => onPause());
        }

        public void SetShots(int shots) => shotsText.text = string.Format(strings.shotsFormat, shots);

        public void SetLevel(int levelNumber, Difficulty difficulty) =>
            levelText.text = string.Format(LevelFormat(difficulty), levelNumber);

        private string LevelFormat(Difficulty difficulty)
        {
            if (difficulty == Difficulty.Hard) return strings.levelHardFormat;
            if (difficulty == Difficulty.Feast) return strings.levelFeastFormat;
            return strings.levelFormat;
        }
    }
}
