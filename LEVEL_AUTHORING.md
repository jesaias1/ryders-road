# RYDERS BLOCK Level Authoring

## Phase 0.7.9 Gameplay Truth Contract

A visible required landing must own a non-trigger gameplay collider at the
same authored footprint. Trigger volumes, water ribbons, decorative islands,
and mechanic indicators are never route supports. Do not overlap a normal
block with a Restore, Boost, Crumble, moving, or Patch support; the mechanic
owns the authoritative collider. Module 003's production route keeps ordinary
edge gaps at or below `3.70 m`, places a Restore before and after its Crumble
pressure beat, and reserves longer crossings for explicit Boost assistance or
optional mastery lines. Ordinary blocks receive no generated route chevrons,
centerlines, pulses, or particles.

## Sequential Crumbling Blocks

- Author gameplay size, pose, and stable ID in `ModuleCrumblingBlockDefinition`.
- Production timing is `0.03` activation grace, `1.0` second to collapse,
  `4.0` seconds gone, and `0.025` maximum visual shake. Stage 2 begins near
  one third of the triggered lifetime and Stage 3 near two thirds.
- Stage meshes resolve from `CrumbleVisualSet`; never add collision to those
  prefabs. The primitive gameplay root owns the stationary BoxCollider.
- Valid CharacterController floor contact starts the lifecycle through
  `CrumblingBlockContactRelay`; the child trigger is a fallback. Leaving,
  landing again, or micro-hopping must not restart it.
- Keep Bronze routes completable without waiting for a reset. Spiral uses
  mandatory short/medium crumble chains with standard/Restore recovery beats.
  One-second chains are pressure beats, so normal safe blocks should remain
  obvious breather points before and after them.

## Phase 0.5.6 Playability Contract

Every production module must author a deterministic start point, bind it to a
stable walkable support ID, and set a module fall threshold appropriate to its
vertical bounds. `ModuleStartSafety` validates the player capsule above the
authored support and may correct an unsafe start; production validation rejects
content that needs such correction. Before the first Restore Point, below-bounds
failure restores to this start. Restore Points continue to use explicit spawn
anchors above visible solid support.

Author route spacing from the unchanged production movement envelope. Current
reference values are 4.35 m normal forward range, 2.83 m comfortable Bronze
edge gap, 3.40 m landing-margin distance, and 3.70 m maximum reasonable Bronze
gap. The rebuilt Spiral's measured maximum edge gap is 2.40 m and maximum
single authored rise is 0.85 m.

Use the small rhythm vocabulary `Setup`, `Easy`, `Normal`, `Commit`, `Flow`,
`Run`, `Vertical`, and `Safe` to avoid identical jump cadence. Zero/short gaps
are valid only as intentional run-ups, multi-block approaches, or breathers.
Large safe areas use exact modular tiling or fitted compound collision so the
visible footprint and standable footprint agree.

Production Spiral authoring disables automatic distant fragments. Decoration
must be authored as a small number of landmarks with a route-composition
purpose; negative space is preferred over scattered asset inventory.

RYDERS BLOCK levels are not generic obstacle courses. They are memorable places
that happen to be parkour courses. A good module should be recognizable from
its overall structure even if the individual jumps were removed: the giant
spiral, the waterfall climb, the broken bridge, the floating tower.

## Core Shape

Favor large readable journeys over isolated challenge strings:

- vertical ascent or spiral around a structure;
- floating tower, bridge, canyon, archipelago, ring, machine, ruins, or shaft;
- visible start, destination, earlier progress, and future route sections;
- Restore Points that divide the journey into fair sections.

Module 03 is the first compact proof of this direction: it wraps upward around
a central floating structure and uses Restore Points to preserve ascent
progress.

Module 04, `THE SPIRAL`, is the first larger proof. Use it as the current
reference for long-form macro structure: visible ascent, earlier route views,
future-route previews, fair Restore spacing, and optional advanced lines.
After Samsung Galaxy S23 feedback on 2026-08-16, its Bronze route was retuned
toward broad, calm, normal run/jump traversal. Preserve that phone-completable
spine when polishing visuals or adding optional mastery lines.
After later S23 feedback that Spiral still felt impossible, the Bronze spine
was reinforced with broad connector terraces through the base, moving, water,
boost, surf, and final sections. Preserve those standard-route connectors
unless a replacement route has equal or better phone-completable spacing.

