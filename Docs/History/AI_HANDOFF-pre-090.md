# RYDER'S ROAD Authoritative Handoff

Updated: 2026-09-01 | Current build: `0.7.7-camera-recovery-platform-visual-truth` |
Unity: `6000.5.6f1` |
Package: `com.rydersblockstudio.rydersblock`

This is the model-neutral continuation file. Keep
`Docs/AI_HANDOFF_CODEX.md` and `Docs/AI_HANDOFF_GEMINI.md` as historical
records; where they conflict with this file or current runtime evidence, this
file and current user feedback win.

## Phase 0.7.7 Camera Recovery And Platform Visual Truth

- Physical 0.7.6 feedback rejected fixed touch pitch and the added platform
  cap geometry. `FirstPersonCameraRig` now consumes manual vertical touch look
  globally. Flow steering keeps left-thumb yaw and ignores right-side
  horizontal camera input; Classic/editor behavior remains available.
- Fresh gameplay starts at `+9` degrees because positive Unity X pitch looks
  downward. Limits are `-75` up / `+75` down. Vertical FOV remains `94`, camera
  height remains `1.62 m`, near clip remains `0.04 m`, and movement/capsule
  values are unchanged.
- Right-side Tap Jump uses the existing classifier: `0.16 s` maximum tap,
  `0.035` normalized tap tolerance, and `0.014` normalized drag activation.
  Camera drag commits before emitting look delta and never jumps on release.
- Fall Restore preserves recent pitch and pre-death yaw. Fresh spawn/manual
  restart use authored yaw and default pitch. `ResetView` now applies pitch to
  the transform immediately instead of waiting one frame.
- The global `Collider Truth Readability Shell` and its generated mesh were
  deleted. Canonical Ryder block prefabs render their own textured geometry;
  root primitive colliders and authored scales remain the gameplay truth.
- `PlayerGroundingCue` remains: it is cheap, edge-clipped, height-faded, and
  complements rather than replaces manual underfoot viewing. Arms remain hidden.
- Module 003 alone received restrained cleanup: the oversized near-summit
  tower decoration was removed, two secondary ruins moved farther/down, and
  existing mist layers were darkened/strengthened without new assets or VFX.
- Validation: source/foundation passed; EditMode `193/193`; PlayMode `6/6`;
  Android ARM64 IL2CPP build succeeded with one legacy-icon warning and zero
  errors.
- APK:
  `Builds/Android/RYDERS-ROAD-0.7.7-camera-recovery-platform-visual-truth-dev.apk`,
  172,100,080 bytes, SHA-256
  `80356AF9566E932BF7CC3E885F42EEFE9E00E643252185AAEAD79D34FF35E43A`.
- No ADB query, install, launch, or physical acceptance was performed. S23
  approval of pitch, Tap Jump discrimination, platform appearance, Module 003,
  and sustained performance is required before the postponed VFX phase.

## Phase 0.7.6 Global Readability And Module 003 Art Pass

- Poor underfoot visibility was caused primarily by neutral `0` degree touch
  pitch plus hidden arms and no world-space body cue. Unity's `94` camera value
  is vertical FOV; increasing it would further shrink platforms. Global normal
  gameplay now uses a profile-owned fixed `-9` degree pitch and retains `94`
  FOV. The believable `1.62 m` camera height and `0.04 m` near clip are unchanged.
- `PlayerGroundingCue` renders one mobile-cheap soft contact shadow. A center
  ray plus a 16-segment support ring clips unsupported wedges at platform
  edges, fades with height, and disappears without a valid `Ground` surface.
- Prefab-backed gameplay blocks now receive one shared three-submesh collider
  truth shell for readable top, edge band, and dark underside. Gameplay
  colliders and movement are unchanged.
- The shared camera, grounding cue, hidden-arm state, and surface shell resolve
  in Module 001, Module 002, Module 003, Spiral, and future ModuleRunner content.
  Restore still preserves pre-death yaw and reapplies the fixed pitch.
- Module 003 alone was recomposed into four background groups: one vertical
  broken-temple hero, a lower abyss tower group, a mid-world ruin island, and a
  broken sanctuary. Route-adjacent art is limited to four low foundations.
- Ancient Abyss mist-disc triangle winding was corrected to face the player;
  its six soft layers now form the intended visible deep mist ocean. The hero
  and abyss tower intersect atmospheric layers while open sky and route
  contrast remain dominant.
- Validation: source and foundation passed; EditMode `190/190`; PlayMode
  `6/6`; Android ARM64 IL2CPP development build succeeded with one legacy-icon
  warning and zero errors.
- APK:
  `Builds/Android/RYDERS-ROAD-0.7.6-global-readability-module003-art-pass-dev.apk`,
  172,131,236 bytes, SHA-256
  `62829520B4339ED7A00537E1EA21A579B2DDC803DABBE99D1209635A5DD382F8`.
- By phase instruction, no ADB query, pairing, install, launch, screenshot, or
  physical acceptance was performed for 0.7.6. S23 camera feel, edge read,
  shadow naturalness, Module 003 art, sustained FPS, and artifacts remain the
  user checklist.

## Phase 0.7.5 Module 003 Gold-Standard Vertical Slice

- **Strategy Paradigm Applied**: "SYSTEMS ARE GLOBAL. CONTENT POLISH IS ONE LEVEL AT A TIME."
- **Scope Discipline**: Campaign Module 003 (Flow Error / Ancient Abyss) is the only level receiving gold-slice content polish. Modules 001, 002, and The Spiral are preserved in their existing visual state except for reusable global systems.
- **Root Cause Diagnosed**: The previous Module 003 pass let generated large landmarks and authored side decorations sit too close to the playable route, and biome world objects had no explicit depth-band metadata. This made background architecture read as near walls instead of distant scale.
- **Global Systems Added**:
  - `EnvironmentBiomeProfile` now supports `BiomeDepthBand` metadata, per-object depth fade, and subtle mist-layer motion data.
  - `ModuleSceneController` applies depth-band atmospheric tint/fade, creates Module 003-only mobile-safe lighting/post settings, and adds lightweight Ancient Abyss dust/landing/water feedback.
  - `RuntimeWorldDrift` gives selected mist layers slow visual motion without physics or gameplay interaction.
  - `FirstPersonArmProfile.ShowFirstPersonArms` temporarily hides first-person arm rendering while preserving all Ryder Arm V2 assets, rig data, fallback references, and profile tuning.
  - `GameConfiguration.DeveloperTelemetryEnabled` hides diagnostics by default while keeping DIAG cycling available.
- **Module 003 Content Polish**:
  - Hero temple moved far/down across the abyss at `(68, -42, 96)` with `FarWorld` depth treatment.
  - Tower and monolith silhouettes moved into `LowerAbyss`; celestial arch moved off-route/far; mid-world island, aqueduct, and sunken spire provide secondary compositions.
  - Near-route authored decorations are reduced and placed as modest below-platform supports instead of player-height walls.
  - Ancient Abyss now has six continuous mist-ocean layers, including a deep deck, horizon veil, summit depth, and sparse drifting wisps.
- **Frozen Mechanics**: Movement solver, 94 FOV gameplay camera authority, input, colliders, Restore/Patch, rank thresholds, progression, save schema, and stable IDs remain unchanged.
- **Validation**:
  - Source validator: passed (`Logs/phase075-gold-source-validator-final.log`).
  - Foundation Project Validator: passed (`Logs/phase075-gold-foundation-validator.log`).
  - EditMode Tests: 188/188 passed (`Logs/phase075-gold-editmode-results.xml`).
  - PlayMode Tests: 6/6 passed (`Logs/phase075-gold-playmode-results.xml`).
  - Android IL2CPP ARM64 Development Build: succeeded with one warning and zero errors (`Logs/phase075-gold-android-build.log`).
  - APK: `Builds/Android/RYDERS-ROAD-0.7.5-module003-gold-standard-vertical-slice-dev.apk` (172,055,830 bytes, SHA-256: `0B95C62BAD98183D5E8AC20019466142189F57C724FE6B6FAEE2EAF05F8D4375`).
  - PHONE NOT CONNECTED by instruction. Human physical-device acceptance pending.

## Phase 0.7.4 Global World System Recovery & Module 003 Ancient Abyss Gold-Standard Pass

- **Strategy Paradigm Applied**: "SYSTEMS ARE GLOBAL. CONTENT POLISH IS ONE LEVEL AT A TIME."
- **Scope Discipline**: Campaign Module 003 (Flow Error / Ancient Abyss) is the ONLY level receiving full production-quality environmental authoring. Modules 001, 002, and The Spiral are preserved in their working states.
- **Root Cause Fixes**:
  - Legacy `CreateDistantFragments()` disabled when `EnvironmentBiomeProfile` is active on a module.
  - Legacy primitive fallback dressers (`CreateTowerCore`, `CreateFloatingIsland`, `CreateVegetation`, `CreateCloudCluster`) retired from active level generation in favor of authored landmark prefabs and biome world objects.
  - Spheroid mist masses replaced with a mobile-safe continuous multi-tier mist ocean using procedural radial disc meshes and smooth radial gradient texture (`TEX_RR_Mist_RadialSoft.png`).
- **Composed Landmarks Created**:
  - `PF_RR_Landmark_AncientTempleComplex`: Stepped ivory foundation, fluted colonnade, broken shrine walls, and glowing cyan runes.
  - `PF_RR_Landmark_ColossalAbyssTower`: Multi-tiered colossal ancient spire plunging deep into the mist ocean.
  - `PF_RR_Landmark_CelestialBrokenArch`: Grand broken celestial arch spanning the upper ascent.
  - `PF_RR_Landmark_FloatingRuinIsland`: Natural rock island, ivory ruin fragments, and stylized vegetation.
  - `PF_RR_Landmark_DistantSunkenMonolith`: Colossal silhouette monolith for horizon scale.
  - `PF_RR_Landmark_AncientCentralSpire`: Multi-tier ruin core anchoring the spiral climb.
  - `PF_RR_Landmark_AqueductButtress`: Arched masonry and rock buttress supporting the water run.
