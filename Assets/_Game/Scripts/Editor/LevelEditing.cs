using System;
using System.Collections.Generic;
using Game.Levels;
using Game.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Game.Editor
{
    /// <summary>
    /// The level editor's model: spawn a level's props into a scene for editing, read them back, save with
    /// validation. Each edit instance is named "Kind#variant#T|D" (target or decoration).
    /// </summary>
    public static class LevelEditing
    {
        private const char Separator = '#';
        private const string Target = "T";
        private const string Decoration = "D";

        public static Transform Spawn(LevelDef level, PropCatalog catalog, Scene scene)
        {
            var root = new GameObject($"LevelEdit_{level.id}");
            SceneManager.MoveGameObjectToScene(root, scene);
            foreach (var placement in level.props) Add(root.transform, catalog, placement);
            return root.transform;
        }

        /// <summary>Adds one prop as an edit instance (kinematic: nothing simulates in the editor).</summary>
        public static GameObject Add(Transform root, PropCatalog catalog, PropPlacement placement)
        {
            var spec = catalog.Get(placement.kind);
            int variant = spec.VariantIndex(placement.variant);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(spec.Variant(variant).prefab, root);
            instance.name = Name(placement.kind, variant, placement.isTarget);
            instance.transform.SetPositionAndRotation(WorldPosition(placement.basePosition, spec.size),
                Quaternion.Euler(0f, placement.yawDegrees, 0f));
            foreach (var body in instance.GetComponentsInChildren<Rigidbody>()) body.isKinematic = true;
            return instance;
        }

        public static PropPlacement[] Capture(Transform root, PropCatalog catalog)
        {
            var props = new List<PropPlacement>(root.childCount);
            foreach (Transform child in root)
            {
                if (!TryParse(child.name, out var kind, out int variant, out bool isTarget)) continue;
                var size = catalog.Get(kind).size;
                props.Add(new PropPlacement
                {
                    kind = kind,
                    variant = variant,
                    isTarget = isTarget,
                    basePosition = child.position - WorldPosition(Vector3.zero, size),
                    yawDegrees = Mathf.DeltaAngle(0f, child.eulerAngles.y),
                });
            }
            return props.ToArray();
        }

        /// <returns>Null when saved, else the validation error (the level is left unchanged).</returns>
        public static string Save(LevelDef level, PropPlacement[] props, int shots, Difficulty difficulty)
        {
            var candidate = Object.Instantiate(level);
            candidate.props = props;
            candidate.shots = shots;
            candidate.difficulty = difficulty;
            bool isValid = candidate.Validate(out string error);
            Object.DestroyImmediate(candidate);
            if (!isValid) return error;

            Undo.RecordObject(level, "Save level");
            level.props = props;
            level.shots = shots;
            level.difficulty = difficulty;
            EditorUtility.SetDirty(level);
            if (AssetDatabase.Contains(level)) AssetDatabase.SaveAssetIfDirty(level);
            return null;
        }

        public static void SetTarget(GameObject instance, bool isTarget)
        {
            if (TryParse(instance.name, out var kind, out int variant, out _)) instance.name = Name(kind, variant, isTarget);
        }

        public static bool IsTarget(GameObject instance) => TryParse(instance.name, out _, out _, out bool isTarget) && isTarget;

        private static Vector3 WorldPosition(Vector3 basePosition, Vector3 size) =>
            new Vector3(0f, LevelSpawner.DeskTopY, 0f) + basePosition + Vector3.up * (size.y / 2f);

        private static string Name(PropKind kind, int variant, bool isTarget) =>
            $"{kind}{Separator}{variant}{Separator}{(isTarget ? Target : Decoration)}";

        private static bool TryParse(string name, out PropKind kind, out int variant, out bool isTarget)
        {
            kind = default;
            variant = 0;
            isTarget = false;
            string[] parts = name.Split(Separator);
            if (parts.Length != 3 || !Enum.TryParse(parts[0], out kind) || !int.TryParse(parts[1], out variant)) return false;
            isTarget = parts[2] == Target;
            return true;
        }
    }
}