Player-facing structure now separates normal Campaign modules from Spiral mode.
Campaign modules should be straight-forward, shorter teaching levels that grow
more difficult one module at a time. Spiral mode is the long-form climb. Do not
solve campaign pacing by turning every module into another giant spiral, and do
not solve Spiral pacing by compressing the whole route into a crowded staircase.
The current Campaign order is explicit stable-ID progression:
`module.001.first-steps`, `module.002.moving-parts`, then
`module.003.flow-error`. `module.004.the-spiral` is entered through Spiral mode.

## Create A Module

1. Create a `ModuleDefinition` asset under
   `Assets/_Game/Levels/Resources/Modules`.
2. Assign a stable module ID such as `module.004.waterfall-climb`.
3. Use an internal name such as `module_004_waterfall_climb`.
4. Set `ProjectID`, `WorldID`, `ContentVersion`, difficulty, clean/casual time,
   mechanics used, lore bug note, and developer notes.
5. Set Silver, Gold, and Diamond rank thresholds. Bronze is automatic for any
   valid completion, so do not create a Bronze time limit.
6. Mark thresholds `Uncalibrated` until real physical-device timing data
   supports them.
7. Set `SceneName` to `ModuleRunner` unless a later phase adds dedicated scene
   streaming.
8. Assign `ModuleVisualProfile` and `ModuleEnvironmentProfile`.
9. Place `StartPoint` at a safe floor height with a clear first route view.
10. Add normal and precision `Blocks` as data entries.
11. Add `RestorePoints` with unique stable IDs and increasing order.
12. Add one `PatchBlock` at the final destination.
13. Add `MovingBlocks`, `WaterVolumes`, `BoostBlocks`, `SurfSurfaces`, and
    `CrumblingBlocks` only as the module needs them.
14. Add optional `Shortcut` metadata and a trigger region through the asset.
15. Add `CameraHints` only for route visualization or later assisted framing;
    hints must not override movement, camera input, Restore, or completion
    rules.
16. Add Smart Camera route data as broad course-flow corridors: route sections,
    forks, merges, shortcuts, and vertical framing. Do not place route-camera
    nodes on individual platform centers or use them as gameplay targets.
17. Add decoration blocks for islands, tower cores, clouds, vegetation, and
    landmarks without colliders unless they are part of the route.
18. Keep the Bronze route solvable with normal run/jump parkour. Normal
    RYDERS BLOCK parkour should prioritize landing precision over maximum jump
    distance. Advanced movement may extend jump range through legitimately
    earned momentum, but ordinary jumps must remain easy to predict. Add
    momentum, bhop, surf, boost-chain, or shortcut routes only as optional
    skill layers unless a later documented phase changes progression rules.
19. For long-form Easy/Bronze routes, cap ordinary adjacent platform gaps with
    generous connector blocks rather than relying on perfect diagonal jumps,
    boost overshoot, surf, or moving-platform timing. THE SPIRAL has an
    EditMode regression test for this spacing.
20. Keep standard-route blocks visually readable in motion. Normal, precision,
    and crumbling route blocks receive visual-only flow chevrons from authored
    block order; moving, boost, water, and surf blocks keep their own mechanic
    direction language.
21. Add the module to the development selector by placing the asset in the
    Resources/Modules folder.
22. Run `RYDERS BLOCK > Apply Project Settings`.
23. Run `RYDERS BLOCK > Validate Project` and all tests.
24. Build and phone-test before approving the module.

## Smart Camera Route Authoring

Smart Camera route data is for framing, not gameplay. It should describe broad
route flow such as "start climb", "water speed line", "boost ascent",
"surf wall", "high route", "summit merge", and optional shortcut corridors.
It should not describe every block or steer the player to safe landings.

Required authoring rule: route data must describe broad course flow, not
individual blocks; do not target individual platforms.

