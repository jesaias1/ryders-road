# RYDER'S ROAD Phase 0.7.1 First-Person Arm Framing And Animation Lock

Build ID: `0.7.1-first-person-arm-framing-and-animation-lock`

## Scope

This is the final planned focused POV arm tuning pass. Ryder Arm V2 remains the
active first-person arm. This phase retunes only arm camera FOV, base framing,
minor wrist presentation, bounded visual-only animation, tests, build identity,
and documentation. It does not replace the arm asset, integrate full-body Ryder,
change world content, alter movement, change world camera authority/FOV, touch
controls, collision, progression, saves, or Android orientation behavior.

## Final Framing

- World/gameplay camera FOV remains 94 degrees.
- First-person arm overlay camera FOV is 78 degrees.
- Base anchors are left `(-0.47, 0.018, 0.375)` / `(0, -52, 202)` and right
  `(0.47, 0.026, 0.385)` / `(1, 52, 158)`, with shared scale `0.36`.
- Hands are farther outward and slightly lower/back than 0.7.0 to frame
  gameplay instead of enclosing the lower-center view.
- Elbow bend remains 58 degrees. Wrist pronation is reduced to 68 degrees.
- Finger rest curls remain restrained: thumb/index/middle/ring/pinky
  `2 / 5 / 6 / 7 / 8` degrees before joint weighting.

## Animation

- `NeutralPoseLocked` remains false.
- Animation remains additive around the canonical base pose and visual-only.
- Idle is smaller than 0.7.0.
- Run still alternates fore/aft between left and right arms. Lateral run offset
  opens outward only and never pulls both hands inward toward center.
- High-speed cycle and displacement are more strongly clamped.
- Jump/rise/fall/landing offsets are smaller. Landing moves hands down/back and
  recovers smoothly, prioritizing landing view clarity.

## Validation

- Source validator: `Logs/phase071-source-validator.txt`, passed.
- Foundation validator: `Logs/phase071-foundation-validator.log`, passed.
- EditMode: `Logs/phase071-editmode-results.xml`, `185/185` passed.
- PlayMode: `Logs/phase071-playmode-results.xml`, `6/6` passed.
- Import audit:
  `Logs/phase071-ryder-arm-v2-framing-and-animation-lock-audit.txt`.
- Android build:
  `Builds/Android/RYDERS-ROAD-0.7.1-first-person-arm-framing-and-animation-lock-dev.apk`,
  171,681,386 bytes, SHA-256
  `110C5E1614DAE8599DAC9EB727C641D567E45D494C4052076A19B7007E5E527B`.

## Known Limits

`Logs/HandPoseLab/final-axis-acceptance.txt` was regenerated, but
`Logs/HandPoseLab/final.png` remained blank gray in this batch environment.
Treat the numeric file as diagnostic only.

PHONE NOT CONNECTED. No Android install, launch, screenshot, ADB reconnect, or
physical gameplay test was attempted for this phase.

USER PHYSICAL ACCEPTANCE PENDING. Human review is still required for base pose,
animation comfort, landing visibility, FPS, fullscreen behavior, and final
material/framing acceptance.
