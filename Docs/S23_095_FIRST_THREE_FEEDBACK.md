# S23 0.9.4 feedback follow-up — 2026-09-07

Status: follow-up candidate; the user reports physical testing and unresolved
issues. The screenshots leave the installed version unconfirmed. Acceptance is OPEN.
The prior absence-of-feedback statements are superseded by this record.

User reports a blue screen immediately after tapping the app, overly simple
main-menu UI and a request for centered selection; platforms clipping into blocks,
a Moving platform clipping through other objects, and repeated assets that feel
random despite rotations. More satisfying reward/movement engagement is wanted
later, explicitly deferred. Exact affected module/landmarks have been requested.

Current scope: native startup/first managed frame presentation, centered selection,
measured Moving swept-volume clearance, block/scenery intersections and deliberate
local asset composition. Preserve movement, Campaign progression, accepted 001,
hidden embodiment and the separate Flow Lab audit. No 004–008.

Initial findings: Unity splash is disabled, native Android splash texture is empty,
and only a blue startup color is configured. Runtime LoadingPresentation is
installed before scenes but initially invisible until the scene-load event.
Thus existing menu-to-level video tests do not cover native cold start.

A new real-scene Moving swept-volume test samples both 002 paths against Ground
colliders, excluding the Moving platform itself. This supplements the previous
boarding, endpoint exits and live carry tests, which did not prove full clearance.

## Screenshot clarification and build identity

User supplied three physical screenshots from CrossDevice/JesaiaS23, timestamped
2026-09-07 16:24:28, 16:24:39 and 16:24:52. The two earlier images identify Level 003
clipping. The image called the city shows prototype navy columns, a plain sky and
the 38-second Diamond threshold; it does not match the authored 0.9.4 Mountain
World captures or the packaged Mountain biome. Installed menu version has been
requested. Treat build identity as unconfirmed, not as proof of a runtime fallback
or physical acceptance of the current Mountain content. Preserve the approved
Sky City until the mismatch is resolved; do not blindly rebuild it again.

## Reproduced causes and bounded corrections

Real-scene swept-volume tests found the 003 ferry intersecting its boarding dock
and both bypass blocks. Ferry's near endpoint moves from z33 to z34, leaving a
visible gap from the unchanged dock. The bypass moves to a three-landing side line outside the ferry sweep; matching
Flow Shards follow the first two stable blocks. Four nearby architectural placements
move clear of landing blocks: Arrival View Arch, both Crossing Bridgeheads and
Sanctum Threshold Arch. Shared source assets/collision remain intact.

The same audit found the current 002 upper lift hitting its optional lift-cut
block and footing. That optional block and shard move from x10 to x12.2. Required
route and Moving motion remain unchanged. Side ledges/pavilion pieces at arrival,
garden and summit move outside block footprints; a decorative garden base crossing
the approach is removed. Repeated per-court trees are removed; hand-placed trees
remain. No collision is disabled to conceal a visual intersection.

Visible block bodies extend beneath their thin support colliders. The clearance
regression therefore also tests their actual renderer bounds against the Moving
sweep. Raising only the bypass collider was rejected during verification; the
final side line clears the visual body as well. Revised optional jumps pass with
ordinary/edge takeoffs in the actual unchanged motor. This proves reachability,
not optimal human timing or calibrated shortcut savings.

Module 002 content version becomes 3; 003 becomes 7. IDs, prior save records,
thresholds and authoritative movement profiles remain unchanged. Existing record
metadata is retained; these content revisions are not retrospective rank calibration.

Native startup now uses the supplied loading poster with matching unblurred Unity
splash background, a two-second blank-logo hold (the poster already includes the
approved brand), and an Android splash image. Runtime LoadingPresentation begins
visible before Bootstrap's load event and releases through the existing bounded
scene readiness/recovery path. The native OS launch period still needs S23 cold-start
verification; Editor tests cannot certify it. Unity documents blank-logo entries
as background-only duration: https://docs.unity3d.com/6000.0/Documentation/Manual/class-PlayerSettingsSplashScreen.html

Main menu uses a centered action row, dominant Campaign action and actual next-world
context. Campaign and Settings panels are centered within the safe area. Locks,
Continue, replay and Next remain unchanged. This is a restrained frontend pass;
no new economy/reward loop or fake progression was added.

## Current validation plan

Run source validation, full EditMode and PlayMode suites, inspect final menu and
world captures, build Android and verify manifest/native poster/module content.
New checks cover complete Moving sweeps, static block body intersections, revised
optional-line/dock reachability, native poster configuration and first managed
poster visibility/release. Physical cold launch, both landscapes, sustained FPS,
Moving boarding comfort and final visual acceptance remain pending.

## Final source and test evidence

Source validation and whitespace checks pass. Full EditMode: 223/223; full PlayMode:
29/29. Results: Logs/phase095-editmode-results.xml and
Logs/phase095-playmode-final-results.xml. Final captures are copied into
Logs/Phase095VisualQA/mountain-world and /ancient-abyss; centered UI captures are
01-main-menu.png and 02-campaign.png in the same QA folder.

The first Android attempt reported Succeeded but included a real native splash
export error (texture not accessible). It was rejected during package verification.
The startup copy now imports as readable/uncompressed; all three Loading092Tests
pass again in Logs/phase095-native-splash-results.xml. Final build evidence must
use phase095-android-build-final.log, not the rejected initial build log.

## Verified final Android handoff

APK: Builds/Android/RYDERS-ROAD-0.9.5-startup-course-clearance-dev.apk
Build succeeded in 00:02:15.901 with ZERO errors and one pre-existing legacy-icon
warning. Size: 184,361,505 bytes; ARM64 only.
Package: com.rydersblockstudio.rydersblock. Version: 0.9.5-startup-course-clearance.
SHA-256: b078c1a32e8c4192df8b9f40d6d65dd4e4ccf8fa928c266945f7b834cca0c865

Manifest, Mountain content and the new third 003 bypass ID are verified inside
the final APK. Packaged video matches the 3,705,679-byte source exactly.
res/drawable/unity_static_splash.png exists at 1280x720; it was extracted and
visually inspected as Logs/Phase095VisualQA/apk-native-startup.png. Evidence:
Logs/phase095-package-verification.json, phase095-apk-badging.txt,
phase095-apk-manifest.txt and phase095-android-build-final.log.

No physical testing of this new 0.9.5 APK was performed by the agent. Install this
exact candidate before judging whether the prototype-map screenshot still applies.
Check cold launch, centered UI, 003 ferry/side bypass and moved architecture,
002 upper lift, ordinary completions, both landscapes and sustained performance.
The unresolved installed-version question and final art/feel acceptance remain
physical decisions; no Gold certification is claimed.
