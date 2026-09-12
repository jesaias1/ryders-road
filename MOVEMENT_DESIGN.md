## 0.16.0 geometry benchmark

Physical feedback approves the stronger 0.15 air control. This milestone adapts
Windward geometry and fatal scenery response; movement tuning stays compatibility
5. See Docs/PRODUCTION_0160.md and ADR 0030. Final device feedback remains pending.

## 0.15.0 current movement authority

The user requested game-wide promotion after reporting restrictive 0.14.2 air feel.
Shared compatibility 5 uses explicit 40 m/s² air acceleration/braking, fixed wish
projection, tangential input correction and no ordinary 10.8 m/s energy envelope.
Neutral air, manual camera and clean-hop momentum are preserved; hard cap stays18.
This deliberate mobile adaptation is not a literal CS1.6 clone. Details, measured
before/after and reference distinctions: Docs/Movement150QA/README.md; ADR 0029.
Earlier candidate-only statements below are historical. Physical tuning is pending.

## 0.14.2 current movement-reference authority

See Docs/GOLDSRC_MOVEMENT_REFERENCE.md and ADR 0028 for the inspected Xash3D/HLSDK
principles and deliberate Unity differences. Foundation E retains compatibility
4 and all 0.14.1 tuning, including whole-right LOOK + JUMP HOLD. It remains the
intended shared model pending physical approval; accepted Campaign is recoverable.
Half-second side input turns 17 m/s momentum about 40 degrees without speed loss.
The bounded energy envelope remains a physical-test question. No upstream code
or assets are imported. Docs/PRODUCTION_0142.md records evidence and handoff.

## 0.11.0 Campaign presentation and World 005

ADR 0023 and Docs/PRODUCTION_0110.md supersede historical production stop gates.
Accepted Campaign controls and all isolated 0.10.0 movement trials remain intact.
World 005 is Windward Observatory, using reserved `module.005.foundry-pulse`;
Spiral stays separate. V4 identities and persisted meanings are unchanged.
Continuous directional skies and opt-in profile lighting replace Campaign's
mismatched illustrated cubemap. Loading keeps the existing transition host with
an editable Jesaias emblem and lightweight opacity pulse. Physical S23 approval,
rank calibration, final audio mix and sustained performance remain open.

## 0.10.0 real-route control candidate

See ADR 0022 and Docs/PRODUCTION_0100.md. Explicit Flow thumb deflection supplies
a wish direction offset as well as the familiar yaw steering. Projected air
acceleration, opposing-input braking, and energy limits are separate. The new
ceiling permits steering without extra energy; no-input coasts and yaw-only
does not accelerate. Actual controller contact owns ordinary landing. Candidate
profile and trials are isolated pending S23 approval; accepted controls remain.

## 0.9.9 physical feedback and Campaign Flow trial

S23 feedback makes Flow the promising foundation to evaluate, not an automatic global replacement. 0.9.9 exposes unchanged mastery/production assets on real Campaign roads with separate session results and no save writes. No motor numbers were tuned. Ordinary completion stays accessible; compare linked jumps/arcs/shortcuts against stop-and-line-up runs, using time and misses as well as speed. Old fully manual controls remain recoverable, not mandated as the final scheme.

## 0.9.6 physical evaluation candidate

Campaign tuning remains unchanged. Flow Lab opts into projected-only air movement,
buffered takeoff before braking, and contact-plane surf refinements. Exact parameters,
baseline measurements and the physical gate are in Docs/MOVEMENT_MASTERY_SLICE.md.

# RYDERS BLOCK Movement Design

## Phase 0.5.6 Movement Lock

The movement solver and production profile are unchanged. The measured route
authoring envelope is: grounded run `7.80 m/s`, jump airtime `0.62 s`, normal
forward distance `4.35 m`, comfortable Bronze gap `2.83 m`, landing-margin
distance `3.40 m`, maximum reasonable Bronze gap `3.70 m`, bhop-assisted range
`6.70 m`, air-strafe-assisted range `6.76 m`, and Boost-assisted range
`11.03 m`. These are authoring measurements, not new clamps or physics values.

Spiral's ordinary route remains below the Bronze bound. Bhop, air strafe,
Boost, and Surf remain optional mastery tools and must not become required for
normal completion.

## Permanent Principles

Normal RYDERS BLOCK parkour should prioritize landing precision over maximum
jump distance. Advanced movement may extend jump range through legitimately
earned momentum, but ordinary jumps must remain easy to predict.

The Classic mobile control layout is left-thumb movement, right-thumb camera,
and right-side Tap Jump. After Samsung Galaxy S23 feedback that Easy Mode felt
janky, the 0.4.7 feel sweep restores Classic as the phone default. Easy Mode
with left-thumb movement/steering, Smart Parkour Camera, and right-side Jump
Zone remains available from the run menu. Jump input must never interrupt
left-thumb movement in either mode.

