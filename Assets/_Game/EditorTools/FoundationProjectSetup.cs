using System.IO;
using Avoidance.Core.Configuration;
using Avoidance.Gameplay.Camera;
using Avoidance.Gameplay.Visuals;
using Avoidance.UI;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Avoidance.EditorTools
{
    public static class FoundationProjectSetup
    {
        private const string RenderPipelinePath =
            "Assets/_Game/Visuals/Settings/FoundationUniversalRenderPipeline.asset";
        private const string RendererPath =
            "Assets/_Game/Visuals/Settings/FoundationUniversalRenderer.asset";
        private const string VisualProfilePath =
            "Assets/_Game/Visuals/Resources/MovementLabVisualProfile.asset";
        private const string CameraProfilePath =
            "Assets/_Game/Camera/Configuration/Resources/Camera_Default.asset";
        private const string GroundingProfilePath =
            "Assets/_Game/Visuals/Resources/PlayerGroundingProfile.asset";
        private const string OpaqueMaterialReferencePath =
            "Assets/_Game/Visuals/Resources/RB_URP_Lit_Reference.mat";
        private const string ParticleMaterialReferencePath =
            "Assets/_Game/Visuals/Resources/RB_URP_Particle_Reference.mat";
        private const string BrandLogoPath =
            "Assets/Branding/Resources/Branding/RydersRoad_Logo_UI.png";
        private const string LegacyIconPath =
            "Assets/Branding/Android/RydersRoad_Icon_Legacy.png";
        private const string AdaptiveIconForegroundPath =
            "Assets/Branding/Android/RydersRoad_Icon_AdaptiveForeground.png";
        private const string AdaptiveIconBackgroundPath =
            "Assets/Branding/Android/RydersRoad_Icon_AdaptiveBackground.png";
        private const string GameVersion = "0.5.0";
        private const string BuildVersion = FoundationProjectValidator.BuildVersion;

        [InitializeOnLoadMethod]
        private static void ScheduleSetup()
        {
            EditorApplication.delayCall += Apply;
        }

        [MenuItem("RYDERS BLOCK/Apply Project Settings")]
        public static void Apply()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            SetSerializedPlayerSetting("useOSAutorotation", true);
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.Android.requestedVisibleInsets = AndroidWindowInsetsType.None;
            PlayerSettings.Android.systemBarsBehavior =
                AndroidSystemBarsBehavior.ShowTransientBarsBySwipe;
            PlayerSettings.Android.renderOutsideSafeArea = true;
            ConfigureStartupPresentation();
            PlayerSettings.productName = BrandPresentation.ProductName;
            PlayerSettings.companyName = "Ryders Block Studio";
            PlayerSettings.bundleVersion = BuildVersion;
            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.Android,
                BrandPresentation.AndroidPackageIdentifier);
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(
                NamedBuildTarget.Android,
                ScriptingImplementation.IL2CPP);

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
                RenderPipelinePath);
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (renderer == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(RendererPath));
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, RendererPath);
            }

            if (pipeline == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(RenderPipelinePath));
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, RenderPipelinePath);
            }

            var serializedPipeline = new SerializedObject(pipeline);
            var rendererList = serializedPipeline.FindProperty("m_RendererDataList");
            if (rendererList != null
                && (rendererList.arraySize != 1
                    || rendererList.GetArrayElementAtIndex(0).objectReferenceValue != renderer))
            {
                rendererList.arraySize = 1;
                rendererList.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
                var defaultRenderer = serializedPipeline.FindProperty("m_DefaultRendererIndex");
                if (defaultRenderer != null)
                {
                    defaultRenderer.intValue = 0;
                }

                serializedPipeline.ApplyModifiedPropertiesWithoutUndo();
            }

            if (GraphicsSettings.defaultRenderPipeline != pipeline)
            {
                GraphicsSettings.defaultRenderPipeline = pipeline;
            }

            if (QualitySettings.renderPipeline != pipeline)
            {
                QualitySettings.renderPipeline = pipeline;
            }

            if (AssetDatabase.LoadAssetAtPath<MovementLabVisualProfile>(
                    VisualProfilePath) == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(VisualProfilePath));
                var visualProfile =
                    MovementLabVisualProfile.CreateRuntimeDefault();
                visualProfile.name = MovementLabVisualProfile.ResourceName;
                visualProfile.hideFlags = HideFlags.None;
                AssetDatabase.CreateAsset(visualProfile, VisualProfilePath);
            }

            EnsureMaterialReference(
                OpaqueMaterialReferencePath,
                VisualMaterialUtility.ResolveOpaqueShader(),
                new Color(0.78f, 0.86f, 0.96f));
            EnsureMaterialReference(
                ParticleMaterialReferencePath,
                VisualMaterialUtility.ResolveParticleShader(),
                new Color(0.1f, 0.92f, 1f));
            ConfigureSharedPresentationProfiles();

            ConfigureBrandAssets();
            ConfigureAndroidLauncherIcons();
            Phase2ModuleContentFactory.EnsurePhase2Content();
            Phase073WorldBiomeImporter.EnsureImportedAssetsAndBiomes();
            UpdateFoundationConfiguration();
            EnsureBuildScenes();

            AssetDatabase.SaveAssets();
        }

        private static void ConfigureSharedPresentationProfiles()
        {
            var cameraProfile = AssetDatabase.LoadAssetAtPath<CameraProfile>(CameraProfilePath);
            if (cameraProfile == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CameraProfilePath));
                cameraProfile = CameraProfile.CreateRuntimeDefault();
                cameraProfile.name = "Camera_Default";
                cameraProfile.hideFlags = HideFlags.None;
                AssetDatabase.CreateAsset(cameraProfile, CameraProfilePath);
            }

            var cameraSerialized = new SerializedObject(cameraProfile);
            cameraSerialized.FindProperty("_minimumPitch").floatValue = -75f;
            cameraSerialized.FindProperty("_maximumPitch").floatValue = 75f;
            cameraSerialized.FindProperty("_defaultGameplayPitch").floatValue = 9f;
            cameraSerialized.FindProperty("_baseFieldOfView").floatValue = 94f;
            cameraSerialized.FindProperty("_touchAutoPitchEnabled").boolValue = false;
            cameraSerialized.FindProperty("_landingAwarenessEnabled").boolValue = false;
            cameraSerialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(cameraProfile);

            var groundingProfile = AssetDatabase.LoadAssetAtPath<PlayerGroundingProfile>(
                GroundingProfilePath);
            if (groundingProfile == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(GroundingProfilePath));
                groundingProfile = PlayerGroundingProfile.CreateRuntimeDefault();
                groundingProfile.name = PlayerGroundingProfile.ResourceName;
                groundingProfile.hideFlags = HideFlags.None;
                AssetDatabase.CreateAsset(groundingProfile, GroundingProfilePath);
            }

            EditorUtility.SetDirty(groundingProfile);
        }

        private static void ConfigureStartupPresentation()
        {
            const string source = "Assets/_Game/UI/Resources/Loading/RydersRoad_LoadingPoster.jpg";
            const string target = "Assets/Branding/Android/RydersRoad_Startup.jpg";
            if (!File.Exists(target))
            {
                File.Copy(source, target);
                AssetDatabase.ImportAsset(target);
            }
            var importer = AssetImporter.GetAtPath(target) as TextureImporter;
            if (importer != null && (importer.textureType != TextureImporterType.Sprite
                || !importer.isReadable || importer.textureCompression != TextureImporterCompression.Uncompressed))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            var poster = AssetDatabase.LoadAssetAtPath<Sprite>(target);
            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            // A blank logo entry holds the supplied poster without duplicating its embedded brand.
            PlayerSettings.SplashScreen.logos = new[] { PlayerSettings.SplashScreenLogo.Create(2f, null) };
            PlayerSettings.SplashScreen.background = poster;
            PlayerSettings.SplashScreen.backgroundPortrait = poster;
            PlayerSettings.SplashScreen.blurBackgroundImage = false;
            PlayerSettings.SplashScreen.overlayOpacity = 0f;
            PlayerSettings.SplashScreen.animationMode = PlayerSettings.SplashScreen.AnimationMode.Static;
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            settings.FindProperty("androidSplashScreen").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Texture2D>(target);
            settings.FindProperty("AndroidSplashScreenScale").intValue = (int)AndroidSplashScreenScale.ScaleToFill;
            settings.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedPlayerSetting(string propertyName, bool value)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets == null || assets.Length == 0)
            {
                return;
            }

            var serializedSettings = new SerializedObject(assets[0]);
            var property = serializedSettings.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            property.boolValue = value;
            serializedSettings.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void UpdateFoundationConfiguration()
        {
            var configuration = AssetDatabase.LoadAssetAtPath<GameConfiguration>(
                "Assets/_Game/Core/Configuration/Resources/FoundationGameConfiguration.asset");
            if (configuration == null)
            {
                return;
            }

            var serialized = new SerializedObject(configuration);
            serialized.FindProperty("_gameTitle").stringValue = BrandPresentation.PlayerFacingTitle;
            serialized.FindProperty("_gameVersion").stringValue = GameVersion;
            serialized.FindProperty("_buildVersion").stringValue = BuildVersion;
            serialized.FindProperty("_initialScene").stringValue = "ModuleSelector";
            var developerTelemetry = serialized.FindProperty("_developerTelemetryEnabled");
            if (developerTelemetry != null)
            {
                developerTelemetry.boolValue = false;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(configuration);
        }

        private static void EnsureBuildScenes()
        {
            var scenePaths = new[]
            {
                FoundationProjectValidator.BootstrapScenePath,
                Phase2ModuleContentFactory.ModuleSelectorScenePath,
                FoundationProjectValidator.MovementLabScenePath,
                Phase2ModuleContentFactory.ModuleRunnerScenePath,
                FoundationProjectValidator.FoundationScenePath
            };
            EditorBuildSettings.scenes = System.Array.ConvertAll(
                scenePaths,
                path => new EditorBuildSettingsScene(path, enabled: true));
        }

        private static void ConfigureBrandAssets()
        {
            ConfigureTextureImporter(BrandLogoPath, maxTextureSize: 2048, alpha: true);
            ConfigureTextureImporter(
                LegacyIconPath,
                maxTextureSize: 1024,
                alpha: false,
                readable: true,
                compression: TextureImporterCompression.Uncompressed);
            ConfigureTextureImporter(
                AdaptiveIconForegroundPath,
                maxTextureSize: 1024,
                alpha: true,
                readable: true,
                compression: TextureImporterCompression.Uncompressed);
            ConfigureTextureImporter(
                AdaptiveIconBackgroundPath,
                maxTextureSize: 1024,
                alpha: false,
                readable: true,
                compression: TextureImporterCompression.Uncompressed);
        }

        private static void ConfigureTextureImporter(
            string path,
            int maxTextureSize,
            bool alpha,
            bool readable = false,
            TextureImporterCompression compression = TextureImporterCompression.CompressedHQ)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"Brand texture is missing: {path}");
                return;
            }

            if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                importer = AssetImporter.GetAtPath(path) as TextureImporter;
            }

            if (importer == null)
            {
                Debug.LogError($"Brand texture importer is unavailable: {path}");
                return;
            }

            importer.textureType = TextureImporterType.Default;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = alpha;
            importer.mipmapEnabled = false;
            importer.isReadable = readable;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = maxTextureSize;
            importer.textureCompression = compression;
            var defaultSettings = importer.GetDefaultPlatformTextureSettings();
            defaultSettings.maxTextureSize = maxTextureSize;
            defaultSettings.textureCompression = compression;
            defaultSettings.format = TextureImporterFormat.Automatic;
            importer.SetPlatformTextureSettings(defaultSettings);

            var androidSettings = importer.GetPlatformTextureSettings("Android");
            androidSettings.overridden = readable;
            androidSettings.maxTextureSize = maxTextureSize;
            androidSettings.textureCompression = compression;
            androidSettings.format = TextureImporterFormat.Automatic;
            importer.SetPlatformTextureSettings(androidSettings);
            importer.SaveAndReimport();
        }

        private static void ConfigureAndroidLauncherIcons()
        {
            var legacy = AssetDatabase.LoadAssetAtPath<Texture2D>(LegacyIconPath);
            var adaptiveForeground =
                AssetDatabase.LoadAssetAtPath<Texture2D>(AdaptiveIconForegroundPath);
            var adaptiveBackground =
                AssetDatabase.LoadAssetAtPath<Texture2D>(AdaptiveIconBackgroundPath);
            if (legacy == null || adaptiveForeground == null || adaptiveBackground == null)
            {
                Debug.LogError("Android launcher icon textures must exist before applying player settings.");
                return;
            }

            var legacySizes =
                PlayerSettings.GetIconSizes(NamedBuildTarget.Android, IconKind.Application);
            var legacyIcons = new Texture2D[legacySizes.Length];
            for (var index = 0; index < legacyIcons.Length; index++)
            {
                legacyIcons[index] = legacy;
            }

            PlayerSettings.SetIcons(
                NamedBuildTarget.Android,
                legacyIcons,
                IconKind.Application);

            var adaptiveIcons =
                PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, AndroidPlatformIconKind.Adaptive);
            for (var index = 0; index < adaptiveIcons.Length; index++)
            {
                adaptiveIcons[index].SetTextures(adaptiveBackground, adaptiveForeground);
            }

            PlayerSettings.SetPlatformIcons(
                NamedBuildTarget.Android,
                AndroidPlatformIconKind.Adaptive,
                adaptiveIcons);
        }

        private static void EnsureMaterialReference(
            string path,
            Shader shader,
            Color color)
        {
            if (VisualMaterialUtility.IsInvalidGameplayShader(shader))
            {
                Debug.LogError($"Cannot create gameplay material reference for invalid shader at {path}.");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            VisualMaterialUtility.ApplyColor(material, color);
            EditorUtility.SetDirty(material);
        }
    }
}

