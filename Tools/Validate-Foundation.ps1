[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$errors = [System.Collections.Generic.List[string]]::new()

function Require-Path {
    param([string]$RelativePath)

    $absolutePath = Join-Path $projectRoot $RelativePath
    if (-not (Test-Path -LiteralPath $absolutePath)) {
        $errors.Add("Missing required path: $RelativePath")
    }
}

function Read-Text {
    param([string]$RelativePath)

    Get-Content -Raw -LiteralPath (Join-Path $projectRoot $RelativePath)
}

$requiredPaths = @(
    'AGENTS.md',
    'ARCHITECTURE.md',
    'ROADMAP.md',
    'CHANGELOG.md',
    'TESTING.md',
    'KNOWN_ISSUES.md',
    'SAVE_SCHEMA.md',
    'ART_DIRECTION.md',
    'ART_ASSET_MANIFEST.md',
    'BETA_FEEDBACK.md',
    'ALPHA_QA.md',
    'LORE_AND_WORLD.md',
    'MOVEMENT_LAB.md',
    'LEVEL_AUTHORING.md',
    'CAMERA_DESIGN.md',
    'Docs/PHASE_1_COMPLETION.md',
    'Docs/PHASE_1B_DEVICE_RESCUE.md',
    'Docs/PHASE_2_FIRST_MODULES.md',
    'Docs/PHASE_3_RANKS_AND_PROGRESSION.md',
    'Docs/PHASE_3B_VISUAL_SLICE.md',
    'Docs/PHASE_0_3_7_PARKOUR_MOVEMENT.md',
    'Docs/PHASE_0_3_8_MOVEMENT_LOCK.md',
    'Docs/PHASE_0_3_9_MOVEMENT_V1.md',
    'Docs/PHASE_0_4_0_VISUAL_IDENTITY.md',
    'Docs/PHASE_0_4_5_SPIRAL_SHOWCASE.md',
    'Docs/PHASE_0_4_6_ALPHA_VERTICAL_SLICE.md',
    'Docs/PHASE_0_4_7_BRAND_ALPHA_PRESENTATION.md',
    'Docs/PHASE_0_5_0_MESHY_ART_KIT.md',
    'Docs/PHASE_0_8_1_MODULE_003_HAND_AUTHORED_ANCIENT_ABYSS.md',
    'MOVEMENT_DESIGN.md',
    'SPIRAL_SHOWCASE_DESIGN.md',
    'VISUAL_BENCHMARKS.md',
    'RANKING_AND_SCORING.md',
    'Docs/GAMEPLAY_VISUAL_ROLE_MAP.md',
    'Docs/Decisions/0006-phase-2-data-driven-modules.md',
    'Docs/Decisions/0007-phase-3-bronze-progression-and-raw-time.md',
    'Docs/Decisions/0008-movement-lock-auto-camera.md',
    'Docs/Decisions/0009-movement-v1-default-controls.md',
    'Docs/Decisions/0010-visual-identity-profile-boundary.md',
    'Docs/Decisions/0011-visual-production-and-flow-skill-boundary.md',
    'Packages/manifest.json',
    'ProjectSettings/ProjectVersion.txt',
    'ProjectSettings/EditorSettings.asset',
    'ProjectSettings/ProjectSettings.asset',
    'ProjectSettings/EditorBuildSettings.asset',
    'Assets/_Game/Levels/Scenes/Bootstrap.unity',
    'Assets/_Game/Levels/Scenes/ModuleSelector.unity',
    'Assets/_Game/Levels/Scenes/MovementLab.unity',
    'Assets/_Game/Levels/Scenes/ModuleRunner.unity',
    'Assets/_Game/Levels/Scenes/FoundationTest.unity',
    'Assets/_Game/Levels/Scenes/PlatformTruthLab.unity',
    'Assets/_Game/Levels/Scenes/GameplayRoleGallery.unity',
    'Assets/_Game/Player/Movement/ParkourMotor.cs',
    'Assets/_Game/Input/PlayerInputRouter.cs',
    'Assets/_Game/UI/Touch/TouchInputCoordinator.cs',
    'Assets/_Game/Camera/ParkourCameraProfile.cs',
    'Assets/_Game/Camera/RouteCameraGraph.cs',
    'Assets/_Game/Camera/BranchIntentResolver.cs',
    'Assets/_Game/Camera/SmartParkourCameraController.cs',
    'Assets/_Game/Camera/RouteCameraDebugView.cs',
    'Assets/_Game/Respawn/RestoreController.cs',
    'Assets/_Game/Levels/ModuleDefinition.cs',
    'Assets/_Game/Levels/ModuleDefinitionValidator.cs',
    'Assets/_Game/Levels/ModuleSelectionState.cs',
    'Assets/_Game/Levels/PatchBlock.cs',
    'Assets/_Game/Levels/ShortcutTrigger.cs',
    'Assets/_Game/Levels/CameraGuideHint.cs',
    'Assets/_Game/Blocks/IModuleResettable.cs',
    'Assets/_Game/Blocks/MovingBlock.cs',
    'Assets/_Game/Blocks/JumpBoostBlock.cs',
    'Assets/_Game/Blocks/WaterFlowVolume.cs',
    'Assets/_Game/Blocks/SurfSurface.cs',
    'Assets/_Game/Blocks/CrumblingBlock.cs',
    'Assets/_Game/Ranking/ModuleRank.cs',
    'Assets/_Game/Timing/RunTimerService.cs',
    'Assets/_Game/Timing/ModuleRunSession.cs',
    'Assets/_Game/Timing/ModuleAttemptTelemetry.cs',
    'Assets/_Game/UI/BrandPresentation.cs',
    'Assets/_Game/UI/DevelopmentModuleSelector.cs',
    'Assets/_Game/UI/ModuleSceneController.cs',
    'Assets/_Game/UI/ModuleHud.cs',
    'Assets/_Game/Visuals/ModuleVisualProfile.cs',
    'Assets/_Game/Visuals/ModuleVisualPrefabLibrary.cs',
    'Assets/_Game/Visuals/ModuleEnvironmentProfile.cs',
    'Assets/_Game/Visuals/ModuleMaterialRole.cs',
    'Assets/_Game/Visuals/FirstPersonHands.cs',
    'Assets/_Game/Visuals/FirstPersonArmProfile.cs',
    'Assets/_Game/EditorTools/RankCalibrationWindow.cs',
    'Assets/_Game/EditorTools/MeshyArtKitIntegrator.cs',
    'Assets/_Game/SaveSystem/Migrations/SaveV2ToV3Migration.cs',
    'Assets/_Game/SaveSystem/SavePathMigrationUtility.cs',
    'Assets/Branding/Source/RydersRoad_Logo_Source.png',
    'Assets/Branding/Source/RydersRoad_AppIcon_Source.png',
    'Assets/Branding/Runtime/RydersRoad_Logo_UI.png',
    'Assets/Branding/Resources/Branding/RydersRoad_Logo_UI.png',
    'Assets/Branding/Android/RydersRoad_Icon_Legacy.png',
    'Assets/Branding/Android/RydersRoad_Icon_AdaptiveForeground.png',
    'Assets/Branding/Android/RydersRoad_Icon_AdaptiveBackground.png',
    'Docs/Branding/RydersRoad_IconMask_circle.png',
    'Docs/Branding/RydersRoad_IconMask_rounded-square.png',
    'Docs/Branding/RydersRoad_IconMask_squircle.png',
    'Docs/Branding/RydersRoad_IconMask_samsung-like.png',
    'Assets/_Game/Levels/Resources/Modules/Module_001_FirstSteps.asset',
    'Assets/_Game/Levels/Resources/Modules/Module_002_MovingParts.asset',
    'Assets/_Game/Levels/Resources/Modules/Module_003_FlowError.asset',
    'Assets/_Game/Levels/Resources/Modules/Module_004_TheSpiral.asset',
    'Assets/_Game/Visuals/Resources/MovementLabVisualProfile.asset',
    'Assets/_Game/Visuals/Resources/ModuleVisualProfile.asset',
    'Assets/_Game/Visuals/Resources/ModuleVisualPrefabLibrary.asset',
    'Assets/_Game/Visuals/Resources/ModuleEnvironmentProfile.asset',
    'Assets/_Game/Visuals/Resources/RB_URP_Lit_Reference.mat',
    'Assets/_Game/Visuals/Resources/RB_URP_Particle_Reference.mat',
    'Assets/_Game/Visuals/Resources/Textures/RydersRoad_RouteStone_02.png',
    'Assets/_Game/Visuals/Resources/Textures/RydersRoad_GoldRoute_01.png',
    'Assets/_Game/Visuals/Resources/Textures/RydersRoad_CyanEnergy_01.png',
    'Assets/_Game/Levels/Scenes/ArtKitGallery.unity',
    'Assets/_Game/Art/Environment/Platforms/PF_RR_Block_Standard.prefab',
    'Assets/_Game/Art/Environment/Platforms/PF_RR_Platform_Long.prefab',
    'Assets/_Game/Art/Environment/Architecture/PF_RR_Pillar_Tall.prefab',
    'Assets/_Game/Art/Environment/Architecture/PF_RR_RuinedTower.prefab',
    'Assets/_Game/Art/Environment/Architecture/PF_RR_FloatingArch.prefab',
    'Assets/_Game/Art/Environment/Decoration/PF_RR_Rock_Broken.prefab',
    'Assets/_Game/Art/Environment/Decoration/PF_RR_EnergyPillar.prefab',
    'Assets/_Game/Art/Environment/Nature/PF_RR_Tree_Floating.prefab',
    'Assets/_Game/Art/Environment/Nature/PF_RR_GrassTopper.prefab',
    'Assets/_Game/Art/Gameplay/Restore/PF_RR_RestorePoint.prefab',
    'Assets/_Game/Art/Gameplay/Boost/PF_RR_BoostPad.prefab',
    'Assets/_Game/Art/Gameplay/Patch/PF_RR_PatchBlock.prefab',
    'Assets/_Game/Art/Materials/MAT_RR_Block_Standard.mat',
    'Assets/_Game/Art/Materials/MAT_RR_Platform_Long.mat',
    'Assets/_Game/Art/Materials/MAT_RR_Pillar_Tall.mat',
    'Assets/_Game/Art/Materials/MAT_RR_Rock_Broken.mat',
    'Assets/_Game/Art/Materials/MAT_RR_Tree_Floating.mat',
    'Assets/_Game/Art/Materials/MAT_RR_GrassTopper.mat',
    'Assets/_Game/Art/Materials/MAT_RR_RestorePoint.mat',
    'Assets/_Game/Art/Materials/MAT_RR_BoostPad.mat',
    'Assets/_Game/Art/Materials/MAT_RR_PatchBlock.mat',
    'Assets/_Game/Art/Materials/MAT_RR_RuinedTower.mat',
    'Assets/_Game/Art/Materials/MAT_RR_FloatingArch.mat',
    'Assets/_Game/Art/Materials/MAT_RR_EnergyPillar.mat',
    'Assets/_Game/Art/FirstPerson/Arms/PF_RR_FP_Arm_Left.prefab',
    'Assets/_Game/Art/FirstPerson/Arms/PF_RR_FP_Arm_Right.prefab',
    'Assets/_Game/Art/Generated/FirstPerson/RR_FP_Arm_Left_00.asset',
    'Assets/_Game/Art/Generated/FirstPerson/RR_FP_Arm_Right_00.asset',
    'Assets/_Game/Visuals/Resources/FirstPersonArmProfile.asset',
    'Assets/_Game/Art/Materials/MAT_RR_FP_Arm_Right.mat',
    'Assets/_Game/Art/Textures/RR_FP_Arm_Right_Base.png',
    'Assets/_Game/Art/Textures/RR_FP_Arm_Right_Normal.png',
    'Assets/_Game/Art/Textures/RR_FP_Arm_Right_Metallic.png',
    'Assets/_Game/Art/Textures/RR_FP_Arm_Right_Roughness.png',
    'Assets/_Game/Art/Generated/EmissionMasks/RR_FP_Arm_Right_CyanEmission.png'
)
$requiredPaths | ForEach-Object { Require-Path $_ }

if (-not (Test-Path -LiteralPath (Join-Path $projectRoot 'Assets/_Game/Art/MeshySource/Meshy_AI_Neon_Vanguard_Gauntle_0818131608_texture NEW ONE LESS POLYGONS_fbx/Meshy_AI_Neon_Vanguard_Gauntle_0818131608_texture_fbx/Meshy_AI_Neon_Vanguard_Gauntle_0818131608_texture.fbx'))) {
    $errors.Add('Optimized first-person arm candidate source is missing from MeshySource.')
}

foreach ($iconMetaPath in @(
    'Assets/Branding/Android/RydersRoad_Icon_Legacy.png.meta',
    'Assets/Branding/Android/RydersRoad_Icon_AdaptiveForeground.png.meta',
    'Assets/Branding/Android/RydersRoad_Icon_AdaptiveBackground.png.meta'
)) {
    $iconMeta = Read-Text $iconMetaPath
    if ($iconMeta -notmatch '(?m)^  isReadable: 1$' `
        -or $iconMeta -notmatch '(?ms)buildTarget: DefaultTexturePlatform.*?textureCompression: 0' `
        -or $iconMeta -notmatch '(?ms)buildTarget: Android.*?textureCompression: 0') {
        $errors.Add("$iconMetaPath must keep Android launcher icons readable and uncompressed.")
    }
}

$requiredModules = @(
    'Core/Bootstrap',
    'Core/Configuration',
    'Core/Events',
    'Core/Services',
    'Core/Utilities',
    'Input',
    'Player',
    'Camera',
    'Checkpoints',
    'Respawn',
    'Timing',
    'Ranking',
    'Ghosts',
    'Hazards',
    'Blocks',
    'Levels',
    'Worlds',
    'Progression',
    'PowerUps',
    'Economy',
    'SaveSystem',
    'UI',
    'Audio',
    'Visuals',
    'Diagnostics',
    'EditorTools',
    'Tests'
)
$requiredModules | ForEach-Object { Require-Path "Assets/_Game/$_" }

$excludedGeneratedDirs = @('Library', 'Logs', 'Temp', 'Obj', 'Build', 'Builds')
$jsonFiles = Get-ChildItem -Path $projectRoot -Recurse -File |
    Where-Object {
        $relativePath = $_.FullName.Substring($projectRoot.Length).TrimStart('\', '/')
        $pathParts = $relativePath.Split(
            [System.IO.Path]::DirectorySeparatorChar,
            [System.StringSplitOptions]::RemoveEmptyEntries)
        $_.Extension -in @('.json', '.asmdef') -and
        -not ($pathParts | Where-Object { $excludedGeneratedDirs -contains $_ })
    }
foreach ($jsonFile in $jsonFiles) {
    try {
        Get-Content -Raw -LiteralPath $jsonFile.FullName |
            ConvertFrom-Json |
            Out-Null
    }
    catch {
        $errors.Add("Invalid JSON: $($jsonFile.FullName)")
    }
}

$assemblies = @{}
$assemblyFiles = Get-ChildItem -Path (Join-Path $projectRoot 'Assets/_Game') `
    -Recurse -Filter '*.asmdef'
foreach ($assemblyFile in $assemblyFiles) {
    $definition = Get-Content -Raw -LiteralPath $assemblyFile.FullName |
        ConvertFrom-Json
    if ($assemblies.ContainsKey($definition.name)) {
        $errors.Add("Duplicate assembly name: $($definition.name)")
    }
    else {
        $assemblies[$definition.name] = $definition
    }
}

$knownPackageAssemblies = @(
    'Unity.InputSystem',
    'Unity.RenderPipelines.Core.Runtime',
    'Unity.RenderPipelines.Universal.Runtime',
    'UnityEngine.UI'
)
foreach ($assemblyName in $assemblies.Keys) {
    foreach ($reference in $assemblies[$assemblyName].references) {
        if (-not $assemblies.ContainsKey($reference) `
            -and $knownPackageAssemblies -notcontains $reference) {
            $errors.Add("Unknown assembly reference: $assemblyName -> $reference")
        }
    }
}

