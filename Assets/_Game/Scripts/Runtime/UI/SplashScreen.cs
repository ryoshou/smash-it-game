using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>The launch card (spec §7): the game's title over a plain backdrop.</summary>
    public sealed class SplashScreen : MonoBehaviour
    {
        [SerializeField] private Text titleText;

        public void Show(string title)
        {
            titleText.text = title;
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
