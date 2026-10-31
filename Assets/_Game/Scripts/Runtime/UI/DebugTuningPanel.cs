using System;
using System.Text;
using Game.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Hidden playtest panel: triple-tap the level label to toggle. Sliders edit the runtime ShotTuning clone only,
    /// never the asset; Reset restores the asset values. The readout line is for screenshots of a setup worth keeping.
    /// </summary>
    public sealed class DebugTuningPanel : MonoBehaviour
    {
        private const float TripleTapWindowSeconds = 1f;

        [SerializeField] private GameBootstrap bootstrap;
        [SerializeField] private ShotTuning tuningAsset;
        [SerializeField] private Button toggleTarget;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Slider[] sliders;
        [SerializeField] private Text[] sliderLabels;
        [SerializeField] private Text readout;
        [SerializeField] private Button resetButton;

        private readonly TripleTapDetector tripleTap = new TripleTapDetector(TripleTapWindowSeconds);
        private readonly StringBuilder text = new StringBuilder();
        private Knob[] knobs;
        private bool isSyncing;

        private readonly struct Knob
        {
            public readonly string Name;
            public readonly float Min;
            public readonly float Max;
            public readonly Func<ShotTuning, float> Get;
            public readonly Action<ShotTuning, float> Set;

            public Knob(string name, float min, float max, Func<ShotTuning, float> get, Action<ShotTuning, float> set)
            {
                Name = name;
                Min = min;
                Max = max;
                Get = get;
                Set = set;
            }
        }

        private static Knob[] CreateKnobs() => new[]
        {
            new Knob("minSpeed", 2f, 8f, t => t.minSpeed, (t, v) => t.minSpeed = v),
            new Knob("maxSpeed", 5f, 15f, t => t.maxSpeed, (t, v) => t.maxSpeed = v),
            new Knob("maxElevationDeg", 10f, 40f, t => t.maxElevationDeg, (t, v) => t.maxElevationDeg = v),
            new Knob("impactSpeedMultiplier", 1f, 3f, t => t.impactSpeedMultiplier, (t, v) => t.impactSpeedMultiplier = v),
            new Knob("burstStrength", 0f, 2f, t => t.burstStrength, (t, v) => t.burstStrength = v),
            new Knob("previewSeconds", 0f, 0.6f, t => t.previewSeconds, (t, v) => t.previewSeconds = v),
            new Knob("gravityY", -20f, -5f, t => t.gravityY, (t, v) =>
            {
                t.gravityY = v;
                Physics.gravity = new Vector3(0f, v, 0f);
            }),
        };

        private void Awake()
        {
            knobs = CreateKnobs();
            if (sliders.Length != knobs.Length || sliderLabels.Length != knobs.Length)
            {
                Debug.LogError($"{nameof(DebugTuningPanel)} needs {knobs.Length} sliders and labels; panel disabled.", this);
                enabled = false;
                return;
            }
            for (int i = 0; i < knobs.Length; i++)
            {
                int index = i;
                sliders[i].minValue = knobs[i].Min;
                sliders[i].maxValue = knobs[i].Max;
                sliders[i].onValueChanged.AddListener(v => OnSlider(index, v));
            }
            toggleTarget.onClick.AddListener(OnToggleTapped);
            resetButton.onClick.AddListener(ResetToAsset);
            panelRoot.SetActive(false);
        }

        private void OnToggleTapped()
        {
            if (!tripleTap.Tap(Time.unscaledTime)) return;
            bool show = !panelRoot.activeSelf;
            panelRoot.SetActive(show);
            if (show) SyncFromTuning();
        }

        private void OnSlider(int index, float value)
        {
            if (isSyncing) return;
            knobs[index].Set(bootstrap.Tuning, value);
            RefreshTexts();
        }

        private void ResetToAsset()
        {
            var tuning = bootstrap.Tuning;
            foreach (var knob in knobs) knob.Set(tuning, knob.Get(tuningAsset));
            SyncFromTuning();
        }

        private void SyncFromTuning()
        {
            isSyncing = true;
            for (int i = 0; i < knobs.Length; i++) sliders[i].value = knobs[i].Get(bootstrap.Tuning);
            isSyncing = false;
            RefreshTexts();
        }

        private void RefreshTexts()
        {
            var tuning = bootstrap.Tuning;
            text.Clear();
            for (int i = 0; i < knobs.Length; i++)
            {
                float value = knobs[i].Get(tuning);
                sliderLabels[i].text = $"{knobs[i].Name}: {value:0.00}";
                if (i > 0) text.Append("  ");
                text.Append(knobs[i].Name).Append('=').Append(value.ToString("0.00"));
            }
            readout.text = text.ToString();
        }
    }
}
