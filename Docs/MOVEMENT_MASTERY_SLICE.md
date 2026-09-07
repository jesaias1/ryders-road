## Final verification and APK (2026-09-08)

Source validation passes. EditMode **223/223**, PlayMode **48/48** (19 new cases).
The first full PlayMode run was 47/48: the existing Editor loading-video decoder
timed out. An unchanged full rerun passed; preserve this intermittent test risk.
Evidence: Logs/mastery-final-EditMode.xml, Logs/mastery-final-PlayMode.xml,
Logs/mastery-PlayMode-first-run.xml, and Logs/mastery-source-validation.log.

ARM64 development APK: Builds/Android/RYDERS-ROAD-0.9.6-movement-mastery-slice-dev.apk.
184,424,345 bytes; build 0 errors / 1 pre-existing legacy-icon warning.
SHA-256: 46837E85E1A3B9459BF8A91B80812C57580CFF2FE01EFB274475760460B8F97A.
Package/version/ABI and embedded training profile/room IDs verified; see
Logs/mastery-package-verification.json. Production movement asset SHA-256 still
matches the pre-change snapshot byte for byte.

After the user authorized the phone, adb install -r succeeded on the SM-S911B
with existing data preserved. Installed version was verified; cold launch returned
Status ok and the main menu/Flow Lab entry is visible in
Logs/MovementMasteryQA/s23-launch.png (a separate phone overlay partially covers
the screenshot). Process logs contain no captured managed or fatal startup exception;
two AHardwareBuffer diagnostic errors were present. This is install/startup smoke
only, not human movement, thermal or FPS validation. The physical Gold gate remains open.

# Movement Mastery evaluation slice — 0.9.6

Candidate only. Stop after delivery for S23 feedback. ADR 0018 records isolation.

## Baseline and evidence

The repository audit was correct: projected acceleration, mobile Flow controls,
tap buffer/coyote windows, explicit SurfSurface, platform inheritance and bounded
momentum already existed. No replacement controller was needed.

Pre-change sources/default asset are copied to Logs/MovementMasteryBaseline.
Logs/mastery-baseline.xml records the initial 3-case EditMode measurement. That
fixture did not run normal Awake/contact callbacks; it is not the final contact
baseline. The corrected PlayMode fixture repeats production and candidate at
30/60/120 Hz; Logs/mastery-focused.xml contains all 19 focused passing cases.

| PlayMode measurement | 30 Hz | 60 Hz | 120 Hz |
|---|---:|---:|---:|
| Production run m/s | 7.80 | 7.80 | 7.80 |
| Production measured jump seconds after first step | .633 | .617 | .608 |
| Production jump travel after first step, m | 5.213 | 5.061 | 4.986 |
| Candidate straight airborne m/s | 8.200 | 8.200 | 8.200 |
| Candidate normalized curved input, m/s | 9.250 | 9.222 | 9.209 |
| Candidate yaw without throttle, m/s | 8.200 | 8.200 | 8.200 |
| Fifth buffered hop takeoff retention | 1.000 | 1.000 | 1.000 |
| Fifth delayed hop takeoff retention | .900 | .900 | .900 |

Flights start at 8.2 m/s, use .5 s of input, and curve uses normalized (.65,1).
These are motor measurements, not human technique/performance or frame-rate feel
certification. The original audit's 4.35 m is a ballistic authoring estimate;
active air acceleration and ground probes explain the different live distance.
The production curve diagnostic uses direct (.65,1), not normalized thumb input;
do not use it as an apples-to-apples candidate performance claim.

The final real surf-room entry/exit takes 2.35 s with 1.367 s contact at 60 Hz. Separate
ramp-jump cases validate departure at all three rates. Five-hop, 20-second air
bound, wall impact and reset checks run through the actual controller.

## Parameters and assists

- Candidate asset: Assets/_Game/Player/Configuration/Resources/Training/Movement_Mastery.asset.
- Run 7.8; projected air acceleration 13 m/s², wish cap 8.2; added-energy ceiling
  10.8; horizontal hard safety 18 m/s. Flow throttle scales acceleration strength.
- Jump 1.66 m / .335 s apex; .17 s buffer; .15 s coyote. No auto-hop.
- Normal takeoff .9–1 speed curve preserved. Chained takeoff within .12 s uses 1;
  existing excess-speed bands .055/.12/.18 s retain 1/.96/.90; late .72.
  Landing .99 retained. Grounded delay still brakes in Flow mode.
- Surf: explicit 28–78° eligibility, one authored 32° downhill surface; projected
  gravity 20, acceleration 12 × control .72, wish 8.2, friction .8, speed cap 18;
  jump exit .95 retention and .12 s no-reattach window.
- Simulated elapsed time replaces Time.time for jump measurements. Complete
  takeoff retention includes hop/surf effects. DisplacementVelocity separately
  exposes actual controller displacement rather than calling stored speed travel.

## Flow Lab

Home → FLOW LAB. Four independently retryable rooms, separate from Campaign:
air-strafe room (gain above the straight-input wish cap), three-hop lane, one
broad cyan surf with catch floor, and a two-jump/surf timed challenge. Large safe
floors, surface ticks, edge bands and gold finish markers prioritize readability.
ROOM cycles exercises; RETRY restores in place; A/B compares production/candidate;
CONTROLS compares the same existing two-thumb and Classic mappings; EXIT returns
home. A/B resets and separates session bests by room/profile. No saved training PB.
Surf retry sets an initial 40° view; subsequent manual pitch is unrestricted.

Room geometry and completion thresholds: Assets/_Game/Levels/Resources/Training/FlowLab.json.
The candidate does not modify 001–003 routes, Spiral IDs, Bronze gates, saves,
rank thresholds, Boost, platform inheritance or Campaign control preferences.

## Physical S23 checklist

1. Confirm 0.9.6, enter Flow Lab, compare A/B walking/jumping and immediate retry.
2. Air: compare straight, gentle forward arc, hard turns, and yaw with no throttle.
   Can you understand and use the extra speed, rather than merely see a number?
3. Bhop: tap slightly before contact, on contact and late. Try three/five hops.
   Report missed jumps, accidental jumps and whether timing feels forgiving.
4. Surf: enter at different speeds/angles; steer left/right; jump off and ride off.
   Report contact loss, sticky edges, unexpected launches or camera difficulty.
5. Complete the short flow challenge twice; does a cleaner line save useful time?
6. Test right vertical pitch, Classic/Editor compatibility, both landscape safe
   areas, falls/retry, then representative normal 001–003 routes/Restore/Patch.
7. Observe sustained S23 frame rate, heat and touch comfort. No device performance
   or physical feel is certified by this run.

## Remaining boundary

Single downhill surf only; no banked wall-surf, seam-transfer certification or
multi-plane corner solver. The probe/controller timestep still affects jump
measurements. No speedrunner balance or mastery time target has been calibrated.
Next iteration should address the first specific S23 control/timing/contact
failure, then compare the same baseline. Do not promote globally or start another
production phase before that feedback.