Branches need stable IDs, clear corridor radii, merge IDs when they rejoin a
standard route, and conservative vertical framing hints. Shortcuts should only
pull camera attention after sustained player velocity/steering intent, and
normal route framing must recover when the player merges, reverses, stops, or
falls off-route.

## Editing Without Code

These changes should require only asset edits:

- move or resize a platform by changing a block pose/size;
- change moving-block speed, path points, pause, loop mode, or easing;
- change water direction, force, drag, or maximum added velocity;
- change boost direction, vertical strength, horizontal strength, or cooldown;
- relocate a Restore Point or Patch Block;
- change expected clean/casual time;
- change Silver, Gold, Diamond thresholds, calibration state, threshold
  version, or designer notes;
- change visual or environment profile;
- change shortcut metadata.

Do not edit player movement, camera, checkpoint, restore, or input code to make
one authored module work. If a genuine controller defect appears, fix it
separately with regression coverage.

## Mechanics

Normal and precision blocks use the same reusable block construction. Precision
is a smaller target, not a separate movement rule.

Moving blocks use `MovingBlock` with editable path points, speed, endpoint
pause, loop mode, easing, and reset behavior. The player inherits movement
through the existing platform tracking in `ParkourMotor`.

A required Moving block must provide meaningful transportation: its path must
bridge the actual route discontinuity, expose a safe boarding window at the
entry, and deliver within a normal jump of the exit. Sideways motion that does
not advance the route is decoration, not a gameplay mechanic. Keep any moving
bypass optional and faster than waiting for the standard ferry.

Jump boosts use `JumpBoostBlock`. They apply an external launch velocity to the
motor and never modify movement profile values.

Validate every required Boost against the exact ascent/fall gravity profile,
target height delta, boost direction, additive approach-speed range, and
landing extents. A nominal approach should land near target center, while slow
and full normal approaches retain a practical boundary margin. Do not approve
a Boost because its arrow merely points at the destination; calculate the
descending arc and confirm it on the target Android device.

Water uses `WaterFlowVolume`. It applies capped directional flow while the
player is inside the volume and lets normal movement recover after exit.

Crumbling blocks use `CrumblingBlock`. Walkable top contact or the fallback
trigger starts a non-restartable delay, then the block disappears until
Restore/restart or its configured reset time.

Surfing is opt-in through `SurfSurface` and `SurfProfile`. Do not rely on
ordinary slopes becoming surf. Surf routes should be marked visually, tested as
optional momentum layers, and kept out of Bronze requirements.

Restore Points use the existing `CheckpointService` and `RestoreController`.
Patch Blocks trigger module completion and local development telemetry only.

## Validation

The module validator checks stable IDs, duplicate module IDs, content version,
required profiles, Patch Block presence, Restore Point ID/order uniqueness,
moving paths, boost directions, water directions, crumbling timing, and required
scene registration. It also validates that rank thresholds are positive and
ordered `Diamond < Gold < Silver`.

Use `RYDERS BLOCK > Rank Calibration` to inspect local completion samples and
manually edit thresholds. Suggestions are not automatic calibration. Physical
Android timing data is required before marking thresholds calibrated.

Debug labels, camera hints, paths, and flow directions are authoring aids.
Normal development gameplay should keep labels hidden unless explicitly
toggled.

Runtime route flow chevrons are player-facing readability aids, not authoring
or gameplay data. They are collider-free child plates generated from authored
block order and must never replace module layout, Smart Camera route data, or
mechanic-specific arrows.

## Phase 3B Visual Workflow

Choose a `ModuleEnvironmentProfile` for sky, fog, sun, ambient light, Null
Space height, cloud density, decoration density, VFX density, and quality
profile. Choose a `ModuleVisualProfile` for block family colors, mechanic
colors, glove colors, HUD colors, and rank colors. A future module should
inherit most identity from those profiles instead of hand-editing every block.
For 0.4.0, the phone target is `MobileBalanced`; use lower density values only
after physical device performance evidence, and reserve `HeroPreview` for
local capture work.

