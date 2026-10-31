using UnityEngine;

namespace Game.Juice
{
    /// <summary>Feedback tuning (sound, shake, haptics). Gameplay never reads this.</summary>
    [CreateAssetMenu(menuName = "Smash/Juice Config")]
    public sealed class JuiceConfig : ScriptableObject
    {
        [Header("Audio")]
        public int sfxVoices = 8;

        [Header("Camera shake")]
        public float shakeMax = 0.15f;
        public float shakeDecaySeconds = 0.25f;
    }
}
