using System;
using System.Collections.Generic;
using Avoidance.Gameplay.Ranking;
using Avoidance.Gameplay.Visuals;
using Avoidance.Gameplay.Worlds;
using UnityEngine;

namespace Avoidance.Gameplay.Levels
{
    public enum ModuleDifficulty
    {
        Intro,
        Easy,
        Medium,
        Hard
    }

    public enum ModuleMechanic
    {
        StandardBlock,
        PrecisionBlock,
        MovingBlock,
        RestorePoint,
        PatchBlock,
        JumpBoost,
        FlowingWater,
        CrumblingBlock,
        Shortcut,
        CameraGuide,
        SurfSurface
    }

    public enum ModuleRewardKind
    {
        Currency,
        InventoryItem,
        Unlock
    }

    public enum ModulePathSpace
    {
        World,
        Local
    }

    public enum ModuleMovingLoopMode
    {
        PingPong,
        Loop
    }

    public enum ModuleEasing
    {
        Linear,
        SmoothStep
    }

    [Serializable]
    public sealed class ModuleRankThresholds
    {
        [SerializeField] private float _silverSeconds = 60f;
        [SerializeField] private float _goldSeconds = 45f;
        [SerializeField] private float _diamondSeconds = 35f;
        [SerializeField] private RankCalibrationState _calibrationState =
            RankCalibrationState.Uncalibrated;
        [SerializeField] private int _thresholdVersion = 1;
        [TextArea(2, 5)]
        [SerializeField] private string _designerNotes =
            "UNCALIBRATED provisional thresholds. Tune after physical Android timing data.";

        public ModuleRankThresholds(
            float silverSeconds,
            float goldSeconds,
            float diamondSeconds,
            RankCalibrationState calibrationState,
            int thresholdVersion,
            string designerNotes)
        {
            _silverSeconds = silverSeconds;
            _goldSeconds = goldSeconds;
            _diamondSeconds = diamondSeconds;
            _calibrationState = calibrationState;
            _thresholdVersion = thresholdVersion;
            _designerNotes = designerNotes;
        }

        public float SilverSeconds => _silverSeconds;
        public float GoldSeconds => _goldSeconds;
        public float DiamondSeconds => _diamondSeconds;
        public RankCalibrationState CalibrationState => _calibrationState;
        public int ThresholdVersion => _thresholdVersion;
        public string DesignerNotes => _designerNotes;
        public bool IsOrdered =>
            _diamondSeconds > 0f
            && _goldSeconds > 0f
            && _silverSeconds > 0f
            && _diamondSeconds < _goldSeconds
            && _goldSeconds < _silverSeconds
            && _thresholdVersion > 0;
    }

    [Serializable]
    public struct ModulePose
    {
        [SerializeField] private Vector3 _position;
        [SerializeField] private Vector3 _eulerAngles;

        public ModulePose(Vector3 position, Vector3 eulerAngles)
        {
            _position = position;
            _eulerAngles = eulerAngles;
        }

        public Vector3 Position => _position;
        public Vector3 EulerAngles => _eulerAngles;
        public Quaternion Rotation => Quaternion.Euler(_eulerAngles);
    }

    [Serializable]
    public sealed class ModuleBlockDefinition
    {
        [SerializeField] private string _stableId;
        [SerializeField] private ModulePose _pose;
        [SerializeField] private Vector3 _size = Vector3.one;
        [SerializeField] private ModuleMaterialRole _visualRole = ModuleMaterialRole.Normal;
        [SerializeField] private bool _editorLabel;
        [SerializeField] private string _label;

        public ModuleBlockDefinition(
            string stableId,
            Vector3 position,
            Vector3 size,
            ModuleMaterialRole visualRole,
            string label = "",
            bool editorLabel = false,
            Vector3 eulerAngles = default)
        {
            _stableId = stableId;
            _pose = new ModulePose(position, eulerAngles);
            _size = size;
            _visualRole = visualRole;
            _label = label;
            _editorLabel = editorLabel;
        }

