# RYDERS BLOCK Centered Dual-Thumb Controls

Version: `0.3.6-centered-dual-thumb`  
Date: 2026-08-12

Status: superseded as the default control model by
`0.3.7-parkour-movement`. Flick Jump remains legacy/experimental only.

## Scope

This is a focused mobile control refinement on top of Phase 3B. It does not
start new modules, ghosts, Crash Wave, economy, shops, online systems,
procedural generation, or narrative systems.

## Default Controls

- Left thumb: relative camera drag.
- Right thumb: camera-relative movement from a floating control.
- Jump: right-thumb upward Flick Jump plus Tap Anywhere fallback.
- Fixed jump button: available through jump-mode switching, hidden by default.
- Touch auto-steer/auto-pitch: disabled in `Camera_Default`.

Thumb comfort is more important than screen-edge utilization. Normal thumb
travel should stay inside ergonomic inner-left and inner-right zones while
retaining broad touch acquisition.

## Defaults

- Left look rest center: `(0.35, 0.38)` in Unity bottom-origin normalized
  safe-area coordinates.
- Right movement rest center: `(0.65, 0.38)`.
- Joystick size: `300`.
- Horizontal edge comfort margin: `96`.
- Vertical edge comfort margin: `82`.
- Camera sensitivity preset: `Fast`.
- Movement sensitivity preset: `Medium`.

Flick thresholds:

- Minimum velocity: `1.35/s`.
- Minimum distance: `0.055`.
- Maximum duration: `0.22s`.
- Maximum horizontal deviation: `0.075`.
- Direction tolerance: `34 degrees`.
- Cooldown: `0.08s`.
- Rearm retreat: `0.045`.
- Velocity window: `0.08s`.

## Architecture

`TouchControlLayout` owns all tunable control, jump, tap, flick, sensitivity,
and comfort values. `TouchControlGeometry` owns origin clamping. The floating
movement origin clamps inside the active safe-area rect so the full thumb
travel circle remains inside safe area plus comfort margin.

`FlickGestureDetector` recognizes an upward jump flick from speed, distance,
duration, direction, and rearm state. Slow upward drag, upward hold, tiny
jitter, side flicks, downward flicks, and too-sideways diagonals are rejected.

Accepted flicks call the same `PressJump`/JumpIntent path as keyboard Space,
Tap Anywhere, and Fixed Jump Button. `ParkourMotor` remains the only owner of
jump physics, jump buffer, and coyote time.

## Physical Testing

Physical Android approval is still pending. Test thumb resting position, full
control range, forward run without accidental jump, 10-second forward hold,
single Flick Jump, movement continuity after flick, rapid flick chains, camera
turns, safe areas, landscape-left, landscape-right, and jump mode comparison.

## Verification

- Unity validator passed:
  `Logs/centered-validate-unity.log`.
- Edit Mode tests passed `86/86`:
  `Logs/centered-editmode-results.xml`.
- Play Mode tests passed `4/4`:
  `Logs/centered-playmode-results.xml`.
- Source validation passed:
  `Tools/Validate-Foundation.ps1`.
- Android development build succeeded with `0` warnings and `0` errors:
  `Logs/centered-android-build.log`.
- APK:
  `C:\Users\lin4s\Downloads\RYDERS-BLOCK-0.3.6-centered-dual-thumb-dev.apk`.
- APK size: `116,484,569` bytes (`111.09 MB`).
- SHA-256:
  `8B68A359847C847FC16274AC9C4C37FED34EF200CE705E8138EB2584C221736F`.

No physical-device testing was performed in this pass.
