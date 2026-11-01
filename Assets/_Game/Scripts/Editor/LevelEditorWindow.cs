using System;
using System.Linq;
using Game.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// Smash/Level Editor: open a level into the scene, add props from a palette (dropped on top of whatever is at
    /// the desk centre), move them with the usual gizmos, Snap & Stack the selection, set shots, difficulty and
    /// targets, Save (validated) and Preview in Play mode (spec §6.3).
    /// </summary>
    public sealed class LevelEditorWindow : EditorWindow
    {
        private LevelDef level;
        private LevelList list;
        private PropCatalog catalog;
        private Transform root;
        private int shots;
        private Difficulty difficulty;
        private string status = "";
        private Vector2 scroll;

        [MenuItem("Smash/Level Editor")]
        public static void Open() => GetWindow<LevelEditorWindow>("Level Editor");

        private void OnEnable() =>
            catalog = AssetDatabase.LoadAssetAtPath<PropCatalog>(PrototypeAssetBuilder.CatalogPath);

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            list = (LevelList)EditorGUILayout.ObjectField("Level list", list, typeof(LevelList), false);
            level = (LevelDef)EditorGUILayout.ObjectField("Level", level, typeof(LevelDef), false);
            using (new EditorGUI.DisabledScope(level == null || catalog == null))
            {
                if (GUILayout.Button("Open in scene")) OpenLevel();
                using (new EditorGUI.DisabledScope(root == null))
                {
                    DrawPalette();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Snap & Stack selection")) SnapSelection();
                    DrawTargetToggle();
                    shots = EditorGUILayout.IntSlider("Shots", shots, LevelDef.MinShots, LevelDef.MaxShots);
                    difficulty = (Difficulty)EditorGUILayout.EnumPopup("Difficulty", difficulty);
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Save")) SaveLevel();
                    using (new EditorGUI.DisabledScope(list == null)) if (GUILayout.Button("Save & Preview")) Preview();
                    EditorGUILayout.EndHorizontal();
                }
            }
            if (status.Length > 0) EditorGUILayout.HelpBox(status, MessageType.Info);
            EditorGUILayout.EndScrollView();
        }

        private void OpenLevel()
        {
            if (root != null) DestroyImmediate(root.gameObject);
            root = LevelEditing.Spawn(level, catalog, EditorSceneManager.GetActiveScene());
            shots = level.shots;
            difficulty = level.difficulty;
            Selection.activeTransform = root;
            status = $"Editing {level.id}: {level.props.Length} props.";
        }

        private void DrawPalette()
        {
            EditorGUILayout.LabelField("Add prop", EditorStyles.boldLabel);
            foreach (var spec in catalog.specs)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(spec.kind.ToString(), GUILayout.Width(110));
                for (int v = 0; v < spec.variants.Length; v++)
                    if (GUILayout.Button(spec.Variant(v).prefab.name, GUILayout.MinWidth(40))) AddProp(spec, v);
                EditorGUILayout.EndHorizontal();
            }
        }

        private void AddProp(PropSpec spec, int variant)
        {
            var current = LevelEditing.Capture(root, catalog);
            float y = LayoutSnap.StackHeight(Vector2.zero, new Vector2(spec.size.x, spec.size.z), 0f, current, catalog);
            var placement = new PropPlacement { kind = spec.kind, variant = variant, isTarget = true, basePosition = new Vector3(0f, y, 0f) };
            var instance = LevelEditing.Add(root, catalog, placement);
            Undo.RegisterCreatedObjectUndo(instance, "Add prop");
            Selection.activeGameObject = instance;
        }

        /// <summary>Snaps each selected prop to the grid, then drops it onto whatever is underneath (lowest first).</summary>
        private void SnapSelection()
        {
            var selected = Selection.transforms.Where(t => t.parent == root).OrderBy(t => t.position.y).ToArray();
            int snapped = 0;
            foreach (var t in selected)
            {
                if (!LevelEditing.TryCapture(t, catalog, out var placement)) continue;
                var others = root.Cast<Transform>()
                    .Where(other => other != t)
                    .Select(other => LevelEditing.TryCapture(other, catalog, out var p) ? (PropPlacement?)p : null)
                    .Where(p => p.HasValue).Select(p => p.Value).ToArray();
                var size = catalog.Get(placement.kind).size;
                var target = LayoutSnap.SnapXZ(placement.basePosition, LayoutSnap.DefaultGrid);
                target.y = LayoutSnap.StackHeight(new Vector2(target.x, target.z), new Vector2(size.x, size.z),
                    placement.yawDegrees, others, catalog);
                Undo.RecordObject(t, "Snap & Stack");
                t.position += target - placement.basePosition;
                snapped++;
            }
            status = $"Snapped {snapped} prop(s).";
        }

        private void DrawTargetToggle()
        {
            var go = Selection.activeGameObject;
            if (go == null || go.transform.parent != root) return;
            bool isTarget = LevelEditing.IsTarget(go);
            bool toggled = EditorGUILayout.Toggle("Selected is a target", isTarget);
            if (toggled == isTarget) return;
            Undo.RecordObject(go, "Toggle target");
            LevelEditing.SetTarget(go, toggled);
        }

        /// <returns>True when saved.</returns>
        private bool SaveLevel()
        {
            var props = LevelEditing.Capture(root, catalog, out int skipped);
            string error = LevelEditing.Save(level, props, shots, difficulty);
            string warning = skipped > 0 ? $" ({skipped} renamed object(s) under the level root were not saved)" : "";
            status = (error ?? $"Saved {level.id}.") + warning;
            return error == null;
        }

        /// <summary>Saves, then plays the level; an invalid level keeps the edit open and nothing is discarded.</summary>
        private void Preview()
        {
            int index = Array.IndexOf(list.levels, level);
            if (index < 0)
            {
                status = $"{level.id} is not in {list.name}.";
                return;
            }
            if (!SaveLevel()) return;
            // The level asset holds the edit now; drop the edit props so they are never saved into a scene.
            DestroyImmediate(root.gameObject);
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(PrototypeSceneBuilder.ScenePath);
            EditorStartLevel.Set(index);
            EditorApplication.isPlaying = true;
        }
    }
}
