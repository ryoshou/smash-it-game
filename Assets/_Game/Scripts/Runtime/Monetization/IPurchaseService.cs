using System;

namespace Game.Monetization
{
    /// <summary>The one non-consumable, Remove Ads (spec §8). M3: SimulatedPurchaseService; M4: Unity IAP.</summary>
    public interface IPurchaseService
    {
        string RemoveAdsPrice { get; }
        void BuyRemoveAds(Action<bool> purchased);
        void RestoreRemoveAds(Action<bool> owned);
    }
}
