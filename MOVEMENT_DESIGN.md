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

