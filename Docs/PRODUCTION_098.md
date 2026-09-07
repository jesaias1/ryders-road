# 0.9.8 — Solar Foundry

2026-09-08. Baseline: `db7f01a` (verified 0.9.7); decision: ADR 0020.

## Implemented

Fourth Campaign road, `module.004.solar-foundry`, content version 1. The standard
route rises from intake pipes to the transfer crane, reactor cooling path and
radiator crown, ending at a Patch Block beside a service house. Three Restore
Points, one moving ferry, two crumble pads and a three-bracket optional bypass
use existing mechanics and the unchanged production motor. Two attempt-local
shards mark the bypass. Higher ranks are optional and provisionally timed.

Original baked industrial art includes a squat intake vessel with elbows, tall
reactor, twin finned cooling towers, transfer crane, large blue solar collectors,
and structures supporting the white gameplay surfaces. Near architecture has
matching collision; far collectors explicitly have none. The explicit authoring
tool creates separate assets; startup does not regenerate them.

Campaign pagination, Continue, unlocks, completion and replay include the fourth
road. Legacy Spiral retains its stable ID and remains separate. V4 save meanings,
historical records, accepted input/camera/motor and the first three worlds remain.
The isolated 0.9.7 Flow Lab comparison and contact audio ship with this build.

## Verification scope

- Focused PlayMode: 4/4 in `Logs/production098-focused-final.xml`.
- Actual production-motor jump simulations cover standard route transitions,
  ferry boarding/exiting, crumble transitions and the optional brackets. These
  are controlled individual traversals, not a continuous human speedrun.
- Nine support ray samples per static platform verify top clearance. A full
  ferry/rider sweep checks 51 positions. Actual platform carry, Restore recovery,
  shard retention/reset, crumble collapse/reset, completion and Retry are tested.
- Memory-backed V4 progression tests verify fresh-save locking, old Bronze records
  unlocking 004, unchanged historical records after queries, Continue, slow Bronze
  completion, no next road/Spiral transition, and replay through the real frontend.
  The fixture does not modify the user's save.
- Content validation checks stable IDs, explicit collision, bounded geometry cost
  and byte preservation through routine project setup. Full-suite results and APK
  metadata are recorded below after validation.
- Five gameplay views at 1560×720 plus the fourth-road menu are captured in
  `Logs/Production098QA/` and visually inspected. Revisions replaced repetitive
  vessels with distinct intake/reactor/radiator silhouettes and fixed the solar
  reflector face winding. Standard surfaces, alternate line, crane clearance and
  the crown destination remain readable at actual gameplay FOV.
- A marginal optional landing was moved 0.5 m closer after its traversal failed;
  the focused rerun passes. Earlier test-only assumptions about a Rigidbody and
  launch position were corrected to match the existing motor/platform contract.

## Physical checks still open

No phone is connected. No install, human movement run, audio audition, sustained
FPS, thermal, safe-area or landscape-orientation approval was performed here.
This is implemented, automatically validated and visually reviewed content;
it is not physically approved, Gold, rank-calibrated or production-frozen.

S23: retain your save; finish/Continue from 003 and open 004 on MORE ROADS. Ride
the ferry, fall after each Restore, try both crumble pads and the optional shard
line, then complete and replay. Check ordinary completion without mastery,
landscape-left/right thumb comfort, audible feedback and sustained frame pacing.
Evaluate the separate Flow Lab VIEW choice only when convenient; Campaign still
uses the accepted movement/input model.

Next coherent work: finish event audio beyond contact foley and review its mix;
then produce world 005 as a complete route and place. Worlds 005–008 remain
unbuilt. Existing 002/003 and new 004 still need physical route/rank/art feedback.

## Final automated evidence

- Source validation passed: `Logs/production098-source-validation.log`.
- EditMode: **239/239**, `Logs/production098-editmode-final.xml`.
- PlayMode: **54/54**, `Logs/production098-playmode.xml`.
- World environment budget: 17,244 triangles and 41 renderers before runtime
  batching; this excludes shared gameplay assets and does not certify 60 FPS.
- The first full EditMode run found two obsolete three-road end/order assertions;
  both now cover the four-road sequence. The full rerun passes.
- Protected Player/Input/Camera/SaveSystem/Respawn/Checkpoint sources and all three
  existing Campaign module assets match `db7f01a` by Git comparison.

Android development build succeeded with 0 errors and 1 existing legacy-icon
warning (`Logs/production098-build.log`). Exact artifact:
`Builds/Android/RYDERS-ROAD-0.9.8-solar-foundry-dev.apk`, 184,803,741 bytes.
SHA256: `BAAA38AFFCDAA38EF154D512380F7B8EE8B3CDC8AE9D5C4F67F26CB22A469C03`.
Manifest confirms unchanged `com.rydersblockstudio.rydersblock`, version
`0.9.8-solar-foundry`, ARM64, min SDK 26 and target SDK 36. Package metadata:
`Logs/production098-package.json`. Post-build source validation also passes in
`Logs/production098-source-validation-final.log`.
