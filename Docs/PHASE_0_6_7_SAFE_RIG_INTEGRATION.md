# Phase 0.6.7 Safe Rig Integration

Build label: `0.6.7-safe-rig-integration-and-locomotion`

Date: 2026-08-24

## Scope

Phase 0.6.7 integrates the user-supplied rigged first-person arm GLB as an
experimental active presentation path while preserving the approved 0.6.6
unrigged first-person arms as fallback assets. This phase is visual-only. It
does not change movement physics, camera/FOV rules, Classic manual camera,
Editor controls, route content, collision, crumble, Restore, ranks, saves,
progression, UI, or Android landscape contracts.

## Source Asset

- Source GLB:
  `C:\Users\lin4s\Downloads\Ryders Block Assets\Meshy_AI_Character_Arm Rigged.glb`
- Copied project source:
  `Assets/_Game/Art/MeshySource/FirstPersonRigged/Meshy_AI_Character_Arm_Rigged.glb`
- Import method: editor-only GLB decode into Unity `Mesh`,
  `SkinnedMeshRenderer`, material, textures, bones, bindposes, and prefabs.
  No runtime glTF package or runtime decoder dependency was added.

Audit summary:

- glTF 2.0, Blender I/O exporter.
- 1 mesh, 1 skin, 1 material, 3 images, 0 authored animations.
- 11,550 vertices, 15,400 triangles.
- 23 generic joints, `Bone_000` through `Bone_022`.
- Source texture sizes include an 8192 base color image and 4096 normal and
  metallic/roughness images. Runtime import settings cap texture size to 2048
  for Android/mobile.

## Runtime Assets

- Active rigged prefabs:
  - `Assets/_Game/Art/FirstPerson/Arms/Rigged/PF_RR_FP_Arm_Rigged_Left.prefab`
  - `Assets/_Game/Art/FirstPerson/Arms/Rigged/PF_RR_FP_Arm_Rigged_Right.prefab`
- Preserved fallback prefabs:
  - `Assets/_Game/Art/FirstPerson/Arms/LegacyApproved/PF_RR_FP_Arm_Left_LegacyApproved.prefab`
  - `Assets/_Game/Art/FirstPerson/Arms/LegacyApproved/PF_RR_FP_Arm_Right_LegacyApproved.prefab`

The left rig uses a baked mirrored mesh and mirrored bone hierarchy. Runtime
object scales remain positive.

## Pose Contract

The 0.6.6 camera-space anchors remain authoritative:

- Left position `(-0.38, -0.40, 0.40)`, euler
  `(348.34290, 263.26410, 257.66420)`.
- Right position `(0.38, -0.40, 0.40)`, euler
  `(348.34290, 96.73593, 102.33580)`.
- Shared profile scale `(0.38, 0.38, 0.38)`.
- Rig prefab `RigCorrectionRoot`: position `(0, -0.02, 0.02)`, euler
  `(0, 0, 0)`, scale `(0.48, 0.48, 0.48)`.

`NeutralPoseLocked` is false for this phase so rigged locomotion can run.
Direction proof from HandPoseLab remains aligned with the 0.6.6 approval pose:
finger-forward dot `0.9651`, inward-palm dot `0.9726`, and thumb-up dot
`0.9568` on both sides.

## Animation

Runtime motion is driven by `FirstPersonHands` and remains camera-space
presentation only. The phase adds subtle idle/run pump, jump/rise, fall brace,
and landing recoil through arm anchors plus additive wrist/finger animation on
the imported bone chains:

- `Bone_006 -> Bone_005 -> Bone_004 -> Bone_003`
- `Bone_010 -> Bone_009 -> Bone_008 -> Bone_007`
- `Bone_014 -> Bone_013 -> Bone_012 -> Bone_011`
- `Bone_018 -> Bone_017 -> Bone_016 -> Bone_015`
- `Bone_022 -> Bone_021 -> Bone_020 -> Bone_019`

The GLB contains no authored animations. All motion is tunable on
`FirstPersonArmProfile`.

## Verification

- Source validation: passed.
- EditMode: `183/183` passed in `Logs/phase067-editmode-results.xml`.
- PlayMode: `6/6` passed in `Logs/phase067-playmode-results.xml`.
- Rig audit: `Logs/phase067-rigged-arm-audit.txt`.
- HandPoseLab render: `Logs/HandPoseLab/final.png`.
- HandPoseLab direction proof:
  `Logs/HandPoseLab/final-axis-acceptance.txt`.
- Android build: succeeded with zero errors and one warning in
  `Logs/phase067-android-build.log`.
- APK:
  `Builds/Android/RYDERS-ROAD-0.6.7-safe-rig-integration-and-locomotion-dev.apk`,
  167,453,368 bytes, SHA-256
  `8AC7BB796A49B648C8AEF9E33163C4CEF08D0F9D3E268A58F53E3B6ED47BF922`.
- Device smoke: installed and launched on Samsung Galaxy S23 `SM-S911B`
  (`RFCW100J1HW`), focused `com.unity3d.player.UnityPlayerActivity`, rendered
  fullscreen landscape at 2340x1080, and captured a gameplay frame with the
  0.6.7 diagnostics overlay and both rigged hands visible. Evidence:
  `Logs/phase067-s23-launch-2.png`,
  `Logs/phase067-s23-launch-log-2.txt`, and
  `Logs/phase067-s23-window.txt`.
- Crash scan: no fatal Android/managed exception patterns were found in the
  captured launch slice.

## Physical QA Checklist

1. Do the rigged arms preserve the approved 0.6.6 first-person pose?
2. Do fingers still read as pointing forward into the level?
3. Do palms face inward/opposite each other without crossing?
4. Do thumbs read upward?
5. Do wrists remain separated in the lower left/right of the frame?
6. Does the center landing view stay clear while idle?
7. Does running animation feel alive without distracting from jumps?
8. Does jump/rise animation avoid covering the next landing?
9. Does falling/brace animation avoid a stiff or broken deformation read?
10. Does landing recoil feel responsive without camera or movement influence?
11. Are there any skinning tears, collapsing fingers, or twisted wrists?
12. Does Classic manual camera still feel unchanged?
13. Does the build hold the 60 FPS mobile target during normal play?

USER PHYSICAL ACCEPTANCE PENDING.
