## Movement Mastery candidate (0.9.6)

ADR 0018 isolates profile-gated motor behavior and a removable FlowLabSession.
Campaign keeps compatibility 1; training compatibility 2 uses the existing motor
and MovementLab composition. See Docs/MOVEMENT_MASTERY_SLICE.md.

<!-- Current follow-up: 0.9.5-startup-course-clearance -->

0.9.5 addresses physical startup/UI and course intersections. See
Docs/S23_095_FIRST_THREE_FEEDBACK.md and ADR 0017. Module 002 v3 / 003 v7 retain
stable IDs and the unchanged motor; whole-path clearance supplements top-ray and
endpoint traversal tests. Native startup uses the approved loading poster.

# Architecture

## 0.9.4 Mountain World authority

Module 002 v2 uses baked mountain geology and explicit near-surface collision. Campaign eligibility derives valid completion evidence from V4 records, and the normal journey has no QA bypass. See Docs/Decisions/0016-mountain-world-sequential-campaign.md.

## 0.9.3 authored Sky City

Module 001 v2 uses baked editor-authored architecture, existing biome/collision contracts, and an isolated environment profile. Startup factories preserve its authored route and city. Optional sky/fog overrides leave other modules at existing defaults. See Docs/Decisions/0015-authored-sky-city-production.md.

## 0.9.2 frontend hotfix authority

SceneTransitionHost persistently owns async loading and readiness; ILevelLoader
callers only observe requests and may be destroyed safely. Presentation events
are isolated from scene control. Module/UI readiness and bounded failures replace
scene-owned coroutine completion. See Docs/Decisions/0014-persistent-scene-transitions.md.

## 0.9.1 current authority

Explicit playable near architecture supersedes blanket visual-only biome rules. AuthoredSurface retains the source mesh across render batching; the loader exposes presentation events, and FlowChallenge is attempt-scoped. See Docs/Decisions/0013-explicit-playable-architecture.md and Docs/S23_091_CONTINUATION.md. Module 003 content version 6; V4 save meaning, movement and manual camera are preserved.


## Current authority — 0.9.0 production milestone

Historical phase descriptions below are superseded by this section and
`Docs/AI_HANDOFF.md` where they conflict. The movement solver and V4 saves are
unchanged. Manual touch pitch remains available, arms remain hidden, and
neither a grounding disc nor a radial world floor is instantiated.

`ModuleSelectionState.GetNextCampaignModuleId` owns Campaign successor lookup;
Resources discovery is not progression order. `ModuleRunResult.PersonalBestSeconds`
is transient presentation data read from the existing save record, not a schema
change. `GameplayAudioProfile` owns clip/volume selection; `MovementFeedback`
maintains its public calls and publishes optional presentation events without
fabricating missing audio. Moving endpoint pauses are part of deterministic
path evaluation. Android permits both landscape directions and no portrait.


## Intent

`0.7.9-module003-first-finished-level` replaces Module 003's trigger-only
opening water span with an entirely collider-truthful Bronze route and removes
duplicate normal supports beneath Boost, Restore, Crumble, and Patch mechanics.
The module is content version 2 with five authored acts, three fair Restores,
three optional mastery cuts, and uncalibrated rank times. A reusable pooled
`GameplayVfxService` now separates event feedback from block construction;
ordinary blocks remain passive. The rejected grounding cue is not instantiated.
Movement, manual camera, controls, hidden-arm architecture, saves, progression,
and stable module identity are unchanged.

