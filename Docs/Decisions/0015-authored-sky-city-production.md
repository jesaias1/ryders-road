# ADR 0015 — Authored Sky City production content

2026-09-06. The user's 0.9.3 brief explicitly advances production to Module 001
and supersedes the previous stop-for-frontend-QA scope. Physical phone acceptance
is not inferred from that authorization.

Every level should be a memorable place containing a parkour course. Module 001
now has a deliberate near/mid/far/below envelope and an open sky. Sky City uses
arrival/garden/overlook architecture, a broken bridge rhythm, civic gateway,
terraced city districts and a sun observatory. Foreground landings have priority
over scenery. No invisible floor cards or broad non-colliding roof impostors.

Editor-authored meshes/prefabs are baked and consumed through the existing biome
profile. AuthoredSurface preserves matching collision for near stone/metal;
small foliage remains permeable. Distant architecture stays beyond ordinary jump
reach. Shapes/material choices/placement are authoring data, not runtime gameplay
constants or per-frame procedural world generation.

ModuleEnvironmentProfile has optional sky material and biome-fog override fields.
Defaults preserve previous modules; Module 001 uses its own environment asset.
The existing approved seamless cubemap supplies clouds/depth without world floor
geometry. Module 003 and the global sky assets are not modified.

Module 001 content version becomes 2 because landing widths changed. Stable route,
Restore, Patch and module IDs, route centers, Bronze progression and V4 save
meaning stay intact. Historical PBs retain recorded content-version metadata;
rank thresholds remain provisional pending physical calibration. No migration is
needed for this content revision.

Startup content factories preserve Module 001 v2 and its authored Sky City biome.
They must not silently replace production assets with v1 prototype defaults.
Regression tests exercise that preservation through the actual setup entrypoint.
Explicit rebuild is SkyCityProductionAuthoring.Build; it only reauthors this city.
