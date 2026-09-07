# S23 collision truth and flow continuation — 0.9.1

Status: development QA candidate. Physical S23 acceptance remains OPEN.
Module 003 is content version 6. No 001/002/004–008 art production was started.

## Exact causes and corrections

1. `Phase074WorldSystemRecovery.AddPrefabPiece` and `SaveLandmarkPrefab` removed
   nested colliders. `ModuleVisualPrefabLibrary.PrepareVisualInstance` removed
   colliders again at runtime. This blanket visual-only contract included all
   five near-place architectural prefabs, including broad landable arch bases,
   ruin ledges and meadow foundations. Their meshes were real, but no collision
   existed. Lowering the foundations in 0.9.0 did not solve the whole problem.
2. All 37 near-place structural/terrain meshes now explicitly opt into PLAYABLE
   via `AuthoredSurface`, with non-convex static MeshColliders using the exact
   authored render source. No bounding-box roof across an arch or empty space.
   Unity static batching replaces render meshes; the validation stores the
   original authored source and verifies collision against it after batching.
   Kenney foliage stays permeable; it is not a support surface.
3. Mid-world broken procession/celestial arch were large enough to overlap the
   apparent play corridor. They now sit at (-110,-60,95)/(120,-70,155); the far
   crown sits at (45,-42,245). These four mid/far/below entries remain SCENERY.
   Their footprint, depth and distance separate them from landing choices.
4. All three Restore shrine visuals had local Z 0.8 under a parent scaled 3:
   center 2.4 m forward, beyond the 1.5 m support half-depth. Local Z is now 0.18,
   keeping the entire shrine inside the support. No trigger/rule changes.
5. Long-platform mapping rendered only ~71% of the collider depth and a top
   ~0.086 m below collision. Exact-footprint fitting now uses the actual mesh
   bounds in support-local space. Start, arrival approach/court, temple entry,
   ferry dock, ferry and boost runup now match their authored BoxCollider.
   Tiled Boost landing art is top-aligned with its unchanged 8×8 collider.

### Every near structural/terrain landing corrected

- `M03_Arrival_Court_Buttress/RR_Pillar_Tall_Model`
- `M03_Arrival_Fallen_Pillar/RR_Pillar_Tall_Model`
- `M03_Arrival_Main_Foundation/RR_Rock_Broken_Model`
- `M03_Arrival_Sanctuary_Ruin/RR_RuinedTower_Model`
- `M03_Arrival_Start_Buttress/RR_Pillar_Tall_Model`
- `M03_Arrival_Upper_Foundation/RR_Rock_Broken_Model`
- `M03_Arrival_View_Arch/RR_FloatingArch_Model`
- `M03_Crossing_Dock_Buttress/RR_Pillar_Tall_Model`
- `M03_Crossing_Exit_Buttress/RR_Pillar_Tall_Model`
- `M03_Crossing_Far_Bridgehead/RR_RuinedTower_Model`
- `M03_Crossing_Far_Foundation/RR_Rock_Broken_Model`
- `M03_Crossing_Far_Mechanism/RR_Pillar_Tall_Model`
- `M03_Crossing_Near_Bridgehead/RR_RuinedTower_Model`
- `M03_Crossing_Near_Foundation/RR_Rock_Broken_Model`
- `M03_Crossing_Near_Mechanism/RR_Pillar_Tall_Model`
- `M03_Sanctum_Ascent_Foundation/RR_Rock_Broken_Model`
- `M03_Sanctum_Broken_Crown/RR_RuinedTower_Model`
- `M03_Sanctum_Crown_Foundation/RR_Rock_Broken_Model`
- `M03_Sanctum_Fallen_Column/RR_Pillar_Tall_Model`
- `M03_Sanctum_Final_Buttress/RR_Pillar_Tall_Model`
- `M03_Sanctum_Lower_Foundation/RR_Rock_Broken_Model`
- `M03_Sanctum_Patch_Nexus/RR_EnergyPillar_Model`
- `M03_Sanctum_Processional_Foundation/RR_Rock_Broken_Model`
- `M03_Sanctum_Threshold_Arch/RR_FloatingArch_Model`
- `M03_Sanctum_Threshold_Foundation/RR_Rock_Broken_Model`
- `M03_Spine_Approach_Foundation/RR_Rock_Broken_Model`
- `M03_Spine_Energy_Relay/RR_EnergyPillar_Model`
- `M03_Spine_Energy_Source/RR_EnergyPillar_Model`
- `M03_Spine_Landing_Foundation/RR_Rock_Broken_Model`
- `M03_Spine_Launch_Foundation/RR_Rock_Broken_Model`
- `M03_Spine_Left_Pylon/RR_Pillar_Tall_Model`
- `M03_Spine_Receiving_Tower/RR_Pillar_Tall_Model`
- `M03_Spine_Right_Pylon/RR_Pillar_Tall_Model`
- `M03_Temple_Collapsed_Wall/RR_RuinedTower_Model`
- `M03_Temple_Fallen_Foundation/RR_Rock_Broken_Model`
- `M03_Temple_Fallen_Hall/RR_RuinedTower_Model`
- `M03_Temple_Main_Foundation/RR_Rock_Broken_Model`