$buildSettings = Read-Text 'ProjectSettings/EditorBuildSettings.asset'
$sceneOrder = @(
    'Assets/_Game/Levels/Scenes/Bootstrap.unity',
    'Assets/_Game/Levels/Scenes/ModuleSelector.unity',
    'Assets/_Game/Levels/Scenes/MovementLab.unity',
    'Assets/_Game/Levels/Scenes/ModuleRunner.unity',
    'Assets/_Game/Levels/Scenes/FoundationTest.unity'
)
$lastSceneIndex = -1
foreach ($scenePath in $sceneOrder) {
    $sceneIndex = $buildSettings.IndexOf($scenePath, [System.StringComparison]::Ordinal)
    if ($sceneIndex -lt 0) {
        $errors.Add("Build Settings missing scene: $scenePath")
    }
    elseif ($sceneIndex -lt $lastSceneIndex) {
        $errors.Add('Phase 2 build-scene order is invalid.')
    }

    $lastSceneIndex = $sceneIndex
}
if ($buildSettings -match 'Assets/_Game/Levels/Scenes/ArtKitGallery\.unity') {
    $errors.Add('ArtKitGallery must remain editor-only and out of Android Build Settings.')
}

$configuration = Read-Text 'Assets/_Game/Core/Configuration/Resources/FoundationGameConfiguration.asset'
$flagIds = [regex]::Matches($configuration, '(?m)^  - _id: (.+)$') |
    ForEach-Object { $_.Groups[1].Value.Trim() }