- All environmental landmarks are strictly collider-free and static batched.
- Movement physics, POV arm camera, animations, HUD, and controls remain 100% frozen and untouched.
- Validation:
  - Foundation Project Validator: Passed (`Logs/phase074-validate.log`).
  - EditMode Tests: 187/187 Passed (`Logs/phase074-editmode-results.xml`).
  - PlayMode Tests: 6/6 Passed (`Logs/phase074-playmode-results.xml`).
  - Android IL2CPP ARM64 Development Build: Succeeded (`Logs/phase074-android-build.log`).
  - APK: `Builds/Android/RYDERS-ROAD-0.7.4-global-world-system-module003-ancient-abyss-dev.apk` (172,021,084 bytes, SHA-256: `EF47FD7E6DD03A454A80420A54A39B8574573FBC47535C0EF6F81752878CDBC4`).
  - PHONE NOT CONNECTED by instruction. Human physical-device acceptance pending.

## Phase 0.7.3 Campaign Recovery, Biomes, And World Assets

- Campaign is again the implementation target. Normal Campaign remains the
  explicit stable-ID chain `module.001.first-steps`,
  `module.002.moving-parts`, `module.003.flow-error`; `module.004.the-spiral`
  remains separate Spiral mode and is not in the campaign unlock list.
- Build ID is now `0.7.3-campaign-recovery-biomes-and-world-assets`.
- Added `EnvironmentBiomeProfile` under `Assets/_Game/Worlds` and module
  references on `ModuleDefinition`. `ModuleSceneController` renders biome
  world objects and a distant lower mist ocean as visual-only objects after
  environment setup and before authored module decorations. Gameplay geometry,
  movement, camera, Restore, Patch, ranks, PB, and save schema are unchanged.
- Imported 29 curated Kenney CC0 FBX models into
  `Assets/_Game/Art/ThirdParty/Kenney`, prepared 29 collider-free background
  prefabs under `Assets/_Game/Art/Environment/WorldAssets`, and adapted their
  materials toward Ryder's Road navy/cyan/stone/vegetation palettes. Full
  license/source records are in `Docs/THIRD_PARTY_ASSETS.md`.
- Created reusable biome assets:
  `Biome_SkyCity`, `Biome_MountainSky`, `Biome_AncientAbyss`, and
  `Biome_EnergyVoid`.
- Applied Campaign biome assignments:
  Module 001 -> Sky City, Module 002 -> Mountain Sky, Module 003 -> Ancient
  Abyss. Energy Void exists for later/harder environments but is not assigned
  to normal Campaign in this pass.
- The lower world now uses authored large flattened sphere mist masses far
  below route height. No gameplay-adjacent fog cards, slabs, black planes, or
  platform-shaped mist objects were added.

### Campaign Inventory

| Module | Source | Unlock | Ranks | PB/ranks | Restore/Patch | Mechanics | Biome/status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `module.001.first-steps` / Campaign 01 - First Steps | `Assets/_Game/Levels/Resources/Modules/Module_001_FirstSteps.asset`, scene `ModuleRunner` | Fresh save first module | Silver 42s, Gold 32s, Diamond 26s; Bronze any valid completion | V4 module progress/PB supported | Restore `restore.module-001.midpoint`; Patch `patch.module-001` | Standard, Precision, Restore, Patch | Sky City. Uses canonical block visuals plus distant city/mist background; no moving/crumble/boost/surf placeholders. |
| `module.002.moving-parts` / Campaign 02 - Moving Parts | `Assets/_Game/Levels/Resources/Modules/Module_002_MovingParts.asset`, scene `ModuleRunner` | Bronze/valid completion of Module 001 | Silver 62s, Gold 48s, Diamond 38s; Bronze any valid completion | V4 module progress/PB supported | Restore `restore.module-002.branch`; Patch `patch.module-002` | Standard, Precision, Moving, Shortcut, Restore, Patch | Mountain Sky. Moving blocks `moving.m02.first` and `moving.m02.final`; no crumble/boost/surf placeholders. |
| `module.003.flow-error` / Campaign 03 - Flow Error | `Assets/_Game/Levels/Resources/Modules/Module_003_FlowError.asset`, scene `ModuleRunner` | Bronze/valid completion of Module 002 | Silver 90s, Gold 68s, Diamond 52s; Bronze any valid completion | V4 module progress/PB supported | Restores `restore.module-003.water-exit`, `restore.module-003.upper-spiral`; Patch `patch.module-003` | Standard, Precision, Moving, Boost, FlowingWater, Crumbling, Shortcut, Restore, Patch, CameraGuide | Ancient Abyss. Uses two one-second crumble blocks, lower water flow, one boost, one moving block, and ruin/mist biome background. |

Functional progression audit:

- `ModuleProgressionData` still derives normal Campaign unlocks from previous
  module completion. Any valid completion records at least Bronze; Silver,
  Gold, and Diamond remain mastery only.
- `ModuleSelectionState.GetCampaignModuleIds()` still returns only Modules
  001-003. Spiral remains `ModuleSelectionState.SpiralModuleId` and does not
  unlock as part of the normal chain.
- `ModuleSceneController.OnNextModule` selects the next stable ID only within
  that Campaign array, so Module 003 returns to Module Select and Spiral does
  not become "next campaign".
- Retry reselects the current module and reloads `ModuleRunner`; it does not
  mutate progression except normal attempt/run flow.

Validation:

- Source validator passed:
  `Logs/phase073-campaign-biomes-source-validator-final.txt`.
- Foundation validator passed:
  `Logs/phase073-foundation-validator.log`.
- EditMode passed `186/186`:
  `Logs/phase073-campaign-biomes-editmode-results.xml`.
- PlayMode passed `6/6`:
  `Logs/phase073-campaign-biomes-playmode-results.xml`.
- Android IL2CPP ARM64 development build succeeded with one warning and zero
  errors:
  `Logs/phase073-campaign-biomes-android-build.log`.
- APK:
  `Builds/Android/RYDERS-ROAD-0.7.3-campaign-recovery-biomes-and-world-assets-dev.apk`,
  171,974,630 bytes, SHA-256
  `4A87EF727E18DCC25BD95CD66E40369388FB849C0CB47B009A946F001726C1F2`.
- PHONE NOT CONNECTED by instruction. No ADB device search, wireless pairing,
  install, launch, screenshot, or physical gameplay test was attempted.
- USER PHYSICAL ACCEPTANCE PENDING.

## Phase 0.7.3 Surgical Arm Presentation Recovery

- Surgical recovery pass after user feedback identified the 0.7.2 arm
  presentation as a visual regression: too low, too hidden, and not reading as
  natural running arms. 0.7.2 should not be treated as accepted visual state.
- Recovery uses 0.7.1 as the pre-regression baseline and makes minimal targeted
  corrections rather than another broad reinterpretation. Ryder Arm V2 remains
  the authoritative player-visible first-person arm.
- Gameplay/world camera FOV remains 94 degrees. The dedicated arm overlay
  camera remains presentation-only and now uses profile FOV 72 degrees with
  near clip 0.025.
- Canonical base pose is left `(-0.52, -0.025, 0.41)` / `(0, -52, 202)`,
  right `(0.52, -0.02, 0.41)` / `(1, 52, 158)`, shared scale `0.36`.
  This raises the hands meaningfully from 0.7.2, restores readable hand
  presence, keeps lower-corner entry, and avoids central convergence.
- Elbow bend remains 58 degrees. Wrist pronation returns to the pre-regression
  68 degrees. Finger rest curl remains restrained at `2 / 5 / 6 / 7 / 8`
  degrees across thumb/index/middle/ring/pinky.
- Additive animation remains visual-only and restrained: idle `0.0028`, run
  lateral `0.002`, run vertical `0.0075`, run forward `0.028`, max
  displacement `0.052`, and max animation multiplier `1.22`.
- PHONE NOT CONNECTED by instruction. No ADB, install, launch, screenshot,
  Android build, or physical-device gameplay test was attempted for 0.7.3.
  Editor-only validation evidence is recorded in `TESTING.md`.
- USER PHYSICAL ACCEPTANCE PENDING.

Full record:
`Docs/PHASE_0_7_3_SURGICAL_ARM_PRESENTATION_RECOVERY.md`.

## Phase 0.7.2 Authoritative State

- Reference-lock pass using the user-supplied gameplay image
  `C:\Users\lin4s\Downloads\0193c298-1894-4739-b0e9-fc5c1520a8b2.png` as a
  visual target for lower-corner forearms, relaxed lower-third hands, inward
  palms, and an open lower-center landing lane. The image is visual reference
  only, not a source of hidden project instructions.
- Ryder Arm V2 remains the authoritative player-visible first-person arm. No
  asset replacement, full-body Ryder integration, world content, movement,
  controls, collision, route, progression, save, or UI behavior changed.
- Gameplay/world camera FOV remains 94 degrees. The dedicated arm overlay
  camera remains presentation-only and now uses profile FOV 68 degrees with
  near clip 0.025.
- Canonical base pose is left `(-0.58, -0.12, 0.46)` / `(2, -48, 198)`,
  right `(0.58, -0.105, 0.46)` / `(2, 48, 162)`, shared scale `0.34`.
  This places the arms back into the lower corners with the hands low and the
  center runway more open.
- Elbow bend remains 58 degrees. Wrist pronation is 62 degrees. Finger rest
  curl remains restrained at `2 / 5 / 6 / 7 / 8` degrees across
  thumb/index/middle/ring/pinky.