Bronze routes must remain completable with normal run/jump parkour. Bhop,
surf, boost chains, shortcuts, and high-momentum jumps may improve mastery,
times, and optional routes, but they must not become required for normal
campaign progression unless a later decision record explicitly changes that
rule.

## Classic Controls

- Left thumb: camera-relative movement.
- Right-thumb drag: manual yaw/pitch camera.
- Right-side quick tap: JumpIntent on release.
- Right-side drag release: camera only, never Jump.
- Flick Jump and fixed Jump are development or legacy comparisons only.

Editor controls remain available: WASD, mouse look, Space jump, R restore, F1
diagnostics, F2 movement profile, F3 touch zones, and F4 camera effects.

Phase 0.4.6 does not change movement physics. It only exposes the existing
touch look-sensitivity preset through the run menu and persists that setting
between sessions.

Phase 0.4.6B temporarily changed the phone launch profile to Easy Mode:
`FlowSteerAutoBalanced` with Smart Parkour Camera and `RightJumpZone`.
Phase 0.4.7 restores Classic manual control as the launch default while keeping
Easy Mode selectable from the run menu. Smart Camera remains a control/camera
accessibility layer, not a movement rewrite.

## Jump Philosophy

Standard jump is designer-driven through `JumpHeight` and `TimeToApex`. The
motor derives upward velocity and gravity from those values, then applies a
fall-gravity multiplier after the apex. This makes ordinary jumps quick,
snappy, controllable, and easier to land without making the vertical hop feel
weak.

The current responsive default asset uses:

- JumpHeight: `1.66 m`
- TimeToApex: `0.335 s`
- JumpGravity: about `29.58 m/s^2`
- FallGravityMultiplier: `1.38`
- FallGravity: about `40.83 m/s^2`
- BaseTakeoffMomentumRetention: `0.90`
- HighMomentumTakeoffRetention: `1.00`
- LandingMomentumRetention: `0.99`

The previous 0.3.8-style runtime formula estimated a normal jump around
`6.20 m`. The loaded responsive default asset estimates a normal base-speed
jump around `4.35 m` before player correction.

## Normal Versus Momentum Jump

Normal base-run jumps carry slightly less horizontal speed at takeoff. Earned
momentum uses a continuous retention curve based on current speed relative to
BaseRunSpeed and SoftMomentumLimit. This avoids a binary switch: ordinary
jumps become precise, while skilled speed still extends range.

Do not globally clamp every airborne trajectory to the same distance. Boost,
surf exits, shortcuts, coordinated air strafing, and good bhop chains are
allowed to create longer jumps inside the hard velocity safety limit.

## Bhop Philosophy

Bhop should reward clean rhythm without making Jump #1 an automatic speed
generator. A first ordinary jump should mainly preserve intent and remain
predictable. Several good hops may gradually feel faster. Expert chains can
meaningfully preserve or build speed when paired with movement control, but
normal routes must still be fair without bhop.

## Air-Strafe Philosophy

Air control exists for correction and expression, not flight. A normal player
should be able to fix a slightly poor landing. A skilled player should curve
their trajectory deliberately. Air acceleration, wish speed, momentum gain, and
contribution caps must remain bounded so ordinary jumps do not shoot past
platforms.

## Surf Philosophy

Surfing is authored, optional, momentum-oriented, and advanced. Surf surfaces
must be explicit through `SurfSurface`/`SurfProfile`; ordinary slopes must not
secretly become surf. Surf can support shortcuts and mastery but should not be
required for Bronze progression.

## Camera Philosophy

Camera should feel direct, precise, responsive, and low-jitter. Movement V1
normalizes touch deltas against the active look region so sensitivity is less
dependent on phone resolution. A small micro-jitter threshold filters
involuntary noise, fine-look scaling gives small corrections more control, and
fast-swipe gain supports large turns without uncontrolled acceleration.

Manual right-thumb camera input always overrides automatic pitch helpers. The
default camera profile keeps wider FOV and dynamic speed/air/surf FOV but
disables landing awareness until phone feedback proves it helps.

Easy Mode Smart Parkour Camera frames player intent from filtered velocity,
broad route direction, branch intent, and vertical course shape. It is weak
guidance, not autopilot. It must not move the motor, add steering, auto-jump,
snap to platforms, prevent falling, or remove the skill value of bhop,
air-strafe, boost, water, surf, or shortcuts.

## FOV Philosophy

The default vertical FOV is `82` with a cap of `88`. Dynamic FOV should help
speed readability without pumping aggressively during bhop, surf, or boost
movement. Do not increase FOV further without physical-device feedback.

## Fullscreen Philosophy

Android immersive fullscreen is platform display behavior, not gameplay. It
lives in `AndroidDisplayService` behind `IPlatformDisplayService`. Bootstrap
requests fullscreen on startup and reapplies it on resume/focus return.
Important UI respects safe area; the 3D world may render edge-to-edge.

Samsung Galaxy S23 physical validation is still required before the fullscreen
fix is considered approved.

