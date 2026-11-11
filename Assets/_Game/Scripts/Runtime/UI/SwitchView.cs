using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Draws a Toggle as an on/off switch: knob right and a green track when on. Follows isOn every frame because
    /// SetIsOnWithoutNotify (PausePanel.Show) raises no event.
    /// </summary>
    public sealed class SwitchView : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private Image track;
        [SerializeField] private RectTransform knob;
        [SerializeField] private Color onColor = new Color(0.45f, 0.77f, 0.5f);
        [SerializeField] private Color offColor = new Color(0.8f, 0.75f, 0.75f);

        private bool hasShown;
        private bool shownOn;
        private float inset;
        private float width;

        /// <summary>Built knob-right (on): its gap to the track's end is kept at either end.</summary>
        private void Awake()
        {
            inset = -knob.offsetMax.x;
            width = knob.offsetMax.x - knob.offsetMin.x;
        }

        private void OnEnable() => hasShown = false;

        private void LateUpdate()
        {
            if (hasShown && shownOn == toggle.isOn) return;
            hasShown = true;
            shownOn = toggle.isOn;
            track.color = shownOn ? onColor : offColor;
            knob.anchorMin = knob.anchorMax = new Vector2(shownOn ? 1f : 0f, 0.5f);
            knob.offsetMin = new Vector2(shownOn ? -inset - width : inset, knob.offsetMin.y);
            knob.offsetMax = new Vector2(shownOn ? -inset : inset + width, knob.offsetMax.y);
        }
    }
}
