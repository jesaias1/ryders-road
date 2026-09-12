# 0.16.0 — Movement-aware Windward benchmark and fatal scenery

2026-09-11. Continues 0.15.0 (`f401a9c22c9f0a3e48b06548be46ae6b5921061e`).
The user's physical feedback says the new air-strafe is substantially better and
asks for geometry that accommodates it. Shared movement compatibility remains 5;
movement profile, acceleration, jump arc, air correction, 18 m/s safety cap, music
and whole-right continuous hold/look controls are unchanged. No new physics mode.

## One benchmark world

**Windward Observatory**, existing Campaign World 005, becomes content version 4.
Its wind instrument, north sweep, direct chord, rising diagonal and telescope
finish provide a representative place to compare movement speeds on the same
geometry. Other worlds' route positions are preserved.

- Arrival grows from a 6×4 m pad to a **10×14 m runway**. Offset approach courts
  retain intermediate safe landings but permit skipping every other platform.
- The north sweep uses roughly **9–11 m wide, 8–11 m long** surfaces with offsets
  and a two-platform crumbling pressure sequence. Their collapse window grows
  from 1.2 to 3 seconds to accommodate ordinary travel across the wider surfaces. There is room to carve, land
  off-center and continue instead of immediately reaching another small edge.
- The optional chord crosses the inside of the loop on three **14×9 m** courts.
  It is still the same authored world and motor, with no target assistance.
- The diagonal ascent ends in a **28×14 m recovery terrace**. It absorbs the old
  `m05.ascent.d` pad, reducing the route from 26 to **25 supports including the two
  pressure platforms**. No other stable route/checkpoint IDs are renamed.
- Telescope approach courts are 12 m wide with 11–12 m runways; the finish is
  **16×14 m**. The broad shapes deliberately allow some run-through transitions;
  the open pressure gaps still require jumping. Basic completion needs no skips.
- Narrow dark underside keels/braces replace the old flat secondary slabs. The
  instrument moves lower and away from the chord; the telescope follows the new
  finish. Scenery retains depth and silhouette without offering bright route decks.

## Representative lines and measurement method

**Safe:** arrival → approach.a → approach.b → approach.c → west Restore → all
north galleries/pressure platforms → east Restore → ascent.a → ascent.b →
ascent.c → lens Restore → lens.a → lens.b → lens.c → Patch.

**Flow:** the same opening, then west Restore → chord.1 → chord.2 → chord.3 →
east Restore, rejoining the same ascent. Continuous held hopping uses the broad
opening landings. Intermediate courts provide recovery instead of mandatory skips.

**Expert opportunities:** arrival → approach.b; approach.b → west Restore;
ascent.a → ascent.c; lens.a → lens.c. These four skips use existing safe-route
surfaces. Each was tested with centered and 0.75 m off-center landing aim.

Controlled 17 m/s incoming-state tests measure the following actual controller
flights. They do not grant speed in runtime or prove a complete human run can
earn and retain 17 m/s through every preceding section. The pilot provides explicit
stick input; near-edge aiming deliberately brakes some entries.

| Skip | Flight distance | Landing speed |
| --- | ---: | ---: |
| arrival → approach.b | 9.10–9.12 m | 13.36–13.44 m/s |
| approach.b → west Restore | 7.45–7.53 m | 8.47–8.57 m/s |
| ascent.a → ascent.c | 10.71–10.72 m | 15.11–15.19 m/s |
| lens.a → lens.c | 9.67–9.68 m | 13.50–13.58 m/s |

Held chains use fixed small alternating thumb-expressible input, with no route
lookup: 7.8 m/s incoming state gives **six jumps / 26.25 m in 3.2 s**, ending at
8.21 m/s; 14 m/s gives **five jumps / 41.23 m in 3 s**, ending at 13.75 m/s.
These are continuous authored-geometry segments without per-hop resets. Full
safe/direct route tests start from rest and log every approach, takeoff, landing,
platform dimension and flight distance in Benchmark160QA CSV evidence.

The first skip test exposed insufficient clearance at an elevated recovery edge;
the deck was enlarged, not the motor changed. Broad pads are intentional. Scripted
timings are feasibility evidence only: **no final rank thresholds were calibrated**.

## Fatal scenery contract and audit

WorldGeometryKind separates Traversable, FatalScenery and NonCollidingScenery.
Module route/mechanism colliders remain traversable. Retained biome mesh collision
defaults to fatal unless the object explicitly opts into traversable architecture.
Distant/non-colliding art retains the existing collider-removal path. AuthoredSurface
preserves collision/render source truth; no player-code name checks or giant new
invisible death floor. The existing lower world threshold still handles total misses.

