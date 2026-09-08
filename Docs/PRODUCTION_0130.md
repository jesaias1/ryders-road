# 0.13.0 — gameplay-quality correction

2026-09-09. This continues the delivered vertical slice after the user's S23
rejection. It is a candidate for physical testing, not final visual quality,
fun or Diamond acceptance. See ADR 0026 and Quality130Evidence.

## Implemented route changes

Windward v3 replaces the repeated eleven-pad arc with six galleries and two
crumbling fins. Different widths/depths, a pressure pair and a three-landing
narrow direct chord give the standard and faster lines different rhythms.
The east Restore is a wider catch court; diagonal ascent remains. Twenty-four
static supports include three skill supports, with two additional crumbling fins.
Existing wind instrument, sails and foundations remain and follow the route.

Foundry v3 separates opening run-ups and landings. Wider optional transfer
brackets, shallow crown beams and the existing ferry/cooling sequence provide
contrasting beats. Only the opening is expanded: downstream relative spacing is
preserved. Continuous input can cut from the first cooling fin to the safe court;
the ordinary two-fin path still works. Environment/collision geometry follows the
revised route. No motor, input, camera or accepted movement tuning changed.

The first three road layouts remain intact. Existing UI, loading, branding,
landscape support, trials, campaign progression and five-road ending are retained.
No new ramp/surf system or Worlds 006–008 were introduced.

## Presentation evidence

The Abyss Restore shrine was being flattened into an ambiguous slab. It is now a
flat corner signal with a short activation pulse and no collision/Restore authority.
Boost keeps its authored mechanical art, with opaque arrows, upright bars and
whole-object pulse removed. Small activation particles remain. The large landing
court uses a complete deck instead of a 6-by-6 field of repeated pad meshes.
Foundry/Windward use complete beveled gallery skins and shared materials. Three
existing skies expose stronger cloud coverage using the same noise sample count.

[Presentation captures](Quality130QA/presentation.jpg) and
[route captures](Quality130QA/routes.jpg) are final Editor renders, not S23 captures.
They show cleanup and route changes, but environment cohesion, architectural
detail, depth and lighting remain below the aspirational concepts. No additional
lights or volumetric renderer was added. Device cost remains unmeasured.

## Ranking and persistence

| Road | Content / threshold version | Silver | Gold | Diamond |
| --- | --- | --- | --- | --- |
| Foundry | 3 / 2 | 38 s | 26 s | 20 s |
| Windward | 3 / 2 | 35 s | 25 s | 19 s |

These are Uncalibrated candidate targets, identified as provisional in the HUD and
results. Any valid finish still earns at least Bronze and permits progression.
The first three roads' targets are unchanged; their first-try Diamond feedback is
still an open calibration issue, not resolved by these two-road changes.

V4 adds historicalRecords and versionedBests. Current PBs require matching module,
content, movement and threshold versions. Old aggregate records/unlocks/rewards
remain; their pre-update PB snapshot is retained even if a new faster time replaces
the aggregate. Legacy latest-run version metadata cannot reliably identify its PB,
so it is never guessed into a current bucket. Old unlocked roads may initially
show current-route PB unavailable while history/progression is retained.

## Verification

- 246/246 EditMode and 92/92 PlayMode tests passed in Unity 6000.5.6f1.
- Source foundation validator and Git LFS hydration passed.
- Real-motor jump, gap, ferry carry, crumble/reset, Restore, campaign/Bronze,
  editor/manual controls, responsive UI and save regression checks remain passing.
- New tests cover historical JSON/reload, slower/faster/invalid versioned results,
  three continuous routes through Patch, and corrected presentation hierarchy.
- Deterministic continuous simulations: Windward normal 24.216 s, direct line
  16.317 s; Foundry optional line 16.550 s. These use immediate heading changes,
  scripted takeoffs and a 60 Hz simulation. Normal Windward includes brief landing
  pauses. They prove traversal and relative opportunity, not human rank distributions
  or thumb comfort. Foundry ordinary route has individual links/ferry coverage,
  not a new whole-route timing benchmark.
- Full XML/logs: Logs/quality130-final-edit.*, quality130-final-play.*.
  Structured retained evidence: Quality130QA/validation.json.

## Physical test focus

On S23, compare ordinary and optional lines for sustained momentum, varied timing,
clear next landings and fair failure/recovery. Check Windward's pressure pair and
chord, Foundry transfer/cooling choices, Restore recognition, Boost activation and
court material quality. Test both landscape orientations, manual Classic control,
Easy camera, repeated Retry/Restore, preserved unlocks and new current PBs. Record
several ordinary/clean/fast runs before accepting rank targets. Assess sustained
frame rate, thermal behavior and effect readability on the actual phone.

No device was connected or tested by the agent. Stop after APK/Git delivery for
physical feedback; do not automatically proceed to another world or milestone.

## Delivered artifact

Builds/Android/RYDERS-ROAD-0.13.0-gameplay-quality-dev.apk — 178,321,994 bytes.
SHA-256: `BC07274E14AD2B8B8AACC96C249A3F5D46AE51955E3188FCEA29B127629A98C1`.
Android development IL2CPP ARM64, package com.rydersblockstudio.rydersblock,
versionCode 1, min SDK 26 / target 36. Build succeeded in 152.098 seconds with
zero errors and one legacy icon warning. APK ZIP CRC and aapt metadata passed.
Previous 0.12.0 APK is retained. APKs remain local build artifacts, not Git content.
Implementation, source assets and retained evidence are committed to private origin/main.