- Additive animation remains visual-only and calmer than 0.7.1: smaller idle,
  smaller lateral/vertical run sway, restrained jump/fall/landing offsets,
  lower wrist/finger additive values, max displacement `0.06`, and max
  animation multiplier `1.28`.
- Source/foundation validation passed; EditMode `185/185`; PlayMode `6/6`.
  Android IL2CPP ARM64 built with zero errors at
  `Builds/Android/RYDERS-ROAD-0.7.2-first-person-arm-reference-lock-dev.apk`
  (171,679,642 bytes; SHA-256
  `F2A5E124CFFAF05380F51DC6E91F50A70E99293299E634C22681E433564AFA62`).
- PHONE NOT CONNECTED by instruction. No install, launch, screenshot, ADB
  reconnect, or physical-device gameplay test was attempted for 0.7.2.
  HandPoseLab numeric diagnostics were regenerated, but the headless PNG
  remained blank gray and is not visual acceptance evidence.
- USER PHYSICAL ACCEPTANCE PENDING.

Full record:
`Docs/PHASE_0_7_2_FIRST_PERSON_ARM_REFERENCE_LOCK.md`.

## Phase 0.7.1 Authoritative State

- Final planned first-person POV arm tuning pass. Ryder Arm V2 remains the
  authoritative player-visible first-person arm, with no asset replacement,
  full-body Ryder integration, world content work, movement changes, or control
  changes.
- Gameplay/world camera FOV remains 94 degrees. The dedicated arm overlay
  camera remains presentation-only and now uses profile FOV 78 degrees, chosen
  from the requested 76/78/80 comparison range to reduce 0.7.0 arm dominance
  while avoiding the earlier wide-FOV stretching.
- Canonical base pose moved outward/lower/back for landing visibility: left
  `(-0.47, 0.018, 0.375)` / `(0, -52, 202)`, right
  `(0.47, 0.026, 0.385)` / `(1, 52, 158)`, scale `0.36`.
- Elbow bend remains 58 degrees. Wrist pronation was softened from 75 to
  68 degrees. Finger rest curl remains restrained at `2 / 5 / 6 / 7 / 8`
  degrees across thumb/index/middle/ring/pinky.
- Animation remains additive and visual-only. Retuning reduces lateral,
  vertical, jump, fall, landing, wrist, and finger additive amplitudes; run
  motion stays alternating fore/aft, while lateral run offset only opens
  outward from center instead of pulling both hands inward.
- Source/foundation validation passed; EditMode `185/185`; PlayMode `6/6`.
  Android IL2CPP ARM64 built with zero errors at
  `Builds/Android/RYDERS-ROAD-0.7.1-first-person-arm-framing-and-animation-lock-dev.apk`
  (171,681,386 bytes; SHA-256
  `110C5E1614DAE8599DAC9EB727C641D567E45D494C4052076A19B7007E5E527B`).
- PHONE NOT CONNECTED. No install, launch, screenshot, or physical-device
  gameplay test was attempted for 0.7.1. HandPoseLab numeric diagnostics were
  regenerated, but the headless PNG remained blank gray and is not visual
  acceptance evidence.
- USER PHYSICAL ACCEPTANCE PENDING.

Full record:
`Docs/PHASE_0_7_1_FIRST_PERSON_ARM_FRAMING_AND_ANIMATION_LOCK.md`.

## Phase 0.7.0 Authoritative State

- Ryder Arm V2 remains the only active player-visible first-person arm.
  The 0.7.0 change moves it to a dedicated `FirstPersonArms` render layer and
  child overlay camera named `First Person Arm Camera`.
- Gameplay/world camera FOV remains 94 degrees. The arm overlay camera uses
  profile-driven FOV 70 degrees and near clip 0.025 so hand scale can be tuned
  without changing movement view, yaw, routes, collision, or landing timing.
- Runtime hierarchy is world camera -> arm camera -> `FirstPersonVisualRoot`
  -> left/right anchors -> V2 prefabs. The world camera excludes
  `FirstPersonArms`; the overlay camera renders only that layer and is stacked
  through URP camera data. No duplicate `AudioListener` is created.
- Canonical animated base pose: left `(-0.42, 0.05, 0.41)` /
  `(0, -60, 205)`, right `(0.42, 0.06, 0.42)` / `(1, 60, 155)`, scale
  `0.36`; true elbow `Bone_003` bends 58 degrees, wrist roll `Bone_002`
  remains mirrored 75 degrees, and digit rest curl is restrained.
- `NeutralPoseLocked` is now false. First-person arm animation is visual-only:
  subtle idle drift, alternating run swing, jump/rise/fall offsets, landing
  compression, and small wrist/finger additive poses. It never writes movement,
  camera aim, physics, collision, input, route, progression, or save state.
- Source/foundation validation passed; EditMode `184/184`; PlayMode `6/6`.
  Android IL2CPP ARM64 built with zero errors at
  `Builds/Android/RYDERS-ROAD-0.7.0-first-person-arm-camera-and-animation-lock-dev.apk`
  (171,677,950 bytes; SHA-256
  `0BC88239B6CF4B1E0FB6FC1E42CB60D791467B7C468F655482FE58182484BFF7`).
- ADB was available, but no Android device was connected during this pass, so
  S23 smoke and human physical acceptance remain pending. HandPoseLab numeric
  axis output updated, but the headless batch PNG was blank gray in this
  environment and is not visual acceptance evidence.
- USER PHYSICAL ACCEPTANCE PENDING.

Full record: `Docs/PHASE_0_7_0_FIRST_PERSON_ARM_CAMERA_AND_ANIMATION_LOCK.md`.

## Phase 0.6.9 Authoritative State

- Ryder Arm V2 is the only active player-visible first-person arm. The prior
  Meshy rig and approved unrigged arms remain referenced as inactive fallbacks.
- Source-right and baked X-mirrored left prefabs use positive hierarchy scales,
  11,953 vertices, 15,541 triangles, 24 joints, generated tangents, and one
  recovered URP material with base, normal, and emissive maps.
- The clean runtime hierarchy is camera -> `FirstPersonVisualRoot` -> named
  left/right anchors -> V2 prefab. No colliders, rigidbodies, sleeves, or
  movement authority were introduced.
- Static runner pose: left `(-0.60, -0.16, 0.48)` / `(0, -60, 205)`, right
  `(0.59, -0.14, 0.50)` / `(1, 60, 155)`, scale `0.38`; true elbow
  `Bone_003` bends 45 degrees, wrist roll `Bone_002` is mirrored 75 degrees,
  and all five digit chains have restrained relaxed curl.
- `NeutralPoseLocked` remains true and all runtime arm locomotion remains off.
  Movement, camera, controls, collision, world, UI, progression, saves, and
  Android landscape contracts are unchanged.
- Source/foundation validation passed; EditMode `183/183`; PlayMode `6/6`.
  Android IL2CPP ARM64 built with zero errors at
  `Builds/Android/RYDERS-ROAD-0.6.9-ryder-arm-v2-full-replacement-dev.apk`
  (171,648,490 bytes; SHA-256
  `49F7D83940545347E3C08CE519E0ED532C0AE76EE7C85C6BCF396999BAC7C0D0`).
- Bounded S23 smoke installed, launched, and loaded Spiral fullscreen at
  2340x1080 with both V2 arms visible and diagnostics at `FPS 60`; crash scan
  was clean. Evidence is listed in `TESTING.md`.
- USER PHYSICAL ACCEPTANCE PENDING.

Full record: `Docs/PHASE_0_6_9_RYDER_ARM_V2_FULL_REPLACEMENT.md`.

## Phase 0.6.8B Authoritative State

- Surgical rigged hand handedness/material recovery phase. It changes only the
  rigged first-person arm import, static rigged pose, material setup, asset
  validation tests, and documentation.
- No movement, physics, camera/FOV/yaw, controls, world content, fog/scenery,
  UI, progression, save schema, collision, route IDs, or locomotion animation
  state-machine behavior was intentionally changed.
- Handedness root cause: no runtime negative-scale mirror was found. The right
  prefab still uses the source rig/mesh, and the left prefab is still a baked
  X-mirrored mesh plus mirrored bone hierarchy with positive runtime scales.
  The perceived swap came from pose/presentation: over-curl plus roll made the
  hands read cupped, ambiguous, and clawed.
- Material root cause: the GLB had no authored tangents, and the glTF
  metallic/roughness texture was being fed into Unity as `_MetallicGlossMap`.
  That packed map is not Unity's metallic/gloss layout, which caused muddy,
  dark/scuffed mobile shading. 0.6.8B generates mesh tangents, imports textures
  by glTF role, keeps base color sRGB and data maps linear, leaves
  `_MetallicGlossMap` unset, restores backface culling, lowers metallic to
  `0.08`, smoothness to `0.42`, and normal strength to `0.35`.
- Canonical profile anchors remain left `(-0.46, -0.48, 0.48)`, right
  `(0.46, -0.48, 0.48)`, shared scale `(0.38, 0.38, 0.38)`, and root eulers
  left `(348.34290, 263.26410, 257.66420)`, right
  `(348.34290, 96.73593, 102.33580)`. `RigCorrectionRoot` remains
  position `(0, -0.08, 0.01)`, euler `(0, 0, 0)`, scale
  `(0.58, 0.42, 0.54)`.
- Prefab visual roll offsets are now left `(0, -70, 0)` and right
  `(0, 70, 0)` to show clean hand backs/fingers while avoiding the previous
  cupped/clawed read. Finger rest curl is reduced to `18` degrees and thumb
  rest curl to `8` degrees before per-chain/joint weights. Static wrist settle
  is smaller on `Bone_001` and `Bone_002`.
- `NeutralPoseLocked` remains true. Runtime idle/run/jump/fall/land/boost/water
  arm offsets and additive rig-bone locomotion remain disabled for this static
  approval baseline.
