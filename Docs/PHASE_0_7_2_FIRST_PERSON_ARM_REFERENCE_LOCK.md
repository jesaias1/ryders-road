# RYDER'S ROAD Phase 0.7.2 First-Person Arm Reference Lock

Build ID: `0.7.2-first-person-arm-reference-lock`

## Scope

This is a surgical first-person arm reference-lock pass. The user-supplied image
`C:\Users\lin4s\Downloads\0193c298-1894-4739-b0e9-fc5c1520a8b2.png` was used
only as a visual target for lower-corner forearms, relaxed hands in the lower
third, inward palms, and an open landing lane through the center.

Ryder Arm V2 remains the active first-person arm. This phase retunes only the
arm camera FOV, base framing, wrist presentation, bounded visual-only animation,
tests, build identity, and documentation. It does not replace the arm asset,
integrate full-body Ryder, change world content, alter movement, change world
camera authority/FOV, touch controls, collision, progression, saves, or Android
orientation behavior.

## Final Framing

- World/gameplay camera FOV remains 94 degrees.
- First-person arm overlay camera FOV is 68 degrees.
- Arm near clip remains 0.025.
- Base anchors are left `(-0.58, -0.12, 0.46)` / `(2, -48, 198)` and right
  `(0.58, -0.105, 0.46)` / `(2, 48, 162)`, with shared scale `0.34`.
- Hands sit farther toward the lower corners than 0.7.1 and leave the
  lower-center route/landing view clearer.
- Elbow bend remains 58 degrees. Wrist pronation is reduced to 62 degrees.
- Finger rest curls remain restrained: thumb/index/middle/ring/pinky
  `2 / 5 / 6 / 7 / 8` degrees before joint weighting.

## Animation

- `NeutralPoseLocked` remains false.
- Animation remains additive around the canonical base pose and visual-only.
- Idle, run lateral/vertical motion, jump/fall offsets, landing compression,
  wrist pitch, and finger curl are reduced from the 0.7.1 profile.
- Run still alternates fore/aft between left and right arms.
- Max presentation displacement is `0.06`; max animation multiplier is `1.28`.

## Validation

- Source validator: `Logs/phase072-source-validator-final.txt`, passed.
- Foundation validator: `Logs/phase072-foundation-validator.log`, passed.
- EditMode: `Logs/phase072-editmode-results.xml`, `185/185` passed.
- PlayMode: `Logs/phase072-playmode-results.xml`, `6/6` passed.
- Import audit:
  `Logs/phase072-first-person-arm-reference-lock-audit.txt`.
- Android build:
  `Builds/Android/RYDERS-ROAD-0.7.2-first-person-arm-reference-lock-dev.apk`,
  171,679,642 bytes, SHA-256
  `F2A5E124CFFAF05380F51DC6E91F50A70E99293299E634C22681E433564AFA62`.

## Known Limits

`Logs/HandPoseLab/final-axis-acceptance.txt` was regenerated, but
`Logs/HandPoseLab/final.png` remained blank gray in this batch environment.
Treat the numeric file as diagnostic only.

PHONE NOT CONNECTED by instruction. No Android install, launch, screenshot, ADB
reconnect, or physical gameplay test was attempted for this phase.

USER PHYSICAL ACCEPTANCE PENDING. Human review is still required for final
reference match, base pose, animation comfort, landing visibility, FPS,
fullscreen behavior, and final material/framing acceptance.
