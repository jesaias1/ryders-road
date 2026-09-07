# Phase 1B Device Rescue Record

Version: `0.1.1-device-rescue`  
Title: `RYDERS BLOCK`  
Status: implemented and PC-verified; physical retest pending

## Trigger

The first Android physical-device feedback reported rightward drift when the
joystick was held forward, janky movement feel, magenta gameplay materials,
weak platform/void readability, oversized diagnostics, crude temporary mobile
controls, mirrored world text, and presentation still far from
`Docs/VisualReferences/target_gameplay.png`.

Known device facts only: Android physical device, landscape, approximately
`2340 x 990`, and approximately 60 FPS observed. Hardware model and Android
version were not recorded.

## Scope

This pass stayed inside Phase 1 systems. It did not add Phase 2 levels, ranks,
ghosts, scoring, economy, shops, progression, online systems, final art,
procedural generation, or narrative systems.

## Implemented

- Fixed joystick local-point normalization so the RectTransform center maps to
  neutral input and straight up maps to `(0, 1)`.
- Preserved explicit touch ownership by stable pointer IDs and made rejected
  release events unable to clear an owned joystick/jump control.
- Added movement diagnostics for raw joystick, post-deadzone joystick,
  normalized movement, camera vectors, projected movement vectors, desired
  velocity, actual horizontal velocity, and lateral velocity.
- Added `STRAIGHT INPUT TEST` with center line, boundaries, 10m/20m/30m
  markers, and drift telemetry.
- Replaced default runtime primitive materials with URP material generation,
  a data-driven visual profile, Resources shader references, and editor
  material validation.
- Improved temporary visual readability with brighter platform roles, darker
  undersides, edge lines, darker void, fog/lighting changes, and distant
  non-colliding floating silhouettes.
- Reduced diagnostics to Normal and Full presentation modes.
- Reworked temporary mobile controls toward circular translucent controls.
- Reoriented world-space labels with camera-facing billboard behavior.
- Tuned movement and camera profiles conservatively for steadier mobile feel.

## Verification

PC verification after implementation:

- Unity Phase 1 validation passed with material validation.
- Edit Mode tests passed 41/41.
- Play Mode tests passed 2/2.

Physical-device approval was not performed in this pass. `BETA_FEEDBACK.md`
contains the retest checklist for build `0.1.1-device-rescue`.

Phase 2 remains blocked until Phase 1B physical-device retest results are
recorded and reviewed.
