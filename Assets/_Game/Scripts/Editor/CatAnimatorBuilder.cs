using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>
    /// The cat's Animator: Idle (loops) ↔ Aim (pose sampled by the Pull parameter), and Release / Celebrate / Annoyed
    /// triggers from any state, each returning to Idle. Also writes the Cat prefab. Safe to re-run.
    /// </summary>
    public static class CatAnimatorBuilder
    {
        public const string ModelPath = "Assets/_Game/Art/Generated/Cat.fbx";
        public const string ControllerPath = "Assets/_Game/Animation/Cat.controller";
        public const string PrefabPath = "Assets/_Game/Prefabs/Cat.prefab";
        private const float BlendSeconds = 0.12f;

        [MenuItem("Smash/Build Cat Animator")]
        public static GameObject BuildAll()
        {
            LoopIdle();
            var controller = BuildController();
            return BuildPrefab(controller);
        }

        private static void LoopIdle()
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(ModelPath);
            var clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
            foreach (var clip in clips)
            {
                clip.name = TakeName(clip.takeName);
                clip.loopTime = clip.name == "Idle";
            }
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }

        /// <summary>FBX takes are named "Rig|Action"; the clip keeps the action name.</summary>
        private static string TakeName(string take) => take.Contains("|") ? take.Substring(take.LastIndexOf('|') + 1) : take;

        private static AnimatorController BuildController()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ControllerPath));
            AssetDatabase.DeleteAsset(ControllerPath);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Aiming", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Pull", AnimatorControllerParameterType.Float);
            foreach (var trigger in new[] { "Release", "Celebrate", "Annoyed" })
                controller.AddParameter(trigger, AnimatorControllerParameterType.Trigger);

            var machine = controller.layers[0].stateMachine;
            var idle = machine.AddState("Idle");
            idle.motion = Clip("Idle");
            machine.defaultState = idle;

            var aim = machine.AddState("Aim");
            aim.motion = Clip("Aim");
            aim.timeParameterActive = true;
            aim.timeParameter = "Pull";
            Transition(idle.AddTransition(aim), "Aiming", true);
            Transition(aim.AddTransition(idle), "Aiming", false);

            foreach (var name in new[] { "Release", "Celebrate", "Annoyed" })
            {
                var state = machine.AddState(name);
                state.motion = Clip(name);
                var enter = machine.AddAnyStateTransition(state);
                enter.AddCondition(AnimatorConditionMode.If, 0f, name);
                enter.duration = BlendSeconds;
                enter.hasExitTime = false;
                enter.canTransitionToSelf = false;
                var leave = state.AddTransition(idle);
                leave.hasExitTime = true;
                leave.exitTime = 1f;
                leave.duration = BlendSeconds;
            }
            AssetDatabase.SaveAssets();
            return controller;
        }

        private static void Transition(AnimatorStateTransition transition, string parameter, bool value)
        {
            transition.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, parameter);
            transition.hasExitTime = false;
            transition.duration = BlendSeconds;
        }

        private static AnimationClip Clip(string name)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(ModelPath))
                if (asset is AnimationClip clip && clip.name == name && !clip.name.StartsWith("__preview")) return clip;
            throw new FileNotFoundException($"Cat take '{name}' missing in {ModelPath} (run tools/blender/build.sh cat)");
        }

        private static GameObject BuildPrefab(AnimatorController controller)
        {
            var root = new GameObject("Cat");
            var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath), root.transform);
            model.name = "Model";
            var material = AtlasMaterialBuilder.Load("TinyTreats");
            foreach (var renderer in model.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = material;
            if (!model.TryGetComponent(out Animator animator)) animator = model.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }
    }
}
