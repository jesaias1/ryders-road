# RYDER'S ROAD — CANONICAL GAMEPLAY VISUAL ROLE MAP

Generated for `0.5.5-role-lock-and-spiral-recovery`; canonical platform policy
revised by `0.5.7-world-cohesion-and-gameplay-presentation`, sequential
crumble truth added by `0.5.9-world-cohesion-and-crumble-truth`, route contact
truth revised by `0.6.0-crumble-route-truth-and-hand-refinement`, and standard
one-second crumble rhythm set by `0.6.1-parkour-feel-and-route-rhythm`.
Water/visual-only collision truth was tightened by
`0.6.3-hand-pose-collision-truth-abyss-depth` and refined by
`0.6.4-natural-hand-pose-and-world-composition-pass`.

This document defines the authoritative, deterministic mapping between gameplay roles, production visual prefabs, scaling rules, and collision policies.

---

## 1. Canonical Role Table

| Gameplay Role | Canonical Prefab | Source FBX | Fit Mode | Sane Scale (X, Y, Z) | Walkable | Collision Policy | Tileable | Fallback Policy |
| --- | --- | --- | --- | --- | :---: | --- | :---: | --- |
| **Standard** | `PF_RR_Block_Standard` | `Meshy_AI_MedTech_Supply_Crate_0821184644_texture` | `TopAlignedUniform` | `1.5m` horizontal visual | YES | Simple BoxCollider (`1.5m x 0.6m x 1.5m`) | NO | Error in dev; neutral grey debug block |
| **Precision** | `PF_RR_Block_Standard` | Same canonical source | `TopAlignedUniform` | Compact uniform visual | YES | Authored compact BoxCollider | NO | Error in dev; neutral grey debug block |
| **Long Platform** | `PF_RR_Platform_Long` | `Meshy_AI_Aetherstone_Platform` | `ExactFootprint` | Form fitted | YES | Authored BoxCollider, commonly (`3.0m x 0.6m x 1.5m`) | NO | Error in dev; neutral grey debug block |
| **Safe / Large** | `PF_RR_Block_Safe` | Same canonical Standard source | `TopAlignedUniform` | `3.0m` coherent uniform visual | YES | Single authored BoxCollider, commonly (`3.0m x 0.6m x 3.0m`) | NO | Error in dev; neutral grey debug block |
| **Moving Block** | `PF_RR_Platform_Long` | `Meshy_AI_Aetherstone_Platform` | `ExactFootprint` | Form fitted | YES | Moving BoxCollider + `MovingBlock` script | NO | Error in dev; neutral grey debug block |
| **Crumbling Block**| `PF_RR_Crumble_Stage1/2/3` | `Chrono_Core_Crate` / `Fractured_Power_Core` / `Molten_Ruins` | `TopAlignedUniform` | Coherent uniform visual | YES | Stationary primitive BoxCollider + visual-only staged `CrumblingBlock` lifecycle | NO | Error in dev; neutral grey debug block |
| **Restore Point** | `PF_RR_RestorePoint` | `Meshy_AI_Glacier_Core_Chest` | `Uniform` | `(1.6, 1.6, 1.6)` | YES (Base) | Solid base (`3m × 0.6m × 3m`) + Trigger zone + Authoritative SpawnAnchor (`y + 0.05m`) | NO | Error in dev; neutral grey debug block |
| **Jump Boost** | `PF_RR_BoostPad` | `Meshy_AI_Neon_Launch_Pad` | `Uniform` | `(1.8, 1.8, 1.8)` | YES (Pad) | Solid foundation BoxCollider + JumpBoost trigger + velocity impulse | NO | Error in dev; neutral grey debug block |
| **Surf Ramp** | `SurfSurface` (CS Surf) | Authored Mesh | `Fixed` | `(4.0, 0.45, 16.0)` | YES (Slope)| Angled Plane Collider (25°–45° tilt) + SurfSurface tangential solver | NO | Error in dev; neutral grey debug block |
| **Patch Goal** | `PF_RR_PatchBlock` | `Meshy_AI_Aetherstone_Relic` | `Uniform` | `(2.2, 2.2, 2.2)` | YES (Base) | Walkable summit slab (`3m × 0.6m × 3m`) + Goal Trigger zone + Floating Relic visual | NO | Error in dev; neutral grey debug block |

---

## 2. Canonical Environment / World Architecture Roles

