# RYDER'S ROAD Phase 0.5.7 Report

Target: `0.5.7-world-cohesion-and-gameplay-presentation`

1. Starting git state: one historical commit (`db061d1`) plus a large inherited dirty/untracked working tree; no unrelated changes were reverted.
2. Handoff read: `Docs/AI_HANDOFF.md` first, then architecture, art, movement, camera, level, testing, known-issue, save, visual-role, and latest ADR documents.
3. New block ZIP: located at the exact user-supplied Downloads path and copied/extracted into project-owned Meshy source; Downloads copy untouched.
4. FBX: `Meshy_AI_MedTech_Supply_Crate_0821184644_texture.fbx`.
5. Geometry: one mesh, 5,217 triangles, 5,101 vertices, one submesh.
6. Material/textures: one material with Base Color, Metallic, Normal, and Roughness maps; runtime textures capped at 1024 by importer policy.
7. Standard prefab: `Assets/_Game/Art/Environment/Platforms/PF_RR_Block_Standard.prefab`.
8. Native bounds: center `(0,0,0)`, size `(1,0.7734,1)`, centered pivot, root orientation `(270,0,0)`, import scale `1`.
9. Runtime Standard dimensions: authored collision remains `1.5 x 0.6 x 1.5m`; uniform visual body is `1.5 x 1.1601 x 1.5m`, top aligned and extending downward.
10. Standard collision: primitive BoxCollider remains authoritative; runtime prefab contains no MeshCollider; horizontal visible/standable edges coincide.
11. Previous Safe strategy: repeated canonical blocks through default modular tiling, producing obvious 2x2/3x3/4x4 carpets.
12. New Safe strategy: dedicated `PF_RR_Block_Safe` prefab selection with one coherent uniform visual over one authored collider.
13. Safe visual dimensions: common `3 x 0.6 x 3m` collider with an approximately `3 x 2.3202 x 3m` top-aligned visual body.
14. Larger version scaling: separate Safe prefab identity using the same canonical mesh with uniform scaling; no arbitrary non-uniform deformation.
15. Safe collision: one authored BoxCollider matches the visible horizontal footprint; decorative mass extends only downward.
16. Remaining tiling: `ModularTile` is reserved for unusual square surfaces at least `6m`; no current Campaign or Spiral gameplay block selects it.
17. Previous hand pose: left/right positions `(-0.42,-0.36,0.46)` and `(0.42,-0.36,0.46)` with near-symmetric fully extended framing.
18. New hand transforms: left `(-0.40,-0.39,0.39)` / `(286.91,41.63,135.63)`; right `(0.39,-0.37,0.43)` / `(284,321,229)`; scale `0.36`.
19. HandPoseLab: GPU render confirms both hands remain visible low in frame with intentional asymmetry; output `Logs/HandPoseLab/final.png`.
20. Previous Restore yaw: both player and camera reset to authored checkpoint rotation after every Restore.
21. New Restore yaw: death Restore captures rendered yaw when requested and reapplies yaw-only rotation at the checkpoint; pitch resets through existing camera architecture.
22. Initial spawn/manual Restore: initial spawn still uses authored StartAnchor rotation; manual Restore still uses authored checkpoint rotation.
23. Restore tests: EditMode covers yaw resolution; PlayMode covers ten authored Spiral entries plus forced death Restore at `127` degrees.
24. Moving audit: the prior platform oscillated sideways near the route and could be ignored without offering a useful line.
25. Moving change: stable ID retained; now crosses the inner gap as the optional `Moving inner bridge` shortcut at `2.35m/s` with `0.7s` endpoint pauses.
26. Red identity: `crumble.m04.final.01` through `.03`, role `Crumbling`, orange root/fault language, authored by `Phase2ModuleContentFactory`.
27. Red resolution: retained as a purposeful upper mastery line; canonical visual replaces primitive appearance and child renderers now follow collider solidity.
28. Automatic decoration: disabled for Spiral; all world dressing is explicit stable-ID content.
29. Structural placements: lower start island/gateway/tree/twin ruins, garden island/tree/arch, energy island/pillars/arch, and paired summit pillars.
30. Mid-distance placements: garden and energy chapter structures frame route progression without becoming collision.
31. Far distance: three floating island/tower groups and three large cloud banks imply a wider civilization at 70-105m offsets.
32. Atmospheric depth: existing light-cyan exponential fog and seamless procedural sky are preserved; explicit clouds create below-route and far-depth parallax.
33. Chapters: Lower Ruins, Garden Sky, Energy Ruins, Summit Ruins, followed by a visible far-world backdrop.
34. Landmarks: lower gateway/tree, garden arch, cyan energy pillar, paired summit pillars/Patch, and north far spire.
35. Environment collision: decorations are instantiated visual-only; gameplay collision remains on authored route/mechanic objects.
36. Performance/LOD: new block is 5.2k triangles; world uses a limited explicit low/mid-poly set, no shadows/probes/motion vectors, and cheap rounded cloud primitives. Formal LOD waits for device profiling. Android builds now delete stale output and clean build cache to avoid intermittent 93 MB packaging padding.
37. Movement regression: no movement profile or solver changes were made; established movement tests pass.
38. Collision regression: Standard/Safe/Long fit tests, authored footprints, ten solid starts, Restore landing, moving sync foundations, crumble visibility logic, and Patch wiring pass.
39. EditMode: `172/172` passed, zero failed; `Logs/phase057-editmode-results.xml`.
40. PlayMode: `5/5` passed, zero failed; `Logs/phase057-playmode-results.xml`.
41. Source validation: `Tools/Validate-Foundation.ps1` and Unity project validator passed.
42. Android build: succeeded, zero errors and one nonfatal Unity legacy-icon deprecation warning, `Logs/phase057-android-build.log`.
43. APK: `Builds/Android/RYDERS-ROAD-0.5.7-world-cohesion-and-gameplay-presentation-dev.apk`.
44. APK size: `160,687,446` bytes.
45. SHA-256: `811A9A67B21C853B49D0AC9F914B40201E9C37CF3E8E815C488F6E54F2ADF000`.
46. S23 install: passed on Samsung `SM-S911B`, serial `RFCW100J1HW`; installed versionName matches `0.5.7-world-cohesion-and-gameplay-presentation`.
47. S23 launch: passed. A preliminary Spiral smoke exposed stripped `SphereCollider` errors from rounded cloud creation; the repaired final APK loads title and Spiral at 60 FPS with `Ground True`, no development console, no SphereCollider errors, and no app crash. Evidence: `Logs/device-057-title-final.png`, `Logs/device-057-spiral-start-fixed.png`.
48. Manual gameplay acceptance: deliberately deferred to the user per human-in-the-loop rule.
49. Handoff: `Docs/AI_HANDOFF.md` updated as authoritative Phase 0.5.7 truth.
50. Physical checklist: included below.
51. Recommended next phase: `0.5.8-s23-physical-acceptance-and-targeted-world-polish` after user feedback.

## S23 Checklist

1. Does the new ordinary Standard block look cleaner?
2. Do its visible edges match where Ryder can stand and fall?
3. Does the coherent Safe platform look better than repeated block carpets, and does its collision match?
4. Do neutral and running hands feel lower, more natural, and free of clipping/jitter?
5. Face a distinctive direction, fall, and confirm Restore preserves that horizontal direction.
6. Confirm initial Spiral entry still faces the opening route.
7. Is the moving inner bridge clearly readable as an optional faster route?
8. Do the three orange crumble blocks read as a mechanic, and disappear fully when non-solid?
9. Does Spiral feel less empty, with recognizable lower, garden, energy, and summit chapters?
10. Check fullscreen, both landscape orientations, sustained FPS, heat, pause/resume, and any overlap.
11. Send screenshots of a Standard close-up, Safe platform, neutral hands, lower/mid environment, wide world composition, and summit if reachable.
