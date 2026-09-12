# World 0.17.0 asset and performance audit

## Origin and processing

This pass uses deterministic Unity Editor authoring, plus processed copies of existing production assets. **No 3DAIStudio output was generated**: no callable integration was exposed or configured in this session. This is a material limitation against the requested external-generation portion, not a claim that another generator was used.

`WorldKitAuthoring` and `WorldMeshBake` are the source for six deck shells and four sparse anchor foundation kits. Their dimensions, bevels, panel arrangement, material colors and roughness are explicit in source. No image references, downloaded models or generated textures are embedded in these new meshes. They are project-authored geometric work. Deck prefabs contain no colliders; route definitions remain the collision authority. New foundation prefabs pair their visible meshes with fatal MeshColliders.

`refit-provenance.csv` records every retained near-object source and final prefab. `WorldSceneryRefit` copies the existing source, maps it along the authored route, moves obstructing architecture aside, lowers support footings as necessary, recalculates normals/bounds and reuses the exact new mesh for visual and fatal collision. No original source file is overwritten. Original third-party terms continue to apply; see `Docs/THIRD_PARTY_ASSETS.md` and `Docs/PHASE_0_5_0_MESHY_ART_KIT.md`. This processing does not establish new licensing rights.

## Material and shader budget

Seven shared, instancing-enabled URP Lit families are introduced: Ivory, Structure, Stone, Rock, Metal, Warm and Cyan. They use no textures, normal maps or transparency. Metal alone uses modest metallic response. Smoothness spans 0.12–0.35. World identity comes from geometry, existing landmark kits and atmosphere as well as color. Existing textured landmark materials remain reusable production assets; they have not all been replaced or flattened into these seven families.

The per-mesh inventory records index-derived triangle and submesh counts. New deck shells are regression-limited below 1,000 triangles each. There are no LOD chains on these small new pieces. Existing source details, including any larger textures and lack of LODs, are recorded in each world budget CSV and remain a device profiling concern. Runtime static batching is retained for near environment; eligible materials enable instancing. These flags do not prove a particular draw-call count.

## Lighting and measurement limits

One directional sun, hemispheric ambient, restrained grading/bloom and distance-limited shadows form the shared lighting system. The 2048 directional map uses two cascades at 65 m with low-quality soft filtering. Decks and selected near scenery receive/cast shadows; far dressing remains cheap. No additional shadowed lights, SSAO, realtime GI or volumetric clouds are introduced.

After-capture budget CSVs report instantiated mesh geometry (combined static meshes counted once), material/texture inventory, LOD and shadow participation. Triangle totals describe scene geometry, not triangles visible in every view or actual GPU draws. Camera.Render CPU wall times are 12 warm desktop samples at 1560×720 on the recorded GPU; they do not measure GPU duration, sustained frame pacing or S23 performance.

Physical S23 profiling is still required for both landscape orientations, sustained 60 FPS, GPU/CPU frame time, shadow cost, memory, thermal throttling and touch readability. Do not call these provisional settings device-approved.