- HandPoseLab production render: `Logs/HandPoseLab/final.png`. Axis proof:
  finger-forward dot `0.9651`, inward-palm dot `0.1720`, and thumb-up dot
  `0.1374` on both sides. The low positive palm/thumb values reflect the
  flatter hand-back presentation. `Logs/HandPoseLab/orientation-report.txt`
  confirms `SkinnedMeshRenderer`, no negative scale, and a baked X-mirrored
  mesh pair.
- Validation: foundation validator passed; source validator passed in
  `Logs/phase068b-source-validator.txt`; EditMode `183/183`; PlayMode `6/6`;
  Android IL2CPP ARM64 build succeeded with zero errors.
- APK:
  `Builds/Android/RYDERS-ROAD-0.6.8b-hand-rig-handedness-and-material-recovery-dev.apk`,
  167,317,614 bytes, SHA-256
  `DE8D2AAA77C9CE62F0370D9C61921B3F6E04E23B9EA7B9FCC7F25FF1BEBD29C6`.
- Samsung Galaxy S23 `SM-S911B` (`RFCW100J1HW`) install/launch/Spiral smoke
  passed: the app installed, focused `com.unity3d.player.UnityPlayerActivity`,
  rendered fullscreen landscape at 2340x1080, showed the 0.6.8B title/menu
  after a tap, and loaded Spiral with both material-recovered rigged hands
  visible from the lower corners. Evidence files are
  `Logs/phase068b-s23-menu.png`, `Logs/phase068b-s23-spiral.png`,
  `Logs/phase068b-s23-spiral-log.txt`, and
  `Logs/phase068b-s23-spiral-window.txt`. Crash scan found no fatal
  Android/managed exception patterns.
- USER PHYSICAL ACCEPTANCE PENDING. Manual review should decide whether
  left/right handedness, shared material family, removed dark/scuffed shading,
  relaxed fingers, simple natural pose, framing, mirror artifacts, and
  sustained FPS are acceptable.

Full record:
`Docs/PHASE_0_6_8B_HAND_RIG_HANDEDNESS_AND_MATERIAL_RECOVERY.md`.

## Phase 0.6.8 Authoritative State

- Surgical rigged hand pose recovery phase. The rigged GLB path remains active
  through `FirstPersonArmPresentationMode.RiggedExperimental`, while the 0.6.6
  approved unrigged arm prefabs remain preserved as the fallback under
  `Assets/_Game/Art/FirstPerson/Arms/LegacyApproved`. This phase changes only
  first-person arm presentation.
- No movement, physics, camera/FOV, yaw, input controls, route geometry,
  collision, progression, UI, save schema, fog, scenery, or world composition
  rules were intentionally changed.
- Static pose lock is restored for the rigged path. `NeutralPoseLocked` is
  true; idle/run/jump/fall/land/boost/water anchor animation is set to zero;
  runtime finger/thumb/wrist additive animation is set to zero so the baked
  recovered pose is not double-curled.
- Recovered camera-space anchors: left position `(-0.46, -0.48, 0.48)`, right
  position `(0.46, -0.48, 0.48)`, shared profile scale `(0.38, 0.38, 0.38)`.
  The 0.6.6 root eulers remain left `(348.34290, 263.26410, 257.66420)` and
  right `(348.34290, 96.73593, 102.33580)`.
- Rigged prefab correction: `RigCorrectionRoot` position `(0, -0.08, 0.01)`,
  euler `(0, 0, 0)`, scale `(0.58, 0.42, 0.54)`. Prefab visual roll offsets
  are left `(0, -45, 0)` and right `(0, 45, 0)`.
- The imported GLB still has 1 mesh, 1 skin, 1 material, 3 textures, 23 joints,
  11,550 vertices, 15,400 triangles, and no authored animations. Fingers are
  baked into a relaxed rest curl: fingers `48` degrees, thumbs `20` degrees,
  with a small static wrist settle on `Bone_001` and `Bone_002`.
- Each rigged prefab now includes a same-material `ForearmEntrySleeve` under
  `RigCorrectionRoot/RiggedArm_*` to soften the bottom-frame arm-base cutoff.
  The sleeve is visual only, has no collider, and does not affect gameplay.
- HandPoseLab production render: `Logs/HandPoseLab/final.png`. Axis proof:
  finger-forward dot `0.9651`, inward-palm dot `0.5668`, thumb-up dot
  `0.5337` on both sides. `Logs/HandPoseLab/orientation-report.txt` confirms
  `SkinnedMeshRenderer`, no negative scale, and a baked X-mirrored mesh pair.
- Validation: source validator passed; EditMode `183/183`; PlayMode `6/6`;
  Android IL2CPP ARM64 build succeeded with one warning and zero errors.
- APK:
  `Builds/Android/RYDERS-ROAD-0.6.8-rigged-hand-pose-recovery-dev.apk`,
  167,455,364 bytes, SHA-256
  `A5185EDB2F743A3AE3907074E033D2D0210A1211E2D0ABFDF59D2A5CCCF8F9C7`.
- Samsung Galaxy S23 `SM-S911B` (`RFCW100J1HW`) install/launch/Spiral smoke
  passed: the app installed, focused `com.unity3d.player.UnityPlayerActivity`,
  rendered fullscreen landscape at 2340x1080, showed the 0.6.8 title/menu after
  a tap, and loaded Spiral with both recovered rigged hands visible from the
  lower corners. The diagnostics overlay reported `FPS 60`, `Ground True`, and
  `PARKOUR_RESPONSIVE_PHYSICAL_V2`. Evidence files are
  `Logs/phase068-s23-launch-after-tap.png`,
  `Logs/phase068-s23-spiral.png`, `Logs/phase068-s23-spiral-log.txt`, and
  `Logs/phase068-s23-spiral-window.txt`. A tight crash scan found no fatal
  Android/managed exception patterns.
- USER PHYSICAL ACCEPTANCE PENDING. Manual review should decide whether the
  relaxed pose, palm direction, thumb direction, bottom-corner entry, arm-base
  cutoff, subtle motion, and sustained FPS are acceptable.

Full record: `Docs/PHASE_0_6_8_RIGGED_HAND_POSE_RECOVERY.md`.

## Phase 0.6.7 Authoritative State

- Safe rig integration phase. The new rigged first-person arms are active
  through `FirstPersonArmPresentationMode.RiggedExperimental`, while the
  0.6.6 approved unrigged prefabs are copied into
  `Assets/_Game/Art/FirstPerson/Arms/LegacyApproved` and remain the fallback.
  The phase does not change movement, physics, camera/FOV, route content,
  collision, progression, UI, saves, or Android orientation contracts.
- Rig source:
  `C:\Users\lin4s\Downloads\Ryders Block Assets\Meshy_AI_Character_Arm Rigged.glb`.
  It is copied to
  `Assets/_Game/Art/MeshySource/FirstPersonRigged/Meshy_AI_Character_Arm_Rigged.glb`.
  Import is editor-only GLB decoding into Unity `Mesh`, `SkinnedMeshRenderer`,
  material, texture, and prefab assets; no runtime glTF dependency was added.
- GLB audit: glTF 2.0 from Blender I/O, 1 mesh, 1 skin, 1 material, 3 images,
  23 generic joints (`Bone_000` through `Bone_022`), 11,550 vertices, 15,400
  triangles, and no authored animations. Source textures include 8192 base
  color and 4096 normal/metallic-roughness images; Unity import caps runtime
  texture size to 2048 for mobile.
- Skinning import preserves all nonzero `JOINTS_0/WEIGHTS_0` and
  `JOINTS_1/WEIGHTS_1` influences through `Mesh.SetBoneWeights`. The source
  includes secondary weights on some vertices and up to seven influences per
  vertex. Automated import/tests found no broken weights, but physical
  deformation review is still pending.
- Rigged prefabs:
  `Assets/_Game/Art/FirstPerson/Arms/Rigged/PF_RR_FP_Arm_Rigged_Left.prefab`
  and
  `Assets/_Game/Art/FirstPerson/Arms/Rigged/PF_RR_FP_Arm_Rigged_Right.prefab`.
  The left side uses a baked mirrored mesh and mirrored bone hierarchy; runtime
  transforms remain positive scale.
- The canonical 0.6.6 camera-space anchors are preserved:
  left position `(-0.38, -0.40, 0.40)`, euler
  `(348.34290, 263.26410, 257.66420)`; right position
  `(0.38, -0.40, 0.40)`, euler `(348.34290, 96.73593, 102.33580)`; shared
  profile scale `(0.38, 0.38, 0.38)`. The rig prefab correction root is
  position `(0, -0.02, 0.02)`, euler `(0, 0, 0)`, scale `(0.48, 0.48, 0.48)`.
- `NeutralPoseLocked` is false again for this rigged phase. Runtime animation
  is visual-only camera-space presentation: subtle idle/run pump, jump/rise,
  fall brace, and landing recoil through arm anchors plus additive rig-bone
  wrist/finger motion. Classic manual camera, Editor controls, and movement
  solver remain untouched.
- HandPoseLab rendered the production hierarchy with rigged arms to
  `Logs/HandPoseLab/final.png`. Direction proof remains aligned with the
  approved 0.6.6 pose: finger-forward dot `0.9651`, inward-palm dot `0.9726`,
  thumb-up dot `0.9568` on both sides. Rendered visual acceptance is still a
  human review item, especially deformation, relaxed finger read, length, and
  motion feel.
- Validation: source validator passed; EditMode `183/183`; PlayMode `6/6`;
  Android IL2CPP ARM64 build succeeded with one warning and zero errors.
- APK:
  `Builds/Android/RYDERS-ROAD-0.6.7-safe-rig-integration-and-locomotion-dev.apk`,
  167,453,368 bytes, SHA-256
  `8AC7BB796A49B648C8AEF9E33163C4CEF08D0F9D3E268A58F53E3B6ED47BF922`.
