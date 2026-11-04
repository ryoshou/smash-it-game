using System.Collections;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>The hidden tuning panel exists only in development builds (the owner's test build keeps it).</summary>
    public sealed class DebugPanelPlayTests
    {
        private DebugTuningPanel panel;
        private Button opener;
        private GameObject panelRoot;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            AppTestKit.ClearSave();
            yield return AppTestKit.LaunchAndPlay();
            panel = Object.FindAnyObjectByType<DebugTuningPanel>(FindObjectsInactive.Include);
            var so = new UnityEditor.SerializedObject(panel);
            opener = (Button)so.FindProperty("toggleTarget").objectReferenceValue;
            panelRoot = (GameObject)so.FindProperty("panelRoot").objectReferenceValue;
        }

        [TearDown]
        public void TearDown() => AppTestKit.ClearSave();

        private void TripleTap()
        {
            for (int i = 0; i < 3; i++) opener.onClick.Invoke();
        }

        [UnityTest]
        public IEnumerator DebugPanel_NotAllowed_HidesPanelAndOpener()
        {
            panel.ApplyBuildGate(false);
            yield return null;

            Assert.IsFalse(opener.enabled, "the level label must not open anything");
            Assert.IsFalse(opener.targetGraphic.raycastTarget);
            TripleTap();
            Assert.IsFalse(panelRoot.activeSelf);
            Assert.IsTrue(opener.gameObject.activeInHierarchy, "the level label itself still shows");
        }

        [UnityTest]
        public IEnumerator DebugPanel_Allowed_OpenerVisible()
        {
            panel.ApplyBuildGate(true);
            yield return null;

            Assert.IsTrue(opener.enabled);
            TripleTap();
            Assert.IsTrue(panelRoot.activeSelf);
            Assert.IsNotNull(panelRoot.GetComponentInChildren<DebugMetaButtons>(true) ?? panelRoot.GetComponent<DebugMetaButtons>());
        }
    }
}
