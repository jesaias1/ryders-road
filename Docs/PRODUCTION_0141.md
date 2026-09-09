# 0.14.1 - entire right camera area holds jump

Input correction requested on 2026-09-09. Foundation E now activates JumpHeld
immediately when a thumb touches anywhere in the existing right gameplay camera
area. The same contact controls yaw/pitch while holding; movement or duration
never cancels the hold. Lift the thumb to stop automatic jumps. No dedicated Jump
button, activation gesture, repeated taps or third finger is required.

The look surface owns the pointer through drag and release. Tiny drags bypass
Unity's activation threshold. Foundation bypasses tap classification and emits
held state only, preventing a buffered airborne press or release tap from causing
a jump after the thumb lifts. The unchanged motor repeats on legitimate landings.
Left movement, menu/button raycasts and safe-area parenting retain their boundaries.
Menu open, cancellation, disable, pause, focus loss and existing Restore/reset paths
clear ownership. Accepted A/normal Campaign retain their previous input contracts.

ParkourMotor, all movement and surf math, compatibility 4, tuning/profile assets,
camera solver, route geometry and save behavior are byte-unchanged from 0.14.0.
This remains isolated E, pending physical approval. No Campaign promotion occurred.

## S23 handoff

Open DEVELOPMENT > CAMPAIGN FLOW TRIAL. E - FOUNDATION is selected by default.
Choose a road and START TRIAL. Flow Lab E also uses the corrected right surface.

- Touch different parts of the right gameplay area: hold should start immediately.
- Keep the thumb down through several landings while dragging yaw/pitch and steering
  with the left thumb. Confirm natural uninterrupted hopping and camera response.
- Lift while airborne: land without an extra automatic hop. Recontact to resume.
- Try menus, pause/resume, Restore and both landscape orientations. UI contacts must
  not activate gameplay jumps. Check comfort and sustained frame pacing on S23.

No physical device testing is claimed. Stop for the user's feedback after delivery.
Previous APK/commit 5a98fa5 remains recoverable; movement tuning is unchanged.

## Verification and artifact

Unity 6000.5.6f1: all 246 EditMode and 126 PlayMode tests passed. Source foundation
and Git LFS hydration checks passed. Full logs/XML: Logs/right-hold-EditMode.* and
Logs/right-hold-PlayMode.*. The new test raycasts nine right-area positions, drives
actual look handlers/router/camera/motor through held chains at 30/60/120 Hz,
and checks release, unrelated-pointer handling, no fixed button, left/UI separation,
menu cancellation/coverage, disable, pause and focus loss. UI coverage is checked
after a rendered UI update; an immediate same-frame raycast preceded registration.
Existing movement, routes, surf, Boost, camera and save/trial regressions pass.

The candidate control screenshot shows the unobstructed right area without a glyph.
Automated tests prove input/state contracts, not S23 ergonomics or 60 FPS.

[Input capture](Movement141QA/right-hold-controls.png) and
[validation summary](Movement141QA/validation.json) are retained for review.

Corrected Android development APK:
`C:/Users/lin4s/Documents/Riders Block/Builds/Android/RYDERS-ROAD-0.14.1-right-hold-look-dev.apk`

178338616 bytes. SHA-256: `D3A5E8C7BA447FD59188AEE472622EDF0F955E7623190FC15D7B3171241E3CE3`.
Build succeeded; APK ZIP CRC and aapt package/ARM64 metadata passed.
Package remains com.rydersblockstudio.rydersblock, min SDK 26 / target 36.
The previous 0.14.0 APK is retained locally. No physical installation or testing.

Build duration 137.468 seconds, zero errors, one existing Unity legacy-icon
deprecation warning. Source, tests, documentation and retained evidence are
committed to origin/main; the APK remains an excluded local build artifact.