Phase 0 established replaceable boundaries. Phase 1 added the focused
legacy `RYDERS BLOCK` movement laboratory. Phase 2 added the first data-driven parkour
modules. Phase 3 adds the first game loop: timer, ranks, score, personal bests,
splits, results, and derived campaign progression. Phase 3B adds the first
true visual vertical slice while keeping gameplay rules unchanged.
`0.4.0-visual-identity` strengthens the reusable visual profile and hero-map
presentation layer while keeping gameplay rules unchanged. `0.4.6b` added
Easy Mode with left-thumb movement/steering, a broad right-side jump zone, and
Smart Parkour Camera. After Samsung Galaxy S23 feel feedback, a 0.4.7 sweep
restores Classic left movement, right manual camera, and right-side Tap Jump
as the phone default while keeping Easy Mode selectable from the run menu.
Normal jumps are tuned for shorter, snappier, more predictable platform
landings while earned momentum, bhop, air-strafe, boost, water, and surf remain
bounded optional skill layers. Android immersive
fullscreen now lives behind a small platform display service and is reapplied
on startup, resume, and focus return. `0.4.7-brand-alpha-presentation` changes
the player-facing title to `RYDER'S ROAD`, imports the approved logo/icon
assets, prepares Android launcher icons, adds a branded title/module/settings
selector, strengthens Android fullscreen reapplication, and adds a product-name
save-path bridge. It intentionally keeps package ID
`com.rydersblockstudio.rydersblock`, stable module IDs, scene
names, and namespaces unchanged. The same 0.4.7 sweep tightens touch response
and calms first-person hand presentation without changing module rules, ranks,
score, or Smart Camera boundaries. The 2026-08-16 progression foundation moves
saves to schema V4 with empty economy/inventory containers and small stable-ID
mutation helpers. Bronze completion rewards can grant one-time currency,
package, and skin-unlock records, while loot opening, shops, and cosmetic UI
remain unstarted.
`0.6.1-parkour-feel-and-route-rhythm` retimes standard production Crumble to
one-second route pressure, updates Spiral rhythm/depth composition, and refines
first-person hand presentation. It does not change the movement solver, 94 FOV,
camera/input contracts, Restore rules, save schema, ranks, progression, or
stable content identity.
`0.6.2-s23-feedback-polish` is a narrow S23 feedback pass over that state:
larger/inward presentation-only arms, separated Spiral core visual masses, and
low red-fog void dressing. It does not alter gameplay contracts, collision,
movement, camera logic, route IDs, saves, or progression.
`0.6.3-hand-pose-collision-truth-abyss-depth` corrects the hand pose after
0.6.2 over-rotated the fingers inward, makes broad visual-only water/decorative
surfaces read as non-landable, and deepens Spiral's abyss fog/red glow. It does
not change the movement solver, camera rules, Restore/Patch behavior, save
schema, progression, rank contracts, route IDs, or authoritative colliders.
`0.6.4-natural-hand-pose-and-world-composition-pass` narrows that into
presentation and content polish: a lower/closer runner-ready hand profile,
segmented non-landable water flow visuals, more intentional campaign depth
clusters, and deeper Spiral haze/silhouettes. It keeps movement, camera,
Restore/Patch, save schema, progression, route supports, ranks, and gameplay
colliders unchanged.
`0.6.5-surgical-recovery-and-correct-hand-pose` is a recovery pass over that
state: it restores a non-crossing camera-space hand profile, removes generated
abyss slab/veil geometry, and trims visual-only filler clutter without changing
movement, route spacing, collision truth, crumble, Restore, ranks, saves,
progression, stable IDs, or input/camera contracts.
`0.6.6-first-person-hand-pose-lock` changes only the camera-space arm profile
and its presentation adapter. A profile flag locks the approved static neutral
pose and bypasses procedural hand offsets; movement, camera/FOV, world content,
collision, route, progression, UI, and save contracts remain unchanged.
`0.6.7-safe-rig-integration-and-locomotion` adds an editor-imported rigged
first-person arm presentation path and visual-only locomotion while preserving
the 0.6.6 unrigged arms as the approved fallback. Rigged arm animation is
camera-space presentation only and must not affect the movement solver,
Classic manual camera, Editor controls, physics, route identity, saves,
progression, or Android landscape behavior.
`0.6.8-rigged-hand-pose-recovery` is a surgical correction of that rigged
presentation path. It keeps the rigged GLB import and legacy fallback, restores
a static neutral approval pose, bakes relaxed finger/thumb curl and wrist
settle into the prefab, adds a visual-only forearm entry sleeve, and disables
runtime locomotion/finger animation pending human pose approval. Movement,
camera/FOV/yaw, controls, world content, progression, saves, and Android
orientation contracts are unchanged.
`0.6.8b-hand-rig-handedness-and-material-recovery` keeps that static approval
boundary and corrects only handedness/material truth. The right rigged prefab
uses the source mesh/rig; the left is a baked X-mirrored mesh and mirrored
bone hierarchy, with positive runtime scales. The importer now generates
tangents, imports textures by glTF role, avoids treating packed
metallic/roughness as Unity gloss, and uses a simpler lower-curl static pose.
Movement, camera/FOV/yaw, controls, locomotion animation state, world content,
progression, saves, collision, and Android orientation contracts are unchanged.
`0.6.9-ryder-arm-v2-full-replacement` adds a separate Ryder Arm V2 import and
presentation path. The camera owns a `FirstPersonVisualRoot` with named left
and right anchors; each anchor owns one visual-only V2 prefab. The old Meshy
rigged and approved unrigged prefabs remain recoverable references but are not
active. Static pose is baked into the V2 skeleton while runtime arm locomotion
remains disabled. Movement, camera, controls, collision, world, UI,
progression, save, stable-ID, and Android orientation contracts are unchanged.
`0.7.0-first-person-arm-camera-and-animation-lock` keeps those contracts while
separating first-person arms into their own `FirstPersonArms` layer and URP
overlay camera. The gameplay camera remains the world camera at the existing
94-degree FOV and excludes arm renderers; the child arm camera renders only the
arm layer at profile FOV 70 and near clip 0.025. `FirstPersonHands` owns
visual-only idle, alternating run, jump/rise/fall, landing, wrist, and finger
additive presentation. These states read movement state but do not alter
movement velocity, camera aim, input, collision, route identity, saves,
progression, world content, UI, or Android orientation behavior.
`0.7.1-first-person-arm-framing-and-animation-lock` is a final focused tuning
pass on that architecture. It keeps the dedicated arm camera/layer and 94-degree
world FOV, sets arm FOV to 78, moves the V2 anchors outward/lower/back, softens
wrist pronation, and clamps additive animation more tightly around landing
visibility. It does not change movement, world camera authority, input,
collision, route identity, world content, UI, progression, saves, or Android
orientation behavior.
`0.7.2-first-person-arm-reference-lock` is a surgical reference-matching pass
against the user-supplied first-person arm gameplay screenshot. It keeps the
same dedicated arm camera/layer, unchanged 94-degree world FOV, and Ryder Arm
V2 asset identity while lowering arm camera FOV to 68, placing the arms farther
out/down/back in the lower corners, reducing wrist pronation, and calming
additive locomotion so the lower-center landing lane stays open. It does not
change movement, world camera authority, input, collision, route identity,
world content, UI, progression, saves, or Android orientation behavior.
`0.7.3-surgical-arm-presentation-recovery` treats that 0.7.2 pose as a visual
regression and recovers toward the 0.7.1 pre-regression baseline. It raises and
slightly enlarges the hands, uses a moderate 72-degree arm overlay FOV, restores
68-degree wrist pronation, and keeps the low-amplitude visual-only animation
guardrails. Movement, world camera authority, input, collision, route identity,
world content, UI, progression, saves, and Android orientation behavior remain
unchanged.
`0.7.3-campaign-recovery-biomes-and-world-assets` shifts production attention
back to the normal Campaign. It adds `EnvironmentBiomeProfile` as data for
visual-only lower-world identity, assigns Sky City/Mountain Sky/Ancient Abyss
to Campaign Modules 001/002/003, imports curated CC0 Kenney background prefabs,
and renders those prefabs plus distant mist ocean layers through
`ModuleSceneController`. Gameplay colliders, movement, camera, Restore, Patch,
ranks, PBs, save schema, and first-person arms remain unchanged.
`0.7.5-module003-gold-standard-vertical-slice` keeps that campaign foundation
and adds reusable depth-band metadata, per-object atmospheric fade, slow
mist-layer drift, hidden-by-default developer telemetry, and an arm visibility
flag that temporarily disables first-person arm rendering while preserving all
Ryder Arm V2 assets and profile data. Only Module 003 receives gold-slice
environment content: far/lower Ancient Abyss landmarks, a continuous mist
ocean, restrained near-route supports, Module 003-only lighting/post
presentation, and lightweight mechanic feedback VFX. Movement, 94 FOV world
camera authority, input, colliders, Restore/Patch, ranks, progression, save
schema, stable IDs, and Android orientation contracts are unchanged.

