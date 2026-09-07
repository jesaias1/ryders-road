# Current authority — 0.9.6 Movement Mastery evaluation slice

Updated 2026-09-08. The user explicitly authorized this focused implementation,
then authorized the attached S23. The APK is installed and cold-launched to the
main menu. **STOP for physical movement feedback. Do not start another phase.**

Home → FLOW LAB opens air-strafe, bhop, one surf ramp and a short timed challenge.
Candidate movement is opt-in within the existing motor; A/B switches to production
and resets the room. Two-thumb Flow controls are session-only; Classic/Editor
remain available. Campaign movement asset is byte-identical to the pre-change
snapshot. 001–003/Spiral IDs, V4 saves, ranks, Restore/Patch and progression remain.
No 004–008, new world/character pass, abilities or persistent training records.

[Implementation, parameters, measured baseline and S23 checklist](MOVEMENT_MASTERY_SLICE.md).
[Isolation decision](Decisions/0018-isolated-movement-mastery-slice.md).
Final source validation passes, **223/223 EditMode + 48/48 PlayMode**. Initial full
PlayMode was 47/48 from an existing Editor video timeout; unchanged rerun passed.
Android build: **0 errors, 1 existing legacy-icon warning**. Verified ARM64 APK:
`Builds/Android/RYDERS-ROAD-0.9.6-movement-mastery-slice-dev.apk` (184,424,345 bytes).
Full hash/package evidence: `Logs/mastery-package-verification.json`.

S23 SM-S911B: installed via adb -r, version verified, cold launch/main menu captured.
No hands-on movement or physical FPS/thermal evaluation was performed. This is
not Gold or production freeze. Single downhill surf only; banked surfaces, seams,
corner behavior and human mastery/comfort remain future feedback-driven work.

## Previous 0.9.5 checkpoint — historical below

# Current authority — 0.9.5 startup and course clearance

Updated 2026-09-07. User supplied S23 feedback and three screenshots. Level 003
clipping was reproduced; the prototype-map screenshot differs from current
Mountain content and the installed phone version is still unconfirmed.

Final APK: `Builds/Android/RYDERS-ROAD-0.9.5-startup-course-clearance-dev.apk`.
184,361,505 bytes; verified native poster, module content and video; build 0 errors.

Current source: Module 001 v2 preserved; 002 v3 / 003 v7 include measured clearance
corrections. 003's bypass is now a three-landing side line, outside the ferry's
collider AND visible-body sweep. Native and first-managed-frame startup use the
approved poster. Frontend is centered with actual next-world context.
Motor, movement profiles, sequential Campaign, V4 compatibility and rank thresholds
are preserved. No new rewards, embodiment, Flow Lab runtime or 004–008.

Source checks pass. Final tests: 223/223 EditMode and 29/29 PlayMode. Android package
build/verification is recorded in [the current report](S23_095_FIRST_THREE_FEEDBACK.md).
Final Unity views: Logs/Phase095VisualQA. These are not S23 captures or Gold certification.

[Movement audit / Flow Lab design](MOVEMENT_MASTERY_FOUNDATION.md) is complete as
a design deliverable. Next physical gate: exact 0.9.5 cold launch, menu/selection,
Moving paths, optional lines, standard completions and both landscape orientations.
Use existing Easy Mode to assess the required two-thumb mapping; do not change the
Campaign default or global momentum before focused physical acceptance.

## Prior checkpoints below — historical, not current authority

> 2026-09-07 update: user has now physically tested 0.9.4 and reported startup,
> UI, clipping and repeated-placement issues. Acceptance remains OPEN; see
> [current physical feedback](S23_095_FIRST_THREE_FEEDBACK.md). Earlier statements
> below that no feedback was supplied describe the preceding audit checkpoint.

# RYDER'S ROAD — current handoff

Current stable candidate: `0.9.4-module002-mountain-world-gold`.
Module 001's approved visual work is preserved. Module 002 v2 is technically
complete and awaiting physical S23 acceptance; Module 003 v6 fixes remain present
with its physical Gold gate open. No new physical results have been supplied.
Do not begin 004–008 or redesign accepted 001.

See [Mountain production/build evidence](S23_094_MOUNTAIN_WORLD.md) and
[Movement Mastery audit and Flow Lab design](MOVEMENT_MASTERY_FOUNDATION.md).
The latest milestone adds audit/design only, with no global motor/profile or
control-default changes. Easy already provides the requested left yaw/right pitch
mapping; Classic is still the default. Evaluate the existing Easy controls on S23
before promoting a new mastery baseline. No Flow Lab runtime is claimed complete.