Use `ModuleVisualPrefabLibrary` for imported Meshy or bespoke visual-only
block skins. Prefabs are mapped by `ModuleMaterialRole`, instantiated as
children of the gameplay collider block, and stripped of child colliders at
runtime so imported art cannot silently change movement spacing, landing
fairness, Restore behavior, or physics.

Use decoration `ModuleBlockDefinition` entries as visual kit pieces:

- `DecorationStone`: floating island or rock chunk.
- `TowerCore`: vertical landmark, broken pillar, machine fragment, or ascent
  anchor.
- `Vegetation`: lightweight trees/bushes/tufts.
- `Cloud`: stylized cloud cluster.
- `Corruption`: subtle code-break accent, never a gameplay rule.

Gameplay platforms should keep `Normal`, `Precision`, `Moving`, `Boost`,
`Water`, `Crumbling`, `Restore`, and `Patch` roles so the runtime visual kit
can add consistent tops, sides, undersides, arrows, glow, foam, and fault-line
details. Phase 0.4.0 also adds visual-only surface highlights, insets, trims,
circuit ticks, warm accents, and island caps through profile-driven child
geometry. Do not infer gameplay behavior from material color or decorative mesh
shape.

For visual QA, cycle fixed benchmark views in development builds with `F6`.
The intended comparison points are `Benchmark_Module01_Start`,
`Benchmark_Module02_Mid`, `Benchmark_Module03_Opening`,
`Benchmark_Module03_Early`, `Benchmark_Module03_Middle`,
`Benchmark_Module03_High`, `Benchmark_Module03_Patch`, and THE SPIRAL section
anchors `Benchmark_Spiral_Start`, `Benchmark_Spiral_Low`,
`Benchmark_Spiral_Mid`, `Benchmark_Spiral_Restore`,
`Benchmark_Spiral_Boost`, `Benchmark_Spiral_High`, and
`Benchmark_Spiral_Summit`; see `VISUAL_BENCHMARKS.md`.

Checklist before approving a visual module:

- next platform is readable at speed;
- Null Space is distinct from landing surfaces;
- Restore, Patch, Boost, Moving, Crumbling, and Water are recognizable without
  text;
- boost and water direction cues match authored gameplay direction;
- decorative islands and vegetation cannot be mistaken for route platforms;
- vegetation does not hide jump edges;
- gloves stay out of the landing zone;
- HUD and controls do not block the central route;
- all runtime material roles resolve to URP-safe shaders;
- environment and visual profiles are assigned.

For Module 003 and future production levels, author environment objects as
coherent place-level compositions rather than isolated prop entries. Keep
actual route collision independent. Background architecture must have no
colliders and must clearly sit outside the landable route. A cloud or mist
ocean may be a sky/background treatment, but never a broad horizontal mesh,
receiver, or billboard field beneath gameplay.

Build large maps as a short sequence of recognizable places. Each place should
use a few overlapping structural masses that explain and support the nearby
route mechanic. Do not preserve old route coordinates when they prevent a
clear composition, and delete generated scenery that reads as blockout slabs,
random prop scatter, or a generic void course. Reusable biome atmosphere may
be shared; near-, mid-, far-, and below-world composition is authored for the
specific level. Panoramic skies must be checked at their 360-degree wrap edge
or imported as seamless cubemaps.

## Phase 0.5.2 World Scale & Gap Standards

- **Standard Parkour Footprint**: ~1.5m × 1.5m (0.6m thickness).
- **Long Platform Footprint**: ~3.0m × 1.5m (0.6m thickness).
- **Start / Safe Area Footprint**: ~3.0m × 3.0m (maximum 4.5m for major landmarks).
- **Jump Gaps**:
  - Calm/Bronze standard flat jump: 2.0m to 2.8m.
  - Hard single jump: 3.2m to 3.8m.
  - Physics absolute maximum: ~4.35m (at 7.8 m/s with perfect timing).
  - Vertical rises > 1.0m should keep horizontal edge gaps <= 0.5m unless assisted by Jump Boost.
- **Vertical Air Separation (Spiral / Multilayer)**: Minimum 5.0m to 6.0m vertical clearance between overlapping route coils to prevent visual occlusion and confusion.
