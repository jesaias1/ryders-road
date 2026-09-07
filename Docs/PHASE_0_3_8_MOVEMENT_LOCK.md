# RYDERS BLOCK Movement Lock Pass

Version: `0.3.8-movement-lock`  
Date: 2026-08-12

Superseded as the default mobile control direction by
`0.3.9-movement-v1` / ADR 0009. This record remains historical.

## Scope

This pass locks the default phone control philosophy after the parkour movement
foundation. It keeps Phase 3B visuals, Phase 3 ranks/progression, save schema
V3, module data, bhop, bounded air control, boosts, water, surf, Restore
Points, and Patch Blocks intact.

## Default Mobile Controls

- Left thumb: movement throttle plus steering intent.
- Right side: large invisible jump zone; touch begin emits one JumpIntent.
- Camera: automatic by default, following player heading with subtle pitch and
  dynamic FOV presentation.
- Manual right-thumb camera, right tap-release jump, fixed jump button, and
  Flick Jump remain legacy/development profiles only.

## Movement And Camera

`TouchControlLayout` defaults to `FlowSteerAutoBalanced` and `RightJumpZone`.
`ParkourMotor` now supports flow steering through `IFlowSteeringInputSource`.
`MovementProfile` owns steering yaw rate, low/high speed yaw rates, steering
acceleration/deceleration, ground/air/surf steering strength, maximum heading
delta, and steering response curve.

`FirstPersonCameraRig` disables manual touch look for flow-steer profiles,
tracks the active auto-camera profile, applies automatic pitch assist, and
adds surf FOV to the existing base/speed/air FOV model. Android immersive
fullscreen remains applied at launch and focus/resume.

## Movement Lab

MovementLab now includes a Movement Lock Lab beyond the momentum/surf lanes:
a forward throttle lane, center-line readability check, small rising steering
spiral, and Restore Point for repeat testing. The original temporary Patch
Block remains in place for the existing completion path.

## Verification

- Source validation passed:
  `Tools/Validate-Foundation.ps1`.
- Unity validator passed:
  `Logs/movement-lock-validate-unity.log`.
- Edit Mode tests passed `95/95`:
  `Logs/movement-lock-editmode-results.xml`.
- Play Mode tests passed `4/4`:
  `Logs/movement-lock-playmode-results.xml`.
- Android development build succeeded with `0` warnings and `0` errors:
  `Logs/movement-lock-android-build.log`.
- APK:
  `C:\Users\lin4s\Downloads\RYDERS-BLOCK-0.3.8-movement-lock-dev.apk`.
- APK size: `116,621,125` bytes.
- SHA-256:
  `D27950AAB8ECD77355C05E619F9AA1F39F28E4D2E434709BFE03A9D39261C717`.

No physical-device testing was performed in this pass.
