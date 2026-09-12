# ADR 0033 — Existing-world flow and visual cohesion

2026-09-12. Implemented review candidate; physical acceptance and external 3DAIStudio generation remain open.

The user's full-game brief supersedes ADR 0032's restriction on art production.
Windward content 6 is the accepted design direction and its geometry is preserved.
The other four Campaign worlds and the separate Spiral challenge receive an
explicit authored pass. No new campaign worlds or movement strategies are added.

WorldFlowAuthoring consumes an explicit stable-ID recipe in
Assets/_Game/EditorTools/Data/WorldFlow170.json. It does not run at startup.
Recipes distinguish lateral width, longitudinal depth and recovery platforms.
Moving-path points and anchor bindings are authored alongside their supports;
the existing V4 content-version buckets separate incompatible records.

ModuleEnvironmentProfile owns optional cohesive lighting settings.
WorldLightingPresentation owns a scene-local, disposable grading/bloom volume
and hemispheric ambient fill. The runner composes it and enables gameplay-camera
post-processing. Geometry skins and near architecture participate in shadows;
far scenery remains inexpensive. Movement, input, camera aim and audio have no
dependency on lighting. Two cascades and low-quality soft shadow filtering are
provisional desktop settings until measured on S23.

Existing authored and generated world assets remain the primary kits.
WorldKitAuthoring produces small, deterministic, collider-free deck shells with
shared material families and world-specific construction. These are authored
meshes, not 3DAIStudio output. No 3DAIStudio tool is exposed or configured in this
session; the requested integration is unresolved and must not be represented as
used. Production provenance must distinguish reused assets, authored meshes and
any later external generations.

Before evidence includes five camera views for all six worlds. Sequence pilots
carry actual controller state between jumps, perturb initial entries, and report
landings and recovery outcomes. An injected initial speed establishes local
feasibility, not earning that speed in an entire human run. Full routes, transfers,
clearance and all regression suites pass (252 EditMode / 156 PlayMode). Android
verification and Git delivery are recorded in the production report.

Do not mark this milestone delivered until its evidence and outstanding limits
are recorded in the production report and handoff. No physical S23 testing is
claimed.
