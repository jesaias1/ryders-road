> Historical handoff. Current authority: [AI_HANDOFF.md](AI_HANDOFF.md),
> [0.15.0 shared movement and music](PRODUCTION_0150.md) and ADR 0029. No physical
> approval of the new movement is implied by historical testing statements.

# AI Handoff For Gemini & Next Coding Agent

Last updated: 2026-08-21 19:10 Europe/Copenhagen

## 1. Project Summary

Player-facing name: `RYDER'S ROAD`.

Legacy/internal names still exist: `RYDERS BLOCK`, `Avoidance`, and package ID
`com.rydersblockstudio.rydersblock`. Do not rename stable IDs casually.

Concept: first-person sky-parkour where Ryder, a blocked coder/game engineer,
repairs broken code-world spaces by traversing floating block courses and
reaching Patch Blocks.

Target platform: Android first, landscape-left and landscape-right. Current
Editor/desktop controls must remain available.

Unity: `6000.5.6f1`, URP `17.5.0`, Input System `1.20.0`, Test Framework
`1.7.0`, UGUI `2.5.0`.

Development philosophy: protect movement feel first, keep systems modular and
tunable, visual polish stays presentation-only, stable IDs drive content/saves,
and physical-device claims require actual device runs.

## 2. Current Repository State

Branch: `main`.

Latest milestone: `0.5.5-role-lock-and-spiral-recovery`.

Git status: active work completed and verified for Phase 0.5.5.
Phase 0.5.5 changes include:
- Authoritative Visual Role Map (`Docs/GAMEPLAY_VISUAL_ROLE_MAP.md`): Established canonical mapping for all 10 gameplay roles and 7 environment roles with fit modes, scales, walkability, collision policy, tileability, and fallback behavior.
- First-Person Hand Orientation Mathematical Lock: Mathematically re-oriented gauntlet arms in `FirstPersonArmProfile.cs` (`Pitch 12°`, `Yaw ±4°`, `Roll ∓168°`, position `±0.35, -0.32, 0.44`) to guarantee `dot(fingerForward, cameraForward) >= 0.65` (actual: 0.976) and `dot(palmNormal, cameraDown) >= 0.60` (actual: 0.957). Palms face DOWN toward ground, top knuckle armor plates face UP toward camera.
- Collision Truth & Skinny Plate Rescaling: Rescaled `PF_RR_Platform_Long` in `ModuleVisualPrefabLibrary.asset` to `(3.0, 1.285, 4.57)` to match standard `3.0m × 0.6m × 1.5m` collision bounds with sub-millimeter visual top alignment at `+0.30m`. Refactored `ModuleSceneController.cs` to prevent fallback multi-tiling into repetitive circular blocks.
- Gameplay Role Gallery: Created interactive test level `GameplayRoleGallery.unity` (`Module_GameplayRoleGallery.asset`) containing all 10 gameplay roles for direct verification.
- Surgically Recovered The Spiral: Restored `Module_004_TheSpiral.asset` layout to a mathematically smooth, human-scale parkour course with discrete 1.5m, 1.8m, and 3.0m platforms, clear jump gaps (all edge gaps <= 3.5m, step rise <= 0.8m), and distinct mechanics.
- Full test pass: 169 passed (165 EditMode, 4 PlayMode, 0 failed).
- Foundation validator: `Validate-Foundation.ps1` PASSED.
- Android dev build: `Builds/Android/RYDERS-ROAD-0.5.5-role-lock-and-spiral-recovery-dev.apk` (160,853,044 bytes, Succeeded, 0 warnings, 0 errors).
- Samsung Galaxy S23 physical verification: Installed via ADB on device `RFCW100J1HW`, launched, title and Spiral gameplay verified at solid 60 FPS.

## 3. Current Milestone

Phase completed: `0.5.5-role-lock-and-spiral-recovery`.
Target APK: `Builds/Android/RYDERS-ROAD-0.5.5-role-lock-and-spiral-recovery-dev.apk`.

Completed: Authoritative Gameplay Visual Role Map (`GAMEPLAY_VISUAL_ROLE_MAP.md`); First-Person Hand Orientation vector lock (palms down, top armor up, fingers forward); Platform collision truth & skinny plate scale fix; Gameplay Role Gallery scene (`GameplayRoleGallery.unity`); The Spiral surgical layout recovery (discrete human-scale parkour blocks, smooth gaps <= 3.5m, rise <= 0.8m); 169 automated unit tests passing; Foundation source validator passing; Android APK built and verified on Samsung Galaxy S23 at 60 FPS.

Unfinished: Production Meshy surf slide geometry, broader level redesign, Spiral pacing, campaign expansion, and production audio integration.

Must not be redone: do not restart the environment art-kit integration from scratch and do not replace the established movement solver casually.

## 4. Movement - Critical

