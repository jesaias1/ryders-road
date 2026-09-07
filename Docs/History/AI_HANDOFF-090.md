# RYDER'S ROAD — authoritative handoff

Updated 2026-09-06. Build `0.9.0-astra-campaign-production`.
Unity `6000.5.6f1`; Android package `com.rydersblockstudio.rydersblock`.

## Production status

Astra takeover delivered an audit and a verified development milestone at
Phase B (Module 003). **No level was newly certified Gold-standard complete.**
The eight-module campaign is not implemented. Do not begin broad production
of 001/002/004–008 or Spiral while Module 003's quality gate remains open.

Current Campaign: `module.001.first-steps` → `module.002.moving-parts` →
`module.003.flow-error`. Any valid completion is at least Bronze and unlocks
the next normal Campaign module. `module.004.the-spiral` remains separate.
The final Campaign module returns to module selection. New-world mini-specs,
reserved IDs, KEEP/IMPROVE/REBUILD/DELETE audit and remaining work are in
[ASTRA_PRODUCTION.md](ASTRA_PRODUCTION.md).

## Changes in this milestone

- Actual results Next action uses explicit Campaign order, replacing sorting
  every loaded Resources module (which could enter Spiral/labs).
- Results prioritize rank, time, actual saved PB and next-rank gap. Score and
  reward records remain compatible but no longer clutter the results panel.
  Development results identify practice and do not claim a new official PB.
- Android setup and Bootstrap allow landscape-left and landscape-right only.
  Enable both landscape flags before disabling portrait: Unity otherwise
  preserves a fallback portrait orientation when no permitted direction exists.
- Moving endpoint dwell is deterministic for Linear and SmoothStep easing,
  driven by authored pause time; evaluation no longer allocates a segment array
  each fixed step. Reset and local/world path contracts are retained.
- Generated placeholder tones removed. `GameplayAudioProfile` is an empty,
  ready-to-author Resources asset. Existing `MovementFeedback` callers emit
  `CueRequested` and play only supplied clips. No suitable production audio
  was found; gameplay is intentionally silent until assets are supplied.
- Module 003 retains its 0.8.1 route/content version 5, three Restores, diagonal
  ferry, three one-second Crumbles, Boost/8×8 m destination and three mastery
  cuts. Its textured foundations are narrower and 9 m lower, separating their
  non-colliding shelves from the playable lane. Five-view inspection rejected
  a low-poly stone replacement as blockout art; that experiment was discarded.
  This is a trust/readability correction, **not final Gold-standard art**.
- No movement solver/profile, manual pitch/yaw architecture, Tap Jump,
  canonical gameplay colliders, hidden-arm infrastructure, stable IDs or V4
  save meaning was changed. Existing sky/depth, shared VFX pooling and approved
  branding remain. No new third-party asset pack was imported.

## Verification

- Source validator passed: `Logs/phase090-source-validation.log`.
- EditMode: **208/208**, `Logs/phase090-editmode-final-results.xml`.
- PlayMode: **9/9**, `Logs/phase090-playmode-final-results.xml`.
- Includes real motor launch via the actual Module 003 Boost trigger at slow,
  nominal and full-run approach speeds, landing on the intended collider with
  margin. Existing tests cover movement/camera, roles, required surfaces,
  Crumble, Restore, loading all current levels, save/progression and hidden arms.
- Five final views: `Logs/Phase090VisualQA`. Renderability passes are not art
  approval. No physical device was queried, installed or played during this run.
- Android ARM64 IL2CPP development build succeeded: **0 errors, 1 legacy-icon
  warning**, `Logs/phase090-android-build-final.log`.
- APK: `Builds/Android/RYDERS-ROAD-0.9.0-astra-campaign-production-dev.apk`,
  **174,370,678 bytes**; SHA-256
  `AF314E4298D9F46DD1799FAA83F995E1E8C8D09002F051394BD9450F12A12AF7`.
- APK metadata verified with SDK aapt: package unchanged, version
  `0.9.0-astra-campaign-production`, ARM64, min SDK 26 / target SDK 36.
  This verifies the built artifact, not physical installation or gameplay.

## Next work and physical QA

Continue Module 003 only. The remaining art issue is its repeated block/arch
language and incomplete integration of route and world; it does not yet achieve the requested
five unmistakable authored places. Improve those compositions without restoring
a floor, grassy false landings, helper geometry or broad route VFX. Validate
all five gameplay views and an entire human Bronze run before moving to 001.

Samsung S23 still needs: complete route and Restore rhythm, Moving boarding,
Boost entry positions/aim/speed tolerance, Crumble feel, Classic/flow controls,
manual vertical look, both landscape safe areas, immersive fullscreen and
resume, fresh/existing saves, Bronze unlock/Next/Retry, sustained 60 FPS and
thermals. Rank thresholds remain uncalibrated. Audio assets and actual haptic
output remain unfinished. The supplied 8-level commercial-game objective is
still active production work, not a completed release.

Earlier contradictory handoff material is retained in
[History/AI_HANDOFF-pre-090.md](History/AI_HANDOFF-pre-090.md); phase-specific
historical documents remain available. This file and current runtime evidence
supersede their old camera, floor, arms and completion claims.
