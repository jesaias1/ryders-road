using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avoidance.Core.Configuration;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Visuals;
using Avoidance.Gameplay.Worlds;
using Avoidance.UI;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
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
        public const string MovementLabScenePath = "Assets/_Game/Levels/Scenes/MovementLab.unity";
        public const string ModuleSelectorScenePath = "Assets/_Game/Levels/Scenes/ModuleSelector.unity";
        public const string ModuleRunnerScenePath = "Assets/_Game/Levels/Scenes/ModuleRunner.unity";
        public const string BrandLogoPath =
            "Assets/Branding/Resources/Branding/RydersRoad_Logo_UI.png";
        public const string BrandSourceLogoPath =
            "Assets/Branding/Source/RydersRoad_Logo_Source.png";
        public const string BrandSourceIconPath =
            "Assets/Branding/Source/RydersRoad_AppIcon_Source.png";
        public const string LegacyIconPath =
            "Assets/Branding/Android/RydersRoad_Icon_Legacy.png";
        public const string AdaptiveIconForegroundPath =
            "Assets/Branding/Android/RydersRoad_Icon_AdaptiveForeground.png";
        public const string AdaptiveIconBackgroundPath =
            "Assets/Branding/Android/RydersRoad_Icon_AdaptiveBackground.png";
        public const string GameVersion = "0.5.0";
        public const string BuildVersion = "0.9.6-movement-mastery-slice";

        [MenuItem("RYDERS BLOCK/Validate Project")]
        public static void ValidateFromMenu()
        {
            var errors = Validate();
            if (errors.Count == 0)
            {
                Debug.Log("RYDER'S ROAD validation passed.");
                return;
            }

            foreach (var error in errors)
            {
                Debug.LogError(error);
            }

            throw new InvalidOperationException(
                $"RYDER'S ROAD validation failed with {errors.Count} error(s).");
        }

        public static IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            ValidateScene(BootstrapScenePath, "GameBootstrap", errors);
            ValidateScene(FoundationScenePath, "FoundationTestSceneController", errors);
            ValidateScene(MovementLabScenePath, "MovementLabSceneController", errors);
            ValidateScene(ModuleSelectorScenePath, "DevelopmentModuleSelector", errors);
            ValidateScene(ModuleRunnerScenePath, "ModuleSceneController", errors);

            var enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            if (enabledScenes.Length < 5
                || enabledScenes[0] != BootstrapScenePath
                || !enabledScenes.Contains(ModuleSelectorScenePath)
                || !enabledScenes.Contains(ModuleRunnerScenePath)
                || !enabledScenes.Contains(MovementLabScenePath)
                || !enabledScenes.Contains(FoundationScenePath))
            {
                errors.Add("Build Settings must contain Bootstrap first, ModuleSelector, MovementLab, ModuleRunner, and FoundationTest.");
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
                else if (configuration.BuildVersion != BuildVersion)
                {
                    errors.Add($"Build version must be {BuildVersion} for the current production phase.");
                }

                if (configuration.GameTitle != BrandPresentation.PlayerFacingTitle)
                {
                    errors.Add($"Game title must be exactly {BrandPresentation.PlayerFacingTitle}.");
                }

                if (configuration.GameVersion != GameVersion)
                {
                    errors.Add($"Game version must be {GameVersion}.");
                }

                if (configuration.InitialScene != "ModuleSelector")
                {
                    errors.Add("Initial scene must be ModuleSelector during current development phases.");
                }

                if (configuration.DeveloperTelemetryEnabled)
                {
                    errors.Add("Developer telemetry must default OFF for the Module 003 gold-standard presentation slice.");
                }
            }

            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.AutoRotation
                || PlayerSettings.allowedAutorotateToPortrait
                || PlayerSettings.allowedAutorotateToPortraitUpsideDown
                || !PlayerSettings.allowedAutorotateToLandscapeLeft
                || !PlayerSettings.allowedAutorotateToLandscapeRight
                || !ReadSerializedPlayerSetting("useOSAutorotation", false))
            {
                errors.Add("Player orientation must allow both landscape directions and prohibit portrait.");
            }

            if (PlayerSettings.fullScreenMode != FullScreenMode.FullScreenWindow)
            {
                errors.Add("Player fullscreen mode must be FullScreenWindow for immersive Android gameplay.");
            }

            ValidateBrandingSettings(errors);

            if (PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android)
                    != ScriptingImplementation.IL2CPP
                || PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
            {
                errors.Add("Android must use IL2CPP with the ARM64 architecture.");
            }

            if (!(GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset))
            {
                errors.Add("The default render pipeline must be a URP asset.");
            }

            ValidateGameplayMaterials(errors);
            ValidateModuleAssets(errors);

            return errors;
        }

        private static bool ReadSerializedPlayerSetting(string propertyName, bool fallback)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets == null || assets.Length == 0)
            {
                return fallback;
            }

            var property = new SerializedObject(assets[0]).FindProperty(propertyName);
            return property == null ? fallback : property.boolValue;
        }

        [MenuItem("RYDERS BLOCK/Validate Gameplay Materials")]
        public static void ValidateGameplayMaterialsFromMenu()
        {
            var errors = new List<string>();
            ValidateGameplayMaterials(errors);
            if (errors.Count == 0)
            {
                Debug.Log("RYDERS BLOCK gameplay material validation passed.");
                return;
            }

            foreach (var error in errors)
            {
                Debug.LogError(error);
            }

            throw new InvalidOperationException(
                $"Gameplay material validation failed with {errors.Count} error(s).");
        }

        private static void ValidateGameplayMaterials(ICollection<string> errors)
        {
            var visualProfile = AssetDatabase.LoadAssetAtPath<MovementLabVisualProfile>(
                "Assets/_Game/Visuals/Resources/MovementLabVisualProfile.asset");
            if (visualProfile == null)
            {
                errors.Add("MovementLabVisualProfile asset is missing.");
            }

            ValidateMaterialAsset(
                "Assets/_Game/Visuals/Resources/RB_URP_Lit_Reference.mat",
                errors);
            ValidateMaterialAsset(
                "Assets/_Game/Visuals/Resources/RB_URP_Particle_Reference.mat",
                errors);

            var opaqueShader = VisualMaterialUtility.ResolveOpaqueShader();
            if (VisualMaterialUtility.IsInvalidGameplayShader(opaqueShader))
            {
                errors.Add("URP opaque gameplay shader is missing or unsupported.");
            }

            var particleShader = VisualMaterialUtility.ResolveParticleShader();
            if (VisualMaterialUtility.IsInvalidGameplayShader(particleShader))
            {
                errors.Add("URP particle gameplay shader is missing or unsupported.");
            }

            ValidateSceneMaterials(BootstrapScenePath, errors);
            ValidateSceneMaterials(FoundationScenePath, errors);
            ValidateSceneMaterials(MovementLabScenePath, errors);
            ValidateSceneMaterials(ModuleSelectorScenePath, errors);
            ValidateSceneMaterials(ModuleRunnerScenePath, errors);
            if (AssetDatabase.LoadAssetAtPath<ModuleVisualProfile>(
                    Phase2ModuleContentFactory.ModuleVisualProfilePath) == null)
            {
                errors.Add("ModuleVisualProfile asset is missing.");
            }

            if (AssetDatabase.LoadAssetAtPath<ModuleEnvironmentProfile>(
                    Phase2ModuleContentFactory.ModuleEnvironmentProfilePath) == null)
            {
                errors.Add("ModuleEnvironmentProfile asset is missing.");
            }

            ValidateModuleVisualProfile(errors);
            ValidateModuleEnvironmentProfile(errors);
            ValidateGlobalReadabilityProfiles(errors);
            ValidateVisualBenchmarkDocumentation(errors);
        }

        private static void ValidateModuleVisualProfile(ICollection<string> errors)
        {
            var visual = AssetDatabase.LoadAssetAtPath<ModuleVisualProfile>(
                Phase2ModuleContentFactory.ModuleVisualProfilePath);
            if (visual == null)
            {
                return;
            }

            foreach (ModuleMaterialRole role in Enum.GetValues(typeof(ModuleMaterialRole)))
            {
                var material = visual.MaterialFor(role);
                if (material == null || VisualMaterialUtility.IsInvalidGameplayShader(material.shader))
                {
                    errors.Add($"Module visual role {role} resolves to an invalid URP material.");
                }

                if (VisualMaterialUtility.IsLikelyErrorColor(visual.ColorFor(role)))
                {
                    errors.Add($"Module visual role {role} uses a magenta/error-like color.");
                }
            }

            if (visual.SurfaceInsetScale <= 0f || visual.EdgeTrimScale <= 0f)
            {
                errors.Add("ModuleVisualProfile presentation trim values must be positive.");
            }

            if (visual.HudPanelColor.a <= 0.1f)
            {
                errors.Add("ModuleVisualProfile HUD panel opacity is too low for mobile readability.");
            }
        }

        private static void ValidateModuleEnvironmentProfile(ICollection<string> errors)
        {
            var environment = AssetDatabase.LoadAssetAtPath<ModuleEnvironmentProfile>(
                Phase2ModuleContentFactory.ModuleEnvironmentProfilePath);
            if (environment == null)
            {
                return;
            }

            if (environment.FogDensity <= 0f || environment.FogDensity > 0.03f)
            {
                errors.Add("ModuleEnvironmentProfile fog density must stay in a mobile-readable range.");
            }

            if (environment.CloudDensity < 0f
                || environment.DecorationDensity < 0f
                || environment.VfxDensity < 0f)
            {
                errors.Add("ModuleEnvironmentProfile visual density values must not be negative.");
            }

            if (environment.QualityProfile != ModuleVisualQualityTier.MobileBalanced)
            {
                errors.Add("ModuleEnvironmentProfile must use MobileBalanced for the Android alpha slice.");
            }
        }

        private static void ValidateGlobalReadabilityProfiles(ICollection<string> errors)
        {
            var camera = AssetDatabase.LoadAssetAtPath<Avoidance.Gameplay.Camera.CameraProfile>(
                "Assets/_Game/Camera/Configuration/Resources/Camera_Default.asset");
            if (camera == null)
            {
                errors.Add("Global Camera_Default profile is missing.");
            }
            else
            {
                if (!Mathf.Approximately(camera.DefaultGameplayPitch, 9f))
                {
                    errors.Add("Global default gameplay camera pitch must be +9 degrees (downward in Unity).");
                }

                if (!Mathf.Approximately(camera.MinimumPitch, -75f)
                    || !Mathf.Approximately(camera.MaximumPitch, 75f))
                {
                    errors.Add("Global manual camera pitch limits must remain -75/+75 degrees.");
                }

                if (!Mathf.Approximately(camera.BaseFieldOfView, 94f))
                {
                    errors.Add("Global gameplay camera FOV must remain 94 degrees.");
                }
            }

            var grounding = AssetDatabase.LoadAssetAtPath<PlayerGroundingProfile>(
                "Assets/_Game/Visuals/Resources/PlayerGroundingProfile.asset");
            if (grounding == null)
            {
                errors.Add("Global PlayerGroundingProfile asset is missing.");
            }
            else if (grounding.Radius <= 0f || grounding.FadeOutHeight <= grounding.FullOpacityHeight)
            {
                errors.Add("PlayerGroundingProfile distance and fade settings are invalid.");
            }
        }

        private static void ValidateBrandingSettings(ICollection<string> errors)
        {
            if (PlayerSettings.productName != BrandPresentation.ProductName)
            {
                errors.Add($"Android product name must be {BrandPresentation.ProductName}.");
            }

            if (PlayerSettings.bundleVersion != BuildVersion)
            {
                errors.Add($"Bundle version must be {BuildVersion}.");
            }

            if (PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android)
                != BrandPresentation.AndroidPackageIdentifier)
            {
                errors.Add("Android package identifier must remain stable for alpha save continuity.");
            }

            ValidateTextureAsset(BrandSourceLogoPath, errors);
            ValidateTextureAsset(BrandSourceIconPath, errors);
            ValidateTextureAsset(BrandLogoPath, errors);
            ValidateTextureAsset(LegacyIconPath, errors);
            ValidateTextureAsset(AdaptiveIconForegroundPath, errors);
            ValidateTextureAsset(AdaptiveIconBackgroundPath, errors);
            ValidateLauncherIconImporter(LegacyIconPath, errors);
            ValidateLauncherIconImporter(AdaptiveIconForegroundPath, errors);
            ValidateLauncherIconImporter(AdaptiveIconBackgroundPath, errors);
            ValidateAndroidIconYaml(errors);
            ValidateAndroidFullscreenYaml(errors);
        }

        private static void ValidateLauncherIconImporter(
            string path,
            ICollection<string> errors)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
            {
                errors.Add($"Launcher icon importer is missing: {path}");
                return;
            }

            if (!importer.isReadable
                || importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                errors.Add($"{path} must be readable and uncompressed for Android icon export.");
            }
        }

        private static void ValidateAndroidIconYaml(ICollection<string> errors)
        {
            const string path = "ProjectSettings/ProjectSettings.asset";
            if (!File.Exists(path))
            {
                errors.Add("ProjectSettings.asset is missing.");
                return;
            }

            var text = File.ReadAllText(path);
            var legacyGuid = AssetDatabase.AssetPathToGUID(LegacyIconPath);
            var foregroundGuid = AssetDatabase.AssetPathToGUID(AdaptiveIconForegroundPath);
            var backgroundGuid = AssetDatabase.AssetPathToGUID(AdaptiveIconBackgroundPath);
            if (string.IsNullOrWhiteSpace(legacyGuid)
                || !text.Contains($"guid: {legacyGuid}", StringComparison.Ordinal))
            {
                errors.Add("Android legacy launcher icon is not configured in ProjectSettings.");
            }

            if (string.IsNullOrWhiteSpace(foregroundGuid)
                || string.IsNullOrWhiteSpace(backgroundGuid)
                || !text.Contains($"guid: {foregroundGuid}", StringComparison.Ordinal)
                || !text.Contains($"guid: {backgroundGuid}", StringComparison.Ordinal))
            {
                errors.Add("Android adaptive launcher icon foreground/background layers are not configured in ProjectSettings.");
            }
        }

        private static void ValidateLegacyIcons(ICollection<string> errors)
        {
            var icons = PlayerSettings.GetIcons(NamedBuildTarget.Android, IconKind.Application);
            if (icons == null || icons.Length == 0)
            {
                errors.Add("Android legacy launcher icons are not configured.");
                return;
            }

            foreach (var icon in icons)
            {
                if (icon == null)
                {
                    errors.Add("Android legacy launcher icon slot is empty.");
                    return;
                }

                if (AssetDatabase.GetAssetPath(icon) != LegacyIconPath)
                {
                    errors.Add("Android legacy launcher icons must use the Ryder's Road legacy icon asset.");
                    return;
                }
            }
        }

        private static void ValidateAdaptiveIcons(ICollection<string> errors)
        {
            var icons =
                PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, AndroidPlatformIconKind.Adaptive);
            if (icons == null || icons.Length == 0)
            {
                errors.Add("Android adaptive launcher icons are not configured.");
                return;
            }

            var getTextures = typeof(PlatformIcon).GetMethod("GetTextures", Type.EmptyTypes);
            if (getTextures == null)
            {
                return;
            }

            foreach (var icon in icons)
            {
                if (getTextures.Invoke(icon, null) is not Texture2D[] textures
                    || textures.Length < 2
                    || textures.Any(texture => texture == null))
                {
                    errors.Add("Android adaptive launcher icon slots must have foreground and background layers.");
                    return;
                }

                var paths = textures.Select(AssetDatabase.GetAssetPath).ToArray();
                if (!paths.Contains(AdaptiveIconForegroundPath)
                    || !paths.Contains(AdaptiveIconBackgroundPath))
                {
                    errors.Add("Android adaptive launcher icons must use the Ryder's Road foreground/background assets.");
                    return;
                }
            }
        }

        private static void ValidateAndroidFullscreenYaml(ICollection<string> errors)
        {
            const string path = "ProjectSettings/ProjectSettings.asset";
            if (!File.Exists(path))
            {
                errors.Add("ProjectSettings.asset is missing.");
                return;
            }

            var text = File.ReadAllText(path);
            if (!text.Contains("androidStartInFullscreen: 1", StringComparison.Ordinal)
                || !text.Contains("androidRequestedVisibleInsets: 0", StringComparison.Ordinal)
                || !text.Contains("androidRenderOutsideSafeArea: 1", StringComparison.Ordinal)
                || !text.Contains("androidSystemBarsBehavior: 2", StringComparison.Ordinal)
                || !text.Contains("androidFullscreenMode: 1", StringComparison.Ordinal))
            {
                errors.Add("Android fullscreen project settings must request no visible insets, render outside the safe area, and use transient system bars.");
            }
        }

        private static void ValidateTextureAsset(
            string path,
            ICollection<string> errors)
        {
            if (!File.Exists(path))
            {
                errors.Add($"Brand texture is missing: {path}");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) == null)
            {
                errors.Add($"Brand texture is not importable by Unity: {path}");
            }
        }

        private static void ValidateVisualBenchmarkDocumentation(ICollection<string> errors)
        {
            const string path = "VISUAL_BENCHMARKS.md";
            if (!File.Exists(path))
            {
                errors.Add("VISUAL_BENCHMARKS.md is missing.");
                return;
            }

            var text = File.ReadAllText(path);
            foreach (var benchmark in ModuleSceneController.RequiredVisualBenchmarkNames)
            {
                if (!text.Contains(benchmark, StringComparison.Ordinal))
                {
                    errors.Add($"VISUAL_BENCHMARKS.md must document {benchmark}.");
                }
            }

            if (!text.Contains("MobileBalanced", StringComparison.Ordinal)
                || !text.Contains("F6", StringComparison.Ordinal))
            {
                errors.Add("VISUAL_BENCHMARKS.md must document MobileBalanced and the F6 benchmark workflow.");
            }
        }

        private static void ValidateModuleAssets(ICollection<string> errors)
        {
            var modules = AssetDatabase.FindAssets(
                    "t:ModuleDefinition",
                    new[] { "Assets/_Game/Levels/Resources/Modules" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ModuleDefinition>)
                .Where(module => module != null)
                .ToArray();
            if (modules.Length < 4)
            {
                errors.Add("Phase 0.4.6 requires at least four ModuleDefinition assets.");
            }

            var required = new[]
            {
                "module.001.first-steps",
                "module.002.moving-parts",
                "module.003.flow-error",
                "module.004.the-spiral"
            };
            foreach (var id in required)
            {
                if (modules.All(module => module.StableModuleId != id))
                {
                    errors.Add($"Required Phase 2 module is missing: {id}");
                }
            }

            foreach (var error in ModuleDefinitionValidator.ValidateCatalog(modules))
            {
                errors.Add(error);
            }

            ValidateCampaignBiomeAssets(modules, errors);
            ValidateGoldStandardPresentation(errors);
        }

        private static void ValidateGoldStandardPresentation(ICollection<string> errors)
        {
            var armProfile = AssetDatabase.LoadAssetAtPath<FirstPersonArmProfile>(
                "Assets/_Game/Visuals/Resources/FirstPersonArmProfile.asset");
            if (armProfile == null)
            {
                errors.Add("FirstPersonArmProfile asset is missing.");
            }
            else if (armProfile.ShowFirstPersonArms)
            {
                errors.Add("First-person arms must be hidden for the Module 003 gold-standard presentation slice.");
            }
        }

        private static void ValidateCampaignBiomeAssets(
            ModuleDefinition[] modules,
            ICollection<string> errors)
        {
            var expected = new Dictionary<string, EnvironmentBiomeKind>(StringComparer.Ordinal)
            {
                ["module.001.first-steps"] = EnvironmentBiomeKind.SkyCity,
                ["module.002.moving-parts"] = EnvironmentBiomeKind.MountainSky,
                ["module.003.flow-error"] = EnvironmentBiomeKind.AncientAbyss
            };

            foreach (var pair in expected)
            {
                var module = modules.FirstOrDefault(item => item.StableModuleId == pair.Key);
                if (module == null)
                {
                    continue;
                }

                var biome = module.EnvironmentBiomeProfile;
                if (biome == null)
                {
                    errors.Add($"{pair.Key} is missing an EnvironmentBiomeProfile.");
                    continue;
                }

                if (biome.Kind != pair.Value)
                {
                    errors.Add($"{pair.Key} uses biome {biome.Kind}; expected {pair.Value}.");
                }

                var invalidAtmosphereData = (biome.Kind == EnvironmentBiomeKind.AncientAbyss || module.EnvironmentProfile.SkyboxMaterial != null)
                    ? biome.MistLayers.Count != 0
                    : biome.MistLayers.Count < 3;
                if (biome.WorldObjects.Count < 3 || invalidAtmosphereData)
                {
                    errors.Add($"{biome.BiomeId} must define useful world objects and valid atmosphere data.");
                }

                foreach (var worldObject in biome.WorldObjects)
                {
                    if (worldObject == null || !worldObject.IsValid)
                    {
                        errors.Add($"{biome.BiomeId} has an invalid world object.");
                        continue;
                    }

                    if (!worldObject.HasPlayableArchitecture && worldObject.Prefab.GetComponentsInChildren<Collider>(true).Length > 0)
                    {
                        errors.Add($"{worldObject.StableId} prefab must be visual-only and collider-free.");
                    }

                    if (worldObject.HasPlayableArchitecture)
                    {
                        var surfaces = worldObject.Prefab.GetComponentsInChildren<AuthoredSurface>(true);
                        if (surfaces.Length == 0 || surfaces.Any(surface => !surface.HasMatchingCollision))
                            errors.Add($"{worldObject.StableId} playable architecture has missing or mismatched mesh collision.");
                    }
                    if (biome.Kind == EnvironmentBiomeKind.AncientAbyss
                        && worldObject.DepthBand == BiomeDepthBand.Gameplay)
                    {
                        errors.Add($"{worldObject.StableId} must not be authored as gameplay depth.");
                    }
                }
            }
        }

        private static void ValidateMaterialAsset(
            string path,
            ICollection<string> errors)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                errors.Add($"Required gameplay material reference is missing: {path}");
                return;
            }

            if (VisualMaterialUtility.IsInvalidGameplayShader(material.shader))
            {
                var shaderName = material.shader == null ? "missing" : material.shader.name;
                errors.Add($"{path} uses invalid shader {shaderName}.");
            }
        }

        private static void ValidateSceneMaterials(
            string scenePath,
            ICollection<string> errors)
        {
            if (!File.Exists(scenePath))
            {
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            foreach (var renderer in scene.GetRootGameObjects()
                         .SelectMany(root => root.GetComponentsInChildren<Renderer>(true)))
            {
                var materials = renderer.sharedMaterials;
                if (materials == null || materials.Length == 0)
                {
                    errors.Add($"{renderer.name} in {scenePath} has no material slots.");
                    continue;
                }

                for (var index = 0; index < materials.Length; index++)
                {
                    var material = materials[index];
                    if (material == null)
                    {
                        errors.Add($"{renderer.name} in {scenePath} has a null material at slot {index}.");
                        continue;
                    }

                    if (VisualMaterialUtility.IsInvalidGameplayShader(material.shader))
                    {
                        var shaderName = material.shader == null ? "missing" : material.shader.name;
                        errors.Add(
                            $"{renderer.name} in {scenePath} uses invalid shader {shaderName}.");
                    }
                }
            }

            EditorSceneManager.CloseScene(scene, true);
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