- Samsung Galaxy S23 `SM-S911B` (`RFCW100J1HW`) install/launch smoke passed:
  the app installed, focused `com.unity3d.player.UnityPlayerActivity`,
  rendered fullscreen landscape at 2340x1080, and the captured gameplay frame
  showed the 0.6.7 diagnostics overlay with both rigged hands visible. Evidence
  files are `Logs/phase067-s23-launch-2.png`,
  `Logs/phase067-s23-launch-log-2.txt`, and
  `Logs/phase067-s23-window.txt`. The captured launch slice had no fatal
  Android/managed exception patterns.
- USER PHYSICAL ACCEPTANCE PENDING.

Full record: `Docs/PHASE_0_6_7_SAFE_RIG_INTEGRATION.md`.

## Phase 0.6.6 Authoritative State

- Hand-only pose-lock phase. Environment, fog, clouds, Spiral, scenery,
  platforms, movement, physics, camera/FOV, crumble, progression, and UI were
  not changed.
- Source orientation is now verified. Both prefab roots use positive identity
  scale. Their model children use local X `270` degrees and positive scale
  `100`; the source mesh long local-Z axis therefore maps to prefab-root local
  `+Y`. The left mesh is a baked X reflection of the right mesh with matching
  vertex order; there is no negative scale or runtime mirror. An editor-only
  endpoint render placed the `+Y` marker on both fingertip ends. Root local
  `-Z` is the palm normal; the semantic thumb side is left `-X`, right `+X`.
- Canonical camera-space hierarchy is `camera -> arm root -> prefab visual ->
  model child`. Prefab visual position/euler offsets are zero. Final arm-root
  transforms are:
  - Left position `(-0.38, -0.40, 0.40)`, euler
    `(348.34290, 263.26410, 257.66420)`.
  - Right position `(0.38, -0.40, 0.40)`, euler
    `(348.34290, 96.73593, 102.33580)`.
  - Shared prefab visual scale `(0.38, 0.38, 0.38)`.
- `FirstPersonArmProfile.NeutralPoseLocked` is authoritative and true.
  Production skips procedural hand events and `LateUpdate` pose offsets, so no
  run/jump/fall/land rotation is added after the canonical profile in this
  approval build.
- `HandPoseLab` rendered the exact production hierarchy at 94 FOV to
  `Logs/HandPoseLab/final.png` (2340x1080). Axis proof:
  finger-forward dot `0.9651`, inward-palm dot `0.9726`, thumb-up dot `0.9568`
  on both sides. Visual review answered yes for upward thumbs, opposing palms,
  forward fingers, diagonal forearms, implied bent elbows, separated wrists,
  inside-frame hands, balanced length, clear landing center, and removal of
  the flat zombie/reaching read. Finger spread is baked into the mesh and was
  intentionally not rigged.
- Validation: source passed; hand-focused EditMode `6/6`; PlayMode `6/6`;
  Android IL2CPP ARM64 build succeeded with one legacy-icon warning and zero
  errors.
- APK: `Builds/Android/RYDERS-ROAD-0.6.6-first-person-hand-pose-lock-dev.apk`,
  164,492,684 bytes, SHA-256
  `412DA9821A86B9BCD3685B10E085DF86D784747DB0CDDF88793B419A93010DD9`.
- Install/launch smoke was unavailable: ADB started successfully but listed no
  connected device on two checks. USER PHYSICAL ACCEPTANCE PENDING.

Full record: `Docs/PHASE_0_6_6_FIRST_PERSON_HAND_POSE_LOCK.md`.

## Phase 0.6.5 Authoritative State

- Recovery phase after 0.6.4. User reported broken interlocked/crossing hands,
  large dark depth slabs/planes, and cluttered world composition. This phase is
  surgical: no movement physics, route spacing, collision truth, crumble,
  Restore, rank, save, or control contract changes.
- First-person hands remain camera-space presentation only. Final local
  transforms are:
  - Left position `(-0.38, -0.40, 0.40)`, euler `(286.91, 36, 136)`, scale
    `(0.38, 0.38, 0.38)`.
  - Right position `(0.38, -0.40, 0.40)`, euler `(286.91, 324, 224)`, scale
    `(0.38, 0.38, 0.38)`.
  Run presentation was kept conservative: forward amount `0.022`, max
  displacement `0.085`, no new bob/mechanic system.
- The 0.6.4 runtime abyss slab path was disabled. `CreateEnvironment` no longer
  calls `CreateNullSpaceDepth`; the giant lower-gradient sky bands are inert,
  and the special abyss cloud veil helper was removed. Authored clouds now use
  the standard sphere-puff cloud cluster path.
- 0.6.4 filler clutter was removed: `m01.depth.*`, `m02.depth.*`,
  `m03.depth.*`, `m03.hero.*`, `m03.distant.*`, and
  `m04.world.depth.silhouette-*` are gone from assets and the regeneration
  factory. Spiral fog banks were scaled back to subtle authored cloud depth;
  the small red-glow depth accents remain visual-only.
- Validation: source passed; EditMode `182/182`; PlayMode `6/6`; Android
  IL2CPP ARM64 development build succeeded with one legacy-icon warning and
  zero errors.
- APK:
  `Builds/Android/RYDERS-ROAD-0.6.5-surgical-recovery-and-correct-hand-pose-dev.apk`,
  164,481,736 bytes, SHA-256
  `6A50AE3017EE8DCB7817BBD2621A1B4C1AEB0316EE392D8A3293BC901F44CF3D`.
- Samsung Galaxy S23 `SM-S911B` (`RFCW100J1HW`) install/launch smoke passed at
  2340x1080 fullscreen landscape. Evidence:
  `Logs/phase065-s23-title-2.png`,
  `Logs/phase065-s23-launch-log-2.txt`, and
  `Logs/phase065-s23-window-2.txt`. The captured launch slice had no fatal
  Android/managed exception patterns.
- USER PHYSICAL ACCEPTANCE PENDING. The device launch confirms rendering and
  focus only; human approval is still needed for final hand pose, no remaining
  dark rectangles from real play viewpoints, cleaner world read, full route
  traversal, sustained FPS, thermals, and pause/resume.

Full record: `Docs/PHASE_0_6_5_SURGICAL_RECOVERY.md`.

## Phase 0.6.4 Authoritative State

- User feedback after 0.6.3: hands still felt too flat/zombie-like, world
  composition still felt sparse/random, abyss fog risked reading as flat
  pancakes, and suspicious blue/special surfaces in non-Spiral areas still
  needed cleanup.
- First-person hands remain camera-space presentation only. The 0.6.4 profile
  uses left/right positions `(-0.30, -0.405, 0.34)` and `(0.30, -0.405, 0.34)`,
  eulers `(301.5, 25, 108)` and `(301.5, 335, 252)`, and scale `0.39`.
  Forward run sway and jump/fall/land displacements were calmed so the hands
  stay lower/closer and read more runner-ready.
- Water/flow trigger visuals are segmented into short filaments and side sparks
  instead of long blue sheets. Trigger/collision behavior is unchanged.
- Campaign 01/02/03 gained small, intentional depth clusters; Module 03 scenic
  towers/islands were reclustered so middle-distance props no longer stack or
  scatter as obviously. Spiral abyss fog is taller/deeper and has far-below
  silhouettes. These changes are visual-only and do not alter route supports.
- Validation: source passed; EditMode `182/182`; PlayMode `6/6`; Android
  IL2CPP ARM64 development build succeeded with one legacy-icon warning and
  zero errors.
- APK:
  `Builds/Android/RYDERS-ROAD-0.6.4-natural-hand-pose-and-world-composition-pass-dev.apk`,
  164,504,168 bytes, SHA-256
  `B5EB0C943C811707428ED52B19B7645D455E3A70C6AE09960558A505E44A48A2`.
- A later explicit install/run request succeeded on Samsung Galaxy S23
  `SM-S911B` (`RFCW100J1HW`): the title/menu reached 60 FPS in fullscreen
  landscape, focused `com.unity3d.player.UnityPlayerActivity`, and the captured
  launch slice had no fatal Android/managed exception patterns. This was launch
  smoke only, not physical gameplay acceptance.
- USER PHYSICAL ACCEPTANCE PENDING. Hand-pose feel, Module 03 water/flow
  readability, non-Spiral platform truth, world cohesion, Spiral abyss mood,
  full route traversal, sustained FPS, thermals, and pause/resume remain
  unapproved.

Full record: `Docs/PHASE_0_6_4_NATURAL_HAND_WORLD_COMPOSITION.md`.

## Phase 0.6.3 Authoritative State

- User feedback after 0.6.2: the hands were still rotated into a stiff
  inward/"zombie" pose, some blue rectangular surfaces in other maps looked
  landable but were non-solid, and the Spiral lower void still needed an
  ominous fog/red-glow depth read.
- First-person hands remain camera-space presentation only. The 0.6.3 base
  profile uses left/right positions `(-0.355, -0.365, 0.42)` and
  `(0.355, -0.365, 0.42)`, eulers `(286.91, 43.5, 140)` and
  `(286.91, 316.5, 220)`, and scale `0.38`. This points fingers more forward,
  keeps only slight inward/downward palms, moves both hands farther inside the
  screen, and keeps the right hand visible without gameplay collision.
- Collision truth was tightened for broad visual-only surfaces. Water/flow
  trigger volumes no longer render as broad blue slabs; they render as
  filaments, side ribbons, and vertical streaks. Broad decorative/visual-only
  platform impostors are converted to non-landable signal strips.
- `Module_004_TheSpiral` keeps route, Crumble, Restore, rank, save, stable ID,
  and movement meaning unchanged. Abyss dressing was moved deeper below the
  route with layered fog and vertical red glow shafts; these pieces are
  visual-only and intentionally non-landable.