        public string StableId => _stableId;
        public ModulePose Pose => _pose;
        public Vector3 Size => _size;
        public ModuleMaterialRole VisualRole => _visualRole;
        public bool EditorLabel => _editorLabel;
        public string Label => _label;
    }

    [Serializable]
    public sealed class ModuleRestorePointDefinition
    {
        [SerializeField] private string _stableId;
        [SerializeField] private int _order;
        [SerializeField] private ModulePose _pose;
        [SerializeField] private string _supportBlockStableId;
        [SerializeField] private Vector3 _restoreOffset = new Vector3(0f, 0.35f, 0f);

        public ModuleRestorePointDefinition(
            string stableId,
            int order,
            Vector3 position,
            Vector3 restoreOffset,
            Vector3 eulerAngles = default,
            string supportBlockStableId = null)
        {
            _stableId = stableId;
            _supportBlockStableId = supportBlockStableId;
            _order = order;
            _pose = new ModulePose(position, eulerAngles);
            _restoreOffset = restoreOffset;
        }

        public string StableId => _stableId;
        public string SupportBlockStableId => _supportBlockStableId;
        public int Order => _order;
        public ModulePose Pose => _pose;
        public Vector3 RestoreOffset => _restoreOffset;
        public Vector3 RestorePosition => _pose.Position + _restoreOffset;
    }

    [Serializable]
    public sealed class ModulePatchBlockDefinition
    {
        [SerializeField] private string _stableId = "patch.default";
        [SerializeField] private ModulePose _pose;
        [SerializeField] private string _supportBlockStableId;
        [SerializeField] private Vector3 _size = new Vector3(2.4f, 2.4f, 2.4f);

        public ModulePatchBlockDefinition(
            string stableId,
            Vector3 position,
            Vector3 size,
            Vector3 eulerAngles = default,
            string supportBlockStableId = null)
        {
            _stableId = stableId;
            _supportBlockStableId = supportBlockStableId;
            _pose = new ModulePose(position, eulerAngles);
            _size = size;
        }

        public string StableId => _stableId;
        public string SupportBlockStableId => _supportBlockStableId;
        public ModulePose Pose => _pose;
        public Vector3 Size => _size;
    }

    [Serializable]
    public sealed class ModuleMovingBlockDefinition
    {
        [SerializeField] private string _stableId;
        [SerializeField] private ModulePose _pose;
        [SerializeField] private Vector3 _size = new Vector3(3f, 0.6f, 3f);
        [SerializeField] private Vector3[] _pathPoints = Array.Empty<Vector3>();
        [SerializeField] private ModulePathSpace _pathSpace = ModulePathSpace.World;
        [SerializeField] private ModuleMovingLoopMode _loopMode = ModuleMovingLoopMode.PingPong;
        [SerializeField] private ModuleEasing _easing = ModuleEasing.SmoothStep;
        [SerializeField] private float _speed = 2.5f;
        [SerializeField] private float _pauseAtEndpoints = 0.15f;
        [SerializeField] private float _startingPhase;

        public ModuleMovingBlockDefinition(
            string stableId,
            Vector3 position,
            Vector3 size,
            Vector3[] pathPoints,
            float speed,
            float pauseAtEndpoints,
            ModulePathSpace pathSpace = ModulePathSpace.World,
            ModuleMovingLoopMode loopMode = ModuleMovingLoopMode.PingPong,
            ModuleEasing easing = ModuleEasing.SmoothStep,
            float startingPhase = 0f)
        {
            _stableId = stableId;
            _pose = new ModulePose(position, Vector3.zero);
            _size = size;
            _pathPoints = pathPoints ?? Array.Empty<Vector3>();
            _speed = speed;
            _pauseAtEndpoints = pauseAtEndpoints;
            _pathSpace = pathSpace;
            _loopMode = loopMode;
            _easing = easing;
            _startingPhase = startingPhase;
        }

