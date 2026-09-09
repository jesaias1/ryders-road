> Historical handoff. Current authority: [AI_HANDOFF.md](AI_HANDOFF.md),
> [0.14.1 right-surface correction](PRODUCTION_0141.md) and ADR 0027. No physical
> approval of the new movement is implied by historical testing statements.

# AI Handoff for Codex — RYDER'S ROAD

**Project Name**: `RYDER'S ROAD` (legacy codename: `RYDERS BLOCK`)  
**Package ID**: `com.rydersblockstudio.rydersblock`  
**Target Milestone**: `0.5.5-role-lock-and-spiral-recovery` (Completed)  
**Next Milestone**: `0.5.6-visual-production-pass`  
**Unity Engine**: `6000.5.6f1` (URP 17.5.0)  
**Primary Device**: Samsung Galaxy S23 (`RFCW100J1HW`, Android 14, 2340×1080 @ 60 FPS)

---

## 1. Executive Summary

Phase 0.5.5 (`0.5.5-role-lock-and-spiral-recovery`) successfully resolved the visual and level-authoring regressions introduced in Phase 0.5.4b, established an authoritative Gameplay Visual Role Map, mathematically locked the first-person hand orientation, corrected platform scale collision truth, recovered The Spiral into a discrete human-scale parkour course, and passed all 169 automated tests and physical device verification on Samsung Galaxy S23.

---

## 2. Completed Phase 0.5.5 Deliverables

### A. Authoritative Gameplay Visual Role Map (`Docs/GAMEPLAY_VISUAL_ROLE_MAP.md`)
- Authoritative specification mapping all 10 gameplay roles (`Normal`, `Precision`, `Moving`, `Boost`, `Water`, `Surf`, `Crumbling`, `RestorePoint`, `PatchBlock`, `FatalZone`) and 7 environment roles (`Tower`, `Arch`, `Pillar`, `Rock`, `Tree`, `Grass`, `Sky`).
- Enforces fit modes (`Tile`, `FitWidth`, `Uniform`, `NineSlice`, `Native`), scale rules, walkability, collision policy, tileability, and fallback behavior.
- Prohibits `PF_RR_Block_Standard` (`Meshy_AI_Neon_Medcore_Cube`) from serving as a universal fallback for large or wide platforms.

### B. First-Person Hand Orientation Mathematical Lock
- Resolved hand coordinate system: `+Z` is fingertips, `+Y` is top/knuckle armor plates, `-Y` is palm sensor pad.
- Configured local arm profile (`FirstPersonArmProfile.cs` & `.asset`):
  - Left Arm Euler: `(12°, -4°, 168°)`
  - Right Arm Euler: `(12°, 4°, -168°)`
  - Base Positions: `(±0.35, -0.32, 0.44)`
  - Scale: `0.38`
- Mathematical Verification:
  - `dot(fingerForward, cameraForward) = 0.976 >= 0.65` (athletic forward-reaching posture).
  - `dot(palmNormal, cameraDown) = 0.957 >= 0.60` (palms facing down toward ground, dark knuckle armor facing player).

### C. Collision Truth & Skinny Plate Rescaling
- Rescaled `PF_RR_Platform_Long` in `ModuleVisualPrefabLibrary.asset` to `(3.0, 1.285, 4.57)` to match standard `3.0m × 0.6m × 1.5m` collision bounds.
- Visual top surface is aligned at `+0.30m` with sub-millimeter precision, eliminating falling through thin plates.
- Refactored `ApplyVisualPrefabOverride` in `ModuleSceneController.cs` to respect role fit modes and avoid multi-tiling long platforms into repetitive circular blocks.

### D. Gameplay Role Gallery Scene (`GameplayRoleGallery.unity`)
- Created `GameplayRoleGallery.unity` (`Module_GameplayRoleGallery.asset`) containing all 10 gameplay roles in interactive stations.
- Registered in `ModuleSelectionState.cs` and `Phase2ModuleContentFactory.cs`.

### E. Surgical Recovery of The Spiral (`Module_004_TheSpiral.asset`)
- Restored `Module_004_TheSpiral.asset` layout from giant `8m–12m` multi-tiled slabs into a discrete, human-scale parkour course (1.5m, 1.8m, and 3.0m platforms).
- Smooth mathematical parkour spacing:
  - Edge gaps between consecutive platforms: `1.2m` to `3.4m` (`<= 3.5m`).
  - Vertical step rise between consecutive platforms: `0.1m` to `0.8m` (`<= 0.8m`).
  - Clear Bronze route accessible with standard jumps (no bhop, air-strafe, surf, or boost overshoot required).

---

## 3. Repository & Asset Architecture

### Critical Documentation
1. `AGENTS.md`: Mandatory working agreement (GoldSrc movement invariant, stable IDs, 60 FPS budget).
2. `Docs/GAMEPLAY_VISUAL_ROLE_MAP.md`: Authoritative gameplay role assignment & mesh mapping rules.
3. `Docs/AI_HANDOFF_GEMINI.md`: Full historical handoff log.
4. `ART_DIRECTION.md`: Phase 3B visual pillars and color strategy.
5. `SAVE_SCHEMA.md`: Persisted progression schema V4.

### Core Source Files
- **Movement**: `Assets/_Game/Player/Movement/ParkourMotor.cs` (GoldSrc physics solver, profile `PARKOUR_RESPONSIVE_PHYSICAL_V2`).
- **Input**: `Assets/_Game/Input/PlayerInputRouter.cs`, `Assets/_Game/UI/Touch/TouchInputCoordinator.cs`.
- **Camera**: `Assets/_Game/Camera/FirstPersonCameraRig.cs` (Base FOV 94°).
- **First-Person Arms**: `Assets/_Game/Visuals/FirstPersonHands.cs`, `FirstPersonArmProfile.cs`.
- **Level Content**: `Assets/_Game/EditorTools/Phase2ModuleContentFactory.cs`, `ModuleSceneController.cs`.
- **Validator**: `Tools/Validate-Foundation.ps1`, `FoundationProjectValidator.cs`.

---

## 4. Verification Results

| Validation Check | Status | Details |
|---|---|---|
| Foundation Validator | **PASSED** | `Tools/Validate-Foundation.ps1` returned 0 errors |
| EditMode Tests | **PASSED** | 165 / 165 tests passed (including 4 Phase 0.5.5 regression tests) |
| PlayMode Tests | **PASSED** | 4 / 4 tests passed |
| Android Build | **PASSED** | `RYDERS-ROAD-0.5.5-role-lock-and-spiral-recovery-dev.apk` (160.8 MB) |
| Device Smoke (S23) | **PASSED** | Galaxy S23 (`RFCW100J1HW`), 60 FPS, Title & Spiral verified |

---

## 5. Guidelines for Codex (Next Phase)

1. **Preserve GoldSrc Movement**: Never alter `ParkourMotor.cs` physics constants or wished-speed acceleration models for visual purposes.
2. **Respect Visual Role Map**: Use `Docs/GAMEPLAY_VISUAL_ROLE_MAP.md` as the authoritative source for mesh scaling and role overrides. Do not reintroduce universal block fallback tiling.
3. **Preserve Hand Math**: Keep arm rotations aligned such that palms face down (`dot >= 0.60`) and fingers face forward (`dot >= 0.65`).
4. **Preserve Stable IDs**: Never rename stable content IDs (`module.004.the-spiral`, `m04.base.01`, etc.) casually as they key persisted save progression.
5. **Report Device Verification Honestly**: Only report physical S23 device testing if actually performed via ADB on connected hardware.
