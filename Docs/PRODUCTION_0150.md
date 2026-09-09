# 0.15.0 — Shared responsive movement and original soundtrack

2026-09-09. The user's physical feedback confirms whole-right hold works but rejects
the restrictive air feel. The user explicitly requests promotion to normal play.
ADR 0029 supersedes the former trial-only gate. New tuning is not physically approved.

## Delivered behavior

One `movement.shared` profile, compatibility **5**, now drives normal Campaign's
five worlds, Spiral, Movement Lab and Flow Lab. Retry/Restore preserve it. Existing
legacy Easy/Classic preferences cannot override its manual right camera and whole
right-surface hold-to-hop contract. Legacy movement remains explicitly development
comparison material. Normal play does not require opening Foundation E.

The new air branch removes the adaptive 0.95 threshold and ordinary 10.8 m/s energy
envelope. A fixed wish projection allows skill-earned acceleration; unused explicit
input budget produces speed-preserving tangential steering. Air acceleration and
opposing braking are 40 m/s². Neutral air and camera yaw alone preserve velocity;
the 18 m/s emergency bound remains. Clean takeoff retains full momentum. Ground
stopping remains prompt. The existing motor, collision system, Boost, surf and
authored routes remain in place; no target assistance or automatic optimal strafe.

Real-controller half-second tests at 60 Hz show a requested 10-degree correction
at 17 m/s now turns 6.95 degrees, versus zero previously. Sustained side input turns
54.08 degrees versus 40.10. At 7.8 m/s those values are 9.14 versus 0.62 and 74.18
versus 61.25. Ground release is 0.108–0.133 seconds across 120–30 Hz; reversal crosses
zero in 0.067 seconds. Four seconds of clean held hops retain 14 m/s. See
[full measurements and reference distinctions](Movement150QA/README.md).

The fresh-contact guard fixes the known reset/immediate-launch stale ground flag.
Moving-platform carry now settles the already-owned support during its separate
controller Move, preserving contact on horizontal and descending motion.

V4 record buckets separate compatibility 5 from old PBs; history, old results and
unlocks are preserved. Rank thresholds/content versions are unchanged. Automated
times are feasibility evidence, not new rank calibration or Gold approval.

## Music and frontend

All three distinct original MP3s were analyzed, retained untouched and used in
measured loop selections. The unsuffixed source supplies the default Road loop;
(1)'s quieter coda supplies menu/Ancient Abyss; (2) supplies Foundry, Spiral and
practice. Final loops are -22 LUFS with headroom, streamed as Vorbis from LFS WAV
production assets. Two persistent music sources crossfade; same-context retries
do not restart playback. Settings exposes independent Music volume. See
[audio provenance and processing](Audio150/README.md).

Home now exposes normal Flow Lab beside Campaign and Spiral. Settings uses a
responsive two-column layout while retaining look, FOV, Master, haptics and graphics.
The approved poster/logo is preserved. [Home capture](Movement150QA/frontend-home.png)
and [Settings capture](Movement150QA/frontend-settings.png) were visually inspected.
No world geometry, art assets, material tuning or unrelated systems were changed.

## Verification

- Full PlayMode: **145/145** passed (`Logs/shared-final-PlayMode.xml`).
- Two subsequently added regression checks: **2/2** passed
  (`Logs/shared-delivery-checks.xml`): grounded reset/immediate airborne launch and
  real frontend Music persistence/layout/Home-to-Flow-Lab navigation.
- Full final EditMode: **248/248** passed (`Logs/shared-final-EditMode.xml`).
- Source foundation and LFS hydration validators passed.
- Normal-world profile/reload/Restore tests cover all five roads and Spiral.
  Actual two-pointer normal Campaign hold/look tests retain repeated legitimate
  jumps and release/cancellation/menu exclusion at 30/60/120 Hz.
- Actual authored moving-platform/lift, Abyss Boost, ramp/surf and legacy camera
  regressions pass. Continuous normal Campaign and explicit E produce identical
  feasibility times: Windward standard **25.04979 s**, Windward fast **17.28324 s**,
  Foundry fast **19.96653 s**, including arrival at Patch Block.
- These scripted pilots provide explicit stick input. They establish authored
  route feasibility, not human movement feel, touch latency or rank thresholds.
- Music tests cross an actual playback loop boundary, mute/unmute, preserve same
  context position, interrupt crossfades and pause/resume. Final waveform boundary
  steps are below interior 99th-percentile steps; perceptual seams remain unapproved.
- Settings rendered and bounds/text checked at 1560×720, 1280×720 and 1920×1080.
  Existing safe-area/landscape and Editor/manual camera regressions remain intact.

The initial full rollout run was 131/145. It exposed serialized shared flags not
loading, a control-cycle bypass and the carry/contact issue. Other failures were
legacy tests assuming the previous default. Flags/controls/carry were corrected;
archived camera comparisons now explicitly choose archived movement. The successful
full run above uses actual responsive shared physics, not the accidentally old path.

## S23 handoff

Open normal CAMPAIGN, FLOW LAB or THE SPIRAL. Development diagnostics display build
version, profile ID/display name and compatibility 5. Whole-right contact holds
jump; dragging looks while the left thumb moves/strafes. Release stops repeating.
Use Development comparisons only to contrast archived motors.

Phone disconnected: no adb wait/install, physical testing or listening is claimed.
Check ordinary/high-speed turns, small corrections, opposing braking, momentum
through held chains, Boost/surf/lifts, both landscape orientations, music seams
and SFX balance through speaker/buds. Sustained 60 FPS, thermals and decode cost
remain physical checks. Stop after delivery for feedback; no Worlds 006–008,
multiplayer, economy or PC-port work.

## Android artifact

APK: `C:/Users/lin4s/Documents/Riders Block/Builds/Android/RYDERS-ROAD-0.15.0-shared-movement-music-dev.apk`

182858783 bytes. SHA-256: `51EBD6EB7F250A4801A61CEECA210ADE7A2ABEC0BB2E0B8B557AF2ABFA4EC6A1`.
Build duration 156.302 seconds, zero errors and one existing legacy-icon warning.
ZIP CRC, APK v2 signature and aapt package verification passed: ARM64, minimum
SDK 26 / target 36, com.rydersblockstudio.rydersblock. Build log:
`Logs/shared-android-build.log`. APK/build caches remain excluded from Git.
Source, documents and music production assets use the existing origin/main + LFS
workflow. No history rewrite, visibility change or unrelated work is included.
