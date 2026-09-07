# RYDER'S ROAD Phase 0.6.9 Ryder Arm V2 Full Replacement

Build ID: `0.6.9-ryder-arm-v2-full-replacement`

## Scope

This phase fully replaces the player-visible Meshy arm with the project-supplied
`Ryders Arm Rigged.glb`. It changes first-person arm import, material,
handedness, static pose, hierarchy, tests, and documentation only. Movement,
physics, camera/FOV/yaw, controls, route geometry, collision, world content,
UI, progression, saves, and Android orientation behavior are unchanged.

## Asset And Import

- Original: `C:\Users\lin4s\Downloads\Ryders Block Assets\Ryders Arm Rigged.glb`.
- Project copy: `Assets/_Game/Art/FirstPerson/Arms/RyderArmV2/Source/Ryders_Arm_Rigged.glb`.
- Both files are 7,900,100 bytes with SHA-256
  `7BFB2177CF7BC8DE31687C9420A753C7B6B6605C5B6A1E7ACC631F124873A328`.
- The editor-only decoder produces a 11,953-vertex, 15,541-triangle,
  24-joint skinned mesh with no runtime glTF dependency.
- The right prefab uses source handedness. The left prefab uses a baked
  X-mirrored mesh and mirrored skeleton; every runtime hierarchy scale remains
  positive.
- Base color, normal, and emissive maps are assigned by glTF role. The packed
  metallic/roughness source remains linear import data and is not assigned as
  Unity `_MetallicGlossMap`. Tangents are generated and backface culling is on.

## Production Pose

- Runtime hierarchy: camera -> `FirstPersonVisualRoot` ->
  `LeftArmAnchor` / `RightArmAnchor` -> V2 prefab.
- Left anchor: position `(-0.60, -0.16, 0.48)`, euler
  `(0, -60, 205)`. Right anchor: `(0.59, -0.14, 0.50)`, euler
  `(1, 60, 155)`. Shared scale is `(0.38, 0.38, 0.38)`.
- `Bone_000` is the upper arm, `Bone_003` is the elbow, `Bone_002` is
  forearm/wrist roll, and `Bone_001` is the palm parent.
- Elbows use a 45-degree local-X bend. Wrists use mirrored 75-degree local-Y
  pronation. Finger chains use small graduated local-Z curls for a relaxed
  hand, not a fist.
- No synthetic sleeve or cutoff patch is used. Full source upper-arm geometry
  exits through the lower frame edge.
- `NeutralPoseLocked` is true. Runtime arm idle, run, jump, fall, landing,
  boost, water, wrist, and finger animation remain disabled.

## Fallbacks And Evidence

- The prior Meshy rigged prefabs remain inactive under
  `Assets/_Game/Art/FirstPerson/Arms/Rigged`.
- The approved unrigged prefabs remain inactive under
  `Assets/_Game/Art/FirstPerson/Arms/LegacyApproved`.
- Import audit: `Logs/phase069-ryder-arm-v2-import-audit.txt`.
- Production render: `Logs/HandPoseLab/final.png`.
- Source validator, foundation validator, EditMode `183/183`, and PlayMode
  `6/6` passed.
- Android IL2CPP ARM64 build succeeded with zero errors and one warning. The
  exact APK is
  `Builds/Android/RYDERS-ROAD-0.6.9-ryder-arm-v2-full-replacement-dev.apk`.
- Bounded Galaxy S23 smoke installed, launched, and loaded Spiral fullscreen
  at 2340x1080 with both V2 arms visible, diagnostics at `FPS 60`, and no
  fatal crash patterns in the captured log slice. Evidence is in `TESTING.md`.

USER PHYSICAL ACCEPTANCE PENDING. Human review remains required for natural
handedness, elbow/wrist/finger pose, lower-corner framing, material response,
mirror quality, landing visibility, and sustained device performance.
