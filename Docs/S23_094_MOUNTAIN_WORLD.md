# 0.9.4 — Mountain World and sequential Campaign

The user accepted Module 001's large visible improvement after physical Samsung
S23 testing of 0.9.3. This phase preserves that production content and Module 003.
No agent physical-device test is claimed. The single ADB inventory found no device.

## Old content removed

Module 002 v1 used a nearly flat 61.5 m route, 1.5 m landings, isolated decorative
trees/arches and seven extremely stretched nature-kit objects. The giant navy
rectangles in the supplied feedback were the oversized cliff-kit placements,
not a fog setting. The authored Mountain094 biome replaces that entire set.
Old module decoration entries are cleared and automatic fragments disabled.
The movement solver, manual camera, touch modes, hidden arms and loading host
remain unchanged.

## Five places and composition

- Cliffside Arrival: substantial fractured rock, broad 6 m landing, rooted alpine
  pines and a sightline to the split-peak waterfall and distant summit.
- Broken Mountain Pass: separated abutments and stone parapets frame the original
  first Moving mechanic; generous boarding surfaces and endpoint dwells remain.
- Waterfall Garden: a wider Restore court, planted side pavilion, layered cliffs
  and a longer valley loop overlooking the waterfall landmark.
- High Ridge: a second Restore at the old lift, a rising diagonal Moving crossing,
  exposed ridge ascent, and an optional windward cut.
- Summit Patch: the highest court, a layered stone crown and an open mountain view
  make the destination visible and the climb legible.

The split peak includes a small cliff temple and an opaque animated waterfall.
Separate near, middle, far and lower rock groups establish depth; distant ranges
extend hundreds of metres. A referenced seamless cloud sky and mountain-specific
fog/material profile supply below-world cloud depth without a planar fog floor.
The profile and geometry are independent of Sky City's architecture and Ancient
Abyss's composition. Five gameplay camera views were captured, inspected and
iterated; initial cone silhouettes, repetitive columns, hidden waterfall, floating
vegetation and overly close distant geometry were corrected during authoring.
Final art acceptance remains a physical S23 judgment, not an automated Gold claim.

## Gameplay and collision

The route climbs 12.4 m over a roughly 100 m journey. Existing stable module,
Moving, first Restore, Patch and old block IDs survive; Module 002 content version
is now 2. Standard landings are generally 2.9 m wide, with larger resting courts.
Two meaningful Restores divide the garden and high-ridge sections. The standard
route uses ordinary jumps and two Moving crossings, without Boost/Crumble gates
or mandatory air-strafe/bhop. Original rank thresholds remain explicitly
provisional pending physical timing calibration.

Two optional mastery lines use the inner rock spine and a windward lift cut.
Two Flow Shards reuse the attempt-local challenge system: optional, not currency,
not required for progression, retained on Restore and reset on Retry.

Every near structural mesh has a matching AuthoredSurface mesh collider. Distant
scenery has no collider and its bounds are kept outside ordinary jump reach.
Restore/Patch definitions can now reference their existing authored support by
stable ID, eliminating duplicate floors in Module 002 while preserving old
module defaults. Runtime support references and validator checks fail explicitly
on missing IDs. No decorative plane prevents falls or replaces the fall rule.

Full captured scene: 257 renderers (including disabled gameplay renderers/UI)
and 168 colliders. These are object counts, not draw-call measurements.

World budget: 60,608 triangles across 47 placed groups, 182 renderers before
static combining. Shared materials, baked meshes, a 256-pixel mineral-grain map,
static geometry combining, no world shadow casters, and a small opaque scrolling
waterfall shader bound the cost. These counts do not establish sustained phone
60 FPS; physical timing, thermals and memory remain unmeasured.

## Campaign and save compatibility

