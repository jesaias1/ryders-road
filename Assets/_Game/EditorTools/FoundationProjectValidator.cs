using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avoidance.Core.Configuration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Avoidance.EditorTools
{
    public static class FoundationProjectValidator
    {
        public const string BootstrapScenePath = "Assets/_Game/Levels/Scenes/Bootstrap.unity";
        public const string FoundationScenePath = "Assets/_Game/Levels/Scenes/FoundationTest.unity";

        [MenuItem("Avoidance/Validate Foundation")]
        public static void ValidateFromMenu()
        {
            var errors = Validate();
            if (errors.Count == 0)
            {
                Debug.Log("Foundation validation passed.");
                return;
            }

            foreach (var error in errors)
            {
                Debug.LogError(error);
            }

            throw new InvalidOperationException(
                $"Foundation validation failed with {errors.Count} error(s).");
        }

        public static IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            ValidateScene(BootstrapScenePath, "GameBootstrap", errors);
            ValidateScene(FoundationScenePath, "FoundationTestSceneController", errors);

            var enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            if (enabledScenes.Length < 2
                || enabledScenes[0] != BootstrapScenePath
                || !enabledScenes.Contains(FoundationScenePath))
            {
                errors.Add("Build Settings must contain enabled Bootstrap first and FoundationTest.");
            }

            var configuration = AssetDatabase.LoadAssetAtPath<GameConfiguration>(
                "Assets/_Game/Core/Configuration/Resources/FoundationGameConfiguration.asset");
            if (configuration == null)
            {
                errors.Add("FoundationGameConfiguration asset is missing.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(configuration.BuildVersion))
                {
                    errors.Add("Build version is empty.");
                }

                if (configuration.InitialScene != "FoundationTest")
                {
                    errors.Add("Initial scene must be FoundationTest during Phase 0.");
                }
            }

            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.AutoRotation
                || PlayerSettings.allowedAutorotateToPortrait
                || PlayerSettings.allowedAutorotateToPortraitUpsideDown
                || !PlayerSettings.allowedAutorotateToLandscapeLeft
                || !PlayerSettings.allowedAutorotateToLandscapeRight)
            {
                errors.Add("Player orientation must allow landscape left/right only.");
            }

            if (!(GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset))
            {
                errors.Add("The default render pipeline must be a URP asset.");
            }

            return errors;
        }

        private static void ValidateScene(
            string scenePath,
            string requiredComponentType,
            ICollection<string> errors)
        {
            if (!File.Exists(scenePath))
            {
                errors.Add($"Required scene is missing: {scenePath}");
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            var hasRequiredComponent = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Component>(true))
                .Any(component => component != null
                    && component.GetType().Name == requiredComponentType);
            EditorSceneManager.CloseScene(scene, true);
            if (!hasRequiredComponent)
            {
                errors.Add($"{scenePath} lacks {requiredComponentType}.");
            }
        }
    }
}
