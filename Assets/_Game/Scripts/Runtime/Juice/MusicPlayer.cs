using UnityEngine;

namespace Game.Juice
{
    /// <summary>One looping music track per room, faded in; playing the same clip again keeps it going.</summary>
    public sealed class MusicPlayer : MonoBehaviour
    {
        [SerializeField] private JuiceConfig config;

        private AudioSource source;
        private float fadeSeconds;
        private float fadeAge;

        public AudioClip Current => source != null ? source.clip : null;

        public void Play(AudioClip loop, float fade)
        {
            EnsureSource();
            if (loop == null)
            {
                source.Stop();
                source.clip = null;
                return;
            }
            if (source.clip == loop && source.isPlaying) return;
            source.clip = loop;
            source.volume = 0f;
            fadeSeconds = Mathf.Max(0f, fade);
            fadeAge = 0f;
            source.Play();
        }

        private void Awake() => EnsureSource();

        private void Update()
        {
            if (source == null || !source.isPlaying || fadeAge >= fadeSeconds && source.volume >= config.musicVolume) return;
            fadeAge += Time.unscaledDeltaTime;
            float t = fadeSeconds <= 0f ? 1f : Mathf.Clamp01(fadeAge / fadeSeconds);
            source.volume = config.musicVolume * t;
        }

        private void EnsureSource()
        {
            if (source != null) return;
            source = gameObject.AddComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
        }
    }
}
