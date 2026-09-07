# RYDER'S ROAD Roadmap

## Core playability lock (`0.5.6-core-playability-lock`)

Implemented and validated on PC plus a bounded Samsung Galaxy S23 rendering
smoke pass. This phase establishes deterministic start/support contracts,
prompt module fall recovery, a measured movement envelope, a clean 58-block
Spiral rebuild with intentional rhythm and landmarks, exact visual/collision
coverage, visually selected first-person arm framing, and a seamless
procedural mobile sky. GoldSrc movement, controls, ranks, progression, saves,
and stable module IDs remain unchanged.

The phase stops here pending human physical evaluation. The required next gate
is an ordinary S23 Bronze run covering the first five jumps, deliberate falls
before/after Restore Points, Moving/Boost/Crumble/Surf/Patch behavior, collision
truth, touch/camera feel, fullscreen/safe areas, and sustained performance.
Only after that feedback should a new phase be selected. Candidate follow-up
work includes targeted route corrections and animation/VFX/art polish; it does
not justify a movement rewrite.

## Phase 0 - Foundation (`0.0.1-foundation`)

Module boundaries, service contracts, versioned save storage, feature flags,
diagnostics, Android landscape configuration, safe area, validation/build
tools, and tests. Completed at commit `db061d1`.

## Phase 1 - Movement laboratory (`0.1.0-movement-lab`)

Implemented and verified in Unity on PC. It added custom first-person movement,
mobile/editor input sources, camera presentation, Restore Point/Null Space/
Restore loop, MovementLab, placeholder feedback, diagnostics, local session
reports, and movement tests.

Phase 1 is not approved by implementation alone.

## Phase 1B - Phone feedback and tuning (`0.1.1-device-rescue`)

Implemented on PC after the first Android feedback. It addressed joystick
drift, multitouch ownership, movement feel, URP material failure, readability,
diagnostics, temporary mobile controls, and mirrored labels. Follow-up
physical-device retesting is still required before those fixes are treated as
approved.

## Phase 2 - First playable modules (`0.2.0-first-modules`)

Implemented under the remote-development exception because the user cannot
locally drive Unity on the laptop. This phase adds the development module
selector, `ModuleRunner`, three data-driven parkour modules, reusable moving,
boost, water-flow, and crumbling block mechanics, module completion telemetry,
authoring documentation, validation, and regression tests.

The original Phase 2 control scheme was provisional and has been superseded by
later control passes. Physical Android feedback is still
required before any mobile control direction is treated as approved.

Phase 2 deliberately does not add ranks, economy, cosmetics, online features,
hub flow, procedural generation, final art, or a narrative system.

## Permanent Map Direction

Future levels should feel like memorable places, not disposable obstacle
strings: tall spiral climbs, floating towers, waterfall ascents, broken
bridges, sky ruins, shafts, machines, and long readable journeys with Restore
Points that preserve progress. Module 03 is the first compact proof of the
vertical-ascent direction.

## Phase 3 - Timer, ranks, score, and progression (`0.3.0-ranks-and-progression`)

Implemented on PC. Adds run timer, Restore Point splits, Bronze through Diamond
ranks, deterministic monotonic score, personal bests, V3 progression saves,
derived module unlocks, results HUD, selector progress summaries, and rank
calibration tooling. Bronze always advances the normal campaign.

Rank thresholds and touch controls remain provisional pending physical Android
feedback. Ghosts, Crash Wave, online leaderboards, economy, cosmetics, and the
final hub are not started.

## Phase 3B - Visual vertical slice (`0.3.5-visual-slice`)

Implemented on PC. Adds the first reusable RYDERS BLOCK visual language:
stylized sky/atmosphere, Null Space depth, floating island kit, vegetation,
water presentation, mechanic visual identities, multi-piece first-person
gloves, cleaner HUD/controls, Module 03 hero composition, visual benchmarks,
and visual QA docs.

Phase 3B is not final art and is not physically approved. Phone checks for
visual readability, safe areas, orientation, performance, controls, and module
pacing remain outstanding. Ghosts, Crash Wave, final hub, procedural maps,
cosmetics, economy, and final narrative are not started.

## Control refinement - Centered dual-thumb (`0.3.6-centered-dual-thumb`)