foreach ($flagId in $flagIds) {
    if ($flagId -notmatch '^[a-z][a-z0-9]*(?:[.-][a-z0-9]+)*$') {
        $errors.Add("Invalid feature flag ID: $flagId")
    }
}

if ($configuration -notmatch "(?m)^  _gameTitle: RYDER'S ROAD$") {
    $errors.Add("Game title is not exactly RYDER'S ROAD.")
}

if ($configuration -notmatch '(?m)^  _gameVersion: 0\.5\.0$') {
    $errors.Add('Game version is not 0.5.0.')
}

if ($configuration -notmatch '(?m)^  _buildVersion: 0\.16\.0-flow-benchmark-fatal-scenery$') {
    $errors.Add('Current production build version is invalid.')
}

if ($configuration -notmatch '(?m)^  _initialScene: ModuleSelector$') {
    $errors.Add('Phase 2 initial scene must be ModuleSelector.')
}

$touchLayout = Read-Text 'Assets/_Game/Input/Configuration/Resources/Touch_Default.asset'
if ($touchLayout -notmatch '(?m)^  _defaultControlProfile: 4$') {
    $errors.Add('Touch default must open Classic left-move/right-look Tap Jump.')
}

if ($touchLayout -notmatch '(?m)^  _defaultJumpMode: 1$') {
    $errors.Add('Touch default must use right-side Tap Jump.')
}