`GameBootstrap` is a composition root, not a game manager: it constructs
services, initializes settings/save/diagnostics, and delegates scene loading.
It must not accumulate gameplay state, module state, or level rules.

## Assembly dependencies

Dependencies point downward only:

```text
Game.Core
|-- Game.Input -> Game.Core, Unity Input System
|-- Game.Gameplay -> Game.Core, Game.Input
|-- Game.Save -> Game.Core
|-- Game.Diagnostics -> Game.Core, Game.Save
|-- Game.UI -> Game.Core, Game.Gameplay, Game.Input, Game.Save, Unity Input System
`-- Game.Bootstrap -> Game.Core, Game.Gameplay, Game.Save, Game.Diagnostics

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
- `IPlatformDisplayService`

Only services needed through Phase 1 have concrete implementations. `GameServices`
publishes the composed container for scene adapters that Unity instantiates;
ordinary domain code should prefer explicit constructor or initializer
dependencies. Registration is one-time and duplicate registration fails.

## Configuration

Unity-object references and designer tuning belong in ScriptableObjects.
`FoundationGameConfiguration` demonstrates the pattern for application/build
metadata and feature flags. Later phases should add focused assets rather than
turning it into a universal configuration object:

- movement and camera profiles;
- smart parkour camera profiles and route-camera graph data;
- touch-control layouts;
- level definitions and rank thresholds;
- block, audio, visual, power-up, and difficulty profiles.