The source mesh/prefab paths, vertex counts and source bounds are recorded in
`Logs/Phase091VisualQA/source-surface-audit.md`. Actual runtime supporting-ray
samples and every ordinary/mechanic support are listed in `runtime-landings.md`;
`runtime-bounds.md` records the rendered versus collider bounds. The Bronze route,
five mastery supports, three Crumbles, ferry, Boost, three Restores and Patch
remain data-authored supports. The base meshes are roughly 3.8–4.5k vertices.

## Five-place composition and mechanic flow

Arrival retains its sanctuary arch and ruin; two under-route buttresses connect
its start and court to vertical ruin mass. Broken Crossing replaces both repeated
arches with broken tower bridgeheads and receives two dock buttresses. The ferry
still crosses the real 14.3 m diagonal break in under three seconds each way,
with 0.24 s endpoint dwell. Opposing pylons mark the mechanism; the two small
optional mastery ledges offer the faster bypass. Crumble court replaces its arch
with a sideways collapsed wall. Energy Spine receives a tower under the Boost
landing; its energy relay moves beside, rather than onto, the receiving deck.
Patch Sanctum retains the destination arch with a final ascent buttress.

Boost direction/strength/cooldown and all movement tuning are unchanged. Existing
ignition particles now pair with an event haptic, threshold/boost-flight streaks,
and landing impact feedback. The actual-trigger motor landing regression covers
slow, nominal and full-run approaches. Wider entry and thumb-aim feel need S23.

## Reusable feel

- Five optional cyan Flow Shards live on arrival, ferry-bypass and temple mastery
  lines. Unique authored IDs count once per attempt. Restore retains collected
  shards; Retry reloads a fresh challenge. Pickup burst, brief HUD count and
  results completion percentage. No economy, save schema, ranks or unlock coupling.
- `GameFeelProfile`: authored speed threshold 9.5 m/s, streak interval 0.2 s,
  Boost streak window 1.2 s, restrained haptic strength/durations. No FOV changes.
- `MomentumPresentation` reads motor state only; sparse lateral world particles
  use the existing bounded eight-emitter pool. Wind has an asset-driven loop,
  gain/fade control and explicit stop on results/disable.
- Existing one-second Crumble warning/collapse and fast Restore VFX remain;
  collapse now also emits its audio role. Moving mechanism has a spatial loop
  and start-intent event, with no per-frame object search.
- Android native one-shot haptics: 12 ms pickup/Restore/Crumble warning, 22 ms
  Boost/hard landing, 38 ms Patch/PB/rank, amplitude 70/255, 180 ms cooldown.
  Uses the existing HAPTICS preference. No normal jump/step/wind vibration.
  Legacy long Flick vibration removed. Android library manifest adds VIBRATE.
- Patch is immediate; PB/rank achievement feedback follows at 250 ms to avoid
  stacking. Rank audio only emits for a rank improvement. Results stop gameplay
  input, momentum audio and Restore checks; no camera/motor rules are rewritten.
- No additional powerup: authored Boost plus mastery collection provides purpose
  without adding a new movement action or weakening Classic movement mastery.

## Audio asset gap — not a completed sound pass

