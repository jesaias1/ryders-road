using System;
using System.IO;
using System.Linq;
using System.Reflection;
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

        [MenuItem("RYDERS BLOCK/Build/Android Development APK")]
        public static void BuildAndroidDevelopment()
        {
            ApplyAndroidToolOverridesFromEnvironment();
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
                BuildOutputFileName(configuration));
            PrepareOutputFile(outputPath);

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
                    | BuildOptions.CleanBuildCache
            };

            var started = DateTime.UtcNow;
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            var readableSummary =
                $"Android development build\n" +
                $"Result: {summary.result}\n" +
                $"Version: {configuration.BuildVersion}\n" +
                $"Output: {Path.GetFullPath(outputPath)}\n" +
                $"APK Size: {ResolveOutputFileSize(outputPath):N0} bytes\n" +
                $"Unity Build Size: {summary.totalSize:N0} bytes\n" +
                $"Warnings: {summary.totalWarnings}\n" +
                $"Errors: {summary.totalErrors}\n" +
                $"Duration: {DateTime.UtcNow - started}";
            Debug.Log(readableSummary);

            if (summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(readableSummary);
            }
        }

        private static string BuildOutputFileName(GameConfiguration configuration)
        {
            var version = configuration?.BuildVersion ?? FoundationProjectValidator.BuildVersion;
            return $"RYDERS-ROAD-{version}-dev.apk";
        }

        public static long ResolveOutputFileSize(string outputPath)
        {
            if (string.IsNullOrWhiteSpace(outputPath) || !File.Exists(outputPath))
            {
                return 0L;
            }

            return new FileInfo(outputPath).Length;
        }

        public static void PrepareOutputFile(string outputPath)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                throw new ArgumentException("Android output path is required.", nameof(outputPath));
            }

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }

        private static void ApplyAndroidToolOverridesFromEnvironment()
        {
            SetAndroidToolPathFromEnvironment("RYDERS_BLOCK_ANDROID_JDK", "jdkRootPath");
            SetAndroidToolPathFromEnvironment("RYDERS_BLOCK_ANDROID_NDK", "ndkRootPath");
            SetAndroidToolPathFromEnvironment("RYDERS_BLOCK_ANDROID_SDK", "sdkRootPath");
        }

        private static void SetAndroidToolPathFromEnvironment(
            string variableName,
            string propertyName)
        {
            var path = Environment.GetEnvironmentVariable(variableName);
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (!Directory.Exists(path))
            {
                throw new BuildFailedException(
                    $"{variableName} points to a missing directory: {path}");
            }

            var settingsType = Type.GetType(
                "UnityEditor.Android.AndroidExternalToolsSettings, UnityEditor.Android.Extensions");
            var property = settingsType?.GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (property == null || !property.CanWrite)
            {
                throw new BuildFailedException(
                    $"Could not configure Android external tool path {propertyName}.");
            }

            property.SetValue(null, path);
            Debug.Log($"{variableName} -> {path}");
        }
    }
}
