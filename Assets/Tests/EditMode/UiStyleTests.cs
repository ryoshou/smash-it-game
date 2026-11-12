using Game.Editor;
using Game.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Tests
{
    /// <summary>The candy pieces every screen is built from: thin rims, a short lip, labels that shrink to fit.</summary>
    public sealed class UiStyleTests
    {
        private GameObject canvas;
        private RectTransform root;

        [SetUp]
        public void SetUp()
        {
            canvas = new GameObject("Canvas", typeof(Canvas));
            root = new GameObject("Play", typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(canvas.transform, false);
            root.sizeDelta = new Vector2(400f, UiStyle.IconSize);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(canvas);

        [Test]
        public void Candy_HasFaceRimLipAndBestFitLabel()
        {
            var (_, label) = UiStyle.Candy(root, CandyLook.Green, 64);

            var face = root.Find("Face").GetComponent<Image>();
            Assert.AreSame(UiSprites.CandyFace, face.sprite);
            Assert.AreEqual(CandyLook.Green.Top, face.color);
            var rim = face.transform.Find("Rim").GetComponent<Image>();
            Assert.AreSame(UiSprites.CandyRim, rim.sprite);
            Assert.AreEqual(CandyLook.Green.Rim, rim.color);
            Assert.AreEqual(-UiStyle.CandyLip, face.GetComponent<Shadow>().effectDistance.y);
            Assert.AreEqual(8f, UiStyle.CandyLip);
            Assert.IsTrue(label.resizeTextForBestFit);
            Assert.LessOrEqual(label.resizeTextMinSize, 24);
            Assert.AreEqual(Color.white, label.color);
            Assert.AreEqual(2f, label.GetComponent<Outline>().effectDistance.x);
        }

        [Test]
        public void Candy_PressSinksTheFace()
        {
            UiStyle.Candy(root, CandyLook.Blue, 64);

            var press = new SerializedObject(root.GetComponent<ButtonPress>());
            Assert.AreSame(root.Find("Face"), press.FindProperty("face").objectReferenceValue);
            Assert.AreSame(root.Find("Face").GetComponent<Shadow>(), press.FindProperty("lip").objectReferenceValue);
        }

        [Test]
        public void IconButton_ShowsIconNotText()
        {
            var button = UiStyle.IconButton(root, CandyLook.Yellow, UiSprites.IconGear);

            Assert.IsNotNull(button);
            Assert.IsEmpty(root.GetComponentsInChildren<Text>(true));
            Assert.IsTrue(System.Array.Exists(root.GetComponentsInChildren<Image>(true), i => i.sprite == UiSprites.IconGear));
            Assert.IsNotNull(root.GetComponent<ButtonPress>());
        }

        /// <summary>The face sits above the lip, so the root is a lip taller than wide and the face is a true circle.</summary>
        [Test]
        public void IconButton_FaceIsRound()
        {
            root.sizeDelta = UiStyle.IconButtonSize;

            UiStyle.IconButton(root, CandyLook.Yellow, UiSprites.IconGear);

            var face = ((RectTransform)root.Find("Face")).rect;
            Assert.AreEqual(face.width, face.height, 0.01f);
        }

        [Test]
        public void Badge_HasWhiteBestFitLabel()
        {
            var label = UiStyle.Badge(root, CandyLook.Purple, 54);

            Assert.AreEqual(Color.white, label.color);
            Assert.IsTrue(label.resizeTextForBestFit);
            Assert.AreEqual(28, label.resizeTextMinSize);
            Assert.IsNull(root.GetComponent<Button>(), "a badge is not a button");
        }

        [Test]
        public void RibbonPanel_ReturnsTheRibbonsTitle()
        {
            var title = UiStyle.RibbonPanel(root, "Paused");

            Assert.AreEqual("Paused", title.text);
            Assert.AreEqual("Ribbon", title.transform.parent.name);
            Assert.AreEqual(CandyLook.Cream.Top, root.GetComponent<Image>().color);
        }
    }
}
