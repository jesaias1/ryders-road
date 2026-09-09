# 0.14.2 - GoldSrc reference verification candidate

2026-09-09. Continues 0.14.1 and ADR 0027; see ADR 0028 and
[technical findings](GOLDSRC_MOVEMENT_REFERENCE.md). The phone is disconnected;
no adb wait, physical installation, phone testing or feel approval is claimed.

## What changed

Reviewed the requested live Xash3D FWGS/HLSDK repositories and pinned their
inspected HEADs. Compared actual source with 0.10.0 and newer Foundation work.
Foundation already has the intended independent mathematical principles, so this
milestone preserves its physics and tuning. The speed envelope still permits
useful correction at ordinary and Boost speeds. Its remaining mobile tradeoffs
are documented explicitly for physical evaluation rather than speculatively
raising speeds or replacing the motor.

Corrected only applied-air-acceleration telemetry after the speed bound, added
regression measurements and strengthened the combined touch test with actual
joystick pointer events. Profile compatibility remains 4. Accepted Campaign,
movement/input/surf profile assets, camera, routes, saves and record meaning remain
unchanged. All trial results stay session-only; no automatic promotion.

## Measured behavior

Real CharacterController, half-second side input, initial forward velocity,
no camera rotation. All 30/50/60/120 Hz cases passed these measurements:

| Initial speed | Final speed | Heading change range |
| --- | --- | --- |
| 7.8 m/s | 10.8 m/s | 60.88-61.19 degrees |
| 10.8 m/s | 10.8 m/s | 55.87-56.41 degrees |
| 14 m/s | 14 m/s | 46.44-46.77 degrees |
| 17 m/s | 17 m/s | 39.79-40.00 degrees |

Ten-degree aligned input at 10.8/14/17 contributes no correction or energy;
explicit input alignment has consequences. Existing left/right, neutral-camera,
opposing-input reversal, held-hop and contact regressions remain in the suite.

The two-pointer test acquires the real left joystick, alternates strafe, holds the
right look surface, sends camera deltas, samples PlayerInputRouter and simulates
the motor. It makes eight hops in five seconds at 30/60/120 Hz; right release stops
subsequent automatic jumps. Nine right-area acquisition points, menu exclusion,
disable, focus/pause cancellation and no dedicated button remain covered. This
is Unity UI event injection, not Android touchscreen or OS event delivery.
Its flat fixture is a control integration test; separate real-route traversal
tests establish representative Campaign feasibility.

The first new neutral-coast test reused a grounded controller immediately after
teleporting it. Unity retained its previous Move contact flag for the first step,
producing one ground-deceleration tick. The revised test uses separate ground and
air fixtures to measure those states independently. Reset-plus-immediate-launch
contact freshness is a known edge case, not fixed or claimed verified here;
ordinary airborne coasting and existing Restore regressions are covered.

## Verification and artifact

Unity 6000.5.6f1: full EditMode 246/246 and full PlayMode 135/135 passed.
The initial focused run was 35/38 before correcting the ground/air fixture;
the full PlayMode run includes all corrected cases. Source foundation and LFS
hydration validators passed. Logs/XML: Logs/reference-EditMode.* and
Logs/reference-PlayMode.*; the build log is Logs/reference-android-build.log.

Representative continuous Foundation results are unchanged from 0.14.0:
Windward normal 25.11646 s, direct 17.33324 s, Foundry fast 19.84987 s.
These use the real motor and authored route collisions with scripted input;
they establish feasibility, not human feel or rank calibration. Existing tests
cover ordinary Foundry links, the Abyss Boost trigger and authored surf behavior.

Fresh [Windward traversal](Movement142QA/windward-scripted.mp4) and
[held bhop lane](Movement142QA/held-bhop-scripted.mp4) captures are retained.
Both decode successfully; representative frames were visually inspected. They
show route correction/turns and straight held hopping separately, not a single
captured human combined-touch turning chain. See [evidence notes](Movement142QA/README.md).

Android development APK:
`C:/Users/lin4s/Documents/Riders Block/Builds/Android/RYDERS-ROAD-0.14.2-goldsrc-reference-dev.apk`

178328552 bytes. SHA-256:
`656D40DC7743123CB1402B5DD22C2F73C5849BE35BBB2AA993B7CC19001D294F`.
Build succeeded in 175.138 seconds, zero errors and one existing Unity
legacy-icon warning. APK ZIP CRC, signature verification and aapt metadata passed:
package com.rydersblockstudio.rydersblock, ARM64, minimum SDK 26 / target 36.
The APK is an excluded local artifact. Source, documentation and retained evidence
follow the existing origin/main workflow. [Validation summary](Movement142QA/validation.json).

## S23 handoff

Open DEVELOPMENT > CAMPAIGN FLOW TRIAL. E - FOUNDATION is selected; choose a road
and START TRIAL. A - ACCEPTED compares the baseline on identical geometry. Normal
Campaign uses accepted physics. For authored surf, select E in Flow Lab.

Touch anywhere in the right gameplay camera area and keep holding. Drag to look
while steering with the left thumb; each valid landing repeats the jump. Release
to stop repeating. No small button, repeated taps, gesture or third finger.

Check both landscapes, release in flight, pause/Restore, thumb comfort, direction
changes, braking and fast correction. Compare Foundry transfers, Windward turns,
Abyss Boost and surf. Evaluate whether the 10.8 energy envelope and small-angle
projection threshold feel restrictive. Sustained 60 FPS, thermals and physical
feel remain unverified. Stop after delivery for the user's feedback.

0.14.1 remains recoverable at `17eab5e`; its APK is retained locally. No saves or
PBs need resetting or migration to evaluate this build.
