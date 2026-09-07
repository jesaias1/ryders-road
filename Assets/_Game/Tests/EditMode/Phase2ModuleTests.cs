using System.Linq;
using Avoidance.Gameplay.Blocks;
using Avoidance.Gameplay.Camera;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Respawn;
using Avoidance.Gameplay.Visuals;
using Avoidance.Gameplay.Worlds;
using Avoidance.Input;
using Avoidance.SaveSystem;
using Avoidance.UI;
using NUnit.Framework;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace Avoidance.Tests.EditMode
{
    public sealed class Phase2ModuleTests
    {
        [Test]
        public void ModuleValidator_AcceptsValidModuleDefinition()
        {
            var module = CreateValidModule("module.test.valid");

            Assert.That(ModuleDefinitionValidator.Validate(module), Is.Empty);

            Object.DestroyImmediate(module.VisualProfile);
            Object.DestroyImmediate(module.EnvironmentProfile);
            Object.DestroyImmediate(module);
        }

        [Test]
        public void ModuleValidator_RejectsDuplicateModuleIds()
        {
            var first = CreateValidModule("module.test.duplicate");
            var second = CreateValidModule("module.test.duplicate");

            var errors = ModuleDefinitionValidator.ValidateCatalog(new[] { first, second });

            Assert.That(errors.Any(error => error.Contains("Duplicate module stable ID")), Is.True);

            Object.DestroyImmediate(first.VisualProfile);
            Object.DestroyImmediate(first.EnvironmentProfile);
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second.VisualProfile);
            Object.DestroyImmediate(second.EnvironmentProfile);
            Object.DestroyImmediate(second);
        }

        [Test]
        public void ModuleValidator_RejectsInvalidStableIdsAndMissingPatchBlock()
        {
            var module = ScriptableObject.CreateInstance<ModuleDefinition>();
            module.Configure(
                "module_bad",
                "Broken",
                "broken",
                "project.ryders-block",
                "world.prototype-sky",
                1,
                ModuleSelectionState.ModuleRunnerSceneName,
                ModuleDifficulty.Intro,
                new ModulePose(Vector3.zero, Vector3.zero),
                null,
                null,
                10f,
                20f,
                null,
                null,
                ModuleEnvironmentProfile.CreateRuntimeDefault(),
                ModuleVisualProfile.CreateRuntimeDefault(),
                string.Empty,
                string.Empty,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            var errors = ModuleDefinitionValidator.Validate(module);

            Assert.That(errors.Any(error => error.Contains("Module stable ID is invalid")), Is.True);
            Assert.That(errors.Any(error => error.Contains("missing a Patch Block")), Is.True);

            Object.DestroyImmediate(module.VisualProfile);
            Object.DestroyImmediate(module.EnvironmentProfile);
            Object.DestroyImmediate(module);
        }

        [Test]
        public void MovingBlockMath_PingPongsAlongConfiguredPath()
        {
            var points = new[] { Vector3.zero, Vector3.forward * 10f };

            Assert.That(
                MovingBlockMotionMath.Evaluate(
                    points,
                    ModuleMovingLoopMode.PingPong,
                    ModuleEasing.Linear,
                    2f,
                    2.5f),
                Is.EqualTo(Vector3.forward * 5f));
            Assert.That(
                MovingBlockMotionMath.Evaluate(
                    points,
                    ModuleMovingLoopMode.PingPong,
                    ModuleEasing.Linear,
                    2f,
                    6f),
                Is.EqualTo(Vector3.forward * 8f));
        }

        [Test]
        public void JumpBoostMath_UsesConfiguredVerticalAndHorizontalStrengths()
        {
            var launch = JumpBoostMath.ResolveLaunchVelocity(
                new Vector3(1f, 0.4f, 1f),
                12f,
                7f);

            Assert.That(launch.y, Is.EqualTo(12f));
            Assert.That(new Vector3(launch.x, 0f, launch.z).magnitude, Is.EqualTo(7f).Within(0.001f));
        }

        [Test]
        public void WaterFlowMath_CapsAddedVelocity()
        {
            var delta = WaterFlowMath.ComputeHorizontalDelta(
                Vector3.forward * 4.8f,
                Vector3.forward,
                12f,
                5f,
                1f);

            Assert.That(delta.z, Is.EqualTo(0.2f).Within(0.001f));
        }

        [Test]
        public void CrumblingState_AdvancesThreeStagesThenDropsAndResets()
        {
            var state = new CrumblingBlockState(0f, 1f);

            state.Activate();
            Assert.That(state.Phase, Is.EqualTo(CrumblingBlockPhase.Stage1));
            state.Tick(0.32f);
            state.Activate();
            Assert.That(state.Phase, Is.EqualTo(CrumblingBlockPhase.Stage1));
            state.Tick(0.02f);
            Assert.That(state.Phase, Is.EqualTo(CrumblingBlockPhase.Stage2));
            state.Tick(0.34f);
            Assert.That(state.Phase, Is.EqualTo(CrumblingBlockPhase.Stage3));
            state.Tick(0.33f);
            Assert.That(state.Phase, Is.EqualTo(CrumblingBlockPhase.Gone));

            state.Reset();
            Assert.That(state.Phase, Is.EqualTo(CrumblingBlockPhase.Stable));
        }

        [Test]
        public void CrumblingBlock_ShakesVisualOnlyAndRestoresDeterministically()
        {
            var blockObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blockObject.transform.localScale = new Vector3(2.6f, 0.6f, 2.6f);
            var visualRoot = new GameObject("Visual Root");
            visualRoot.transform.SetParent(blockObject.transform, false);
            var stages = Enumerable.Range(1, 3)
                .Select(index => new GameObject("Stage " + index))
                .ToArray();
            foreach (var stage in stages)
            {
                stage.transform.SetParent(visualRoot.transform, false);
            }

            var playerObject = new GameObject("Player", typeof(CharacterController), typeof(ParkourMotor));
            var motor = playerObject.GetComponent<ParkourMotor>();
            motor.Initialize(MovementProfile.CreateRuntimeDefault());
            var crumble = blockObject.AddComponent<CrumblingBlock>();
            crumble.Initialize(0f, 1f, 4f, 0.025f, visualRoot.transform, stages[0], stages[1], stages[2], null, null);
            var blockPosition = blockObject.transform.position;

            crumble.Activate(motor);
            crumble.Tick(0.34f);
            Assert.That(crumble.Phase, Is.EqualTo(CrumblingBlockPhase.Stage2));
            Assert.That(blockObject.transform.position, Is.EqualTo(blockPosition));
            Assert.That(crumble.SolidColliderEnabled, Is.True);
            Assert.That(visualRoot.transform.localPosition, Is.Not.EqualTo(Vector3.zero));
            Assert.That(visualRoot.transform.position.magnitude, Is.LessThanOrEqualTo(0.04f));

            crumble.Tick(0.66f);
            Assert.That(crumble.Phase, Is.EqualTo(CrumblingBlockPhase.Gone));
            Assert.That(crumble.SolidColliderEnabled, Is.False);
            crumble.Tick(4f);
            Assert.That(crumble.Phase, Is.EqualTo(CrumblingBlockPhase.Stable));
            Assert.That(crumble.SolidColliderEnabled, Is.True);
            Assert.That(stages[0].activeSelf, Is.True);
            Assert.That(stages[1].activeSelf, Is.False);
            Assert.That(stages[2].activeSelf, Is.False);

            Object.DestroyImmediate(blockObject);
            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void CrumblingContactRelay_ActivatesOnlyFromWalkableSupportContact()
        {
            var blockObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var crumble = blockObject.AddComponent<CrumblingBlock>();
            crumble.Initialize(0f, 1f, 4f, 0.025f, null, null);
            var playerObject = new GameObject(
                "Player",
                typeof(CharacterController),
                typeof(ParkourMotor),
                typeof(CrumblingBlockContactRelay));
            var motor = playerObject.GetComponent<ParkourMotor>();
            motor.Initialize(MovementProfile.CreateRuntimeDefault());
            var relay = playerObject.GetComponent<CrumblingBlockContactRelay>();
            relay.Initialize(motor);

            relay.NotifyLanding(blockObject.GetComponent<Collider>(), Vector3.right);
            Assert.That(crumble.Phase, Is.EqualTo(CrumblingBlockPhase.Stable));
            relay.NotifyLanding(blockObject.GetComponent<Collider>(), Vector3.up);
            Assert.That(crumble.Phase, Is.EqualTo(CrumblingBlockPhase.Stage1));

            Object.DestroyImmediate(blockObject);
            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void PatchBlock_CompletesOnlyOnce()
        {
            var patchObject = new GameObject("Patch", typeof(BoxCollider), typeof(PatchBlock));
            var playerObject = new GameObject("Player", typeof(CharacterController), typeof(ParkourMotor));
            var motor = playerObject.GetComponent<ParkourMotor>();
            motor.Initialize(MovementProfile.CreateRuntimeDefault());
            var patch = patchObject.GetComponent<PatchBlock>();
            patch.Initialize(null, null, null);

            Assert.That(patch.TryComplete(motor), Is.True);
            Assert.That(patch.TryComplete(motor), Is.False);

            Object.DestroyImmediate(patchObject);
            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void ParkourMotor_ExternalLaunchDoesNotMutateMovementProfile()
        {
            var playerObject = new GameObject("Player", typeof(CharacterController), typeof(ParkourMotor));
            var motor = playerObject.GetComponent<ParkourMotor>();
            var profile = MovementProfile.CreateRuntimeDefault();
            motor.Initialize(profile);

            motor.ApplyLaunch(new Vector3(2f, 11f, 0f), replaceHorizontal: true);

            Assert.That(motor.VerticalSpeed, Is.EqualTo(11f));
            Assert.That(motor.Profile, Is.SameAs(profile));

            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(playerObject);
        }

        [Test]
        public void DefaultTouchCamera_DoesNotAutoSteerWithoutManualLookDelta()
        {
            var player = new GameObject("Camera Test Player");
            var cameraObject = new GameObject("Camera", typeof(UnityEngine.Camera), typeof(FirstPersonCameraRig));
            cameraObject.transform.SetParent(player.transform, false);
            var rig = cameraObject.GetComponent<FirstPersonCameraRig>();
            rig.Initialize(
                player.transform,
                cameraObject.GetComponent<UnityEngine.Camera>(),
                CameraProfile.CreateRuntimeDefault());

            rig.ApplyLook(
                new FakeInput { MoveValue = Vector2.right },
                PlayerInputMode.Touch,
                1f);

            Assert.That(Mathf.DeltaAngle(rig.Yaw, 0f), Is.EqualTo(0f).Within(0.01f));

            Object.DestroyImmediate(player);
        }

        [Test]
        public void Phase2ModuleAssets_ArePresentAndValid()
        {
            var modules = Resources.LoadAll<ModuleDefinition>("Modules");

            Assert.That(modules.Select(module => module.StableModuleId), Does.Contain("module.001.first-steps"));
            Assert.That(modules.Select(module => module.StableModuleId), Does.Contain("module.002.moving-parts"));
            Assert.That(modules.Select(module => module.StableModuleId), Does.Contain("module.003.flow-error"));
            Assert.That(modules.Select(module => module.StableModuleId), Does.Contain("module.004.the-spiral"));
            Assert.That(modules.All(module => module.RankThresholds != null && module.RankThresholds.IsOrdered), Is.True);
            Assert.That(
                modules.All(module => module.RankThresholds.CalibrationState
                    == Avoidance.Gameplay.Ranking.RankCalibrationState.Uncalibrated),
                Is.True);
            Assert.That(ModuleDefinitionValidator.ValidateCatalog(modules), Is.Empty);
        }

        [Test]
        public void Phase076VisualSlice_UsesRouteSupportRolesAndGroupedVegetation()
        {
            var modules = Resources.LoadAll<ModuleDefinition>("Modules");
            var flowError = modules.First(module => module.StableModuleId == "module.003.flow-error");
            var decorationRoles = flowError.Decorations
                .Select(decoration => decoration.VisualRole)
                .ToArray();

            Assert.That(decorationRoles, Is.Empty);

            var arrivalSanctuary = flowError.EnvironmentBiomeProfile.WorldObjects.Single(
                item => item.StableId == "biome.ancient-abyss.place.arrival-sanctuary");
            var materialNames = arrivalSanctuary.Prefab
                .GetComponentsInChildren<Renderer>(true)
                .Select(renderer => renderer.sharedMaterial != null
                    ? renderer.sharedMaterial.name
                    : string.Empty)
                .ToArray();
            Assert.That(materialNames, Does.Contain("MAT_RR_World_Subtle_Vegetation"));
        }

        [Test]
        public void Phase073CampaignBiomes_AreAssignedAndUsePreparedWorldAssets()
        {
            var modules = Resources.LoadAll<ModuleDefinition>("Modules")
                .ToDictionary(module => module.StableModuleId);

            Assert.That(
                modules["module.001.first-steps"].EnvironmentBiomeProfile.Kind,
                Is.EqualTo(EnvironmentBiomeKind.SkyCity));
            Assert.That(
                modules["module.002.moving-parts"].EnvironmentBiomeProfile.Kind,
                Is.EqualTo(EnvironmentBiomeKind.MountainSky));
            Assert.That(
                modules["module.003.flow-error"].EnvironmentBiomeProfile.Kind,
                Is.EqualTo(EnvironmentBiomeKind.AncientAbyss));
            Assert.That(
                modules["module.004.the-spiral"].EnvironmentBiomeProfile,
                Is.Null);

            foreach (var moduleId in ModuleSelectionState.GetCampaignModuleIds())
            {
                var biome = modules[moduleId].EnvironmentBiomeProfile;
                Assert.That(biome, Is.Not.Null, moduleId);
                Assert.That(biome.WorldObjects.Count, Is.GreaterThanOrEqualTo(3), moduleId);
                Assert.That(
                    (biome.Kind == EnvironmentBiomeKind.AncientAbyss || modules[moduleId].EnvironmentProfile.SkyboxMaterial != null)
                        ? biome.MistLayers.Count == 0
                        : biome.MistLayers.Count >= 3,
                    Is.True,
                    moduleId);
                Assert.That(
                    biome.WorldObjects.All(worldObject =>
                        worldObject.Prefab != null
                        && (worldObject.HasPlayableArchitecture || worldObject.Prefab.GetComponentsInChildren<Collider>(true).Length == 0)),
                    Is.True,
                    moduleId);
            }
        }

        [Test]
        public void Phase04VisualIdentity_ModuleThreeHasCleanMapDressing()
        {
            var flowError = Resources.LoadAll<ModuleDefinition>("Modules")
                .First(module => module.StableModuleId == "module.003.flow-error");
            var decorationIds = flowError.Decorations
                .Select(decoration => decoration.StableId)
                .ToArray();

            Assert.That(decorationIds, Is.Empty);
            Assert.That(flowError.EnvironmentBiomeProfile.WorldObjects.Count, Is.EqualTo(9));
        }

        [Test]
        public void Phase080AncientAbyss_HasCuratedWorldEnvelopeWithoutPlanarMistFloor()
        {
            var flowError = Resources.LoadAll<ModuleDefinition>("Modules")
                .First(module => module.StableModuleId == "module.003.flow-error");
            var biome = flowError.EnvironmentBiomeProfile;

            Assert.That(biome, Is.Not.Null);
            Assert.That(biome.Kind, Is.EqualTo(EnvironmentBiomeKind.AncientAbyss));
            Assert.That(biome.MistOceanY, Is.EqualTo(-72f));
            Assert.That(biome.MistLayers, Is.Empty);
            Assert.That(biome.WorldObjects.Count, Is.EqualTo(9));

            var objectIds = biome.WorldObjects.Select(obj => obj.StableId).ToArray();
            Assert.That(objectIds, Does.Contain("biome.ancient-abyss.place.arrival-sanctuary"));
            Assert.That(objectIds, Does.Contain("biome.ancient-abyss.place.broken-crossing"));
            Assert.That(objectIds, Does.Contain("biome.ancient-abyss.place.collapsed-temple"));
            Assert.That(objectIds, Does.Contain("biome.ancient-abyss.place.energy-spine"));
            Assert.That(objectIds, Does.Contain("biome.ancient-abyss.place.patch-sanctum"));
            Assert.That(objectIds, Does.Contain("biome.ancient-abyss.hero.shattered-crown"));
            Assert.That(objectIds, Does.Contain("biome.ancient-abyss.mid.broken-procession"));
            Assert.That(objectIds, Does.Contain("biome.ancient-abyss.mid.celestial-arch"));
            Assert.That(objectIds, Does.Contain("biome.ancient-abyss.below.sunken-city"));
            Assert.That(objectIds, Does.Not.Contain("biome.ancient-abyss.near.pilgrim-causeway"));
            Assert.That(objectIds, Does.Not.Contain("biome.ancient-abyss.mid.reclaimed-sanctuary"));
            Assert.That(objectIds, Does.Not.Contain("biome.ancient-abyss.colossal.abyss-tower"));
            Assert.That(objectIds, Does.Not.Contain("biome.ancient-abyss.group.ruin-island"));

            foreach (var worldObject in biome.WorldObjects)
            {
                Assert.That(worldObject.Prefab, Is.Not.Null, worldObject.StableId);
                if (!worldObject.HasPlayableArchitecture) Assert.That(worldObject.Prefab.GetComponentsInChildren<Collider>(true).Length, Is.EqualTo(0), $"{worldObject.StableId} scenery must be collider-free");
            }

            Assert.That(flowError.DisableAutomaticDistantFragments, Is.True);

            var placeObjects = biome.WorldObjects
                .Where(obj => obj.StableId.StartsWith("biome.ancient-abyss.place.", System.StringComparison.Ordinal))
                .ToArray();
            Assert.That(placeObjects.Length, Is.EqualTo(5));
            Assert.That(placeObjects.Sum(obj => obj.Prefab.GetComponentsInChildren<Renderer>(true).Length),
                Is.LessThanOrEqualTo(45), "Five authored places must remain inside the mobile renderer budget.");
            Assert.That(placeObjects.SelectMany(obj => obj.Prefab.GetComponentsInChildren<Renderer>(true))
                .All(renderer => renderer.sharedMaterial == null
                    || !renderer.sharedMaterial.name.Contains("Ancient_Sandstone", System.StringComparison.Ordinal)),
                Is.True, "Raw brown sandstone is forbidden in the five near authored places.");
        }

        [Test]
        public void Phase076AncientAbyss_HasGroupedDepthCompositionAndHeroLandmarkFocus()
        {
            var flowError = Resources.LoadAll<ModuleDefinition>("Modules")
                .First(module => module.StableModuleId == "module.003.flow-error");
            var biome = flowError.EnvironmentBiomeProfile;

            Assert.That(biome, Is.Not.Null);

            var heroLandmark = biome.WorldObjects.Single(obj => obj.StableId == "biome.ancient-abyss.hero.shattered-crown");
            Assert.That(heroLandmark.Position.x, Is.GreaterThan(25f));
            Assert.That(heroLandmark.Position.y, Is.LessThanOrEqualTo(-20f));
            Assert.That(heroLandmark.Position.z, Is.GreaterThanOrEqualTo(170f));
            Assert.That(heroLandmark.Scale.x, Is.GreaterThanOrEqualTo(1.3f));
            Assert.That(heroLandmark.DepthBand, Is.EqualTo(BiomeDepthBand.FarWorld));

            var sunkenCity = biome.WorldObjects.Single(obj => obj.StableId == "biome.ancient-abyss.below.sunken-city");
            Assert.That(sunkenCity.Position.y, Is.LessThanOrEqualTo(-80f));
            Assert.That(sunkenCity.DepthBand, Is.EqualTo(BiomeDepthBand.LowerAbyss));

            Assert.That(biome.WorldObjects.Count, Is.EqualTo(9));
            Assert.That(biome.WorldObjects.Count(obj => obj.DepthBand == BiomeDepthBand.NearEnvironment), Is.EqualTo(5));
            Assert.That(biome.WorldObjects.Count(obj => obj.DepthBand == BiomeDepthBand.MidWorld), Is.EqualTo(2));
            Assert.That(biome.WorldObjects.Count(obj => obj.DepthBand == BiomeDepthBand.FarWorld), Is.EqualTo(1));
            Assert.That(biome.WorldObjects.Count(obj => obj.DepthBand == BiomeDepthBand.LowerAbyss), Is.EqualTo(1));
            Assert.That(biome.MistLayers, Is.Empty);
            Assert.That(biome.MistColor.b - biome.MistColor.r, Is.GreaterThan(0.2f));
            Assert.That(flowError.Decorations, Is.Empty);
        }

        [Test]
        public void Phase058Spiral_HasExplicitSafeStartConnectedLandmarksAndOptionalMasteryLines()
        {
            var spiral = Resources.LoadAll<ModuleDefinition>("Modules")
                .First(module => module.StableModuleId == "module.004.the-spiral");

            Assert.That(spiral.DisplayName, Is.EqualTo("THE SPIRAL"));
            Assert.That(spiral.Difficulty, Is.EqualTo(ModuleDifficulty.Easy));
            Assert.That(spiral.ContentVersion, Is.EqualTo(2));
            Assert.That(spiral.ExpectedCleanTime, Is.EqualTo(150f));
            Assert.That(spiral.EstimatedCasualTime, Is.EqualTo(300f));
            Assert.That(spiral.StartAnchorStableId, Is.EqualTo("SpiralStartAnchor"));
            Assert.That(spiral.StartSupportBlockStableId, Is.EqualTo("m04.start"));
            Assert.That(spiral.ResolveFallThreshold(-26f), Is.EqualTo(-7.5f));
            Assert.That(spiral.DisableAutomaticDistantFragments, Is.True);
            Assert.That(spiral.RestorePoints.Count, Is.EqualTo(5));
            Assert.That(spiral.Blocks.Count, Is.GreaterThanOrEqualTo(35));
            Assert.That(spiral.Decorations.Count, Is.GreaterThanOrEqualTo(24));
            Assert.That(spiral.OptionalShortcuts.Count, Is.EqualTo(5));
            Assert.That(spiral.SurfSurfaces, Is.Empty);
            Assert.That(spiral.MechanicsUsed, Does.Contain(ModuleMechanic.JumpBoost));
            Assert.That(spiral.MechanicsUsed.Contains(ModuleMechanic.SurfSurface), Is.False);
            Assert.That(spiral.MechanicsUsed, Does.Contain(ModuleMechanic.CrumblingBlock));
            Assert.That(
                spiral.DeveloperNotes,
                Does.Contain("mandatory one-second crumble pressure chains"));
            Assert.That(spiral.Decorations.All(block => block.StableId.StartsWith("m04.world.")), Is.True);
        }

        [Test]
        public void Phase056Spiral_RhythmLanguageAndMechanicsStayBronzeFriendly()
        {
            var spiral = Resources.LoadAll<ModuleDefinition>("Modules")
                .First(module => module.StableModuleId == "module.004.the-spiral");
            var ids = spiral.Blocks.Select(block => block.StableId).ToArray();
            Assert.That(ids.Any(id => id.EndsWith(".setup")), Is.True);
            Assert.That(ids.Any(id => id.EndsWith(".easy")), Is.True);
            Assert.That(ids.Any(id => id.EndsWith(".normal")), Is.True);
            Assert.That(ids.Any(id => id.EndsWith(".commit")), Is.True);
            Assert.That(ids.Any(id => id.EndsWith(".flow")), Is.True);
            Assert.That(ids.Any(id => id.EndsWith(".run")), Is.True);
            Assert.That(ids.Any(id => id.EndsWith(".vertical")), Is.True);
            Assert.That(spiral.MovingBlocks.Count, Is.EqualTo(1));
            Assert.That(spiral.MovingBlocks[0].StableId, Is.EqualTo("moving.m04.required-crossing"));
            Assert.That(spiral.MovingBlocks[0].Speed, Is.InRange(2f, 2.2f));
            Assert.That(spiral.MovingBlocks[0].PauseAtEndpoints, Is.GreaterThanOrEqualTo(0.55f));
            Assert.That(spiral.WaterVolumes, Is.Empty);
            Assert.That(spiral.BoostBlocks.Count, Is.EqualTo(1));
            Assert.That(spiral.BoostBlocks[0].HorizontalStrength, Is.LessThanOrEqualTo(7.2f));
            Assert.That(spiral.CrumblingBlocks.Count, Is.EqualTo(19));
            Assert.That(spiral.CrumblingBlocks.Min(block => block.FallDelay), Is.EqualTo(1f).Within(0.001f));
            Assert.That(spiral.CrumblingBlocks.Min(block => block.Size.x), Is.GreaterThanOrEqualTo(2.4f));
            Assert.That(
                spiral.RankThresholds.DesignerNotes,
                Does.Contain("0.6.3 hand/collision/abyss polish pass"));
        }

        [Test]
        public void Phase056Spiral_BronzeSupportsHaveSafeVariedMeasuredSpacing()
        {
            var spiral = Resources.LoadAll<ModuleDefinition>("Modules")
                .First(module => module.StableModuleId == "module.004.the-spiral");
            var supports = OrderedSpiralSupports(spiral).ToArray();
            var gaps = AdjacentSupportGaps(supports).ToArray();
            Assert.That(supports.Length, Is.GreaterThanOrEqualTo(60));
            foreach (var row in gaps)
            {
                Assert.That(row.EdgeGap, Is.LessThanOrEqualTo(3.25f), row.Label);
                Assert.That(row.Rise, Is.LessThanOrEqualTo(0.86f), row.Label);
            }

            Assert.That(gaps.Min(gap => gap.EdgeGap), Is.LessThan(0.8f));
            Assert.That(gaps.Max(gap => gap.EdgeGap), Is.GreaterThan(1.8f));
            Assert.That(gaps.Count(gap => gap.EdgeGap > 0.5f), Is.GreaterThanOrEqualTo(40));
            Assert.That(spiral.PatchBlock.Pose.Position.y - spiral.StartPoint.Position.y, Is.GreaterThan(25f));
        }

        [Test]
        public void Phase050ModeSplit_CampaignOrderExcludesSpiralAndUnlocksLinearly()
        {
            var orderedIds = ModuleSelectionState.GetCampaignModuleIds();
            var modules = Resources.LoadAll<ModuleDefinition>("Modules")
                .ToDictionary(module => module.StableModuleId);
            var progression = new ProgressionData();

            Assert.That(orderedIds, Is.EqualTo(new[]
            {
                "module.001.first-steps",
                "module.002.moving-parts",
                "module.003.flow-error"
            }));
            Assert.That(orderedIds, Does.Not.Contain(ModuleSelectionState.SpiralModuleId));
            Assert.That(ModuleSelectionState.IsCampaignModule("module.001.first-steps"), Is.True);
            Assert.That(ModuleSelectionState.IsCampaignModule(ModuleSelectionState.SpiralModuleId), Is.False);
            Assert.That(ModuleSelectionState.IsSpiralModule(ModuleSelectionState.SpiralModuleId), Is.True);
            Assert.That(modules["module.001.first-steps"].DisplayName, Is.EqualTo("Campaign 01 - First Steps"));
            Assert.That(modules["module.002.moving-parts"].DisplayName, Is.EqualTo("Campaign 02 - Moving Parts"));
            Assert.That(modules["module.003.flow-error"].DisplayName, Is.EqualTo("Campaign 03 - Flow Error"));
            Assert.That(
                ModuleProgressionData.IsUnlocked(progression, orderedIds, "module.001.first-steps"),
                Is.True);
            Assert.That(
                ModuleProgressionData.IsUnlocked(progression, orderedIds, "module.002.moving-parts"),
                Is.False);

            ModuleProgressionData.GetOrCreateRecord(progression, "module.001.first-steps").completed = true;
            Assert.That(
                ModuleProgressionData.IsUnlocked(progression, orderedIds, "module.002.moving-parts"),
                Is.True);
            Assert.That(
                ModuleProgressionData.IsUnlocked(progression, orderedIds, "module.003.flow-error"),
                Is.False);

            ModuleProgressionData.GetOrCreateRecord(progression, "module.002.moving-parts").completed = true;
            Assert.That(
                ModuleProgressionData.IsUnlocked(progression, orderedIds, "module.003.flow-error"),
                Is.True);
            Assert.That(modules["module.001.first-steps"].ExpectedCleanTime, Is.LessThan(modules["module.002.moving-parts"].ExpectedCleanTime));
            Assert.That(modules["module.002.moving-parts"].ExpectedCleanTime, Is.LessThan(modules["module.003.flow-error"].ExpectedCleanTime));
        }

        [Test]
        public void Phase04VisualIdentity_ProfileUsesMobileBalancedQuality()
        {
            var environment = Resources.Load<ModuleEnvironmentProfile>(ModuleEnvironmentProfile.ResourceName)
                ?? ModuleEnvironmentProfile.CreateRuntimeDefault();
            var visual = Resources.Load<ModuleVisualProfile>(ModuleVisualProfile.ResourceName)
                ?? ModuleVisualProfile.CreateRuntimeDefault();

            Assert.That(environment.QualityProfile, Is.EqualTo(ModuleVisualQualityTier.MobileBalanced));
            Assert.That(environment.FogDensity, Is.GreaterThan(0f));
            Assert.That(environment.VfxDensity, Is.InRange(0f, 1f));
            Assert.That(visual.SurfaceInsetScale, Is.InRange(0.45f, 0.92f));
            Assert.That(visual.EdgeTrimScale, Is.InRange(0.035f, 0.12f));
            Assert.That(visual.HudPanelColor.a, Is.GreaterThan(0.1f));
        }

        [Test]
        public void Phase047VisualIdentity_ProfileKeepsRouteWarmReadableAndNullSpaceDeep()
        {
            var environment = Resources.Load<ModuleEnvironmentProfile>(ModuleEnvironmentProfile.ResourceName)
                ?? ModuleEnvironmentProfile.CreateRuntimeDefault();
            var visual = Resources.Load<ModuleVisualProfile>(ModuleVisualProfile.ResourceName)
                ?? ModuleVisualProfile.CreateRuntimeDefault();

            var normal = visual.ColorFor(ModuleMaterialRole.Normal);
            var warmAccent = visual.ColorFor(ModuleMaterialRole.WarmAccent);
            var nullSpace = visual.ColorFor(ModuleMaterialRole.NullSpace);
            var stoneSide = visual.ColorFor(ModuleMaterialRole.StoneSide);

            Assert.That(warmAccent.r, Is.GreaterThan(warmAccent.b + 0.4f));
            Assert.That(normal.r, Is.GreaterThanOrEqualTo(normal.b - 0.08f));
            Assert.That(nullSpace.maxColorComponent, Is.LessThan(0.09f));
            Assert.That(stoneSide.maxColorComponent, Is.LessThan(normal.maxColorComponent));
            Assert.That(environment.HorizonLayerOpacity, Is.GreaterThanOrEqualTo(0.8f));
        }

        [Test]
        public void Phase079RouteReadability_OrdinaryBlocksHaveNoGeneratedFlowHelpers()
        {
            var spiral = Resources.LoadAll<ModuleDefinition>("Modules")
                .First(module => module.StableModuleId == "module.004.the-spiral");
            var cueEligible = spiral.Blocks.Count(block =>
                ModuleSceneController.ShouldCreateRouteFlowCue(block.VisualRole, block.Size));

            Assert.That(cueEligible, Is.EqualTo(0));
            Assert.That(
                ModuleSceneController.ShouldCreateRouteFlowCue(
                    ModuleMaterialRole.Normal,
                    new Vector3(4f, 0.6f, 4f)),
                Is.False);
            Assert.That(
                ModuleSceneController.ShouldCreateRouteFlowCue(
                    ModuleMaterialRole.Precision,
                    new Vector3(2.4f, 0.6f, 2.4f)),
                Is.False);
            Assert.That(
                ModuleSceneController.ShouldCreateRouteFlowCue(
                    ModuleMaterialRole.Moving,
                    new Vector3(5f, 0.6f, 5f)),
                Is.False);
            Assert.That(
                ModuleSceneController.ShouldCreateRouteFlowCue(
                    ModuleMaterialRole.Boost,
                    new Vector3(5f, 0.6f, 5f)),
                Is.False);
            Assert.That(
                ModuleSceneController.ShouldCreateRouteFlowCue(
                    ModuleMaterialRole.Normal,
                    new Vector3(1.2f, 0.6f, 1.2f)),
                Is.False);
        }

        [Test]
        public void Phase04VisualIdentity_BenchmarkNamesAreStable()
        {
            Assert.That(
                ModuleSceneController.RequiredVisualBenchmarkNames,
                Does.Contain("Benchmark_Module01_Start"));
            Assert.That(
                ModuleSceneController.RequiredVisualBenchmarkNames,
                Does.Contain("Benchmark_Module02_Mid"));
            Assert.That(
                ModuleSceneController.RequiredVisualBenchmarkNames,
                Does.Contain("Benchmark_Module03_Opening"));
            Assert.That(
                ModuleSceneController.RequiredVisualBenchmarkNames,
                Does.Contain("Benchmark_Module03_Early"));
            Assert.That(
                ModuleSceneController.RequiredVisualBenchmarkNames,
                Does.Contain("Benchmark_Module03_Middle"));
            Assert.That(
                ModuleSceneController.RequiredVisualBenchmarkNames,
                Does.Contain("Benchmark_Module03_High"));
            Assert.That(
                ModuleSceneController.RequiredVisualBenchmarkNames,
                Does.Contain("Benchmark_Module03_Patch"));
            Assert.That(
                ModuleSceneController.RequiredVisualBenchmarkNames,
                Does.Contain("Benchmark_Spiral_Start"));
            Assert.That(
                ModuleSceneController.RequiredVisualBenchmarkNames,
                Does.Contain("Benchmark_Spiral_Mid"));
            Assert.That(
                ModuleSceneController.RequiredVisualBenchmarkNames,
                Does.Contain("Benchmark_Spiral_Summit"));
            Assert.That(
                ModuleSceneController.OptionalVisualBenchmarkNames,
                Does.Contain("Benchmark_Spiral_Low"));
            Assert.That(
                ModuleSceneController.OptionalVisualBenchmarkNames,
                Does.Contain("Benchmark_Spiral_Restore"));
            Assert.That(
                ModuleSceneController.OptionalVisualBenchmarkNames,
                Does.Contain("Benchmark_Spiral_Boost"));
            Assert.That(
                ModuleSceneController.OptionalVisualBenchmarkNames,
                Does.Contain("Benchmark_Spiral_Surf"));
            Assert.That(
                ModuleSceneController.OptionalVisualBenchmarkNames,
                Does.Contain("Benchmark_Spiral_High"));
        }

        [Test]
        public void ModuleVisualProfile_AllMaterialRolesResolveToUrpSafeMaterials()
        {
            var profile = Resources.Load<ModuleVisualProfile>(ModuleVisualProfile.ResourceName)
                ?? ModuleVisualProfile.CreateRuntimeDefault();

            foreach (ModuleMaterialRole role in System.Enum.GetValues(typeof(ModuleMaterialRole)))
            {
                Assert.That(
                    VisualMaterialUtility.IsInvalidGameplayShader(profile.MaterialFor(role).shader),
                    Is.False,
                    $"{role} should resolve to a URP-safe material.");
                Assert.That(
                    VisualMaterialUtility.IsLikelyErrorColor(profile.ColorFor(role)),
                    Is.False,
                    $"{role} should not use an error-magenta debug color.");
            }
        }

        [Test]
        public void Phase050Presentation_TextureAssetsAndModeSplitAreAvailable()
        {
            Assert.That(ModuleSelectionState.SpiralModuleId, Is.EqualTo("module.004.the-spiral"));
            Assert.That(
                Resources.Load<Texture2D>("Textures/RydersRoad_SkyPanorama_01"),
                Is.Not.Null);
            Assert.That(
                Resources.Load<Texture2D>("Textures/RydersRoad_RouteStone_01"),
                Is.Not.Null);
            Assert.That(
                Resources.Load<Texture2D>("Textures/RydersRoad_RouteStone_02"),
                Is.Not.Null);
            Assert.That(
                Resources.Load<Texture2D>("Textures/RydersRoad_GoldRoute_01"),
                Is.Not.Null);
            Assert.That(
                Resources.Load<Texture2D>("Textures/RydersRoad_CyanEnergy_01"),
                Is.Not.Null);
            Assert.That(
                Resources.Load<ModuleVisualPrefabLibrary>(ModuleVisualPrefabLibrary.ResourceName),
                Is.Not.Null);
        }

        [Test]
        public void Phase051VisualPrefabLibrary_DefaultsEmptyAndKeepsPrefabVisualOnly()
        {
            var library = ModuleVisualPrefabLibrary.CreateRuntimeDefault();
            var prefab = new GameObject("Visual Test Prefab", typeof(BoxCollider), typeof(MeshRenderer));

            Assert.That(library.HasPrefabFor(ModuleMaterialRole.Normal), Is.False);
            ModuleVisualPrefabLibrary.PrepareVisualInstance(prefab);
            Assert.That(prefab.GetComponent<Collider>(), Is.Null);
            Assert.That(
                prefab.GetComponent<Renderer>().motionVectorGenerationMode,
                Is.EqualTo(MotionVectorGenerationMode.ForceNoMotion));

            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(library);
        }

        [Test]
        public void Phase050MeshyArtKit_EnvironmentPrefabsResolveAndStayVisualOnly()
        {
            foreach (var prefabPath in MeshyEnvironmentPrefabPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                Assert.That(prefab, Is.Not.Null, prefabPath);
                Assert.That(prefab.GetComponentsInChildren<Renderer>(includeInactive: true).Length, Is.GreaterThan(0), prefabPath);
                Assert.That(prefab.GetComponentsInChildren<Collider>(includeInactive: true), Is.Empty, prefabPath);

                foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(includeInactive: true))
                {
                    Assert.That(
                        renderer.motionVectorGenerationMode,
                        Is.EqualTo(MotionVectorGenerationMode.ForceNoMotion),
                        prefabPath);
                    foreach (var material in renderer.sharedMaterials.Where(material => material != null))
                    {
                        Assert.That(
                            VisualMaterialUtility.IsInvalidGameplayShader(material.shader),
                            Is.False,
                            prefabPath);
                    }
                }
            }
        }

        [Test]
        public void Phase050MeshyArtKit_PrefabLibraryMapsRolesAndLongNormalBlocks()
        {
            var library = Resources.Load<ModuleVisualPrefabLibrary>(ModuleVisualPrefabLibrary.ResourceName);

            Assert.That(library, Is.Not.Null);
            foreach (var role in MeshyMappedRoles)
            {
                Assert.That(library.PlacementFor(role, new Vector3(4f, 1f, 4f)).IsValid, Is.True, role.ToString());
            }

            var smallNormal = library.PlacementFor(ModuleMaterialRole.Normal, new Vector3(4f, 1f, 4f));
            var longNormal = library.PlacementFor(ModuleMaterialRole.Normal, new Vector3(8f, 1f, 4f));
            Assert.That(smallNormal.Prefab.name, Does.Contain("RR_Block_Safe"));
            Assert.That(longNormal.Prefab.name, Does.Contain("RR_Platform_Long"));
        }

        [Test]
        public void Phase050MeshyArtKit_FirstPersonOptimizedArmsAreIntegratedAndVisualOnly()
        {
            var profile = Resources.Load<FirstPersonArmProfile>(FirstPersonArmProfile.ResourceName);
            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.HasLegacyApprovedFallback, Is.True);
            Assert.That(profile.HasActiveArmPrefabs, Is.True);
            Assert.That(profile.UseRiggedArms, Is.True);
            Assert.That(AssetDatabase.GetAssetPath(profile.LegacyRightArmPrefab), Is.EqualTo(MeshyLegacyRightArmPrefabPath));
            Assert.That(AssetDatabase.GetAssetPath(profile.LegacyLeftArmPrefab), Is.EqualTo(MeshyLegacyLeftArmPrefabPath));
            Assert.That(AssetDatabase.GetAssetPath(profile.RiggedRightArmPrefab), Is.EqualTo(MeshyRiggedRightArmPrefabPath));
            Assert.That(AssetDatabase.GetAssetPath(profile.RiggedLeftArmPrefab), Is.EqualTo(MeshyRiggedLeftArmPrefabPath));

            foreach (var runtimePath in MeshyArmRuntimePaths)
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(runtimePath);
                Assert.That(asset, Is.Not.Null, runtimePath);
            }

            AssertArmPrefabIsVisualOnly(profile.LegacyRightArmPrefab);
            AssertArmPrefabIsVisualOnly(profile.LegacyLeftArmPrefab);
            AssertArmPrefabIsVisualOnly(profile.RiggedRightArmPrefab);
            AssertArmPrefabIsVisualOnly(profile.RiggedLeftArmPrefab);
            Assert.That(TriangleCountAtPath(MeshyRightArmMeshPath), Is.EqualTo(15400));
            Assert.That(TriangleCountAtPath(MeshyLeftArmMeshPath), Is.EqualTo(15400));
            Assert.That(TriangleCountAtPath(MeshyRiggedRightArmMeshPath), Is.EqualTo(15400));
            Assert.That(TriangleCountAtPath(MeshyRiggedLeftArmMeshPath), Is.EqualTo(15400));

            var manifest = File.ReadAllText(Path.Combine(Application.dataPath, "../ART_ASSET_MANIFEST.md"));
            Assert.That(manifest, Does.Contain("RR_FP_Arm_Right"));
            Assert.That(manifest, Does.Contain("Meshy_AI_Neon_Vanguard_Gauntle_0818131608_texture.fbx"));
            Assert.That(manifest, Does.Contain("15400"));
            Assert.That(manifest, Does.Contain("Integrated runtime arms"));
        }

        [Test]
        public void Phase050MeshyArtKit_GallerySceneIsEditorOnly()
        {
            Assert.That(
                AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/_Game/Levels/Scenes/ArtKitGallery.unity"),
                Is.Not.Null);
            Assert.That(
                EditorBuildSettings.scenes.Select(scene => scene.path),
                Does.Not.Contain("Assets/_Game/Levels/Scenes/ArtKitGallery.unity"));
        }

        [Test]
        public void Phase057MeshyArtKit_SpiralHasIntentionalVisualOnlyWorldLayers()
        {
            var spiral = Resources.LoadAll<ModuleDefinition>("Modules")
                .First(module => module.StableModuleId == "module.004.the-spiral");
            var decorationIds = spiral.Decorations
                .Select(decoration => decoration.StableId)
                .ToArray();

            Assert.That(decorationIds.Length, Is.GreaterThanOrEqualTo(24));
            Assert.That(decorationIds, Does.Contain("m04.world.garden.arch"));
            Assert.That(decorationIds, Does.Contain("m04.world.energy.pillar-cyan"));
            Assert.That(decorationIds, Does.Not.Contain("m04.world.far.spire-north"));
            Assert.That(decorationIds, Does.Contain("m04.world.depth.cloud-low-center"));
        }

        [Test]
        public void SkyCity093_OpeningTerraceAndStandardStepsUseWelcomingFootprints()
        {
            var modules = Resources.LoadAll<ModuleDefinition>("Modules");
            var m01 = modules.First(m => m.StableModuleId == "module.001.first-steps");
            var startBlock = m01.Blocks.First(b => b.StableId == "m01.start");
            Assert.That(startBlock.Size.x, Is.EqualTo(6f));
            Assert.That(startBlock.Size.z, Is.EqualTo(4f));

            var step01 = m01.Blocks.First(b => b.StableId == "m01.step.01");
            Assert.That(step01.Size.x, Is.EqualTo(2.5f));
            Assert.That(step01.Size.z, Is.EqualTo(2.5f));
        }

        [Test]
        public void Phase052WorldScale_TheSpiralHasFiveRestorePointsAndHumanScaleBlocks()
        {
            var spiral = Resources.LoadAll<ModuleDefinition>("Modules")
                .First(m => m.StableModuleId == "module.004.the-spiral");
            Assert.That(spiral.RestorePoints.Count, Is.EqualTo(5));
            Assert.That(spiral.PatchBlock, Is.Not.Null);
            Assert.That(spiral.PatchBlock.Size.x, Is.LessThanOrEqualTo(3.2f));
            Assert.That(spiral.Blocks.Count, Is.GreaterThanOrEqualTo(35));
            Assert.That(spiral.Decorations.Count, Is.GreaterThanOrEqualTo(24));
            Assert.That(spiral.DisableAutomaticDistantFragments, Is.True);
        }

        [Test]
        public void Phase053HandPresentation_ProfileRetainsSubstantialCameraSpaceScale()
        {
            var profile = Resources.Load<FirstPersonArmProfile>(FirstPersonArmProfile.ResourceName);
            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.HasArmPrefabs, Is.True);
            Assert.That(profile.PrefabLocalScale.x, Is.InRange(0.33f, 0.65f));
            Assert.That(profile.LeftBaseLocalPosition.z, Is.InRange(0.32f, 0.5f));
            Assert.That(profile.LeftBaseLocalPosition.y, Is.InRange(-0.55f, 0.08f));
            Assert.That(profile.LeftBaseLocalPosition.x, Is.InRange(-0.65f, -0.25f));
            Assert.That(profile.RightBaseLocalPosition.x, Is.InRange(0.25f, 0.65f));
        }

        [Test]
        public void Phase053WorldCleanup_EnvironmentProfileHasBrightCyanSkyAndAiryFog()
        {
            var environment = Resources.Load<ModuleEnvironmentProfile>(ModuleEnvironmentProfile.ResourceName);
            Assert.That(environment, Is.Not.Null);
            Assert.That(environment.FogDensity, Is.InRange(0.0005f, 0.0035f));
            Assert.That(environment.FogColor.b, Is.GreaterThanOrEqualTo(environment.FogColor.r));
            Assert.That(environment.SkyHorizon.b, Is.GreaterThanOrEqualTo(environment.SkyHorizon.r));
        }

        [Test]
        public void Phase056Skybox_SeamlessProceduralMaterialIsAvailableInResources()
        {
            var skyMat = Resources.Load<Material>("Materials/MAT_RR_Skybox_Seamless");
            Assert.That(skyMat, Is.Not.Null);
            Assert.That(skyMat.shader.name, Is.EqualTo("Skybox/Procedural"));
            Assert.That(skyMat.HasProperty("_Tex"), Is.False);
        }

        [Test]
        public void Phase081AncientAbyssSkybox_UsesSeamlessCubemapProjection()
        {
            var skyMat = Resources.Load<Material>("Materials/MAT_RR_AncientAbyssSky");
            Assert.That(skyMat, Is.Not.Null);
            Assert.That(skyMat.shader.name, Is.EqualTo("Skybox/Cubemap"));
            Assert.That(skyMat.GetTexture("_Tex"), Is.TypeOf<Cubemap>());

            var importer = AssetImporter.GetAtPath(
                "Assets/_Game/Art/Sky/Skybox_Ryders_Road_AncientAbyss_081.png") as TextureImporter;
            Assert.That(importer, Is.Not.Null);
            Assert.That(importer.textureShape, Is.EqualTo(TextureImporterShape.TextureCube));
            Assert.That(importer.generateCubemap, Is.EqualTo(TextureImporterGenerateCubemap.AutoCubemap));
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            Assert.That(settings.seamlessCubemap, Is.True);
        }

        [Test]
        public void Phase056HandPoseLab_UsesProductionProfileAtExactGameplayFov()
        {
            Assert.That(
                AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/_Game/Levels/Scenes/HandPoseLab.unity"),
                Is.Not.Null);
            var cameraProfile = Resources.Load<CameraProfile>("Camera_Default");
            Assert.That(cameraProfile, Is.Not.Null);
            Assert.That(cameraProfile.BaseFieldOfView, Is.EqualTo(94f).Within(0.01f));
        }

        [Test]
        public void Phase052WorldScale_ArtScaleCollisionLabSceneExists()
        {
            Assert.That(
                AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/_Game/Levels/Scenes/ArtScaleCollisionLab.unity"),
                Is.Not.Null);
        }

        private static ModuleDefinition CreateValidModule(string stableId)
        {
            var module = ScriptableObject.CreateInstance<ModuleDefinition>();
            module.Configure(
                stableId,
                "Valid Module",
                "valid_module",
                "project.ryders-block",
                "world.prototype-sky",
                1,
                ModuleSelectionState.ModuleRunnerSceneName,
                ModuleDifficulty.Intro,
                new ModulePose(new Vector3(0f, 0.55f, 0f), Vector3.zero),
                new ModulePatchBlockDefinition(
                    "patch.valid",
                    Vector3.forward * 12f,
                    Vector3.one * 2f),
                new[]
                {
                    new ModuleRestorePointDefinition(
                        "restore.valid.mid",
                        0,
                        Vector3.forward * 6f,
                        Vector3.forward)
                },
                20f,
                40f,
                new[] { ModuleMechanic.StandardBlock, ModuleMechanic.PatchBlock },
                null,
                ModuleEnvironmentProfile.CreateRuntimeDefault(),
                ModuleVisualProfile.CreateRuntimeDefault(),
                string.Empty,
                string.Empty,
                new[]
                {
                    new ModuleBlockDefinition(
                        "block.valid",
                        Vector3.zero,
                        Vector3.one,
                        ModuleMaterialRole.Normal)
                },
                null,
                null,
                null,
                null,
                null,
                null);
            module.ConfigurePlayability(
                "start.valid",
                "block.valid",
                -8f,
                false);
            return module;
        }

        private static readonly string[] MeshyEnvironmentPrefabPaths =
        {
            "Assets/_Game/Art/Environment/Platforms/PF_RR_Block_Standard.prefab",
            "Assets/_Game/Art/Environment/Platforms/PF_RR_Platform_Long.prefab",
            "Assets/_Game/Art/Environment/Architecture/PF_RR_Pillar_Tall.prefab",
            "Assets/_Game/Art/Environment/Decoration/PF_RR_Rock_Broken.prefab",
            "Assets/_Game/Art/Environment/Nature/PF_RR_Tree_Floating.prefab",
            "Assets/_Game/Art/Environment/Nature/PF_RR_GrassTopper.prefab",
            "Assets/_Game/Art/Gameplay/Restore/PF_RR_RestorePoint.prefab",
            "Assets/_Game/Art/Gameplay/Boost/PF_RR_BoostPad.prefab",
            "Assets/_Game/Art/Gameplay/Patch/PF_RR_PatchBlock.prefab",
            "Assets/_Game/Art/Environment/Architecture/PF_RR_RuinedTower.prefab",
            "Assets/_Game/Art/Environment/Architecture/PF_RR_FloatingArch.prefab",
            "Assets/_Game/Art/Environment/Decoration/PF_RR_EnergyPillar.prefab"
        };

        private static readonly ModuleMaterialRole[] MeshyMappedRoles =
        {
            ModuleMaterialRole.Normal,
            ModuleMaterialRole.Precision,
            ModuleMaterialRole.Moving,
            ModuleMaterialRole.Crumbling,
            ModuleMaterialRole.Restore,
            ModuleMaterialRole.Boost,
            ModuleMaterialRole.Patch,
            ModuleMaterialRole.TowerCore,
            ModuleMaterialRole.DecorationStone,
            ModuleMaterialRole.Vegetation,
            ModuleMaterialRole.Flower,
            ModuleMaterialRole.ShortcutCue,
            ModuleMaterialRole.CircuitLine
        };

        private const string MeshyRightArmPrefabPath = "Assets/_Game/Art/FirstPerson/Arms/PF_RR_FP_Arm_Right.prefab";
        private const string MeshyLeftArmPrefabPath = "Assets/_Game/Art/FirstPerson/Arms/PF_RR_FP_Arm_Left.prefab";
        private const string MeshyLegacyRightArmPrefabPath = "Assets/_Game/Art/FirstPerson/Arms/LegacyApproved/PF_RR_FP_Arm_Right_LegacyApproved.prefab";
        private const string MeshyLegacyLeftArmPrefabPath = "Assets/_Game/Art/FirstPerson/Arms/LegacyApproved/PF_RR_FP_Arm_Left_LegacyApproved.prefab";
        private const string MeshyRiggedRightArmPrefabPath = "Assets/_Game/Art/FirstPerson/Arms/Rigged/PF_RR_FP_Arm_Rigged_Right.prefab";
        private const string MeshyRiggedLeftArmPrefabPath = "Assets/_Game/Art/FirstPerson/Arms/Rigged/PF_RR_FP_Arm_Rigged_Left.prefab";
        private const string MeshyRightArmMeshPath = "Assets/_Game/Art/Generated/FirstPerson/RR_FP_Arm_Right_00.asset";
        private const string MeshyLeftArmMeshPath = "Assets/_Game/Art/Generated/FirstPerson/RR_FP_Arm_Left_00.asset";
        private const string MeshyRiggedRightArmMeshPath = "Assets/_Game/Art/FirstPerson/Arms/Rigged/Meshes/RR_FP_Rigged_Arm_Right.asset";
        private const string MeshyRiggedLeftArmMeshPath = "Assets/_Game/Art/FirstPerson/Arms/Rigged/Meshes/RR_FP_Rigged_Arm_Left.asset";
        private const string RyderArmV2RightPrefabPath = "Assets/_Game/Art/FirstPerson/Arms/RyderArmV2/PF_RR_FP_RyderArmV2_Right.prefab";
        private const string RyderArmV2LeftPrefabPath = "Assets/_Game/Art/FirstPerson/Arms/RyderArmV2/PF_RR_FP_RyderArmV2_Left.prefab";
        private const string RyderArmV2RightMeshPath = "Assets/_Game/Art/FirstPerson/Arms/RyderArmV2/Meshes/RR_FP_RyderArmV2_Right.asset";
        private const string RyderArmV2LeftMeshPath = "Assets/_Game/Art/FirstPerson/Arms/RyderArmV2/Meshes/RR_FP_RyderArmV2_Left.asset";
        private const string RyderArmV2MaterialPath = "Assets/_Game/Art/FirstPerson/Arms/RyderArmV2/Materials/MAT_RR_FP_RyderArmV2.mat";
        private const string RyderArmV2SourcePath = "Assets/_Game/Art/FirstPerson/Arms/RyderArmV2/Source/Ryders_Arm_Rigged.glb";

        private static readonly string[] MeshyArmRuntimePaths =
        {
            MeshyLeftArmPrefabPath,
            MeshyRightArmPrefabPath,
            MeshyLegacyLeftArmPrefabPath,
            MeshyLegacyRightArmPrefabPath,
            MeshyRiggedLeftArmPrefabPath,
            MeshyRiggedRightArmPrefabPath,
            MeshyLeftArmMeshPath,
            MeshyRightArmMeshPath,
            MeshyRiggedLeftArmMeshPath,
            MeshyRiggedRightArmMeshPath,
            "Assets/_Game/Visuals/Resources/FirstPersonArmProfile.asset",
            "Assets/_Game/Art/Materials/MAT_RR_FP_Arm_Right.mat",
            "Assets/_Game/Art/FirstPerson/Arms/Rigged/Materials/MAT_RR_FP_Rigged_Arm.mat",
            "Assets/_Game/Art/MeshySource/FirstPersonRigged/Meshy_AI_Character_Arm_Rigged.glb",
            "Assets/_Game/Art/Textures/RR_FP_Arm_Right_Base.png",
            "Assets/_Game/Art/Textures/RR_FP_Arm_Right_Normal.png",
            "Assets/_Game/Art/Textures/RR_FP_Arm_Right_Metallic.png",
            "Assets/_Game/Art/Textures/RR_FP_Arm_Right_Roughness.png",
            "Assets/_Game/Art/FirstPerson/Arms/Rigged/Textures/RR_FP_Rigged_Arm_Image_0.jpg",
            "Assets/_Game/Art/FirstPerson/Arms/Rigged/Textures/RR_FP_Rigged_Arm_Image_1.jpg",
            "Assets/_Game/Art/FirstPerson/Arms/Rigged/Textures/RR_FP_Rigged_Arm_Image_2.jpg",
            "Assets/_Game/Art/Generated/EmissionMasks/RR_FP_Arm_Right_CyanEmission.png"
        };

        private static int TriangleCountAtPath(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Mesh>()
                .Sum(mesh => mesh.triangles.Length / 3);
        }

        private static void AssertArmPrefabIsVisualOnly(GameObject prefab)
        {
            Assert.That(prefab.GetComponentsInChildren<Renderer>(includeInactive: true).Length, Is.GreaterThan(0));
            Assert.That(prefab.GetComponentsInChildren<Collider>(includeInactive: true), Is.Empty);
            foreach (var transform in prefab.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                Assert.That(transform.localScale.x, Is.GreaterThan(0f), transform.name);
                Assert.That(transform.localScale.y, Is.GreaterThan(0f), transform.name);
                Assert.That(transform.localScale.z, Is.GreaterThan(0f), transform.name);
            }
        }

        private static bool AreBakedXMirrors(Mesh left, Mesh right)
        {
            if (left == null || right == null || left.vertexCount != right.vertexCount)
            {
                return false;
            }

            var leftVertices = left.vertices;
            var rightVertices = right.vertices;
            for (var index = 0; index < leftVertices.Length; index++)
            {
                var expected = new Vector3(-rightVertices[index].x, rightVertices[index].y, rightVertices[index].z);
                if ((leftVertices[index] - expected).sqrMagnitude > 0.000000000001f)
                {
                    return false;
                }
            }

            return true;
        }

        private static System.Collections.Generic.IEnumerable<RouteSupport> OrderedSpiralSupports(
            ModuleDefinition module)
        {
            foreach (var block in module.Blocks)
            {
                yield return new RouteSupport(block.StableId, block.Pose, block.Size);
            }

            foreach (var restore in module.RestorePoints)
            {
                yield return new RouteSupport(
                    restore.StableId,
                    restore.Pose,
                    new Vector3(3f, 0.6f, 3f));
            }

            foreach (var crumble in module.CrumblingBlocks)
            {
                yield return new RouteSupport(crumble.StableId, crumble.Pose, crumble.Size);
            }

            foreach (var moving in module.MovingBlocks)
            {
                for (var index = 0; index < moving.PathPoints.Count; index++)
                {
                    var position = moving.PathSpace == ModulePathSpace.Local
                        ? moving.Pose.Position + moving.PathPoints[index]
                        : moving.PathPoints[index];
                    yield return new RouteSupport(
                        moving.StableId + ".path." + index,
                        new ModulePose(position, moving.Pose.EulerAngles),
                        moving.Size);
                }
            }

            var patch = module.PatchBlock;
            yield return new RouteSupport(
                patch.StableId,
                new ModulePose(
                    patch.Pose.Position + Vector3.down * 0.8f,
                    patch.Pose.EulerAngles),
                new Vector3(3f, 0.6f, 3f));
        }

        private static System.Collections.Generic.IEnumerable<RouteGap> AdjacentSupportGaps(
            System.Collections.Generic.IReadOnlyList<RouteSupport> unsortedSupports)
        {
            var supports = unsortedSupports.OrderBy(support => support.TopY).ToArray();
            for (var index = 0; index < supports.Length - 1; index++)
            {
                var from = supports[index];
                var to = supports[index + 1];
                var delta = new Vector2(
                    to.Pose.Position.x - from.Pose.Position.x,
                    to.Pose.Position.z - from.Pose.Position.z);
                var centerDistance = delta.magnitude;
                var direction = centerDistance > 0.001f
                    ? delta / centerDistance
                    : Vector2.zero;
                var worldDirection = new Vector3(direction.x, 0f, direction.y);
                var fromLocalDirection = Quaternion.Inverse(from.Pose.Rotation) * worldDirection;
                var toLocalDirection = Quaternion.Inverse(to.Pose.Rotation) * worldDirection;
                var fromRadius = Mathf.Abs(fromLocalDirection.x) * from.Size.x * 0.5f
                    + Mathf.Abs(fromLocalDirection.z) * from.Size.z * 0.5f;
                var toRadius = Mathf.Abs(toLocalDirection.x) * to.Size.x * 0.5f
                    + Mathf.Abs(toLocalDirection.z) * to.Size.z * 0.5f;

                yield return new RouteGap(
                    Mathf.Max(0f, centerDistance - fromRadius - toRadius),
                    to.TopY - from.TopY,
                    $"{from.StableId} -> {to.StableId}");
            }
        }

        private readonly struct RouteSupport
        {
            public RouteSupport(string stableId, ModulePose pose, Vector3 size)
            {
                StableId = stableId;
                Pose = pose;
                Size = size;
            }

            public string StableId { get; }
            public ModulePose Pose { get; }
            public Vector3 Size { get; }
            public float TopY => Pose.Position.y + Size.y * 0.5f;
        }

        private readonly struct RouteGap
        {
            public RouteGap(float edgeGap, float rise, string label)
            {
                EdgeGap = edgeGap;
                Rise = rise;
                Label = label;
            }

            public float EdgeGap { get; }
            public float Rise { get; }
            public string Label { get; }
        }

        [Test]
        public void Phase054b_PlatformTruthLab_HasAllTenCanonicalMechanicsAndStations()
        {
            var lab = Resources.Load<ModuleDefinition>("Modules/Module_PlatformTruthLab");
            Assert.That(lab, Is.Not.Null);
            Assert.That(lab.Blocks.Count, Is.GreaterThanOrEqualTo(8));
            Assert.That(lab.MovingBlocks.Count, Is.EqualTo(1));
            Assert.That(lab.BoostBlocks.Count, Is.EqualTo(1));
            Assert.That(lab.CrumblingBlocks.Count, Is.EqualTo(1));
            Assert.That(lab.RestorePoints.Count, Is.EqualTo(1));
            Assert.That(lab.SurfSurfaces.Count, Is.EqualTo(1));
            Assert.That(lab.PatchBlock, Is.Not.Null);
            Assert.That(lab.MechanicsUsed, Does.Contain(ModuleMechanic.StandardBlock));
            Assert.That(lab.MechanicsUsed, Does.Contain(ModuleMechanic.MovingBlock));
            Assert.That(lab.MechanicsUsed, Does.Contain(ModuleMechanic.JumpBoost));
            Assert.That(lab.MechanicsUsed, Does.Contain(ModuleMechanic.CrumblingBlock));
            Assert.That(lab.MechanicsUsed, Does.Contain(ModuleMechanic.SurfSurface));
            Assert.That(lab.MechanicsUsed, Does.Contain(ModuleMechanic.RestorePoint));
            Assert.That(lab.MechanicsUsed, Does.Contain(ModuleMechanic.PatchBlock));
        }

        [Test]
        public void Phase054b_RestorePoints_AllHaveCenteredSafelyElevatedSpawnAnchors()
        {
            var modules = new[]
            {
                "Modules/Module_001_FirstSteps",
                "Modules/Module_002_MovingParts",
                "Modules/Module_003_FlowError",
                "Modules/Module_004_TheSpiral",
                "Modules/Module_PlatformTruthLab"
            };

            foreach (var path in modules)
            {
                var module = Resources.Load<ModuleDefinition>(path);
                Assert.That(module, Is.Not.Null, path);
                foreach (var restore in module.RestorePoints)
                {
                    Assert.That(restore.RestoreOffset.y, Is.GreaterThanOrEqualTo(0.05f), $"{module.StableModuleId} -> {restore.StableId}");
                    Assert.That(Mathf.Abs(restore.RestoreOffset.x), Is.LessThanOrEqualTo(0.5f), $"{module.StableModuleId} -> {restore.StableId}");
                    Assert.That(Mathf.Abs(restore.RestoreOffset.z), Is.LessThanOrEqualTo(0.5f), $"{module.StableModuleId} -> {restore.StableId}");
                }
            }
        }

        [Test]
        public void Phase075_ArmPresentation_HidesButPreservesRecoveredRyderArmV2()
        {
            var armProfile = Resources.Load<FirstPersonArmProfile>("FirstPersonArmProfile");
            Assert.That(armProfile, Is.Not.Null);
            Assert.That(armProfile.PresentationMode, Is.EqualTo(FirstPersonArmPresentationMode.RyderArmV2));
            Assert.That(armProfile.ShowFirstPersonArms, Is.False);
            Assert.That(armProfile.LeftBaseLocalEulerAngles, Is.EqualTo(new Vector3(0f, -52f, 202f)).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(armProfile.RightBaseLocalEulerAngles, Is.EqualTo(new Vector3(1f, 52f, 158f)).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(armProfile.LeftBaseLocalPosition, Is.EqualTo(new Vector3(-0.52f, -0.025f, 0.41f)).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(armProfile.RightBaseLocalPosition, Is.EqualTo(new Vector3(0.52f, -0.02f, 0.41f)).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(armProfile.PrefabLocalScale.x, Is.EqualTo(0.36f).Within(0.001f));
            Assert.That(armProfile.NeutralPoseLocked, Is.False);
            Assert.That(armProfile.ArmFieldOfView, Is.EqualTo(72f).Within(0.001f));
            Assert.That(FirstPersonHands.ResolveArmLayer(), Is.GreaterThanOrEqualTo(0));
            Assert.That(armProfile.RunForwardAmount, Is.GreaterThan(0f));
            Assert.That(armProfile.RunForwardAmount, Is.LessThanOrEqualTo(0.03f));
            Assert.That(armProfile.RunLateralAmount, Is.LessThanOrEqualTo(0.0025f));
            Assert.That(armProfile.JumpOffset, Is.GreaterThan(0f));
            Assert.That(armProfile.LandImpulse, Is.GreaterThan(0f));
            Assert.That(armProfile.MaximumPresentationDisplacement, Is.LessThanOrEqualTo(0.055f));
            Assert.That(armProfile.LeftPrefabEulerOffset, Is.EqualTo(Vector3.zero).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(armProfile.RightPrefabEulerOffset, Is.EqualTo(Vector3.zero).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(armProfile.UseRiggedArms, Is.True);
            Assert.That(armProfile.HasRyderArmV2Prefabs, Is.True);
            Assert.That(armProfile.HasPreviousRiggedFallback, Is.True);
            Assert.That(armProfile.HasLegacyApprovedFallback, Is.True);
            Assert.That(AssetDatabase.GetAssetPath(armProfile.LeftArmPrefab), Is.EqualTo(RyderArmV2LeftPrefabPath));
            Assert.That(AssetDatabase.GetAssetPath(armProfile.RightArmPrefab), Is.EqualTo(RyderArmV2RightPrefabPath));
            Assert.That(AssetDatabase.GetAssetPath(armProfile.RiggedLeftArmPrefab), Is.EqualTo(MeshyRiggedLeftArmPrefabPath));
            Assert.That(AssetDatabase.GetAssetPath(armProfile.RiggedRightArmPrefab), Is.EqualTo(MeshyRiggedRightArmPrefabPath));
            AssertArmPrefabIsVisualOnly(armProfile.RiggedLeftArmPrefab);
            AssertArmPrefabIsVisualOnly(armProfile.RiggedRightArmPrefab);
            AssertArmPrefabIsVisualOnly(armProfile.LeftArmPrefab);
            AssertArmPrefabIsVisualOnly(armProfile.RightArmPrefab);
            Assert.That(armProfile.RightArmPrefab.transform.Find("RigCorrectionRoot/RyderArmV2Rig_Right/Skeleton"), Is.Not.Null);
            Assert.That(armProfile.LeftArmPrefab.transform.Find("RigCorrectionRoot/RyderArmV2Rig_Left/Skeleton"), Is.Not.Null);

            var leftSkin = armProfile.LeftArmPrefab.GetComponentInChildren<SkinnedMeshRenderer>(true);
            var rightSkin = armProfile.RightArmPrefab.GetComponentInChildren<SkinnedMeshRenderer>(true);
            Assert.That(leftSkin, Is.Not.Null);
            Assert.That(rightSkin, Is.Not.Null);
            Assert.That(leftSkin.sharedMesh.vertexCount, Is.EqualTo(11953));
            Assert.That(rightSkin.sharedMesh.vertexCount, Is.EqualTo(11953));
            Assert.That(leftSkin.sharedMesh.triangles.Length / 3, Is.EqualTo(15541));
            Assert.That(rightSkin.sharedMesh.triangles.Length / 3, Is.EqualTo(15541));
            Assert.That(leftSkin.bones, Has.Length.EqualTo(24));
            Assert.That(rightSkin.bones, Has.Length.EqualTo(24));
            foreach (var boneName in new[]
                     {
                         "Bone_000", "Bone_003", "Bone_002", "Bone_001",
                         "Bone_007", "Bone_011", "Bone_015", "Bone_019", "Bone_023"
                     })
            {
                Assert.That(leftSkin.bones.Select(bone => bone.name), Does.Contain(boneName));
                Assert.That(rightSkin.bones.Select(bone => bone.name), Does.Contain(boneName));
            }
            Assert.That(
                armProfile.LeftArmPrefab.GetComponentsInChildren<Transform>(true).Select(item => item.name),
                Does.Not.Contain("ForearmEntrySleeve"));
            Assert.That(
                armProfile.RightArmPrefab.GetComponentsInChildren<Transform>(true).Select(item => item.name),
                Does.Not.Contain("ForearmEntrySleeve"));
            Assert.That(leftSkin.sharedMesh.vertexCount, Is.EqualTo(rightSkin.sharedMesh.vertexCount));
            Assert.That(leftSkin.sharedMesh.tangents, Has.Length.EqualTo(leftSkin.sharedMesh.vertexCount));
            Assert.That(rightSkin.sharedMesh.tangents, Has.Length.EqualTo(rightSkin.sharedMesh.vertexCount));
            Assert.That(AreBakedXMirrors(leftSkin.sharedMesh, rightSkin.sharedMesh), Is.True);

            var material = AssetDatabase.LoadAssetAtPath<Material>(
                RyderArmV2MaterialPath);
            Assert.That(material, Is.Not.Null);
            Assert.That(material.GetTexture("_BaseMap"), Is.Not.Null);
            Assert.That(material.GetTexture("_BumpMap"), Is.Not.Null);
            Assert.That(material.GetTexture("_EmissionMap"), Is.Not.Null);
            Assert.That(material.GetTexture("_MetallicGlossMap"), Is.Null);
            Assert.That(material.GetFloat("_Metallic"), Is.LessThanOrEqualTo(0.1f));
            Assert.That(material.GetFloat("_Smoothness"), Is.InRange(0.36f, 0.48f));
            Assert.That(material.GetFloat("_BumpScale"), Is.LessThanOrEqualTo(0.4f));
            Assert.That(material.GetFloat("_Cull"), Is.EqualTo(2f));

            Assert.That(AssetDatabase.LoadAssetAtPath<Object>(RyderArmV2SourcePath), Is.Not.Null);
            Assert.That(TriangleCountAtPath(RyderArmV2LeftMeshPath), Is.EqualTo(15541));
            Assert.That(TriangleCountAtPath(RyderArmV2RightMeshPath), Is.EqualTo(15541));
        }

        [Test]
        public void Phase056_Skybox_MaterialIsProceduralAndRuntimeReady()
        {
            var skyMat = Resources.Load<Material>("Materials/MAT_RR_Skybox_Seamless");
            Assert.That(skyMat, Is.Not.Null);
            Assert.That(skyMat.shader.name, Is.EqualTo("Skybox/Procedural"));
            Assert.That(skyMat.GetFloat("_Exposure"), Is.InRange(0.9f, 1.2f));
        }

        [Test]
        public void Phase054b_SurfAndBoost_HaveDistinctRolesAndProfiles()
        {
            Assert.That(ModuleMaterialRole.Surf, Is.Not.EqualTo(ModuleMaterialRole.Boost));
            var gallery = Resources.Load<ModuleDefinition>("Modules/Module_GameplayRoleGallery");
            Assert.That(gallery, Is.Not.Null);
            Assert.That(gallery.SurfSurfaces.Count, Is.GreaterThan(0));
            Assert.That(gallery.BoostBlocks.Count, Is.GreaterThan(0));
        }

        [Test]
        public void Phase056_SpiralStart_ResolvesDeterministicallyWithoutCorrection()
        {
            var spiral = Resources.Load<ModuleDefinition>("Modules/Module_004_TheSpiral");
            Assert.That(spiral, Is.Not.Null);
            ModuleStartResolution? first = null;
            for (var index = 0; index < 10; index++)
            {
                var result = ModuleStartSafety.Resolve(spiral);
                Assert.That(result.UsedSafetyCorrection, Is.False, result.Diagnostic);
                Assert.That(result.SupportBlockStableId, Is.EqualTo("m04.start"));
                if (first.HasValue)
                {
                    Assert.That(result.Pose.Position, Is.EqualTo(first.Value.Pose.Position).Using(Vector3ComparerWithEqualsOperator.Instance));
                    Assert.That(result.Pose.EulerAngles, Is.EqualTo(first.Value.Pose.EulerAngles).Using(Vector3ComparerWithEqualsOperator.Instance));
                }
                first = result;
            }

            Assert.That(spiral.ResolveFallThreshold(-30f), Is.EqualTo(-7.5f).Within(0.01f));
        }

        [Test]
        public void Phase056_MovementEnvelope_IsMeasuredFromLockedProductionProfile()
        {
            var profile = Resources.Load<MovementProfile>("MovementProfiles/Movement_Default");
            Assert.That(profile, Is.Not.Null);
            var metrics = MovementEnvelopeMeasurement.Measure(profile);

            Assert.That(metrics.RunSpeed, Is.EqualTo(profile.RunSpeed).Within(0.001f));
            Assert.That(metrics.JumpAirtime, Is.EqualTo(profile.EstimatedAirtime).Within(0.001f));
            Assert.That(metrics.ComfortableBronzeGap, Is.LessThan(metrics.LandingMarginDistance));
            Assert.That(metrics.LandingMarginDistance, Is.LessThan(metrics.MaximumBronzeGap));
            Assert.That(metrics.MaximumBronzeGap, Is.LessThan(metrics.NormalJumpDistance));
            Assert.That(metrics.BhopAssistedDistance, Is.GreaterThan(metrics.NormalJumpDistance));
            Assert.That(metrics.BoostAssistedDistance, Is.GreaterThan(metrics.MaximumBronzeGap));
        }

        [Test]
        public void Phase055_GameplayRoleGallery_AssetAndSceneExistWithAllRoles()
        {
            var gallery = Resources.Load<ModuleDefinition>("Modules/Module_GameplayRoleGallery");
            Assert.That(gallery, Is.Not.Null);
            Assert.That(gallery.StableModuleId, Is.EqualTo("module.gameplay-role-gallery"));
            Assert.That(gallery.Blocks.Count, Is.GreaterThan(5));
            Assert.That(gallery.MovingBlocks.Count, Is.EqualTo(1));
            Assert.That(gallery.BoostBlocks.Count, Is.EqualTo(1));
            Assert.That(gallery.CrumblingBlocks.Count, Is.EqualTo(1));
            Assert.That(gallery.SurfSurfaces.Count, Is.EqualTo(1));
            Assert.That(gallery.RestorePoints.Count, Is.EqualTo(1));
            Assert.That(gallery.PatchBlock, Is.Not.Null);

            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/_Game/Levels/Scenes/GameplayRoleGallery.unity");
            Assert.That(scene, Is.Not.Null);
        }

        [Test]
        public void Phase055_TheSpiral_RecoveredHumanScaleParkourLayout()
        {
            var spiral = Resources.Load<ModuleDefinition>("Modules/Module_004_TheSpiral");
            Assert.That(spiral, Is.Not.Null);

            // Assert that vast continuous bridge slabs (width > 6m) have been removed
            foreach (var block in spiral.Blocks)
            {
                Assert.That(
                    Mathf.Max(block.Size.x, block.Size.z),
                    Is.LessThanOrEqualTo(5.0f),
                    $"Block '{block.StableId}' should not be an oversized bridge slab (size={block.Size})");
            }
        }

        [Test]
        public void Phase055_PlatformLong_FormFittingScale_MatchesTopSurface()
        {
            var library = Resources.Load<ModuleVisualPrefabLibrary>(ModuleVisualPrefabLibrary.ResourceName);
            Assert.That(library, Is.Not.Null);

            var movingPlacement = library.PlacementFor(ModuleMaterialRole.Moving, new Vector3(3f, 0.6f, 1.5f));
            Assert.That(movingPlacement.IsValid, Is.True);
            Assert.That(movingPlacement.Prefab.name, Does.Contain("RR_Platform_Long"));
            Assert.That(movingPlacement.LocalScale.y, Is.GreaterThan(1.0f).And.LessThan(2.0f));
        }

        [Test]
        public void Phase057_CanonicalPlatformsResolveWithoutDefaultCarpetTiling()
        {
            var library = Resources.Load<ModuleVisualPrefabLibrary>(ModuleVisualPrefabLibrary.ResourceName);
            Assert.That(library, Is.Not.Null);

            var standard = library.PlacementFor(ModuleMaterialRole.Normal, new Vector3(1.5f, 0.6f, 1.5f));
            var safe = library.PlacementFor(ModuleMaterialRole.Normal, new Vector3(3f, 0.6f, 3f));
            var longPlatform = library.PlacementFor(ModuleMaterialRole.Normal, new Vector3(3f, 0.6f, 1.5f));

            Assert.That(standard.Prefab.name, Is.EqualTo("PF_RR_Block_Standard"));
            Assert.That(standard.FitMode, Is.EqualTo(VisualFitMode.TopAlignedUniform));
            Assert.That(safe.Prefab.name, Is.EqualTo("PF_RR_Block_Safe"));
            Assert.That(safe.FitMode, Is.EqualTo(VisualFitMode.TopAlignedUniform));
            Assert.That(longPlatform.Prefab.name, Is.EqualTo("PF_RR_Platform_Long"));
            Assert.That(longPlatform.FitMode, Is.EqualTo(VisualFitMode.ExactFootprint));
        }

        [Test]
        public void Phase057_StandardAndSafeKeepTruthfulAuthoredColliderFootprints()
        {
            var firstSteps = Resources.Load<ModuleDefinition>("Modules/Module_001_FirstSteps");
            var standard = firstSteps.Blocks.Single(block => block.StableId == "m01.step.01");
            var safe = firstSteps.Blocks.Single(block => block.StableId == "m01.start");

            Assert.That(standard.Size, Is.EqualTo(new Vector3(2.5f, 0.6f, 2.5f)));
            Assert.That(safe.Size, Is.EqualTo(new Vector3(6f, 0.6f, 4f)));
            Assert.That(
                ModuleVisualPrefabLibrary.ResolveTopAlignedUniformLocalScale(standard.Size),
                Is.EqualTo(new Vector3(1f, 2.5f / .6f, 1f)));
            Assert.That(
                ModuleVisualPrefabLibrary.ResolveTopAlignedUniformLocalScale(safe.Size),
                Is.EqualTo(new Vector3(4f / 6f, 4f / .6f, 1f)));
        }

        [Test]
        public void Phase057_DeathRestorePreservesYawWhileManualRestoreUsesCheckpointYaw()
        {
            var checkpointRotation = Quaternion.Euler(0f, 42f, 0f);
            var deathRestore = RestoreController.ResolveRestoreRotation(checkpointRotation, true, 127f);
            var manualRestore = RestoreController.ResolveRestoreRotation(checkpointRotation, false, 127f);

            Assert.That(Mathf.DeltaAngle(deathRestore.eulerAngles.y, 127f), Is.EqualTo(0f).Within(0.01f));
            Assert.That(Mathf.DeltaAngle(manualRestore.eulerAngles.y, 42f), Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void Phase060_SpiralWorldHasMandatorySequentialCrumbleLanguage()
        {
            var spiral = Resources.Load<ModuleDefinition>("Modules/Module_004_TheSpiral");
            var crumbleVisuals = Resources.Load<CrumbleVisualSet>(CrumbleVisualSet.ResourceName);

            Assert.That(spiral.DisableAutomaticDistantFragments, Is.True);
            Assert.That(spiral.Decorations.Count, Is.GreaterThanOrEqualTo(31));
            Assert.That(spiral.Decorations.All(item => item.StableId.StartsWith("m04.world.")), Is.True);
            Assert.That(spiral.Decorations.Any(item => item.StableId.StartsWith("m04.world.far.")), Is.True);
            Assert.That(spiral.Decorations.Count(item => item.StableId.StartsWith("m04.world.depth.")), Is.GreaterThanOrEqualTo(7));
            Assert.That(spiral.Decorations.Count(item => item.StableId.StartsWith("m04.world.core.")), Is.EqualTo(6));
            Assert.That(spiral.Decorations.Any(item => item.StableId == "m04.world.core.base"), Is.True);
            Assert.That(spiral.Decorations.Any(item => item.StableId == "m04.world.far.spire-north"), Is.False);
            Assert.That(spiral.Decorations.Any(item => item.StableId == "m04.world.depth.cloud-low-center"), Is.True);
            Assert.That(spiral.Decorations.Any(item => item.VisualRole == ModuleMaterialRole.Corruption), Is.False);
            Assert.That(spiral.Decorations.Count(item => item.StableId.StartsWith("m04.world.depth.")), Is.GreaterThanOrEqualTo(14));

            var coreTowerPieces = spiral.Decorations
                .Where(item => item.StableId == "m04.world.core.lower"
                    || item.StableId == "m04.world.core.mid"
                    || item.StableId == "m04.world.core.upper")
                .ToArray();
            Assert.That(coreTowerPieces.Length, Is.EqualTo(3));
            for (var outer = 0; outer < coreTowerPieces.Length; outer++)
            {
                for (var inner = outer + 1; inner < coreTowerPieces.Length; inner++)
                {
                    var first = Vector3.ProjectOnPlane(coreTowerPieces[outer].Pose.Position, Vector3.up);
                    var second = Vector3.ProjectOnPlane(coreTowerPieces[inner].Pose.Position, Vector3.up);
                    Assert.That(Vector3.Distance(first, second), Is.GreaterThan(6f));
                }
            }

            var redGlows = spiral.Decorations
                .Where(item => item.StableId.StartsWith("m04.world.depth.red-glow"))
                .ToArray();
            Assert.That(redGlows.Length, Is.GreaterThanOrEqualTo(3));
            Assert.That(redGlows.All(item => item.VisualRole == ModuleMaterialRole.CrumbleFault), Is.True);
            Assert.That(redGlows.All(item => item.Pose.Position.y < -70f), Is.True);
            Assert.That(redGlows.All(item => item.Size.y > item.Size.x), Is.True);

            var abyssFog = spiral.Decorations
                .Where(item => item.StableId.Contains("fog"))
                .ToArray();
            Assert.That(abyssFog.Count(item => item.Pose.Position.y < -35f), Is.GreaterThanOrEqualTo(4));
            Assert.That(abyssFog.Where(item => item.Pose.Position.y < -35f).Max(item => item.Size.x), Is.GreaterThanOrEqualTo(100f));
            Assert.That(abyssFog.Where(item => item.Pose.Position.y < -35f).Max(item => item.Size.y), Is.LessThanOrEqualTo(16f));

            var deepSilhouettes = spiral.Decorations
                .Where(item => item.StableId.StartsWith("m04.world.depth.silhouette"))
                .ToArray();
            Assert.That(deepSilhouettes, Is.Empty);

            Assert.That(crumbleVisuals, Is.Not.Null);
            Assert.That(crumbleVisuals.IsComplete, Is.True);
            Assert.That(crumbleVisuals.Stage1Prefab.name, Is.EqualTo("PF_RR_Crumble_Stage1"));
            Assert.That(crumbleVisuals.Stage2Prefab.name, Is.EqualTo("PF_RR_Crumble_Stage2"));
            Assert.That(crumbleVisuals.Stage3Prefab.name, Is.EqualTo("PF_RR_Crumble_Stage3"));
            Assert.That(
                new[] { crumbleVisuals.Stage1Prefab, crumbleVisuals.Stage2Prefab, crumbleVisuals.Stage3Prefab }
                    .SelectMany(prefab => prefab.GetComponentsInChildren<Collider>(true)),
                Is.Empty);
            Assert.That(spiral.CrumblingBlocks.Count, Is.EqualTo(19));
            Assert.That(spiral.CrumblingBlocks.All(item => item.StableId.StartsWith("crumble.m04.route.")), Is.True);
            Assert.That(spiral.CrumblingBlocks.All(item => Mathf.Approximately(item.FallDelay, 1f)), Is.True);
            Assert.That(spiral.CrumblingBlocks.All(item => Mathf.Approximately(item.ResetTime, 4f)), Is.True);
            Assert.That(
                spiral.CrumblingBlocks.All(crumble => spiral.Blocks.All(
                    block => Vector3.Distance(block.Pose.Position, crumble.Pose.Position) > 0.01f)),
                Is.True,
                "A mandatory crumble support must replace, not overlap, its normal route block.");
            Assert.That(spiral.MovingBlocks.Single().PathPoints.Count, Is.EqualTo(2));
            Assert.That(
                spiral.MovingBlocks.Single().PathPoints.All(point => spiral.Blocks.All(
                    block => Vector3.Distance(block.Pose.Position, point) > 0.01f)),
                Is.True,
                "The required moving crossing must replace its two normal route supports.");
        }

        [Test]
        public void Phase063_ExposedVisualOnlyDecorationsDoNotPretendToBePlatforms()
        {
            var modules = Resources.LoadAll<ModuleDefinition>("Modules");
            foreach (var module in modules)
            {
                foreach (var decoration in module.Decorations)
                {
                    Assert.That(
                        ModuleSceneController.IsVisualOnlyPlatformImpostor(decoration.VisualRole, decoration.Size),
                        Is.False,
                        $"{module.StableModuleId} decoration '{decoration.StableId}' reads like a landable visual-only platform.");
                }
            }
        }

        [Test]
        public void Phase079_FlowErrorDoesNotPutTriggerOnlyWaterOnTheRequiredRoute()
        {
            var flowError = Resources.Load<ModuleDefinition>("Modules/Module_003_FlowError");
            Assert.That(flowError, Is.Not.Null);
            Assert.That(flowError.WaterVolumes, Is.Empty);
            Assert.That(flowError.MechanicsUsed.Contains(ModuleMechanic.FlowingWater), Is.False);
        }

        [Test]
        public void Phase079_FlowErrorRequiredRouteIsDistinctSupportedAndBronzeBounded()
        {
            var module = Resources.Load<ModuleDefinition>("Modules/Module_003_FlowError");
            var blocks = module.Blocks.ToDictionary(item => item.StableId);
            var restores = module.RestorePoints.ToDictionary(item => item.StableId);
            var boost = module.BoostBlocks.Single();
            var moving = module.MovingBlocks.Single();
            var crumbles = module.CrumblingBlocks.ToDictionary(item => item.StableId);

            Assert.That(module.ContentVersion, Is.EqualTo(7));
            Assert.That(module.Difficulty, Is.EqualTo(ModuleDifficulty.Medium));
            Assert.That(module.RestorePoints.Count, Is.EqualTo(3));
            Assert.That(module.OptionalShortcuts.Count, Is.EqualTo(3));
            Assert.That(module.CrumblingBlocks.Count, Is.EqualTo(3));
            Assert.That(module.CrumblingBlocks.All(item => Mathf.Approximately(item.FallDelay, 1f)), Is.True);
            Assert.That(module.RankThresholds.CalibrationState,
                Is.EqualTo(Avoidance.Gameplay.Ranking.RankCalibrationState.Uncalibrated));
            Assert.That(module.RankThresholds.DesignerNotes, Does.Contain("Samsung S23"));

            var route = new[]
            {
                Support(blocks["m03.start"]),
                Support(blocks["m03.opening.01"]),
                Support(blocks["m03.opening.02"]),
                Support(blocks["m03.opening.03"]),
                Support(blocks["m03.arrival.approach"]),
                Support(blocks["m03.arrival.court"]),
                Support(restores["restore.module-003.arrival-sanctuary"]),
                Support(blocks["m03.crossing.approach"]),
                Support(blocks["m03.moving-setup"]),
                new RouteSupport(moving.StableId + ".entry", new ModulePose(moving.PathPoints[0], Vector3.zero), moving.Size),
                new RouteSupport(moving.StableId + ".exit", new ModulePose(moving.PathPoints[moving.PathPoints.Count - 1], Vector3.zero), moving.Size),
                Support(blocks["m03.motion-exit"]),
                Support(restores["restore.module-003.broken-crossing"]),
                Support(blocks["m03.temple.entry"]),
                Support(crumbles["crumble.m03.01"]),
                Support(crumbles["crumble.m03.02"]),
                Support(crumbles["crumble.m03.03"]),
                Support(blocks["m03.temple.exit"]),
                Support(restores["restore.module-003.collapsed-temple"]),
                Support(blocks["m03.spine.approach"]),
                Support(blocks["m03.boost-runup"]),
                new RouteSupport(boost.StableId, boost.Pose, boost.Size),
                Support(blocks["m03.boost-landing"]),
                Support(blocks["m03.final.01"]),
                Support(blocks["m03.final.02"]),
                Support(blocks["m03.final.03"]),
                Support(blocks["m03.final.04"]),
                Support(blocks["m03.final.05"]),
                Support(blocks["m03.final.06"]),
                new RouteSupport(
                    module.PatchBlock.StableId + ".platform",
                    new ModulePose(
                        module.PatchBlock.Pose.Position + Vector3.down * 0.8f,
                        module.PatchBlock.Pose.EulerAngles),
                    new Vector3(3f, 0.6f, 3f))
            };

            Assert.That(route.Take(10).Select(item => item.StableId), Does.Not.Contain("water.m03.lower-flow"));
            for (var index = 0; index < route.Length - 1; index++)
            {
                var assistedBoostCrossing = route[index].StableId == boost.StableId;
                var carriedMovingCrossing = route[index].StableId == moving.StableId + ".entry"
                    && route[index + 1].StableId == moving.StableId + ".exit";
                if (!assistedBoostCrossing && !carriedMovingCrossing)
                {
                    Assert.That(
                        HorizontalEdgeGap(route[index], route[index + 1]),
                        Is.LessThanOrEqualTo(3.7f),
                        $"{route[index].StableId} -> {route[index + 1].StableId} exceeds the Bronze route envelope");
                }
            }

            var specialPositions = module.RestorePoints.Select(item => item.Pose.Position)
                .Concat(module.BoostBlocks.Select(item => item.Pose.Position))
                .Concat(module.CrumblingBlocks.Select(item => item.Pose.Position))
                .Append(module.PatchBlock.Pose.Position + Vector3.down * 0.8f)
                .ToArray();
            Assert.That(
                module.Blocks.All(block => specialPositions.All(position =>
                    Vector3.Distance(block.Pose.Position, position) > 0.01f)),
                Is.True,
                "Restore, Boost, Crumble, and Patch supports must replace rather than overlap normal blocks.");
        }

        [Test]
        public void Phase081_Module003BoostArcComfortablyReachesTheAuthoredLanding()
        {
            var module = Resources.Load<ModuleDefinition>("Modules/Module_003_FlowError");
            var profile = Resources.Load<MovementProfile>("MovementProfiles/Movement_Default");
            var boost = module.BoostBlocks.Single(item => item.StableId == "boost.m03.first");
            var landing = module.Blocks.Single(item => item.StableId == "m03.boost-landing");
            var launch = JumpBoostMath.ResolveLaunchVelocity(
                boost.LaunchDirection,
                boost.VerticalStrength,
                boost.HorizontalStrength);
            var toLanding = landing.Pose.Position - boost.Pose.Position;
            var horizontalToLanding = Vector3.ProjectOnPlane(toLanding, Vector3.up);
            var launchHorizontal = Vector3.ProjectOnPlane(launch, Vector3.up);

            Assert.That(Vector3.Dot(horizontalToLanding.normalized, launchHorizontal.normalized),
                Is.GreaterThan(0.999f), "Boost arrow must point at the actual Bronze destination.");
            Assert.That(JumpBoostMath.TryEstimateDescendingFlightTime(
                launch.y,
                toLanding.y,
                profile.JumpGravity,
                profile.FallGravity,
                out var flightTime), Is.True);

            var direction = horizontalToLanding.normalized;
            var projectedHalfExtent = Mathf.Abs(direction.x) * landing.Size.x * 0.5f
                + Mathf.Abs(direction.z) * landing.Size.z * 0.5f;
            var targetCenter = horizontalToLanding.magnitude;
            var targetFront = targetCenter - projectedHalfExtent;
            var targetBack = targetCenter + projectedHalfExtent;
            var slowApproachTravel = JumpBoostMath.EstimateAlignedHorizontalTravel(
                launchHorizontal.magnitude, 3f, flightTime);
            var nominalTravel = JumpBoostMath.EstimateAlignedHorizontalTravel(
                launchHorizontal.magnitude, 5f, flightTime);
            var fullRunTravel = JumpBoostMath.EstimateAlignedHorizontalTravel(
                launchHorizontal.magnitude, profile.BaseRunSpeed, flightTime);

            Assert.That(Mathf.Abs(nominalTravel - targetCenter), Is.LessThan(0.8f),
                "A reasonable Bronze approach should reach the useful central landing region.");
            Assert.That(slowApproachTravel, Is.GreaterThan(targetFront + 1.5f),
                "A slower human approach still needs front-edge safety margin.");
            Assert.That(fullRunTravel, Is.LessThan(targetBack - 1.5f),
                "A full normal run must not overshoot the destination.");
        }

        [Test]
        public void Phase081_Module003MovingBridgeProvidesUsefulSafeTransportation()
        {
            var module = Resources.Load<ModuleDefinition>("Modules/Module_003_FlowError");
            var blocks = module.Blocks.ToDictionary(item => item.StableId);
            var moving = module.MovingBlocks.Single(item => item.StableId == "moving.m03.motion-gallery");
            var setup = Support(blocks["m03.moving-setup"]);
            var exit = Support(blocks["m03.motion-exit"]);
            var entry = new RouteSupport(moving.StableId + ".entry",
                new ModulePose(moving.PathPoints[0], Vector3.zero), moving.Size);
            var carried = new RouteSupport(moving.StableId + ".exit",
                new ModulePose(moving.PathPoints[1], Vector3.zero), moving.Size);
            var travelDistance = Vector3.Distance(moving.PathPoints[0], moving.PathPoints[1]);

            Assert.That(travelDistance, Is.GreaterThan(8f));
            Assert.That(travelDistance / moving.Speed, Is.LessThan(3f),
                "Destroyed-bridge ferry should cross promptly without a long wait.");
            Assert.That(HorizontalEdgeGap(setup, entry), Is.LessThan(1.25f),
                "Bronze boarding must be comfortable at the near pier.");
            Assert.That(HorizontalEdgeGap(carried, exit), Is.LessThan(1.25f),
                "Bronze exit must be comfortable at the far pier.");
            Assert.That(HorizontalEdgeGap(setup, exit), Is.GreaterThan(6f),
                "The Moving platform must bridge a gap the ordinary route cannot simply jump.");
            Assert.That(Mathf.Abs(moving.PathPoints[1].x - moving.PathPoints[0].x), Is.GreaterThan(8f));
            Assert.That(moving.PathPoints[1].z, Is.GreaterThan(moving.PathPoints[0].z));
        }

        [Test]
        public void Phase065_Recovery_RemovesFillerDepthClustersFromEarlyModules()
        {
            var firstSteps = Resources.Load<ModuleDefinition>("Modules/Module_001_FirstSteps");
            var movingParts = Resources.Load<ModuleDefinition>("Modules/Module_002_MovingParts");
            var flowError = Resources.Load<ModuleDefinition>("Modules/Module_003_FlowError");

            Assert.That(firstSteps.Decorations.Any(item => item.StableId.StartsWith("m01.depth.")), Is.False);
            Assert.That(movingParts.Decorations.Any(item => item.StableId.StartsWith("m02.depth.")), Is.False);
            Assert.That(flowError.Decorations.Any(item => item.StableId.StartsWith("m03.depth.")), Is.False);
        }

        [Test]
        public void Phase065_Recovery_FlowErrorKeepsOnlyIntentionalCleanLandmarks()
        {
            var flowError = Resources.Load<ModuleDefinition>("Modules/Module_003_FlowError");
            Assert.That(flowError.Decorations, Is.Empty);
            Assert.That(flowError.EnvironmentBiomeProfile.WorldObjects.Count, Is.EqualTo(9));
        }

        [Test]
        public void Phase058_SpiralBoostPointsAtReachableGapCloserLanding()
        {
            var spiral = Resources.Load<ModuleDefinition>("Modules/Module_004_TheSpiral");
            var boost = spiral.BoostBlocks.Single(item => item.StableId == "boost.m04.first-ascent");
            var shortcut = spiral.OptionalShortcuts.Single(item => item.StableId == "shortcut.module-004.boost-overshoot");
            var toLanding = shortcut.ExitPosition - boost.Pose.Position;
            var horizontalToLanding = Vector3.ProjectOnPlane(toLanding, Vector3.up);
            var horizontalLaunch = Vector3.ProjectOnPlane(boost.LaunchDirection, Vector3.up);

            Assert.That(horizontalToLanding.magnitude, Is.InRange(8f, 11.5f));
            Assert.That(Mathf.Abs(toLanding.y), Is.LessThan(1.5f));
            Assert.That(Vector3.Dot(horizontalToLanding.normalized, horizontalLaunch.normalized), Is.GreaterThan(0.995f));
            Assert.That(shortcut.DisplayName, Is.EqualTo("Boost gap closer"));
        }

        [Test]
        public void Phase057_ModularTilingRemainsOnlyAsExplicitHugeSurfaceFallback()
        {
            var library = Resources.Load<ModuleVisualPrefabLibrary>(ModuleVisualPrefabLibrary.ResourceName);
            var huge = library.PlacementFor(ModuleMaterialRole.Normal, new Vector3(8f, 0.6f, 8f));

            Assert.That(huge.FitMode, Is.EqualTo(VisualFitMode.ModularTile));
            Assert.That(
                ModuleVisualPrefabLibrary.ResolveModularTileCounts(new Vector3(8f, 0.6f, 8f)),
                Is.EqualTo(new Vector2Int(6, 6)));
        }

        private sealed class FakeInput : IPlayerInputSource
        {
            public Vector2 MoveValue;
            public Vector2 Move => MoveValue;
            public Vector2 LookDelta => Vector2.zero;
            public bool JumpPressed => false;
            public void ResetState() { }
        }

        private static RouteSupport Support(ModuleBlockDefinition block) =>
            new RouteSupport(block.StableId, block.Pose, block.Size);

        private static RouteSupport Support(ModuleRestorePointDefinition restore) =>
            new RouteSupport(restore.StableId, restore.Pose, new Vector3(3f, 0.6f, 3f));

        private static RouteSupport Support(ModuleCrumblingBlockDefinition crumble) =>
            new RouteSupport(crumble.StableId, crumble.Pose, crumble.Size);

        private static float HorizontalEdgeGap(RouteSupport first, RouteSupport second)
        {
            var delta = Vector3.ProjectOnPlane(second.Pose.Position - first.Pose.Position, Vector3.up);
            if (delta.sqrMagnitude <= 0.0001f)
            {
                return 0f;
            }

            var direction = delta.normalized;
            var firstRadius = first.Size.x * 0.5f * Mathf.Abs(direction.x)
                + first.Size.z * 0.5f * Mathf.Abs(direction.z);
            var secondRadius = second.Size.x * 0.5f * Mathf.Abs(direction.x)
                + second.Size.z * 0.5f * Mathf.Abs(direction.z);
            return Mathf.Max(0f, delta.magnitude - firstRadius - secondRadius);
        }
    }
}