Project Assets, local `Downloads/Ryders Block Assets`, and all 19 archives were
searched. No production gameplay audio was found. Unrelated downloaded music
and speech were not used. Evidence: `Logs/phase091-audio-search.txt`.
The supplied video has a music track; loading playback is intentionally muted,
with its visual derivative retained. No placeholder tones are generated.

`GameplayAudioProfile` needs licensed production clips for:

| Role | Hook / intended asset |
| --- | --- |
| Jump | Jump; short airy takeoff |
| Light/hard landing | Landing / HardLanding; two distinct stone impacts |
| Momentum | Wind loop; optional HighSpeed onset |
| Pickup | Pickup; bright, short crystal collect |
| Boost | Boost; directional energy ignition |
| Crumble | CrumbleWarning / CrumbleCollapse; stress and falling debris |
| Ferry | MovingMechanism; seamless mechanical energy loop |
| Restore | Restore / RestorePoint; reconstruction and checkpoint activation |
| Patch | Patch; strongest short repair resolution |
| Rank/PB | Rank / Diamond / PersonalBest; distinct improvement accents |
| Menu | Ui / Retry; restrained glass/select and restart |
| Other existing roles | Fall, Split, Water; optional BiomeAmbience loop |

One-shot, loop, event and native haptic paths exist. Missing clips remain silent.
Actual audio quality therefore remains an explicit production gap under the
user-authorized missing-asset fallback, not a passed Gold audio gate.

## Product flow

The supplied 1280×720/24 fps/11 s film is integrated as a compact H.264 derivative
in `UI/Resources/Loading`. A matching first-frame poster appears immediately;
video becomes visible only after a decoded frame, loops during longer loads,
and stops/releases its clip after reveal. FitInParent preserves landscape aspect
with blue side areas; error/slow preparation holds the poster. Shared loader
start/finish events own one persistent overlay. Minimum poster presentation is
0.3 s plus a 0.2 s reveal, not an 11 s forced wait. Unity splash is disabled in
project settings; Android process/cold-start flash behavior still requires QA.

Main menu uses supplied branded floating-world artwork, a cyan/navy glass action
panel and one orange Campaign CTA. Campaign shows numbered world/title cards,
real locks, completion/PB, count fixed and normal Bronze progression. It pages
three cards at a time for future eight-module content without publishing fake
levels. Spiral is a separate challenge entry. QA practice appears only in dev.
Settings preserve sensitivity, FOV, Classic/Easy, audio and haptics controls.

HUD distinguishes the large neutral timer from smaller target/PB values and a
quieter rank strip; transient shard and ahead-of-PB split feedback stay below it.
Results emphasize rank/time/PB/next-gap and optional shard percentage, reveal
quickly, prevent duplicate MODULE FIXED text and keep Retry/Next accessible.
Final Campaign module offers Campaign return without duplicate selection buttons.
The run menu includes a normal Restore action for exploration of lower ruins.
Development toolbar/build text are compile-guarded; release does not instantiate
them. Unity's Development Build watermark belongs to development builds only.

## Performance and QA limits

37 static near mesh colliders, 41 near-place renderers, no new dynamic lights,
no post-process speed overlay, no FOV changes, maximum eight pooled emitters,
five small animated collectibles, and one 720p video decoder only during loading.
The original movie is ~19.8 MB; derivative size is recorded in handoff. Unseen
selector geometry was removed. These are structural budgets, not measured S23
60 FPS/thermal evidence. Native haptic output and movie decoding need the phone.

Representative five gameplay views plus menu, Campaign, loading, HUD and results
are in `Logs/Phase091VisualQA`. The mechanical tests are not a human Bronze run.
**Stop here for physical S23 QA.** Do not start the next campaign art phase until
Module 003 is physically accepted. Rank thresholds remain uncalibrated.

## Final build evidence

Source validator passed; EditMode 211/211 and PlayMode 11/11 passed.
Android ARM64 IL2CPP development build: 0 errors, 1 warning.
APK: `Builds/Android/RYDERS-ROAD-0.9.1-s23-truth-and-flow-dev.apk`
Size: 176,751,077 bytes. SHA-256:
`340D1EE2A1F81CB9E1ECAE461656D38A1B544C025220EF88705A6A3F7CB85AF3`.
Artifact metadata and native vibration permission verified with aapt.
No physical S23 testing was performed in this continuation.