if ($touchLayout -notmatch '(?m)^  _defaultMovementSensitivity: 2$') {
    $errors.Add('Touch default movement sensitivity must be Fast for the responsive feel pass.')
}

if ($touchLayout -notmatch '(?m)^  _floatingJoystick: 1$') {
    $errors.Add('Touch default must use floating movement origin.')
}

if ($touchLayout -notmatch '(?m)^  _joystickDeadZone: 0\.08$' `
    -or $touchLayout -notmatch '(?m)^  _dragActivationDistance: 0\.014$') {
    $errors.Add('Touch default must keep the responsive dead zone and camera drag threshold.')
}

if ($touchLayout -notmatch '(?m)^  _horizontalEdgeComfortMargin: 96$') {
    $errors.Add('Touch default horizontal comfort margin is not configured.')
}

$movementProfileCount = (
    Get-ChildItem -Path (
        Join-Path $projectRoot `
            'Assets/_Game/Player/Configuration/Resources/MovementProfiles') `
        -Filter '*.asset' -ErrorAction SilentlyContinue
).Count
if ($movementProfileCount -lt 4) {
    $errors.Add('Four Phase 1 movement profiles are required.')
}

$moduleAssetFiles = Get-ChildItem -Path (
        Join-Path $projectRoot 'Assets/_Game/Levels/Resources/Modules') `
    -Filter '*.asset' -ErrorAction SilentlyContinue