        public string StableId => _stableId;
        public ModulePose Pose => _pose;
        public Vector3 Size => _size;
        public IReadOnlyList<Vector3> PathPoints => _pathPoints;
        public ModulePathSpace PathSpace => _pathSpace;
        public ModuleMovingLoopMode LoopMode => _loopMode;
        public ModuleEasing Easing => _easing;
        public float Speed => _speed;
        public float PauseAtEndpoints => _pauseAtEndpoints;
        public float StartingPhase => _startingPhase;
    }

    [Serializable]
    public sealed class ModuleBoostBlockDefinition
    {
        [SerializeField] private string _stableId;
        [SerializeField] private ModulePose _pose;
        [SerializeField] private Vector3 _size = new Vector3(3f, 0.6f, 3f);
        [SerializeField] private Vector3 _launchDirection = Vector3.forward;
        [SerializeField] private float _verticalStrength = 12f;
        [SerializeField] private float _horizontalStrength = 7f;
        [SerializeField] private float _cooldown = 0.35f;

        public ModuleBoostBlockDefinition(
            string stableId,
            Vector3 position,
            Vector3 size,
            Vector3 launchDirection,
            float verticalStrength,
            float horizontalStrength,
            float cooldown,
            Vector3 eulerAngles = default)
        {
            _stableId = stableId;
            _pose = new ModulePose(position, eulerAngles);
            _size = size;
            _launchDirection = launchDirection;
            _verticalStrength = verticalStrength;
            _horizontalStrength = horizontalStrength;
            _cooldown = cooldown;
        }

        public string StableId => _stableId;
        public ModulePose Pose => _pose;
        public Vector3 Size => _size;
        public Vector3 LaunchDirection => _launchDirection;
        public float VerticalStrength => _verticalStrength;
        public float HorizontalStrength => _horizontalStrength;
        public float Cooldown => _cooldown;
    }

    [Serializable]
    public sealed class ModuleWaterVolumeDefinition
    {
        [SerializeField] private string _stableId;
        [SerializeField] private ModulePose _pose;
        [SerializeField] private Vector3 _size = new Vector3(5f, 1.4f, 8f);
        [SerializeField] private Vector3 _flowDirection = Vector3.forward;
        [SerializeField] private float _flowForce = 16f;
        [SerializeField] private float _maxAddedVelocity = 5f;
        [SerializeField] private float _verticalInfluence;
        [SerializeField] private float _drag = 1f;

        public ModuleWaterVolumeDefinition(
            string stableId,
            Vector3 position,
            Vector3 size,
            Vector3 flowDirection,
            float flowForce,
            float maxAddedVelocity,
            float verticalInfluence,
            float drag,
            Vector3 eulerAngles = default)
        {
            _stableId = stableId;
            _pose = new ModulePose(position, eulerAngles);
            _size = size;
            _flowDirection = flowDirection;
            _flowForce = flowForce;
            _maxAddedVelocity = maxAddedVelocity;
            _verticalInfluence = verticalInfluence;
            _drag = drag;
        }

        public string StableId => _stableId;
        public ModulePose Pose => _pose;
        public Vector3 Size => _size;
        public Vector3 FlowDirection => _flowDirection;
        public float FlowForce => _flowForce;
        public float MaxAddedVelocity => _maxAddedVelocity;
        public float VerticalInfluence => _verticalInfluence;
        public float Drag => _drag;
    }

    [Serializable]
    public sealed class ModuleSurfSurfaceDefinition
    {
        [SerializeField] private string _stableId;
        [SerializeField] private ModulePose _pose;
        [SerializeField] private Vector3 _size = new Vector3(4f, 0.45f, 10f);
        [SerializeField] private Vector3 _flowDirection = Vector3.forward;

        public ModuleSurfSurfaceDefinition(
            string stableId,
            Vector3 position,
            Vector3 size,
            Vector3 flowDirection,
            Vector3 eulerAngles)
        {
            _stableId = stableId;
            _pose = new ModulePose(position, eulerAngles);
            _size = size;
            _flowDirection = flowDirection;
        }

