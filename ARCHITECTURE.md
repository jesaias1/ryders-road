# Architecture

## Intent

Phase 0 establishes replaceable seams without implementing future gameplay.
`GameBootstrap` is a composition root, not a game manager: it constructs
services, initializes settings/save/diagnostics, and delegates scene loading.
It must not accumulate gameplay state or rules.

## Assembly dependencies

Dependencies point downward only:

```text
Game.Core
├── Game.Input -> Game.Core, Unity Input System
├── Game.Gameplay -> Game.Core, Game.Input
├── Game.Save -> Game.Core
├── Game.Diagnostics -> Game.Core, Game.Save
├── Game.UI -> Game.Core, Game.Gameplay, Unity Input System
└── Game.Bootstrap -> Game.Core, Game.Gameplay, Game.Save, Game.Diagnostics

Game.Editor -> runtime assemblies (Editor only)
Game.EditModeTests -> runtime + Editor assemblies (Editor only)
Game.PlayModeTests -> runtime assemblies
```

Runtime modules must never reference `Game.Bootstrap`, `Game.Editor`, or test
assemblies. UI may consume service contracts/data models but must not own
gameplay rules. Save owns serialization and migration, not feature behavior.

## Composition and service access

`GameBootstrap.ComposeServices` creates concrete services behind:

- `IPlayerInputSource`
- `ISaveService`
- `ILevelLoader`
- `ICheckpointService`
- `IRunTimerService`
- `IAudioService`
- `ISettingsService`
- `IDiagnosticsService`

Only services needed in Phase 0 have concrete implementations. `GameServices`
publishes the composed container for scene adapters that Unity instantiates;
ordinary domain code should prefer explicit constructor or initializer
dependencies. Registration is one-time and duplicate registration fails.

## Configuration

Unity-object references and designer tuning belong in ScriptableObjects.
`FoundationGameConfiguration` demonstrates the pattern for application/build
metadata and feature flags. Later phases should add focused assets rather than
turning it into a universal configuration object:

- movement and camera profiles;
- touch-control layouts;
- level definitions and rank thresholds;
- block, audio, visual, power-up, and difficulty profiles.

Runtime scripts may contain true invariants and safe fallback defaults.
Designer-tunable gameplay values must live in configuration assets.

## Stable identifiers

Persistent IDs use lowercase ASCII tokens separated by `.` or `-`, begin with a
letter, and are at most 64 characters. Examples:

- `world.sky-gardens`
- `level.sky-gardens.01`
- `block.jump-boost`
- `powerup.air-dash`
- `cosmetic.gloves-neon`

An ID is immutable after content ships. Renames affect display text, not IDs.
IDs never derive solely from scene names, object names, indexes, GUID display,
or hierarchy position. Retired IDs are not reused. Save migrations provide
explicit aliases when identity must change.

## Lifecycle

`Bootstrap` is build index zero. It persists its single composition object,
loads settings and the versioned save, attaches diagnostics in development,
then loads `FoundationTest` using `ILevelLoader`. App pause/focus loss flushes
settings and save data. Gameplay scenes do not recreate services.

## Scenes

- `Bootstrap`: service composition and loading only.
- `FoundationTest`: minimal runtime-created camera, light, floating platform,
  void marker, safe-area visualization, and reload button.

The test scene controller is temporary scene presentation, not a future level
authoring pattern.