A fresh Campaign exposes only Module 001. Normal UI cannot launch locked future
modules, even in a development APK; the QA PRACTICE bypass is removed.
Continue selects the highest unlocked unfinished module. Older unlocked modules
remain deliberate replays. A valid completion earns at least Bronze and opens
the next explicit Campaign ID. Results preserve rank/time/PB/next-rank gap, make
Next primary and Retry secondary, and identify a newly unlocked road.

The explicit order is 001 -> 002 -> 003. The Spiral is separate, and Next never
enters it. Future Campaign 004–008 are unbuilt and unavailable in this APK.

V4 saves are retained without a wipe. Historical all-finish completion counts
retain their meaning. Eligibility uses existing validity and earned rank evidence:
practice-only records/attempts do not unlock roads, a later practice attempt never
removes a genuine prior rank, and genuine later completions preserve earlier road
access. Explicit unlock grants are retained. Derivation does not modify old PBs,
rank records or content-version metadata. See ADR 0016 for the compatibility rule.

## Evidence

Source validation: Logs/phase094-source-validation.log.
Final source checks pass; 222/222 EditMode and 25/25 PlayMode tests pass.
The four mountain cases were rerun after adding final UI captures and also pass.
Full EditMode and PlayMode results: Logs/phase094-editmode-final-results.xml and
Logs/phase094-playmode-final-results.xml. New tests cover fresh/legacy Campaign,
real Bronze Next/replay/Continue, source-factory preservation of all three
production modules, near/far collision, every standard jump, both Moving docks,
live platform carry, both Restores, actual shard pickup and Retry reset.
World/camera evidence: Logs/Phase094VisualQA/before and /after, world-budget.json,
jumps.txt and after/scene-budget.txt. Screenshots are Unity renders at 1560x720 and
94-degree gameplay FOV, not phone captures. Jump checks use the real unchanged
motor with ordinary forward input; they are not human timing calibration.

## Physical Samsung S23 gate — stop here

1. On a genuinely fresh installation/save, Campaign only permits 001; future
   cards cannot bypass locks. Finish 001 at any valid rank, use Next to 002,
   replay 001, then check Continue still targets unfinished 002. Do not erase an
   existing valuable save merely to run this check; use a separate test install.
2. Check the mountain opening is a visible quality jump, old navy slabs are gone,
   the route feels embedded in geology, all five places are distinct, and this
   world differs from both Sky City and Ancient Abyss.
3. Complete the normal route: readable jumps, fair Moving boarding/exits, no
   attractive surface fall-through, reliable garden/ridge Restores and an earned
   summit destination. Explore both optional lines and shard Restore/Retry rules.
4. Check sustained FPS/heat, both landscape safe areas, immersive fullscreen,
   thumb comfort, Classic/manual camera, Easy Mode, loading video/fallback,
   Retry/Next, app pause/resume and an existing save's unlocked progress.

Do not advance to another production module until this build receives physical
feedback. No physical S23 result or final Gold acceptance is inferred here.

## Verified Android package — 2026-09-07

Build succeeded in 00:02:16 with 0 errors and one existing legacy-icon warning.
APK: Builds/Android/RYDERS-ROAD-0.9.4-module002-mountain-world-gold-dev.apk
Size: 181,852,683 bytes. ARM64 only. Package com.rydersblockstudio.rydersblock;
versionName 0.9.4-module002-mountain-world-gold, versionCode 1.
SHA-256: ce45384ea9b382597a201b2935075ce75a7be8e3610bcab5fe198113b80c770b
APK content inspection confirms Module 002, Mountain biome, SplitPeakWaterfall,
Flow Challenge and waterfall shader. The packaged 3,705,679-byte loading video
matches the source byte-for-byte. Evidence: Logs/phase094-package-verification.json,
phase094-apk-badging.txt, phase094-apk-manifest.txt and phase094-android-build.log.
The single ADB inventory returned no attached device; physical acceptance is OPEN.

The subsequent milestone audit is in MOVEMENT_MASTERY_FOUNDATION.md. It changes
no runtime content or movement and does not require another APK build.
