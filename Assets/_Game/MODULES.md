## 0.13.0 correction ownership

EditorTools owns explicit GameplayQualityAuthoring and the Windward v3 authoring
revision. Visuals owns optional per-profile prefab libraries and tunable Boost/
cloud presentation. SaveSystem owns additive compatible PB/history records; UI
selects matching records and labels provisional ranks. Accepted Player/Input/
Camera systems are unchanged. See ADR 0026 and Docs/PRODUCTION_0130.md.

## 0.9.9 physical feedback and Campaign Flow trial

Levels owns removable CampaignFlowTrial selection/session times; UI owns its menu, runner composition and no-save boundary. Worlds AuthoredSurface carries explicit non-route landing intent; Respawn SceneryLandingRecovery forwards qualifying contacts to the existing RestoreController. Player motor/Input/save implementations are unchanged. Normal Campaign is still 001–004; Spiral stays separate.

## 0.9.8 Solar Foundry

Campaign now orders 001 first-steps, 002 moving-parts, 003 flow-error and 004 solar-foundry. Legacy Spiral is separate. Solar Foundry v1 uses the existing ModuleRunner, motor, Restore, moving/crumbling blocks, FlowChallenge, Patch and progression services. The new biome enum value is appended, preserving existing numeric meanings. See Docs/PRODUCTION_098.md.

## 0.9.7 autonomous production continuation

The user's 2026-09-08 mission authorizes independent production beyond historical
STOP gates; candidate movement promotion still requires physical approval.
ADR 0019 and Docs/PRODUCTION_097.md record the isolated Flow Lab landing-view
comparison, ordinary precision room, session-best isolation, contact foley and
Ancient Abyss startup preservation. Campaign motor/input/profile/save/content
bytes are preserved. Current verification and artifact: Docs/AI_HANDOFF.md.
No physical approval, final audio mix or production freeze is claimed.
## 0.9.6 Movement Mastery ownership

Player owns the opt-in strategy and contact math; Levels owns FlowLabSession and
training JSON; UI reuses MovementLab player/touch composition. Campaign, saves and
module IDs remain intact. See ADR 0018 and Docs/MOVEMENT_MASTERY_SLICE.md.

<!-- Current follow-up: 0.9.5-startup-course-clearance -->

0.9.5 addresses physical startup/UI and course intersections. See
Docs/S23_095_FIRST_THREE_FEEDBACK.md and ADR 0017. Module 002 v3 / 003 v7 retain
stable IDs and the unchanged motor; whole-path clearance supplements top-ray and
endpoint traversal tests. Native startup uses the approved loading poster.

# Module folders

## 0.9.4 Mountain World authority

EditorTools owns MountainWorldProductionAuthoring and Mountain094 baked assets; Worlds owns near collision intent; Visuals owns the isolated mountain environment and lightweight waterfall shader. Levels reuses FlowChallenge. SaveSystem derives eligibility; UI presents Continue and primary Next. Module 001 and 003 production content is preserved.

## 0.9.3 Module 001 production

EditorTools owns SkyCityProductionAuthoring and baked SkyCity093 assets. Worlds retains BiomeWorldObject/AuthoredSurface ownership. Visuals owns optional sky/fog profile overrides. UI only resolves these optional presentation settings; movement and camera controls remain unchanged. See Docs/S23_093_SKY_CITY.md.

## 0.9.1 modules

Worlds owns AuthoredSurface and explicit collision intent. Levels owns attempt-local FlowChallenge/Profile/Pickup. Audio owns GameFeelProfile, native AndroidGameplayHaptics, MomentumPresentation and MechanismAudio. UI owns LoadingPresentation and frontend/results. No runtime-to-Editor dependency and no movement solver changes. Module 003 content version 6; see Docs/S23_091_CONTINUATION.md.


Folders mirror product capabilities so later phases can grow without moving
unrelated files. Runtime code belongs in the assembly that owns its behavior;
reusable contracts belong in Core.

Phase 2 implemented the first real parkour module layer. Phase 3 adds the
first saved ranking/progression loop. Phase 3B adds the first reusable visual
vertical-slice layer:

- `Levels`: `ModuleDefinition`, module validation, selection state, Patch
  Block, shortcut triggers, camera guide metadata, and the generated
  `ModuleSelector`/`ModuleRunner` scenes.
- `Blocks`: reusable moving, jump-boost, water-flow, and crumbling block
  components plus reset contracts.
- `UI`: development module selector, module scene adapter, touch controls, and
  module HUD.
- `Timing`: run timer, current-run splits, module run results, and local
  module-attempt telemetry reports.
- `Ranking`: rank, run-validity, and deterministic score logic.
- `SaveSystem`: V3 progression records and V2-to-V3 migration.
- `Visuals`: module visual/environment profiles, URP-safe runtime materials,
  sky/atmosphere colors, mechanic color language, gloves, HUD colors, and
  visual-slice quality/density controls.

Module 003 content version 5 is the hand-authored Ancient Abyss gameplay slice.
Its Bronze route travels through Arrival Sanctuary, Broken Crossing, Collapsed
Temple, Energy Spine, and Patch Sanctum. Substantial collider-free foundations
embed the diagonal Moving ferry, Crumble court, gravity-validated Boost, and
final ascent in the world without changing gameplay collision. Physical S23
acceptance remains pending.

Reserved folders with only `.gitkeep` files remain reserved, not invitations
to add placeholders. Do not add future systems merely to make a folder nonempty.

Production 0.9.0 adds a focused Audio profile and feedback event boundary; see
`Docs/ASTRA_PRODUCTION.md`. Campaign successor lookup is explicit. Module 003
keeps gameplay content version 5; final art and physical acceptance are open.
