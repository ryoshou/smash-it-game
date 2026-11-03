using System;
using Game.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Monetization
{
    /// <summary>
    /// A "Test purchase" confirm dialog. Ownership is kept "store-side" in its own PlayerPrefs key, apart from the save,
    /// so Reset save followed by Restore exercises the real restore path.
    /// </summary>
    public sealed class SimulatedPurchaseService : MonoBehaviour, IPurchaseService
    {
        public const string OwnershipKey = "smash.simstore.remove_ads";

        [SerializeField] private AppConfig config;
        [SerializeField] private UiStrings strings;
        [SerializeField] private GameObject dialog;
        [SerializeField] private Text messageText;
        [SerializeField] private Button buyButton;
        [SerializeField] private Text buyLabel;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Text cancelLabel;

        private Action<bool> pending;

        public bool IsShowing => pending != null;
        public string RemoveAdsPrice => config.simulatedPrice;

        public void BuyRemoveAds(Action<bool> purchased)
        {
            if (IsShowing || purchased == null) return;
            pending = purchased;
            messageText.text = string.Format(strings.testPurchaseFormat, RemoveAdsPrice);
            dialog.SetActive(true);
        }

        public void RestoreRemoveAds(Action<bool> owned) => owned?.Invoke(PlayerPrefs.GetInt(OwnershipKey, 0) == 1);

        public void Confirm()
        {
            if (!IsShowing) return;
            PlayerPrefs.SetInt(OwnershipKey, 1);
            PlayerPrefs.Save();
            Finish(true);
        }

        public void Cancel()
        {
            if (IsShowing) Finish(false);
        }

        private void Awake()
        {
            dialog.SetActive(false);
            buyButton.onClick.AddListener(Confirm);
            cancelButton.onClick.AddListener(Cancel);
            buyLabel.text = strings.buy;
            cancelLabel.text = strings.cancel;
        }

        private void Finish(bool purchased)
        {
            var callback = pending;
            pending = null;
            dialog.SetActive(false);
            callback(purchased);
        }
    }
}
