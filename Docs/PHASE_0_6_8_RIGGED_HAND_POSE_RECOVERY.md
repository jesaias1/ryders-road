# Phase 0.6.8 Rigged Hand Pose Recovery

Build ID: `0.6.8-rigged-hand-pose-recovery`
Date: 2026-08-24

## Scope

This phase is a surgical recovery of the rigged first-person hands only. It
keeps the rigged GLB integration from 0.6.7 active and preserves the 0.6.6
unrigged approved arms as the fallback.

No movement, physics, camera/FOV/yaw, controls, world content, fog/scenery,
UI, progression, collision, save schema, or Android orientation behavior was
intentionally changed.

## Pose

- Profile anchors: left `(-0.46, -0.48, 0.48)`, right
  `(0.46, -0.48, 0.48)`, shared scale `(0.38, 0.38, 0.38)`.
- Root eulers: left `(348.34290, 263.26410, 257.66420)`, right
  `(348.34290, 96.73593, 102.33580)`.
- Prefab visual roll offsets: left `(0, -45, 0)`, right `(0, 45, 0)`.
- `RigCorrectionRoot`: position `(0, -0.08, 0.01)`, euler `(0, 0, 0)`, scale
  `(0.58, 0.42, 0.54)`.
- Baked finger curl: fingers `48` degrees, thumbs `20` degrees.
- Static wrist settle: small baked rotations on `Bone_001` and `Bone_002`.
- Forearm-base recovery: visual-only `ForearmEntrySleeve` added under each
  rigged arm root, using the arm material and no collider.
- Runtime animation: neutral pose locked; idle/run/jump/fall/land/boost/water
  offsets and additive rig-bone motion are disabled for approval.

## Verification

- Source validator: passed.
- EditMode: `183/183` passed in `Logs/phase068-editmode-results.xml`.
- PlayMode: `6/6` passed in `Logs/phase068-playmode-results.xml`.
- HandPoseLab production render: `Logs/HandPoseLab/final.png`.
- Direction proof: `Logs/HandPoseLab/final-axis-acceptance.txt`.
  - Finger-forward dot: `0.9651` both sides.
  - Inward-palm dot: `0.5668` both sides.
  - Thumb-up dot: `0.5337` both sides.
- Orientation report: `Logs/HandPoseLab/orientation-report.txt` confirms
  `SkinnedMeshRenderer`, no negative scale, and baked X-mirrored mesh pair.
- Android build:
  `Builds/Android/RYDERS-ROAD-0.6.8-rigged-hand-pose-recovery-dev.apk`,
  167,455,364 bytes, SHA-256
  `A5185EDB2F743A3AE3907074E033D2D0210A1211E2D0ABFDF59D2A5CCCF8F9C7`.
- Samsung Galaxy S23 `SM-S911B` (`RFCW100J1HW`) smoke: install succeeded,
  title/menu rendered after a tap, Spiral loaded fullscreen landscape at
  2340x1080, both recovered rigged hands were visible from the lower corners,
  and the diagnostics overlay reported `FPS 60`, `Ground True`, and
  `PARKOUR_RESPONSIVE_PHYSICAL_V2`.
- Device evidence:
  - `Logs/phase068-s23-launch-after-tap.png`
  - `Logs/phase068-s23-spiral.png`
  - `Logs/phase068-s23-spiral-log.txt`
  - `Logs/phase068-s23-spiral-window.txt`
- Tight crash scan found no fatal Android/managed exception patterns in the
  captured Spiral launch slice.

## Manual QA Checklist

1. Do the hands now feel natural at rest?
2. Are the fingers slightly curled instead of stiff?
3. Do the palms face each other?
4. Do the thumbs point upward?
5. Do the arms still enter naturally from the bottom corners?
6. Is the ugly arm-base cutoff fixed?
7. Does the pose feel like a relaxed running stance?
8. Is any idle motion subtle and non-distracting?
9. FPS check.

## Status

Implementation and automated/device smoke validation are complete. Human
physical pose acceptance is still pending.
