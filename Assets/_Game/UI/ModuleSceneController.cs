using System;
using System.Collections.Generic;
using System.Linq;
using Avoidance.Core.Configuration;
using Avoidance.Core.Services;
using Avoidance.Gameplay.Audio;
using Avoidance.Gameplay.Blocks;
using Avoidance.Gameplay.Camera;
using Avoidance.Gameplay.Checkpoints;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Ranking;
using Avoidance.Gameplay.Respawn;
using Avoidance.Gameplay.Timing;
using Avoidance.Gameplay.Visuals;
using Avoidance.Gameplay.Worlds;
using Avoidance.Input;
using Avoidance.SaveSystem;
using Avoidance.UI.Touch;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Avoidance.UI
{
    [DisallowMultipleComponent]
    public sealed class ModuleSceneController : MonoBehaviour
    {
        private readonly List<IModuleResettable> _moduleResettables =
            new List<IModuleResettable>();
        private readonly List<GameObject> _debugLabels = new List<GameObject>();
        private readonly Dictionary<ModuleMaterialRole, Material> _particleMaterials =
            new Dictionary<ModuleMaterialRole, Material>();
        private readonly List<VisualBenchmarkAnchor> _visualBenchmarks =
            new List<VisualBenchmarkAnchor>();
        private static Sprite _circleRingSprite;
        private static Sprite _filledCircleSprite;
        public static readonly string[] RequiredVisualBenchmarkNames =
        {
            "Benchmark_Module01_Start",
            "Benchmark_Module02_Mid",
            "Benchmark_Module03_Opening",
            "Benchmark_Module03_Early",
            "Benchmark_Module03_Middle",
            "Benchmark_Module03_High",
            "Benchmark_Module03_Patch",
            "Benchmark_Spiral_Start",
            "Benchmark_Spiral_Mid",
            "Benchmark_Spiral_Summit"
        };
        public static readonly string[] OptionalVisualBenchmarkNames =
        {
            "Benchmark_Spiral_Low",
            "Benchmark_Spiral_Restore",
            "Benchmark_Spiral_Boost",
            "Benchmark_Spiral_Surf",
            "Benchmark_Spiral_High"
        };

        private ModuleDefinition _module;
        private ModuleVisualProfile _visuals;
        private ModuleVisualPrefabLibrary _prefabLibrary;
        private CrumbleVisualSet _crumbleVisuals;
        private ModuleEnvironmentProfile _environment;
        private GameplayVfxService _gameplayVfx;
        private EnvironmentBiomeProfile _biome;
        private RouteCameraGraph _routeCameraGraph;
        private ModuleAttemptTelemetry _telemetry;
        private ModuleRunSession _runSession;
        private ISaveService _save;
        private ISettingsService _settings;
        private ModuleProgressRecord _progressRecord;
        private readonly Dictionary<string, GameObject> _authoredSupportObjects = new Dictionary<string, GameObject>();
        private ModuleHud _hud;
        private IDiagnosticsService _diagnostics;
        private bool _debugLabelsVisible;
        private float _diagnosticTimer;
        private int _visualObjectCount;
        private int _particleEmitterCount;
        private float _performanceDiagnosticTimer;
        private int _rendererCount;
        private int _shadowCasterCount;
        private int _realtimeLightCount;
        private int _colliderCount;
        private int _activeRigidbodyCount;
        private int _activeParticleSystemCount;
        private int _benchmarkIndex;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public ModuleDefinition ActiveModule => _module;
        private float _constructionBudgetSeconds;
        private bool _ready;

        private void Awake()
        {
            // Preserve immediate identity discovery while construction is incremental.
            _module = LoadSelectedModule();
            MusicDirector.SetContext(_module.StableModuleId);
            _constructionBudgetSeconds = (Resources.Load<LoadingTransitionProfile>("LoadingTransitionProfile")?.ConstructionBudgetMilliseconds ?? 4f) * .001f;
        }

        private System.Collections.IEnumerator Start()
        {
            var configuration = Resources.Load<GameConfiguration>("FoundationGameConfiguration")
                ?? GameConfiguration.CreateRuntimeDefault();
            _visuals = _module.VisualProfile != null
                ? _module.VisualProfile
                : ModuleVisualProfile.CreateRuntimeDefault();
            _prefabLibrary = _visuals.PrefabLibrary != null ? _visuals.PrefabLibrary : Resources.Load<ModuleVisualPrefabLibrary>(ModuleVisualPrefabLibrary.ResourceName)
                ?? ModuleVisualPrefabLibrary.CreateRuntimeDefault();
            _crumbleVisuals = Resources.Load<CrumbleVisualSet>(CrumbleVisualSet.ResourceName);
            _environment = _module.EnvironmentProfile != null
                ? _module.EnvironmentProfile
                : ModuleEnvironmentProfile.CreateRuntimeDefault();
            _biome = _module.EnvironmentBiomeProfile;
            _gameplayVfx = gameObject.AddComponent<GameplayVfxService>();
            _gameplayVfx.Initialize(_visuals, _environment, () => _particleEmitterCount++);

            if (GameServices.Current != null)
            {
                GameServices.Current.TryGet<IDiagnosticsService>(out _diagnostics);
                GameServices.Current.TryGet<ISaveService>(out _save);
                GameServices.Current.TryGet<ISettingsService>(out _settings);
            }

            yield return null;
            CreateEnvironment();
            yield return CreateBiomeWorld();
            _routeCameraGraph = CreateRouteCameraGraph();
            CreateRouteCameraDebugView(_routeCameraGraph);
            var checkpointService = ResolveCheckpointService();
            var start = ModuleStartSafety.Resolve(_module);
            if (start.UsedSafetyCorrection)
            {
                Debug.LogError(
                    $"{_module.StartAnchorStableId}: {start.Diagnostic} "
                    + "Fix the module asset; runtime correction prevented an unsafe spawn.");
            }

            checkpointService.SetStart(start.Pose.Position, start.Pose.Rotation);
            yield return CreateModuleContent(checkpointService);
            var touchInput = CreateTouchInterface(out var statusText, out var completionText);
            CreatePlayer(
                start.Pose.Position,
                start.Pose.Rotation,
                LoadMovementProfiles(),
                Resources.Load<CameraProfile>("Camera_Default")
                    ?? CameraProfile.CreateRuntimeDefault(),
                _routeCameraGraph,
                touchInput,
                checkpointService,
                configuration.BuildVersion,
                statusText,
                completionText);
            _ready = true;
            UnitySceneLevelLoader.NotifyReady(gameObject.scene.name, _module.StableModuleId);
        }

        private void Update()
        {
            if (!_ready) return;
            _telemetry?.Tick(Time.unscaledDeltaTime);
            _runSession?.Timer.Tick(Time.unscaledDeltaTime);
            HandleBenchmarkWarp();
            PublishDiagnostics(Time.unscaledDeltaTime);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                _telemetry?.Export();
            }
        }

        private ModuleDefinition LoadSelectedModule()
        {
            var modules = Resources.LoadAll<ModuleDefinition>("Modules");
            var selected = modules.FirstOrDefault(
                module => module.StableModuleId == ModuleSelectionState.SelectedModuleId)
                ?? modules.FirstOrDefault(
                    module => module.StableModuleId == ModuleSelectionState.DefaultModuleId)
                ?? modules.OrderBy(module => module.StableModuleId).FirstOrDefault();
            if (selected == null)
            {
                throw new InvalidOperationException(
                    "No ModuleDefinition assets were found in Resources/Modules.");
            }

            return selected;
        }

        private static MovementProfileSet LoadMovementProfiles()
        {
            if (CampaignFlowTrial.Active)
                return new MovementProfileSet(new[] { Resources.Load<MovementProfile>(CampaignFlowTrial.MovementResource) });
            return new MovementProfileSet(new[] { MovementProfile.LoadShared() });
        }

        private static int ProfileOrder(MovementProfile profile)
        {
            if (profile == null)
            {
                return int.MaxValue;
            }

            switch (profile.ProfileId)
            {
                case "movement.default": return 0;
                case "movement.forgiving": return 1;
                case "movement.precise": return 2;
                case "movement.experimental": return 3;
                default: return 100;
            }
        }

        private static CheckpointService ResolveCheckpointService()
        {
            if (GameServices.Current != null
                && GameServices.Current.TryGet<ICheckpointService>(out var service)
                && service is CheckpointService checkpointService)
            {
                checkpointService.Reset();
                return checkpointService;
            }

            return new CheckpointService();
        }

        private void CreateEnvironment()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = _environment.OverrideBiomeFog ? _environment.FogColor : (_biome != null ? _biome.HazeColor : _environment.FogColor);
            RenderSettings.fogDensity = _environment.OverrideBiomeFog ? _environment.FogDensity : (!_environment.AuthoredLighting && ShouldUseGoldSlicePresentation())
                ? 0.0055f
                : (_biome != null ? 0.0075f : _environment.FogDensity);
            if (_environment.AuthoredLighting) RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = (!_environment.AuthoredLighting && ShouldUseGoldSlicePresentation())
                ? new Color(0.52f, 0.58f, 0.74f)
                : _environment.AmbientLight;
            RenderSettings.skybox = CreateProceduralSkybox();

            var sunObject = new GameObject("Module Sun", typeof(Light));
            sunObject.transform.rotation = _environment.AuthoredLighting ? Quaternion.Euler(_environment.SunEuler) : (!_environment.AuthoredLighting && ShouldUseGoldSlicePresentation())
                ? Quaternion.Euler(42f, -46f, 0f)
                : Quaternion.Euler(48f, -32f, 0f);
            var sun = sunObject.GetComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = (!_environment.AuthoredLighting && ShouldUseGoldSlicePresentation())
                ? Mathf.Max(_environment.SunIntensity, 1.86f)
                : _environment.SunIntensity;
            sun.color = _environment.AuthoredLighting ? _environment.SunLight : ShouldUseGoldSlicePresentation()
                ? new Color(1f, 0.88f, 0.68f)
                : (_biome != null ? Color.Lerp(_environment.SunLight, _biome.AtmosphereTint, 0.25f) : _environment.SunLight);
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.35f;

            CreateMobilePresentationVolume();
            CreateAmbientClouds();
            if (_environment.AmbienceClip != null)
                new GameObject("World Air").AddComponent<WorldAmbience>().Initialize(_environment.AmbienceClip, _environment.AmbienceGain);
            CreateDistantFragments();
            CreateVisualBenchmarks();

            foreach (var decoration in _module.Decorations)
            {
                CreateDecoration(decoration);
            }
        }

        private System.Collections.IEnumerator CreateModuleContent(CheckpointService checkpoints)
        {
            var budgetStart = Time.realtimeSinceStartup;
            for (var index = 0; index < _module.Blocks.Count; index++)
            {
                var block = _module.Blocks[index];
                var blockObject = CreateGameplayBlock(block.StableId, block.Pose, block.Size, block.VisualRole);
                _authoredSupportObjects.Add(block.StableId, blockObject);
                if (Time.realtimeSinceStartup - budgetStart > ConstructionBudgetSeconds)
                { yield return null; budgetStart = Time.realtimeSinceStartup; }
                if (block.EditorLabel)
                {
                    CreateWorldLabel(block.Label, block.Pose.Position + Vector3.up * 1.2f, debugOnly: true);
                }
            }

            foreach (var moving in _module.MovingBlocks)
            {
                var block = CreateGameplayBlock(
                    moving.StableId,
                    moving.Pose,
                    moving.Size,
                    ModuleMaterialRole.Moving);
                var motion = block.AddComponent<MovingBlock>();
                block.AddComponent<MechanismAudio>();
                motion.Configure(moving);
                _moduleResettables.Add(motion);
            }

            foreach (var water in _module.WaterVolumes)
            {
                CreateWater(water);
            }

            foreach (var surf in _module.SurfSurfaces)
            {
                CreateSurf(surf);
            }

            foreach (var boost in _module.BoostBlocks)
            {
                CreateBoost(boost);
            }

            foreach (var crumbling in _module.CrumblingBlocks)
            {
                CreateCrumbling(crumbling);
            }

            foreach (var restore in _module.RestorePoints)
            {
                CreateRestorePoint(checkpoints, restore);
            }

            foreach (var shortcut in _module.OptionalShortcuts)
            {
                CreateShortcutTrigger(shortcut);
            }

            foreach (var hint in _module.CameraHints)
            {
                CreateCameraHint(hint);
            }
        }

        private void CreatePlayer(
            Vector3 startPosition,
            Quaternion startRotation,
            MovementProfileSet profiles,
            CameraProfile cameraProfile,
            RouteCameraGraph routeCameraGraph,
            TouchInputCoordinator touchInput,
            CheckpointService checkpoints,
            string buildVersion,
            Text statusText,
            Text completionText)
        {
            var player = new GameObject(
                "Player",
                typeof(CharacterController),
                typeof(ParkourMotor),
                typeof(CrumblingBlockContactRelay),
                typeof(AudioSource),
                typeof(MovementFeedback),
                typeof(RestoreController),
                typeof(SceneryLandingRecovery),
                typeof(MovementSessionReporter),
                typeof(PlayerRuntimeCoordinator));
            player.layer = LayerMask.NameToLayer("Player");
            player.transform.SetPositionAndRotation(startPosition, startRotation);
            var character = player.GetComponent<CharacterController>();
            character.height = 1.8f;
            character.radius = 0.38f;
            character.center = new Vector3(0f, 0.9f, 0f);
            character.skinWidth = 0.045f;
            character.stepOffset = 0.32f;
            character.slopeLimit = 52f;
            var cameraObject = new GameObject(
                "First Person Camera",
                typeof(UnityEngine.Camera),
                typeof(AudioListener),
                typeof(FirstPersonCameraRig),
                typeof(FirstPersonHands));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(player.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            var camera = cameraObject.GetComponent<UnityEngine.Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.backgroundColor = _environment.SkyZenith;
            camera.nearClipPlane = 0.04f;

            var motor = player.GetComponent<ParkourMotor>();
            motor.Initialize(profiles.Current);
            player.GetComponent<CrumblingBlockContactRelay>().Initialize(motor);
            var cameraRig = cameraObject.GetComponent<FirstPersonCameraRig>();
            cameraRig.Initialize(
                player.transform,
                camera,
                cameraProfile,
                ParkourCameraProfile.CreateRuntimeDefault(),
                routeCameraGraph);
            if (motor.Profile.MovementFoundation)
            {
                touchInput.EnableFoundationJumpLook();
            }
            else if (CampaignFlowTrial.UsesCandidate)
            {
                touchInput.SetSessionControlProfile(TouchControlProfileKind.FlowSteerAutoDirect);
                if (CampaignFlowTrial.Mode == CampaignTrialMode.FlowLanding)
                {
                    var training = JsonUtility.FromJson<FlowLabDefinition>(Resources.Load<TextAsset>("Training/FlowLab").text);
                    cameraRig.ConfigureLandingView(training.landingView);
                    cameraRig.ResetView(startRotation);
                }
            }
            var hands = cameraObject.GetComponent<FirstPersonHands>();
            if (CampaignFlowTrial.Active)
                player.AddComponent<MovementTrialTrace>().Configure(motor, _module.StableModuleId + "." + CampaignFlowTrial.Mode);
            hands.Initialize(motor);

            var input = new PlayerInputRouter(touchInput);
            var stats = new MovementSessionStats();
            var feedback = player.GetComponent<MovementFeedback>();
            player.AddComponent<GroundTravelAudio>().Initialize(motor, feedback);
            player.AddComponent<AndroidGameplayHaptics>();
            player.AddComponent<MomentumPresentation>().Initialize(motor, feedback, _gameplayVfx, cameraObject.transform);
            var challenge = gameObject.AddComponent<FlowChallenge>();
            var challengeProfile = Resources.LoadAll<FlowChallengeProfile>("FlowChallenges")
                .FirstOrDefault(item => item.ModuleId == _module.StableModuleId);
            challenge.Initialize(challengeProfile, feedback, _gameplayVfx);
            challenge.Changed += (count, total) => _hud?.ShowFlowPickup(count, total);
            var reporter = player.GetComponent<MovementSessionReporter>();
            reporter.Initialize(stats, input, motor, buildVersion);
            _telemetry = new ModuleAttemptTelemetry();
            _telemetry.Initialize(_module, input, motor, buildVersion);
            InitializeRunSession(motor.Profile);
            motor.Jumped += _telemetry.RecordJump;
            motor.Landed += speed => _gameplayVfx?.PlayLanding(player.transform.position, speed);

            var restoreController = player.GetComponent<RestoreController>();
            restoreController.Initialize(
                motor,
                cameraRig,
                input,
                checkpoints,
                null,
                _moduleResettables.ToArray(),
                _ =>
                {
                    stats.RecordRestore();
                    _telemetry.RecordRestore();
                    feedback.PlayRestore();
                    _gameplayVfx?.PlayRestore(player.transform.position);
                },
                () =>
                {
                    stats.RecordFall();
                    _telemetry.RecordFall(player.transform.position);
                    feedback.PlayFall();
                },
                _module.ResolveFallThreshold(_environment.NullSpaceY));

            var coordinator = player.GetComponent<PlayerRuntimeCoordinator>();
            coordinator.Initialize(
                input,
                touchInput,
                profiles,
                motor,
                cameraRig,
                restoreController,
                feedback,
                stats,
                reporter,
                checkpoints,
                _diagnostics);

            _hud = statusText.GetComponentInParent<Canvas>().gameObject.AddComponent<ModuleHud>();
            _hud.Initialize(
                statusText,
                completionText,
                _module,
                coordinator,
                restoreController,
                _telemetry,
                _runSession,
                _progressRecord,
                buildVersion,
                RestartModule,
                NextModule,
                ModuleSelect);
            CreatePatchBlock(feedback);
        }

        public static bool ShouldCreateRouteFlowCue(ModuleMaterialRole role, Vector3 size)
        {
            return false;
        }

        public static bool IsVisualOnlyPlatformImpostor(ModuleMaterialRole role, Vector3 size)
        {
            var maximumPlatformLikeHeight = role == ModuleMaterialRole.Water ? 1.6f : 0.75f;
            var broadFlatSurface = size.y <= maximumPlatformLikeHeight
                && Mathf.Min(size.x, size.z) >= 1.2f
                && Mathf.Max(size.x, size.z) >= 2.4f;
            if (!broadFlatSurface)
            {
                return false;
            }

            switch (role)
            {
                case ModuleMaterialRole.Normal:
                case ModuleMaterialRole.Precision:
                case ModuleMaterialRole.Moving:
                case ModuleMaterialRole.Boost:
                case ModuleMaterialRole.Water:
                case ModuleMaterialRole.WaterFoam:
                case ModuleMaterialRole.MovingAccent:
                case ModuleMaterialRole.CircuitLine:
                case ModuleMaterialRole.SurfaceHighlight:
                case ModuleMaterialRole.SurfaceInset:
                case ModuleMaterialRole.Surf:
                    return true;
                default:
                    return false;
            }
        }

        private void InitializeRunSession(MovementProfile movementProfile)
        {
            var progression = CampaignFlowTrial.Active ? new ProgressionData() : EnsureProgression();
            _progressRecord = ModuleProgressionData.GetVersionedBest(progression, _module.StableModuleId,
                _module.ContentVersion, movementProfile.CompatibilityVersion, _module.RankThresholds.ThresholdVersion);
            ModuleProgressionData.RecordAttempt(progression, _module.StableModuleId);
            if (!CampaignFlowTrial.Active) _save?.Save();

            var validity = ModuleSelectionState.DevelopmentOverride
                ? RunValidity.InvalidDevelopment
                : RunValidity.ValidUnassisted;
            _runSession = new ModuleRunSession();
            _runSession.Begin(
                _module,
                validity,
                ToRunSplits(_progressRecord?.bestSplits));
        }

        private ProgressionData EnsureProgression()
        {
            if (_save == null || CampaignFlowTrial.Active)
            {
                return new ProgressionData();
            }

            _save.Current.progression ??= new ProgressionData();
            ModuleProgressionData.EnsureArrays(_save.Current.progression);
            return _save.Current.progression;
        }

        private void CompleteModuleRun(MovementFeedback feedback)
        {
            if (_runSession == null)
            {
                return;
            }

            var movementVersion = 0;
            var player = FindAnyObjectByType<ParkourMotor>();
            if (player != null && player.Profile != null)
            {
                movementVersion = player.Profile.CompatibilityVersion;
            }

            var nextCampaignId = ModuleSelectionState.GetNextCampaignModuleId(_module.StableModuleId);
            var wasNextUnlocked = nextCampaignId != null && ModuleProgressionData.IsUnlocked(EnsureProgression(), ModuleSelectionState.GetCampaignModuleIds(), nextCampaignId);
            var result = _runSession.Complete(movementVersion);
            var previousBest = _progressRecord == null ? 0d : _progressRecord.bestTimeSeconds;
            Enum.TryParse<ModuleRank>(_progressRecord?.highestRank, out var previousRank);
            result.IsNewPersonalBest = false;
            result.PersonalBestDeltaSeconds = 0d;
            if (_save != null && !CampaignFlowTrial.Active)
            {
                var splits = ToSaveSplits(result.Splits);
                var utcNow = DateTime.UtcNow.ToString("O");
                result.IsNewPersonalBest = ModuleProgressionData.RecordVersionedCompletion(
                    EnsureProgression(),
                    result.ModuleId,
                    result.CompletionSeconds,
                    result.Rank.ToString(),
                    result.Score,
                    splits,
                    ModuleRankUtility.IsValidForPersonalBest(result.Validity),
                    result.ModuleContentVersion,
                    result.MovementCompatibilityVersion,
                    result.RankThresholdVersion,
                    result.RankCalibrationState,
                    result.Validity.ToString(),
                    utcNow);
                result.PersonalBestDeltaSeconds = result.IsNewPersonalBest && previousBest > 0d
                    ? result.CompletionSeconds - previousBest
                    : 0d;
                _progressRecord = ModuleProgressionData.GetVersionedBest(_save.Current.progression, result.ModuleId,
                    result.ModuleContentVersion, result.MovementCompatibilityVersion, result.RankThresholdVersion);
                ModuleCompletionRewardUtility.ApplyCompletionRewards(
                    _module,
                    _save.Current,
                    result.Rank,
                    result.Validity,
                    utcNow,
                    out var rewardLines);
                result.RewardLines = rewardLines;
                _save.Save();
            }

            if (!wasNextUnlocked && nextCampaignId != null && ModuleProgressionData.IsUnlocked(EnsureProgression(), ModuleSelectionState.GetCampaignModuleIds(), nextCampaignId))
                result.NewlyUnlockedModuleId = nextCampaignId;
            result.PersonalBestSeconds = _progressRecord != null ? _progressRecord.bestTimeSeconds : 0d;
            _telemetry?.ApplyRunResult(result);
            if (ModuleRankUtility.IsValidForPersonalBest(result.Validity))
                StartCoroutine(PlayAchievementFeedback(feedback, result, result.Rank > previousRank));

            if (CampaignFlowTrial.Active) CampaignFlowTrial.Complete(_module, result.CompletionSeconds);
            _hud?.ShowResults(result);
        }

        private System.Collections.IEnumerator PlayAchievementFeedback(MovementFeedback feedback, ModuleRunResult result, bool improvedRank)
        {
            yield return new WaitForSecondsRealtime(0.25f);
            if (result.IsNewPersonalBest) feedback?.PlayPersonalBest();
            else if (improvedRank)
            {
                if (result.Rank == ModuleRank.Diamond) feedback?.PlayDiamond(); else feedback?.PlayRank();
            }
        }

        private void NextModule()
        {
            var nextId = ModuleSelectionState.GetNextCampaignModuleId(_module.StableModuleId);
            if (nextId != null && _runSession?.Result != null
                && ModuleRankUtility.IsValidForPersonalBest(_runSession.Result.Validity))
            {
                ModuleSelectionState.Select(nextId);
                StartCoroutine(LoadScene(ModuleSelectionState.ModuleRunnerSceneName));
                return;
            }

            ModuleSelect();
        }

        private void ModuleSelect()
        {
            StartCoroutine(LoadScene(ModuleSelectionState.ModuleSelectorSceneName));
        }

        private static RunSplit[] ToRunSplits(ModuleSplitRecord[] records)
        {
            if (records == null || records.Length == 0)
            {
                return Array.Empty<RunSplit>();
            }

            return records
                .Where(record => record != null)
                .Select(record => new RunSplit
                {
                    checkpointId = record.checkpointId,
                    seconds = record.seconds
                })
                .ToArray();
        }

        private static ModuleSplitRecord[] ToSaveSplits(RunSplit[] splits)
        {
            if (splits == null || splits.Length == 0)
            {
                return Array.Empty<ModuleSplitRecord>();
            }

            return splits
                .Where(split => split != null)
                .Select(split => new ModuleSplitRecord
                {
                    checkpointId = split.checkpointId,
                    seconds = split.seconds
                })
                .ToArray();
        }

        private GameObject CreateGameplayBlock(
            string objectName,
            ModulePose pose,
            Vector3 size,
            ModuleMaterialRole role)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = objectName;
            block.transform.SetPositionAndRotation(pose.Position, pose.Rotation);
            block.transform.localScale = size;
            block.layer = LayerMask.NameToLayer("Ground");
            block.GetComponent<Renderer>().sharedMaterial = _visuals.MaterialFor(role);
            if (role == ModuleMaterialRole.Crumbling && _crumbleVisuals != null && _crumbleVisuals.IsComplete)
            {
                block.GetComponent<Renderer>().enabled = false;
                _visualObjectCount++;
                return block;
            }

            var hasPrefabVisual = ApplyVisualPrefabOverride(block, size, role);
            if (!hasPrefabVisual)
            {
                AddReadabilityTrim(block, size);
                AddRoleDetails(block, size, role);
            }
            else
            {
                if (role == ModuleMaterialRole.Crumbling)
                {
                    AddRoleDetails(block, size, role);
                }
            }

            _visualObjectCount++;
            return block;
        }

        private bool ApplyVisualPrefabOverride(GameObject block, Vector3 blockSize, ModuleMaterialRole role)
        {
            if (block == null || _prefabLibrary == null)
            {
                return false;
            }

            var placement = _prefabLibrary.PlacementFor(role, blockSize);
            if (!placement.IsValid)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (_prefabLibrary.HasPrefabFor(role))
                {
                    Debug.LogWarning($"[VisualRole] No prefab placement fits role '{role}' on block '{block.name}' with size {blockSize}.");
                }
#endif
                return false;
            }

            var tileCounts = ModuleVisualPrefabLibrary.ResolveModularTileCounts(blockSize);
            var countX = tileCounts.x;
            var countZ = tileCounts.y;

            if (placement.FitMode == VisualFitMode.ModularTile && (countX > 1 || countZ > 1))
            {
                var standardPrefab = placement.Prefab;
                if (standardPrefab != null)
                {
                    var stepX = blockSize.x / countX;
                    var stepZ = blockSize.z / countZ;
                    var startX = -blockSize.x * 0.5f + stepX * 0.5f;
                    var startZ = -blockSize.z * 0.5f + stepZ * 0.5f;

                    for (var ix = 0; ix < countX; ix++)
                    {
                        for (var iz = 0; iz < countZ; iz++)
                        {
                            var tile = Instantiate(standardPrefab, block.transform);
                            tile.name = $"{role} Tile [{ix},{iz}]";
                            tile.transform.localPosition = new Vector3(
                                (startX + ix * stepX) / blockSize.x,
                                0.5f,
                                (startZ + iz * stepZ) / blockSize.z);
                            tile.transform.localRotation = Quaternion.identity;
                            tile.transform.localScale = new Vector3(
                                stepX / blockSize.x,
                                1.5f / blockSize.y,
                                stepZ / blockSize.z);
                            ModuleVisualPrefabLibrary.PrepareVisualInstance(tile);
                        }
                    }

                    block.GetComponent<Renderer>().enabled = false;
                    _visualObjectCount += countX * countZ;
                    return true;
                }
            }

            var visual = Instantiate(placement.Prefab, block.transform);
            visual.name = role + " Visual";
            visual.transform.localPosition = placement.FitMode == VisualFitMode.TopAlignedUniform
                ? placement.LocalPosition + Vector3.up * 0.5f
                : placement.LocalPosition;
            visual.transform.localRotation = Quaternion.Euler(placement.LocalEulerAngles);
            if (placement.FitMode == VisualFitMode.TopAlignedUniform)
            {
                visual.transform.localScale =
                    ModuleVisualPrefabLibrary.ResolveTopAlignedUniformLocalScale(blockSize);
            }
            else if (placement.FitMode == VisualFitMode.ExactFootprint)
            {
                visual.transform.localScale =
                    ModuleVisualPrefabLibrary.ResolveExactFootprintLocalScale(
                        placement.LocalScale,
                        new Vector3(3f, 0.6f, 3f));
            }
            else
            {
                var targetScale = placement.LocalScale;
                visual.transform.localScale = new Vector3(
                    Mathf.Max(0.01f, targetScale.x / Mathf.Max(0.01f, blockSize.x)),
                    Mathf.Max(0.01f, targetScale.y / Mathf.Max(0.01f, blockSize.y)),
                    Mathf.Max(0.01f, targetScale.z / Mathf.Max(0.01f, blockSize.z)));
            }
            if (placement.FitMode == VisualFitMode.ExactFootprint)
                ModuleVisualPrefabLibrary.FitExactBounds(visual.transform, block.transform);
            ModuleVisualPrefabLibrary.PrepareVisualInstance(visual);
            block.GetComponent<Renderer>().enabled = false;
            _visualObjectCount++;
            return true;
        }

        private void CreateBoost(ModuleBoostBlockDefinition boost)
        {
            var block = CreateGameplayBlock(
                boost.StableId,
                boost.Pose,
                boost.Size,
                ModuleMaterialRole.Boost);
            var boostBlock = block.AddComponent<JumpBoostBlock>();
            boostBlock.Initialize(
                boost.LaunchDirection,
                boost.VerticalStrength,
                boost.HorizontalStrength,
                boost.Cooldown,
                position =>
                {
                    _telemetry?.RecordBoost();
                    FindAnyObjectByType<FirstPersonHands>()?.NotifyBoost();
                    _gameplayVfx?.PlayBoost(position, boost.LaunchDirection);
                });
            _moduleResettables.Add(boostBlock);

            var trigger = new GameObject("Boost Trigger", typeof(BoxCollider), typeof(JumpBoostTrigger));
            trigger.transform.SetParent(block.transform, false);
            trigger.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            trigger.transform.localScale = new Vector3(1f, 0.5f, 1f);
            trigger.GetComponent<BoxCollider>().isTrigger = true;
            trigger.GetComponent<JumpBoostTrigger>().Initialize(boostBlock);
            // Direction marks are part of the pad skin; no opaque bars over the launch view.

        }

        private void CreateWater(ModuleWaterVolumeDefinition water)
        {
            var volume = new GameObject(water.StableId, typeof(BoxCollider), typeof(WaterFlowVolume));
            volume.transform.SetPositionAndRotation(water.Pose.Position, water.Pose.Rotation);
            volume.transform.localScale = water.Size;
            volume.GetComponent<BoxCollider>().isTrigger = true;
            CreateWaterVolumeVisuals(volume.transform, water.FlowDirection, water.Size);

            var component = volume.GetComponent<WaterFlowVolume>();
            component.Initialize(
                water.FlowDirection,
                water.FlowForce,
                water.MaxAddedVelocity,
                water.VerticalInfluence,
                water.Drag,
                position =>
                {
                    _telemetry?.RecordWaterPulse();
                    FindAnyObjectByType<FirstPersonHands>()?.NotifyWaterPulse();
                });
        }

        private void CreateSurf(ModuleSurfSurfaceDefinition surf)
        {
            var block = CreateGameplayBlock(
                surf.StableId,
                surf.Pose,
                surf.Size,
                ModuleMaterialRole.Surf);
            block.AddComponent<SurfSurface>();
            block.AddComponent<RuntimeVisualPulse>().Initialize(
                _visuals.ColorFor(ModuleMaterialRole.Surf),
                _visuals.ColorFor(ModuleMaterialRole.WaterFoam),
                3.8f,
                0.2f);
            CreateArrowPlate(block.transform, ModuleMaterialRole.WaterFoam, surf.FlowDirection);
            CreateVerticalStreaks(block.transform, ModuleMaterialRole.WaterFoam, 4);
        }

        private void CreateCrumbling(ModuleCrumblingBlockDefinition crumbling)
        {
            var block = CreateGameplayBlock(
                crumbling.StableId,
                crumbling.Pose,
                crumbling.Size,
                ModuleMaterialRole.Crumbling);
            var component = block.AddComponent<CrumblingBlock>();
            CreateCrumbleStageVisuals(
                block,
                crumbling.Size,
                out var visualRoot,
                out var stage1,
                out var stage2,
                out var stage3);
            component.Initialize(
                crumbling.ActivationDelay,
                crumbling.FallDelay,
                crumbling.ResetTime,
                crumbling.ShakeAmount,
                visualRoot,
                stage1,
                stage2,
                stage3,
                position => _gameplayVfx?.PlayCrumbleWarning(position),
                position => { _gameplayVfx?.PlayCrumbleCollapse(position); FindAnyObjectByType<ParkourMotor>()?.GetComponent<MovementFeedback>()?.PlayCue(GameplayAudioCue.CrumbleCollapse); });
            _moduleResettables.Add(component);

            var trigger = new GameObject(
                "Crumbling Trigger",
                typeof(BoxCollider),
                typeof(CrumblingBlockTrigger));
            trigger.transform.SetParent(block.transform, false);
            trigger.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            trigger.transform.localScale = new Vector3(1f, 0.35f, 1f);
            trigger.GetComponent<BoxCollider>().isTrigger = true;
            trigger.GetComponent<CrumblingBlockTrigger>().Initialize(component);
        }

        private void CreateCrumbleStageVisuals(
            GameObject block,
            Vector3 blockSize,
            out Transform visualRoot,
            out GameObject stage1,
            out GameObject stage2,
            out GameObject stage3)
        {
            visualRoot = null;
            stage1 = null;
            stage2 = null;
            stage3 = null;
            if (block == null || _crumbleVisuals == null || !_crumbleVisuals.IsComplete)
            {
                return;
            }

            var root = new GameObject("Crumble Stage Visuals");
            root.transform.SetParent(block.transform, false);
            visualRoot = root.transform;
            stage1 = CreateCrumbleStageVisual(root.transform, _crumbleVisuals.Stage1Prefab, "Crumble Stage 1", blockSize);
            stage2 = CreateCrumbleStageVisual(root.transform, _crumbleVisuals.Stage2Prefab, "Crumble Stage 2", blockSize);
            stage3 = CreateCrumbleStageVisual(root.transform, _crumbleVisuals.Stage3Prefab, "Crumble Stage 3", blockSize);
            stage2.SetActive(false);
            stage3.SetActive(false);
        }

        private GameObject CreateCrumbleStageVisual(
            Transform parent,
            GameObject prefab,
            string objectName,
            Vector3 blockSize)
        {
            var visual = Instantiate(prefab, parent);
            visual.name = objectName;
            visual.transform.localPosition = Vector3.up * 0.5f;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = ModuleVisualPrefabLibrary.ResolveTopAlignedUniformLocalScale(blockSize);
            ModuleVisualPrefabLibrary.PrepareVisualInstance(visual);
            _visualObjectCount++;
            return visual;
        }

        private GameObject ResolveAuthoredMechanicSupport(string stableId)
        {
            if (string.IsNullOrEmpty(stableId)) return null;
            if (_authoredSupportObjects.TryGetValue(stableId, out var support)) return support;
            throw new InvalidOperationException("Missing authored mechanic support: " + stableId);
        }

        private void CreateRestorePoint(
            CheckpointService checkpoints,
            ModuleRestorePointDefinition restore)
        {
            var platform = ResolveAuthoredMechanicSupport(restore.SupportBlockStableId) ?? CreateGameplayBlock(
                restore.StableId + ".platform",
                restore.Pose,
                new Vector3(3f, 0.6f, 3f),
                ModuleMaterialRole.Normal);

            var floorTopY = restore.Pose.Position.y + 0.3f;
            var spawnAnchor = new Vector3(restore.Pose.Position.x, floorTopY + 0.05f, restore.Pose.Position.z);

            var triggerObject = new GameObject(restore.StableId + ".trigger", typeof(BoxCollider), typeof(RestorePoint));
            triggerObject.transform.position = restore.Pose.Position + Vector3.up * 1.5f;
            triggerObject.transform.rotation = restore.Pose.Rotation;
            var box = triggerObject.GetComponent<BoxCollider>();
            box.size = new Vector3(3.2f, 3f, 3.2f);
            box.isTrigger = true;

            var restorePoint = triggerObject.GetComponent<RestorePoint>();
            restorePoint.Initialize(
                checkpoints,
                restore.StableId,
                restore.Order,
                spawnAnchor,
                () =>
                {
                    _telemetry?.RecordRestoreSplit(restore.StableId);
                    var split = _runSession?.RecordSplit(restore.StableId);
                    _hud?.ShowSplit(split);
                    FindAnyObjectByType<MovementFeedback>()?.PlayRestorePoint();
                    FindAnyObjectByType<MovementFeedback>()?.PlaySplit();
                    _gameplayVfx?.PlayRestore(spawnAnchor);
                    platform.GetComponentInChildren<RestoreSurfaceMarker>()?.Activate();
                });

            var restorePrefab = _prefabLibrary != null ? _prefabLibrary.PrefabFor(ModuleMaterialRole.Restore) : null;
            if (restorePrefab != null)
            {
                var shrine = Instantiate(restorePrefab, platform.transform);
                shrine.name = "Restore Shrine Visual";
                shrine.transform.localPosition = new Vector3(0f, 0.5f, 0.18f);
                shrine.transform.localRotation = Quaternion.identity;
                var supportScale = platform.transform.lossyScale;
                shrine.transform.localScale = new Vector3(1.2f / supportScale.x, 1.2f / supportScale.y, 1.2f / supportScale.z);
                // A surface marker fills the support and leaves the landing center open.
                if (shrine.GetComponent<RestoreSurfaceMarker>() != null)
                {
                    shrine.transform.localPosition=new Vector3(0,.504f,0);
                    shrine.transform.localScale=Vector3.one;
                }
                ModuleVisualPrefabLibrary.PrepareVisualInstance(shrine);
            }

            CreateWorldLabel("RESTORE", restore.Pose.Position + Vector3.up * 2f, debugOnly: true);
        }

        private void CreatePatchBlock(MovementFeedback feedback)
        {
            var patch = _module.PatchBlock;

            var platform = ResolveAuthoredMechanicSupport(patch.SupportBlockStableId) ?? CreateGameplayBlock(
                patch.StableId + ".platform",
                new ModulePose(patch.Pose.Position + Vector3.down * 0.8f, patch.Pose.EulerAngles),
                new Vector3(3f, 0.6f, 3f),
                ModuleMaterialRole.Normal);

            var patchObject = new GameObject(patch.StableId, typeof(BoxCollider), typeof(PatchBlock));
            patchObject.transform.SetPositionAndRotation(patch.Pose.Position, patch.Pose.Rotation);
            var trigger = patchObject.GetComponent<BoxCollider>();
            trigger.size = new Vector3(2.5f, 2.5f, 2.5f);
            trigger.isTrigger = true;

            var finish = patchObject.GetComponent<PatchBlock>();
            finish.Initialize(
                _telemetry,
                feedback,
                () =>
                {
                    _gameplayVfx?.PlayPatch(patch.Pose.Position);
                    _hud.ShowModuleFixed();
                },
                _ => CompleteModuleRun(feedback));

            var patchPrefab = _prefabLibrary != null ? _prefabLibrary.PrefabFor(ModuleMaterialRole.Patch) : null;
            if (patchPrefab != null)
            {
                var visual = Instantiate(patchPrefab, patchObject.transform);
                visual.name = "Patch Relic Visual";
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale = Vector3.one * 1.8f;
                ModuleVisualPrefabLibrary.PrepareVisualInstance(visual);
            }

            patchObject.AddComponent<RuntimeVisualPulse>().Initialize(
                _visuals.ColorFor(ModuleMaterialRole.Patch),
                _visuals.ColorFor(ModuleMaterialRole.WarmAccent),
                5.2f,
                0.22f);

            CreateWorldLabel("PATCH BLOCK", patch.Pose.Position + Vector3.up * 2.2f, debugOnly: true);
        }

        private void CreateShortcutTrigger(ModuleShortcutDefinition shortcut)
        {
            var center = Vector3.Lerp(shortcut.EntryPosition, shortcut.ExitPosition, 0.5f);
            var length = Vector3.Distance(shortcut.EntryPosition, shortcut.ExitPosition);
            var trigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trigger.name = shortcut.StableId;
            trigger.transform.position = center + Vector3.up * 1.2f;
            trigger.transform.localScale = new Vector3(2.4f, 2.2f, Mathf.Max(2f, length));
            trigger.GetComponent<Renderer>().enabled = false;
            trigger.GetComponent<BoxCollider>().isTrigger = true;
            var direction = shortcut.ExitPosition - shortcut.EntryPosition;
            if (direction.sqrMagnitude > 0.001f)
            {
                trigger.transform.rotation = Quaternion.LookRotation(
                    Vector3.ProjectOnPlane(direction, Vector3.up).normalized,
                    Vector3.up);
            }

            var shortcutTrigger = trigger.AddComponent<ShortcutTrigger>();
            shortcutTrigger.Initialize(shortcut.StableId, _telemetry);
            CreateWorldLabel(shortcut.DisplayName.ToUpperInvariant(), center + Vector3.up * 2.6f, debugOnly: true);
        }

        private void CreateCameraHint(ModuleCameraHintDefinition hint)
        {
            var trigger = new GameObject(hint.StableId, typeof(BoxCollider), typeof(CameraGuideHint));
            trigger.transform.position = hint.Position;
            trigger.transform.localScale = hint.Size;
            trigger.GetComponent<BoxCollider>().isTrigger = true;
            trigger.GetComponent<CameraGuideHint>().Configure(
                hint.PreferredYaw,
                hint.PreferredPitch,
                hint.MaxAssistDegrees);
            CreateWorldLabel("CAMERA HINT", hint.Position + Vector3.up * 2.1f, debugOnly: true);
        }

        private void AddReadabilityTrim(GameObject block, Vector3 size)
        {
            if (size.x < 1.2f || size.z < 1.2f)
            {
                return;
            }

            var inset = _visuals.SurfaceInsetScale;
            var trim = _visuals.EdgeTrimScale;
            CreateChildPlate(
                block.transform,
                "Top Surface Inset",
                new Vector3(0f, 0.59f, 0f),
                new Vector3(inset * 0.92f, 0.045f / Mathf.Max(0.1f, size.y), inset * 0.92f),
                ModuleMaterialRole.SurfaceHighlight);
            CreateStonePanelGrooves(block.transform, size);
            CreateChildPlate(
                block.transform,
                "Top Front Trim",
                new Vector3(0f, 0.62f, -0.47f),
                new Vector3(0.92f, 0.035f / Mathf.Max(0.1f, size.y), trim),
                ModuleMaterialRole.Edge);
            CreateChildPlate(
                block.transform,
                "Top Back Trim",
                new Vector3(0f, 0.62f, 0.47f),
                new Vector3(0.92f, 0.035f / Mathf.Max(0.1f, size.y), trim),
                ModuleMaterialRole.Edge);
            CreateChildPlate(
                block.transform,
                "Underside",
                new Vector3(0f, -0.56f, 0f),
                new Vector3(1.02f, 0.08f / Mathf.Max(0.1f, size.y), 1.02f),
                ModuleMaterialRole.Underside);
            CreateChildPlate(
                block.transform,
                "Front Side Shade",
                new Vector3(0f, 0f, -0.505f),
                new Vector3(1.01f, 0.86f, 0.035f / Mathf.Max(0.1f, size.z)),
                ModuleMaterialRole.StoneSide);
            CreateChildPlate(
                block.transform,
                "Right Side Shade",
                new Vector3(0.505f, 0f, 0f),
                new Vector3(0.035f / Mathf.Max(0.1f, size.x), 0.86f, 1.01f),
                ModuleMaterialRole.StoneSide);
            CreateChildPlate(
                block.transform,
                "Left Edge",
                new Vector3(-0.5f, 0.56f, 0f),
                new Vector3(trim / Mathf.Max(0.1f, size.x), 0.08f / Mathf.Max(0.1f, size.y), 1f),
                ModuleMaterialRole.Edge);
            CreateChildPlate(
                block.transform,
                "Right Edge",
                new Vector3(0.5f, 0.56f, 0f),
                new Vector3(trim / Mathf.Max(0.1f, size.x), 0.08f / Mathf.Max(0.1f, size.y), 1f),
                ModuleMaterialRole.Edge);
            if (size.x > 2.2f && size.z > 2.2f)
            {
                CreateChildPlate(
                    block.transform,
                    "Circuit Tick A",
                    new Vector3(-0.28f, 0.65f, 0.22f),
                    new Vector3(0.18f, 0.035f / Mathf.Max(0.1f, size.y), 0.035f),
                    ModuleMaterialRole.CircuitLine);
                CreateChildPlate(
                    block.transform,
                    "Circuit Tick B",
                    new Vector3(0.3f, 0.65f, -0.18f),
                    new Vector3(0.035f, 0.035f / Mathf.Max(0.1f, size.y), 0.18f),
                    ModuleMaterialRole.CircuitLine);
            }
        }

        private void AddRoleDetails(GameObject block, Vector3 size, ModuleMaterialRole role)
        {
            switch (role)
            {
                case ModuleMaterialRole.Moving:
                    CreateChildPlate(
                        block.transform,
                        "Moving Left Rail",
                        new Vector3(-0.27f, 0.68f, 0f),
                        new Vector3(0.055f, 0.055f / Mathf.Max(0.1f, size.y), 0.58f),
                        ModuleMaterialRole.MovingAccent);
                    CreateChildPlate(
                        block.transform,
                        "Moving Right Rail",
                        new Vector3(0.27f, 0.68f, 0f),
                        new Vector3(0.055f, 0.055f / Mathf.Max(0.1f, size.y), 0.58f),
                        ModuleMaterialRole.CircuitLine);
                    break;
                case ModuleMaterialRole.Crumbling:
                    CreateCrumbleFaultLines(block.transform);
                    break;
                case ModuleMaterialRole.Precision:
                    CreateChildPlate(
                        block.transform,
                        "Precision Top Cue",
                        new Vector3(0f, 0.68f, 0f),
                        new Vector3(0.52f, 0.055f / Mathf.Max(0.1f, size.y), 0.52f),
                        ModuleMaterialRole.MovingAccent);
                    break;
                case ModuleMaterialRole.Boost:
                    CreateChildPlate(
                        block.transform,
                        "Boost Warm Rim",
                        new Vector3(0f, 0.69f, 0f),
                        new Vector3(0.88f, 0.055f / Mathf.Max(0.1f, size.y), 0.88f),
                        ModuleMaterialRole.WarmAccent);
                    break;
            }
        }

        private void CreateStonePanelGrooves(Transform parent, Vector3 size)
        {
            if (size.x < 2.4f || size.z < 2.4f)
            {
                return;
            }

            var thinY = 0.034f / Mathf.Max(0.1f, size.y);
            CreateChildPlate(
                parent,
                "Stone Panel Groove X",
                new Vector3(-0.18f, 0.666f, -0.06f),
                new Vector3(0.028f, thinY, 0.66f),
                ModuleMaterialRole.Edge);
            CreateChildPlate(
                parent,
                "Stone Panel Groove Z",
                new Vector3(0.16f, 0.668f, 0.13f),
                new Vector3(0.58f, thinY, 0.028f),
                ModuleMaterialRole.Edge);
            if (size.x > 4f || size.z > 4f)
            {
                var crack = CreateChildPlate(
                    parent,
                    "Stone Hairline Crack",
                    new Vector3(0.08f, 0.671f, -0.21f),
                    new Vector3(0.022f, thinY, 0.34f),
                    ModuleMaterialRole.StoneSide);
                crack.transform.localRotation = Quaternion.Euler(0f, 28f, 0f);
            }
        }

        private Vector3 RouteCueDirection(int blockIndex)
        {
            var current = _module.Blocks[blockIndex];
            var currentPosition = current.Pose.Position;
            for (var nextIndex = blockIndex + 1; nextIndex < _module.Blocks.Count; nextIndex++)
            {
                var next = _module.Blocks[nextIndex];
                var candidate = Vector3.ProjectOnPlane(next.Pose.Position - currentPosition, Vector3.up);
                if (candidate.sqrMagnitude > 0.05f)
                {
                    return candidate;
                }
            }

            if (_module.PatchBlock != null)
            {
                var toPatch = Vector3.ProjectOnPlane(
                    _module.PatchBlock.Pose.Position - currentPosition,
                    Vector3.up);
                if (toPatch.sqrMagnitude > 0.05f)
                {
                    return toPatch;
                }
            }

            return Vector3.forward;
        }

        private void AddRouteFlowCue(
            GameObject block,
            Vector3 size,
            ModuleMaterialRole role,
            Vector3 worldDirection)
        {
            if (block == null || !ShouldCreateRouteFlowCue(role, size))
            {
                return;
            }

            var direction = Vector3.ProjectOnPlane(worldDirection, Vector3.up);
            if (direction.sqrMagnitude <= 0.05f)
            {
                return;
            }

            var localDirection = block.transform.InverseTransformDirection(direction.normalized);
            localDirection.y = 0f;
            if (localDirection.sqrMagnitude <= 0.001f)
            {
                return;
            }

            localDirection.Normalize();
            var angle = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
            var center = localDirection * 0.14f;
            var cross = new Vector3(localDirection.z, 0f, -localDirection.x);
            CreateRouteCueSegment(
                block.transform,
                center - cross * 0.075f,
                angle - 32f,
                ModuleMaterialRole.WarmAccent);
            CreateRouteCueSegment(
                block.transform,
                center + cross * 0.075f,
                angle + 32f,
                ModuleMaterialRole.CircuitLine);
        }

        private void CreateRouteCueSegment(
            Transform parent,
            Vector3 localCenter,
            float yaw,
            ModuleMaterialRole role)
        {
            var cue = CreateChildPlate(
                parent,
                "Route Flow Cue",
                new Vector3(localCenter.x, 0.735f, localCenter.z),
                new Vector3(0.05f, 0.042f, 0.32f),
                role);
            cue.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private void CreateDecoration(ModuleBlockDefinition decoration)
        {
            if (CreateVisualPrefabDecoration(decoration))
            {
                return;
            }

            if (decoration.StableId.StartsWith("m04.world.depth.red-glow", StringComparison.Ordinal))
            {
                CreateAbyssGlow(decoration.StableId, decoration.Pose.Position, decoration.Size);
                return;
            }

            switch (decoration.VisualRole)
            {
                case ModuleMaterialRole.DecorationStone:
                    CreateFloatingIsland(decoration);
                    break;
                case ModuleMaterialRole.Vegetation:
                    CreateVegetation(decoration.StableId, decoration.Pose.Position, decoration.Size);
                    break;
                case ModuleMaterialRole.Cloud:
                    CreateCloudCluster(decoration.StableId, decoration.Pose.Position, decoration.Size);
                    break;
                case ModuleMaterialRole.TowerCore:
                    CreateTowerCore(decoration.StableId, decoration.Pose.Position, decoration.Size);
                    break;
                default:
                    CreateVisualOnly(
                        decoration.StableId,
                        decoration.Pose.Position,
                        decoration.Size,
                        decoration.VisualRole,
                        decoration.Pose.Rotation);
                    break;
            }
        }

        private bool CreateVisualPrefabDecoration(ModuleBlockDefinition decoration)
        {
            var placement = _prefabLibrary == null
                ? default
                : _prefabLibrary.PlacementFor(decoration.VisualRole, decoration.Size);
            if (!placement.IsValid)
            {
                return false;
            }

            var root = new GameObject(decoration.StableId);
            root.transform.SetPositionAndRotation(decoration.Pose.Position, decoration.Pose.Rotation);
            var visual = Instantiate(placement.Prefab, root.transform);
            visual.name = decoration.VisualRole + " Visual";
            visual.transform.localPosition = placement.LocalPosition;
            visual.transform.localRotation = Quaternion.Euler(placement.LocalEulerAngles);
            var uniformScale = placement.LocalScale.x;
            var maxDim = Mathf.Max(decoration.Size.x, decoration.Size.y, decoration.Size.z);
            if (maxDim > 0.01f && Mathf.Abs(maxDim - 1f) > 0.01f)
            {
                uniformScale = Mathf.Max(0.1f, maxDim);
            }

            visual.transform.localScale = Vector3.one * uniformScale;
            ModuleVisualPrefabLibrary.PrepareVisualInstance(visual);
            _visualObjectCount++;
            return true;
        }

        private void CreateFloatingIsland(ModuleBlockDefinition decoration)
        {
            var root = new GameObject(decoration.StableId);
            root.transform.SetPositionAndRotation(decoration.Pose.Position, decoration.Pose.Rotation);
            _visualObjectCount++;
            CreateKitPiece(
                root.transform,
                "Light Island Top",
                Vector3.up * 0.22f,
                decoration.Size,
                ModuleMaterialRole.DecorationStone);
            CreateKitPiece(
                root.transform,
                "Soft Grass Cap",
                Vector3.up * (decoration.Size.y * 0.58f),
                new Vector3(decoration.Size.x * 0.78f, 0.18f, decoration.Size.z * 0.76f),
                ModuleMaterialRole.Vegetation);
            CreateKitPiece(
                root.transform,
                "Dark Tapered Underside",
                Vector3.down * (decoration.Size.y * 0.42f + 0.25f),
                new Vector3(decoration.Size.x * 0.72f, decoration.Size.y * 1.1f, decoration.Size.z * 0.72f),
                ModuleMaterialRole.Underside);
            CreateKitPiece(
                root.transform,
                "Broken Side Chunk A",
                new Vector3(decoration.Size.x * 0.42f, -0.1f, decoration.Size.z * 0.18f),
                decoration.Size * 0.22f,
                ModuleMaterialRole.StoneSide);
            CreateKitPiece(
                root.transform,
                "Broken Side Chunk B",
                new Vector3(-decoration.Size.x * 0.36f, -0.28f, -decoration.Size.z * 0.28f),
                decoration.Size * 0.18f,
                ModuleMaterialRole.StoneSide);
            if (_visuals.HeroAccentDensity > 0.45f)
            {
                CreateKitPiece(
                    root.transform,
                    "Warm Island Marker",
                    new Vector3(0f, decoration.Size.y * 0.78f, -decoration.Size.z * 0.22f),
                    new Vector3(decoration.Size.x * 0.18f, 0.12f, 0.18f),
                    ModuleMaterialRole.WarmAccent);
            }
        }

        private void CreateTowerCore(string stableId, Vector3 position, Vector3 size)
        {
            var root = new GameObject(stableId);
            root.transform.position = position;
            _visualObjectCount++;
            CreateKitPiece(root.transform, "Core Body", Vector3.zero, size, ModuleMaterialRole.TowerCore);
            CreateKitPiece(
                root.transform,
                "Core Cap",
                Vector3.up * (size.y * 0.52f),
                new Vector3(size.x * 1.18f, 0.35f, size.z * 1.18f),
                ModuleMaterialRole.Edge);
            CreateKitPiece(
                root.transform,
                "Patch Glyph",
                new Vector3(0f, size.y * 0.24f, -size.z * 0.51f),
                new Vector3(size.x * 0.34f, 0.08f, 0.045f),
                ModuleMaterialRole.PatchCore);
            CreateKitPiece(
                root.transform,
                "Cyan Circuit Slot",
                new Vector3(0f, -size.y * 0.08f, -size.z * 0.52f),
                new Vector3(size.x * 0.52f, 0.07f, 0.04f),
                ModuleMaterialRole.CircuitLine);
            CreateKitPiece(
                root.transform,
                "Warm Core Notch",
                new Vector3(0f, size.y * 0.4f, -size.z * 0.52f),
                new Vector3(size.x * 0.18f, 0.16f, 0.04f),
                ModuleMaterialRole.WarmAccent);
        }

        private void CreateVegetation(string stableId, Vector3 position, Vector3 size)
        {
            var root = new GameObject(stableId);
            root.transform.position = position;
            _visualObjectCount++;
            CreateKitPiece(root.transform, "Trunk", Vector3.down * 0.35f, new Vector3(0.32f, size.y, 0.32f), ModuleMaterialRole.TowerCore);
            CreateKitPiece(root.transform, "Leaf Crown", Vector3.up * (size.y * 0.38f), size, ModuleMaterialRole.Vegetation);
            CreateKitPiece(root.transform, "Leaf Chunk", new Vector3(size.x * 0.32f, size.y * 0.42f, 0f), size * 0.55f, ModuleMaterialRole.Vegetation);
            CreateKitPiece(root.transform, "Flower Accent", new Vector3(-size.x * 0.22f, size.y * 0.72f, size.z * 0.18f), size * 0.18f, ModuleMaterialRole.Flower);
        }

        private void CreateCloudCluster(string stableId, Vector3 position, Vector3 size)
        {
            var root = new GameObject(stableId);
            root.transform.position = position;
            _visualObjectCount++;
            CreateCloudPuff(root.transform, "Cloud Core", Vector3.zero, size);
            CreateCloudPuff(root.transform, "Cloud Puff L", new Vector3(-size.x * 0.3f, size.y * 0.05f, 0f), size * 0.62f);
            CreateCloudPuff(root.transform, "Cloud Puff R", new Vector3(size.x * 0.3f, size.y * 0.03f, 0f), size * 0.58f);
            CreateCloudPuff(root.transform, "Cloud Crown", new Vector3(0f, size.y * 0.28f, -size.z * 0.05f), new Vector3(size.x * 0.58f, size.y * 0.72f, size.z * 0.55f));
            CreateCloudPuff(root.transform, "Cloud Trail", new Vector3(-size.x * 0.14f, -size.y * 0.12f, size.z * 0.24f), new Vector3(size.x * 0.66f, size.y * 0.45f, size.z * 0.5f));
        }

        private void CreateAbyssGlow(string stableId, Vector3 position, Vector3 size)
        {
            var root = new GameObject(stableId);
            root.transform.position = position;
            _visualObjectCount++;
            CreateKitPiece(
                root.transform,
                "Deep Red Glow Core",
                Vector3.zero,
                new Vector3(size.x * 0.24f, size.y, size.z * 0.24f),
                ModuleMaterialRole.CrumbleFault);
            for (var index = 0; index < 4; index++)
            {
                var angle = index * 90f;
                var offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(size.x * 0.28f, 0f, 0f);
                var shaft = CreateKitPiece(
                    root.transform,
                    "Red Fog Shaft",
                    offset,
                    new Vector3(size.x * 0.08f, size.y * 0.82f, size.z * 0.08f),
                    ModuleMaterialRole.CrumbleFault);
                shaft.transform.localRotation = Quaternion.Euler(0f, angle + 18f, index % 2 == 0 ? 7f : -7f);
            }

            CreateKitPiece(
                root.transform,
                "Dark Abyss Halo",
                new Vector3(0f, -size.y * 0.32f, 0f),
                new Vector3(size.x * 0.82f, size.y * 0.12f, size.z * 0.82f),
                ModuleMaterialRole.Corruption);
        }

        private void CreateCloudPuff(
            Transform parent,
            string objectName,
            Vector3 localPosition,
            Vector3 localScale)
        {
            var puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            puff.name = objectName;
            puff.transform.SetParent(parent, false);
            puff.transform.localPosition = localPosition;
            puff.transform.localScale = localScale;
            puff.GetComponent<Renderer>().sharedMaterial = _visuals.MaterialFor(ModuleMaterialRole.Cloud);
            // The generic reference keeps SphereCollider available to IL2CPP while
            // CreatePrimitive constructs the temporary primitive hierarchy.
            var primitiveCollider = puff.GetComponent<SphereCollider>();
            if (primitiveCollider != null)
            {
                Destroy(primitiveCollider);
            }

            ModuleVisualPrefabLibrary.PrepareVisualInstance(puff);
            _visualObjectCount++;
        }

        private float ConstructionBudgetSeconds => _constructionBudgetSeconds;

        private System.Collections.IEnumerator CreateBiomeWorld()
        {
            if (_biome == null)
            {
                yield break;
            }

            var root = new GameObject("Biome World - " + _biome.DisplayName);
            root.transform.position = Vector3.zero;
            yield return CreateBiomeObjects(root.transform);
            if (_biome.CombineStaticGeometry) StaticBatchingUtility.Combine(root);
        }

        private System.Collections.IEnumerator CreateBiomeObjects(Transform parent)
        {
            var budgetStart = Time.realtimeSinceStartup;
            foreach (var worldObject in _biome.WorldObjects)
            {
                if (worldObject == null || !worldObject.IsValid)
                {
                    continue;
                }

                var instance = Instantiate(
                    worldObject.Prefab,
                    worldObject.Position,
                    worldObject.Rotation,
                    parent);
                instance.name = worldObject.StableId;
                instance.transform.localScale = worldObject.Scale;
                instance.isStatic = worldObject.StaticBatch;
                foreach (var child in instance.GetComponentsInChildren<Transform>(includeInactive: true))
                {
                    child.gameObject.isStatic = worldObject.StaticBatch;
                }

                ModuleVisualPrefabLibrary.PrepareVisualInstance(instance, worldObject.HasPlayableArchitecture);
                foreach(var surface in instance.GetComponentsInChildren<Avoidance.Gameplay.Worlds.AuthoredSurface>(true))
                    surface.SetRestoreOnLanding(worldObject.GeometryKind == Avoidance.Gameplay.Worlds.WorldGeometryKind.FatalScenery);
                if(_environment.NearArchitectureShadows && worldObject.DepthBand==BiomeDepthBand.NearEnvironment)
                    foreach(var renderer in instance.GetComponentsInChildren<Renderer>())
                    { renderer.shadowCastingMode=ShadowCastingMode.On; renderer.receiveShadows=true; }
                ApplyDepthBandPresentation(
                    instance,
                    worldObject.DepthBand,
                    worldObject.DepthFadeStrength);
                _visualObjectCount++;
                if (Time.realtimeSinceStartup - budgetStart > ConstructionBudgetSeconds)
                { yield return null; budgetStart = Time.realtimeSinceStartup; }
            }
        }

        private void ApplyDepthBandPresentation(
            GameObject instance,
            BiomeDepthBand depthBand,
            float fadeStrength)
        {
            if (_biome == null || instance == null)
            {
                return;
            }

            var hazeAmount = DepthBandHazeAmount(depthBand) * Mathf.Clamp01(fadeStrength);
            if (hazeAmount <= 0.001f)
            {
                return;
            }

            var propertyBlock = new MaterialPropertyBlock();
            var haze = _biome.HazeColor;
            haze.a = 1f;
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                if (renderer == null)
                {
                    continue;
                }

                var source = Color.white;
                var material = renderer.sharedMaterial;
                if (material != null)
                {
                    if (material.HasProperty(BaseColorId))
                    {
                        source = material.GetColor(BaseColorId);
                    }
                    else if (material.HasProperty(ColorId))
                    {
                        source = material.GetColor(ColorId);
                    }
                }

                source.a = 1f;
                var color = Color.Lerp(source, haze, hazeAmount);
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, color);
                propertyBlock.SetColor(ColorId, color);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }

        private static float DepthBandHazeAmount(BiomeDepthBand depthBand)
        {
            return depthBand switch
            {
                BiomeDepthBand.Gameplay => 0f,
                BiomeDepthBand.NearEnvironment => 0.06f,
                BiomeDepthBand.MidWorld => 0.22f,
                BiomeDepthBand.FarWorld => 0.48f,
                BiomeDepthBand.LowerAbyss => 0.68f,
                _ => 0.22f
            };
        }

        private static float StablePhase(string stableId)
        {
            if (string.IsNullOrEmpty(stableId))
            {
                return 0f;
            }

            unchecked
            {
                var hash = 17;
                for (var index = 0; index < stableId.Length; index++)
                {
                    hash = hash * 31 + stableId[index];
                }

                return Mathf.Abs(hash % 6283) * 0.001f;
            }
        }

        private void CreateMobilePresentationVolume()
        {
            if (!ShouldUseGoldSlicePresentation())
            {
                return;
            }

            var volumeObject = new GameObject("Mobile Presentation Volume - Ancient Abyss", typeof(Volume));
            var volume = volumeObject.GetComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 1f;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "VP_RR_AncientAbyss_Mobile";
            profile.hideFlags = HideFlags.HideAndDontSave;
            var color = profile.Add<ColorAdjustments>(true);
            color.postExposure.value = -0.02f;
            color.contrast.value = 11f;
            color.saturation.value = 3f;
            var bloom = profile.Add<Bloom>(true);
            bloom.intensity.value = 0.12f;
            bloom.threshold.value = 1.24f;
            bloom.scatter.value = 0.42f;
            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.value = 0.045f;
            vignette.smoothness.value = 0.22f;
            volume.sharedProfile = profile;
        }

        private bool ShouldUseGoldSlicePresentation() =>
            _module != null
            && _module.StableModuleId == "module.003.flow-error"
            && _biome != null
            && _biome.Kind == EnvironmentBiomeKind.AncientAbyss;

        private void CreateAmbientClouds()
        {
            // Ambient clouds are integrated into the high-quality panoramic skybox.
        }

        private void CreateNullSpaceDepth()
        {
            // Phase 0.6.5 recovery keeps abyss depth to authored decorations only.
        }

        private void CreateDistantFragments()
        {
            if (_module.DisableAutomaticDistantFragments
                || _biome != null
                || _environment.DistantIslandDensity <= 0.01f)
            {
                return;
            }

            CreateDecoration(new ModuleBlockDefinition(
                "distant.patch-tower.a",
                new Vector3(-28f, 9f, 92f),
                new Vector3(6.5f, 6.5f, 6.5f),
                ModuleMaterialRole.TowerCore));
            CreateDecoration(new ModuleBlockDefinition(
                "distant.patch-tower.b",
                new Vector3(32f, 12f, 124f),
                new Vector3(6.5f, 6.5f, 6.5f),
                ModuleMaterialRole.TowerCore));
            CreateDecoration(new ModuleBlockDefinition(
                "distant.floating-island.a",
                new Vector3(-48f, -3f, 86f),
                new Vector3(4.5f, 4.5f, 4.5f),
                ModuleMaterialRole.DecorationStone));
            CreateDecoration(new ModuleBlockDefinition(
                "distant.floating-island.b",
                new Vector3(48f, -2.4f, 112f),
                new Vector3(4.5f, 4.5f, 4.5f),
                ModuleMaterialRole.DecorationStone));
        }

        private void CreateVisualBenchmarks()
        {
            foreach (var anchor in BenchmarksForActiveModule())
            {
                var anchorObject = new GameObject(anchor.name, typeof(VisualBenchmarkAnchor));
                anchorObject.transform.SetPositionAndRotation(
                    anchor.position,
                    Quaternion.Euler(0f, anchor.yaw, 0f));
                var benchmark = anchorObject.GetComponent<VisualBenchmarkAnchor>();
                benchmark.Initialize(anchor.name, anchor.yaw);
                _visualBenchmarks.Add(benchmark);
            }
        }

        private RouteCameraGraph CreateRouteCameraGraph()
        {
            if (_module == null || _module.StableModuleId != "module.004.the-spiral")
            {
                return new RouteCameraGraph(
                    "route-camera.current.simple",
                    new[]
                    {
                        new RouteCameraBranch(
                            "route.standard",
                            RouteCameraBranchKind.Standard,
                            new[]
                            {
                                RouteNode("route.current.start", _module.StartPoint.Position, Vector3.forward),
                                RouteNode("route.current.patch", _module.PatchBlock.Pose.Position, Vector3.forward)
                            },
                            8f)
                    },
                    "route.standard");
            }

            var standard = new RouteCameraBranch(
                "route.spiral.standard",
                RouteCameraBranchKind.Standard,
                new[]
                {
                    RouteNode("route.spiral.start", new Vector3(0f, 0.35f, -42f), new Vector3(0f, 0f, 1f), 0f),
                    RouteNode("route.spiral.base", new Vector3(16f, 1.3f, -25f), new Vector3(1f, 0f, 1f), 1.5f),
                    RouteNode("route.spiral.low", new Vector3(18f, 3.25f, 18f), new Vector3(-1f, 0f, 1f), 2f),
                    RouteNode("route.spiral.moving", new Vector3(-25f, 7.35f, -9f), new Vector3(-1f, 0f, -1f), 2.5f),
                    RouteNode("route.spiral.water", new Vector3(22f, 9.95f, -12f), new Vector3(1f, 0f, 0f), 3f),
                    RouteNode("route.spiral.boost", new Vector3(24f, 12.2f, 12f), new Vector3(-1f, 0f, 1f), 3f),
                    RouteNode("route.spiral.surf", new Vector3(-8f, 16.45f, 34f), new Vector3(-1f, 0f, 0f), 3.5f),
                    RouteNode("route.spiral.high", new Vector3(-9f, 19.85f, -28f), new Vector3(1f, 0f, -1f), 3.5f),
                    RouteNode("route.spiral.summit", new Vector3(0f, 32.1f, 0f), new Vector3(0f, 0f, 1f), 1.5f)
                },
                7.5f);

            return new RouteCameraGraph(
                "route-camera.module-004.the-spiral.v1",
                new[]
                {
                    standard,
                    new RouteCameraBranch(
                        "route.spiral.shortcut.inner-gap",
                        RouteCameraBranchKind.Shortcut,
                        new[]
                        {
                            RouteNode("route.spiral.shortcut.inner-gap.entry", new Vector3(18f, 2.2f, 18f), new Vector3(-1f, 0f, 1f), 1f),
                            RouteNode("route.spiral.shortcut.inner-gap.exit", new Vector3(-2f, 3.3f, 32f), new Vector3(-1f, 0f, 1f), 1f)
                        },
                        5.5f,
                        standard.StableId),
                    new RouteCameraBranch(
                        "route.spiral.shortcut.water-speed",
                        RouteCameraBranchKind.Shortcut,
                        new[]
                        {
                            RouteNode("route.spiral.shortcut.water.entry", new Vector3(-12f, 7.3f, -31f), new Vector3(1f, 0f, 0.2f), 0.5f),
                            RouteNode("route.spiral.shortcut.water.exit", new Vector3(15f, 8.8f, -24f), new Vector3(1f, 0f, 0.4f), 0.5f)
                        },
                        6f,
                        standard.StableId),
                    new RouteCameraBranch(
                        "route.spiral.shortcut.boost-overshoot",
                        RouteCameraBranchKind.Shortcut,
                        new[]
                        {
                            RouteNode("route.spiral.shortcut.boost.entry", new Vector3(24f, 9.8f, -2f), new Vector3(-0.5f, 0f, 1f), 1f),
                            RouteNode("route.spiral.shortcut.boost.exit", new Vector3(10f, 12.8f, 34f), new Vector3(-1f, 0f, 1f), 1f)
                        },
                        6f,
                        standard.StableId),
                    new RouteCameraBranch(
                        "route.spiral.shortcut.high-risk-drop",
                        RouteCameraBranchKind.Shortcut,
                        new[]
                        {
                            RouteNode("route.spiral.shortcut.drop.entry", new Vector3(-9f, 20.1f, -28f), new Vector3(1f, 0f, 1f), -1f),
                            RouteNode("route.spiral.shortcut.drop.exit", new Vector3(8f, 24.5f, 26f), new Vector3(0.3f, 0f, 1f), 1f)
                        },
                        6f,
                        standard.StableId)
                },
                standard.StableId);
        }

        private static RouteCameraNode RouteNode(
            string stableId,
            Vector3 position,
            Vector3 forward,
            float verticalFramingHint = 0f)
        {
            return new RouteCameraNode(stableId, position, forward, verticalFramingHint);
        }

        private static void CreateRouteCameraDebugView(RouteCameraGraph graph)
        {
            if (graph == null || !graph.IsValid)
            {
                return;
            }

            var viewObject = new GameObject("Route Camera Debug View", typeof(RouteCameraDebugView));
            viewObject.GetComponent<RouteCameraDebugView>().Initialize(graph, false);
        }

        private List<(string name, Vector3 position, float yaw)> BenchmarksForActiveModule()
        {
            var anchors = new List<(string name, Vector3 position, float yaw)>
            {
                ("Benchmark_Current_Start", _module.StartPoint.Position + Vector3.up * 1.4f, 0f)
            };

            if (_module.StableModuleId == "module.001.first-steps")
            {
                anchors.Add(("Benchmark_Module01_Start", _module.StartPoint.Position + Vector3.up * 1.4f, 0f));
                return anchors;
            }

            if (_module.StableModuleId == "module.002.moving-parts")
            {
                anchors.Add(("Benchmark_Module02_Mid", new Vector3(4f, 2f, 38f), 8f));
                return anchors;
            }

            if (_module.StableModuleId == "module.003.flow-error")
            {
                anchors.Add(("Benchmark_Module03_Opening", new Vector3(0f, 1.7f, -7f), -22f));
                anchors.Add(("Benchmark_Module03_Early", new Vector3(-4f, 2.9f, 15f), 38f));
                anchors.Add(("Benchmark_Module03_Middle", new Vector3(-5f, 4.9f, 40.5f), 34f));
                anchors.Add(("Benchmark_Module03_High", new Vector3(7f, 9.8f, 76f), -28f));
                anchors.Add(("Benchmark_Module03_Patch", new Vector3(-4f, 15.2f, 97f), 42f));
                return anchors;
            }

            if (_module.StableModuleId == "module.004.the-spiral")
            {
                var route = _module.Blocks.OrderBy(block => block.Pose.Position.y).ToArray();
                var low = route[Mathf.Min(10, route.Length - 1)];
                var mid = route[Mathf.Min(30, route.Length - 1)];
                var high = route[Mathf.Min(50, route.Length - 1)];
                var restore = _module.RestorePoints[0];
                var boost = _module.BoostBlocks[0];
                anchors.Add(("Benchmark_Spiral_Start", _module.StartPoint.Position + Vector3.up * 1.4f, _module.StartPoint.EulerAngles.y));
                anchors.Add(("Benchmark_Spiral_Low", low.Pose.Position + Vector3.up * 1.4f, low.Pose.EulerAngles.y));
                anchors.Add(("Benchmark_Spiral_Mid", mid.Pose.Position + Vector3.up * 1.4f, mid.Pose.EulerAngles.y));
                anchors.Add(("Benchmark_Spiral_Restore", restore.Pose.Position + Vector3.up * 1.4f, restore.Pose.EulerAngles.y));
                anchors.Add(("Benchmark_Spiral_Boost", boost.Pose.Position + Vector3.up * 1.4f, boost.Pose.EulerAngles.y));
                anchors.Add(("Benchmark_Spiral_High", high.Pose.Position + Vector3.up * 1.4f, high.Pose.EulerAngles.y));
                anchors.Add(("Benchmark_Spiral_Summit", _module.PatchBlock.Pose.Position + Vector3.up * 2.2f, 180f));
                return anchors;
            }

            anchors.Add(("Benchmark_Module02_Mid", new Vector3(4f, 2f, 38f), 8f));
            return anchors;
        }

        private void CreateArrowPlate(Transform parent, ModuleMaterialRole role, Vector3 direction)
        {
            var forward = Vector3.ProjectOnPlane(direction, Vector3.up);
            var angle = forward.sqrMagnitude <= 0.001f ? 0f : Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            for (var index = 0; index < 3; index++)
            {
                var plate = CreateChildPlate(
                    parent,
                    "Direction Cue",
                    new Vector3(0f, 0.72f, -0.22f + index * 0.22f),
                    new Vector3(0.18f, 0.08f, 0.48f),
                    role);
                plate.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            }
        }

        private void CreateVerticalStreaks(Transform parent, ModuleMaterialRole role, int count)
        {
            for (var index = 0; index < count; index++)
            {
                var x = Mathf.Lerp(-0.34f, 0.34f, count <= 1 ? 0.5f : index / (float)(count - 1));
                CreateChildPlate(
                    parent,
                    "Energy Streak",
                    new Vector3(x, 0.9f, 0f),
                    new Vector3(0.045f, 0.7f, 0.045f),
                    role);
            }
        }

        private void CreateWaterVolumeVisuals(Transform parent, Vector3 direction, Vector3 size)
        {
            var forward = Vector3.ProjectOnPlane(direction, Vector3.up);
            var angle = forward.sqrMagnitude <= 0.001f
                ? 0f
                : Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            for (var lane = 0; lane < 4; lane++)
            {
                var x = Mathf.Lerp(-0.38f, 0.38f, lane / 3f);
                for (var segment = 0; segment < 5; segment++)
                {
                    var z = Mathf.Lerp(-0.42f, 0.42f, segment / 4f);
                    var filament = CreateChildPlate(
                        parent,
                        "Water Flow Filament",
                        new Vector3(x, 0.02f + Mathf.Sin((lane + segment) * 1.7f) * 0.04f, z),
                        new Vector3(0.03f / Mathf.Max(0.1f, size.x), 0.58f, 0.16f / Mathf.Max(0.1f, size.z)),
                        ModuleMaterialRole.Water);
                    filament.transform.localRotation = Quaternion.Euler(
                        Mathf.Lerp(-5f, 5f, segment / 4f),
                        angle + Mathf.Lerp(-8f, 8f, lane / 3f),
                        lane % 2 == 0 ? 6f : -6f);
                }
            }

            for (var side = -1; side <= 1; side += 2)
            {
                for (var segment = 0; segment < 4; segment++)
                {
                    var ribbon = CreateChildPlate(
                        parent,
                        "Water Side Spark",
                        new Vector3(side * 0.48f, 0.04f, Mathf.Lerp(-0.36f, 0.36f, segment / 3f)),
                        new Vector3(0.04f / Mathf.Max(0.1f, size.x), 0.48f, 0.12f / Mathf.Max(0.1f, size.z)),
                        ModuleMaterialRole.WaterFoam);
                    ribbon.transform.localRotation = Quaternion.Euler(0f, angle + side * 4f, side * 10f);
                }
            }

            CreateVerticalStreaks(parent, ModuleMaterialRole.WaterFoam, 5);
        }

        private void CreateWaterStreaks(Transform parent, Vector3 direction, Vector3 size)
        {
            var forward = Vector3.ProjectOnPlane(direction, Vector3.up);
            var angle = forward.sqrMagnitude <= 0.001f ? 0f : Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            for (var index = 0; index < 5; index++)
            {
                var offset = Mathf.Lerp(-0.36f, 0.36f, index / 4f);
                var streak = CreateChildPlate(
                    parent,
                    "Water Flow Streak",
                    new Vector3(offset, 0.74f, 0f),
                    new Vector3(0.035f / Mathf.Max(0.1f, size.x), 0.055f, 0.72f),
                    ModuleMaterialRole.WaterFoam);
                streak.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            }
        }

        private void CreateCrumbleFaultLines(Transform parent)
        {
            for (var index = 0; index < 3; index++)
            {
                var line = CreateChildPlate(
                    parent,
                    "Crumble Fault",
                    new Vector3(-0.24f + index * 0.24f, 0.61f, -0.08f + index * 0.04f),
                    new Vector3(0.045f, 0.065f, 0.55f),
                    ModuleMaterialRole.CrumbleFault);
                line.transform.localRotation = Quaternion.Euler(0f, 22f - index * 17f, 0f);
            }
        }

        private void CreateRestoreFrame(Transform parent)
        {
            CreateChildPlate(parent, "Restore Left Pillar", new Vector3(-0.45f, 0f, -0.62f), new Vector3(0.07f, 0.9f, 0.07f), ModuleMaterialRole.MovingAccent);
            CreateChildPlate(parent, "Restore Right Pillar", new Vector3(0.45f, 0f, -0.62f), new Vector3(0.07f, 0.9f, 0.07f), ModuleMaterialRole.MovingAccent);
            CreateChildPlate(parent, "Restore Flag", new Vector3(0f, 0.22f, -0.7f), new Vector3(0.38f, 0.08f, 0.04f), ModuleMaterialRole.WaterFoam);
        }

        private void CreatePatchOrnaments(Transform parent)
        {
            CreateChildPlate(parent, "Patch Bright Core", new Vector3(0f, 0f, -0.54f), new Vector3(0.45f, 0.45f, 0.06f), ModuleMaterialRole.PatchCore);
            for (var index = 0; index < 4; index++)
            {
                var angle = index * 90f;
                var fragment = CreateChildPlate(
                    parent,
                    "Floating Repair Fragment",
                    Quaternion.Euler(0f, angle, 0f) * new Vector3(0.58f, 0.18f, -0.58f),
                    new Vector3(0.16f, 0.08f, 0.32f),
                    ModuleMaterialRole.BoostArrow);
                fragment.transform.localRotation = Quaternion.Euler(0f, angle + 28f, 0f);
            }
        }

        private GameObject CreateKitPiece(
            Transform parent,
            string objectName,
            Vector3 localPosition,
            Vector3 localScale,
            ModuleMaterialRole role)
        {
            var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = objectName;
            piece.transform.SetParent(parent, false);
            piece.transform.localPosition = localPosition;
            piece.transform.localScale = localScale;
            piece.GetComponent<Renderer>().sharedMaterial = _visuals.MaterialFor(role);
            Destroy(piece.GetComponent<Collider>());
            _visualObjectCount++;
            return piece;
        }

        private GameObject CreateChildPlate(
            Transform parent,
            string objectName,
            Vector3 localPosition,
            Vector3 localScale,
            ModuleMaterialRole role)
        {
            var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = objectName;
            plate.transform.SetParent(parent, false);
            plate.transform.localPosition = localPosition;
            plate.transform.localScale = localScale;
            plate.GetComponent<Renderer>().sharedMaterial = _visuals.MaterialFor(role);
            Destroy(plate.GetComponent<Collider>());
            _visualObjectCount++;
            return plate;
        }

        private GameObject CreateVisualOnly(
            string objectName,
            Vector3 position,
            Vector3 scale,
            ModuleMaterialRole role,
            Quaternion rotation = default)
        {
            if (IsVisualOnlyPlatformImpostor(role, scale))
            {
                return CreateNonLandableSignalVisual(
                    objectName,
                    position,
                    scale,
                    role,
                    rotation == default ? Quaternion.identity : rotation);
            }

            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = objectName;
            block.transform.SetPositionAndRotation(
                position,
                rotation == default ? Quaternion.identity : rotation);
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = _visuals.MaterialFor(role);
            Destroy(block.GetComponent<Collider>());
            _visualObjectCount++;
            return block;
        }

        private GameObject CreateNonLandableSignalVisual(
            string objectName,
            Vector3 position,
            Vector3 scale,
            ModuleMaterialRole role,
            Quaternion rotation)
        {
            var root = new GameObject(objectName);
            root.transform.SetPositionAndRotation(position, rotation);
            var width = Mathf.Max(0.08f, scale.x * 0.08f);
            var height = Mathf.Max(0.5f, scale.y * 2.4f);
            var length = Mathf.Max(0.8f, scale.z * 0.72f);
            for (var index = 0; index < 3; index++)
            {
                var offset = Mathf.Lerp(-0.32f, 0.32f, index / 2f);
                var strip = CreateKitPiece(
                    root.transform,
                    "Decorative Non-Landable Signal",
                    new Vector3(scale.x * offset, 0f, 0f),
                    new Vector3(width, height, length),
                    role);
                strip.transform.localRotation = Quaternion.Euler(0f, -18f + index * 18f, 8f);
            }

            return root;
        }

        private void CreateSkyBand(
            string objectName,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            var band = GameObject.CreatePrimitive(PrimitiveType.Cube);
            band.name = objectName;
            band.transform.position = position;
            band.transform.localScale = scale;
            band.GetComponent<Renderer>().sharedMaterial =
                VisualMaterialUtility.CreateRuntimeTransparentMaterial(objectName + " Material", color);
            Destroy(band.GetComponent<Collider>());
            _visualObjectCount++;
        }

        private Material CreateProceduralSkybox()
        {
            if (_environment.SkyboxMaterial != null) return _environment.SkyboxMaterial;
            if (ShouldUseGoldSlicePresentation())
            {
                var ancientAbyssSky = Resources.Load<Material>("Materials/MAT_RR_AncientAbyssSky");
                if (ancientAbyssSky != null)
                {
                    return ancientAbyssSky;
                }
            }

            var precompiled = Resources.Load<Material>("Materials/MAT_RR_Skybox_Seamless");
            if (precompiled != null)
            {
                return precompiled;
            }

            var shader = Shader.Find("Skybox/Procedural");
            if (shader == null)
            {
                return RenderSettings.skybox;
            }

            var material = new Material(shader)
            {
                name = "RYDER'S ROAD Procedural Sky",
                hideFlags = HideFlags.DontSave
            };
            if (material.HasProperty("_SkyTint"))
            {
                material.SetColor("_SkyTint", new Color(0.18f, 0.68f, 0.98f));
            }

            if (material.HasProperty("_GroundColor"))
            {
                material.SetColor("_GroundColor", new Color(0.68f, 0.86f, 0.98f));
            }

            if (material.HasProperty("_AtmosphereThickness"))
            {
                material.SetFloat("_AtmosphereThickness", 0.65f);
            }

            if (material.HasProperty("_Exposure"))
            {
                material.SetFloat("_Exposure", 1.12f);
            }

            return material;
        }

        private void CreateSkyPanorama()
        {
            var texture = Resources.Load<Texture2D>("Textures/RydersRoad_SkyPanorama_01");
            if (texture == null)
            {
                return;
            }

            var panorama = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panorama.name = "Sky Panorama Backdrop";
            panorama.transform.position = new Vector3(0f, 42f, 158f);
            panorama.transform.localScale = new Vector3(280f, 105f, 1f);
            panorama.GetComponent<Renderer>().sharedMaterial =
                VisualMaterialUtility.CreateRuntimeUnlitTexturedMaterial(
                    "Sky Panorama Backdrop Material",
                    new Color(1f, 1f, 1f, 0.92f),
                    texture,
                    Vector2.one);
            Destroy(panorama.GetComponent<Collider>());
            _visualObjectCount++;
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }

        private TouchInputCoordinator CreateTouchInterface(
            out Text statusText,
            out Text completionText)
        {
            var layout = Resources.Load<TouchControlLayout>("Touch_Default")
                ?? TouchControlLayout.CreateRuntimeDefault();
            var canvasObject = new GameObject(
                "Module UI",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(TouchInputCoordinator));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var safeAreaObject = CreateUiObject(
                "Safe Area",
                canvasObject.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                typeof(SafeAreaFitter));
            var safeArea = safeAreaObject.GetComponent<RectTransform>();

            var lookObject = CreateUiObject(
                "Right Look Zone",
                safeArea,
                new Vector2(0.48f, 0.08f),
                new Vector2(1f, 0.92f),
                Vector2.zero,
                Vector2.zero,
                typeof(Image),
                typeof(TouchLookControl));
            var lookImage = lookObject.GetComponent<Image>();
            lookImage.color = new Color(0.1f, 0.75f, 1f, layout.LookZoneOpacity);
            lookObject.GetComponent<TouchLookControl>().Configure(
                lookObject.GetComponent<RectTransform>(),
                layout.LookZoneOpacity);

            var movementZoneObject = CreateUiObject(
                "Left Movement Zone",
                safeArea,
                new Vector2(0f, 0.08f),
                new Vector2(0.52f, 0.92f),
                Vector2.zero,
                Vector2.zero,
                typeof(Image),
                typeof(MovementJoystickControl));
            var movementZoneImage = movementZoneObject.GetComponent<Image>();
            movementZoneImage.color = new Color(1f, 0.84f, 0.16f, 0.001f);

            var joystickObject = CreateUiObject(
                "Movement Floating Origin",
                safeArea,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(layout.JoystickSize, layout.JoystickSize),
                typeof(Image));
            var joystickRect = joystickObject.GetComponent<RectTransform>();
            joystickRect.pivot = new Vector2(0.5f, 0.5f);
            var joystickImage = joystickObject.GetComponent<Image>();
            joystickImage.sprite = GetCircleRingSprite();
            joystickImage.color = new Color(0.9f, 0.97f, 1f, layout.JoystickRingOpacity * 0.72f);
            joystickImage.raycastTarget = false;

            var knobObject = CreateUiObject(
                "Knob",
                joystickRect,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                Vector2.one * layout.JoystickSize * 0.42f,
                typeof(Image));
            var knobImage = knobObject.GetComponent<Image>();
            knobImage.sprite = GetFilledCircleSprite();
            knobImage.color = new Color(0.82f, 0.94f, 1f, layout.JoystickKnobOpacity * 0.68f);
            knobImage.raycastTarget = false;
            var joystick = movementZoneObject.GetComponent<MovementJoystickControl>();
            joystick.Configure(
                movementZoneObject.GetComponent<RectTransform>(),
                safeArea,
                joystickRect,
                knobObject.GetComponent<RectTransform>(),
                layout.JoystickDeadZone,
                layout.FloatingJoystick,
                layout.HorizontalEdgeComfortMargin,
                layout.VerticalEdgeComfortMargin,
                layout.JoystickRingOpacity,
                layout.JoystickKnobOpacity);

            var jumpObject = CreateUiObject(
                "Right Jump Zone",
                safeArea,
                new Vector2(0.52f, 0.08f),
                new Vector2(1f, 0.92f),
                Vector2.zero,
                Vector2.zero,
                typeof(Image),
                typeof(JumpTouchControl));
            var jumpRect = jumpObject.GetComponent<RectTransform>();
            jumpRect.pivot = new Vector2(0.5f, 0.5f);
            var jumpImage = jumpObject.GetComponent<Image>();
            jumpImage.sprite = GetFilledCircleSprite();
            jumpImage.color = new Color(0.18f, 0.86f, 1f, layout.JumpButtonOpacity * 0.78f);
            jumpObject.GetComponent<JumpTouchControl>().Configure(layout.JumpButtonOpacity);
            CreateJumpGlyph(jumpRect);

            statusText = CreateText(
                "Module Status",
                safeArea,
                string.Empty,
                24,
                TextAnchor.UpperCenter,
                new Vector2(0.34f, 1f),
                new Vector2(0.66f, 1f),
                new Vector2(8f, -94f),
                new Vector2(-8f, -18f));
            completionText = CreateText(
                "Module Completion",
                safeArea,
                string.Empty,
                48,
                TextAnchor.MiddleCenter,
                new Vector2(0.28f, 0.68f),
                new Vector2(0.72f, 0.9f),
                Vector2.zero,
                Vector2.zero);
            completionText.enabled = false;

            var coordinator = canvasObject.GetComponent<TouchInputCoordinator>();

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                new GameObject(
                    "Event System",
                    typeof(EventSystem),
                    typeof(InputSystemUIInputModule));
            }

            coordinator.Initialize(
                layout,
                joystick,
                lookObject.GetComponent<TouchLookControl>(),
                jumpObject.GetComponent<JumpTouchControl>(),
                _settings);
            CreateAlphaMenu(safeArea, coordinator);
            CreateDevelopmentToolbar(safeArea, coordinator);
            return coordinator;
        }

        private void CreateAlphaMenu(RectTransform safeArea, TouchInputCoordinator coordinator)
        {
            var panelObject = CreateUiObject(
                "Alpha Run Menu",
                safeArea,
                new Vector2(0.36f, 0.18f),
                new Vector2(0.64f, 0.84f),
                Vector2.zero,
                Vector2.zero,
                typeof(Image));
            panelObject.GetComponent<Image>().color = _visuals.HudPanelColor;
            panelObject.SetActive(false);

            CreateText(
                "Alpha Run Menu Title",
                panelObject.transform,
                BrandPresentation.PlayerFacingTitle,
                30,
                TextAnchor.UpperCenter,
                new Vector2(0f, 0.84f),
                Vector2.one,
                new Vector2(18f, -18f),
                new Vector2(-18f, -12f));

            Text lookText = null;
            Text zonesText = null;
            Text modeText = null;
            void RefreshSettingsText()
            {
                if (modeText != null)
                {
                    modeText.text = coordinator.RuntimeProfile.AutoCameraProfile == AutoCameraProfileKind.SmartParkour
                        ? "EASY MODE"
                        : "CLASSIC MODE";
                }

                if (lookText != null)
                {
                    lookText.text = "LOOK " + coordinator.RuntimeProfile.CameraSensitivity.ToString().ToUpperInvariant();
                }

                if (zonesText != null)
                {
                    zonesText.text = coordinator.ZonesVisible ? "ZONES ON" : "ZONES OFF";
                }
            }

            CreateHudButton(
                safeArea,
                "II",
                new Vector2(0f, 1f),
                new Vector2(64f, -64f),
                new Vector2(74f, 74f),
                () =>
                {
                    panelObject.SetActive(!panelObject.activeSelf);
                    coordinator.ResetState();
                    RefreshSettingsText();
                });
            CreateHudButton(
                panelObject.transform,
                "RESUME",
                new Vector2(0.5f, 0.68f),
                Vector2.zero,
                new Vector2(280f, 52f),
                () =>
                {
                    panelObject.SetActive(false);
                    coordinator.ResetState();
                });
            CreateHudButton(
                panelObject.transform,
                "RETRY",
                new Vector2(0.5f, 0.56f),
                Vector2.zero,
                new Vector2(280f, 52f),
                () =>
                {
                    panelObject.SetActive(false);
                    coordinator.ResetState();
                    RestartModule();
                });
            CreateHudButton(
                panelObject.transform,
                "MODULE SELECT",
                new Vector2(0.5f, 0.46f),
                Vector2.zero,
                new Vector2(280f, 52f),
                () =>
                {
                    coordinator.ResetState();
                    StartCoroutine(LoadScene(ModuleSelectionState.ModuleSelectorSceneName));
                });
            if (CampaignFlowTrial.Active)
            {
                CreateHudButton(panelObject.transform, "RESTORE", new Vector2(.5f,.24f), Vector2.zero,
                    new Vector2(280f,52f), () =>
                    {
                        panelObject.SetActive(false); coordinator.ResetState();
                        FindAnyObjectByType<RestoreController>()?.RequestRestore(false);
                    });
                return;
            }
            modeText = CreateHudButton(
                panelObject.transform,
                "MODE",
                new Vector2(0.5f, 0.34f),
                Vector2.zero,
                new Vector2(280f, 52f),
                () =>
                {
                    if (coordinator.RuntimeProfile.AutoCameraProfile == AutoCameraProfileKind.SmartParkour)
                    {
                        coordinator.UseClassicManualMode();
                    }
                    else
                    {
                        coordinator.UseSmartParkourEasyMode();
                    }

                    RefreshSettingsText();
                });
            lookText = CreateHudButton(
                panelObject.transform,
                "LOOK",
                new Vector2(0.5f, 0.22f),
                Vector2.zero,
                new Vector2(280f, 52f),
                () =>
                {
                    coordinator.CycleCameraSensitivity();
                    RefreshSettingsText();
                });
            CreateHudButton(panelObject.transform, "RESTORE", new Vector2(0.5f, 0.1f), Vector2.zero, new Vector2(280f, 52f), () =>
            {
                panelObject.SetActive(false); coordinator.ResetState();
                FindAnyObjectByType<RestoreController>()?.RequestRestore(false);
            });
            RefreshSettingsText();
        }

        private void CreateDevelopmentToolbar(RectTransform safeArea, TouchInputCoordinator coordinator)
        {
            if (CampaignFlowTrial.Active) return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var toolbarObject = CreateUiObject(
                "Development Toolbar",
                safeArea,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            toolbarObject.SetActive(false);

            CreateHudButton(
                safeArea,
                "DEV",
                new Vector2(0.5f, 0f),
                new Vector2(0f, 24f),
                new Vector2(116f, 48f),
                () => toolbarObject.SetActive(!toolbarObject.activeSelf),
                new Color(0.04f, 0.06f, 0.12f, 0.54f));

            CreateDevelopmentButton(
                toolbarObject.transform,
                "CTRL",
                new Vector2(-1090f, -126f),
                coordinator.CycleControlProfile);
            CreateDevelopmentButton(
                toolbarObject.transform,
                "JUMP",
                new Vector2(-925f, -126f),
                coordinator.CycleJumpMode);
            CreateDevelopmentButton(
                toolbarObject.transform,
                "LOOK",
                new Vector2(-1090f, -182f),
                coordinator.CycleCameraSensitivity);
            CreateDevelopmentButton(
                toolbarObject.transform,
                "MOVE",
                new Vector2(-925f, -182f),
                coordinator.CycleMovementSensitivity);
            CreateDevelopmentButton(
                toolbarObject.transform,
                "MENU",
                new Vector2(-760f, -126f),
                () => StartCoroutine(LoadScene(ModuleSelectionState.ModuleSelectorSceneName)));
            CreateDevelopmentButton(
                toolbarObject.transform,
                "RESTART",
                new Vector2(-590f, -126f),
                RestartModule);
            CreateDevelopmentButton(
                toolbarObject.transform,
                "RESTORE",
                new Vector2(-420f, -126f),
                coordinator.PressRestart);
            CreateDevelopmentButton(
                toolbarObject.transform,
                "LABELS",
                new Vector2(-250f, -126f),
                ToggleDebugLabels);
            CreateDevelopmentButton(
                toolbarObject.transform,
                "DIAG",
                new Vector2(-80f, -126f),
                coordinator.PressToggleDiagnostics);
            CreateDevelopmentButton(
                toolbarObject.transform,
                "DEVTP",
                new Vector2(90f, -126f),
                DevelopmentTeleportNext);
#endif
        }

        private void RestartModule()
        {
            Time.timeScale = 1f;
            FindAnyObjectByType<MovementFeedback>()?.PlayRetry();
            if (CampaignFlowTrial.Active) CampaignFlowTrial.Launch(_module.StableModuleId, CampaignFlowTrial.Mode);
            else ModuleSelectionState.Select(_module.StableModuleId, ModuleSelectionState.DevelopmentOverride);
            StartCoroutine(LoadScene(ModuleSelectionState.ModuleRunnerSceneName));
        }

        private void HandleBenchmarkWarp()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (UnityEngine.InputSystem.Keyboard.current == null
                || !UnityEngine.InputSystem.Keyboard.current.f6Key.wasPressedThisFrame)
            {
                return;
            }

            var motor = FindAnyObjectByType<ParkourMotor>();
            if (motor == null)
            {
                return;
            }

            DevelopmentTeleportNext();
#endif
        }

        private void DevelopmentTeleportNext()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var motor = FindAnyObjectByType<ParkourMotor>();
            if (motor == null)
            {
                return;
            }

            var benchmark = ResolveBenchmark(_benchmarkIndex++);
            motor.ResetMotion(benchmark.position, Quaternion.Euler(0f, benchmark.yaw, 0f), 0.18f);
            _runSession?.InvalidateForDevelopmentUse();
            _telemetry?.RecordDevelopmentTeleport();
            _diagnostics?.SetValue("Dev teleport", _visualBenchmarks.Count == 0 ? "fallback" : _visualBenchmarks[Mathf.Abs(_benchmarkIndex - 1) % _visualBenchmarks.Count].BenchmarkId);
