using System.Collections.Generic;
using System.IO;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Ranking;
using Avoidance.Gameplay.Visuals;
using Avoidance.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Avoidance.EditorTools
{
    public static class Phase2ModuleContentFactory
    {
        public const string ModuleVisualProfilePath =
            "Assets/_Game/Visuals/Resources/ModuleVisualProfile.asset";
        public const string ModuleEnvironmentProfilePath =
            "Assets/_Game/Visuals/Resources/ModuleEnvironmentProfile.asset";
        public const string ModuleVisualPrefabLibraryPath =
            "Assets/_Game/Visuals/Resources/ModuleVisualPrefabLibrary.asset";
        public const string ModuleResourcesDirectory =
            "Assets/_Game/Levels/Resources/Modules";
        public const string ModuleSelectorScenePath =
            "Assets/_Game/Levels/Scenes/ModuleSelector.unity";
        public const string ModuleRunnerScenePath =
            "Assets/_Game/Levels/Scenes/ModuleRunner.unity";

        public const string ArtScaleCollisionLabScenePath =
            "Assets/_Game/Levels/Scenes/ArtScaleCollisionLab.unity";

        public const string PlatformTruthLabScenePath =
            "Assets/_Game/Levels/Scenes/PlatformTruthLab.unity";

        public const string GameplayRoleGalleryScenePath =
            "Assets/_Game/Levels/Scenes/GameplayRoleGallery.unity";

        public const string HandPoseLabScenePath =
            "Assets/_Game/Levels/Scenes/HandPoseLab.unity";

        public const string SeamlessSkyMaterialPath =
            "Assets/_Game/Art/Resources/Materials/MAT_RR_Skybox_Seamless.mat";

        public static void EnsurePhase2Content()
        {
            Directory.CreateDirectory("Assets/_Game/Visuals/Resources");
            Directory.CreateDirectory(ModuleResourcesDirectory);
            var visual = EnsureAsset(
                ModuleVisualProfilePath,
                ModuleVisualProfile.CreateRuntimeDefault);
            visual.name = ModuleVisualProfile.ResourceName;
            visual.hideFlags = HideFlags.None;
            visual.ApplyVisualIdentityDefaults();
            EditorUtility.SetDirty(visual);

            var environment = EnsureAsset(
                ModuleEnvironmentProfilePath,
                ModuleEnvironmentProfile.CreateRuntimeDefault);
            environment.name = ModuleEnvironmentProfile.ResourceName;
            environment.hideFlags = HideFlags.None;
            environment.ApplyVisualIdentityDefaults();
            EditorUtility.SetDirty(environment);

            var prefabLibrary = EnsureAsset(
                ModuleVisualPrefabLibraryPath,
                ModuleVisualPrefabLibrary.CreateRuntimeDefault);
            prefabLibrary.name = ModuleVisualPrefabLibrary.ResourceName;
            prefabLibrary.hideFlags = HideFlags.None;
            EditorUtility.SetDirty(prefabLibrary);
            EnsureSeamlessSkyMaterial(environment);

            EnsureModule(
                "Assets/_Game/Levels/Resources/Modules/Module_001_FirstSteps.asset",
                module => ConfigureFirstSteps(module, visual, environment));
            EnsureModule(
                "Assets/_Game/Levels/Resources/Modules/Module_002_MovingParts.asset",
                module => ConfigureMovingParts(module, visual, environment));
            EnsureModule(
                "Assets/_Game/Levels/Resources/Modules/Module_003_FlowError.asset",
                module => ConfigureFlowError(module, visual, environment));
            EnsureModule(
                "Assets/_Game/Levels/Resources/Modules/Module_004_TheSpiral.asset",
                module => ConfigureTheSpiral056(module, visual, environment));
            EnsureModule(
                "Assets/_Game/Levels/Resources/Modules/Module_PlatformTruthLab.asset",
                module => ConfigurePlatformTruthLab(module, visual, environment));
            EnsureModule(
                "Assets/_Game/Levels/Resources/Modules/Module_GameplayRoleGallery.asset",
                module => ConfigureGameplayRoleGallery(module, visual, environment));

            EnsureScene(
                ModuleSelectorScenePath,
                "Development Module Selector",
                typeof(DevelopmentModuleSelector));
            EnsureScene(
                ModuleRunnerScenePath,
                "Module Runner",
                typeof(ModuleSceneController));
            EnsureScene(
                ArtScaleCollisionLabScenePath,
                "Art Scale Collision Lab",
                typeof(ModuleSceneController));
            EnsureScene(
                PlatformTruthLabScenePath,
                "Platform Truth Lab",
                typeof(ModuleSceneController));
            EnsureScene(
                GameplayRoleGalleryScenePath,
                "Gameplay Role Gallery",
                typeof(ModuleSceneController));
            EnsureScene(
                HandPoseLabScenePath,
                "Hand Pose Lab",
                typeof(HandPoseLabController));
            AssetDatabase.SaveAssets();
        }

        private static void EnsureSeamlessSkyMaterial(ModuleEnvironmentProfile environment)
        {
            var shader = Shader.Find("Skybox/Procedural");
            if (shader == null)
            {
                Debug.LogError("Skybox/Procedural is unavailable; seamless sky material was not generated.");
                return;
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(SeamlessSkyMaterialPath);
            if (material == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SeamlessSkyMaterialPath));
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, SeamlessSkyMaterialPath);
            }

            material.name = "MAT_RR_Skybox_Seamless";
            material.shader = shader;
            material.SetColor("_SkyTint", environment.SkyMid);
            material.SetColor("_GroundColor", environment.SkyHorizon);
            material.SetFloat("_AtmosphereThickness", 0.68f);
            material.SetFloat("_Exposure", 1.08f);
            material.SetFloat("_SunSize", 0.035f);
            material.SetFloat("_SunSizeConvergence", 6f);
            EditorUtility.SetDirty(material);
        }

        private static T EnsureAsset<T>(string path, System.Func<T> create)
            where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            asset = create();
            asset.hideFlags = HideFlags.None;
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureModule(
            string path,
            System.Action<ModuleDefinition> configure)
        {
            var module = AssetDatabase.LoadAssetAtPath<ModuleDefinition>(path);
            if (module != null)
            {
                configure(module);
                EditorUtility.SetDirty(module);
                return;
            }

            module = ScriptableObject.CreateInstance<ModuleDefinition>();
            configure(module);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            AssetDatabase.CreateAsset(module, path);
        }

        private static void EnsureScene(
            string path,
            string rootName,
            System.Type controllerType)
        {
            if (File.Exists(path))
            {
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject(rootName, controllerType);
            root.transform.position = Vector3.zero;
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void ConfigureFirstSteps(
            ModuleDefinition module,
            ModuleVisualProfile visual,
            ModuleEnvironmentProfile environment)
        {
            // Version 2 is authored production content; startup setup must not restore the v1 blockout.
            if (module.ContentVersion >= 2 && module.StableModuleId == "module.001.first-steps") return;
            module.name = "Module_001_FirstSteps";
            module.Configure(
                "module.001.first-steps",
                "Campaign 01 - First Steps",
                "module_001_first_steps",
                "project.ryders-block",
                "world.prototype-sky",
                1,
                ModuleSelectionState.ModuleRunnerSceneName,
                ModuleDifficulty.Intro,
                new ModulePose(new Vector3(0f, 0.35f, -0.8f), Vector3.zero),
                new ModulePatchBlockDefinition(
                    "patch.module-001",
                    new Vector3(7f, 1.45f, 46.5f),
                    new Vector3(2.2f, 2.2f, 2.2f)),
                new[]
                {
                    new ModuleRestorePointDefinition(
                        "restore.module-001.midpoint",
                        0,
                        new Vector3(5.8f, 0.45f, 25f),
                        new Vector3(0f, 0.35f, 0f))
                },
                26f,
                55f,
                new[]
                {
                    ModuleMechanic.StandardBlock,
                    ModuleMechanic.PrecisionBlock,
                    ModuleMechanic.RestorePoint,
                    ModuleMechanic.PatchBlock
                },
                null,
                environment,
                visual,
                "The basic movement function has fragmented into readable stepping blocks.",
                "Campaign intro route: 1.5m standard parkour blocks, readable jump gaps, one earned Restore Point, and a clean Patch summit.",
                new[]
                {
                    Block("m01.start", 0f, 0f, 0f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m01.step.01", 0f, 0f, 4.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m01.step.02", 0f, 0f, 8.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m01.step.03", 0f, 0f, 12.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m01.turn.01", 1.8f, 0.25f, 16.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m01.turn.02", 4.0f, 0.45f, 20.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m01.restore.approach", 5.8f, 0.45f, 25f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m01.flow.01", 5.8f, 0.65f, 29.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Precision),
                    Block("m01.flow.02", 7.4f, 0.85f, 33.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Precision),
                    Block("m01.flow.03", 5.6f, 1.05f, 37.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m01.final-runup", 7f, 1.15f, 41.5f, 3f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m01.patch.base", 7f, 1.15f, 46.5f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal)
                },
                null,
                null,
                null,
                null,
                new[]
                {
                    Block("m01.arch.restore", 5.8f, 0.45f, 23.5f, 5f, 5f, 5f, ModuleMaterialRole.ShortcutCue),
                    Block("m01.island.left", -12f, -2f, 16f, 4.5f, 4.5f, 4.5f, ModuleMaterialRole.DecorationStone),
                    Block("m01.tree.left", -12f, 0.2f, 16f, 4f, 4f, 4f, ModuleMaterialRole.Vegetation),
                    Block("m01.pillar.right", 14f, 2f, 28f, 8f, 8f, 8f, ModuleMaterialRole.TowerCore),
                    Block("m01.island.right", 16f, -2.5f, 36f, 4.5f, 4.5f, 4.5f, ModuleMaterialRole.DecorationStone),
                    Block("m01.tree.right", 16f, -0.3f, 36f, 4f, 4f, 4f, ModuleMaterialRole.Vegetation)
                },
                new[]
                {
                    new ModuleCameraHintDefinition(
                        "camera.m01.final",
                        new Vector3(6f, 2.6f, 41f),
                        new Vector3(5f, 4f, 8f),
                        0f,
                        -6f,
                        8f)
                });
            module.ConfigureRankThresholds(Thresholds(42f, 32f, 26f, "Campaign 01 provisional thresholds. Bronze is any valid completion."));
            module.ConfigureSurfSurfaces(null);
            module.ConfigureCompletionRewards(BronzeRewards(
                "module-001",
                100,
                "loot.package.module-001",
                "skin.ryder.default"));
        }

        private static void ConfigureMovingParts(
            ModuleDefinition module,
            ModuleVisualProfile visual,
            ModuleEnvironmentProfile environment)
        {
            if (module.ContentVersion >= 2 && module.StableModuleId == "module.002.moving-parts") return;
            module.name = "Module_002_MovingParts";
            module.Configure(
                "module.002.moving-parts",
                "Campaign 02 - Moving Parts",
                "module_002_moving_parts",
                "project.ryders-block",
                "world.prototype-sky",
                1,
                ModuleSelectionState.ModuleRunnerSceneName,
                ModuleDifficulty.Easy,
                new ModulePose(new Vector3(0f, 0.35f, -0.8f), Vector3.zero),
                new ModulePatchBlockDefinition(
                    "patch.module-002",
                    new Vector3(4f, 1.4f, 61.5f),
                    new Vector3(2.2f, 2.2f, 2.2f)),
                new[]
                {
                    new ModuleRestorePointDefinition(
                        "restore.module-002.branch",
                        0,
                        new Vector3(4f, 0.45f, 26f),
                        new Vector3(0f, 0.35f, 0f))
                },
                36f,
                80f,
                new[]
                {
                    ModuleMechanic.StandardBlock,
                    ModuleMechanic.PrecisionBlock,
                    ModuleMechanic.MovingBlock,
                    ModuleMechanic.RestorePoint,
                    ModuleMechanic.Shortcut,
                    ModuleMechanic.PatchBlock
                },
                new[]
                {
                    new ModuleShortcutDefinition(
                        "shortcut.module-002.risky-inner",
                        "Risky inner route",
                        new Vector3(7f, 0.8f, 31f),
                        new Vector3(4f, 1f, 47f),
                        7f,
                        new[] { ModuleMechanic.PrecisionBlock, ModuleMechanic.MovingBlock })
                },
                environment,
                visual,
                "A positioning bug makes some coordinates refuse to stay still.",
                "Campaign route: safe branch is wider and longer; risky branch is visible and shorter but never mandatory.",
                new[]
                {
                    Block("m02.start", 0f, 0f, 0f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m02.rhythm.01", 0f, 0f, 4.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m02.rhythm.02", 0f, 0f, 8.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m02.after-moving.01", 3f, 0f, 17.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m02.after-moving.02", 4f, 0.2f, 21.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m02.restore.platform", 4f, 0.45f, 26f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m02.safe.01", 1f, 0.45f, 30.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m02.safe.02", -0.5f, 0.6f, 34.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m02.safe.03", 0.5f, 0.75f, 38.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m02.safe.04", 3f, 0.85f, 42.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m02.risky.01", 7f, 0.55f, 31f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Precision),
                    Block("m02.risky.02", 8f, 0.85f, 37f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Precision),
                    Block("m02.rejoin", 4f, 0.95f, 47f, 3f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m02.final-runup", 4f, 1.1f, 57f, 3f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m02.patch.base", 4f, 1.1f, 61.5f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal)
                },
                new[]
                {
                    Moving(
                        "moving.m02.first",
                        new Vector3(-2.5f, 0f, 13f),
                        new Vector3(3f, 0.6f, 1.5f),
                        new[] { new Vector3(-2.5f, 0f, 13f), new Vector3(2.5f, 0f, 13f) },
                        1.8f,
                        0.25f),
                    Moving(
                        "moving.m02.final",
                        new Vector3(1.5f, 1.05f, 52f),
                        new Vector3(3f, 0.6f, 1.5f),
                        new[] { new Vector3(1.5f, 1.05f, 52f), new Vector3(6.5f, 1.05f, 52f) },
                        2.2f,
                        0.15f)
                },
                null,
                null,
                null,
                new[]
                {
                    Block("m02.island.a", -14f, -2.2f, 25f, 4.5f, 4.5f, 4.5f, ModuleMaterialRole.DecorationStone),
                    Block("m02.tree.a", -14f, 0.2f, 25f, 4f, 4f, 4f, ModuleMaterialRole.Vegetation),
                    Block("m02.tower.a", 15f, 4f, 40f, 6.5f, 6.5f, 6.5f, ModuleMaterialRole.TowerCore),
                    Block("m02.pillar.broken", -12f, 4f, 55f, 8f, 8f, 8f, ModuleMaterialRole.TowerCore)
                },
                new[]
                {
                    new ModuleCameraHintDefinition(
                        "camera.m02.branch",
                        new Vector3(4f, 2.5f, 28f),
                        new Vector3(10f, 5f, 10f),
                        14f,
                        -5f,
                        10f)
                });
            module.ConfigureRankThresholds(Thresholds(62f, 48f, 38f, "Campaign 02 provisional thresholds. Requires physical timing calibration."));
            module.ConfigureSurfSurfaces(null);
            module.ConfigureCompletionRewards(BronzeRewards(
                "module-002",
                150,
                "loot.package.module-002",
                "skin.ryder.signal-blue"));
        }

        private static void ConfigureFlowError(
            ModuleDefinition module,
            ModuleVisualProfile visual,
            ModuleEnvironmentProfile environment)
        {
            if (module.ContentVersion >= 7 && module.StableModuleId == "module.003.flow-error") return;
            module.name = "Module_003_FlowError";
            module.Configure(
                "module.003.flow-error",
                "Campaign 03 - Flow Error",
                "module_003_flow_error",
                "project.ryders-block",
                "world.prototype-sky",
                1,
                ModuleSelectionState.ModuleRunnerSceneName,
                ModuleDifficulty.Easy,
                new ModulePose(new Vector3(0f, 0.35f, -6f), Vector3.zero),
                new ModulePatchBlockDefinition(
                    "patch.module-003",
                    new Vector3(7f, 7.8f, 82f),
                    new Vector3(2.2f, 2.2f, 2.2f)),
                new[]
                {
                    new ModuleRestorePointDefinition(
                        "restore.module-003.water-exit",
                        0,
                        new Vector3(-4f, 0.35f, 20f),
                        new Vector3(0f, 0.35f, 0f)),
                    new ModuleRestorePointDefinition(
                        "restore.module-003.upper-spiral",
                        1,
                        new Vector3(-7f, 4.8f, 58f),
                        new Vector3(0f, 0.35f, 0f))
                },
                44f,
                115f,
                new[]
                {
                    ModuleMechanic.StandardBlock,
                    ModuleMechanic.PrecisionBlock,
                    ModuleMechanic.MovingBlock,
                    ModuleMechanic.RestorePoint,
                    ModuleMechanic.JumpBoost,
                    ModuleMechanic.FlowingWater,
                    ModuleMechanic.CrumblingBlock,
                    ModuleMechanic.Shortcut,
                    ModuleMechanic.PatchBlock,
                    ModuleMechanic.CameraGuide
                },
                new[]
                {
                    new ModuleShortcutDefinition(
                        "shortcut.module-003.water-boost",
                        "Water boost shortcut",
                        new Vector3(-7f, 4.8f, 62f),
                        new Vector3(1f, 6.5f, 72f),
                        9f,
                        new[] { ModuleMechanic.FlowingWater, ModuleMechanic.JumpBoost, ModuleMechanic.PrecisionBlock })
                },
                environment,
                visual,
                "Movement force, water flow, and unstable code are tangled around a vertical structure.",
                "Campaign ascent proof: route wraps upward so the player can see lower sections below and Patch Block above.",
                new[]
                {
                    Block("m03.start", 0f, 0f, -6f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m03.opening.01", -3.5f, 0f, -1.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m03.opening.02", -6f, 0.15f, 3f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m03.opening.03", -5.5f, 0.3f, 8f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m03.water-exit", -4f, 0.35f, 20f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m03.boost-runup", 1f, 0.65f, 26f, 3f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m03.boost-platform", 1f, 0.95f, 29f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m03.boost-landing", 3f, 2.2f, 38f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m03.moving-exit", 3f, 2.6f, 49f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m03.crumble-exit", -6f, 4.4f, 54f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m03.safe-upper.01", -7f, 4.8f, 58f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m03.safe-upper.02", -4.5f, 5.3f, 63f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m03.final-landing", 1f, 6.8f, 72f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m03.final-runup", 5f, 7.4f, 77f, 3f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m03.patch.base", 7f, 7.8f, 82f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal)
                },
                new[]
                {
                    Moving(
                        "moving.m03.mid-spiral",
                        new Vector3(6f, 2.3f, 42f),
                        new Vector3(3f, 0.6f, 1.5f),
                        new[] { new Vector3(6f, 2.3f, 42f), new Vector3(6f, 2.3f, 46f) },
                        2.1f,
                        0.18f)
                },
                new[]
                {
                    new ModuleBoostBlockDefinition(
                        "boost.m03.first",
                        new Vector3(1f, 0.95f, 29f),
                        new Vector3(1.8f, 0.6f, 1.8f),
                        new Vector3(0.15f, 0.1f, 1f),
                        11.5f,
                        7.4f,
                        0.35f)
                },
                new[]
                {
                    new ModuleWaterVolumeDefinition(
                        "water.m03.lower-flow",
                        new Vector3(-5f, 0.52f, 14f),
                        new Vector3(2.5f, 1.4f, 7f),
                        Vector3.forward,
                        15f,
                        4.6f,
                        0f,
                        1.4f)
                },
                new[]
                {
                    new ModuleCrumblingBlockDefinition(
                        "crumble.m03.01",
                        new Vector3(0f, 3.2f, 51f),
                        new Vector3(1.5f, 0.6f, 1.5f),
                        0.03f,
                        1f,
                        4f,
                        0.03f),
                    new ModuleCrumblingBlockDefinition(
                        "crumble.m03.02",
                        new Vector3(-3f, 3.8f, 53f),
                        new Vector3(1.5f, 0.6f, 1.5f),
                        0.03f,
                        1f,
                        4f,
                        0.03f)
                },
                new[]
                {
                    Block("m03.landmark.water-buttress", -5f, -1.2f, 14f, 3f, 2f, 7f, ModuleMaterialRole.DecorationStone),
                    Block("m03.landmark.boost-flank-island", 1f, -0.8f, 29f, 3.5f, 2f, 3.5f, ModuleMaterialRole.DecorationStone),
                    Block("m03.landmark.central-spire", -1f, -2.5f, 42f, 4.5f, 5f, 4.5f, ModuleMaterialRole.TowerCore),
                    Block("m03.landmark.upper-spire-base", -6f, 1.8f, 54f, 2f, 2f, 2f, ModuleMaterialRole.TowerCore),
                    Block("m03.landmark.ancient-tree-island", -18f, -4f, 60f, 6f, 6f, 6f, ModuleMaterialRole.Vegetation),
                    Block("m03.landmark.summit-support-ruin", 7f, 4.8f, 82f, 3.5f, 3f, 3.5f, ModuleMaterialRole.TowerCore)
                },
                new[]
                {
                    new ModuleCameraHintDefinition(
                        "camera.m03.first-boost",
                        new Vector3(1f, 2.5f, 29f),
                        new Vector3(7f, 5f, 9f),
                        4f,
                        -10f,
                        12f),
                    new ModuleCameraHintDefinition(
                        "camera.m03.final-ascent",
                        new Vector3(0f, 7.5f, 75f),
                        new Vector3(9f, 7f, 12f),
                        12f,
                        -12f,
                        14f)
                });
            module.ConfigurePlayability(
                "Module03StartAnchor",
                "m03.start",
                -12f,
                disableAutomaticDistantFragments: true);
            module.ConfigureRankThresholds(Thresholds(90f, 68f, 52f, "Campaign 03 provisional thresholds. Vertical climb pacing is uncalibrated."));
            module.ConfigureSurfSurfaces(null);
            module.ConfigureCompletionRewards(BronzeRewards(
                "module-003",
                200,
                "loot.package.module-003",
                "skin.ryder.circuit-mint"));
        }

        private static void ConfigureTheSpiral056(
            ModuleDefinition module,
            ModuleVisualProfile visual,
            ModuleEnvironmentProfile environment)
        {
            var authored = BuildSpiral056Route();
            module.name = "Module_004_TheSpiral";
            module.Configure(
                "module.004.the-spiral",
                "THE SPIRAL",
                "module_004_the_spiral",
                "project.ryders-block",
                "world.prototype-sky",
                2,
                ModuleSelectionState.ModuleRunnerSceneName,
                ModuleDifficulty.Easy,
                authored.StartPose,
                new ModulePatchBlockDefinition(
                    "patch.module-004",
                    authored.PatchPosition,
                    new Vector3(2.2f, 2.2f, 2.2f)),
                authored.RestorePoints,
                150f,
                300f,
                new[]
                {
                    ModuleMechanic.StandardBlock,
                    ModuleMechanic.PrecisionBlock,
                    ModuleMechanic.MovingBlock,
                    ModuleMechanic.RestorePoint,
                    ModuleMechanic.JumpBoost,
                    ModuleMechanic.CrumblingBlock,
                    ModuleMechanic.Shortcut,
                    ModuleMechanic.PatchBlock,
                    ModuleMechanic.CameraGuide
                },
                CreateSpiral056Shortcuts(authored),
                environment,
                visual,
                "A deliberate ascending route wraps a broken sky spire and converges on its Patch summit.",
                "0.6.5 recovery: measured Bronze spacing, five Restore decks, mandatory one-second crumble pressure chains, a required moving crossing, targeted boost gap closer, separated central core masses, truthful non-landable water visuals, and simplified abyss depth without slab or filler geometry. GoldSrc movement is unchanged.",
                authored.Blocks,
                CreateSpiral056MovingBlocks(authored),
                CreateSpiral056BoostBlocks(authored),
                null,
                CreateSpiral056CrumblingBlocks(authored),
                CreateSpiral056Landmarks(authored),
                CreateSpiral056CameraHints(authored));
            module.ConfigurePlayability(
                "SpiralStartAnchor",
                "m04.start",
                -7.5f,
                disableAutomaticDistantFragments: true);
            module.ConfigureSurfSurfaces(null);
            module.ConfigureRankThresholds(Thresholds(
                255f,
                190f,
                150f,
                "THE SPIRAL 0.6.3 hand/collision/abyss polish pass. Bronze remains any valid completion; higher ranks require route mastery and physical-device calibration."));
            module.ConfigureCompletionRewards(BronzeRewards(
                "module-004",
                250,
                "loot.package.module-004",
                "skin.ryder.patch-glow"));
        }

        private enum SpiralRhythm
        {
            Setup,
            Easy,
            Normal,
            Commit,
            Flow,
            Run,
            Vertical,
            Safe
        }

        private readonly struct SpiralBeat
        {
            public SpiralBeat(SpiralRhythm rhythm, float angleDelta, float rise)
            {
                Rhythm = rhythm;
                AngleDelta = angleDelta;
                Rise = rise;
            }

            public SpiralRhythm Rhythm { get; }
            public float AngleDelta { get; }
            public float Rise { get; }
        }

        private sealed class Spiral056Authoring
        {
            public ModulePose StartPose;
            public Vector3 PatchPosition;
            public ModuleBlockDefinition[] Blocks;
            public ModuleRestorePointDefinition[] RestorePoints;
            public List<Vector3> RoutePositions;
        }

        private static Spiral056Authoring BuildSpiral056Route()
        {
            var beats = new[]
            {
                Beat(SpiralRhythm.Setup, 7f, 0.20f), Beat(SpiralRhythm.Easy, 9f, 0.30f),
                Beat(SpiralRhythm.Flow, 6f, 0.25f), Beat(SpiralRhythm.Normal, 11f, 0.50f),
                Beat(SpiralRhythm.Run, 5f, 0.10f), Beat(SpiralRhythm.Vertical, 7f, 0.85f),
                Beat(SpiralRhythm.Normal, 10f, 0.50f), Beat(SpiralRhythm.Commit, 12f, 0.70f),
                Beat(SpiralRhythm.Easy, 8f, 0.30f), Beat(SpiralRhythm.Safe, 6f, 0.15f),

                Beat(SpiralRhythm.Flow, 7f, 0.25f), Beat(SpiralRhythm.Normal, 10f, 0.50f),
                Beat(SpiralRhythm.Vertical, 7f, 0.85f), Beat(SpiralRhythm.Setup, 6f, 0.20f),
                Beat(SpiralRhythm.Commit, 12f, 0.70f), Beat(SpiralRhythm.Run, 5f, 0.10f),
                Beat(SpiralRhythm.Easy, 9f, 0.30f), Beat(SpiralRhythm.Normal, 11f, 0.50f),
                Beat(SpiralRhythm.Flow, 7f, 0.25f), Beat(SpiralRhythm.Safe, 6f, 0.15f),

                Beat(SpiralRhythm.Setup, 6f, 0.20f), Beat(SpiralRhythm.Vertical, 7f, 0.85f),
                Beat(SpiralRhythm.Normal, 10f, 0.50f), Beat(SpiralRhythm.Commit, 12f, 0.70f),
                Beat(SpiralRhythm.Easy, 8f, 0.30f), Beat(SpiralRhythm.Run, 5f, 0.10f),
                Beat(SpiralRhythm.Flow, 7f, 0.25f), Beat(SpiralRhythm.Normal, 11f, 0.50f),
                Beat(SpiralRhythm.Commit, 12f, 0.70f), Beat(SpiralRhythm.Safe, 6f, 0.15f),

                Beat(SpiralRhythm.Flow, 7f, 0.25f), Beat(SpiralRhythm.Easy, 9f, 0.30f),
                Beat(SpiralRhythm.Vertical, 7f, 0.85f), Beat(SpiralRhythm.Normal, 10f, 0.50f),
                Beat(SpiralRhythm.Run, 5f, 0.10f), Beat(SpiralRhythm.Commit, 12f, 0.70f),
                Beat(SpiralRhythm.Setup, 6f, 0.20f), Beat(SpiralRhythm.Normal, 11f, 0.50f),
                Beat(SpiralRhythm.Flow, 7f, 0.25f), Beat(SpiralRhythm.Safe, 6f, 0.15f),

                Beat(SpiralRhythm.Setup, 6f, 0.20f), Beat(SpiralRhythm.Normal, 10f, 0.50f),
                Beat(SpiralRhythm.Commit, 12f, 0.70f), Beat(SpiralRhythm.Vertical, 7f, 0.85f),
                Beat(SpiralRhythm.Run, 5f, 0.10f), Beat(SpiralRhythm.Easy, 9f, 0.30f),
                Beat(SpiralRhythm.Flow, 7f, 0.25f), Beat(SpiralRhythm.Normal, 11f, 0.50f),
                Beat(SpiralRhythm.Commit, 12f, 0.70f), Beat(SpiralRhythm.Safe, 6f, 0.15f),

                Beat(SpiralRhythm.Flow, 7f, 0.30f), Beat(SpiralRhythm.Vertical, 7f, 0.85f),
                Beat(SpiralRhythm.Normal, 10f, 0.55f), Beat(SpiralRhythm.Commit, 12f, 0.70f),
                Beat(SpiralRhythm.Run, 5f, 0.10f), Beat(SpiralRhythm.Easy, 9f, 0.35f),
                Beat(SpiralRhythm.Vertical, 7f, 0.85f), Beat(SpiralRhythm.Normal, 10f, 0.55f)
            };

            var blocks = new List<ModuleBlockDefinition>();
            var restores = new List<ModuleRestorePointDefinition>();
            var route = new List<Vector3>();
            var angle = 180f;
            var radius = 24f;
            var height = 0f;
            var startPosition = SpiralPosition(angle, radius, height);
            blocks.Add(SpiralBlock(
                "m04.start",
                startPosition,
                new Vector3(4.5f, 0.8f, 4.5f),
                ModuleMaterialRole.Normal,
                SpiralTangentYaw(angle)));
            route.Add(startPosition);

            var restoreIds = new[]
            {
                "restore.module-004.r1-base-turn",
                "restore.module-004.r2-moving-exit",
                "restore.module-004.r3-water-exit",
                "restore.module-004.r4-boost-landing",
                "restore.module-004.r5-final-gate"
            };
            var restoreIndex = 0;
            for (var index = 0; index < beats.Length; index++)
            {
                var beat = beats[index];
                angle += ResolvedAngleDelta(beat, radius);
                radius += RadiusDeltaFor(beat.Rhythm);
                height += beat.Rise;
                var position = SpiralPosition(angle, radius, height);
                route.Add(position);
                if (beat.Rhythm == SpiralRhythm.Safe)
                {
                    restores.Add(new ModuleRestorePointDefinition(
                        restoreIds[restoreIndex],
                        restoreIndex,
                        position,
                        new Vector3(0f, 0.35f, 0f)));
                    restoreIndex++;
                    continue;
                }

                var routeIndex = index + 1;
                if (IsSpiralCrumbleRouteIndex(routeIndex)
                    || IsSpiralMovingRouteIndex(routeIndex))
                {
                    continue;
                }

                blocks.Add(SpiralBlock(
                    $"m04.route.{index + 1:00}.{beat.Rhythm.ToString().ToLowerInvariant()}",
                    position,
                    SizeFor(beat.Rhythm),
                    beat.Rhythm == SpiralRhythm.Commit
                        ? ModuleMaterialRole.Precision
                        : ModuleMaterialRole.Normal,
                    SpiralTangentYaw(angle)));
            }

            var finalDirection = new Vector3(
                Mathf.Sin(angle * Mathf.Deg2Rad),
                0f,
                Mathf.Cos(angle * Mathf.Deg2Rad));
            var inwardRadii = new[] { 13.5f, 10.5f, 7.5f, 4.3f };
            for (var index = 0; index < inwardRadii.Length; index++)
            {
                height += index == 0 ? 0.45f : 0.75f;
                var position = finalDirection * inwardRadii[index] + Vector3.up * height;
                route.Add(position);
                blocks.Add(SpiralBlock(
                    $"m04.summit.approach.{index + 1:00}",
                    position,
                    new Vector3(index == 0 ? 3.2f : 2.8f, 0.6f, index == 0 ? 3.2f : 2.8f),
                    ModuleMaterialRole.Normal,
                    Mathf.Atan2(-finalDirection.x, -finalDirection.z) * Mathf.Rad2Deg));
            }

            var patchPosition = new Vector3(0f, height + 1.1f, 0f);
            return new Spiral056Authoring
            {
                StartPose = new ModulePose(
                    startPosition + Vector3.up * 0.45f,
                    new Vector3(0f, SpiralTangentYaw(180f), 0f)),
                PatchPosition = patchPosition,
                Blocks = blocks.ToArray(),
                RestorePoints = restores.ToArray(),
                RoutePositions = route
            };
        }

        private static SpiralBeat Beat(SpiralRhythm rhythm, float angleDelta, float rise)
        {
            return new SpiralBeat(rhythm, angleDelta, rise);
        }

        private static float ResolvedAngleDelta(SpiralBeat beat, float radius)
        {
            float targetCenterDistance;
            switch (beat.Rhythm)
            {
                case SpiralRhythm.Setup: targetCenterDistance = 3.3f; break;
                case SpiralRhythm.Easy: targetCenterDistance = 3.8f; break;
                case SpiralRhythm.Normal: targetCenterDistance = 4.4f; break;
                case SpiralRhythm.Commit: targetCenterDistance = 4.8f; break;
                case SpiralRhythm.Flow: targetCenterDistance = 3.4f; break;
                case SpiralRhythm.Run: targetCenterDistance = 2.25f; break;
                case SpiralRhythm.Vertical: targetCenterDistance = 3.35f; break;
                case SpiralRhythm.Safe: targetCenterDistance = 3.5f; break;
                default: targetCenterDistance = 3.8f; break;
            }

            var measuredAngle = targetCenterDistance
                / Mathf.Max(1f, radius)
                * Mathf.Rad2Deg;
            return Mathf.Max(beat.AngleDelta, measuredAngle);
        }

        private static float RadiusDeltaFor(SpiralRhythm rhythm)
        {
            switch (rhythm)
            {
                case SpiralRhythm.Commit: return -0.18f;
                case SpiralRhythm.Safe: return -0.32f;
                case SpiralRhythm.Run: return 0.04f;
                default: return -0.11f;
            }
        }

        private static Vector3 SizeFor(SpiralRhythm rhythm)
        {
            switch (rhythm)
            {
                case SpiralRhythm.Setup: return new Vector3(2.3f, 0.6f, 2.3f);
                case SpiralRhythm.Easy: return new Vector3(2.3f, 0.6f, 2.3f);
                case SpiralRhythm.Commit: return new Vector3(2.3f, 0.6f, 2.3f);
                case SpiralRhythm.Flow: return new Vector3(2.2f, 0.6f, 2.7f);
                case SpiralRhythm.Run: return new Vector3(2.4f, 0.6f, 3f);
                case SpiralRhythm.Vertical: return new Vector3(2.3f, 0.6f, 2.3f);
                default: return new Vector3(2.2f, 0.6f, 2.2f);
            }
        }

        private static Vector3 SpiralPosition(float angleDegrees, float radius, float height)
        {
            var radians = angleDegrees * Mathf.Deg2Rad;
            return new Vector3(
                Mathf.Sin(radians) * radius,
                height,
                Mathf.Cos(radians) * radius);
        }

        private static float SpiralTangentYaw(float angleDegrees)
        {
            var radians = angleDegrees * Mathf.Deg2Rad;
            var tangent = new Vector3(Mathf.Cos(radians), 0f, -Mathf.Sin(radians));
            return Mathf.Atan2(tangent.x, tangent.z) * Mathf.Rad2Deg;
        }

        private static ModuleBlockDefinition SpiralBlock(
            string id,
            Vector3 position,
            Vector3 size,
            ModuleMaterialRole role,
            float yaw)
        {
            return new ModuleBlockDefinition(
                id,
                position,
                size,
                role,
                id,
                editorLabel: true,
                new Vector3(0f, yaw, 0f));
        }

        private static ModuleShortcutDefinition[] CreateSpiral056Shortcuts(Spiral056Authoring authored)
        {
            var route = authored.RoutePositions;
            return new[]
            {
                new ModuleShortcutDefinition("shortcut.module-004.inner-gap", "Lower inner gap", route[8], route[14], 8f, new[] { ModuleMechanic.PrecisionBlock }),
                new ModuleShortcutDefinition("shortcut.module-004.water-speed", "Moving crossing pace line", route[17], route[20], 4f, new[] { ModuleMechanic.MovingBlock, ModuleMechanic.PrecisionBlock }),
                new ModuleShortcutDefinition("shortcut.module-004.boost-overshoot", "Boost gap closer", route[29], route[32], 6f, new[] { ModuleMechanic.JumpBoost }),
                new ModuleShortcutDefinition("shortcut.module-004.crumble-bridge", "Lower crumble pace line", route[11], route[17], 4f, new[] { ModuleMechanic.CrumblingBlock, ModuleMechanic.PrecisionBlock }),
                new ModuleShortcutDefinition("shortcut.module-004.high-risk-drop", "Upper crumble pace line", route[50], route[54], 4f, new[] { ModuleMechanic.CrumblingBlock, ModuleMechanic.PrecisionBlock })
            };
        }

        private static ModuleMovingBlockDefinition[] CreateSpiral056MovingBlocks(Spiral056Authoring authored)
        {
            var entry = authored.RoutePositions[18];
            var exit = authored.RoutePositions[19];
            return new[]
            {
                Moving(
                    "moving.m04.required-crossing",
                    entry,
                    new Vector3(3f, 0.6f, 2.4f),
                    new[] { entry, exit },
                    2.1f,
                    0.85f)
            };
        }

        private static ModuleBoostBlockDefinition[] CreateSpiral056BoostBlocks(Spiral056Authoring authored)
        {
            var anchor = InwardOffset(authored.RoutePositions[29], 3f);
            var next = authored.RoutePositions[32];
            var direction = (next - anchor).normalized;
            return new[]
            {
                new ModuleBoostBlockDefinition(
                    "boost.m04.first-ascent",
                    anchor,
                    new Vector3(2.2f, 0.6f, 2.2f),
                    direction,
                    12.4f,
                    7.2f,
                    0.35f)
            };
        }

        private static ModuleCrumblingBlockDefinition[] CreateSpiral056CrumblingBlocks(Spiral056Authoring authored)
        {
            var routeIndices = new[]
            {
                11, 12, 13,
                15, 16,
                21, 22, 23, 24,
                31, 32, 33,
                41, 42, 43, 44,
                51, 52, 53
            };
            var blocks = new ModuleCrumblingBlockDefinition[routeIndices.Length];
            for (var index = 0; index < routeIndices.Length; index++)
            {
                var routeIndex = routeIndices[index];
                var position = authored.RoutePositions[routeIndex];
                var angle = Mathf.Atan2(position.x, position.z) * Mathf.Rad2Deg;
                blocks[index] = Crumble(
                    $"crumble.m04.route.{routeIndex:00}",
                    position,
                    SpiralTangentYaw(angle));
            }

            return blocks;
        }

        private static ModuleCrumblingBlockDefinition Crumble(string id, Vector3 position, float yaw)
        {
            return new ModuleCrumblingBlockDefinition(
                id,
                position,
                new Vector3(2.4f, 0.6f, 2.4f),
                0.03f,
                1f,
                4f,
                0.025f,
                new Vector3(0f, yaw, 0f));
        }

        private static bool IsSpiralCrumbleRouteIndex(int routeIndex)
        {
            switch (routeIndex)
            {
                case 11: case 12: case 13:
                case 15: case 16:
                case 21: case 22: case 23: case 24:
                case 31: case 32: case 33:
                case 41: case 42: case 43: case 44:
                case 51: case 52: case 53:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsSpiralMovingRouteIndex(int routeIndex)
        {
            return routeIndex == 18 || routeIndex == 19;
        }

        private static ModuleBlockDefinition[] CreateSpiral056Landmarks(Spiral056Authoring authored)
        {
            return new[]
            {
                // A continuous broken spire gives the circular route one readable destination.
                Block("m04.world.core.base", 0f, -10f, 0f, 15f, 7f, 15f, ModuleMaterialRole.DecorationStone),
                Block("m04.world.core.lower", -3.8f, -0.6f, -2.6f, 6.6f, 7f, 6.2f, ModuleMaterialRole.TowerCore),
                Block("m04.world.core.mid", 4.6f, 7.8f, 3.2f, 5.6f, 6.2f, 5.4f, ModuleMaterialRole.TowerCore),
                Block("m04.world.core.upper", -4.2f, 15.8f, 4.8f, 4.8f, 5.2f, 4.6f, ModuleMaterialRole.TowerCore),
                Block("m04.world.core.energy-low", -7f, 3.6f, -1.5f, 1.8f, 1.8f, 1.8f, ModuleMaterialRole.CircuitLine),
                Block("m04.world.core.energy-high", 6.7f, 12.5f, 3.4f, 1.6f, 1.6f, 1.6f, ModuleMaterialRole.CircuitLine),

                // Lower ruins: close structural anchors establish that the route belongs to a place.
                Block("m04.world.lower.start-island", -4f, -4.2f, -28f, 10f, 7f, 10f, ModuleMaterialRole.DecorationStone),
                Block("m04.world.lower.gateway-island", -30f, -2.2f, -10f, 10f, 6f, 10f, ModuleMaterialRole.DecorationStone),
                Block("m04.world.lower.gateway", -31f, 1.5f, -12f, 5.5f, 5.5f, 5.5f, ModuleMaterialRole.ShortcutCue),
                Block("m04.world.lower.tree", -28f, 2.5f, -9f, 4.2f, 4.2f, 4.2f, ModuleMaterialRole.Vegetation),
                Block("m04.world.lower.ruin-west-island", -34f, 0.4f, 13f, 9f, 6f, 9f, ModuleMaterialRole.DecorationStone),
                Block("m04.world.lower.ruin-west", -34f, 5f, 13f, 6.5f, 6.5f, 6.5f, ModuleMaterialRole.TowerCore),
                Block("m04.world.lower.ruin-east-island", 31f, -0.2f, 15f, 8f, 6f, 8f, ModuleMaterialRole.DecorationStone),
                Block("m04.world.lower.ruin-east", 31f, 4f, 15f, 5.5f, 5.5f, 5.5f, ModuleMaterialRole.TowerCore),
                Block("m04.world.lower.cloud-bank", 2f, -9f, -50f, 28f, 5f, 14f, ModuleMaterialRole.Cloud),
                Block("m04.world.depth.cloud-west", -48f, -25f, -15f, 34f, 5f, 18f, ModuleMaterialRole.Cloud),
                Block("m04.world.depth.cloud-east", 50f, -28f, 6f, 36f, 5f, 18f, ModuleMaterialRole.Cloud),
                Block("m04.world.depth.ruin-west", -58f, -31f, 35f, 7f, 14f, 7f, ModuleMaterialRole.TowerCore),
                Block("m04.world.depth.ruin-east", 61f, -34f, 42f, 6f, 12f, 6f, ModuleMaterialRole.TowerCore),
                Block("m04.world.depth.cloud-low-center", 3f, -46f, 18f, 44f, 5f, 24f, ModuleMaterialRole.Cloud),
                Block("m04.world.depth.cloud-south", -12f, -58f, -58f, 40f, 4f, 18f, ModuleMaterialRole.Cloud),
                Block("m04.world.depth.ruin-faint-north", 24f, -54f, 76f, 5f, 16f, 5f, ModuleMaterialRole.TowerCore),
                Block("m04.world.depth.ominous-fog-west", -18f, -38f, -8f, 70f, 13f, 46f, ModuleMaterialRole.Cloud),
                Block("m04.world.depth.ominous-fog-east", 24f, -54f, 42f, 76f, 14f, 48f, ModuleMaterialRole.Cloud),
                Block("m04.world.depth.abyss-fog-deep", 4f, -72f, -16f, 104f, 16f, 68f, ModuleMaterialRole.Cloud),
                Block("m04.world.depth.abyss-fog-back", -6f, -86f, 64f, 86f, 12f, 54f, ModuleMaterialRole.Cloud),
                Block("m04.world.depth.red-glow-core", 0f, -74f, 8f, 7f, 12f, 7f, ModuleMaterialRole.CrumbleFault),
                Block("m04.world.depth.red-glow-south", -18f, -82f, -44f, 6f, 10f, 6f, ModuleMaterialRole.CrumbleFault),
                Block("m04.world.depth.red-glow-east", 34f, -78f, 32f, 5f, 9f, 5f, ModuleMaterialRole.CrumbleFault),

                // Garden sky: warmer vegetation and a broad arch make the second ascent memorable.
                Block("m04.world.garden.island", -38f, 6f, 23f, 9f, 7f, 9f, ModuleMaterialRole.DecorationStone),
                Block("m04.world.garden.tree", -38f, 10f, 23f, 5f, 5f, 5f, ModuleMaterialRole.Vegetation),
                Block("m04.world.garden.flower", -33f, 9.4f, 20f, 2.8f, 2.8f, 2.8f, ModuleMaterialRole.Flower),
                Block("m04.world.garden.arch", 24f, 11.5f, 25f, 6f, 6f, 6f, ModuleMaterialRole.ShortcutCue),
                Block("m04.world.garden.cloud-bank", -50f, 0f, 5f, 22f, 4f, 12f, ModuleMaterialRole.Cloud),

                // Energy ruins: cyan vertical markers echo the moving, water, and surf mastery lines.
                Block("m04.world.energy.island", 36f, 9f, -22f, 8f, 7f, 8f, ModuleMaterialRole.DecorationStone),
                Block("m04.world.energy.pillar-low", 34f, 13f, -19f, 6f, 6f, 6f, ModuleMaterialRole.TowerCore),
                Block("m04.world.energy.pillar-cyan", 30f, 15f, -23f, 4.5f, 4.5f, 4.5f, ModuleMaterialRole.CircuitLine),
                Block("m04.world.energy.arch", -22f, 17f, -30f, 5.5f, 5.5f, 5.5f, ModuleMaterialRole.ShortcutCue),

                // Summit ruins: exposed paired markers frame the final inward approach and Patch.
                Block("m04.world.summit.pillar-west", -6.5f, authored.PatchPosition.y + 2.8f, 4.5f, 6.5f, 6.5f, 6.5f, ModuleMaterialRole.TowerCore),
                Block("m04.world.summit.pillar-east", 6.5f, authored.PatchPosition.y + 2.8f, -4.5f, 6.5f, 6.5f, 6.5f, ModuleMaterialRole.TowerCore),
                Block("m04.world.summit.energy", 0f, authored.PatchPosition.y + 4.5f, 8f, 4.5f, 4.5f, 4.5f, ModuleMaterialRole.CircuitLine),

                // Cheap cloud layers imply a wider floating civilization without filling the route view.
                Block("m04.world.far.island-west", -82f, 8f, 36f, 7f, 7f, 7f, ModuleMaterialRole.DecorationStone),
                Block("m04.world.far.tower-west", -80f, 18f, 38f, 8f, 8f, 8f, ModuleMaterialRole.TowerCore),
                Block("m04.world.far.island-east", 88f, 15f, 22f, 7f, 7f, 7f, ModuleMaterialRole.DecorationStone),
                Block("m04.world.far.tower-east", 86f, 26f, 23f, 8f, 8f, 8f, ModuleMaterialRole.TowerCore),
                Block("m04.world.far.island-north", 16f, 24f, 105f, 6f, 6f, 6f, ModuleMaterialRole.DecorationStone),
                Block("m04.world.far.cloud-west", -72f, -2f, -22f, 32f, 5f, 16f, ModuleMaterialRole.Cloud),
                Block("m04.world.far.cloud-east", 74f, 4f, -35f, 30f, 5f, 14f, ModuleMaterialRole.Cloud),
                Block("m04.world.far.cloud-north", 5f, 10f, 92f, 36f, 6f, 18f, ModuleMaterialRole.Cloud)
            };
        }

        private static ModuleCameraHintDefinition[] CreateSpiral056CameraHints(Spiral056Authoring authored)
        {
            var route = authored.RoutePositions;
            return new[]
            {
                CameraHint("camera.m04.establish-tower", route[4], 70f, -12f),
                CameraHint("camera.m04.water-entry", route[19], -20f, -10f),
                CameraHint("camera.m04.big-boost", route[34], 160f, -14f),
                CameraHint("camera.m04.summit", route[52], -110f, -18f)
            };
        }

        private static ModuleCameraHintDefinition CameraHint(
            string id,
            Vector3 position,
            float yaw,
            float pitch)
        {
            return new ModuleCameraHintDefinition(
                id,
                position + Vector3.up * 1.2f,
                new Vector3(10f, 6f, 10f),
                yaw,
                pitch,
                12f);
        }

        private static Vector3 InwardOffset(Vector3 position, float distance)
        {
            var horizontal = new Vector3(position.x, 0f, position.z);
            return horizontal.sqrMagnitude <= 0.001f
                ? position
                : position - horizontal.normalized * distance;
        }

        private static void ConfigureTheSpiral(
            ModuleDefinition module,
            ModuleVisualProfile visual,
            ModuleEnvironmentProfile environment)
        {
            module.name = "Module_004_TheSpiral";
            module.Configure(
                "module.004.the-spiral",
                "THE SPIRAL",
                "module_004_the_spiral",
                "project.ryders-block",
                "world.prototype-sky",
                1,
                ModuleSelectionState.ModuleRunnerSceneName,
                ModuleDifficulty.Easy,
                new ModulePose(new Vector3(0f, 0.35f, -76f), new Vector3(0f, 18f, 0f)),
                new ModulePatchBlockDefinition(
                    "patch.module-004",
                    new Vector3(0f, 32.1f, 0f),
                    new Vector3(2.2f, 2.2f, 2.2f)),
                new[]
                {
                    new ModuleRestorePointDefinition(
                        "restore.module-004.r1-base-turn",
                        0,
                        new Vector3(18f, 3.25f, 18f),
                        new Vector3(0f, 0.35f, 0f)),
                    new ModuleRestorePointDefinition(
                        "restore.module-004.r2-moving-exit",
                        1,
                        new Vector3(-25f, 7.35f, -9f),
                        new Vector3(0f, 0.35f, 0f)),
                    new ModuleRestorePointDefinition(
                        "restore.module-004.r3-water-exit",
                        2,
                        new Vector3(22f, 9.95f, -12f),
                        new Vector3(0f, 0.35f, 0f)),
                    new ModuleRestorePointDefinition(
                        "restore.module-004.r4-boost-landing",
                        3,
                        new Vector3(-8f, 16.45f, 34f),
                        new Vector3(0f, 0.35f, 0f)),
                    new ModuleRestorePointDefinition(
                        "restore.module-004.r5-final-gate",
                        4,
                        new Vector3(-9f, 19.85f, -28f),
                        new Vector3(0f, 0.35f, 0f))
                },
                210f,
                420f,
                new[]
                {
                    ModuleMechanic.StandardBlock,
                    ModuleMechanic.PrecisionBlock,
                    ModuleMechanic.MovingBlock,
                    ModuleMechanic.RestorePoint,
                    ModuleMechanic.JumpBoost,
                    ModuleMechanic.FlowingWater,
                    ModuleMechanic.SurfSurface,
                    ModuleMechanic.CrumblingBlock,
                    ModuleMechanic.Shortcut,
                    ModuleMechanic.PatchBlock,
                    ModuleMechanic.CameraGuide
                },
                new[]
                {
                    new ModuleShortcutDefinition(
                        "shortcut.module-004.inner-gap",
                        "Inner corner skip",
                        new Vector3(18f, 2.2f, 18f),
                        new Vector3(-2f, 3.3f, 32f),
                        18f,
                        new[] { ModuleMechanic.PrecisionBlock, ModuleMechanic.MovingBlock }),
                    new ModuleShortcutDefinition(
                        "shortcut.module-004.water-speed",
                        "Water speed line",
                        new Vector3(-12f, 7.3f, -31f),
                        new Vector3(15f, 8.8f, -24f),
                        16f,
                        new[] { ModuleMechanic.FlowingWater, ModuleMechanic.JumpBoost }),
                    new ModuleShortcutDefinition(
                        "shortcut.module-004.boost-overshoot",
                        "Boost overshoot",
                        new Vector3(24f, 9.8f, -2f),
                        new Vector3(10f, 12.8f, 34f),
                        22f,
                        new[] { ModuleMechanic.JumpBoost, ModuleMechanic.PrecisionBlock }),
                    new ModuleShortcutDefinition(
                        "shortcut.module-004.surf-line",
                        "Inner surf line",
                        new Vector3(-8f, 16.2f, 34f),
                        new Vector3(-20f, 18.3f, -19f),
                        24f,
                        new[] { ModuleMechanic.SurfSurface, ModuleMechanic.PrecisionBlock }),
                    new ModuleShortcutDefinition(
                        "shortcut.module-004.high-risk-drop",
                        "Summit drop skip",
                        new Vector3(-9f, 20.1f, -28f),
                        new Vector3(8f, 24.5f, 26f),
                        20f,
                        new[] { ModuleMechanic.PrecisionBlock, ModuleMechanic.JumpBoost })
                },
                environment,
                visual,
                "A large vertical route curls around a broken tower while multiple movement systems overlap.",
                "S23 retune: broad Bronze spine, five Restore Points, standard completion without bhop, air-strafe, surf, boost overshoot, or shortcut mastery.",
                new[]
                {
                    Block("m04.start", 0f, 0f, -76f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),
                    Block("m04.base.step-01", 0f, 0.1f, -72f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.base.01", 0f, 0.2f, -68f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.base.step-02", 2f, 0.35f, -64f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.base.02", 4.5f, 0.5f, -60f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.base.step-03", 7.5f, 0.65f, -56f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.base.03", 10.5f, 0.8f, -52f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.base.step-04", 13.5f, 0.95f, -48f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.base.bronze-bridge.01", 16.5f, 1.1f, -44f, 1.8f, 0.6f, 1.8f, ModuleMaterialRole.Normal),
                    Block("m04.base.step-05", 19f, 1.25f, -40.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.base.05", 21f, 1.4f, -37f, 1.8f, 0.6f, 1.8f, ModuleMaterialRole.Normal),
                    Block("m04.base.step-06", 23f, 1.55f, -33f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.base.step-06b", 24.5f, 1.6f, -28.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.base.step-06c", 25f, 1.65f, -24f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.base.long-jump-a", 25f, 1.7f, -19f, 3.0f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.base.step-07", 25f, 1.85f, -14f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.base.long-jump-b", 24f, 2.0f, -9f, 3.0f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.base.step-08", 23f, 2.15f, -4f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.base.hub", 21f, 2.3f, 1f, 3.0f, 0.6f, 3.0f, ModuleMaterialRole.Normal),
                    Block("m04.moving.approach-step", 18.5f, 2.35f, 5.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.moving.approach", 16.5f, 2.4f, 9.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.moving.step-01", 14f, 2.45f, 14f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.moving.runup", 11.5f, 2.5f, 18.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.moving.bronze-bridge.01", 9.5f, 2.6f, 22f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.moving.step-02", 7f, 2.65f, 25.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.moving.step-03", 4.5f, 2.68f, 28.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.moving.exit", 2f, 2.7f, 30f, 1.8f, 0.6f, 1.8f, ModuleMaterialRole.Normal),
                    Block("m04.moving.safe.01", -1.5f, 2.8f, 30f, 1.8f, 0.6f, 1.8f, ModuleMaterialRole.Normal),
                    Block("m04.moving.step-04", -5f, 2.85f, 29f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.moving.safe.02", -8.5f, 2.9f, 27f, 1.8f, 0.6f, 1.8f, ModuleMaterialRole.Normal),
                    Block("m04.moving.safe.03", -12f, 3.2f, 24f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.moving.step-05", -15f, 3.4f, 20.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.moving.lift-entry", -18f, 3.6f, 16f, 1.8f, 0.6f, 1.8f, ModuleMaterialRole.Normal),
                    Block("m04.moving.step-06", -21f, 4.1f, 12f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.moving.lift-mid", -23.5f, 4.7f, 8f, 1.8f, 0.6f, 1.8f, ModuleMaterialRole.Normal),
                    Block("m04.moving.step-07", -25.5f, 5.2f, 4f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.moving.lift-exit", -27f, 5.8f, 0.5f, 3.0f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.moving.step-08", -26.5f, 5.95f, -4f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.moving.terrace", -25f, 6.1f, -8f, 3.0f, 0.6f, 3.0f, ModuleMaterialRole.Normal),
                    Block("m04.water.step-01", -23.5f, 6.2f, -12f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.water.entry", -21f, 6.3f, -16f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.water.step-02", -18.5f, 6.45f, -20f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.water.wrap.01", -15.5f, 6.6f, -24f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.water.step-03", -12f, 6.75f, -27f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.water.wrap.02", -8f, 6.9f, -29.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.water.step-04", -4f, 7.0f, -31f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.water.wrap.03", 0f, 7.1f, -32f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.water.step-05", 4f, 7.2f, -31.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.water.wrap.04", 8f, 7.3f, -30f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.water.step-06", 12f, 7.45f, -27f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.water.wrap.05", 15.5f, 7.6f, -23.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.water.wrap.06", 18.5f, 7.9f, -19.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.water.step-07", 21f, 8.05f, -15f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.water.bronze-bridge.06", 23f, 8.2f, -10.5f, 1.8f, 0.6f, 1.8f, ModuleMaterialRole.Normal),
                    Block("m04.boost.step-01", 24.5f, 8.55f, -6f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.boost.runup", 25f, 8.9f, -1.5f, 1.8f, 0.6f, 1.8f, ModuleMaterialRole.Normal),
                    Block("m04.boost.bronze-bridge.01", 25f, 9.5f, 2.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.boost.step-02", 24.5f, 9.9f, 6.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.boost.bronze-bridge.02", 24f, 10.4f, 10.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.boost.landing.01", 23f, 10.9f, 14.5f, 3.0f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.boost.climb.01", 21.5f, 11.4f, 18.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.boost.step-03", 19f, 11.6f, 22f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.boost.climb.02", 16f, 11.8f, 25.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.boost.step-04", 13f, 12.0f, 28.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.boost.bronze-bridge.03", 9.5f, 12.7f, 31f, 1.8f, 0.6f, 1.8f, ModuleMaterialRole.Normal),
                    Block("m04.boost.climb.04", 6f, 13.5f, 33f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.boost.step-05", 2.5f, 14.0f, 34.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.boost.big-landing", -1f, 14.5f, 35.5f, 3.0f, 0.6f, 3.0f, ModuleMaterialRole.Normal),
                    Block("m04.surf.step-01", -4.5f, 14.8f, 35f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.surf.exit", -8f, 15.0f, 33.5f, 1.8f, 0.6f, 1.8f, ModuleMaterialRole.Normal),
                    Block("m04.surf.outer.01", -12f, 15.4f, 30.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.surf.step-02", -15.5f, 15.6f, 27f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.surf.outer.02", -18.5f, 15.8f, 23.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.surf.step-03", -21.5f, 16.0f, 19.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.surf.outer.03", -24f, 16.2f, 15.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.surf.step-04", -26f, 16.4f, 11f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.surf.outer.04", -27f, 16.6f, 6.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.surf.step-05", -27f, 16.8f, 2f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.surf.outer.05", -26f, 17.0f, -2.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.surf.step-06", -24f, 17.2f, -7f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.surf.outer.06", -21.5f, 17.4f, -11f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.surf.step-07", -18.5f, 17.6f, -15f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.surf.outer.07", -15f, 17.8f, -18.5f, 1.8f, 0.6f, 1.8f, ModuleMaterialRole.Normal),
                    Block("m04.surf.step-08", -11.5f, 18.0f, -21.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.surf.outer.08", -7.5f, 18.2f, -23.5f, 1.8f, 0.6f, 1.8f, ModuleMaterialRole.Normal),
                    Block("m04.surf.step-09", -3.5f, 18.35f, -24.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.surf.bronze-bridge.06", 0.5f, 18.5f, -24.5f, 3.0f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.step-01", 4.5f, 18.7f, -23.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.01", 8.5f, 18.9f, -21.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.step-02", 12f, 19.2f, -18.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.02", 15f, 19.4f, -15f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.03", 17.5f, 19.7f, -11f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.crumble-runup", 19f, 20.0f, -6.5f, 3.0f, 0.6f, 3.0f, ModuleMaterialRole.Normal),
                    Block("m04.final.step-03", 20f, 20.15f, -2f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.bronze-bridge.02", 20f, 20.3f, 2.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.step-04", 19f, 20.45f, 7f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.moving-entry", 17f, 20.6f, 11f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.moving-mid", 14.5f, 20.9f, 14.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.bronze-bridge.03", 11.5f, 21.1f, 17.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.moving-exit", 8f, 21.3f, 20f, 1.8f, 0.6f, 1.8f, ModuleMaterialRole.Normal),
                    Block("m04.final.boost-runup", 4.5f, 21.7f, 21.5f, 1.8f, 0.6f, 1.8f, ModuleMaterialRole.Normal),
                    Block("m04.final.bronze-bridge.04", 1f, 22.4f, 22.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.bronze-bridge.05", -2.5f, 23.1f, 22f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.step-05", -5.5f, 23.5f, 20f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.launch-landing", -8f, 23.8f, 17f, 3.0f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.climb.01", -9.5f, 24.5f, 13f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.climb.02", -9.5f, 25.3f, 8.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.climb.03", -8f, 26.1f, 4.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.climb.04", -5.5f, 26.9f, 1.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.climb.05", -3f, 27.7f, -1f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("m04.final.bronze-bridge.07", -1f, 28.5f, 0f, 1.8f, 0.6f, 1.8f, ModuleMaterialRole.Normal),
                    Block("m04.summit", 0f, 29.3f, 0f, 4.5f, 0.8f, 4.5f, ModuleMaterialRole.Normal)
                },
                new[]
                {
                    Moving(
                        "moving.m04.side-cycle",
                        new Vector3(8f, 2.35f, 26f),
                        new Vector3(3.0f, 0.6f, 1.5f),
                        new[] { new Vector3(11f, 2.35f, 26f), new Vector3(2f, 2.35f, 29f) },
                        1.35f,
                        0.45f),
                    Moving(
                        "moving.m04.vertical-lift",
                        new Vector3(-27f, 4.65f, 8f),
                        new Vector3(3.0f, 0.6f, 1.5f),
                        new[] { new Vector3(-27f, 4.65f, 8f), new Vector3(-27f, 6.0f, 8f) },
                        1.05f,
                        0.5f),
                    Moving(
                        "moving.m04.final-sway",
                        new Vector3(23f, 21.2f, 3f),
                        new Vector3(3.0f, 0.6f, 1.5f),
                        new[] { new Vector3(25f, 21.2f, 3f), new Vector3(18f, 21.2f, 9f) },
                        1.25f,
                        0.45f)
                },
                new[]
                {
                    new ModuleBoostBlockDefinition(
                        "boost.m04.first-ascent",
                        new Vector3(25f, 9.25f, -1.5f),
                        new Vector3(1.8f, 0.6f, 1.8f),
                        new Vector3(-0.05f, 0.18f, 1f),
                        12.4f,
                        7.2f,
                        0.35f),
                    new ModuleBoostBlockDefinition(
                        "boost.m04.sky-kick",
                        new Vector3(10f, 12.55f, 34f),
                        new Vector3(1.8f, 0.6f, 1.8f),
                        new Vector3(-0.85f, 0.24f, 0.5f),
                        13.2f,
                        8.0f,
                        0.35f),
                    new ModuleBoostBlockDefinition(
                        "boost.m04.final-launch",
                        new Vector3(18f, 22.15f, 18f),
                        new Vector3(1.8f, 0.6f, 1.8f),
                        new Vector3(-0.8f, 0.18f, 0.58f),
                        13.5f,
                        8.2f,
                        0.35f)
                },
                new[]
                {
                    new ModuleWaterVolumeDefinition(
                        "water.m04.stream-a",
                        new Vector3(-18f, 6.92f, -24f),
                        new Vector3(5f, 1.35f, 13f),
                        new Vector3(0.75f, 0f, -0.25f),
                        9f,
                        3.2f,
                        0f,
                        1.35f,
                        new Vector3(0f, -24f, 0f)),
                    new ModuleWaterVolumeDefinition(
                        "water.m04.stream-b",
                        new Vector3(-4f, 7.25f, -33f),
                        new Vector3(5f, 1.35f, 15f),
                        new Vector3(1f, 0f, 0.12f),
                        9f,
                        3.3f,
                        0f,
                        1.25f,
                        new Vector3(0f, 86f, 0f)),
                    new ModuleWaterVolumeDefinition(
                        "water.m04.stream-c",
                        new Vector3(12f, 7.75f, -28f),
                        new Vector3(5f, 1.35f, 14f),
                        new Vector3(0.62f, 0f, 0.78f),
                        9.5f,
                        3.4f,
                        0f,
                        1.2f,
                        new Vector3(0f, 38f, 0f))
                },
                new[]
                {
                    new ModuleCrumblingBlockDefinition(
                        "crumble.m04.final.01",
                        new Vector3(22f, 20.25f, -15f),
                        new Vector3(4.8f, 0.6f, 4.4f),
                        0.03f,
                        1f,
                        4f,
                        0.03f),
                    new ModuleCrumblingBlockDefinition(
                        "crumble.m04.final.02",
                        new Vector3(27f, 20.65f, -10f),
                        new Vector3(4.6f, 0.6f, 4.2f),
                        0.03f,
                        1f,
                        4f,
                        0.03f),
                    new ModuleCrumblingBlockDefinition(
                        "crumble.m04.final.03",
                        new Vector3(27f, 21.05f, -4f),
                        new Vector3(4.6f, 0.6f, 4.2f),
                        0.03f,
                        1f,
                        4f,
                        0.03f)
                },
                new[]
                {
                    Block("m04.tower.core.low", 42f, 8f, 6f, 6.5f, 6.5f, 6.5f, ModuleMaterialRole.TowerCore),
                    Block("m04.tower.core.mid", 42f, 22f, 6f, 8f, 8f, 8f, ModuleMaterialRole.TowerCore),
                    Block("m04.tower.core.high", 42f, 36f, 6f, 8f, 8f, 8f, ModuleMaterialRole.TowerCore),
                    Block("m04.tower.ring.low.n", 42f, 10f, 22f, 12f, 0.9f, 2.4f, ModuleMaterialRole.DecorationStone),
                    Block("m04.tower.ring.low.e", 58f, 10f, 6f, 2.4f, 0.9f, 12f, ModuleMaterialRole.DecorationStone),
                    Block("m04.tower.ring.mid.s", 42f, 25f, -10f, 13f, 0.9f, 2.4f, ModuleMaterialRole.DecorationStone),
                    Block("m04.tower.ring.mid.w", 26f, 25f, 6f, 2.4f, 0.9f, 13f, ModuleMaterialRole.DecorationStone),
                    Block("m04.tower.ring.high.n", 42f, 39f, 20f, 10f, 0.9f, 2.2f, ModuleMaterialRole.DecorationStone),
                    Block("m04.island.base-left", -24f, -2.2f, -34f, 4.5f, 4.5f, 4.5f, ModuleMaterialRole.DecorationStone),
                    Block("m04.island.base-right", 34f, -1.6f, -16f, 4.5f, 4.5f, 4.5f, ModuleMaterialRole.DecorationStone),
                    Block("m04.island.garden-low", 28f, 0.4f, 18f, 4.5f, 4.5f, 4.5f, ModuleMaterialRole.DecorationStone),
                    Block("m04.island.garden-mid", -36f, 9f, 20f, 4.5f, 4.5f, 4.5f, ModuleMaterialRole.DecorationStone),
                    Block("m04.island.waterfall-fragment", -34f, 3.4f, -28f, 4.5f, 4.5f, 4.5f, ModuleMaterialRole.DecorationStone),
                    Block("m04.island.final-overlook", 28f, 18f, 24f, 4.5f, 4.5f, 4.5f, ModuleMaterialRole.DecorationStone),
                    Block("m04.arch.water-overlook", 3f, 10.5f, -39f, 5f, 5f, 5f, ModuleMaterialRole.ShortcutCue),
                    Block("m04.energy.pillar.low", 31f, 5.8f, -7f, 4.5f, 4.5f, 4.5f, ModuleMaterialRole.CircuitLine),
                    Block("m04.tree.low.gold", 29f, 2.8f, 17f, 4f, 4f, 4f, ModuleMaterialRole.Vegetation),
                    Block("m04.tree.low.pink", 24f, 3.0f, 21f, 2.5f, 2.5f, 2.5f, ModuleMaterialRole.Flower),
                    Block("m04.tree.mid.green", -38f, 12f, 20f, 4f, 4f, 4f, ModuleMaterialRole.Vegetation),
                    Block("m04.tree.mid.pink", -34f, 11.6f, 25f, 2.5f, 2.5f, 2.5f, ModuleMaterialRole.Flower),
                    Block("m04.corruption.low", -16f, 4.5f, -26f, 2.2f, 4.2f, 2.2f, ModuleMaterialRole.Corruption),
                    Block("m04.corruption.final", 30f, 22f, -10f, 3f, 7f, 3f, ModuleMaterialRole.Corruption),
                    Block("m04.summit.marker.a", -5f, 35f, -5f, 2.4f, 7f, 2.4f, ModuleMaterialRole.TowerCore),
                    Block("m04.summit.marker.b", 5f, 35f, 5f, 2.4f, 7f, 2.4f, ModuleMaterialRole.TowerCore),
                    Block("m04.distant.island.base", -48f, 12f, -20f, 4.5f, 4.5f, 4.5f, ModuleMaterialRole.DecorationStone),
                    Block("m04.distant.island.water", 28f, 10f, -42f, 4.5f, 4.5f, 4.5f, ModuleMaterialRole.DecorationStone),
                    Block("m04.distant.pillar.boost", 36f, 18f, 26f, 8f, 8f, 8f, ModuleMaterialRole.TowerCore),
                    Block("m04.distant.arch.summit", -18f, 32f, 18f, 5f, 5f, 5f, ModuleMaterialRole.ShortcutCue)
                },
                new[]
                {
                    new ModuleCameraHintDefinition(
                        "camera.m04.establish-tower",
                        new Vector3(8f, 1.6f, -31f),
                        new Vector3(14f, 6f, 14f),
                        16f,
                        -12f,
                        10f),
                    new ModuleCameraHintDefinition(
                        "camera.m04.water-entry",
                        new Vector3(-22f, 7.6f, -18f),
                        new Vector3(11f, 6f, 11f),
                        124f,
                        -8f,
                        12f),
                    new ModuleCameraHintDefinition(
                        "camera.m04.big-boost",
                        new Vector3(24f, 11f, 12f),
                        new Vector3(13f, 7f, 14f),
                        -18f,
                        -13f,
                        14f),
                    new ModuleCameraHintDefinition(
                        "camera.m04.summit",
                        new Vector3(-7f, 29.2f, 3f),
                        new Vector3(12f, 8f, 12f),
                        42f,
                        -18f,
                        14f)
                });
            module.ConfigurePlayability(
                "SpiralStartAnchor",
                "m04.start",
                -7.5f,
                disableAutomaticDistantFragments: true);
            module.ConfigureSurfSurfaces(new[]
            {
                new ModuleSurfSurfaceDefinition(
                    "surf.m04.inner-wall-a",
                    new Vector3(-12f, 16.0f, 18f),
                    new Vector3(4f, 0.45f, 18f),
                    new Vector3(-0.25f, 0f, -1f),
                    new Vector3(0f, -30f, 42f)),
                new ModuleSurfSurfaceDefinition(
                    "surf.m04.inner-wall-b",
                    new Vector3(-14f, 17.0f, -2f),
                    new Vector3(4f, 0.45f, 20f),
                    new Vector3(-0.15f, 0f, -1f),
                    new Vector3(0f, 8f, -42f))
            });
            module.ConfigureRankThresholds(Thresholds(
                360f,
                260f,
                210f,
                "THE SPIRAL provisional S23 retune. Bronze is any valid completion; standard route should be beatable with calm Classic run/jump play and no advanced movement."));
            module.ConfigureCompletionRewards(BronzeRewards(
                "module-004",
                250,
                "loot.package.module-004",
                "skin.ryder.patch-glow"));
        }

        private static void ConfigurePlatformTruthLab(
            ModuleDefinition module,
            ModuleVisualProfile visual,
            ModuleEnvironmentProfile environment)
        {
            module.name = "Module_PlatformTruthLab";
            module.Configure(
                "module.platform-truth-lab",
                "Platform Truth Lab (Collision & Mechanic Calibration)",
                "module_platform_truth_lab",
                "project.ryders-block",
                "world.prototype-sky",
                99,
                ModuleRunnerScenePath,
                ModuleDifficulty.Intro,
                new ModulePose(new Vector3(0f, 0.35f, 0f), Vector3.zero),
                new ModulePatchBlockDefinition(
                    "patch.truth",
                    new Vector3(0f, 2.8f, 78f),
                    new Vector3(2.2f, 2.2f, 2.2f)),
                new[]
                {
                    new ModuleRestorePointDefinition(
                        "restore.truth.midpoint",
                        0,
                        new Vector3(0f, 0.8f, 38f),
                        new Vector3(0f, 0.35f, 0f))
                },
                60f,
                120f,
                new[]
                {
                    ModuleMechanic.StandardBlock,
                    ModuleMechanic.PrecisionBlock,
                    ModuleMechanic.MovingBlock,
                    ModuleMechanic.CrumblingBlock,
                    ModuleMechanic.JumpBoost,
                    ModuleMechanic.SurfSurface,
                    ModuleMechanic.RestorePoint,
                    ModuleMechanic.PatchBlock
                },
                null,
                environment,
                visual,
                "Platform truth and collision calibration regression lab.",
                "Verify 1x1 golden reference, long/large platforms, restore spawn, patch goal, boost pad, surf ramp.",
                new[]
                {
                    // Station 1: Start platform (2x2 modular)
                    Block("truth.platform.start", 0f, 0f, 0f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),

                    // Station 2: Standard 1x1 Blocks (Golden Reference)
                    Block("truth.block.std.01", 0f, 0f, 4.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("truth.block.std.02", 0f, 0f, 8.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),

                    // Station 3: Long 2x1 Platforms
                    Block("truth.platform.long.x", 0f, 0f, 13.0f, 3.0f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("truth.platform.long.z", 0f, 0f, 17.5f, 1.5f, 0.6f, 3.0f, ModuleMaterialRole.Normal),

                    // Station 4: Multi-tile 2x2 Platform
                    Block("truth.platform.large", 0f, 0.2f, 22.5f, 3.0f, 0.6f, 3.0f, ModuleMaterialRole.Normal),

                    // Station 5: Moving Platform Runup & Exit
                    Block("truth.moving.runup", 0f, 0.4f, 26.5f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("truth.moving.exit", 0f, 0.5f, 33.0f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),

                    // Station 7: Boost Landing
                    Block("truth.boost.landing", 0f, 3.2f, 54f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),

                    // Station 9: Surf Landing
                    Block("truth.surf.landing", 0f, 2.0f, 72f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal),

                    // Station 10: Patch Base Platform
                    Block("truth.patch.base", 0f, 2.0f, 78f, 3f, 0.6f, 3f, ModuleMaterialRole.Normal)
                },
                new[]
                {
                    // Station 5: Moving Platform
                    Moving(
                        "truth.moving.platform",
                        new Vector3(0f, 0.5f, 29.5f),
                        new Vector3(3.0f, 0.6f, 1.5f),
                        new[] { new Vector3(-3.0f, 0.5f, 29.5f), new Vector3(3.0f, 0.5f, 29.5f) },
                        2.5f,
                        0.4f)
                },
                new[]
                {
                    // Station 7: Jump Boost Pad
                    new ModuleBoostBlockDefinition(
                        "truth.boost.01",
                        new Vector3(0f, 0.8f, 44f),
                        new Vector3(1.5f, 0.6f, 1.5f),
                        new Vector3(0f, 0.75f, 1f),
                        10f,
                        8.5f,
                        0.35f)
                },
                null,
                new[]
                {
                    // Station 6: Crumbling Block
                    new ModuleCrumblingBlockDefinition(
                        "truth.crumble.01",
                        new Vector3(0f, 0.6f, 35.5f),
                        new Vector3(1.5f, 0.6f, 1.5f),
                        0.03f,
                        1f,
                        4f,
                        0.03f)
                },
                null,
                null);

            module.ConfigureSurfSurfaces(new[]
            {
                // Station 8: Surf Ramp
                new ModuleSurfSurfaceDefinition(
                    "surf.truth.ramp",
                    new Vector3(-4f, 4.5f, 63f),
                    new Vector3(4f, 0.45f, 16f),
                    new Vector3(0f, 0f, 1f),
                    new Vector3(0f, -25f, 42f))
            });

            module.ConfigureRankThresholds(Thresholds(60f, 45f, 35f, "Platform Truth Lab regression thresholds."));
            module.ConfigureCompletionRewards(BronzeRewards("truth-lab", 50, "loot.package.truth", "skin.ryder.truth"));
        }

        private static void ConfigureGameplayRoleGallery(
            ModuleDefinition module,
            ModuleVisualProfile visual,
            ModuleEnvironmentProfile environment)
        {
            module.name = "Module_GameplayRoleGallery";
            module.Configure(
                "module.gameplay-role-gallery",
                "Gameplay Role Gallery (Visual Identity & Collision Truth)",
                "module_gameplay_role_gallery",
                "project.ryders-block",
                "world.prototype-sky",
                100,
                ModuleRunnerScenePath,
                ModuleDifficulty.Intro,
                new ModulePose(new Vector3(0f, 0.35f, 0f), Vector3.zero),
                new ModulePatchBlockDefinition(
                    "patch.gallery",
                    new Vector3(0f, 1.8f, 62f),
                    new Vector3(2.2f, 2.2f, 2.2f)),
                new[]
                {
                    new ModuleRestorePointDefinition(
                        "restore.gallery.midpoint",
                        0,
                        new Vector3(0f, 0.8f, 32f),
                        new Vector3(0f, 0.35f, 0f))
                },
                60f,
                120f,
                new[]
                {
                    ModuleMechanic.StandardBlock,
                    ModuleMechanic.PrecisionBlock,
                    ModuleMechanic.MovingBlock,
                    ModuleMechanic.CrumblingBlock,
                    ModuleMechanic.JumpBoost,
                    ModuleMechanic.SurfSurface,
                    ModuleMechanic.RestorePoint,
                    ModuleMechanic.PatchBlock
                },
                null,
                environment,
                visual,
                "Gameplay visual role gallery displaying 1 canonical instance of every gameplay & environment role.",
                "Verify standard, long, large safe, moving, crumble, restore, boost, surf, patch, and world props.",
                new[]
                {
                    // Station 1: STANDARD (1.5m block)
                    Block("gallery.role.standard", 0f, 0f, 0f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),

                    // Station 2: LONG PLATFORM (3m platform)
                    Block("gallery.role.long", 0f, 0f, 5.0f, 3.0f, 0.6f, 1.5f, ModuleMaterialRole.Normal),

                    // Station 3: SAFE HUB (2x2 Modular 3m hub)
                    Block("gallery.role.safe", 0f, 0f, 11.0f, 3.0f, 0.6f, 3.0f, ModuleMaterialRole.Normal),

                    // Station 4: MOVING RUNUP & EXIT
                    Block("gallery.role.moving.runup", 0f, 0f, 16.0f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),
                    Block("gallery.role.moving.exit", 0f, 0f, 22.0f, 1.5f, 0.6f, 1.5f, ModuleMaterialRole.Normal),

                    // Station 7: BOOST RUNUP & LANDING
                    Block("gallery.role.boost.landing", 0f, 2.5f, 44.0f, 3.0f, 0.6f, 3.0f, ModuleMaterialRole.Normal),

                    // Station 8: SURF LANDING
                    Block("gallery.role.surf.landing", 0f, 1.5f, 56.0f, 3.0f, 0.6f, 3.0f, ModuleMaterialRole.Normal),

                    // Station 9: PATCH PLATFORM
                    Block("gallery.role.patch.base", 0f, 1.0f, 62.0f, 3.0f, 0.6f, 3.0f, ModuleMaterialRole.Normal),

                    // Environment Props
                    Block("gallery.env.tree", 10f, 0f, 5f, 4f, 4f, 4f, ModuleMaterialRole.Vegetation),
                    Block("gallery.env.arch", 10f, 0f, 15f, 5f, 5f, 5f, ModuleMaterialRole.ShortcutCue),
                    Block("gallery.env.ruin", 10f, 0f, 25f, 6.5f, 6.5f, 6.5f, ModuleMaterialRole.TowerCore),
                    Block("gallery.env.pillar", 10f, 0f, 35f, 8f, 8f, 8f, ModuleMaterialRole.TowerCore),
                    Block("gallery.env.energy", 10f, 0f, 45f, 4.5f, 4.5f, 4.5f, ModuleMaterialRole.CircuitLine),
                    Block("gallery.env.rock", -10f, 0f, 15f, 4.5f, 4.5f, 4.5f, ModuleMaterialRole.DecorationStone),
                    Block("gallery.env.flower", -10f, 0f, 25f, 2.5f, 2.5f, 2.5f, ModuleMaterialRole.Flower)
                },
                new[]
                {
                    // Station 4: MOVING PLATFORM
                    Moving(
                        "gallery.role.moving.platform",
                        new Vector3(0f, 0f, 19.0f),
                        new Vector3(3.0f, 0.6f, 1.5f),
                        new[] { new Vector3(-2.5f, 0f, 19.0f), new Vector3(2.5f, 0f, 19.0f) },
                        2.0f,
                        0.4f)
                },
                new[]
                {
                    // Station 6: BOOST PAD
                    new ModuleBoostBlockDefinition(
                        "gallery.role.boost.pad",
                        new Vector3(0f, 0.5f, 37.0f),
                        new Vector3(1.8f, 0.6f, 1.8f),
                        new Vector3(0f, 0.75f, 1f),
                        9.5f,
                        7.5f,
                        0.35f)
                },
                null,
                new[]
                {
                    // Station 5: CRUMBLING BLOCK
                    new ModuleCrumblingBlockDefinition(
                        "gallery.role.crumble.block",
                        new Vector3(0f, 0.2f, 26.5f),
                        new Vector3(1.4f, 0.6f, 1.4f),
                        0.03f,
                        1f,
                        4f,
                        0.03f)
                },
                null,
                null);

            module.ConfigureSurfSurfaces(new[]
            {
                // Station 8: SURF RAMP
                new ModuleSurfSurfaceDefinition(
                    "gallery.role.surf.ramp",
                    new Vector3(-3.5f, 3.8f, 50.0f),
                    new Vector3(4f, 0.45f, 12f),
                    new Vector3(0f, 0f, 1f),
                    new Vector3(0f, -22f, 38f))
            });

            module.ConfigureRankThresholds(Thresholds(60f, 45f, 35f, "Gameplay Role Gallery thresholds."));
            module.ConfigureCompletionRewards(BronzeRewards("role-gallery", 50, "loot.package.gallery", "skin.ryder.gallery"));
        }

        private static ModuleCompletionRewardDefinition[] BronzeRewards(
            string moduleToken,
            int patchShards,
            string lootPackageId,
            string skinId)
        {
            return new[]
            {
                new ModuleCompletionRewardDefinition(
                    $"reward.{moduleToken}.bronze.shards",
                    ModuleRewardKind.Currency,
                    "currency.patch-shards",
                    patchShards),
                new ModuleCompletionRewardDefinition(
                    $"reward.{moduleToken}.bronze.package",
                    ModuleRewardKind.InventoryItem,
                    lootPackageId,
                    1),
                new ModuleCompletionRewardDefinition(
                    $"reward.{moduleToken}.bronze.skin",
                    ModuleRewardKind.Unlock,
                    skinId,
                    1,
                    ModuleRank.Bronze,
                    true,
                    "Skin")
            };
        }

        private static ModuleRankThresholds Thresholds(
            float silver,
            float gold,
            float diamond,
            string notes)
        {
            return new ModuleRankThresholds(
                silver,
                gold,
                diamond,
                RankCalibrationState.Uncalibrated,
                1,
                "UNCALIBRATED - " + notes);
        }

        private static ModuleBlockDefinition Block(
            string id,
            float x,
            float y,
            float z,
            float sx,
            float sy,
            float sz,
            ModuleMaterialRole role)
        {
            return new ModuleBlockDefinition(
                id,
                new Vector3(x, y, z),
                new Vector3(sx, sy, sz),
                role,
                id,
                editorLabel: true);
        }

        private static ModuleMovingBlockDefinition Moving(
            string id,
            Vector3 position,
            Vector3 size,
            Vector3[] points,
            float speed,
            float pause)
        {
            return new ModuleMovingBlockDefinition(
                id,
                position,
                size,
                points,
                speed,
                pause,
                ModulePathSpace.World,
                ModuleMovingLoopMode.PingPong,
                ModuleEasing.SmoothStep);
        }
    }
}
