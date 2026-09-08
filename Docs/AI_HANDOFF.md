# Current authority — 0.11.0 worlds and Windward Observatory

Updated 2026-09-08. Milestone delivered for physical Samsung S23 feedback.
**STOP after delivery.** ADR 0023 and PRODUCTION_0110.md are current authority.
The user's new brief superseded 0.10.0's movement-only production stop while
explicitly keeping the accepted Campaign foundation.

## Accepted foundation

User physically tested 0.9.9 and provisionally prefers accepted Campaign controls
for predictability and natural looking. Normal Campaign retains accepted motor
compatibility 1 and existing saved control selection. No motor/input/camera source
or tuning asset changes. 0.10.0 candidate, previous Flow and landing-view trials
remain recoverable and isolated; no global promotion, new movement buttons,
artificial strafe reward, physical approval or production freeze.
Previous 0.10.0 details remain in PRODUCTION_0100.md and ADR 0022.

## Shipped work

- Minimal loading: supplied Jesaias emblem traced to
  `Assets/Branding/Source/Jesaias_Emblem.svg`; transparent runtime PNG and dark
  native startup derivative. Tools/Trace-Jesaias.py regenerates from the SVG.
  Gentle unscaled opacity pulse; no video decode or intentional logo hold.
  Existing SceneTransitionHost readiness, timeouts and menu recovery remain.
- Campaign sky source-wrap seam removed with a continuous directional sky shader.
  Five authored palette/light/fog profiles: clear Sky City, soft alpine Mountain,
  deep cool Ancient Abyss, amber Solar Foundry and lavender Windward. Authored
  lighting bypasses the old 003 intensity override; Mountain materials are matte.
  Existing route/collision/ranks remain, including Foundry crane/recovery fixes.
- Complete fifth road **Windward Observatory**, reserved stable ID
  `module.005.foundry-pulse`, content v1. Wind-instrument arc, telescope ascent,
  three Restores, Patch and five-pad optional chord. Serialized original art;
  explicit authoring never runs on startup. Bronze 004 unlocks 005; replay/PB
  use existing V4 contracts. Spiral keeps `module.004.the-spiral` separately.
  Worlds 006–008 remain unbuilt. Fifth-road rank times are provisional.
- Quiet ground-travel footsteps, Restore cues and original offline wind bed.
  Existing licensed contact foley remains. Master volume stays authoritative.
  Dedicated completion/Boost/mechanism audio and final mix remain future work.

## Verification and artifact

243/243 EditMode and 87/87 PlayMode pass. Source and LFS hydration validation pass.
Covers standard/optional fifth-road jumps with accepted motor, Restores, Bronze
unlock/Continue/replay, 20 road/trial-mode loads and save isolation, prior motor/
camera/Editor regressions, Foundry recovery/clearance, loading navigation/failure
and grounded footsteps. Individual real-motor jump fixtures are not human play.
25 player-camera and 20 sky-direction views plus loading were inspected.
Tracked contact sheets and validation.json: Docs/Production110QA.
Full tests/renders/logs: Logs/production110-* and Logs/Production110QA.

ARM64 IL2CPP Android development build succeeded: 0 errors, 1 existing Unity
legacy-icon warning, 2m50s. ZIP CRC and package/ABI checks pass.
Exact APK:
`C:\Users\lin4s\Documents\Riders Block\Builds\Android\RYDERS-ROAD-0.11.0-worlds-and-windward-dev.apk`
182,818,218 bytes. SHA256:
`D2F2A181D36B0F7515FFB31B383997F725003AC6F786D5EA7D2B06477B66BAB7`.
Package ID remains com.rydersblockstudio.rydersblock.

No device was queried, installed, launched or physically played by the agent.
No physical approval or sustained 60 FPS claim. S23 checklist: exact-build cold
launch/resume and both orientations; accepted control feel/free looking; full
sky turns and landing readability; Foundry scenery recovery; 004 → 005 Bronze,
Restore/chord/Retry/PB replay; speaker/headphone mix/master volume/pause; 10–15
minute frame pacing and thermal run. Detailed checklist: PRODUCTION_0110.md.

## Git and next work

Private remote: https://github.com/jesaias1/ryders-road.git, origin/main.
Recoverable pre-milestone commit: 277b438. This handoff ships in the coherent
milestone commit; the final delivery records its exact SHA and push result.
No history rewrite. GitBackups, caches, test logs and APK remain ignored/local.
Fresh checkouts: git lfs pull; git lfs checkout; Tools/Validate-LfsAssets.ps1.

Wait for physical feedback. Address concrete findings, then continue with reserved
006 Broken Meridian. Preserve accepted controls and avoid another global movement
promotion without explicit physical acceptance. No Gold label is assigned.