$moduleIds = @()
foreach ($moduleAssetFile in $moduleAssetFiles) {
    $moduleText = Get-Content -Raw -LiteralPath $moduleAssetFile.FullName
    $match = [regex]::Match($moduleText, '(?m)^  _stableModuleId: (.+)$')
    if ($match.Success) {
        $moduleIds += $match.Groups[1].Value.Trim()
    }
}

$requiredModuleIds = @(
    'module.001.first-steps',
    'module.002.moving-parts',
    'module.003.flow-error',
    'module.004.the-spiral'
)
foreach ($moduleId in $requiredModuleIds) {
    if ($moduleIds -notcontains $moduleId) {
        $errors.Add("Required module ID is missing: $moduleId")
    }
}

$duplicateModuleIds = $moduleIds |
    Group-Object |
    Where-Object { $_.Count -gt 1 } |
    ForEach-Object { $_.Name }
foreach ($duplicateModuleId in $duplicateModuleIds) {
    $errors.Add("Duplicate module ID: $duplicateModuleId")
}

foreach ($moduleId in $moduleIds) {
    if ($moduleId -notmatch '^[a-z][a-z0-9]*(?:[.-][a-z0-9]+)*$') {
        $errors.Add("Invalid module ID: $moduleId")
    }
}

foreach ($moduleAssetFile in $moduleAssetFiles) {
    $moduleText = Get-Content -Raw -LiteralPath $moduleAssetFile.FullName
    if ($moduleText -notmatch '(?m)^  _rankThresholds:') {
        $errors.Add("Module asset lacks rank thresholds: $($moduleAssetFile.Name)")
    }
    if ($moduleText -notmatch '(?m)^    _calibrationState: 0') {
        $errors.Add("Module thresholds must start uncalibrated: $($moduleAssetFile.Name)")
    }
}

$visualProfile = Read-Text 'Assets/_Game/Visuals/Resources/ModuleVisualProfile.asset'
foreach ($requiredVisualField in @(
    '_surfaceHighlight',
    '_surfaceInset',
    '_circuitLine',
    '_warmAccent',
    '_surf',
    '_surfaceInsetScale',
    '_edgeTrimScale',
    '_heroAccentDensity'
)) {
    if ($visualProfile -notmatch "(?m)^  ${requiredVisualField}:") {
        $errors.Add("Module visual profile lacks $requiredVisualField.")
    }
}

