using UnityEngine;

namespace Game.Meta
{
    /// <summary>App, ad and purchase numbers (spec §8). Gameplay numbers stay in ShotTuning.</summary>
    [CreateAssetMenu(menuName = "Smash/App Config")]
    public sealed class AppConfig : ScriptableObject
    {
        public float splashSeconds = 1.2f;
        [Tooltip("After a win, the confetti and the cat's celebration play this long before the result panel opens.")]
        public float winPanelDelaySeconds = 1.3f;
        [Tooltip("Hidden in settings while empty.")]
        public string privacyPolicyUrl = "";

        [Header("Interstitials (spec §8)")]
        [Tooltip("1-based: none after levels 1–5.")]
        public int firstInterstitialLevel = 6;
        public int minLevelsBetweenInterstitials = 2;
        public float minSecondsBetweenInterstitials = 90f;

        [Header("Simulated services (M3)")]
        public float simulatedAdSeconds = 3f;
        public string removeAdsProductId = "remove_ads";
        public string simulatedPrice = "$3.99";
    }
}
