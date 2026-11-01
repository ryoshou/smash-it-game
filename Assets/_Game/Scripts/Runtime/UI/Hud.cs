using Game.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>Shot counter and level number (labelled when Hard, spec §6.1).</summary>
    public sealed class Hud : MonoBehaviour
    {
        [SerializeField] private Text shotsText;
        [SerializeField] private Text levelText;

        public void SetShots(int shots) => shotsText.text = $"Shots: {shots}";

        public void SetLevel(int level, Difficulty difficulty) =>
            levelText.text = difficulty == Difficulty.Hard ? $"Level {level} · Hard" : $"Level {level}";
    }
}
