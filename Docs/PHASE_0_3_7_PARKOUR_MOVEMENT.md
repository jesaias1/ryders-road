# RYDERS BLOCK Parkour Movement Pass

Version: `0.3.7-parkour-movement`  
Date: 2026-08-12

## Scope

This pass supersedes the centered Flick Jump control experiment as the default
mobile profile. It keeps Phase 3B modules, ranks, saves, and visuals intact.
It does not start ghosts, Crash Wave, economy, shops, procedural generation,
online systems, final hub work, or a narrative system.

## Default Mobile Controls

- Left thumb: broad floating movement control centered around normalized
  `x=0.30-0.38`.
- Right thumb: direct manual camera look.
- Jump: short right-region tap on release, using the normal JumpIntent buffer.
- Drag: once displacement crosses the drag threshold, the gesture becomes
  camera-only and release never jumps.
- Fixed jump: available through fallback profiles, hidden by default.
- Flick Jump: legacy/experimental only, no longer default.
- Auto-steer/auto-pitch: disabled by default.

## Movement Foundation

- Ground movement keeps tight acceleration/deceleration and avoids ice.
- Horizontal velocity is not overwritten every frame; useful forward momentum
  can carry through jumps, boosts, air strafing, and surf exits.
- Bunny-hop support is retention based, not auto-bhop and not free speed.
- Air strafing uses bounded wish-direction acceleration and a hard safety cap.
- Surfing is opt-in through authored `SurfSurface` components and `SurfProfile`
  values. Ordinary slopes remain ordinary ground.
- Restore clears transient movement, surf, boost, air-control, and camera FOV
  state.

Bronze progression remains based on normal route completion. Advanced movement
can improve flow and future rank mastery, but Bronze does not require bhop,
surf, shortcuts, or perfect momentum.

## Camera And Presentation

`Camera_Default` originally used an 82 degree base FOV, fast manual touch look,
subtle speed/air FOV increase capped at 88 degrees, and a small landing
awareness pitch when descending without manual right-thumb look input. No
camera roll is introduced.

Android runtime startup applies immersive sticky fullscreen to hide phone
status/navigation bars and reapplies it after focus/resume.

## Verification

- Source validation passed:
  `Tools/Validate-Foundation.ps1`.
- Unity validator passed:
  `Logs/parkour-validate-unity.log`.
- Edit Mode tests passed `94/94`:
  `Logs/parkour-editmode-results.xml`.
- Play Mode tests passed `4/4`:
  `Logs/parkour-playmode-results.xml`.
- Android development build succeeded with `0` warnings and `0` errors:
  `Logs/parkour-android-build.log`.
- APK:
  `C:\Users\lin4s\Downloads\RYDERS-BLOCK-0.3.7-parkour-movement-dev.apk`.
- APK size: `116,560,757` bytes.
- SHA-256:
  `BF79D3052EC7809DFAA5039295A3A86C2A19DDE8790698ACA25880647C232562`.

No physical-device testing was performed in this pass.