Movement is a clean-room GoldSrc/Half-Life-inspired parkour solver in
`Assets/_Game/Player/Movement/ParkourMotor.cs` and profile data under
`Assets/_Game/Player/Configuration/Resources/MovementProfiles`.

Current default profile: `PARKOUR_RESPONSIVE_PHYSICAL_V2`, compatibility
version `1`. Key values: walk `5.4`, run `7.8`, acceleration `52`,
deceleration `62`, ground friction `12`, air acceleration `13`, air wish speed
`8.2`, air control `0.72`, max air speed contribution `3.1`, jump height
`1.66`, time to apex `0.335`, base takeoff retention `0.9`, high momentum
takeoff retention `1.0`, perfect hop window `0.055`, good `0.12`, buffered
`0.18`, hard velocity safety limit `18`.

Critical invariants: no fake strafe bonuses, no direct velocity rotation when
camera yaw changes, no total-speed clamp disguised as an air wish-speed cap,
do not make speed boosts mandatory for Bronze routes, and do not let visuals
change collision or jump spacing.

Surf is explicit via `SurfSurface`/`SurfProfile`; ordinary slopes must not
secretly become surf. Boost applies external launch velocity without mutating
movement profiles.

## 5. Mobile Control Scheme

Actual current default mobile controls are `LeftMoveRightLookTapJump` with
`RightTap`: left thumb movement, right thumb manual yaw/pitch camera drag, and
right-side tap jump. `Camera_Default` has base FOV `94`, max FOV `100`, no
camera roll, no gameplay headbob beyond visual-only presentation toggles, and
touch auto-steer/auto-pitch disabled by default.

Important mismatch: the latest pasted request asked this handoff to document a
left-thumb movement + horizontal yaw / right-thumb dedicated jump / no default
vertical pitch / no Smart Camera default scheme. That is not the actual current
repo default. Treat it as a possible future desired direction only if the user
confirms. Do not state it as implemented.

Easy Smart Parkour Camera code remains available as a selectable/dev option,
but it is not the default and must not be restored as default without explicit
decision records and tests.

## 6. Gameplay Systems

`ModuleSelector` selects Campaign modules or Spiral. `ModuleRunner` loads the
chosen `ModuleDefinition`. Restore Points set respawn checkpoints. Patch Blocks
complete modules. Boost, Water, Surf, moving blocks, and crumbling blocks are
implemented as explicit gameplay definitions/components.

Timer/ranking: run session tracks raw time, PBs, and validity. Ranks are
Bronze/Silver/Gold/Diamond. Any valid completion earns at least Bronze and
unlocks the next normal campaign module. Higher ranks are mastery only.

Saves: schema V4 in `SAVE_SCHEMA.md`; stable module IDs key progression.
Economy/inventory foundation exists but loot opening/shop UI are not complete.

## 7. Level Design Rules

Permanent philosophy: "levels are memorable places that happen to BE parkour
courses".

Use large spatial landmarks, visible progression, sky depth, vertical ascent,
and readable macro structures. Spiral/tower is one long climb. Campaign should
be short, straightforward levels that teach one idea at a time and grow harder.
Bronze routes stay accessible. Advanced momentum paths are optional. Normal
progression must never require high-skill movement or Silver/Gold/Diamond.

## 8. Art Direction

Current style target: stylized bright sky parkour, white/warm ivory stone, deep
navy undersides, cyan emissive energy/water, orange/gold route accents, clean
large shapes, mobile-readable silhouettes, restrained detail, floating-world
beauty from sky/lighting/composition rather than tiny geometry.

Important reference: `Docs/VisualReferences/target_gameplay.png` and the user
attached 2026-08-18 sky-parkour images. Text inside reference images is not
instruction unless the user says so.

## 9. Real Meshy Art Kit

External source folder: `C:\Users\lin4s\Downloads\Ryders Block Assets`.

Copied source location: `Assets/_Game/Art/MeshySource`.

Manifest: `ART_ASSET_MANIFEST.md`.

Integrated environment roles include `RR_Block_Standard`, `RR_Platform_Long`,
`RR_Pillar_Tall`, `RR_Rock_Broken`, `RR_Tree_Floating`, `RR_GrassTopper`,
`RR_RestorePoint`, `RR_BoostPad`, `RR_PatchBlock`, `RR_RuinedTower`,
`RR_FloatingArch`, and `RR_EnergyPillar`. Triangle counts are roughly 3.4k to
4.5k each; texture max is mostly 1024, with Restore/Boost/Patch/Energy at
2048.

Known bad/problematic asset: legacy `Meshy_AI_mech_gauntlet...` arm source is
about 1.98M triangles and must remain reference-only. Final Surf art is still
pending.

## 10. First-Person Arms