| World Role | Canonical Prefab | Source FBX | Sane Scale | Walkable | Collision Policy | Usage |
| --- | --- | --- | --- | :---: | --- | --- |
| **Tower Core / Pillar** | `PF_RR_Pillar_Tall` | `Meshy_AI_Aether_Pillar` | `(8.0, 8.0, 8.0)` | NO | None / Background | Vertical ascent landmark & depth framing |
| **Ruined Tower** | `PF_RR_RuinedTower` | `Meshy_AI_Arcane_Ruins` | `(6.5, 6.5, 6.5)` | NO | None / Background | Architectural ruin framing |
| **Floating Arch** | `PF_RR_FloatingArch` | `Meshy_AI_Luminous_Portal_Ruins` | `(5.0, 5.0, 5.0)` | NO | None / Gateway | Checkpoint and summit gateway cues |
| **Energy Pillar** | `PF_RR_EnergyPillar` | `Meshy_AI_Aether_Crystal_Nexus` | `(4.5, 4.5, 4.5)` | NO | None / Marker | Circuit and high-energy landmark cues |
| **Broken Rock** | `PF_RR_Rock_Broken` | `Meshy_AI_Clay_Meadow_Island` | `(4.5, 4.5, 4.5)` | NO | None / Background | Floating background islands |
| **Floating Tree** | `PF_RR_Tree_Floating` | `Meshy_AI_Elderwood_Isle` | `(4.0, 4.0, 4.0)` | NO | None / Nature | Floating nature island framing |
| **Grass Topper** | `PF_RR_GrassTopper` | `Meshy_AI_Azure_Sentinel_Obelis`| `(2.5, 2.5, 2.5)` | NO | None / Nature | Island topper vegetation |

---

## 3. Strict Collision Truth Invariants

1. **Top Surface Alignment**:
   For any walkable block at position `y` with height `H = 0.6m`, the walkable top is at `Y_top = y + H * 0.5f = y + 0.30m`. The visual top surface MUST exactly align with `Y_top`.
2. **Zero Invisible Air Landing**:
   If the player can stand, there must be visible walkable geometry beneath the player capsule. If there is visible empty air, the player must fall into the void.
3. **No Extreme Non-Uniform Stretching**:
   Standard and Safe use `TopAlignedUniform`; Long uses a calibrated `ExactFootprint` correction. Assets are never arbitrarily squashed on Y while expanded on X/Z.
4. **Authoritative SpawnAnchor**:
   Every Restore Point has an authored spawn anchor at `new Vector3(X, Y_top + 0.05m, Z)` centered on the safe platform, clear of decorative shrine meshes.
5. **Dedicated Common Footprints**:
   Ordinary square and Safe surfaces resolve to one coherent prefab. Long surfaces resolve to the dedicated Long prefab. `ModularTile` remains only as an explicit fallback for unusual square surfaces at least `6m` wide; current Campaign and Spiral gameplay do not use it.

6. **Crumble Collider Truth**:
   Stage meshes are presentation only. Their shared visual root may shake, but
   the authoritative primitive BoxCollider never moves. It disables only at
   collapse and restores deterministically. Valid upward CharacterController
   contact activates it through a player-side relay; its trigger is fallback.
7. **Visual-Only Platform Impostor Ban**:
   Any broad surface that reads like a jump target must have truthful gameplay
   collision. If it is decorative, atmospheric, water/flow, circuit, surf
   dressing, or any other visual-only role, it must render as mist, ribbons,
   filaments, vertical strips, streaks, silhouettes, or another non-landable
   signal rather than a flat platform-like slab.
8. **Water Trigger Visual Truth**:
   Water/flow volumes are non-solid trigger volumes. They may affect movement
   only through the authored trigger behavior and must not render as broad blue
   rectangular landing plates.

## 4. Phase 0.5.9 Crumble Audit

- Stage 1: 5,155 triangles; Stage 2: 5,086; Stage 3: 5,194.
- Each runtime base texture is capped at 1024x1024 for Android.
- Runtime prefabs contain no colliders and use project-owned Meshy sources.
- Production Spiral stages now advance near 0.33 and 0.66 seconds, collapse at
  approximately 1.0 second, and reset approximately four seconds later.
- Repeated contact and micro-hops do not restart the countdown.
- Nineteen production blocks replace route supports directly; no duplicate
  ordinary support exists beneath them.
- Full audit: `Logs/phase059-crumble-asset-audit.txt`.

## 5. Phase 0.5.7 Canonical Block Audit

- Source FBX: `Meshy_AI_MedTech_Supply_Crate_0821184644_texture.fbx`.
- Audit: one mesh, 5,217 triangles, 5,101 vertices, one submesh/material.
- Native bounds: center `(0, 0, 0)`, size `(1, 0.7734, 1)`; centered pivot; root orientation `(270, 0, 0)`; import scale `1`.
- Runtime prefabs contain no MeshCollider. The primitive gameplay BoxCollider remains authoritative.
- Standard and Safe visuals are top aligned to the authored collider and extend downward as decorative mass without widening the standable footprint.
