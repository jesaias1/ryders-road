# 0.17.0 — Existing-world flow and cohesion

This is a desktop-verified review candidate; physical acceptance and the requested
3DAIStudio generation remain outstanding. The full-game brief supersedes the previous
art-production stop. Windward's content-6 geometry and shared compatibility-5
movement remain unchanged. No physical S23 testing has been performed.

## Geometry phase

Sky City, Mountain, Ancient Abyss and Solar Foundry use explicit stable-ID recipes
in `Assets/_Game/EditorTools/Data/WorldFlow170.json`. Width, depth, heading and
recovery areas are independent authored values. Spiral retains its helical course
with modest catches on easy/setup/flow beats. Content versions are 4/5/9/5/6/3
in Campaign order followed by Spiral. V4 save meaning and unlocks are unchanged.

AllWorldFlowTests passes 180 slow/flow/expert sequence entries, including lateral,
heading and timing perturbations. Initial incoming speed is a fixture input;
subsequent jumps carry actual controller state. It does not establish that a
human earns every initial speed in a complete run. The existing Windward 72-case
matrix remains required in the full suite.

WorldContinuousRouteTests reaches each Campaign Patch through actual physics
triggers at 80% and 100% ground-stick pace, with explicit test-only aerial stick
correction. It includes live moving paths, crumbles and the Abyss Boost. There
are no per-hop teleports or player velocity writes. Mountain's final lift uses a
longer diagonal climb rather than the rejected steep path, which produced
unwanted carried-speed growth. Shared motor code is untouched.

Mountain’s optional lift bypass now climbs outside the moving sweep; Abyss’s
bypass includes an outside rejoin ledge. Foundry has a tested optional Boost
landing and a valid surf spillway with manual jump detachment.

Two obsolete Abyss temple mastery ledges are retired because they intersected
the broad shared route. Existing content-version separation preserves history.
Near architecture is refitted with its matching fatal mesh collider; tall
objects that intersect the flight corridor are moved aside as whole objects.
New sparse anchor foundations remain fatal scenery. No generated visual skin
has gameplay collision.

## Visual phase

Six authored deck shells share seven untextured URP Lit material families, with
world-specific panel, geological, masonry and gantry construction. Four worlds
receive sparse structural foundations; Windward retains its original anchors.
Existing world kits and landmarks are reused. Windward gains a clear blue sky
and stronger directional light; its route geometry remains unchanged.

ModuleEnvironmentProfile now owns optional hemispheric ambient, shadow and
post-processing parameters. WorldLightingPresentation owns a scene-local volume
and releases it on unload. Two shadow cascades, low-quality soft filtering,
selective near/deck shadow participation and restrained bloom are provisional
mobile settings. No SSAO, volumetric clouds or additional shadow lights are added.

3DAIStudio was not exposed in the session tool inventory or configured MCP list.
The integration question is unresolved. No asset may be described as newly
3DAIStudio-generated: current new meshes are deterministic Editor-authored
geometry or processed copies of existing production assets. External generation
and its licensing cannot be claimed.

## Evidence and remaining work

Thirty baseline and thirty after images are packaged in `Docs/World170QA`, covering
opening, mid-route, hero vista, fast approach and destination for all six worlds.
The gallery, route CSVs, per-world budgets, per-mesh inventory and provenance
reports are included there. All five after views of each world were visually reviewed. Desktop Camera.Render timings are CPU wall time only, not GPU
cost, device FPS or sustained S23 measurements.

Complete EditMode 252/252 and PlayMode 156/156 suites pass with no skipped tests.
Source validation and LFS hydration checks pass. Full result XML and hashes are
packaged in `Docs/World170QA/tests`. The 180-case matrix, Windward 72-case matrix,
ten continuous Campaign completions, optional routes, moving clearance, collision
truth, controls, saves/progression and audio regressions pass.

Rank thresholds remain provisional. Physical
feel, touch, landscape safe areas, thermals, frame pacing and final lighting
approval require S23 feedback after delivery.

## Android delivery

- APK: `C:\Users\lin4s\Documents\Riders Block\Builds\Android\RYDERS-ROAD-0.17.0-world-flow-dev.apk`
- Build succeeded with 0 errors and 1 existing legacy-icon deprecation warning; approved branding remains intact.
- Size: 189,190,245 bytes. ARM64 IL2CPP; package `com.rydersblockstudio.rydersblock`; minimum API 26 / target API 36; version `0.17.0-world-flow`.
- ZIP integrity and APK v2 signature verification passed.
- SHA-256: `AD4DA3EBDBCAAB5ED6CD7A225113CBA499E3BA8AB006A64D4FE714DFB0009B39`.
- Full machine-readable verification: `Docs/World170QA/apk-verification.json`.

Delivery branch: `main`. The final commit SHA and working-tree state are reported
in the delivery message. Stop for physical and visual feedback. Do not begin
Worlds 006–008, economy, multiplayer or PC work. The unavailable 3DAIStudio
portion remains explicitly incomplete; do not relabel authored meshes as generated.