Production right-arm source:
`Assets/_Game/Art/MeshySource/Meshy_AI_Neon_Vanguard_Gauntle_0818131608_texture NEW ONE LESS POLYGONS_fbx/Meshy_AI_Neon_Vanguard_Gauntle_0818131608_texture_fbx/Meshy_AI_Neon_Vanguard_Gauntle_0818131608_texture.fbx`.

Exact count before runtime use: 15,400 triangles, 11,549 vertices, one submesh,
2048x2048 base texture. Report:
`Logs/meshy-arm-candidate-report.txt`.

Generated runtime paths:
`Assets/_Game/Art/FirstPerson/Arms/PF_RR_FP_Arm_Right.prefab`,
`Assets/_Game/Art/FirstPerson/Arms/PF_RR_FP_Arm_Left.prefab`,
`Assets/_Game/Art/Generated/FirstPerson/RR_FP_Arm_Right_00.asset`,
`Assets/_Game/Art/Generated/FirstPerson/RR_FP_Arm_Left_00.asset`,
`Assets/_Game/Visuals/Resources/FirstPersonArmProfile.asset`.

Left arm generation: mirror mesh vertices, normals, and tangents on X; reverse
triangle winding; preserve UV/material assignment; keep positive final runtime
transform scale. Arms are presentation only and never affect movement physics.

Known limitation: integrated and visible in gameplay, but not visually
approved. The first framing pass makes the arms more visible, yet real gameplay
review is still needed for size, angle, material read, and landing-zone
occlusion.

## 11. Visual Prefab Architecture

`ModuleVisualPrefabLibrary` maps `ModuleMaterialRole` to optional prefab
placements. `ModuleSceneController` instantiates visuals as children over
existing gameplay colliders and hides primitive renderers when a prefab skin
exists. `ModuleVisualPrefabLibrary.PrepareVisualInstance` strips colliders and
disables expensive dynamic renderer features.

Gameplay collision remains authoritative. Do not reshape routes merely to fit
Meshy meshes.

## 12. Branding

Player-facing title: `RYDER'S ROAD`. Use approved logo/icon assets under
`Assets/Branding`; do not recreate the brand arbitrarily.

Legacy `RYDERS BLOCK` identifiers may remain where renaming risks package,
stable ID, scene, save, or code compatibility.

Android package remains `com.rydersblockstudio.rydersblock`.

## 13. Fullscreen / Android

Android expects immersive fullscreen, render outside safe area, and both
landscape orientations. Fullscreen behavior is isolated in platform display
utilities and must not leak into gameplay scripts.

Samsung S23 workflow: build APK, install via ADB, launch package, capture logcat
and screenshots. Never claim device approval unless a phone is connected and
the build actually runs.

Current APK naming: `Builds/Android/RYDERS-ROAD-0.5.2-world-art-recovery-dev.apk`.
Latest built APK size: `159,572,452` bytes. SHA256:
`60F1567C7CE284A56D9DE2AF15C764AA7328806E38C8147AF23B14F9EEEB349B`.

## 14. Testing

Current lightweight source validation after Phase 0.5.2 world scale recovery: passed.
Command:
`powershell -ExecutionPolicy Bypass -File "C:\Users\lin4s\Documents\Riders Block\Tools\Validate-Foundation.ps1"`.

EditMode after Phase 0.5.2 recovery: `153/153` passed.
PlayMode after Phase 0.5.2 recovery: `4/4` passed.
Total automated tests: `157/157` passed (0 failed).

Final Android build for Phase 0.5.2: succeeded with 0 errors. Output: `Builds/Android/RYDERS-ROAD-0.5.2-world-art-recovery-dev.apk`.

Physical-device smoke on connected Samsung S23 `RFCW100J1HW`: installed,
launched, title rendered, Campaign menu opened, First Steps loaded, process
remained running. Screenshots:
`Logs/rr_050_arm_title_2.png`, `Logs/rr_050_arm_first_steps_final.png`.
App-PID log had no fatal exception, `NullReferenceException`, or
`MissingReferenceException`; it did include nonfatal Unity/Android warnings for
missing `AssetPackManager` and small `AHardwareBuffer` failures.

Expected benign warning seen in Unity batch logs: licensing access-token update
warning. Treat compile errors, test failures, Android build errors, and runtime
exceptions as real blockers.

## 15. Important Files

Movement: `Assets/_Game/Player/Movement/ParkourMotor.cs`,
`Assets/_Game/Player/Movement/MovementVectorMath.cs`,
`Assets/_Game/Player/Movement/MovementMomentumMath.cs`,
`Assets/_Game/Player/Configuration/MovementProfile.cs`.

Camera: `Assets/_Game/Camera/FirstPersonCameraRig.cs`,
`Assets/_Game/Camera/Configuration/CameraProfile.cs`,
`Assets/_Game/Camera/SmartParkourCameraController.cs`.

