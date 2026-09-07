# Module 001 — Sky City production pass (0.9.3)

2026-09-06. Implements the user's Module 001 production brief. The new brief
supersedes the prior frontend-only stop scope; it does not constitute physical
S23 acceptance. No additional Campaign levels were produced in this phase.

## Audit and visible rebuild

The original 47 m introductory course used 12 narrow blocks, isolated generic
props and seven city meshes stretched to extreme heights. Representative views
showed enormous dark façades crowding the route, repeated ancient arches, little
local architecture, and a pale lower void with a plain gradient sky.

The new city is an authored sequence:

- **Arrival Terrace:** a broad departure roof with planted side terraces and a
  first view of the whole skyline and distant sun gateway.
- **Broken Sky Bridge:** readable separated roof supports with tapered structural
  piers and restrained brass seams. The adjacent aqueduct places the broken route
  in a larger city's transport network.
- **Suspended Garden:** a widened Restore court with colliding garden terraces,
  benches and approved textured bonsai crowns. The next landing stays unobscured.
- **Sun Gateway:** a civic arch whose clear central opening frames the final
  approach. A separate twin-tower sun observatory anchors the distant skyline.
- **Patch Overlook:** a generous landing and planted balcony with a clear Patch
  focal point and an open view across the city.

The envelope contains 21 intentionally placed near/mid/far/lower groups using
8 baked architectural prefabs. Ivory cornices, teal glazing, vertical window
mullions, brass caps, setback roofs and terraced districts replace stretched
blockout towers. The approved seamless sky cubemap supplies the cloud ocean and
open upper sky; no floor cards, dark landing circles or cloud slabs were added.

The first visual iteration's simple near-tree shapes were rejected. The refined
version reuses the project's textured tree crown, removes its floating-rock base,
and scales foliage back to preserve the forward view. Source tree assets remain
unchanged. Distant foliage uses inexpensive simplified silhouettes. The two city
landmarks have distinct civic roles; obsolete repeated route arches are removed.

Five gameplay views were captured and reviewed repeatedly, including actual
spawn camera position and early/mid/gateway/final approach at 94-degree FOV and
S23-shaped 1560x720 viewport. Evidence is in `Logs/Phase093VisualQA/before` and
`Logs/Phase093VisualQA/after`. The before set was captured at the same course
stations, with opening view centered on the start roof; the final opening uses
the actual spawn camera position. These are Unity renders, not phone captures.

## Gameplay, collision and scope

All 12 route centers and the original jump cadence are retained. Standard landing
width grows from 1.5 m to 2.5 m; arrival/Patch roofs are 6 m wide and the Restore
court is 5 m wide. The final run-up is 3.4 m wide. The route stays introductory,
with one earned Restore and no mandatory advanced mechanic or new pickup system.

Near stone/metal geometry uses the same baked mesh for rendering and collision
through AuthoredSurface. Garden roofs are real supports. Foliage is permeable;
there are no floating-rock tree bases pretending to provide extra platforms.
Distant architecture is outside ordinary jump reach and collider-free.

A focused test samples nine support positions on each of the twelve route blocks,
checks actual runtime authored surfaces, and lands all eleven links using the
unmodified motor at a constant 85% forward input. Each link starts from a reset
source platform; this is a per-jump regression, not a continuous human completion
or timing calibration. Full Campaign, Retry and Next integration tests remain.

Module content version is now 2 because landing widths changed. Stable module,
route, Restore and Patch IDs, normal Bronze progression and V4 persisted meaning
are unchanged. Historical PB metadata is retained; thresholds remain provisional.
Movement physics, Classic/manual camera, editor/touch controls, hidden arms,
0.9.2 loading/fullscreen behavior, Modules 002/003 and Spiral content are preserved.

## Authoring and reproducibility

`SkyCityProductionAuthoring.Build` explicitly bakes the city assets; runtime does
not procedurally rebuild meshes. The normal startup factory was found to overwrite
Module 001 and its biome on every Editor launch. It now preserves authored v2
content, and a regression runs the actual project setup to verify preservation.

ModuleEnvironmentProfile now permits an optional sky material and biome-fog
override. Only Module 001's separate environment asset opts in; previous defaults
remain for other modules. Geometry is grouped by material in each prefab, with
static batching allowed and no city shadow casters, per-frame building scripts,
additional realtime lights, or new particle systems.

See ADR 0015 for ownership and compatibility policy. No changes were made to the
user's original media/branding or unrelated pre-existing repository work.

## Validation and APK

- Source validation passed: `Logs/phase093-source-validation.log`.
- EditMode: **217/217**, `Logs/phase093-editmode-final-results.xml`.
- PlayMode: **21/21**, `Logs/phase093-playmode-final-results.xml`.
- All 11 ordinary jumps and 108 landing support samples pass using actual runtime
  geometry/motor. Existing Campaign/Spiral/Patch/Retry/Next/loading-fallback,
  movement/manual-camera, progression/save and Module 003 regressions pass.
- Authored content survives the real Editor setup routine; distant roofs clear
  the route; near render/collision meshes match, including after static batching.
- City world: **133,258 rendered triangles**, **123 mesh renderers before batching**
  across 21 placements. Full captured scene: 157 renderers / 79 colliders.
  Evidence: `Logs/Phase093VisualQA/world-budget.json` and
  `Logs/Phase093VisualQA/after/scene-budget.txt`. These counts do not prove 60 FPS.
- All five final views inspected; final spawn rail panels were replaced by open
  railings after the close-range review. No screenshot-only geometry was used.
- Android ARM64 development APK built successfully with **0 errors / 1 legacy-icon
  deprecation warning**: `Logs/phase093-android-build.log`.
- APK: `Builds/Android/RYDERS-ROAD-0.9.3-module001-sky-city-gold-production-pass-dev.apk` (180,247,689 bytes).
- SHA-256: `72f71004eb02b829b708449d4214e3aeca5bac0f59ffa9bc06ab703c06e26521`.
- Manifest verified: requested 0.9.3 version, unchanged package
  `com.rydersblockstudio.rydersblock`, ARM64-only, minimum API 26, target API 36.
  Evidence: `Logs/phase093-apk-badging.txt`, `Logs/phase093-apk-manifest.txt`.
- All eight city prefabs appear in Unity's built asset list. Module 001 v2 survives
  build setup. The unchanged 3,705,679-byte loading MP4 is verified byte-for-byte
  in the APK: `Logs/phase093-package-verification.json`.
- The historical legacy-icon warning remains; no new build warnings were added.

## Remaining physical checks

No S23 was connected in the single ADB inventory check
(`Logs/phase093-adb-inventory.txt`). Physical testing was not performed.

Confirm the opening impression and five-place composition on the phone, novice
jump rhythm, footing/collision of garden and balcony surfaces, Restore, Patch,
Retry/Next, both landscape safe areas, loading-video/fullscreen/resume stability,
and sustained 60 FPS/thermal behavior. Rank timing requires physical calibration.
The level is a visibly transformed production candidate; final human Gold/art
acceptance and device performance are not certified by Editor renders or tests.
