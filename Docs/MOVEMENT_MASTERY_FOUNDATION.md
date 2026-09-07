## Implementation follow-up — 0.9.6

The user's explicit vertical-slice brief advances this design to a playable, isolated
Flow Lab candidate. The audit below remains historical baseline evidence. Current
implementation, test results and remaining physical gate: [Movement Mastery slice](MOVEMENT_MASTERY_SLICE.md).

> 2026-09-07 update: user has now physically tested 0.9.4 and reported startup,
> UI, clipping and repeated-placement issues. Acceptance remains OPEN; see
> [current physical feedback](S23_095_FIRST_THREE_FEEDBACK.md). Earlier statements
> below that no feedback was supplied describe the preceding audit checkpoint.

# Movement Mastery foundation — repository audit and Flow Lab design

2026-09-07. Audit/design only after the 0.9.4 production candidate. No motor,
movement profile, control default, rank threshold or Campaign content changed by
this audit. This document supersedes old plans to proceed directly to 004–008.

## Actual production state

- 001 Sky City v2: user-approved visible quality improvement; preserve authored
  work. This is not evidence that every device/rank check is certified.
- 002 Mountain World v2: technically complete candidate, awaiting physical S23
  acceptance. Final source checks, 222 EditMode and 25 PlayMode cases pass; all
  four mountain cases also pass after final screenshot helper changes. Standard
  jumps, both Moving docks, live carry, Restores, shards and Retry are covered.
  Optional shortcut timing/discoverability and ranks still need human play.
- 003 Ancient Abyss v6: recorded collision/Restore/support/Boost corrections are
  present and covered by the passing regression suite. Physical Gold gate remains
  open. No new physical failure report was supplied for this candidate. Do not
  invent another redesign to stand in for acceptance.
- Actual Campaign buttons exercise fresh locks, Bronze Next, replay and Continue;
  progression uses valid completion evidence, preserves V4 and stable IDs, and
  keeps Spiral separate. All-three UI navigation is covered by HotfixNavigationTests.
- No post-0.9.4 runtime phase is inferred. Current deliverable remains the 0.9.4
  APK; the subsequent work here is package verification and this audit/design.

Evidence: S23_094_MOUNTAIN_WORLD.md, S23_091_CONTINUATION.md, ADR 0016,
Logs/phase094-editmode-final-results.xml, Logs/phase094-playmode-final-results.xml,
Logs/phase094-final-capture-results.xml, Logs/phase094-package-verification.json.
No physical device was attached during the recorded ADB inventory.

## Solver and input findings

Sources: ParkourMotor.cs, MovementMomentumMath.cs, JumpWindowState.cs,
SurfMovementMath.cs, Movement_Default.asset, JumpBoostBlock.cs,
PlayerInputRouter.cs, TouchControlLayout.cs and TouchInputCoordinator.cs.
All paths are under Assets/_Game; no replacement motor is warranted.

| Area | Actual implementation | Implication / focused gap |
|---|---|---|
| Ground acceleration | Both branches use vector MoveTowards. Classic preserves aligned overspeed up to the soft limit and brakes reversals; Flow steering always targets throttle/run speed. | Ground acceleration is not projected GoldSrc acceleration. Flow can lose earned overspeed before a buffered takeoff. Measure this before considering an isolated change. |
| Friction | With no input, linear deceleration plus ground friction; with input, acceleration toward desired velocity. | Not a speed-proportional Source friction law. Preserve accepted behavior pending measurements. |
| Wish direction | Classic resolves body/camera yaw-relative forward/right; Flow maps left X to smoothed body yaw and left Y to forward/reverse throttle. | Flow has no independently commanded lateral strafe. A turned forward wish can still create projected acceleration; mobile skill benefit needs measurement. |
| Air acceleration | Both use dot(velocity, wish), positive wish-speed deficit and an acceleration cap. Default air acceleration 13, wish speed 8.2. | Real projected acceleration already exists; do not add a duplicate strafe system. |
| Air steering/gain | Classic additionally rotates velocity toward wish and blends it, then adds 0.32 gain below soft limit when aligned. Flow has yaw steering and its own gain condition. | This is assisted custom movement, not exact Source. Straight input can gain too; successful strafe feedback must compare useful output, not just positive speed delta. |
| Momentum | Default run 7.8, soft 10.8, safety 18; takeoff retention interpolates 0.9–1, landing retains 0.99. | Soft limit is not a universal speed clamp; Boost/air state can exceed it until hard safety. |
| Jump/bhop | Buffer 0.17 s, coyote 0.15 s; hop bands 0.055/0.12/0.18 s, excess-speed retention 1/0.96/0.9, late 0.72. No held auto-hop. | Existing assistance is substantial. Hop loss applies above base speed, after takeoff retention. Ground update occurs before jump consumption. |
| Timing feedback | Hop classification uses elapsed time since landing; pre-landing buffered input consumed near landing can classify Perfect. | Do not call that exact human timing. Record press-to-contact offset separately if teaching timing. Current LastTakeoffMomentumRetention excludes subsequent hop retention. |
| Collision/velocity | CharacterController.Move resolves displacement; ceiling flag clears upward velocity. No general wall-normal clipping of stored horizontal velocity. | Wall contact may leave reported velocity different from actual displacement. Training needs both; multi-plane clipping is a separate solver experiment, not a cosmetic fix. |
| Surf | Authored SurfSurface plus downward sphere probe and angle validation. Velocity projected onto plane; downslope acceleration, steering, friction and surf speed cap. Surf jump applies exit retention/influence. | Real mechanic exists, but sustained contact, seams and transfers are not established by math tests. No generic wall becomes surf. |
| Moving | Ground transform delta carries controller; horizontal platform velocity inherited on departure. | Existing 002 live carry/dock checks pass. Measure takeoff retention on moving support in a later mastery fixture. |
| Boost | Trigger adds authored horizontal impulse, raises vertical speed if stronger, clears surf/jump window, enforces safety cap and cooldown. | Existing 003 actual-trigger destination test passes at tested approach speeds. Do not alter physics to repair route geometry. |

