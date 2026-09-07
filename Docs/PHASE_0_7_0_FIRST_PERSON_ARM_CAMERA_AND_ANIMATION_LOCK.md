# RYDER'S ROAD Phase 0.7.0 First-Person Arm Camera And Animation Lock

Build ID: `0.7.0-first-person-arm-camera-and-animation-lock`

## Scope

This phase keeps Ryder Arm V2 as the active first-person arm and changes only
its presentation architecture, pose tuning, visual-only animation, tests, build
identity, and documentation. It does not intentionally change movement,
physics, world camera aim/FOV, controls, route geometry, collision, world
content, UI, progression, saves, stable IDs, or Android orientation behavior.

## Arm Camera Architecture

- `FirstPersonHands` creates or reuses a child overlay camera named
  `First Person Arm Camera`.
- Arm renderers are recursively assigned to the `FirstPersonArms` layer.
- The world gameplay camera keeps the existing 94-degree FOV and excludes the
  arm layer.
- The arm camera renders only `FirstPersonArms`, uses profile FOV 70, near clip
  0.025, far clip 4, depth above the world camera, and no `AudioListener`.
- URP camera data stacks the arm camera as an overlay on the world base camera.

## Pose And Animation

- Base anchors: left `(-0.42, 0.05, 0.41)` / `(0, -60, 205)`, right
  `(0.42, 0.06, 0.42)` / `(1, 60, 155)`, shared scale `0.36`.
- `Bone_003` elbow bend is 58 degrees. `Bone_002` wrist pronation remains
  mirrored 75 degrees. Resting finger/thumb curl is restrained.
- `NeutralPoseLocked` is false for this phase.
- Runtime arm motion is presentation-only: subtle idle, alternating fore/aft run
  swing, jump lift, rise/fall posture, landing compression, and small additive
  wrist/finger response.
- Animation reads movement state but never writes velocity, grounded state,
  camera yaw/pitch, collisions, route selection, completion, progression, or
  saves.

## Validation

- Source validator: `Logs/phase070-source-validator-final.txt`, passed.
- Foundation validator: `Logs/phase070-foundation-validator.log`, passed.
- EditMode: `Logs/phase070-editmode-results-final.xml`, `184/184` passed.
- PlayMode: `Logs/phase070-playmode-results-final.xml`, `6/6` passed.
- Import audit:
  `Logs/phase070-ryder-arm-v2-animation-lock-audit.txt`.
- Android build:
  `Builds/Android/RYDERS-ROAD-0.7.0-first-person-arm-camera-and-animation-lock-dev.apk`,
  171,677,950 bytes, SHA-256
  `0BC88239B6CF4B1E0FB6FC1E42CB60D791467B7C468F655482FE58182484BFF7`.

## Known Limits

`Logs/HandPoseLab/final-axis-acceptance.txt` was regenerated, but the batch
`Logs/HandPoseLab/final.png` output was blank gray in this environment. Treat
the numeric axis file as diagnostic only; physical runtime review remains the
deciding visual acceptance path.

USER PHYSICAL ACCEPTANCE PENDING. Human device review is still required for
arm handedness, base pose, motion comfort, landing visibility, landscape-left
and landscape-right framing, sustained FPS/thermals, and full route play.
