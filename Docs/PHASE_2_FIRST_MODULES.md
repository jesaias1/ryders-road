# Phase 2 First Modules Record

Version: `0.2.0-first-modules`  
Date: 2026-08-11  
Status: implemented on PC; physical Android testing pending

## Scope Implemented

- Development `ModuleSelector` as the default scene after Bootstrap.
- `ModuleRunner` scene that builds the selected module from data.
- Three `ModuleDefinition` assets:
  - `module.001.first-steps`
  - `module.002.moving-parts`
  - `module.003.flow-error`
- Reusable module mechanics:
  - `MovingBlock`
  - `JumpBoostBlock`
  - `WaterFlowVolume`
  - `CrumblingBlock`
  - `PatchBlock`
  - `ShortcutTrigger`
  - `CameraGuideHint`
- Module HUD, development controls, local attempt telemetry, and authoring
  documentation.
- Module visual/environment profiles using original URP-safe prototype
  materials.

## Map Direction

Future RYDERS BLOCK maps should be memorable large places, not loose strings of
obstacles. The main direction is continuous first-person block parkour through
vertical spirals, towers, bridges, water ascents, ruins, shafts, machines, and
floating islands. Module 03 is a compact proof of the vertical-ascent idea.

## Controls

Phase 2 modules use provisional phone controls: left-thumb movement with
steering assist, a right-side jump button, and auto-camera pitch. Editor
keyboard/mouse controls remain available.

The control scheme is not physically approved. It exists so the remote PC
development pass can produce an APK for real Android testing.

## Explicitly Not Implemented

Phase 2 does not include ranks, score thresholds, campaign progression, save
schema changes, ghosts, online features, cosmetics, shop/economy, hub flow,
procedural generation, final art, or a narrative system.

## Verification Required

Before approval, test the APK on a real Android phone and complete
`BETA_FEEDBACK.md`. Required checks include:

- install and launch;
- landscape-left and landscape-right;
- safe area/cutout behavior;
- joystick steering and auto-camera comfort;
- jump button placement and responsiveness;
- Module 01, 02, and 03 completion;
- Restore Point and Patch Block behavior;
- no magenta materials;
- readability against sky/void;
- sustained 60 FPS, thermals, and suspend/resume.
