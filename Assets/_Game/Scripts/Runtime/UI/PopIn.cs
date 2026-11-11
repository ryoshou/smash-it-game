using UnityEngine;

namespace Game.UI
{
    /// <summary>Panels grow in with a small overshoot each time they open. Unscaled time: it plays while the game is paused.</summary>
    public sealed class PopIn : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private float seconds = 0.22f;
        [SerializeField] private float fromScale = 0.8f;

        private float elapsed;

        private void OnEnable()
        {
            elapsed = 0f;
            Apply(0f);
        }

        private void Update()
        {
            if (elapsed >= seconds) return;
            elapsed += Time.unscaledDeltaTime;
            Apply(Mathf.Clamp01(elapsed / seconds));
        }

        private void Apply(float t) => target.localScale = Vector3.one * Mathf.LerpUnclamped(fromScale, 1f, EaseOutBack(t));

        /// <summary>Overshoots to about 1.05 and settles at 1.</summary>
        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float u = t - 1f;
            return 1f + c3 * u * u * u + c1 * u * u;
        }
    }
}
