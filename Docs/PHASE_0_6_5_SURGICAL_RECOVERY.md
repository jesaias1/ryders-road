# Phase 0.6.5 Surgical Recovery

Target: `0.6.5-surgical-recovery-and-correct-hand-pose`

## Recovery Scope

- Corrected the first-person hands as a transform/profile recovery, not a new
  art or animation system.
- Removed the broken 0.6.4 generated abyss slab/veil path.
- Removed recent visual filler clutter from early modules, Flow Error, and
  Spiral while keeping gameplay systems untouched.

## Final Hand Profile

- Left local position: `(-0.38, -0.40, 0.40)`
- Left local euler: `(286.91, 36, 136)`
- Right local position: `(0.38, -0.40, 0.40)`
- Right local euler: `(286.91, 324, 224)`
- Prefab local scale: `(0.38, 0.38, 0.38)`
- Conservative presentation values: run forward `0.022`, max displacement
  `0.085`, jump offset `0.032`.

## Removed Visual Regressions

- `CreateEnvironment` no longer calls `CreateNullSpaceDepth`.
- The giant lower-gradient abyss bands are inert.
- The special abyss fog veil renderer was removed.
- Removed `m01.depth.*`, `m02.depth.*`, `m03.depth.*`, `m03.hero.*`,
  `m03.distant.*`, and `m04.world.depth.silhouette-*` visual-only clutter.
- Spiral fog banks were scaled back to subtle authored cloud depth.

## Validation

- Source validation: passed.
- EditMode: `182/182` passed in `Logs/phase065-editmode-results.xml`.
- PlayMode: `6/6` passed in `Logs/phase065-playmode-results.xml`.
- Android development build: succeeded with zero errors and one legacy-icon
  warning in `Logs/phase065-android-build.log`.
- APK:
  `Builds/Android/RYDERS-ROAD-0.6.5-surgical-recovery-and-correct-hand-pose-dev.apk`,
  164,481,736 bytes, SHA-256
  `6A50AE3017EE8DCB7817BBD2621A1B4C1AEB0316EE392D8A3293BC901F44CF3D`.
- S23 launch smoke: installed and launched on `SM-S911B` (`RFCW100J1HW`) at
  2340x1080 fullscreen landscape; `Logs/phase065-s23-title-2.png` shows the
  0.6.5 diagnostics overlay and both hands visible. No fatal Android/managed
  exception patterns were found in `Logs/phase065-s23-launch-log-2.txt`.

## Still Needs Human Device Review

- Final hand orientation and comfort during real play.
- No dark slabs/rectangles from gameplay viewpoints.
- Cleaner world composition compared with 0.6.4.
- Full route traversal, sustained FPS, thermals, and pause/resume.