Runtime scripts may contain true invariants and safe fallback defaults.
Designer-tunable gameplay values must live in configuration assets.

## Smart Parkour Camera

`ParkourCameraProfile` owns Easy Mode camera tunables: camera cone, velocity
look-ahead filtering, route guidance weight, branch intent thresholds,
hysteresis, pitch framing, and speed limits. `SmartParkourCameraController`
turns player position, body yaw, rendered camera yaw, motor velocity, grounded
state, surf state, vertical speed, and current touch input into camera yaw/pitch
presentation only. It does not call `ParkourMotor`, mutate velocity, steer the
player transform, jump, choose a route, clamp falls, or target platform centers.

`RouteCameraGraph`, `RouteCameraBranch`, and `RouteCameraNode` describe broad
course flow for the camera. Nodes identify route corridors, branches, merges,
shortcuts, and vertical framing hints. They are not gameplay identity, not
completion criteria, and not a replacement for `ModuleDefinition` stable IDs.
The current `THE SPIRAL` graph is generated by `ModuleSceneController` as a
prototype authoring bridge; future production work should move route-camera
data into focused assets without changing movement physics.

`FirstPersonCameraRig` renders Smart Camera yaw as a local camera offset during
Easy Mode so the camera can look ahead without continuously rotating the
player body used by flow-steering movement. When switching back to Classic,
the rendered yaw is adopted once into the body yaw so manual look resumes
cleanly.

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

## Phase 1 movement flow

```text
Editor keyboard/mouse ----\
                           PlayerInputRouter -> PlayerRuntimeCoordinator
Touch joystick/look/jump -/                         |
                                                    +-> ParkourMotor
                                                    +-> FirstPersonCameraRig
                                                    +-> RestoreController
                                                    +-> Diagnostics values
                                                    +-> Session reporter

RestorePoint -> CheckpointService -> RestoreController
MovingPlatformMotion ----------------> ParkourMotor
```

`IPlayerInputSource` remains the only player-control boundary. The router
selects Editor or touch input while UI touch components own independent pointer
IDs. `ParkourMotor` owns locomotion state only. Camera effects, placeholder
hands, feedback, restore flow, diagnostics publication, and reporting are
separate components.

Movement and camera values live in `MovementProfile`, `CameraProfile`, and
`TouchControlLayout` assets. Movement profiles carry a compatibility version
for future trace systems but no ghost functionality exists in Phase 1.
Phase 1B keeps phone rescue tuning in those same assets and adds
`MovementLabVisualProfile` for temporary visual roles/materials.