Implemented on PC. Changes the default mobile controls to left-thumb direct
camera drag, right-thumb movement, and right-thumb upward Flick Jump plus Tap
Anywhere fallback. Adds inward ergonomic anchors, safe-origin clamping, comfort
margins, runtime control/jump/sensitivity switching, gesture diagnostics, and
regression tests.

This pass was superseded as the default control model by
`0.3.7-parkour-movement`; Flick Jump is now legacy/experimental only.

## Parkour movement foundation (`0.3.7-parkour-movement`)

Implemented on PC. Changes the default mobile controls to left-thumb movement,
right-thumb direct camera, and right-region tap jump. Adds tap-vs-drag
classification, Android immersive fullscreen, wider/dynamic FOV, subtle landing
awareness, bounded momentum retention, hop timing retention, air-strafe
foundation, authored surf surfaces, Movement Lab momentum/surf lanes, and
regression tests.

This pass was briefly superseded by `0.3.8-movement-lock`, then restored as
the default control philosophy by `0.3.9-movement-v1` after phone feedback.

## Movement lock / auto-camera foundation (`0.3.8-movement-lock`)

Implemented on PC. Locks the default mobile controls to left-thumb
movement/steering, a broad right-side touch-begin jump zone, and automatic
camera. Adds flow-steering profiles, steering yaw/response tuning,
auto-camera profile diagnostics, surf FOV, and a Movement Lab steering spiral.
Physical Android approval is still pending.

Bronze progression remains normal-route friendly and does not require bhop,
surfing, shortcuts, or advanced momentum. Physical Android comfort,
fullscreen, safe-area, performance, and movement-feel checks remain
outstanding.

This pass does not add new modules, ghosts, Crash Wave, economy, shops,
procedural generation, or narrative systems.

ADR 0009 supersedes this as the player-facing default. The flow-steer and
auto-camera work remains available only as a development experiment.

## Movement V1 polish (`0.3.9-movement-v1`)

Implemented on PC from the latest physical-device direction. Restores the
default mobile controls to left-thumb movement, right-thumb camera, and
right-side Tap Jump without redesigning the control system. Adds a
designer-readable jump model, shorter/snappier normal jump tuning, continuous
normal-vs-momentum takeoff retention, air/bhop tuning, camera jitter filtering
and response curve polish, a Jump Calibration Lane, fullscreen display service,
safe-area/fullscreen diagnostics, and regression tests.

This pass is not physically locked. Samsung Galaxy S23 fullscreen status bar
behavior, normal jump overshoot, camera feel, safe areas, touch comfort,
performance, bhop, air-strafe, and surf still need a real phone test.

## Visual identity / hero map pass (`0.4.0-visual-identity`)

Implemented on PC. Strengthens the current game look toward the supplied target
image with richer sky/horizon layers, profile-driven block surface details,
stronger water/boost/patch/restore identity, pooled bursts, subtle visual
pulses, improved first-person gloves, compact rank HUD polish, named benchmark
anchors, and extra Module 03 hero dressing around the existing ascent.

This pass preserves Movement V1. It does not change GoldSrc-style movement,
left movement/right camera/right Tap Jump, ranks, progression, saves, Restore,
Patch completion, or module rules.

This pass is not physically approved. Phone visual readability, sustained
60 FPS, thermals, fullscreen status-bar behavior, safe areas, touch comfort,
and Module 03 hero readability still need real Android testing.

## Big spiral showcase map (`0.4.5-spiral-showcase`)

Implemented on PC. Adds `THE SPIRAL`, stable ID `module.004.the-spiral`, as
the first substantial vertical showcase map: six route sections, five Restore
Points, water run, boost ascent, optional surf/momentum wall, final corruption
ascent, summit Patch Block, shortcut metadata, Spiral benchmark anchors,
development teleports that invalidate PBs, and expanded diagnostics.

This pass preserves Movement V1 and the default controls. The standard route
is intended for ordinary Bronze campaign completion without requiring bhop,
air-strafe, surf, shortcut routing, or boost overshoot.

Follow-up Samsung Galaxy S23 feedback on 2026-08-16 reported that Spiral still
felt impossible, so the Bronze spine now has broad connector terraces through
the base, moving, water, boost, surf, and final sections, plus an EditMode gap
regression test. Manual full-route completion is still pending.

This pass is not physically approved. The Spiral still needs real Android
testing for route comfort, jump reasonableness, visual readability, fullscreen
status bar behavior, sustained 60 FPS, thermals, and provisional rank
thresholds.

