using System;

namespace Game.Monetization
{
    /// <summary>
    /// Interstitial and rewarded ads. M3: SimulatedAdService; M4: AppLovin MAX behind the same calls. Each Show calls
    /// back exactly once, also when no ad is ready or it fails to show (closed() / finished(false)); a call made while
    /// an ad is already showing is ignored.
    /// </summary>
    public interface IAdService
    {
        bool InterstitialReady { get; }
        bool RewardedReady { get; }
        void ShowInterstitial(Action closed);
        /// <summary>finished(true) only when the ad was watched to the end.</summary>
        void ShowRewarded(Action<bool> finished);
    }
}