The current default touch profile is `LeftMoveRightLookTapJump`: left thumb
owns movement, right-thumb drag owns manual camera/look, and a quick right-side
tap emits JumpIntent without interrupting left-thumb movement. Flow steering,
right jump zones, fixed buttons, and Flick Jump remain isolated to development
or legacy profiles. `ParkourMotor` owns Grounded/Airborne/Surfing movement
state, jump height/time-to-apex/fall-gravity derivation, takeoff momentum
retention, bhop retention, air-strafe limits, authored `SurfSurface`
interaction, and hard velocity safety caps. Camera presentation owns manual
look response, micro-jitter filtering, dynamic FOV, and optional landing
awareness; it does not own movement, jump, or route rules. Manual right-thumb
look always overrides automatic pitch helpers.

`CheckpointService` stores stable restore snapshots. `RestoreController`
handles Null Space/manual restore, input/motion/camera reset, moving-platform
reset, and a brief protection window without reloading the scene.

## Phase 2 module flow

```text
Bootstrap -> ModuleSelector -> ModuleSelectionState -> ModuleRunner
                                                   |
                                                   +-> ModuleDefinition
                                                   +-> ModuleSceneController
                                                   +-> ModuleAttemptTelemetry

ModuleDefinition -> blocks, Restore Points, Patch Block, shortcuts,
                    moving blocks, boost blocks, water volumes,
                    crumbling blocks, decorations, camera hints,
                    visual profile, environment profile
```

`ModuleDefinition` assets are the source of truth for the first real parkour
modules. Each module has a stable module ID, project/world placeholders,
content version, display/internal names, timing targets, difficulty,
mechanics-used metadata, Restore Points, a Patch Block, optional shortcuts,
environment biome profile, and data-authored route pieces. IDs follow the
stable identifier rules above.

`ModuleSceneController` is a temporary development scene adapter. It builds the
selected module from data, wires the existing player, camera, checkpoint,
restore, feedback, and touch systems, and writes local module-attempt reports.
It does not own campaign progression or global game state.

Reusable Phase 2 mechanics live as independent gameplay components:

- `MovingBlock`: translating platform with editable path points, speed, pause,
  loop mode, easing, and reset.
- `JumpBoostBlock`: applies an external launch velocity without changing the
  movement profile.
- `WaterFlowVolume`: applies capped directional flow while the player remains
  inside a trigger volume.
- `CrumblingBlock`: warns, hides its route block, and resets through restore or
  optional timing.
- `PatchBlock`: completes a module attempt once and records local development
  telemetry.

`CameraGuideHint` and `ShortcutTrigger` are metadata/telemetry helpers. Camera
hints must not override movement, camera, checkpoint, restore, or completion
rules.

For touch development after `0.3.9-movement-v1`, the default module UI uses
left-thumb movement, right-thumb camera, and right-side Tap Jump. The left
movement control has broad acquisition but clamps its floating origin inside an
ergonomic safe area so the full thumb-travel circle stays away from screen
edges and cutouts. A short right-side tap creates `JumpIntent`; a right-side
drag becomes camera input and never jumps on release. Jump gestures do not
implement independent jump physics. Editor keyboard/mouse controls remain
available.

## Phase 3 ranking/progression flow

```text
ModuleRunner -> ModuleRunSession -> RunTimerService
                         |
                         +-> ModuleRankThresholds
                         +-> ModuleRankUtility
                         +-> ModuleScoreService
                         +-> ModuleHud results
                         +-> ModuleProgressRecord (save V4 progression)

RestorePoint -> split record -> current run and PB delta
PatchBlock -> freeze timer -> rank/score/PB/progression -> results
```

`ModuleRankThresholds` live on `ModuleDefinition` assets. Silver, Gold, and
Diamond are configurable. Bronze has no time threshold: any valid completion
of a main module earns at least Bronze and advances normal campaign
progression.

`RunTimerService` owns elapsed/final time. Ordinary Restore does not reset the
timer. A full module restart reloads the module and starts a fresh run.

`ModuleRunSession` owns current-run splits and final run result data. It is
scalable to future long maps with many Restore Points.

`ModuleScoreService` is deterministic and monotonic with completion time for
comparable runs. Raw time remains authoritative; score is a replaceable
presentation layer.

Save schema V4 stores module progress by stable module ID: completion state,
raw PB time, rank, score, split records, attempt/completion counts, latest
completion, content version, movement compatibility version, rank threshold
version, calibration state, and run validity. Normal campaign unlocks are
derived from previous module completion; exceptional unlocks exist for future
special cases. V4 also adds stable-ID economy balances, stackable inventory
items, unlock records, and equipped cosmetic slot records. Current runtime
helpers can add/spend currency, grant items, set unlocks, and equip unlocked
cosmetics only. Module definitions author one-time Bronze reward records that
the module scene adapter applies after valid completion. No loot opening flow,
shop, or cosmetic presentation exists yet.