- Validation: source passed; EditMode `180/180`; PlayMode `6/6`; Android
  IL2CPP ARM64 development build succeeded with one legacy-icon warning and
  zero errors.
- APK:
  `Builds/Android/RYDERS-ROAD-0.6.3-hand-pose-collision-truth-abyss-depth-dev.apk`,
  164,488,244 bytes, SHA-256
  `8604F583934CAB7A61984D8344D28947CD9043E0A484660AB863684DA0BA2C23`.
- Samsung Galaxy S23 `SM-S911B` (`RFCW100J1HW`) install/launch passed at
  2340x1080 fullscreen landscape. The title/menu reached 60 FPS with the 0.6.3
  diagnostics overlay, focused `com.unity3d.player.UnityPlayerActivity`, and
  the captured launch slice had no fatal Android/managed exception patterns.
- `HandPoseLabRenderer` produced `Logs/HandPoseLab/final.png`, but the batch
  render was blank gray and is not visual acceptance evidence.
- USER PHYSICAL ACCEPTANCE PENDING. Hand-pose feel, Module 03 water/flow
  readability, exposed-map false-platform scan, Spiral abyss mood, full Spiral
  traversal, sustained FPS, thermals, and pause/resume remain unapproved.

Full record: `Docs/PHASE_0_6_3_HAND_COLLISION_ABYSS.md`.

## Phase 0.6.2 Authoritative State

- User S23 feedback accepted most of the 0.6.1 checklist but requested more
  inward palms, larger arms, less stiff "zombie" hand rotation, removal of
  stacked Spiral center structures, and a creepier lower void with ominous fog
  and red glow.
- First-person hands remain camera-space presentation only. The 0.6.2 profile
  scales both arms up, moves them slightly outward/lower, and rotates palms
  farther inward while preserving mirrored geometry and no gameplay collision.
- `Module_004_TheSpiral` keeps route, Crumble, Restore, rank, save, and stable
  ID meaning unchanged. Only visual-world composition changed: the central core
  is split into horizontally offset lower/mid/upper masses, and red-glow/fog
  depth pieces were added far below the playable path.
- Validation: source passed; EditMode `178/178`; PlayMode `6/6`; Android
  IL2CPP ARM64 development build succeeded with one legacy-icon warning and
  zero errors.
- APK:
  `Builds/Android/RYDERS-ROAD-0.6.2-s23-feedback-polish-dev.apk`,
  164,474,952 bytes, SHA-256
  `60F57373EE3B1F0A968F7EC71798F3291CE1357D2BAD35489EE79C52BA459C50`.
- Samsung Galaxy S23 `SM-S911B` install/launch passed at 2340x1080 fullscreen.
  A live campaign level loaded at 60 FPS with the `0.6.2` diagnostics overlay
  and revised hands visible.
- USER PHYSICAL ACCEPTANCE PENDING. Final hand-pose approval, full Spiral
  composition review, lower-void mood from Spiral gameplay, sustained FPS,
  thermals, and pause/resume remain unapproved.

Full record: `Docs/PHASE_0_6_2_S23_FEEDBACK_POLISH.md`.

## Phase 0.6.1 Authoritative State

- Standard Crumble timing is now a one-second pressure mechanic. Stage 2 begins
  at one third of the authored fall delay, Stage 3 at two thirds, collapse at
  one second for production route crumbles, and reset remains four seconds.
- `Module_004_TheSpiral` keeps its 19 mandatory crumble supports as route
  pressure chains with normal/safe breathers and Restore decks between chains.
  The phase intentionally changes rhythm/timing, not the GoldSrc movement
  solver, camera rules, Restore rules, ranks, saves, progression, or stable IDs.
- First-person hands keep mirrored geometry and equal scale. The 0.6.1 profile
  turns palms more inward, makes the right hand more visible, increases
  camera-space run/jump/fall/land presentation motion, and remains visual-only:
  no hand colliders, rigidbodies, root movement, or gameplay velocity edits.
- The large high-sky rectangle was traced to title/menu decorative primitives:
  `Selector Sky Haze`, `Selector R Beacon`, and two `Selector Hero Pillar`
  objects in `DevelopmentModuleSelector`. Those traced objects were removed,
  and the approved title panorama now renders opaque behind the logo/UI.
- Spiral world composition was cleaned by removing accidental high far-spire
  decoration and adding lower-void cloud/ruin depth pieces. The void reads
  deeper without adding collision, random fragments, or a flat floor.
- Validation: source passed; EditMode `178/178`; PlayMode `6/6`; Android
  IL2CPP ARM64 development build succeeded with one legacy-icon warning and
  zero errors.
- APK:
  `Builds/Android/RYDERS-ROAD-0.6.1-parkour-feel-and-route-rhythm-dev.apk`,
  164,474,820 bytes, SHA-256
  `7E2C2B120BEC6117A95E0767C7A9B3F27E9C7ADE9F27E2B38922B7A52CE37617`.
- Samsung Galaxy S23 `SM-S911B` install/launch passed at 2340x1080 fullscreen.
  The second title capture shows the approved logo/UI over the panorama with
  the large rectangle gone. Launch log showed first frame at 60.000004 Hz and
  no fatal exception in the captured slice.
- USER PHYSICAL ACCEPTANCE PENDING. Human crumble timing, Spiral route rhythm,
  in-motion hand feel, full-route traversal, sustained FPS, thermals,
  pause/resume, and actual phone play are not approved by this smoke.

## 0.6.1 Phone Checklist

1. Does Crumble collapse after roughly one second?
2. Do Crumble sequences force you to keep moving instead of waiting?
3. Are normal/safe blocks spaced well as breathers between crumble chains?
4. Do the palms now angle inward enough?
5. Do both arms appear the same actual length?
6. Is the right hand properly visible?
7. Do the hands visibly feel like running when moving?
8. Does jump/fall/landing hand motion feel natural?
9. Do hands stay out of the important landing view?
10. Is the giant rectangle gone from the title/menu sky?
11. Are other strange world artifacts gone?
12. Does the lower void feel deeper and more dangerous?
13. Does Spiral still feel readable and fair?
14. Does the phone hold fullscreen 60 FPS without obvious heat, bars, or
    pause/resume problems?

The remaining 0.6.0 and earlier sections are historical baselines where they do
not conflict with the state above.

Full record: `Docs/PHASE_0_6_1_PARKOUR_FEEL.md`.

## Phase 0.6.0 Authoritative State

- Nineteen `crumble.m04.route.*` blocks replace ordinary Spiral supports in
  six short/medium chains. They are mandatory road, not side decoration.
- Floor contact activates crumble through
  `CrumblingBlockContactRelay`; the child trigger remains a fallback. Stage 2
  begins around 1.21 seconds, Stage 3 around 1.72, collapse around 2.2, and
  reset occurs after four seconds or immediately on module Restore/reset.
- `moving.m04.required-crossing` replaces route supports 18 and 19 and is the
  only ordinary path into Restore 2. Runtime path-marker cubes and the broad
  cyan top plate were removed.
- The temporary `surf.m04.inner-wall-a` 11m cyan panel and its Spiral
  shortcut/camera branch were removed. Surf remains implemented in the role
  gallery.
- Hands use symmetric position/euler `(+-0.38,-0.39,0.40)`, left
  `(286.91,44,146)`, right `(286.91,316,214)`, and scale `0.35`.
  Run translation is capped at 0.01/0.006/0.012m, run rotations are tunable,
  and maximum presentation displacement is 0.075m.
- Movement physics, 94 FOV, camera/input contracts, Restore behavior, ranks,
  saves, progression, fixed landscape, and five Restore IDs are unchanged.
- Validation: source passed; EditMode `176/176`; PlayMode `6/6`; Android
  build succeeded with zero errors. APK:
  `Builds/Android/RYDERS-ROAD-0.6.0-crumble-route-truth-and-hand-refinement-dev.apk`,
  164,472,892 bytes, SHA-256
  `8036FB1B4FFAC83992400EBEF49FC9FAF3545873419958AEA3735F88588AA5CF`.
- S23 install/launch passed at 2340x1080 fullscreen. Title and grounded Spiral
  start rendered at 60 FPS without a crash. No autonomous gameplay was used.
- USER PHYSICAL ACCEPTANCE PENDING. Stop after this update and wait for the
  user's test response. Do not start a goal or continue automatically.

Full record: `Docs/PHASE_0_6_0_CRUMBLE_ROUTE_TRUTH.md`.

The remaining 0.5.9 section is historical baseline where it does not conflict
with the state above.

## Phase 0.5.9 Authoritative State

- Project-owned Meshy sources `Chrono_Core_Crate`, `Fractured_Power_Core`, and
  `Molten_Ruins` are integrated as `PF_RR_Crumble_Stage1/2/3`. They measure
  5,155 / 5,086 / 5,194 triangles, use 1024 Android base textures, and contain
  no colliders. Audit: `Logs/phase059-crumble-asset-audit.txt`.
- The old red/orange objects were `Crumbling` role roots using the Standard
  prefab plus three generated `CrumbleFault` plates. Production crumble blocks
  now instantiate the sequential stage set instead.
- First contact starts Stage 1. Stage 2 begins near 0.8 seconds, Stage 3 near
  1.6, collapse near 2.4, and reset occurs four seconds later. Re-contact does
  not restart timing. Shake moves only the visual root; the primitive gameplay
  BoxCollider remains stationary until collapse.
- Hands now use left position/euler `(-0.41,-0.41,0.39)` /
  `(286.91,41.63,143.5)` and right `(0.39,-0.37,0.43)` /
  `(284,321,221.5)`. Scale remains `0.36`; the right is slightly more visible,
  both palms angle inward, and `Logs/HandPoseLab/final.png` is the GPU render.
- Standard, Long, and Safe prefab/collision policy is unchanged. Death Restore
  still preserves pre-death rendered yaw; initial/manual Restore uses authored
  yaw. `moving.m04.side-cycle` remains a purposeful optional inner bridge.
