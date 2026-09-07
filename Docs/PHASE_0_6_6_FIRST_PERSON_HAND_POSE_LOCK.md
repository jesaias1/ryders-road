# Phase 0.6.6 - First-Person Hand Pose Lock

Target: `0.6.6-first-person-hand-pose-lock`

## Scope

This is a hand-only static-pose phase. No environment, level, gameplay,
movement, physics, camera/FOV, collision, progression, UI, or save behavior was
changed.

## Source Orientation

- Right and left prefabs have identity roots and positive scale.
- Both model children use local euler `(270, 0, 0)` and scale
  `(100, 100, 100)`.
- The source mesh long local-Z axis maps to prefab-root local `+Y`.
- The left mesh is a baked X-mirrored counterpart of the right mesh; vertex
  order matches and no negative-scale mirror exists.
- An editor-only endpoint render confirms prefab-root `+Y` reaches the
  fingertips on both meshes.
- Palm normal is root local `-Z`. Thumb direction is left root local `-X` and
  right root local `+X`.

## Canonical Hierarchy And Transforms

Hierarchy: `camera -> arm root -> prefab visual -> model child`.

- Left arm root: position `(-0.38, -0.40, 0.40)`, euler
  `(348.34290, 263.26410, 257.66420)`.
- Right arm root: position `(0.38, -0.40, 0.40)`, euler
  `(348.34290, 96.73593, 102.33580)`.
- Prefab visual: local position/euler zero, scale `(0.38, 0.38, 0.38)`.
- `NeutralPoseLocked = true`; production applies no later procedural hand
  position or rotation.

## HandPoseLab Acceptance

`Logs/HandPoseLab/final.png` is a 2340x1080 render at production 94 FOV.
Measured camera-space alignment is symmetric: fingers `0.9651` forward, palms
`0.9726` inward, and thumbs `0.9568` upward. Visual review passed all ten phase
questions: thumbs up, palms opposing, fingers forward, diagonal forearms,
implied bent elbows, separated wrists, inside-frame hands, balanced lengths,
clear landing center, and no flat zombie/reaching silhouette. Finger spread is
baked into the mesh and was not rigged.

## Verification

- Source validation: passed.
- Relevant EditMode tests: `6/6` passed.
- PlayMode tests: `6/6` passed.
- Android IL2CPP ARM64 build: succeeded, one warning, zero errors.
- APK: `Builds/Android/RYDERS-ROAD-0.6.6-first-person-hand-pose-lock-dev.apk`.
- SHA-256:
  `412DA9821A86B9BCD3685B10E085DF86D784747DB0CDDF88793B419A93010DD9`.
- Install/launch smoke: unavailable because ADB listed no connected device.
- USER PHYSICAL ACCEPTANCE PENDING.