        public string StableId => _stableId;
        public ModulePose Pose => _pose;
        public Vector3 Size => _size;
        public Vector3 FlowDirection => _flowDirection;
    }

    [Serializable]
    public sealed class ModuleCrumblingBlockDefinition
    {
        [SerializeField] private string _stableId;
        [SerializeField] private ModulePose _pose;
        [SerializeField] private Vector3 _size = new Vector3(2.6f, 0.6f, 2.6f);
        [SerializeField] private float _activationDelay = 0.03f;
        [SerializeField] private float _fallDelay = 1f;
        [SerializeField] private float _resetTime = 4f;
        [SerializeField] private float _shakeAmount = 0.025f;

        public ModuleCrumblingBlockDefinition(
            string stableId,
            Vector3 position,
            Vector3 size,
            float activationDelay,
            float fallDelay,
            float resetTime,
            float shakeAmount,
            Vector3 eulerAngles = default)
        {
            _stableId = stableId;
            _pose = new ModulePose(position, eulerAngles);
            _size = size;
            _activationDelay = activationDelay;
            _fallDelay = fallDelay;
            _resetTime = resetTime;
            _shakeAmount = shakeAmount;
        }

        public string StableId => _stableId;
        public ModulePose Pose => _pose;
        public Vector3 Size => _size;
        public float ActivationDelay => _activationDelay;
        public float FallDelay => _fallDelay;
        public float ResetTime => _resetTime;
        public float ShakeAmount => _shakeAmount;
    }

    [Serializable]
    public sealed class ModuleShortcutDefinition
    {
        [SerializeField] private string _stableId;
        [SerializeField] private string _displayName;
        [SerializeField] private Vector3 _entryPosition;
        [SerializeField] private Vector3 _exitPosition;
        [SerializeField] private float _estimatedTimeSaved;
        [SerializeField] private ModuleMechanic[] _mechanics = Array.Empty<ModuleMechanic>();

        public ModuleShortcutDefinition(
            string stableId,
            string displayName,
            Vector3 entryPosition,
            Vector3 exitPosition,
            float estimatedTimeSaved,
            ModuleMechanic[] mechanics)
        {
            _stableId = stableId;
            _displayName = displayName;
            _entryPosition = entryPosition;
            _exitPosition = exitPosition;
            _estimatedTimeSaved = estimatedTimeSaved;
            _mechanics = mechanics ?? Array.Empty<ModuleMechanic>();
        }

        public string StableId => _stableId;
        public string DisplayName => _displayName;
        public Vector3 EntryPosition => _entryPosition;
        public Vector3 ExitPosition => _exitPosition;
        public float EstimatedTimeSaved => _estimatedTimeSaved;
        public IReadOnlyList<ModuleMechanic> Mechanics => _mechanics;
    }

    [Serializable]
    public sealed class ModuleCameraHintDefinition
    {
        [SerializeField] private string _stableId;
        [SerializeField] private Vector3 _position;
        [SerializeField] private Vector3 _size = new Vector3(5f, 4f, 5f);
        [SerializeField] private float _preferredYaw;
        [SerializeField] private float _preferredPitch;
        [SerializeField] private float _maxAssistDegrees = 12f;

        public ModuleCameraHintDefinition(
            string stableId,
            Vector3 position,
            Vector3 size,
            float preferredYaw,
            float preferredPitch,
            float maxAssistDegrees)
        {
            _stableId = stableId;
            _position = position;
            _size = size;
            _preferredYaw = preferredYaw;
            _preferredPitch = preferredPitch;
            _maxAssistDegrees = maxAssistDegrees;
        }

        public string StableId => _stableId;
        public Vector3 Position => _position;
        public Vector3 Size => _size;
        public float PreferredYaw => _preferredYaw;
        public float PreferredPitch => _preferredPitch;
        public float MaxAssistDegrees => _maxAssistDegrees;
    }