The required mobile mapping already exists in FlowSteerAutoBalanced (Easy): left
movement/yaw, right tap Jump, right vertical drag pitch. Right yaw is suppressed
in the Flow camera path. Default Touch_Default is enum 4, Classic manual; saved
versioned Easy choices are preserved. Editor WASD/mouse remains manual. The hard
mobile requirement is therefore supported as an option, not the shipped default.
Changing that default and forcing saved Classic users would be a behavioral
migration; this audit makes neither change. No new permanent buttons are needed.

## Smallest coherent next implementation

1. Use the existing optional Easy/Flow input mapping for a separate training
   candidate and preserve Classic as a compatibility option. Keep the same motor
   and authoritative movement asset initially. Do not silently switch Campaign.
2. Add a small deterministic motor measurement fixture before teaching speed
   gain: straight flight versus matched-duration alternating steering; buffered
   hop versus delayed hop; a five-hop chain; wall impact; single ramp contact and
   ramp seam transfer. Run both input branches at 30/60/120 Hz with identical
   initial states. Record displacement, velocity, time, takeoff/landing speed,
   contact duration and failures. Use simulated elapsed time, not Time.time-based
   jump telemetry when manually stepping multiple frames in one test frame.
3. If Flow ground braking erases useful timing rewards, compare one isolated
   profile/strategy experiment preserving aligned overspeed during buffered
   takeoff. Do not rewrite the complete solver or enable auto-hop by default.
   Profile-only changes cannot express every branch behavior; keep any needed
   strategy opt-in and disabled for Campaign until regression and S23 acceptance.
4. Select target deltas only after baseline measurement. Require a repeatable
   skilled-line advantage over the ordinary line without increasing Bronze jump
   requirements. Existing Campaign ranks remain provisional and unchanged.

## Flow Lab — optional design, not implemented

Reuse the existing Movement Lab's loadable scene/module infrastructure after
inspection; do not mistake its loadability test for a finished training mode.
Add a distinct frontend training entry outside the ordered Campaign catalog,
accessible from a fresh save without ranks. Keep existing lab IDs stable; proposed
new content IDs use a separate training namespace, never Campaign list indexes.

| Room | Repeatable exercise | Feedback / release condition |
|---|---|---|
| A Air steering | Broad takeoff and forgiving curved landing | Useful travel direction and landing; safe first slice with current solver. |
| B Air-strafing | Paired straight and curved lanes | Exit speed and lane time; release only after steering advantage is measured. |
| C Bhop timing | One repeatable landing/takeoff | Press-to-contact timing and complete retained momentum, not a misleading Perfect label. |
| D Bhop chaining | Three then five wide landings | Consecutive jumps and speed retention; ordinary completion remains possible. |
| E Surf introduction | One broad authored ramp and catch platform | Contact state and exit speed; requires real controller contact test. |
| F Surf transfers | Two ramps with generous restart | Transfer success and useful exit speed; defer if seams are unreliable. |
| G Flow course | Short loop combining released exercises | Time/PB and optional cleaner line; no Campaign rank or unlock writes. |

Use a removable training session component for room start/finish/retry, authored
room definitions for tunables, and a presentation-only feedback component reading
motor events plus measured displacement. It must not apply forces or correct
landings. Retry resets motor pose/velocity, input ownership, camera, timer and
room resettable mechanics in place; no scene reload. A failed attempt offers
immediate return to the room start. Short contextual hints and fading speed/timing
feedback remain training-only. Reuse existing UI conventions; no ability buttons.

First prototype should be A plus retry and a speed trace. B–G are conditional,
not a promise that the current solver already supports expert-level mastery.
Initially keep room PBs in memory. If persistence is later wanted, add an explicit
save-schema migration with training ID, movement compatibility/profile and room
version; never write training results into normal Campaign records.

## Acceptance boundary and next physical decision

No speculative Flow Lab runtime was added while first-three acceptance remains
open. Test the verified 0.9.4 APK on S23: accept/reject 002's standard route and
Moving exits, and confirm whether 003's corrected supports, Boost, Crumble and
Restores resolve the previously reported failures. Give the exact location and
symptom for any remaining problem. Check both landscapes and sustained performance.

Then assess the existing Easy mapping with two thumbs: can steering/yaw and
right-tap jumping maintain a deliberate line while right vertical drag provides
comfortable pitch? That physical result determines the training candidate's
control/feel baseline before changing the Campaign default or momentum rules.
Modules 004–008, embodiment and new powerups remain deferred.

