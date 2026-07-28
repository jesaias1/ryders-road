using System;
using System.IO;
using System.Linq;
using Avoidance.Core.Configuration;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Avoidance.EditorTools
{
    public static class AndroidDevelopmentBuilder
    {
        private const string OutputDirectory = "Builds/Android";

        [MenuItem("Avoidance/Build/Android Development APK")]
        public static void BuildAndroidDevelopment()
        {
            FoundationProjectSetup.Apply();
            var validationErrors = FoundationProjectValidator.Validate();
            if (validationErrors.Count > 0)
            {
                throw new BuildFailedException(
                    "Android build stopped:\n- " + string.Join("\n- ", validationErrors));
            }

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                throw new BuildFailedException(
                    "Android is not the active build target. Switch via File > Build Profiles.");
            }

            var configuration = AssetDatabase.LoadAssetAtPath<GameConfiguration>(
                "Assets/_Game/Core/Configuration/Resources/FoundationGameConfiguration.asset");
            Directory.CreateDirectory(OutputDirectory);
            var outputPath = Path.Combine(
                OutputDirectory,
                $"Avoidance-{configuration.BuildVersion}-dev.apk");
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.Development
                    | BuildOptions.AllowDebugging
                    | BuildOptions.ConnectWithProfiler
            };

            var started = DateTime.UtcNow;
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            var readableSummary =
                $"Android development build\n" +
                $"Result: {summary.result}\n" +
                $"Version: {configuration.BuildVersion}\n" +
                $"Output: {Path.GetFullPath(outputPath)}\n" +
                $"Size: {summary.totalSize:N0} bytes\n" +
                $"Warnings: {summary.totalWarnings}\n" +
                $"Errors: {summary.totalErrors}\n" +
                $"Duration: {DateTime.UtcNow - started}";
            Debug.Log(readableSummary);

            if (summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(readableSummary);
            }
        }
    }
}
