# Phase 0.3.9 - Movement V1

Version: `0.3.9-movement-v1`
Date: 2026-08-12

## Scope

This pass implements the attached Phase 0.3.5 Movement Lock, Camera Polish, and
True Mobile Fullscreen brief as the next forward project build. The repo had
already reached `0.3.8-movement-lock`, so the metadata remains forward-moving
instead of downgrading to `0.3.5`.

## Implemented

- Restored the default mobile contract to left-thumb movement, right-thumb
  camera, and right-side Tap Jump.
- Kept flow steering, broad right jump zones, fixed Jump, and Flick Jump as
  development/legacy comparisons only.
- Added designer-readable jump parameters: `JumpHeight`, `TimeToApex`,
  `GravityScale`, `FallGravityMultiplier`, takeoff momentum retention, and
  landing momentum retention.
- Tuned the loaded default movement asset for shorter normal jump distance:
  `JumpHeight 1.62`, `TimeToApex 0.34`, fall gravity multiplier `1.35`, base
  takeoff retention `0.88`.
- Retuned default air acceleration, air control, air momentum gain, and air
  contribution cap so ordinary jumps remain controllable but not flight-like.
- Preserved momentum, bhop retention, boost, water, and authored surf support.
- Added jump takeoff/apex/airtime/distance diagnostics and movement snapshots.
- Added a Jump Calibration Lane to MovementLab.
- Normalized right-camera deltas against the active look zone, filtered
  micro-jitter, added fine-look/fast-swipe response, and disabled default
  landing awareness.
- Added `IPlatformDisplayService` and `AndroidDisplayService` with modern
  Android WindowInsets hiding on API 30+ and legacy immersive-sticky fallback.
- Reapplies immersive fullscreen on startup, app resume, and focus return.
- Added fullscreen/safe-area/focus/API diagnostics.
- Updated validators, tests, beta plan, and movement documentation.

## Measurements

Formula estimates before physical retest:

- Previous normal jump estimate from the older runtime formula: about `6.20 m`.
- New runtime fallback normal jump estimate: about `4.45 m`.
- New loaded Movement V1 default asset normal jump estimate: about `4.12 m`.

These are deterministic model estimates, not Samsung Galaxy S23 physical
measurements.

## Verification

- Source validation: passed.
- Unity project validation: passed, `Logs/movement-v1-validate-unity.log`.
- EditMode tests: passed `100/100`,
  `Logs/movement-v1-editmode-results.xml`.
- PlayMode tests: passed `4/4`,
  `Logs/movement-v1-playmode-results.xml`.
- Android development build: succeeded with `0` warnings and `0` errors,
  `Logs/movement-v1-android-build.log`.
- APK copied to
  `C:\Users\lin4s\Downloads\RYDERS-BLOCK-0.3.9-movement-v1-dev.apk`.
- APK size: `116,652,221` bytes.
- SHA-256:
  `CFDB89308C97D4595F9C9BC600EF44B06E36B724AB234F96EBB6FAAED8CA282A`.

## Still Requires Phone Testing

- Samsung Galaxy S23 status bar and navigation bar behavior.
- Landscape-left and landscape-right safe-area behavior after immersive mode.
- Normal jump overshoot rate across 20 ordinary jumps.
- Camera tiny corrections, 90 degree swipes, tap-vs-drag transition, and
  perceived jitter/lag/float/stickiness.
- Bhop, air-strafe, surf, boost, and water compatibility.
- Sustained 60 FPS, thermals, and touch comfort.

Movement V1 must not be declared physically locked until that test is complete.
The large visual/art phase was not started.