Validation: source passes; 222/222 EditMode, 25/25 PlayMode, and 4/4 final mountain
capture cases pass. Verified ARM64 APK is 181,852,683 bytes; package/content/video
checks pass. Full hash and paths are in the mountain report. Physical 60 FPS,
phone controls, route/rank calibration and Gold acceptance remain open.

## Historical record below — superseded where it conflicts

# RYDER'S ROAD — authoritative handoff

Updated 2026-09-06. Current phase: `0.9.3-module001-sky-city-gold-production-pass`.

The user explicitly advanced production to Module 001 Sky City. Its rebuilt
arrival terrace, broken bridge, suspended garden, sun gateway, Patch overlook and
layered city envelope are the current candidate. Module content version 2 widens
landings while retaining route centers, stable IDs and the unmodified motor.

Report: [S23_093_SKY_CITY.md](S23_093_SKY_CITY.md).
Decision: [ADR 0015](Decisions/0015-authored-sky-city-production.md).
Representative before/after views: `Logs/Phase093VisualQA`.
Source validation, 217/217 EditMode and 21/21 PlayMode tests pass.
APK: `Builds/Android/RYDERS-ROAD-0.9.3-module001-sky-city-gold-production-pass-dev.apk`.
Build succeeded: 0 errors / 1 legacy-icon warning; 180,247,689 bytes.
Manifest, city inclusion and loading-video bytes verified. Full evidence is in
the production report.
Explicit authoring entrypoint: `SkyCityProductionAuthoring.Build`.
Startup setup preserves authored v2 content instead of reverting to blockout.

No physical S23 was connected. Phone art acceptance, jump feel, rank calibration,
thermal/60 FPS behavior and retained fullscreen/loading flows need user QA.
Do not claim physical Gold certification. Module 002/003/Spiral production and
hidden arms remain outside this completed phase's scope.

# Historical 0.9.2 handoff

Updated 2026-09-06. Current candidate: `0.9.2-s23-loading-fullscreen-hotfix`.

## STOP — physical S23 frontend QA

The user rejected 0.9.1: menu side bars and mode selection stuck on loading.
This hotfix fixes persistent scene ownership, explicit readiness, bounded recovery,
COVER backgrounds, and video preparation/rendering/fallback. Campaign and Spiral
independently reach gameplay in real-button tests; Patch/Retry/Next also pass.
213 EditMode and 19 PlayMode tests pass. Source validation passes.

Authoritative report: [S23_092_HOTFIX.md](S23_092_HOTFIX.md).
Architecture: [ADR 0014](Decisions/0014-persistent-scene-transitions.md).
APK: `Builds/Android/RYDERS-ROAD-0.9.2-s23-loading-fullscreen-hotfix-dev.apk`.
Build succeeded with 0 errors / 1 legacy-icon warning, 179,015,667 bytes; exact video bytes verified in the APK. Build/package evidence is in the hotfix report. Android video/immersive acceptance
is still pending; no phone was connected. Do not claim physical success or Gold.
Do not continue Campaign/world production before user physical QA feedback.
Module 003 remains v6; V4 saves, stable IDs, Bronze continuation, movement and
Classic/manual camera are unchanged.

## Historical 0.9.1 handoff (superseded for frontend/loading)

Updated 2026-09-06. Build `0.9.1-s23-truth-and-flow`.
Unity `6000.5.6f1`; Android package `com.rydersblockstudio.rydersblock`.

## STOP — physical S23 QA gate

The user physically tested 0.9.0 and rejected Module 003 collision truth and
product completeness. This continuation produced a new QA candidate, not Gold
certification. **Stop for physical S23 QA. Do not start 001/002/004–008 art.**
The normal Campaign still contains 001 → 002 → 003; Spiral stays separate.
Bronze completion remains sufficient for normal progression. Future modules
remain design-only in `Docs/ASTRA_PRODUCTION.md`.

## Current implementation

Detailed changes and every corrected near surface:
[ S23_091_CONTINUATION.md ](S23_091_CONTINUATION.md).
Authoritative collision decision:
[ADR 0013](Decisions/0013-explicit-playable-architecture.md).