- The six central `m04.world.core.*` pieces are smaller, vertically separated,
  and mildly offset. Four `m04.world.depth.*` pieces add structured low cloud
  and ruin silhouettes. Random fragments remain disabled and the void has no
  flat floor.
- No third-party free assets were imported; no license record was required.
  GoldSrc movement, camera/input, 94 FOV, ranks, PB, saves, progression, stable
  IDs, and fixed landscape remain unchanged.
- Validation passed: source, EditMode `175/175`, PlayMode `5/5`, and Android
  IL2CPP ARM64 build. APK:
  `Builds/Android/RYDERS-ROAD-0.5.9-world-cohesion-and-crumble-truth-dev.apk`,
  164,471,168 bytes, SHA-256
  `59E1597F4F071B0724B0BE2CFDF45377D56D192FCAFB45C2A728A13D6DC696EC`.
- Samsung S23 install/launch passed at 2340x1080 fullscreen. Title and grounded
  Spiral start rendered at 60 FPS without a crash. The `AssetPackManager`
  lookup remains nonfatal. Crumble gameplay and full-route acceptance were not
  physically performed.
- USER PHYSICAL ACCEPTANCE PENDING. Stop after this update and wait for the
  user's numbered test answers. Do not start a goal or continue automatically.

Full record: `Docs/PHASE_0_5_9_WORLD_COHESION.md`.

The remaining 0.5.8 section is historical baseline where it does not conflict
with the state above.

## Phase 0.5.8 Authoritative State

- The 0.5.7 S23 human pass accepted Standard/Safe visuals and collision,
  death-Restore yaw, initial Spiral yaw, moving-bridge purpose, and fullscreen.
  It rejected the asymmetric hands, unreachable orange boost, late/invisible
  crumble language, disconnected world dressing, and live rotation behavior.
- Hands now use left position/euler `(-0.39,-0.37,0.43)` /
  `(286.91,41.63,139.63)` and right `(0.39,-0.38,0.41)` /
  `(284,321,225)`. Scale remains `0.36`; `Logs/HandPoseLab/final.png` is the
  exact 94 FOV GPU review render.
- `boost.m04.first-ascent` now launches from `(17.376,11.75,-2.535)` toward
  route support 32 at `(15.669,12.45,-12.462)`, an approximately 10m horizontal
  gap. Strength remains `12.4` vertical and `7.2` horizontal.
- Four `crumble.m04.lower.*` blocks form an early optional bridge from route
  support 12 toward 17. The three `crumble.m04.final.*` blocks remain as an
  upper mastery line.
- Six visual-only `m04.world.core.*` pieces create a continuous central broken
  spire. Lower gateway and twin ruin props now have attached island bases;
  automatic random fragments remain disabled.
- Android is fixed to `LandscapeLeft` with Unity and OS autorotation disabled.
  The built manifest now reports `screenOrientation=landscape` instead of
  `userLandscape`, directly addressing the captured 1080x2340 letterbox state.
- GoldSrc movement, Classic/Easy camera boundaries, controls, Restore rules,
  saves, progression, 94 FOV, stable gameplay IDs, and Bronze route are unchanged.
- Validation passed: source, Unity project, EditMode `174/174`, PlayMode `5/5`,
  and clean Android build. APK is
  `Builds/Android/RYDERS-ROAD-0.5.8-s23-physical-acceptance-and-targeted-world-polish-dev.apk`,
  `160,687,330` bytes, SHA-256
  `97B08A6917E9C55D183D4148053AE44A8847F0256C965650B21DD257777E4D0E`.
- S23 `SM-S911B` install and bounded title/Spiral smoke passed at 2340x1080,
  fullscreen, 60 FPS, grounded start, and no crash. Screenshots are
  `Logs/device-058-title.png` and `Logs/device-058-spiral-start.png`.
  Manual acceptance of the changed hands, boost, crumble line, world cohesion,
  and orientation comfort is still required.
- Work stops after this update per user request. Future work should be handled
  one explicitly prompted update at a time, without a long-running goal.

Full record: `Docs/PHASE_0_5_8_TARGETED_POLISH.md`.

The remaining 0.5.7 section is historical baseline where it does not conflict
with the state above.

## Phase 0.5.7 Authoritative State

- The user-supplied MedTech Supply Crate FBX is the canonical ordinary block:
  one mesh, 5,217 triangles, one material, centered pivot, native bounds
  `(1, 0.7734, 1)`, import scale `1`, and no MeshCollider. Production source is
  under `Assets/_Game/Art/MeshySource`; runtime prefabs are
  `PF_RR_Block_Standard` and `PF_RR_Block_Safe`.
- Standard `1.5 x 0.6 x 1.5m` and Safe `3 x 0.6 x 3m` gameplay colliders are
  unchanged. Standard/Safe use a single top-aligned uniform visual; rectangular
  surfaces use `PF_RR_Platform_Long`. Modular carpets are reserved for unusual
  square surfaces at least `6m`; current Campaign and Spiral routes do not use
  them.
- Death Restore captures `FirstPersonCameraRig.RenderedYaw` before resetting
  motion and reapplies that horizontal heading at the checkpoint. Manual
  Restore and initial module spawn retain authored checkpoint/StartAnchor yaw.
- Hands preserve the accepted forward/down orientation family but now sit lower
  and closer with asymmetric framing. Left position/euler is
  `(-0.40, -0.39, 0.39)` / `(286.91, 41.63, 135.63)`; right is
  `(0.39, -0.37, 0.43)` / `(284, 321, 229)`; scale remains `0.36`.
  GPU HandPoseLab output is `Logs/HandPoseLab/final.png`.
- `moving.m04.side-cycle` is no longer an arbitrary side-to-side prop. It
  crosses the inner gap from `(-4.36, 7.37, 18.26)` to
  `(13.07, 10.02, 12.13)` at `2.35m/s` with `0.7s` endpoint pauses as an
  optional faster mastery bridge.
- The red/orange objects were traced to three authored upper-route crumble
  blocks, not debug geometry. They now use the canonical block plus explicit
  CrumbleFault detail, and all child renderers disable with the collider so
  visible-but-nonsolid remnants cannot remain.
- Spiral automatic fragments remain disabled. Its 27 explicit, visual-only
  decorations form lower ruins, garden sky, energy ruins, summit ruins, and far
  floating-civilization/cloud layers. No third-party free assets were imported.
- GoldSrc movement, camera boundaries, touch/Editor controls, 94 FOV, seamless
  procedural sky, stable IDs, progression, and save schema are unchanged.
- Validation passed: source validation, Unity project validator, EditMode
  `172/172`, PlayMode `5/5`, and Android build with zero errors. Its one warning
  is Unity's legacy-icon deprecation; adaptive icons are already configured.
- APK: `Builds/Android/RYDERS-ROAD-0.5.7-world-cohesion-and-gameplay-presentation-dev.apk`,
  `160,687,446` bytes, SHA-256
  `811A9A67B21C853B49D0AC9F914B40201E9C37CF3E8E815C488F6E54F2ADF000`.
- S23 `SM-S911B` (`RFCW100J1HW`) install and launch succeeded. The first bounded
  Spiral smoke exposed an IL2CPP-stripped `SphereCollider` dependency used by
  rounded cloud creation; a direct generic reference now preserves it. The
  repaired APK loaded the title and Spiral at 60 FPS with `Ground True`, no
  development console, no SphereCollider errors, and no app crash. Screenshots:
  `Logs/device-057-title-final.png` and
  `Logs/device-057-spiral-start-fixed.png`.
- Physical movement, collision, Restore-yaw feel, route completion, thermals,
  and sustained performance remain user acceptance items and must not be
  claimed. Full evidence and checklist:
  `Docs/PHASE_0_5_7_WORLD_COHESION.md`.

The remaining sections preserve the 0.5.6 baseline for context. Where they
conflict with this Phase 0.5.7 section, the section above wins.

## Takeover And Starting State

- `Docs/AI_HANDOFF_CODEX.md` was read first. The Gemini handoff, architecture,
  art, movement, camera, level-authoring, testing, save, known-issue, visual-role,
  and latest decision records were inspected before editing.
- Git has one historical commit, `db061d1 chore: establish Unity Phase 0
  foundation`, plus a very large inherited dirty/untracked working tree
  containing subsequent project phases. No unrelated work was reverted or
  broadly renamed.
- Confirmed from 0.5.5: the optimized approximately 15.4k-triangle right arm
  and mirrored left arm exist; the approximately 1.98M-triangle source remains
  reference-only; canonical gameplay roles and mechanic foundations exist;
  stable Spiral/module/progression identifiers remain usable.
- Rejected from 0.5.5 runtime acceptance: old hands looked upright on S23;
  Spiral entry could fall indefinitely; route cadence/composition was not
  trustworthy; automatic decoration looked scattered; the panoramic sky had a
  visible seam. Prior install/launch evidence was not treated as gameplay
  approval.

## Spiral Start And Restore

- Root cause: modules exposed a raw start transform but no authoritative stable
  support binding or capsule-aware validation. Spiral could therefore begin
  unsupported after route/content changes. Its broad/default failure bound also
  allowed an excessively long descent.
- `ModuleDefinition` now stores explicit start support ID, optional custom fall
  threshold, and automatic-fragment policy. `ModuleStartSafety` resolves the
  player capsule against authored solid support deterministically. Production
  validation rejects starts that would need correction.
- Spiral start is `(0.00, 0.45, -24.00)`, faces the opening route, and binds to
  solid support `m04.start`. It does not depend on renderer bounds, mesh pivots,
  or decoration.
- Spiral's fall threshold is `-7.50 m`. Crossing it uses the existing Restore
  architecture: start before the first Restore Point, latest explicit SpawnAnchor
  afterward. There is no visible death plane or emergency teleport system.