Run validity distinguishes normal `ValidUnassisted` runs from future assisted
runs and invalid development runs. Development override can launch locked
modules in debug/editor contexts, but those runs must not update official PBs.

## Lifecycle

`Bootstrap` is build index zero. It persists its single composition object,
loads settings and the versioned save, attaches diagnostics in development,
then loads the configured initial scene using `ILevelLoader`. Phase 2 sets the
initial scene to `ModuleSelector`. App pause/focus loss flushes settings and
save data. Gameplay scenes do not recreate services.

## Scenes

- `Bootstrap`: service composition and loading only.
- `ModuleSelector`: development-only module selection, MovementLab access, and
  local test-record clearing.
- `ModuleRunner`: runtime-authored selected module scene driven by
  `ModuleDefinition` data.
- `FoundationTest`: minimal runtime-created camera, light, floating platform,
  void marker, safe-area visualization, and reload button.
- `MovementLab`: runtime-authored temporary blocks covering running, seams,
  standard/precision/turning/ascending/descending jumps, head clearance,
  moving-platform inheritance, maximum-distance measurement, air control,
  Restore Point, Null Space, momentum/bhop/surf, jump calibration lane,
  manual-camera spiral checks, and a temporary Patch Block.

The test scene controllers are temporary development presentation, not the
future level-authoring or narrative architecture.

## Phase 1B device rescue boundaries

`0.1.1-device-rescue` fixes physical-device feedback inside existing Phase 1
systems. `TouchStickMath` owns joystick coordinate normalization,
`MovementVectorMath` owns camera-relative movement vector resolution, and
`StraightInputTestRecorder` publishes drift telemetry for the MovementLab
straight-line test. Rendering fixes use URP-safe temporary visual roles and
editor validation rather than final art or gameplay-rule changes.

## Phase 2 first-module boundaries

`0.2.0-first-modules` introduces first playable modules and level-authoring
infrastructure. It intentionally avoids Phase 3 rank/timer results,
progression unlocks, economy, cosmetics, hub flow, online features, procedural
generation, final art, and narrative systems. Local module telemetry can inform
design tuning, but it is not a persisted player-progression contract.

## Phase 3 boundaries

`0.3.0-ranks-and-progression` introduces the first saved game loop. It does not
start ghosts, Debug Trace, Crash Wave, online leaderboards, accounts,
multiplayer, economy, cosmetics, shops, ads, final hub flow, procedural
generation, final production art, or a narrative system. Rank thresholds remain
`Uncalibrated` until physical Android timing data exists.

## Phase 3B visual slice flow

`0.3.5-visual-slice` upgrades presentation through reusable visual profiles
and the runtime module scene adapter:

```text
ModuleDefinition -> ModuleVisualProfile + ModuleEnvironmentProfile
                 -> ModuleSceneController visual kit
                 -> sky/depth/islands/vegetation/mechanic props/HUD/gloves
```

Visual data remains separate from behavior. Gameplay scripts do not inspect
colors or meshes to decide movement, Restore, Patch, rank, score, or save
rules. Module 03 is the hero slice for the future spiral/ascent direction, but
Modules 01 and 02 use the same visual language.

Phase 3B does not start ghosts, Debug Trace, Crash Wave, online features,
economy, cosmetics, final hub flow, procedural generation, final narrative, or
final production art. Physical visual approval is still pending.

## Phase 0.4.0 visual identity flow

`0.4.0-visual-identity` keeps the Phase 3B presentation boundary but adds more
explicit profile data and validation:

```text
ModuleDefinition decoration data
        + ModuleVisualProfile
        + ModuleEnvironmentProfile
        -> ModuleSceneController visual-only kit
        -> named VisualBenchmarkAnchor review positions
```

`ModuleVisualProfile` owns palette, HUD color, rank colors, block surface
presentation values, glove colors, and mechanic accent colors.
`ModuleEnvironmentProfile` owns sky, fog, sun, ambient light, Null Space,
visual density, and the `MobileBalanced` quality target. `RuntimeVisualPulse`
and pooled burst emitters are visual-only feedback helpers.

