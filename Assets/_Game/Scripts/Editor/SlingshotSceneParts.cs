using System;
using Game.Juice;
using Game.View;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>Placeholder slingshot: handle, two fork posts, rubber bands, pouch and preview dots.</summary>
    internal static class SlingshotSceneParts
    {
        private const string JuiceConfigPath = PrototypeAssetBuilder.ConfigFolder + "/JuiceConfig.asset";
        private const int PreviewDots = 8;
        private const float ForkHalfWidth = 0.13f;
        private const float ForkHeight = 0.16f;
        private const float PostThickness = 0.025f;
        private const float HandleLength = 0.35f;
        private const float BandWidth = 0.012f;
        private const float PouchSize = 0.12f;
        private const float DotSize = 0.035f;

        public static SlingshotView BuildSlingshot(Transform pouch, string materialsFolder, Func<string, Color, Material> material)
        {
            var wood = material("SlingshotWood", new Color(0.62f, 0.42f, 0.3f));
            var rubber = material("SlingshotBand", new Color(0.55f, 0.3f, 0.45f));
            var root = new GameObject("Slingshot").transform;
            root.position = pouch.position + new Vector3(0f, -ForkHeight, 0.06f);

            var handle = PrototypeSceneBuilder.Visual(PrimitiveType.Cube, root, "Handle", wood);
            handle.localScale = new Vector3(PostThickness * 1.4f, HandleLength, PostThickness * 1.4f);
            handle.localPosition = new Vector3(0f, -HandleLength / 2f, 0f);
            var left = Post(root, wood, -1f);
            var right = Post(root, wood, 1f);

            var pouchVisual = PrototypeSceneBuilder.Visual(PrimitiveType.Sphere, root, "Pouch Visual", rubber);
            pouchVisual.localScale = Vector3.one * PouchSize;
            pouchVisual.position = pouch.position;

            var view = root.gameObject.AddComponent<SlingshotView>();
            PrototypeSceneBuilder.SetReference(view, "juice", LoadOrCreateJuice());
            PrototypeSceneBuilder.SetReference(view, "pouchRest", pouch);
            PrototypeSceneBuilder.SetReference(view, "pouchVisual", pouchVisual);
            PrototypeSceneBuilder.SetReference(view, "forkLeftTip", left);
            PrototypeSceneBuilder.SetReference(view, "forkRightTip", right);
            PrototypeSceneBuilder.SetReference(view, "bandLeft", Band(root, "Band Left", rubber));
            PrototypeSceneBuilder.SetReference(view, "bandRight", Band(root, "Band Right", rubber));
            SetDots(view, root, material("PreviewDot", Color.white));
            return view;
        }

        /// <summary>Builds one fork post and returns its tip.</summary>
        private static Transform Post(Transform root, Material wood, float side)
        {
            var post = PrototypeSceneBuilder.Visual(PrimitiveType.Cube, root, side < 0 ? "Fork Left" : "Fork Right", wood);
            post.localScale = new Vector3(PostThickness, ForkHeight * 1.2f, PostThickness);
            post.localPosition = new Vector3(side * ForkHalfWidth, ForkHeight * 0.6f, 0f);
            var tip = new GameObject("Tip").transform;
            tip.SetParent(root, false);
            tip.localPosition = new Vector3(side * ForkHalfWidth, ForkHeight * 1.2f, 0f);
            return tip;
        }

        private static LineRenderer Band(Transform root, string name, Material rubber)
        {
            var band = new GameObject(name).AddComponent<LineRenderer>();
            band.transform.SetParent(root, false);
            band.positionCount = 2;
            band.useWorldSpace = true;
            band.widthMultiplier = BandWidth;
            band.sharedMaterial = rubber;
            band.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return band;
        }

        private static void SetDots(SlingshotView view, Transform root, Material dotMaterial)
        {
            var so = new SerializedObject(view);
            var dots = so.FindProperty("dots");
            dots.arraySize = PreviewDots;
            for (int i = 0; i < PreviewDots; i++)
            {
                var dot = PrototypeSceneBuilder.Visual(PrimitiveType.Sphere, root, $"Dot {i}", dotMaterial);
                dot.localScale = Vector3.one * DotSize;
                dot.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                dots.GetArrayElementAtIndex(i).objectReferenceValue = dot;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static JuiceConfig LoadOrCreateJuice()
        {
            var juice = AssetDatabase.LoadAssetAtPath<JuiceConfig>(JuiceConfigPath);
            if (juice != null) return juice;
            juice = ScriptableObject.CreateInstance<JuiceConfig>();
            AssetDatabase.CreateAsset(juice, JuiceConfigPath);
            return juice;
        }
    }
}
