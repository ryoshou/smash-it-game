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
        private const float ForkHeight = 0.16f;
        private const float BandWidth = 0.012f;
        private const float PouchSize = 0.12f;
        private const float DotSize = 0.035f;

        public static SlingshotView BuildSlingshot(Transform pouch, string materialsFolder, Func<string, Color, Material> material)
        {
            var rubber = material("SlingshotBand", new Color(0.55f, 0.3f, 0.45f));
            var root = new GameObject("Slingshot").transform;
            root.position = pouch.position + new Vector3(0f, -ForkHeight, 0.06f);

            // The generated Y frame: its origin is this root and its band anchors sit at the fork tips.
            var frame = PrototypeSceneBuilder.ArtVisual(root, "Generated/Slingshot.fbx", "TinyTreats", Vector3.zero, 1f);
            var left = frame.Find("BandAnchorL");
            var right = frame.Find("BandAnchorR");

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

        public static JuiceConfig LoadOrCreateJuice()
        {
            var juice = AssetDatabase.LoadAssetAtPath<JuiceConfig>(JuiceConfigPath);
            if (juice != null) return juice;
            juice = ScriptableObject.CreateInstance<JuiceConfig>();
            AssetDatabase.CreateAsset(juice, JuiceConfigPath);
            return juice;
        }
    }
}