- PlayMode loaded Spiral ten times: `10/10` starts had raycast-confirmed solid
  support and no correction. A forced fall below threshold produced exactly one
  Restore, reset horizontal speed, and returned above solid start within the
  test's two-second cap. Runtime handling begins immediately after threshold
  crossing; human perception timing remains a device-play item.
- S23 smoke entered Spiral and showed Ryder grounded on the authored start with
  diagnostics `Ground True`. A deliberate human-thumb device fall was not
  performed in this pass; ADB synthetic swipes did not reliably drive Unity's
  gameplay touch input.

## Locked Movement Envelope

No GoldSrc-style physics values or movement solver behavior changed.

| Measurement | Production value |
| --- | ---: |
| Grounded run speed | 7.80 m/s |
| Jump airtime | 0.62 s |
| Normal forward jump distance | 4.35 m |
| Comfortable Bronze edge gap | 2.83 m |
| Landing-margin distance | 3.40 m |
| Maximum reasonable Bronze gap | 3.70 m |
| Bhop-assisted range | 6.70 m |
| Air-strafe-assisted range | 6.76 m |
| Boost-assisted range | 11.03 m |

The measurement implementation is
`Assets/_Game/Player/Movement/MovementEnvelopeMeasurement.cs`; the latest
report is `Logs/phase056-production-audit.txt`.

## Spiral Rebuild

- Strategy: controlled regeneration of `Module_004_TheSpiral` content version
  2, preserving module ID, progression compatibility, rank configuration where
  applicable, Restore/Patch identities, and shared gameplay systems.
- Route: 58 authored blocks, five Restore supports, and one Patch support for 64
  walkable supports and 63 measured transitions. The route rises roughly 28 m
  around a shrinking-radius macro spiral.
- Rhythm: Setup 5, Easy 7, Normal 12, Commit 8, Flow 8, Run 6, Vertical 7,
  Safe 5. Short connected sections are deliberate run-ups/breathers rather
  than accidental sidewalk massing.
- Measured edge gaps: minimum `0.00 m`, mean `0.99 m`, maximum `2.40 m`; 43
  gaps exceed `0.50 m`. Maximum authored single rise is `0.85 m`. The maximum
  ordinary gap remains below the measured comfortable Bronze value.
- Mechanics: Moving 1, Boost 1, Crumble 3, optional Surf 1, Water 0, Patch 1.
  Normal completion does not require bhop, air strafe, Surf mastery, or Boost
  overshoot. Advanced movement remains useful for pace and optional lines.
- Automatic distant fragments are disabled for production Spiral. The old
  scattered look came from runtime module decoration generation being applied
  broadly without a production-module opt-out.
- Five deliberate landmarks remain: lower tree island, low ruin frame, mid
  arch, upper cyan-energy landmark, and summit pillar/Patch composition.
- Automated spacing/start/collision tests pass. S23 confirms the opening route
  is visible and the player starts safely. A complete ordinary Bronze run is
  still required before claiming final route playability or feel approval.

## Collision And Mechanics

- Standard 1x1 remains the physical truth reference: `1.5 x 0.6 x 1.5 m`.
- `ModuleVisualPrefabLibrary.ResolveModularTileCounts` uses ceiling coverage on
  X and Z independently. `ModuleSceneController` tiles when either axis needs
  multiple pieces, removing unsupported collider lips on narrow/rectangular
  platforms.
- Canonical policies remain documented in
  `Docs/GAMEPLAY_VISUAL_ROLE_MAP.md`: Standard, Long, Safe, Moving, Crumble,
  Restore, Boost, Surf, and Patch each retain explicit visual/collision policy.
- Deterministic lab/reference content covers Restore, Boost, Surf, Crumble,
  Moving, and Patch independently. Automated tests verify mappings, collider
  lifecycle/configuration, explicit SpawnAnchors, reset references, and Patch
  completion wiring. Boost uses configured velocity direction/magnitude; Surf
  remains an intentional angled tangential surface, not a disguised Boost.
- Center/edge/corner footprint math is covered by exact collider/visual bounds
  and tile-count tests. Slow walk-off, diagonal landing, slightly-outside
  landing, and every mechanic's feel still need human device play. Do not turn
  automated role proof into a physical acceptance claim.
- Final Surf art remains pending; its current presentation is intentionally
  temporary.

## First-Person Hands

- Rejected 0.5.5 profile: left `(12, -4, 168)`, right `(12, 4, -168)`, positions
  approximately `(+-0.35, -0.32, 0.44)`, scale `0.38`. Its vector assumptions
  did not match perceived anatomy after the source prefab, mirrored mesh, and
  presentation-root transforms were composed.
- `HandPoseLab.unity` reproduces the exact production camera hierarchy and 94
  FOV. `HandPoseLabRenderer` rendered identity, local X/Y/Z `+-90`, and local X
  `180` candidate rotations. Rendered camera view, not dot products, selected
  the local-X-plus-90 orientation family.
- Final left position/euler:
  `(-0.42, -0.36, 0.46)` / `(286.91, 41.63, 135.63)`.
- Final right position/euler:
  `(0.42, -0.36, 0.46)` / `(286.91, 318.37, 224.37)`.
- Production scale: `0.36`. Fingers visibly extend into the scene, palms read
  mainly downward, glove backs are visible, and the landing center stays clear.
- Final lab render: `Logs/HandPoseLab/final.png`. S23 Spiral-start capture also
  confirms the static pose. Existing idle/run/jump/air/fall/land/speed
  procedural reactions remain; sophisticated animation was intentionally not
  added. Motion clipping and animation feel need human gameplay review.

## Seamless Sky

- Root cause: a visually attractive but non-seamless flat reference was bound
  to a `Skybox/Panoramic` material and forced to wrap through 360 degrees.
- Runtime now loads `Assets/_Game/Art/Resources/Materials/
  MAT_RR_Skybox_Seamless.mat`, using `Skybox/Procedural`, exposure `1.08`, and
  low-cost tint/ground/horizon settings. The panoramic image remains reference
  art only.
- Editor render and S23 gameplay view show no vertical seam. The sky remains
  bright/open and HUD text remains readable. It is deliberately simple; cloud
  and distant-world layering is future art work.

## Regression And Build Evidence

- Movement: implementation untouched; measured-envelope and existing movement
  regression tests pass.
- Rank/progression/save: stable IDs and schema V4 contracts preserved; existing
  rank, Bronze progression, PB, and save tests pass.
- Source validation: `Tools/Validate-Foundation.ps1` passed. The script's stale
  build-version and retired-panorama assertions were updated to the current
  0.5.6 contracts.
- EditMode: `167/167` passed, 0 failed,
  `Logs/editmode-056-final.xml`.
- PlayMode: `5/5` passed, 0 failed,
  `Logs/playmode-056-final.xml`.
- Android: IL2CPP ARM64 development build passed with zero reported
  warnings/errors, `Logs/android-build-056.log`.
- APK: `Builds/Android/RYDERS-ROAD-0.5.6-core-playability-lock-dev.apk`.
- APK size: `160,790,104` bytes.
- SHA-256:
  `B899E8D60FCCB66779BAA83CE2C8FBA6E588DC3F557BAC3A69901A85DDE70BA7`.

## S23 Evidence And Limits

- Device: Samsung Galaxy S23 `SM-S911B`, serial `RFCW100J1HW`.
- ADB install: success. Launch: success. Development startup took roughly 15
  seconds while the player profiler attempted connection.
- Actual exercised scope: menu rendered; Spiral mode was entered; Ryder visibly
  started on solid support; static hands, opening collision truth, route view,
  sky, HUD, fullscreen, and idle FPS were inspected. Forced landscape rotations
  1 and 3 both rendered at 2340x1080; auto-rotate was restored afterward.
- FPS: diagnostics showed 60 FPS at the Spiral start. This is not a sustained
  thermal/performance result.
- Fullscreen: no status/navigation bars were visible in either landscape
  orientation. Cutout comfort and suspend/resume remain pending.
- Not performed: human thumb movement, first five jumps, deliberate physical
  fall/Restore, mid-route collisions, complete Bronze run, advanced movement,
  Moving/Boost/Crumble/Surf/Patch interaction, or Patch completion.
- `logcat` showed a nonfatal optional Play Asset Delivery `AssetPackManager`
  class lookup and development-profiler connection warnings. No crash occurred.
- Screenshots:
  `Logs/device-056-title-ready.png`,
  `Logs/device-056-spiral-start.png`,
  `Logs/device-056-landscape-rotation-1.png`, and
  `Logs/device-056-landscape-rotation-3.png`.

## Remaining Work And Next Gate

The user should physically play the 0.5.9 APK before requesting another update:

1. Review natural/inward hands and right-hand visibility.
2. Confirm all three new crumble visuals are distinct and readable.
3. Judge the approximately 2.4-second crumble timing.
4. Confirm collapse and four-second reset work reliably.
5. Confirm no old red placeholder blocks remain.
6. Confirm death Restore preserves Ryder's look direction.
7. Confirm the moving platform reads as a useful optional bridge.
8. Check Large/Safe platform visual and collision coherence.
9. Check that central structures no longer look piled or overlapping.
10. Judge whether the world feels less empty.
11. Judge whether the bottom of the void feels deeper and more dangerous.
12. Complete as much Spiral as possible and report route readability/playability.
13. Report FPS, fullscreen, heat, and pause/resume behavior.
14. Attach screenshots of anything wrong or unclear.

Do not continue automatically. The user explicitly ended long-running goal
work and will request future changes one update at a time.

The deferred loading video remains at
`C:\Users\lin4s\Downloads\Ryders Block Assets\Ryders Road Loading Screen.mp4`.

Phase 0.5.9 implementation hard stop reached; targeted human acceptance is pending.