    [Serializable]
    public sealed class ModuleCompletionRewardDefinition
    {
        [SerializeField] private string _stableId;
        [SerializeField] private ModuleRewardKind _kind = ModuleRewardKind.Currency;
        [SerializeField] private string _contentId;
        [SerializeField] private int _quantity = 1;
        [SerializeField] private ModuleRank _minimumRank = ModuleRank.Bronze;
        [SerializeField] private bool _firstCompletionOnly = true;
        [SerializeField] private string _unlockType;

        public ModuleCompletionRewardDefinition(
            string stableId,
            ModuleRewardKind kind,
            string contentId,
            int quantity,
            ModuleRank minimumRank = ModuleRank.Bronze,
            bool firstCompletionOnly = true,
            string unlockType = "")
        {
            _stableId = stableId;
            _kind = kind;
            _contentId = contentId;
            _quantity = quantity;
            _minimumRank = minimumRank;
            _firstCompletionOnly = firstCompletionOnly;
            _unlockType = unlockType;
        }

        public string StableId => _stableId;
        public ModuleRewardKind Kind => _kind;
        public string ContentId => _contentId;
        public int Quantity => _quantity;
        public ModuleRank MinimumRank => _minimumRank;
        public bool FirstCompletionOnly => _firstCompletionOnly;
        public string UnlockType => _unlockType;
    }

    [CreateAssetMenu(menuName = "RYDERS BLOCK/Module Definition")]
    public sealed class ModuleDefinition : ScriptableObject
    {
        [SerializeField] private string _stableModuleId;
        [SerializeField] private string _displayName;
        [SerializeField] private string _internalName;
        [SerializeField] private string _projectId = "project.ryders-block";
        [SerializeField] private string _worldId = "world.prototype-sky";
        [SerializeField] private int _contentVersion = 1;
        [SerializeField] private string _sceneName = "ModuleRunner";
        [SerializeField] private ModuleDifficulty _difficulty = ModuleDifficulty.Intro;
        [SerializeField] private ModulePose _startPoint;
        [SerializeField] private string _startAnchorStableId;
        [SerializeField] private string _startSupportBlockStableId;
        [SerializeField] private bool _useCustomFallThreshold;
        [SerializeField] private float _fallThresholdY = -12f;
        [SerializeField] private bool _disableAutomaticDistantFragments;
        [SerializeField] private ModulePatchBlockDefinition _patchBlock;
        [SerializeField] private ModuleRestorePointDefinition[] _restorePoints = Array.Empty<ModuleRestorePointDefinition>();
        [SerializeField] private float _expectedCleanTime = 30f;
        [SerializeField] private float _estimatedCasualTime = 60f;
        [SerializeField] private ModuleRankThresholds _rankThresholds =
            new ModuleRankThresholds(
                60f,
                45f,
                35f,
                RankCalibrationState.Uncalibrated,
                1,
                "UNCALIBRATED provisional thresholds.");
        [SerializeField] private ModuleMechanic[] _mechanicsUsed = Array.Empty<ModuleMechanic>();
        [SerializeField] private ModuleShortcutDefinition[] _optionalShortcuts = Array.Empty<ModuleShortcutDefinition>();
        [SerializeField] private ModuleEnvironmentProfile _environmentProfile;
        [SerializeField] private EnvironmentBiomeProfile _environmentBiomeProfile;
        [SerializeField] private ModuleVisualProfile _visualProfile;
        [TextArea(2, 5)]
        [SerializeField] private string _loreBugDescription;
        [TextArea(2, 6)]
        [SerializeField] private string _developerNotes;
        [SerializeField] private ModuleBlockDefinition[] _blocks = Array.Empty<ModuleBlockDefinition>();
        [SerializeField] private ModuleMovingBlockDefinition[] _movingBlocks = Array.Empty<ModuleMovingBlockDefinition>();
        [SerializeField] private ModuleBoostBlockDefinition[] _boostBlocks = Array.Empty<ModuleBoostBlockDefinition>();
        [SerializeField] private ModuleWaterVolumeDefinition[] _waterVolumes = Array.Empty<ModuleWaterVolumeDefinition>();
        [SerializeField] private ModuleSurfSurfaceDefinition[] _surfSurfaces = Array.Empty<ModuleSurfSurfaceDefinition>();
        [SerializeField] private ModuleCrumblingBlockDefinition[] _crumblingBlocks = Array.Empty<ModuleCrumblingBlockDefinition>();
        [SerializeField] private ModuleBlockDefinition[] _decorations = Array.Empty<ModuleBlockDefinition>();
        [SerializeField] private ModuleCameraHintDefinition[] _cameraHints = Array.Empty<ModuleCameraHintDefinition>();
        [SerializeField] private ModuleCompletionRewardDefinition[] _completionRewards =
            Array.Empty<ModuleCompletionRewardDefinition>();