Gameplay systems must not inspect visual colors, material names, decorative
meshes, particles, or benchmark anchors to make movement, Restore, Patch,
rank, progression, or save decisions. Module 03 is the current hero visual
slice, but it remains data-authored content in the same module system as
Modules 01 and 02.

## Phase 0.5.0 Meshy art-kit flow

`0.5.0-meshy-art-kit` keeps Meshy source and runtime presentation separated:

```text
Assets/_Game/Art/MeshySource
        -> MeshyArtKitIntegrator
        -> Assets/_Game/Art production prefabs/materials/textures
        -> ModuleVisualPrefabLibrary
        -> ModuleSceneController visual-only instances over gameplay colliders
```

`ModuleVisualPrefabLibrary` maps `ModuleMaterialRole` values to optional prefab
placements. `ModuleSceneController` instantiates those prefabs as children of
the existing gameplay blocks or decoration anchors, strips colliders, disables
expensive dynamic renderer features, and hides the primitive renderer when a
prefab visual exists. The gameplay block collider, stable ID, route position,
size, movement behavior, Restore rules, Patch rules, timing, scoring, and saves
remain owned by the data-driven module/gameplay systems.

`ArtKitGallery.unity` is an editor-only review scene and must not be listed in
Android Build Settings. `ART_ASSET_MANIFEST.md` records Meshy source names,
canonical runtime names, rough triangle/vertex counts, texture size, material,
prefab, and deferred assets.

`FirstPersonArmProfile` is the runtime hook for real first-person arms. The
production `RR_FP_Arm_Right` source is the optimized Meshy Neon Vanguard
Gauntlet candidate at 15,400 triangles; `MeshyArtKitIntegrator` clones it into
a runtime right-arm mesh and creates `RR_FP_Arm_Left` by mirroring mesh
vertices/normals/tangents on X and reversing triangle winding. Runtime arm
transforms must keep positive scale. The old approximately 1.98M-triangle mech
gauntlet source remains reference-only and is ignored for runtime generation.
Arms are presentation-only and never affect movement, collision, Restore,
Patch, timing, ranks, progression, or saves.

## Mobile control and parkour movement pass

`0.3.7-parkour-movement` keeps Phase 3B visuals and Phase 3 progression intact
while replacing the centered Flick Jump default. The permanent control
principle is: thumb comfort is more important than screen-edge utilization.
RYDERS BLOCK controls should keep normal thumb travel inside ergonomic
inner-left and inner-right zones while retaining broad touch acquisition.

`TouchControlLayout` owns the active control profile, ergonomic anchors,
comfort margins, jump mode, tap thresholds, drag threshold, sensitivity
presets, and optional legacy flick/debugging/haptics flags. `TouchControlGeometry`
owns safe-origin clamping math. `TapDragGestureClassifier` owns right-region
tap-vs-drag classification. `FlickGestureDetector` remains isolated for
legacy/experimental profiles. `TouchInputCoordinator` converts accepted
gestures into the same abstract jump edge consumed by `ParkourMotor`, preserving
the existing jump buffer and coyote-time rules.

Default anchors are expressed as normalized safe-area positions using Unity's
bottom-origin UI convention: left movement rest center `(0.35, 0.38)` clamped
inside `x=0.30-0.38`, and right look rest center `(0.65, 0.38)`. The default
jump mode is `RightTap`; fixed jump remains available as a development
fallback. The default tap thresholds are `0.16s` maximum duration, `0.035`
movement tolerance, and `0.018` drag activation distance.

## Movement lock pass

`0.3.8-movement-lock` keeps the parkour movement foundation and changes the
default phone contract to `FlowSteerAutoBalanced`: left-thumb throttle and
steering, right-side touch-begin jump, and automatic camera. `MovementProfile`
owns steering yaw rate, low/high-speed yaw rates, acceleration, deceleration,
ground/air/surf steering strength, maximum heading delta, and response curve.
`PlayerInputRouter` exposes flow steering and auto-camera profile state through
`IFlowSteeringInputSource`.

