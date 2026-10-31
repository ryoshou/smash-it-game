using UnityEngine;

namespace Game.Juice
{
    /// <summary>Small additive shake of the transform it sits on: kicks add amplitude up to a cap, which decays to zero.</summary>
    public sealed class CameraShake : MonoBehaviour
    {
        [SerializeField] private JuiceConfig config;

        private Vector3 restPosition;
        private float amplitude;

        public void Kick(float amount) => amplitude = Mathf.Min(config.shakeMax, amplitude + amount);

        private void Awake() => restPosition = transform.localPosition;

        private void LateUpdate()
        {
            if (amplitude <= 0f)
            {
                transform.localPosition = restPosition;
                return;
            }
            transform.localPosition = restPosition + (Vector3)(Random.insideUnitCircle * amplitude);
            amplitude = Mathf.Max(0f, amplitude - config.shakeMax * Time.unscaledDeltaTime / config.shakeDecaySeconds);
        }
    }
}