        public string StableModuleId => _stableModuleId;
        public string DisplayName => _displayName;
        public string InternalName => _internalName;
        public string ProjectId => _projectId;
        public string WorldId => _worldId;
        public int ContentVersion => _contentVersion;
        public string SceneName => _sceneName;
        public ModuleDifficulty Difficulty => _difficulty;
        public ModulePose StartPoint => _startPoint;
        public string StartAnchorStableId => string.IsNullOrWhiteSpace(_startAnchorStableId)
            ? $"start.{_stableModuleId}"
            : _startAnchorStableId;
        public string StartSupportBlockStableId => _startSupportBlockStableId;
        public bool DisableAutomaticDistantFragments => _disableAutomaticDistantFragments;
        public ModulePatchBlockDefinition PatchBlock => _patchBlock;
        public IReadOnlyList<ModuleRestorePointDefinition> RestorePoints => _restorePoints;
        public float ExpectedCleanTime => _expectedCleanTime;
        public float EstimatedCasualTime => _estimatedCasualTime;
        public ModuleRankThresholds RankThresholds => _rankThresholds;
        public IReadOnlyList<ModuleMechanic> MechanicsUsed => _mechanicsUsed;
        public IReadOnlyList<ModuleShortcutDefinition> OptionalShortcuts => _optionalShortcuts;
        public ModuleEnvironmentProfile EnvironmentProfile => _environmentProfile;
        public EnvironmentBiomeProfile EnvironmentBiomeProfile => _environmentBiomeProfile;
        public ModuleVisualProfile VisualProfile => _visualProfile;
        public string LoreBugDescription => _loreBugDescription;
        public string DeveloperNotes => _developerNotes;
        public IReadOnlyList<ModuleBlockDefinition> Blocks => _blocks;
        public IReadOnlyList<ModuleMovingBlockDefinition> MovingBlocks => _movingBlocks;
        public IReadOnlyList<ModuleBoostBlockDefinition> BoostBlocks => _boostBlocks;
        public IReadOnlyList<ModuleWaterVolumeDefinition> WaterVolumes => _waterVolumes;
        public IReadOnlyList<ModuleSurfSurfaceDefinition> SurfSurfaces => _surfSurfaces;
        public IReadOnlyList<ModuleCrumblingBlockDefinition> CrumblingBlocks => _crumblingBlocks;
        public IReadOnlyList<ModuleBlockDefinition> Decorations => _decorations;
        public IReadOnlyList<ModuleCameraHintDefinition> CameraHints => _cameraHints;
        public IReadOnlyList<ModuleCompletionRewardDefinition> CompletionRewards => _completionRewards;

        public void ConfigureRankThresholds(ModuleRankThresholds thresholds)
        {
            _rankThresholds = thresholds;
        }

        public void ConfigureSurfSurfaces(ModuleSurfSurfaceDefinition[] surfSurfaces)
        {
            _surfSurfaces = surfSurfaces ?? Array.Empty<ModuleSurfSurfaceDefinition>();
        }

