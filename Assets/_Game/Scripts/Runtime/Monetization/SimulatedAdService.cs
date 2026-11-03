using System;
using Game.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Monetization
{
    /// <summary>
    /// A full-screen "Test ad" with a countdown in unscaled time. An interstitial can only be closed after the countdown;
    /// a rewarded ad can be closed at any time, but only pays out after it.
    /// </summary>
    public sealed class SimulatedAdService : MonoBehaviour, IAdService
    {
        [SerializeField] private AppConfig config;
        [SerializeField] private UiStrings strings;
        [SerializeField] private GameObject overlay;
        [SerializeField] private Text titleText;
        [SerializeField] private Text countdownText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text closeLabel;

        private Action onInterstitialClosed;
        private Action<bool> onRewardedFinished;
        private float endsAt;
        private int shownSeconds = -1;

        /// <summary>Debug "Ads offline": nothing is ready, as with no fill or no network.</summary>
        public bool Offline { get; set; }
        public bool IsShowing { get; private set; }
        public float AdSeconds => config.simulatedAdSeconds;
        public bool InterstitialReady => !Offline && !IsShowing;
        public bool RewardedReady => !Offline && !IsShowing;
        private bool CountdownDone => Time.unscaledTime >= endsAt;

        public void ShowInterstitial(Action closed)
        {
            if (IsShowing) return;
            if (!InterstitialReady)
            {
                closed?.Invoke();
                return;
            }
            onInterstitialClosed = closed;
            Open();
        }

        public void ShowRewarded(Action<bool> finished)
        {
            if (IsShowing) return;
            if (!RewardedReady)
            {
                finished?.Invoke(false);
                return;
            }
            onRewardedFinished = finished;
            Open();
        }

        /// <summary>The close button. Ignored for an interstitial still counting down.</summary>
        public void Close()
        {
            if (!IsShowing || (onInterstitialClosed != null && !CountdownDone)) return;
            bool rewarded = CountdownDone;
            var interstitial = onInterstitialClosed;
            var rewardedAd = onRewardedFinished;
            onInterstitialClosed = null;
            onRewardedFinished = null;
            IsShowing = false;
            overlay.SetActive(false);
            interstitial?.Invoke();
            rewardedAd?.Invoke(rewarded);
        }

        private void Awake()
        {
            overlay.SetActive(false);
            closeButton.onClick.AddListener(Close);
            titleText.text = strings.testAd;
            closeLabel.text = strings.close;
        }

        private void Open()
        {
            IsShowing = true;
            endsAt = Time.unscaledTime + config.simulatedAdSeconds;
            shownSeconds = -1;
            overlay.SetActive(true);
            Refresh();
        }

        private void Update()
        {
            if (IsShowing) Refresh();
        }

        private void Refresh()
        {
            int seconds = Mathf.Max(0, Mathf.CeilToInt(endsAt - Time.unscaledTime));
            if (seconds == shownSeconds) return;
            shownSeconds = seconds;
            countdownText.text = seconds > 0 ? seconds.ToString() : "";
            closeButton.interactable = onInterstitialClosed == null || seconds == 0;
        }
    }
}
