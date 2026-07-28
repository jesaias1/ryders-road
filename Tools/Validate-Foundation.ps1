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

$requiredPaths = @(
    'AGENTS.md',
    'ARCHITECTURE.md',
    'ROADMAP.md',
    'CHANGELOG.md',
    'TESTING.md',
    'KNOWN_ISSUES.md',
    'SAVE_SCHEMA.md',
    'ART_DIRECTION.md',
    'BETA_FEEDBACK.md',
    'Packages/manifest.json',
    'ProjectSettings/ProjectVersion.txt',
    'ProjectSettings/EditorSettings.asset',
    'ProjectSettings/ProjectSettings.asset',
    'ProjectSettings/EditorBuildSettings.asset',
    'Assets/_Game/Levels/Scenes/Bootstrap.unity',
    'Assets/_Game/Levels/Scenes/FoundationTest.unity'
)
$requiredPaths | ForEach-Object { Require-Path $_ }

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

$jsonFiles = Get-ChildItem -Path $projectRoot -Recurse -File |
    Where-Object { $_.Extension -in @('.json', '.asmdef') }
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

$buildSettings = Get-Content -Raw -LiteralPath (
    Join-Path $projectRoot 'ProjectSettings/EditorBuildSettings.asset')
$bootstrapIndex = $buildSettings.IndexOf(
    'Assets/_Game/Levels/Scenes/Bootstrap.unity',
    [System.StringComparison]::Ordinal)
$foundationIndex = $buildSettings.IndexOf(
    'Assets/_Game/Levels/Scenes/FoundationTest.unity',
    [System.StringComparison]::Ordinal)
if ($bootstrapIndex -lt 0 -or $foundationIndex -lt 0 `
    -or $bootstrapIndex -gt $foundationIndex) {
    $errors.Add('Bootstrap and FoundationTest build-scene ordering is invalid.')
}

$configuration = Get-Content -Raw -LiteralPath (
    Join-Path $projectRoot `
        'Assets/_Game/Core/Configuration/Resources/FoundationGameConfiguration.asset')
$flagIds = [regex]::Matches($configuration, '(?m)^  - _id: (.+)$') |
    ForEach-Object { $_.Groups[1].Value.Trim() }
foreach ($flagId in $flagIds) {
    if ($flagId -notmatch '^[a-z][a-z0-9]*(?:[.-][a-z0-9]+)*$') {
        $errors.Add("Invalid feature flag ID: $flagId")
    }
}

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Output "Foundation source validation passed."
Write-Output "Assemblies: $($assemblies.Count)"
Write-Output "Feature flags: $($flagIds.Count)"
Write-Output "Unity execution is still required for compilation, tests, and Android build."