## Alpha vertical slice lock (`0.4.6-alpha-vertical-slice`)

Implemented on PC. This is a polish and lock pass for the current alpha slice,
not a feature expansion. It keeps THE SPIRAL as the focus, hides development
shortcuts behind an explicit drawer, adds a compact run menu, persists touch
look sensitivity, resets touch state on app pause/focus loss, and expands
Spiral benchmark teleports for START/LOW/MID/WATER/BOOST/SURF/HIGH/SUMMIT
review.

This pass preserves Movement V1 and default left movement/right camera/right
Tap Jump controls. Debug Trace, Crash Wave, ghosts, enemies, multiplayer,
economy, cosmetics, procedural levels, and Phase 0.5.0 are still not started.

This pass is not physically approved. It should be followed by a real Android
alpha review, especially on Samsung Galaxy S23 fullscreen behavior, safe areas,
touch comfort, route readability, and sustained 60 FPS.

## Smart parkour camera (`0.4.6b-smart-parkour-camera`)

Implemented on PC. Adds Easy Mode Smart Parkour Camera as a camera-only layer
for left-thumb movement/steering and right-side Jump Zone play. The system
uses camera cone response, filtered velocity look-ahead, broad route-camera
graphs, branch intent hysteresis, shortcut/merge awareness, off-route fallback,
and vertical framing. `THE SPIRAL` now has prototype broad route-camera graph
data, Smart Camera diagnostics, and a run-menu Easy/Classic toggle.

This pass does not change movement physics, jump rules, route completion,
Restore, ranks, progression, saves, Classic manual camera, editor controls,
Debug Trace, Crash Wave, ghosts, enemies, multiplayer, economy, cosmetics,
procedural levels, or Phase 0.5.0.

This pass is not physically approved and Smart Camera is not final. It needs
real Android testing for comfort, route readability, shortcut handling,
fullscreen status/navigation bars, safe areas, sustained 60 FPS, and thermals.

## Brand alpha presentation (`0.4.7-brand-alpha-presentation`)

Implemented on PC. Changes the player-facing product identity to
`RYDER'S ROAD`, imports the approved logo and icon sources, prepares runtime
logo and Android legacy/adaptive icon derivatives, adds a branded title/menu,
module selector, and alpha settings surface, strengthens Android immersive
fullscreen reapplication, and preserves old save paths when a renamed
editor/desktop product path has no current save.

This pass intentionally keeps the legacy package identifier
`com.rydersblockstudio.rydersblock`, stable module IDs,
movement physics, Smart Camera behavior, ranks, progression, Classic manual
camera, editor controls, and scene names unchanged.

The 2026-08-16 progression foundation moves saves to schema V4 with
economy/inventory containers, tested stable-ID helper methods, and one-time
Bronze module-completion rewards. It does not add loot opening, shops,
skin-equipment UI, or cosmetic presentation.

This pass is not physically approved. Samsung Galaxy S23 must still verify
launcher icon appearance, app label, true fullscreen top/status bar hiding,
safe areas, resume behavior, sustained 60 FPS, and thermals. Debug Trace,
Crash Wave, ghosts, enemies, multiplayer, reward systems, shops, procedural
levels, and Phase 0.5.0 are still not started.

## Meshy art-kit integration (`0.5.0-meshy-art-kit`)

In progress on PC. Imports the real Meshy environment kit as copied source
under `Assets/_Game/Art/MeshySource`, produces project-owned runtime
prefabs/materials/textures under `Assets/_Game/Art`, and routes those prefabs
through `ModuleVisualPrefabLibrary` as visual-only skins over existing gameplay
colliders.

This pass preserves Movement V1, Classic controls, Smart Camera boundaries,
stable IDs, saves, ranks, rewards, Restore, Patch completion, and authored
jump spacing. `ArtKitGallery.unity` is editor-only and is not part of Android
Build Settings.

The current `RR_FP_Arm_Right` Meshy source is about 1.98M triangles and is
reference-only. Runtime arm integration, mirrored left-arm generation, and
`FirstPersonArmProfile.asset` activation are deferred until the optimized
approximately 15k-triangle remesh is supplied. The final Surf asset is also
pending.

## Phase 4 - Debug traces, crash chase, and lore integration

Not started. The long-term premise remains documented. Gameplay rules,
movement, camera, checkpointing, restore, and level completion must remain
independent from lore presentation.
