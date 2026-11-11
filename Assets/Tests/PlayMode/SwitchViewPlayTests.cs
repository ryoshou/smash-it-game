using System.Collections;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>The settings switches show their state, including after PausePanel sets it without an event.</summary>
    public sealed class SwitchViewPlayTests
    {
        private static readonly Color OnColor = Color.green;
        private static readonly Color OffColor = Color.grey;

        private GameObject row;
        private Toggle toggle;
        private Image track;
        private RectTransform knob;

        [SetUp]
        public void SetUp()
        {
            row = new GameObject("Row", typeof(RectTransform));
            row.SetActive(false);
            var trackRect = new GameObject("Track", typeof(RectTransform)).GetComponent<RectTransform>();
            trackRect.SetParent(row.transform, false);
            trackRect.sizeDelta = new Vector2(160f, 84f);
            track = trackRect.gameObject.AddComponent<Image>();
            knob = new GameObject("Knob", typeof(RectTransform)).GetComponent<RectTransform>();
            knob.SetParent(trackRect, false);
            knob.anchorMin = knob.anchorMax = new Vector2(1f, 0.5f);
            knob.offsetMin = new Vector2(-78f, -34f);
            knob.offsetMax = new Vector2(-8f, 34f);
            toggle = row.AddComponent<Toggle>();
            toggle.isOn = true;
            var view = row.AddComponent<SwitchView>();
            var so = new UnityEditor.SerializedObject(view);
            so.FindProperty("toggle").objectReferenceValue = toggle;
            so.FindProperty("track").objectReferenceValue = track;
            so.FindProperty("knob").objectReferenceValue = knob;
            so.FindProperty("onColor").colorValue = OnColor;
            so.FindProperty("offColor").colorValue = OffColor;
            so.ApplyModifiedPropertiesWithoutUndo();
            row.SetActive(true);
        }

        [TearDown]
        public void TearDown() => Object.Destroy(row);

        [UnityTest]
        public IEnumerator SetOffWithoutNotify_KnobSlidesLeft_TrackTurnsOff()
        {
            toggle.SetIsOnWithoutNotify(false);
            yield return null;

            Assert.AreEqual(0f, knob.anchorMin.x);
            Assert.AreEqual(8f, knob.offsetMin.x, 1e-3f);
            Assert.AreEqual(78f, knob.offsetMax.x, 1e-3f);
            Assert.AreEqual(OffColor, track.color);
        }

        [UnityTest]
        public IEnumerator OffThenOn_KnobBackAtTheRight_SameSize()
        {
            toggle.isOn = false;
            yield return null;
            toggle.isOn = true;
            yield return null;

            Assert.AreEqual(1f, knob.anchorMin.x);
            Assert.AreEqual(-78f, knob.offsetMin.x, 1e-3f);
            Assert.AreEqual(-8f, knob.offsetMax.x, 1e-3f);
            Assert.AreEqual(OnColor, track.color);
        }
    }
}
