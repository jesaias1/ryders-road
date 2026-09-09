## 2026-09-09 correction - 0.14.1

This supersedes the dedicated glyph scheme described below. The user explicitly
requires the entire right gameplay camera area to be LOOK + JUMP HOLD. Foundation
JumpHeld derives from Look pointer ownership immediately on down. No fixed button,
activation gesture, repeated taps or third finger is required. Every drag delta
continues through the accepted camera normalization/sensitivity; hold is independent
of displacement, gesture duration and camera movement. Unity's drag threshold is
disabled for this candidate surface, and tap classification is bypassed.

Right touch supplies held intent only, not a separate buffered press or release
tap. The unchanged motor rearms the hold at valid contacts, so lifting the thumb
before landing does not leave a queued auto-hop. Editor Space and legacy accepted
tap input retain their contracts. Releasing, disabling the surface, canceling the
contact, pause/focus loss, Restore/retry or opening the menu clears ownership.
UI receives its own raycasts; left movement and existing safe-area boundaries remain.

Keep compatibility 4 and all 0.14.0 movement math/tuning exactly unchanged. This is
an input correction within trial E / Flow Lab E, not Campaign promotion. The prior
fixed-button implementation and APK remain recoverable in commit 5a98fa5. Current
verification/build/handoff: ../PRODUCTION_0141.md. No physical testing is claimed.

# ADR 0027 - intended shared movement foundation, isolated physical candidate

2026-09-09. The user's new movement mission supersedes ADR 0026's prohibition on
movement work. Quake-inspired movement is the intended future shared standard,
not an advanced-physics setting. Physical S23 approval remains a promotion gate.
Official Campaign keeps accepted compatibility 1; existing candidates 2 and 3,
0.13.0 route/art work, versioned PB history and V4 progression are preserved.

## Implementation boundary

Reuse ParkourMotor, MovementProfile, RouteAirControlMath, JumpWindowState,
SurfMovementMath and existing trial composition. Add training-only
movement.foundation-v1 (compatibility 4). Foundation forces the existing manual
view-relative stick path, including lateral-only input; Flow yaw interpretation
cannot change this motor's physics. No automatic strafe, camera-driven velocity
rotation, target/platform tracking, route selection or landing assistance is added.

Candidate E is selected when opening Development > Campaign Flow Trial. A still
loads the accepted motor and saved controls on identical roads. B/C/D retain their
old meanings. The five-road/five-mode menu fits all buttons within its panel.
Flow Lab's motor cycle is B -> A -> D -> E -> B; E enables the new manual input,
and leaving E restores the previous jump-zone geometry and Flow input. Its surf
room remains the authored surf evaluation venue. No course geometry is changed.

## Air and contact model

View-relative wish direction comes from the ordinary two-axis stick vector.
Acceleration acts on dot(velocity, wish), preserving perpendicular velocity below
the energy bound. Reuse ADR 0022's post-acceleration energy limiting; at the bound
it retains a useful heading change instead of rejecting perpendicular input.
The candidate wish cap is max(8.2 * input magnitude, current horizontal speed *
0.95), with the ratio owned by MovementProfile. This permits deliberate oblique
correction above ordinary speed without snapping boost momentum to run speed.
Straight forward input at overspeed remains projection-limited; yaw without move
input applies no acceleration. The ratio does not choose a wish angle for players.

Keep acceleration 26 m/s^2, opposing-input braking 30 m/s^2, run 7.8 m/s, soft
energy ceiling 10.8 m/s and emergency horizontal cap 18 m/s. Existing overspeed
raises the energy ceiling to its current value; it cannot gain more air energy
until it falls below the ordinary envelope. Full opposing input can reverse
velocity. Neutral air input coasts. Manual ground response keeps aligned overspeed
instead of immediately targeting the soft limit. Normal ground braking still
applies when remaining grounded. Clean-hop landing retention is 1.0 rather than
0.99; ordinary takeoff, gravity, jump height and timing windows retain ADR 0022's
values. Jump height 1.66 m, apex time .335 s, descent gravity multiplier 1.38.

Ordinary grounding still requires CharacterController contact. Probe-based surf
recognition, slope-plane gravity/acceleration, collision clipping, platform
inheritance, boost impulses and surf detach timing reuse the previous candidate.
Surf keeps its own existing surface profile and 18 m/s safety envelope; it does
not get camera-driven steering. Walkable ramps retain CharacterController slope
behavior; this milestone does not invent a new ramp-launch mechanic.

## Hold and simultaneous looking

IHeldJumpInputSource is an optional capability; the existing IPlayerInputSource
contract and tap-only implementations remain valid. PlayerInputRouter samples
Space.isPressed in Editor and held contact from the touch coordinator. Only the
candidate profile enables auto-hop. A held intent is rearmed at legitimate ground
or surf contact, never continuously buffered while airborne. Tap presses retain
buffer/coyote behavior. Jump consumption still invalidates the current coyote
window, and now waits for any movement lock to expire before consuming an intent.
A held contact adds neither speed nor an airborne jump. Contact loss, pause,
Restore, retry and input reset clear touch ownership. Surf jumps retain detach
protection. Releasing does not synthesize a second tap.

The existing fixed Jump glyph becomes a distinct 220-layout-unit button at the
layout's inner right rest center (0.65, 0.38), relative to the safe area. Start a
contact on it to jump/hold; drag that same contact anywhere to keep looking and
holding. Jump owns that pointer until release; IDragHandler forwards deltas using
the adjacent look zone's normalization and sensitivity. No third finger is needed.
Start outside the button to look without holding; existing quick right-side taps
remain jump intents. Crossing onto the button after starting a look drag does not
arm jump. Left movement ownership is independent. Existing layouts and preference
bytes are not rewritten by selecting the candidate. The physical trial evaluates
accepted manual view; Landing/Flow camera is not its default.

## Reference principles and intentional differences

Reviewed Valve AirMove/AirAccelerate/Friction and id Software PM_Accelerate/AirMove:
https://github.com/ValveSoftware/source-sdk-2013/blob/master/src/game/shared/gamemovement.cpp
https://github.com/id-Software/Quake-III-Arena/blob/master/code/game/bg_pmove.c
No source/assets were copied. This is an original adaptation of projected wish
acceleration, explicit directional input and separate grounded response. It is
not exact Quake/Source: acceleration uses project m/s^2 units, adaptive overspeed
wish cap and bounded air energy; ground MoveTowards response, asymmetric gravity,
coyote/buffer windows, hold-hop, authored surf and Unity CharacterController remain.

## Promotion and rollback

All Campaign trials skip attempts, completions, ranks, rewards, PB and progression
writes; session bests separate mode, road and content version. New trial E has its
own session key. V4 schema and existing versioned/history meanings do not change.
After physical approval, plan an explicit compatibility-4 Campaign promotion,
including version-compatible PB/rank calibration and shared control rollout.
Do not relabel old PBs, wipe history or promote automatically. Rejecting E requires
only selecting A/ordinary Campaign; the accepted profile assets remain byte-identical.
Keep both old candidates recoverable until the new standard is physically accepted.

Validation, known limitations, APK and the handoff checklist: ../PRODUCTION_0140.md.
Automated feasibility is not a claim of enjoyable feel, Gold, or physical testing.