$environmentProfile = Read-Text 'Assets/_Game/Visuals/Resources/ModuleEnvironmentProfile.asset'
if ($environmentProfile -notmatch '(?m)^  _qualityProfile: 1$' `
    -or $environmentProfile -notmatch '(?m)^  _distantIslandDensity:' `
    -or $environmentProfile -notmatch '(?m)^  _horizonLayerOpacity:') {
    $errors.Add('Module environment profile lacks required MobileBalanced visual identity fields.')
}

$visualBenchmarks = Read-Text 'VISUAL_BENCHMARKS.md'
foreach ($benchmark in @(
    'Benchmark_TitleScreen',
    'Benchmark_ModuleSelector',
    'Benchmark_Module01_Start',
    'Benchmark_Module02_Mid',
    'Benchmark_Module03_Opening',
    'Benchmark_Module03_Early',
    'Benchmark_Module03_Middle',
    'Benchmark_Module03_High',
    'Benchmark_Module03_Patch',
    'Benchmark_Spiral_Start',
    'Benchmark_Spiral_Mid',
    'Benchmark_Spiral_Water',
    'Benchmark_Spiral_Boost',
    'Benchmark_Spiral_Summit'
)) {
    if ($visualBenchmarks -notmatch [regex]::Escape($benchmark)) {
        $errors.Add("VISUAL_BENCHMARKS.md lacks $benchmark.")
    }
}

if ($visualBenchmarks -notmatch 'MobileBalanced' -or $visualBenchmarks -notmatch 'F6') {
    $errors.Add('VISUAL_BENCHMARKS.md must document MobileBalanced and the F6 workflow.')
}

$projectSettings = Read-Text 'ProjectSettings/ProjectSettings.asset'
if ($projectSettings -notmatch "(?m)^  productName: Ryder's Road$" `
    -or $projectSettings -notmatch '(?m)^  bundleVersion: 0\.16\.0-flow-benchmark-fatal-scenery$' `
    -or $projectSettings -notmatch '(?m)^  defaultScreenOrientation: 4$' `
    -or $projectSettings -notmatch '(?m)^  allowedAutorotateToPortrait: 0$' `
    -or $projectSettings -notmatch '(?m)^  allowedAutorotateToPortraitUpsideDown: 0$' `
    -or $projectSettings -notmatch '(?m)^  allowedAutorotateToLandscapeRight: 1$' `
    -or $projectSettings -notmatch '(?m)^  allowedAutorotateToLandscapeLeft: 1$' `
    -or $projectSettings -notmatch '(?m)^  useOSAutorotation: 1$' `
    -or $projectSettings -notmatch '(?m)^  androidStartInFullscreen: 1$' `
    -or $projectSettings -notmatch '(?m)^  androidRequestedVisibleInsets: 0$' `
    -or $projectSettings -notmatch '(?m)^  androidRenderOutsideSafeArea: 1$' `
    -or $projectSettings -notmatch '(?m)^  androidSystemBarsBehavior: 2$' `
    -or $projectSettings -notmatch '(?m)^  androidFullscreenMode: 1$' `
    -or $projectSettings -notmatch '(?m)^  fullscreenMode: 1$' `
    -or $projectSettings -notmatch '(?m)^  AndroidMinSdkVersion: 26$' `
    -or $projectSettings -notmatch '(?m)^  AndroidTargetArchitectures: 2$' `
    -or $projectSettings -notmatch '(?ms)^  scriptingBackend:\s+Android: 1$') {
    $errors.Add('Android identity, fullscreen, minimum SDK, ARM64, or IL2CPP configuration is invalid.')
}

if ($projectSettings -notmatch '(?m)^    Android: com\.rydersblockstudio\.rydersblock$') {
    $errors.Add('Android package identifier must remain com.rydersblockstudio.rydersblock.')
}

$tagManager = Read-Text 'ProjectSettings/TagManager.asset'
if ($tagManager -notmatch '(?m)^  - FirstPersonArms$') {
    $errors.Add('FirstPersonArms layer is missing.')
}