- Blanket generation/runtime collider stripping caused the architecture failure.
  37 near structural/terrain meshes now carry exact static mesh collision and an
  explicit AuthoredSurface/source contract. Four distant world entries remain
  scenery, moved safely away from the play corridor. Four foliage meshes remain
  permeable. No giant approximate invisible support boxes were introduced.
- All three Restore shrine visuals are now inside their actual support footprint.
  Long-platform art fits authored collider bounds; the tiled 8×8 Boost landing
  art meets its collision plane. Gameplay BoxColliders/Boost trajectory unchanged.
- Module 003 content version 6: three repeated arches become broken ruins/wall,
  six under-route buttresses connect the route to larger mass, and the energy
  relay moves off the Boost destination. Bright sky/depth, vegetation, palette,
  hidden arms, Classic/manual camera and GoldSrc movement remain intact.
- Five optional Flow Shards use stable IDs and attempt-local counts. Restore
  keeps collection, Retry resets it. No economy, rank or Bronze dependency.
- Speed/Boost streaks, existing landing/Crumble/Restore/Patch particle events,
  preference-aware Android one-shot haptics and complete one-shot/loop audio
  hooks are implemented. No additional movement powerup was needed.
- **Audio assets remain missing.** Project/local game assets and 19 archives
  contained no suitable gameplay sound library. Exact role list is in the
  continuation report. No placeholder tones or unrelated music were added.
- Supplied loading film: 720p/24 fps, 11 s, 1,486,737-byte visual derivative,
  matching poster, aspect fit, first-frame handoff, loop/error fallback and short
  reveal. Main menu uses supplied artwork; Campaign uses real journey cards and
  locks with future pagination. Spiral remains separate. HUD/results prioritize
  timer/target/PB, rank/time/gap and quick Retry/Next. Results stop active controls
  and suppress duplicate completion text. Normal run menu includes Restore.
- Release guards omit QA practice, build-version text and development toolbar.
  Unity splash disabled. Development APK still carries Unity development status.
- V4 saves and public movement/input/camera contracts remain compatible. Existing
  PBs retain historical version metadata; no migration or record deletion.

## Verification and artifact

- Source validation passed: `Logs/phase091-source-validation.log`.
- EditMode **211/211**: `Logs/phase091-editmode-final-results.xml`.
- PlayMode **11/11**: `Logs/phase091-playmode-final-results.xml`.
- Tests cover all 37 architectural colliders after batching, supporting-ray
  samples, 34 route/mechanic/mastery supports, long visual bounds, all Restore
  shrine footprints, actual shard trigger, duplicate pickup protection, haptic
  role bounds, actual Boost trigger/motor at three approach speeds, preserved
  Classic/manual camera and hidden arms, prior modules, saves and progression.
- Five gameplay views plus menu/Campaign/loading/HUD/results were rendered and
  inspected: `Logs/Phase091VisualQA`. Captures are not human route completion.
- Android ARM64 IL2CPP development build succeeded: **0 errors, 1 warning**.
  Log: `Logs/phase091-android-build.log`.
- APK: `Builds/Android/RYDERS-ROAD-0.9.1-s23-truth-and-flow-dev.apk`
- Size: **176,751,077 bytes**.
- SHA-256: `340D1EE2A1F81CB9E1ECAE461656D38A1B544C025220EF88705A6A3F7CB85AF3`.
- aapt confirms unchanged package, version `0.9.1-s23-truth-and-flow`, ARM64,
  min SDK 26 / target 36, landscape orientation `0xb`, VIBRATE permission.
- Content budget: 41 near renderers, 37 static mesh colliders, five pickups,
  maximum eight pooled particle emitters and one loading-only 720p decoder.
  No physical FPS/thermal measurements were performed.

## Required user S23 acceptance

Complete Bronze plus obvious optional ruin/grass landings; test recovery from
lower architecture, Moving boarding/exit/wait rhythm, Boost entry angles and
landing feel, one-second Crumble escape, shard Restore/Retry behavior, Classic
and Easy input, manual pitch, both landscape safe areas, native haptic strength
and off setting, loading cold start/movie/loop/error/resume, fresh/existing save
progression, Bronze Next and results Retry, sustained 60 FPS and thermals.
Rank times remain uncalibrated. Production sound assets remain a declared gap.

No physical device installation or gameplay was performed by the agent.
Resume campaign art production only after the user's physical acceptance.
Previous handoff: [History/AI_HANDOFF-090.md](History/AI_HANDOFF-090.md).

