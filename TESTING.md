# Testing

## Pinned environment

- Unity Editor target: `6000.0.77f1`
- Packages: Input System `1.11.2`, URP `17.0.3`, Test Framework `1.4.5`
- First device platform: Android, IL2CPP, minimum API 26
- Runtime target: landscape left/right, 60 FPS

The Editor and Android modules were unavailable when this foundation was
authored, so the commands below have not yet been executed in this workspace.

## First open

1. Install Unity `6000.0.77f1` with Android Build Support, SDK/NDK, and OpenJDK.
2. Open the repository root.
3. Wait for package import and compilation.
4. Run `Avoidance > Apply Foundation Project Settings`.
5. Save the generated URP assets under `Assets/_Game/Visuals/Settings`.
6. Run `Avoidance > Validate Foundation`.

## Unity UI

Open `Window > General > Test Runner`:

- Run all Edit Mode tests.
- Run all Play Mode tests.
- Open `Assets/_Game/Levels/Scenes/Bootstrap.unity` and enter play mode.
- Confirm FoundationTest appears and F1 toggles diagnostics.
- Resize Game view across the reference profiles and confirm the cyan safe-area
  region plus reload button remain visible.

Reference view profiles:

- 1920 × 1080 (16:9 baseline)
- 2400 × 1080 (20:9 wide phone)
- 2340 × 1080 (19.5:9)
- 1280 × 720 (low-resolution baseline)

## Source-only validation

This does not replace Unity compilation or tests, but checks repository
structure, JSON, assembly references, build-scene order, and flag IDs:

```powershell
& 'C:\Avoidance\Tools\Validate-Foundation.ps1'
```

## Batch commands

PowerShell, with Unity installed at the pinned path:

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.0.77f1\Editor\Unity.exe'
& $unity -batchmode -nographics -quit -projectPath 'C:\Avoidance' -runTests -testPlatform EditMode -testResults 'C:\Avoidance\Logs\editmode-results.xml' -logFile 'C:\Avoidance\Logs\editmode.log'
& $unity -batchmode -nographics -quit -projectPath 'C:\Avoidance' -runTests -testPlatform PlayMode -testResults 'C:\Avoidance\Logs\playmode-results.xml' -logFile 'C:\Avoidance\Logs\playmode.log'
```

## Android build

Switch the active platform to Android, then use
`Avoidance > Build > Android Development APK`.

Batch equivalent:

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.0.77f1\Editor\Unity.exe'
& $unity -batchmode -nographics -quit -projectPath 'C:\Avoidance' -buildTarget Android -executeMethod Avoidance.EditorTools.AndroidDevelopmentBuilder.BuildAndroidDevelopment -logFile 'C:\Avoidance\Logs\android-build.log'
```

Output:
`Builds/Android/Avoidance-0.0.1-foundation-dev.apk`.

Never claim device validation from Editor or batch results. Record phone model,
Android version, safe area, orientation switching, pause/resume save behavior,
touch behavior, thermals, and sustained FPS separately.
