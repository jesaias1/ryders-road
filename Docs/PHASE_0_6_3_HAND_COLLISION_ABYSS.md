# RYDER'S ROAD Phase 0.6.3 Report

Target: `0.6.3-hand-pose-collision-truth-abyss-depth`

## Delivered

- Corrected the 0.6.2 first-person hand pose without changing movement,
  camera, collision, or route logic. The hands now sit farther inside the
  screen, keep scale `0.38`, point fingers more forward, and hold only a slight
  inward/downward palm angle.
- Preserved the first-person hands as camera-space presentation only. No hand
  colliders, rigidbodies, movement forces, Restore rules, ranks, saves, or
  progression contracts were added or changed.
- Reworked broad blue water/flow trigger rendering so it no longer appears as
  a landable rectangular platform. Runtime visuals now use non-solid filaments,
  side ribbons, and vertical streaks around the trigger volume.
- Added a guard for broad visual-only platform impostors. Decorative roles that
  would read as jumpable slabs render as non-landable signal strips instead of
  false platforms.
- Deepened Spiral's abyss atmosphere with layered fog banks and vertical red
  glow shafts far below the playable path. These objects are visual-only and do
  not affect collision, fall detection, Restore, Patch, ranks, or saves.

## Verification

- Source validation passed: `Tools/Validate-Foundation.ps1`.
- EditMode `180/180` passed: `Logs/phase063-editmode-results.xml`.
- PlayMode `6/6` passed: `Logs/phase063-playmode-results.xml`.
- Android IL2CPP ARM64 development build succeeded with zero errors and one
  legacy-icon warning: `Logs/phase063-android-build.log`.
- APK:
  `Builds/Android/RYDERS-ROAD-0.6.3-hand-pose-collision-truth-abyss-depth-dev.apk`.
- APK size: 164,488,244 bytes.
- SHA-256:
  `8604F583934CAB7A61984D8344D28947CD9043E0A484660AB863684DA0BA2C23`.
- Samsung Galaxy S23 `SM-S911B`, serial `RFCW100J1HW`: ADB install succeeded,
  launch focused `com.unity3d.player.UnityPlayerActivity`, and the app
  rendered fullscreen at 2340x1080 landscape.
- S23 title smoke `Logs/phase063-s23-title-2.png` shows the title/menu at
  60 FPS with the `0.6.3-hand-pose-collision-truth-abyss-depth` overlay.
- Crash scan found no fatal Android/managed exception patterns in
  `Logs/phase063-s23-launch-log-2.txt`.

## Manual QA Checklist

1. Confirm both hands feel like a natural runner/parkour pose in motion:
   fingers forward, slight inward/downward palms, not stiff or zombie-like.
2. Confirm the hands sit comfortably inside the screen, remain balanced, and
   keep the right hand visible without blocking landing reads.
3. Load Module 03 and confirm blue water/flow visuals do not look like solid
   missing-collision platforms.
4. Scan other exposed map views for any large blue/rectangular/jump-looking
   surfaces that appear landable but are not.
5. Traverse/look through Spiral viewpoints and confirm the abyss reads as deep,
   ominous fog with red glow below the route, not a nearby floor or clutter.
6. Complete a full Spiral route pass and confirm readability, fairness, and
   route rhythm remain intact.
7. Check sustained S23 FPS/heat plus pause/resume in landscape-left and
   landscape-right.

## Acceptance Gate

Physical gameplay acceptance is still required for final hand feel, water/flow
readability, exposed-map collision truth, Spiral abyss mood, full Spiral
traversal, sustained performance, thermals, and pause/resume behavior. The
batch `HandPoseLabRenderer` output was blank gray and is not acceptance
evidence.