**55 previously unmarked solid world objects, containing 224 mesh colliders**, are
now fatal: Sky City 13/61, Mountain 37/126, Ancient Abyss 5/37. Foundry's five objects
(21 colliders) and Windward's three (8 colliders) were already top-landing fatal;
their side/underside contacts now also fail immediately. Across the five worlds,
**63 solid scenery placements / 253 mesh colliders** are classified. Spiral has
no retained solid biome scenery. These are placement/collider counts, not individual
triangles or every visually combined rock inside a mesh.

Actual tests drop controllers onto five scenery samples in each solid-scenery
world, check all ordinary route decks for false deaths, and verify side contact
even during spawn protection. Fatal contact immediately records the normal fall,
clears input and suspends further motor simulation during the existing 0.08 s
Restore delay. No extra held jump or scenery checkpoint is possible. This reuses
the normal Restore/reset flow and preserves its progression semantics.

The whole-suite check exposed Mountain's garden arch intersecting the valid jump
to its Restore platform. The shrine moved from (4,3,26) to **(18,3,26)** as a side
landmark. Its collision stays fatal; the route itself is unchanged. This is a
scenery-clearance correction, not a second benchmark reauthoring.

## Compatibility and physical handoff

Campaign content versions become Sky City 3, Mountain 4, Ancient Abyss 8, Foundry 4,
Windward 4. The first four bumps identify changed scenery rules; Spiral remains 2.
Movement remains 5, V4 saves/history/unlocks remain, and existing versioned PB
buckets separate old content from new results. All music files/settings are kept.

Play **normal Campaign → Windward Observatory** using the current whole-right
hold/drag controls. Development diagnostics still show build and shared profile.
Check safe completion, held turns, early/late landings, four skip opportunities,
recovery surfaces, fatal side/top contact, Restore, and both landscape directions.
Only physical S23 feedback can decide whether this space fits the current speed;
no max-speed/air-control reduction is justified by the old route spacing alone.

No S23 install, physical playtest, sustained FPS/thermal test or new music listening
is claimed. Stop after delivery for feedback. No Worlds 006–008, multiplayer,
economy, PC port or unrelated frontend redesign.

## Verification status — 2026-09-12

The final full PlayMode suite passed **151/151**, with zero failures
(`Logs/benchmark-release-PlayMode.xml`). Normal Windward safe completion took
41.683 s; the direct chord took 30.733 s. The equivalent development trial used
the same geometry and produced the same timings. A slow ordinary route using
70% input completed in 71.517 s. These are scripted feasibility measurements,
not rank targets or claims of human/device performance.

| Route | Recorded approach speed | Recorded landing speed | Flight distance |
| --- | ---: | ---: | ---: |
| Normal safe, 21 links | 0.635–7.800 m/s | 4.480–9.846 m/s | 3.650–5.432 m |
| Direct, 16 links | 0.635–10.556 m/s | 4.061–10.858 m/s | 3.636–6.128 m |
| Slow, 21 links | 0.224–3.469 m/s | 5.740–6.156 m/s | 2.664–3.620 m |

Slow input is 70% of normal ground input; approach measurements include stop/go
setup and are not constant-speed running. At the broad lens recovery terrace,
the safe pilot walks across the deck before turning toward the next jump.

Foundation source validation and Git LFS hydration checks passed. The final
EditMode suite passed **249/249**, with zero failures
(`Logs/benchmark-release-EditMode-after-license-20260912-142604.xml`). The
license issue recorded in the earlier `benchmark-release-EditMode*.log` files
was resolved by the Unity Hub activation visible at 2026-09-12 14:21 local time.

The Android development APK was built successfully with one known Unity
legacy-icon warning and zero errors:

- Path: `C:\Users\lin4s\Documents\Riders Block\Builds\Android\RYDERS-ROAD-0.16.0-flow-benchmark-fatal-scenery-dev.apk`
- Size: `182,867,615` bytes
- SHA-256: `9BFC0CC2DA87CC0018C2F34A5344BC1BB623F4C57E9FF6D13F6C5C0C08A81825`
- Package: `com.rydersblockstudio.rydersblock`
- Version: `0.16.0-flow-benchmark-fatal-scenery`
- Native code: `arm64-v8a`
- SDK: min `26`, target `36`
- Signature: APK Signature Scheme v2 verified, one signer

The APK zip test passed (`651` entries). No physical S23 install, frame-rate
measurement, thermal check or human playtest is claimed; this build is ready for
that feedback pass.