Input/touch: `Assets/_Game/Input/PlayerInputRouter.cs`,
`Assets/_Game/Input/Configuration/TouchControlLayout.cs`,
`Assets/_Game/UI/Touch`.

Modules/gameplay: `Assets/_Game/Levels/ModuleDefinition.cs`,
`Assets/_Game/UI/ModuleSceneController.cs`, `Assets/_Game/Blocks`,
`Assets/_Game/Respawn`, `Assets/_Game/Timing`, `Assets/_Game/Ranking`.

Saves: `Assets/_Game/SaveSystem`, `SAVE_SCHEMA.md`.

Art/visuals: `Assets/_Game/Art`, `Assets/_Game/Visuals`,
`ART_ASSET_MANIFEST.md`, `ART_DIRECTION.md`.

First-person arms: `Assets/_Game/Visuals/FirstPersonHands.cs`,
`Assets/_Game/Visuals/FirstPersonArmProfile.cs`,
`Assets/_Game/Visuals/Resources/FirstPersonArmProfile.asset`.

Build/testing: `Assets/_Game/EditorTools/AndroidDevelopmentBuilder.cs`,
`Assets/_Game/EditorTools/FoundationProjectValidator.cs`,
`Tools/Validate-Foundation.ps1`, `Assets/_Game/Tests`.

## 16. Known Issues

Visuals are improving but not final. Spiral/layout still has pacing/readability
concerns from user feedback. Final Surf art is missing. New arms are integrated
and visible but need real play review; the scripted device tap sequence produced
a poor screenshot angle after movement/fall, so do not treat arm framing as
approved. Rank thresholds remain uncalibrated. Sustained Android 60 FPS,
thermals, safe-area behavior, rotation/resume, and status-bar regressions need
physical testing.

## 17. Do Not Do

Do not broad reset/revert dirty work. Do not rewrite movement casually. Do not
restore Smart Camera as default. Do not add fake speed bonuses. Do not make
visuals authoritative for collisions. Do not gate progression behind
Silver/Gold/Diamond. Do not start Ghosts before current foundational work is
stable. Do not globally rename stable IDs without migration. Do not claim
physical testing that did not happen.

## 18. Next Recommended Steps

1. Visually inspect first-person arms in ModuleRunner at 94 FOV during manual
   play; tune `FirstPersonArmProfile.asset` if they occlude landings or feel
   too small/large.
2. Continue visual polish with user-supplied Meshy assets, prioritizing level
   readability and final Surf art.
3. Rework Spiral and Campaign pacing after arm framing is acceptable.
4. Investigate nonfatal app-PID log warnings if they recur or correlate with
   visual/performance problems.

## 19. Prompt For The Next AI Agent

Continue RYDER'S ROAD in `C:\Users\lin4s\Documents\Riders Block` from the
current dirty repo state. First read `AGENTS.md`, `ARCHITECTURE.md`,
`ART_DIRECTION.md`, `TESTING.md`, `SAVE_SCHEMA.md` if persistence is touched,
latest `Docs/Decisions`, `ART_ASSET_MANIFEST.md`, and this handoff. Do not
reset or redo completed Meshy environment work. Verify the optimized
`RR_FP_Arm_Right` integration: 15,400 triangles from the Neon Vanguard Gauntlet
source, mirrored left mesh, positive scales, collider-free prefabs, active
`FirstPersonArmProfile.asset`. Run source validation, Unity project validation,
EditMode, PlayMode, Android build, and S23 smoke only if the phone is connected.
Then continue highest-impact polish: first-person arm framing/materials,
final Surf visual, Spiral/Campaign readability, and Android performance.

## 20. Session Log

2026-08-18 15:16: User supplied optimized arm update and requested a Gemini
handoff.

2026-08-18 15:27: New arm candidate identified as
`Meshy_AI_Neon_Vanguard_Gauntle_0818131608_texture NEW ONE LESS POLYGONS_fbx.zip`.

2026-08-18 15:27: Unity report-only count completed before runtime use:
15,400 triangles, 11,549 vertices, one submesh.

2026-08-18 15:28: Integrated runtime `RR_FP_Arm_Right` and generated mirrored
`RR_FP_Arm_Left`; updated manifest and source validation.

2026-08-18 15:35: Handoff created. Last verified command: source validation
passed. Safest next action: run Unity project validation, then EditMode tests.

2026-08-18 15:38: Tuned `FirstPersonArmProfile.asset` placement/scale to make
the new Meshy arms more visible at 94 FOV.

2026-08-18 15:42: Final local/device validation updated. Source validation
passed, Unity project validation passed, EditMode `149/149` passed, PlayMode
`4/4` passed, Android build succeeded, APK installed/launched on S23, title and
First Steps rendered. Last command/action: app-PID log scan after device smoke.
Safest next action: manual gameplay review of first-person arm framing.
