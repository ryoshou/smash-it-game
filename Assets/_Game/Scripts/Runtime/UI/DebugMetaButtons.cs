using Game.Monetization;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>Playtest-only buttons on the hidden debug panel: wipe the save, take the test ads offline, jump to the next room.</summary>
    public sealed class DebugMetaButtons : MonoBehaviour
    {
        [SerializeField] private AppController app;
        [SerializeField] private SimulatedAdService ads;
        [SerializeField] private Button resetSaveButton;
        [SerializeField] private Button adsOfflineButton;
        [SerializeField] private Text adsOfflineLabel;
        [SerializeField] private Button nextRoomButton;

        private void Awake()
        {
            resetSaveButton.onClick.AddListener(app.ResetSave);
            adsOfflineButton.onClick.AddListener(ToggleAdsOffline);
            nextRoomButton.onClick.AddListener(app.JumpToNextRoom);
            RefreshLabel();
        }

        private void ToggleAdsOffline()
        {
            ads.Offline = !ads.Offline;
            RefreshLabel();
        }

        private void RefreshLabel() => adsOfflineLabel.text = ads.Offline ? "Ads: offline" : "Ads: online";
    }
}
