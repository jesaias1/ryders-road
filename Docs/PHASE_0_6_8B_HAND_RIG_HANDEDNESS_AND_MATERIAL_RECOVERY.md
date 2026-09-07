# Phase 0.6.8B Hand Rig Handedness and Material Recovery

Build ID: `0.6.8b-hand-rig-handedness-and-material-recovery`
Date: 2026-08-24

## Scope

This phase is a surgical recovery of the rigged first-person hand presentation
only. It keeps the 0.6.7 rigged GLB path active and keeps the 0.6.6 unrigged
arms preserved as fallback.

No movement, physics, camera/FOV/yaw, controls, world content, fog/scenery,
UI, progression, collision, save schema, Android orientation behavior, or
runtime rigged locomotion animation state was intentionally changed.

## Root Cause

- Handedness: no runtime negative-scale mirror was found. The right prefab uses
  the source mesh/rig. The left prefab is a baked X-mirrored mesh with mirrored
  bones. Runtime hierarchy scales remain positive. The perceived swap came
  from pose/presentation: the previous curl and roll made the hands read
  cupped, ambiguous, and clawed.
- Material: the GLB has no authored mesh tangents, and its packed glTF
  metallic/roughness texture was being assigned to Unity's `_MetallicGlossMap`.
  That is not Unity's metallic/gloss layout, which caused muddy dark/scuffed
  shading on mobile.

## Implementation

- Generated tangents for both rigged meshes.
- Imported textures by source role: base color as sRGB, normal as normal map,
  and metallic/roughness as linear data.
- Left `_MetallicGlossMap` unset, disabled the metallic-gloss keyword, restored
  backface culling, and set material values to metallic `0.08`, smoothness
  `0.42`, and normal strength `0.35`.
- Preserved canonical anchors: left `(-0.46, -0.48, 0.48)`, right
  `(0.46, -0.48, 0.48)`, shared scale `(0.38, 0.38, 0.38)`.
- Preserved root eulers: left `(348.34290, 263.26410, 257.66420)`, right
  `(348.34290, 96.73593, 102.33580)`.
- Preserved `RigCorrectionRoot`: position `(0, -0.08, 0.01)`, euler
  `(0, 0, 0)`, scale `(0.58, 0.42, 0.54)`.
- Set prefab visual roll offsets to left `(0, -70, 0)` and right `(0, 70, 0)`.
- Reduced baked finger curl to `18` degrees and thumb curl to `8` degrees,
  with smaller static wrist settle on `Bone_001` and `Bone_002`.
- Kept `NeutralPoseLocked` true and kept runtime rigged idle/locomotion/finger
  animation disabled for static approval.

## Verification

- Foundation validator: passed in `Logs/phase068b-foundation-validator.log`.
- Source validator: passed in `Logs/phase068b-source-validator.txt`.
- EditMode: `183/183` passed in `Logs/phase068b-editmode-results.xml`.
- PlayMode: `6/6` passed in `Logs/phase068b-playmode-results.xml`.
- HandPoseLab production render: `Logs/HandPoseLab/final.png`.
- Direction proof: `Logs/HandPoseLab/final-axis-acceptance.txt`.
  - Finger-forward dot: `0.9651` both sides.
  - Inward-palm dot: `0.1720` both sides.
  - Thumb-up dot: `0.1374` both sides.
- Orientation report: `Logs/HandPoseLab/orientation-report.txt` confirms
  `SkinnedMeshRenderer`, no negative scale, and baked X-mirrored mesh pair.
- Android build:
  `Builds/Android/RYDERS-ROAD-0.6.8b-hand-rig-handedness-and-material-recovery-dev.apk`,
  167,317,614 bytes, SHA-256
  `DE8D2AAA77C9CE62F0370D9C61921B3F6E04E23B9EA7B9FCC7F25FF1BEBD29C6`.
- Samsung Galaxy S23 `SM-S911B` (`RFCW100J1HW`) smoke: install succeeded,
  title/menu rendered after a tap, Spiral loaded fullscreen landscape at
  2340x1080, both recovered rigged hands were visible from the lower corners,
  and the diagnostics overlay reported `FPS 60`, `Ground True`, and
  `PARKOUR_RESPONSIVE_PHYSICAL_V2`.
- Device evidence:
  - `Logs/phase068b-s23-menu.png`
  - `Logs/phase068b-s23-spiral.png`
  - `Logs/phase068b-s23-spiral-log.txt`
  - `Logs/phase068b-s23-spiral-window.txt`
- Crash scan found no fatal Android/managed exception patterns in the captured
  Spiral launch slice.

## Manual QA Checklist

1. Is left/right now correct?
2. Do both arms look like the same material family again?
3. Is the dark/scuffed shading gone?
4. Do the fingers look relaxed instead of clawed?
5. Does the pose feel natural and simple?
6. Are both hands framed well on screen?
7. Any obvious mirror artifact left?
8. FPS check.

## Status

Implementation, automated validation, Android build, and S23 smoke validation
are complete. Human physical pose/material acceptance is still pending.