$brandPresentation = Read-Text 'Assets/_Game/UI/BrandPresentation.cs'
if ($brandPresentation -notmatch 'PlayerFacingTitle = "RYDER''S ROAD"' `
    -or $brandPresentation -notmatch 'ProductName = "Ryder''s Road"' `
    -or $brandPresentation -notmatch 'LegacyCodename = "RYDERS BLOCK"' `
    -or $brandPresentation -notmatch 'LogoResourcePath = "Branding/RydersRoad_Logo_UI"') {
    $errors.Add('BrandPresentation constants are not configured for Ryder''s Road.')
}

$selector = Read-Text 'Assets/_Game/UI/DevelopmentModuleSelector.cs'
if ($selector -notmatch 'BrandPresentation\.LoadLogoSprite' `
    -or $selector -notmatch 'MusicVolumeKey' `
    -or $selector -notmatch 'FirstPersonCameraRig\.FieldOfViewPreferenceKey') {
    $errors.Add('Module selector must use the approved logo, shared music settings and FOV controls.')
}

if ($selector -notmatch 'ProductionButton' `
    -or $selector -notmatch 'SafeAreaFitter' `
    -or $selector -notmatch 'ModuleSelectionState\.SpiralModuleId' `
    -or $selector -notmatch 'ModuleSelectionState\.GetCampaignModuleIds\(\)' `
    -or $selector -notmatch 'button.interactable = unlocked') {
    $errors.Add('Campaign journey must retain safe-area controls, explicit campaign ordering, real locks, and separate Spiral access.')
}


$moduleSelection = Read-Text 'Assets/_Game/Levels/ModuleSelectionState.cs'
if ($moduleSelection -notmatch '"module\.001\.first-steps"' `
    -or $moduleSelection -notmatch '"module\.002\.moving-parts"' `
    -or $moduleSelection -notmatch '"module\.003\.flow-error"' `
    -or $moduleSelection -notmatch 'SpiralModuleId = "module\.004\.the-spiral"' `
    -or $moduleSelection -notmatch 'IsCampaignModule' `
    -or $moduleSelection -notmatch 'IsSpiralModule') {
    $errors.Add('ModuleSelectionState must preserve explicit Campaign level order and separate Spiral mode identity.')
}

$touchCoordinator = Read-Text 'Assets/_Game/UI/Touch/TouchInputCoordinator.cs'
if ($touchCoordinator -notmatch 'return state == TapDragGestureState\.Camera;') {
    $errors.Add('Touch camera drag must feed look input on the activation frame.')
}

$cameraRig = Read-Text 'Assets/_Game/Camera/FirstPersonCameraRig.cs'
$cameraProfile = Read-Text 'Assets/_Game/Camera/Configuration/Resources/Camera_Default.asset'
if ($cameraRig -notmatch 'DefaultPreferredFieldOfView = 94f' `
    -or $cameraRig -notmatch 'MaximumPreferredFieldOfView = 94f' `
    -or $cameraProfile -notmatch '(?m)^  _baseFieldOfView: 94$' `
    -or $cameraProfile -notmatch '(?m)^  _maximumFieldOfView: 100$') {
    $errors.Add('Default first-person FOV must stay at the 94-degree mobile parkour target.')
}

$hands = Read-Text 'Assets/_Game/Visuals/FirstPersonHands.cs'
if ($hands -notmatch '_leftRenderedPosition' `
    -or $hands -notmatch 'Quaternion\.Slerp' `
    -or $hands -notmatch 'new Vector3\(-0\.74f, -0\.68f, 0\.78f\)' `
    -or $hands -notmatch 'hand\.localScale = Vector3\.one \* 0\.68f' `
    -or $hands -notmatch 'FirstPersonArmProfile' `
    -or $hands -notmatch 'CreateArmFromProfile' `
    -or $hands -notmatch 'ArmCameraName' `
    -or $hands -notmatch 'ArmRenderingLayerName' `
    -or $hands -notmatch 'UniversalAdditionalCameraData' `
    -or $hands -notmatch '_smoothedHorizontalSpeed' `
    -or $hands -notmatch '_groundedBlend' `
    -or $hands -notmatch '_riseBlend' `
    -or $hands -notmatch '_fallBlend' `
    -or $hands -notmatch 'ResolveOpposedRunForwardOffsets') {
    $errors.Add('First-person hands must keep the dedicated arm-camera and bounded additive locomotion presentation pass.')
}

$moduleScene = Read-Text 'Assets/_Game/UI/ModuleSceneController.cs'
if ($moduleScene -notmatch 'CreateProceduralSkybox' `
    -or $moduleScene -notmatch 'Materials/MAT_RR_Skybox_Seamless' `
    -or $moduleScene -notmatch 'ModuleVisualPrefabLibrary' `
    -or $moduleScene -notmatch 'ApplyVisualPrefabOverride' `
    -or $moduleScene -notmatch 'CreateVisualPrefabDecoration' `
    -or $moduleScene -notmatch 'Stone Panel Groove' `
    -or $moduleScene -notmatch 'GameplayVfxService') {
    $errors.Add('ModuleSceneController must keep the production skybox and lightweight route-surface detailing pass.')
}
if ($moduleScene -match 'Gold Route Centerline' -or $moduleScene -match 'PlayerGroundingCue>\(\)\.Initialize') {
    $errors.Add('Ordinary blocks and the player must not regenerate the removed route helper or grounding disc.')
}
if ($moduleScene -match 'blockSize\.[xyz] \* placement\.LocalScale') {
    $errors.Add('Gameplay block prefab visuals must not double-scale by authored block size.')
}

