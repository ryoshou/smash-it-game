using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>Shot counter and level number.</summary>
    public sealed class Hud : MonoBehaviour
    {
        [SerializeField] private Text shotsText;
        [SerializeField] private Text levelText;

        public void SetShots(int shots) => shotsText.text = $"Shots: {shots}";

        public void SetLevel(int level) => levelText.text = $"Level {level}";
    }
}
