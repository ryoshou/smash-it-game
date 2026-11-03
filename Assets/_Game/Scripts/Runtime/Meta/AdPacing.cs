namespace Game.Meta
{
    /// <summary>
    /// Spec §8 interstitial pacing: only after a win, none after levels before firstInterstitialLevel, at most one per
    /// minLevelsBetweenInterstitials wins and per minSecondsBetweenInterstitials. The session start counts as the last ad.
    /// </summary>
    public sealed class AdPacing
    {
        private readonly AppConfig config;
        private float lastAdTime;
        private int winsSinceAd;

        public AdPacing(AppConfig config, float sessionStart)
        {
            this.config = config;
            lastAdTime = sessionStart;
            winsSinceAd = config.minLevelsBetweenInterstitials;
        }

        public void LevelWon() => winsSinceAd++;

        public void InterstitialShown(float now)
        {
            lastAdTime = now;
            winsSinceAd = 0;
        }

        public bool ShouldShowInterstitial(int wonLevelNumber, float now, bool removeAds) =>
            !removeAds
            && wonLevelNumber >= config.firstInterstitialLevel
            && winsSinceAd >= config.minLevelsBetweenInterstitials
            && now - lastAdTime >= config.minSecondsBetweenInterstitials;
    }
}