        public void ConfigureCompletionRewards(ModuleCompletionRewardDefinition[] rewards)
        {
            _completionRewards = rewards ?? Array.Empty<ModuleCompletionRewardDefinition>();
        }

        public void ConfigureEnvironmentBiome(EnvironmentBiomeProfile environmentBiomeProfile)
        {
            _environmentBiomeProfile = environmentBiomeProfile;
        }

        public void ConfigurePlayability(
            string startAnchorStableId,
            string startSupportBlockStableId,
            float fallThresholdY,
            bool disableAutomaticDistantFragments)
        {
            _startAnchorStableId = startAnchorStableId;
            _startSupportBlockStableId = startSupportBlockStableId;
            _useCustomFallThreshold = true;
            _fallThresholdY = fallThresholdY;
            _disableAutomaticDistantFragments = disableAutomaticDistantFragments;
        }

        public float ResolveFallThreshold(float fallbackThresholdY)
        {
            return _useCustomFallThreshold ? _fallThresholdY : fallbackThresholdY;
        }

        public void Configure(
            string stableModuleId,
            string displayName,
            string internalName,
            string projectId,
            string worldId,
            int contentVersion,
            string sceneName,
            ModuleDifficulty difficulty,
            ModulePose startPoint,
            ModulePatchBlockDefinition patchBlock,
            ModuleRestorePointDefinition[] restorePoints,
            float expectedCleanTime,
            float estimatedCasualTime,
            ModuleMechanic[] mechanicsUsed,
            ModuleShortcutDefinition[] optionalShortcuts,
            ModuleEnvironmentProfile environmentProfile,
            ModuleVisualProfile visualProfile,
            string loreBugDescription,
            string developerNotes,
            ModuleBlockDefinition[] blocks,
            ModuleMovingBlockDefinition[] movingBlocks,
            ModuleBoostBlockDefinition[] boostBlocks,
            ModuleWaterVolumeDefinition[] waterVolumes,
            ModuleCrumblingBlockDefinition[] crumblingBlocks,
            ModuleBlockDefinition[] decorations,
            ModuleCameraHintDefinition[] cameraHints)
        {
            _stableModuleId = stableModuleId;
            _displayName = displayName;
            _internalName = internalName;
            _projectId = projectId;
            _worldId = worldId;
            _contentVersion = contentVersion;
            _sceneName = sceneName;
            _difficulty = difficulty;
            _startPoint = startPoint;
            _patchBlock = patchBlock;
            _restorePoints = restorePoints ?? Array.Empty<ModuleRestorePointDefinition>();
            _expectedCleanTime = expectedCleanTime;
            _estimatedCasualTime = estimatedCasualTime;
            _mechanicsUsed = mechanicsUsed ?? Array.Empty<ModuleMechanic>();
            _optionalShortcuts = optionalShortcuts ?? Array.Empty<ModuleShortcutDefinition>();
            _environmentProfile = environmentProfile;
            _visualProfile = visualProfile;
            _loreBugDescription = loreBugDescription;
            _developerNotes = developerNotes;
            _blocks = blocks ?? Array.Empty<ModuleBlockDefinition>();
            _movingBlocks = movingBlocks ?? Array.Empty<ModuleMovingBlockDefinition>();
            _boostBlocks = boostBlocks ?? Array.Empty<ModuleBoostBlockDefinition>();
            _waterVolumes = waterVolumes ?? Array.Empty<ModuleWaterVolumeDefinition>();
            _surfSurfaces = Array.Empty<ModuleSurfSurfaceDefinition>();
            _crumblingBlocks = crumblingBlocks ?? Array.Empty<ModuleCrumblingBlockDefinition>();
            _decorations = decorations ?? Array.Empty<ModuleBlockDefinition>();
            _cameraHints = cameraHints ?? Array.Empty<ModuleCameraHintDefinition>();
        }
    }
}