$prefabLibrary = Read-Text 'Assets/_Game/Visuals/ModuleVisualPrefabLibrary.cs'
if ($prefabLibrary -notmatch 'PrepareVisualInstance' `
    -or $prefabLibrary -notmatch 'PlacementFor' `
    -or $prefabLibrary -notmatch 'VisualPrefabPlacement' `
    -or $prefabLibrary -notmatch 'MinimumBlockExtent' `
    -or $prefabLibrary -notmatch 'GetComponentsInChildren<Collider>' `
    -or $prefabLibrary -notmatch 'motionVectorGenerationMode = MotionVectorGenerationMode\.ForceNoMotion') {
    $errors.Add('ModuleVisualPrefabLibrary must keep Meshy visual prefabs collider-free and presentation-only.')
}

$artManifest = Read-Text 'ART_ASSET_MANIFEST.md'
foreach ($manifestToken in @(
    'RR_Block_Standard',
    'RR_Platform_Long',
    'RR_RestorePoint',
    'RR_BoostPad',
    'RR_PatchBlock',
    'RR_EnergyPillar',
    'Final Surf asset pending',
    'RR_FP_Arm_Right',
    'Meshy_AI_Neon_Vanguard_Gauntle_0818131608_texture.fbx',
    '15400',
    'Integrated runtime arms',
    'PF_RR_FP_Arm_Left.prefab',
    'Legacy high-poly arm key'
)) {
    if ($artManifest -notmatch [regex]::Escape($manifestToken)) {
        $errors.Add("ART_ASSET_MANIFEST.md lacks required Meshy note: $manifestToken")
    }
}

$artIntegrator = Read-Text 'Assets/_Game/EditorTools/MeshyArtKitIntegrator.cs'
if ($artIntegrator -notmatch 'TryIntegrateOptimizedArmCandidate' `
    -or $artIntegrator -notmatch 'MirrorMeshX' `
    -or $artIntegrator -notmatch 'LegacyHighPolyArmSourceKey' `
    -or $artIntegrator -notmatch 'MaximumRuntimeArmTriangles' `
    -or $artIntegrator -notmatch 'ConfigureModulePrefabLibrary' `
    -or $artIntegrator -notmatch 'CreateGalleryScene') {
    $errors.Add('MeshyArtKitIntegrator must integrate environment art and generate optimized mirrored first-person arms only from the production candidate.')
}

$immersiveMode = Read-Text 'Assets/_Game/Core/Utilities/AndroidImmersiveMode.cs'
if ($immersiveMode -notmatch 'setDecorFitsSystemWindows' `
    -or $immersiveMode -notmatch 'ApplyLegacyImmersive\(window\)' `
    -or $immersiveMode -notmatch 'setStatusBarColor' `
    -or $immersiveMode -notmatch 'Screen\.fullScreenMode = FullScreenMode\.FullScreenWindow') {
    $errors.Add('Android immersive mode must apply modern and legacy fullscreen paths.')
}

if ($immersiveMode -match 'using var activity = unityPlayer\.GetStatic<AndroidJavaObject>\("currentActivity"\)' `
    -or $immersiveMode -notmatch 'LogImmersiveFailure\(exception\)' `
    -or $immersiveMode -notmatch 'using \(activity\)') {
    $errors.Add('Android immersive UI-thread callback must keep currentActivity alive until the callback runs.')
}

$bootstrap = Read-Text 'Assets/_Game/Core/Bootstrap/GameBootstrap.cs'
if ($bootstrap -notmatch 'ScheduleImmersiveReapply' `
    -or $bootstrap -notmatch 'SavePathMigrationUtility\.CopyLegacySaveIfCurrentMissing') {
    $errors.Add('Bootstrap must reapply Android immersive mode and preserve legacy save paths.')
}

$qualitySettings = Read-Text 'ProjectSettings/QualitySettings.asset'
if ($qualitySettings -notmatch '(?m)^    Android: 2$') {
    $errors.Add('Android default quality must remain the Medium/MobileBalanced slot for the alpha slice.')
}

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Output "RYDER'S ROAD Meshy Art Kit source validation passed."
Write-Output "Assemblies: $($assemblies.Count)"
Write-Output "Feature flags: $($flagIds.Count)"
Write-Output "Movement profiles: $movementProfileCount"
Write-Output "Module assets: $($moduleIds.Count)"
Write-Output "Run Unity Test Runner and Android build after source changes."
