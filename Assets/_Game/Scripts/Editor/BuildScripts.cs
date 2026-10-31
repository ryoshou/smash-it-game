using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>Device builds for playtesting. Android is deferred until release (owner has no Android phone).</summary>
    public static class BuildScripts
    {
        public const string IosOutput = "Builds/iOS";

        /// <summary>Development Xcode project for the Game scene. Signing happens in Xcode with the owner's Apple ID.</summary>
        [MenuItem("Smash/Build iOS (development)")]
        public static void BuildIos()
        {
            var options = new BuildPlayerOptions
            {
                scenes = new[] { PrototypeSceneBuilder.ScenePath },
                locationPathName = IosOutput,
                target = BuildTarget.iOS,
                targetGroup = BuildTargetGroup.iOS,
                options = BuildOptions.Development,
            };
            Report(BuildPipeline.BuildPlayer(options));
        }

        private static void Report(BuildReport report)
        {
            var summary = report.summary;
            Debug.Log($"[BuildScripts] {summary.platform} {summary.result}: {summary.outputPath}, " +
                      $"{summary.totalSize / (1024f * 1024f):0.0} MB, {summary.totalErrors} errors, {summary.totalTime:mm\\:ss}");
            if (summary.result == BuildResult.Succeeded) return;
            if (Application.isBatchMode) EditorApplication.Exit(1);
            throw new InvalidOperationException($"{summary.platform} build {summary.result} with {summary.totalErrors} errors.");
        }
    }
}