`FirstPersonCameraRig` disables manual touch look for flow-steer profiles,
follows player heading directly, applies automatic pitch presentation, and
keeps base/speed/air/surf FOV data-driven. Development cycling can still switch
to auto direct, auto flow, and legacy manual profiles without a rebuild.

ADR 0009 supersedes ADR 0008 for the default player-facing controls. The
0.3.8 flow-steer/auto-camera work remains a development experiment only.

## Movement V1 pass

`0.3.9-movement-v1` restores the physically preferred mobile contract:
left-thumb movement, right-thumb camera, and right-side Tap Jump. The control
system was not redesigned; the pass polishes jump distance, camera response,
and Android fullscreen.

`MovementProfile` now exposes designer-readable jump timing: `JumpHeight`,
`TimeToApex`, `GravityScale`, `FallGravityMultiplier`,
`BaseTakeoffMomentumRetention`, `HighMomentumTakeoffRetention`, and
`LandingMomentumRetention`. Normal base-run jumps use lower takeoff retention
and snappier fall gravity to reduce overshoot. Higher earned momentum follows a
continuous retention curve toward the high-momentum value so advanced movement
still extends jump range.

`AndroidDisplayService` implements `IPlatformDisplayService`. It requests
Unity fullscreen, applies Android window fullscreen flags, uses modern
WindowInsets/WindowInsetsController hiding on Android API 30+, falls back to
legacy immersive-sticky system UI flags on older devices, and reports safe
area/display state to diagnostics. Physical Samsung Galaxy S23 confirmation is
still required before fullscreen is considered approved.

## Phase 0.7.6 Global Gameplay Readability

`CameraProfile.FixedGameplayPitch` is the shared normal-gameplay camera
framing authority. `FirstPersonCameraRig` applies it to touch play and Restore,
while editor mouse controls remain available. FOV, eye transform, controller,
movement, input yaw, and Restore yaw ownership remain separate contracts.

`PlayerGroundingProfile` owns the cheap contact-shadow radius, receiving
distance, fade, surface normal, segment count, and color. `PlayerGroundingCue`
is visual-only: it raycasts `Ground`, clips its dynamic disc to supported
wedges, has no collider or Rigidbody, and may be removed when a future Ryder
body/shadow solution supersedes it.

`ModuleSceneController` applies the same collider-truth readability shell to
prefab-backed gameplay surfaces in every ModuleRunner level. The shell renders
top, edge, and underside values from `ModuleVisualProfile`; it does not define
collision or gameplay identity. Module 003 biome grouping and landmark
placement remain content data, not global gameplay logic.

## Phase 0.7.7 Camera Recovery And Platform Truth

Physical feedback supersedes the 0.7.6 fixed-pitch and readability-shell
experiment. `CameraProfile.DefaultGameplayPitch` owns only the fresh-spawn
angle; pitch is player-controlled afterward. `FirstPersonCameraRig` consumes
right-side vertical drag in both manual and flow-steering profiles. Flow mode
continues to own yaw through the player root and ignores right-side horizontal
look, while Classic/editor public behavior remains intact.

`RestoreController` captures camera pitch with pre-death yaw and supplies both
to `FirstPersonCameraRig.ResetView` only for fall Restore. Manual restart uses
checkpoint yaw and the profile default. This is presentation state and does
not change checkpoint identity, motor reset, velocity, or save data.

The generated `Collider Truth Readability Shell` is removed globally. Runtime
gameplay blocks retain one root primitive collider at the authored transform;
canonical prefab instances are presentation children with colliders stripped.
Visual geometry may never extend gameplay authority beyond that root collider.

## Phase 0.8.0 Ancient Abyss World Contract

`EnvironmentBiomeProfile` remains the data authority for authored world-object
placement and depth bands. Module 003 owns five curated world groups: a near
causeway, two mid-world places, one hero temple, and one lower ruin city.
World scenery is visual-only and stripped of colliders at authoring/runtime.

Ancient Abyss owns no `BiomeMistLayer`. `ModuleSceneController` no longer has a
radial mist-disc generator or universal world-floor dependency. The biome's
far and lower envelope comes from a Module 003 panoramic sky material, normal
fog/depth grading, and authored non-gameplay structures. This presentation
boundary cannot provide grounding, collision, Restore, Patch, route, movement,
or save behavior.

