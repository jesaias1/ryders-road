# 0.9.7 — landing-view evaluation and production preservation

2026-09-08. Mission authority: ADR 0019. Recoverable inherited checkpoint:
`159565a` (complete 0.9.6 project sources, content and settings). Existing ignored
builds/logs/caches and unrelated root captures remain in place.

## Delivered

- Home → FLOW LAB begins at LANDING PRECISION: two ordinary gaps and a lower
  final support, with wide retryable landings and no invisible helper floor.
- VIEW compares the original manual pitch with optional landing framing. MOTOR
  A/B independently compares the unchanged production/mastery profiles. CONTROLS
  retains Flow/Classic; Editor mouse/keyboard remain intact. Retry is immediate.
- Optional view begins at 16° down (surf room retains its 40° authored start),
  adds at most 8° during descent, caps the framed target at 24°, and changes at
  at most 36°/s. No descent assist while surfing or ascending. Manual drag takes
  over the rendered angle and holds off assist for 1.2 s. No forced recenter.
- Best times are separated by room/motor/controls/view. Nothing enters Campaign
  records. Existing four Flow exercises and their room IDs remain.
- Six licensed contact clips cover jump, ordinary/hard landing, crumble warning,
  collapse and menu taps. 46,327 source OGG bytes total; short mono decode-on-load.
  Original files, license copies and role/gain table: THIRD_PARTY_ASSETS.md.
- Loop changes now change the clip and spatial mode, missing cues stop old audio,
  profile replacement stops loops, and missing cues publish only once per start.
- Routine project setup preserves authored Ancient Abyss route/biome/landmarks
  instead of recreating them. Explicit Recovery remains available. Build setup
  now uses the validator's single version constant.

## Evidence

- Source checks: Logs/production097-source-validation-final.log.
- EditMode: 236/236, Logs/production097-editmode-final.xml. Includes smooth
  bounded camera at 30/60/120 Hz, manual takeover/reset, audio asset checks,
  loop lifecycle and serialized authored-content preservation.
- Initial full PlayMode: 50/50, Logs/production097-playmode.xml. Final rerun
  after the preservation fix: Logs/production097-playmode-final.xml (see handoff).
- All five training rooms, four existing mastery route tests, both motors ×
  both views on the new course, independent session PBs, real touch/Editor
  control paths, retry and motor non-interference are covered.
- Render comparisons: Logs/Production097QA/landing-*.png, plus five room views
  in Logs/MovementMasteryQA. Actual gameplay FOV at 1560×720. Inspected route
  edges/landing visibility, unobstructed view and truthful broad supports. These
  remain functional training visuals; no claim of finished Flow Lab art.
- First focused failure was a fixture comparing pre-retry velocity with reset
  velocity. Corrected measurement starts after retry; 5/5 focused rerun passed.
- Player motor/profile, Input, touch UI, save, Campaign module and landmark bytes
  match the inherited checkpoint. Test-generated prefab rewrites were reverted
  to that checkpoint before validating the new preservation guard.
- adb checked once: no attached devices. No installation, hands-on movement,
  sound audition, physical FPS/thermal testing or approval in this run.

## S23 decision

1. Verify 0.9.7; Home → FLOW LAB → LANDING PRECISION. Do three runs in each VIEW
   with the same MOTOR. Can you see the near edge and lower landing without
   guessing? Does optional framing feel helpful, too weak or intrusive?
2. While descending, drag right vertically. Check immediate manual takeover and
   no snap; test short Tap Jump versus drag release. Try Classic and both
   landscape orientations. Thumb movement should remain in the inner regions.
3. Cycle ROOM through air-strafe, bhop, surf and short flow. Compare MOTOR A/B
   without changing VIEW. Retry/fall must reset cleanly; no view is promoted.
4. Check Campaign 001/002/003, Bronze Next/replay and a retained save. Listen to
   jump/landing, hard landing, crumble warning/collapse and menu taps, with audio
   on/off. Judge repetition, harshness and loudness on speaker/headphones.
5. Observe sustained 60 FPS, heat, loading/resume and safe areas.

## Continue

Movement promotion waits for the specific camera decision. Independent production
is authorized. Sky City direction has recorded physical acceptance; Mountain and
Ancient Abyss latest clearance candidates remain open, not Gold. Preserve them
while gathering route/rank/visual approval. Campaign is still 001 → 002 → 003;
Spiral stays separate; 004–008 remain unbuilt.

Next coherent independent slice: finish authored game-event audio (Restore,
Boost, completion/PB and ambience) with actual listening review, then produce
one complete fourth Campaign world using the protected authored-content boundary.
Do not promote the movement candidate or inflate content count with blockouts.

APK build succeeded: 0 errors / 1 existing legacy-icon warning. Exact artifact:
Builds/Android/RYDERS-ROAD-0.9.7-landing-view-evaluation-dev.apk.
Size/hash/manifest: Logs/production097-package.json. Final PlayMode: 50/50.