#endif
        }

        private (Vector3 position, float yaw) ResolveBenchmark(int index)
        {
            if (_visualBenchmarks.Count > 0)
            {
                var anchor = _visualBenchmarks[Mathf.Abs(index) % _visualBenchmarks.Count];
                return (anchor.transform.position, anchor.Yaw);
            }

            var slot = Mathf.Abs(index) % 3;
            switch (slot)
            {
                case 0:
                    return (_module.StartPoint.Position + Vector3.up * 1.4f, 0f);
                case 1:
                    return (_module.StableModuleId == "module.003.flow-error"
                        ? new Vector3(1f, 3.8f, 58f)
                        : new Vector3(4f, 2f, 38f), 8f);
                default:
                    return (_module.StableModuleId == "module.003.flow-error"
                        ? new Vector3(-5f, 7.1f, 91f)
                        : _module.PatchBlock.Pose.Position + new Vector3(-4f, 1.6f, -8f), 18f);
            }
        }

        private System.Collections.IEnumerator LoadScene(string sceneName)
        {
            if (GameServices.Current != null
                && GameServices.Current.TryGet<ILevelLoader>(out var loader))
            {
                yield return loader.LoadAsync(sceneName);
                yield break;
            }

            yield return new UnitySceneLevelLoader().LoadAsync(sceneName);
        }

        private void ToggleDebugLabels()
        {
            _debugLabelsVisible = !_debugLabelsVisible;
            foreach (var label in _debugLabels)
            {
                if (label != null)
                {
                    label.SetActive(_debugLabelsVisible);
                }
            }
        }

        private void CreateWorldLabel(string label, Vector3 position, bool debugOnly)
        {
            var labelObject = new GameObject(label, typeof(TextMesh), typeof(WorldSpaceBillboardLabel));
            labelObject.transform.position = position;
            labelObject.transform.rotation = Quaternion.identity;
            var text = labelObject.GetComponent<TextMesh>();
            text.text = label;
            text.alignment = TextAlignment.Center;
            text.anchor = TextAnchor.MiddleCenter;
            text.fontSize = 38;
            text.characterSize = 0.048f;
            text.color = new Color(1f, 1f, 1f, debugOnly ? 0.62f : 0.86f);
            if (debugOnly)
            {
                labelObject.SetActive(_debugLabelsVisible);
                _debugLabels.Add(labelObject);
            }
        }

        private Material ParticleMaterialFor(ModuleMaterialRole role)
        {
            if (_particleMaterials.TryGetValue(role, out var material))
            {
                return material;
            }

            material = VisualMaterialUtility.CreateRuntimeMaterial(
                "RB Module Particle Burst " + role,
                _visuals.ColorFor(role),
                VisualMaterialUtility.ResolveParticleShader());
            _particleMaterials[role] = material;
            return material;
        }

        private void PublishDiagnostics(float deltaTime)
        {
            if (_diagnostics == null || _module == null || _telemetry == null)
            {
                return;
            }

            _diagnosticTimer -= deltaTime;
            if (_diagnosticTimer > 0f)
            {
                return;
            }

            _diagnosticTimer = 0.25f;
            RefreshPerformanceDiagnostics(deltaTime);
            _diagnostics.SetValue("Module", _module.StableModuleId);
            _diagnostics.SetValue("Module content version", _module.ContentVersion.ToString());
            _diagnostics.SetValue(
                "Run state",
                _runSession == null
                    ? "none"
                    : (_runSession.Timer.IsCompleted ? "completed" : (_runSession.Timer.IsRunning ? "running" : "stopped")));
            _diagnostics.SetValue(
                "Module timer",
                RunTimerFormatting.Format(_runSession?.Timer.ElapsedSeconds ?? _telemetry.ElapsedSeconds));
            _diagnostics.SetValue("Module falls", _telemetry.Falls.ToString());
            _diagnostics.SetValue(
                "Current achievable rank",
                ModuleRankUtility.Display(ModuleRankUtility.CurrentTarget(
                    _runSession?.Timer.ElapsedSeconds ?? 0d,
                    _module.RankThresholds)));
            _diagnostics.SetValue("Silver threshold", RunTimerFormatting.Format(_module.RankThresholds.SilverSeconds));
            _diagnostics.SetValue("Gold threshold", RunTimerFormatting.Format(_module.RankThresholds.GoldSeconds));
            _diagnostics.SetValue("Diamond threshold", RunTimerFormatting.Format(_module.RankThresholds.DiamondSeconds));
            _diagnostics.SetValue(
                "PB time",
                _progressRecord != null && _progressRecord.bestTimeSeconds > 0d
                    ? RunTimerFormatting.Format(_progressRecord.bestTimeSeconds)
                    : "none");
            _diagnostics.SetValue("Run validity", _runSession?.Validity.ToString() ?? "none");
            _diagnostics.SetValue("Rank threshold version", _module.RankThresholds.ThresholdVersion.ToString());
            _diagnostics.SetValue("Rank calibration", _module.RankThresholds.CalibrationState.ToString());
            _diagnostics.SetValue("Module report", _telemetry.LastExportPath ?? "pending");
            _diagnostics.SetValue("Visual objects", _visualObjectCount.ToString());
            _diagnostics.SetValue("Particle emitters", _particleEmitterCount.ToString());
            _diagnostics.SetValue("Renderers", _rendererCount.ToString());
            _diagnostics.SetValue("Shadow casters", _shadowCasterCount.ToString());
            _diagnostics.SetValue("Realtime lights", _realtimeLightCount.ToString());
            _diagnostics.SetValue("Colliders", _colliderCount.ToString());
            _diagnostics.SetValue("Active rigidbodies", _activeRigidbodyCount.ToString());
            _diagnostics.SetValue("Active particle systems", _activeParticleSystemCount.ToString());
            _diagnostics.SetValue("Boost uses", _telemetry.BoostsUsed.ToString());
            _diagnostics.SetValue("Water pulses", _telemetry.WaterPulses.ToString());
            _diagnostics.SetValue("Surf used", (_telemetry.SurfTicks > 0).ToString());
            _diagnostics.SetValue("Peak module speed", _telemetry.PeakHorizontalSpeed.ToString("0.00"));
            _diagnostics.SetValue("Visual quality profile", _environment.QualityProfile.ToString());
        }

        private void RefreshPerformanceDiagnostics(float deltaTime)
        {
            _performanceDiagnosticTimer -= deltaTime;
            if (_performanceDiagnosticTimer > 0f)
            {
                return;
            }

            _performanceDiagnosticTimer = 1.5f;
            var renderers = FindObjectsByType<Renderer>();
            _rendererCount = renderers.Length;
            _shadowCasterCount = renderers.Count(renderer =>
                renderer.enabled
                && renderer.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off);
            _realtimeLightCount = FindObjectsByType<Light>()
                .Count(light => light.enabled);
            _colliderCount = FindObjectsByType<Collider>().Length;
            _activeRigidbodyCount = FindObjectsByType<Rigidbody>()
                .Count(body => body.gameObject.activeInHierarchy && !body.IsSleeping());
            _activeParticleSystemCount = FindObjectsByType<ParticleSystem>()
                .Count(system => system.isPlaying);
        }

        private static GameObject CreateUiObject(
            string objectName,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            params Type[] extraComponents)
        {
            var types = new List<Type> { typeof(RectTransform) };
            types.AddRange(extraComponents);
            var gameObject = new GameObject(objectName, types.ToArray());
            gameObject.transform.SetParent(parent, false);
            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            return gameObject;
        }

        private static Text CreateText(
            string objectName,
            Transform parent,
            string value,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            var textObject = CreateUiObject(
                objectName,
                parent,
                anchorMin,
                anchorMax,
                Vector2.zero,
                Vector2.zero,
                typeof(Text));
            var rect = textObject.GetComponent<RectTransform>();
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            var text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Text CreateHudButton(
            Transform parent,
            string label,
            Vector2 anchor,
            Vector2 anchoredPosition,
            Vector2 size,
            UnityEngine.Events.UnityAction action,
            Color? color = null)
        {
            var buttonObject = CreateUiObject(
                label + " Button",
                parent,
                anchor,
                anchor,
                anchoredPosition,
                size,
                typeof(Image),
                typeof(Button));
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0.5f);
            buttonObject.GetComponent<Image>().color =
                color ?? new Color(0.08f, 0.12f, 0.22f, 0.82f);
            buttonObject.GetComponent<Button>().onClick.AddListener(action);
            var text = CreateText(
                label,
                buttonObject.transform,
                label,
                20,
                TextAnchor.MiddleCenter,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            text.raycastTarget = false;
            return text;
        }

        private static void CreateDevelopmentButton(
            Transform parent,
            string label,
            Vector2 anchoredPosition,
            UnityEngine.Events.UnityAction action)
        {
            var buttonObject = CreateUiObject(
                label + " Button",
                parent,
                Vector2.one,
                Vector2.one,
                anchoredPosition,
                new Vector2(150f, 48f),
                typeof(Image),
                typeof(Button));
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.pivot = Vector2.one;
            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.04f, 0.06f, 0.12f, 0.58f);
            var button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(action);
            var text = CreateText(
                label,
                buttonObject.transform,
                label,
                20,
                TextAnchor.MiddleCenter,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            text.raycastTarget = false;
        }

        private static void CreateJumpGlyph(RectTransform parent)
        {
            for (var index = 0; index < 3; index++)
            {
                var bar = CreateUiObject(
                    "Jump Chevron",
                    parent,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2((index - 1) * 26f, 12f - index * 10f),
                    new Vector2(20f, 54f),
                    typeof(Image));
                bar.GetComponent<RectTransform>().localRotation =
                    Quaternion.Euler(0f, 0f, index == 1 ? 0f : (index == 0 ? -35f : 35f));
                bar.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.78f);
            }
        }

        private static Sprite GetCircleRingSprite()
        {
            if (_circleRingSprite != null)
            {
                return _circleRingSprite;
            }

            _circleRingSprite = CreateCircleSprite("Module Ring", filled: false);
            return _circleRingSprite;
        }

        private static Sprite GetFilledCircleSprite()
        {
            if (_filledCircleSprite != null)
            {
                return _filledCircleSprite;
            }

            _filledCircleSprite = CreateCircleSprite("Module Filled Circle", filled: true);
            return _filledCircleSprite;
        }

        private static Sprite CreateCircleSprite(string name, bool filled)
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = name + " Texture",
                hideFlags = HideFlags.DontSave
            };
            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            var pixels = new Color[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x, y), center) / (size * 0.5f);
                    var alpha = filled
                        ? Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.88f, 1f, distance))
                        : Mathf.Clamp01(
                            Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.92f, 0.78f, distance))
                            * Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.92f, 1f, distance)));
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f);
        }
    }
}
