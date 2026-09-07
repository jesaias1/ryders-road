# RYDER'S ROAD Phase 0.7.3 Surgical Arm Presentation Recovery

Build ID: `0.7.3-surgical-arm-presentation-recovery`

## Scope

This is a recovery pass. User review rejected the 0.7.2 first-person arm result
as too low, too hidden, and not natural enough for running-arm presentation.
0.7.2 is not accepted as the visual baseline.

The recovery uses 0.7.1 as the most recent pre-regression baseline and makes
small targeted corrections only. Ryder Arm V2 remains the active first-person
arm. This phase retunes only arm camera FOV, base framing, wrist presentation,
bounded visual-only animation, tests, build identity, and documentation. It
does not replace the arm asset, integrate full-body Ryder, change world content,
alter movement, change world camera authority/FOV, touch controls, collision,
progression, saves, or Android orientation behavior.

## Final Framing

- World/gameplay camera FOV remains 94 degrees.
- First-person arm overlay camera FOV is 72 degrees.
- Arm near clip remains 0.025.
- Base anchors are left `(-0.52, -0.025, 0.41)` / `(0, -52, 202)` and right
  `(0.52, -0.02, 0.41)` / `(1, 52, 158)`, with shared scale `0.36`.
- Hands are meaningfully higher than 0.7.2, more readable, and still separated
  from the lower-left/lower-right corners.
- Elbow bend remains 58 degrees. Wrist pronation returns to 68 degrees.
- Finger rest curls remain restrained: thumb/index/middle/ring/pinky
  `2 / 5 / 6 / 7 / 8` degrees before joint weighting.

## Animation

- `NeutralPoseLocked` remains false.
- Animation remains additive around the canonical base pose and visual-only.
- Idle, run lateral/vertical motion, jump/fall offsets, landing compression,
  wrist pitch, and finger curl are small and conservative.
- Run still alternates fore/aft between left and right arms.
- Max presentation displacement is `0.052`; max animation multiplier is `1.22`.

## Validation

- Source validator: `Logs/phase073-source-validator.txt`, passed.
- Foundation validator: `Logs/phase073-foundation-validator.log`, passed.
- EditMode: `Logs/phase073-editmode-results.xml`, `185/185` passed.
- Import audit:
  `Logs/phase073-surgical-arm-presentation-recovery-audit.txt`.
- HandPoseLab numeric diagnostics:
  `Logs/HandPoseLab/final-axis-acceptance.txt`.

## Known Limits

`Logs/HandPoseLab/final.png` remained the same small batch-render artifact in
this environment. Treat the numeric file as diagnostic only.

PHONE NOT CONNECTED by instruction. No ADB, install, launch, screenshot,
Android build, or physical gameplay test was attempted for this phase.

USER PHYSICAL ACCEPTANCE PENDING. Human review is still required for final pose
comfort, landing visibility, FPS, fullscreen behavior, and material/framing
acceptance.
