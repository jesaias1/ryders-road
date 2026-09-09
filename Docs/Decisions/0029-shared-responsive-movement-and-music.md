# ADR 0029 — Shared responsive movement and soundtrack

2026-09-09. Accepted for implementation by the user's explicit game-wide rollout
request after physical 0.14.2 feedback. Physical approval of the new tuning is pending.
This supersedes ADR 0028's candidate-only rollout gate, not its historical evidence.

## Decision

Normal Campaign, all five roads, Spiral, Movement Lab and Flow Lab load the single
`movement.shared` resource, compatibility **5**. Retry/Restore keep that contract.
Legacy assets remain only for explicit development comparisons. Saved Easy/Classic
and jump-mode preferences cannot replace shared whole-right hold/manual-camera
controls; preferences are preserved for legacy comparisons. Editor inputs remain.

The former adaptive 0.95 wish threshold and ordinary 10.8 m/s energy envelope made
small corrections disappear at speed. Responsive air control uses a fixed wish
projection cap, 40 m/s² explicit acceleration/braking, and spends unused input
budget on tangential direction correction with unchanged speed for that correction.
The projection push can earn speed. Neutral input and camera-only yaw add no force.
The 18 m/s emergency horizontal cap stays; it is not a target or ordinary soft cap.
Clean held jumps preserve takeoff momentum. Ground stop/reverse tuning stays prompt.
All tunables live in Movement_Shared.asset; the old numerical path is retained.

This is an independent, deliberately forgiving mobile adaptation of GoldSrc
momentum/projection principles, not a numerical CS1.6 clone. It contains no engine
source, automatic strafe optimizer, target platform lookup, landing magnetism or
camera-generated input. Tangential correction is an explicit departure from literal
GoldSrc projection. See Movement150QA for source distinctions and measured results.

Shared grounding requires a fresh CharacterController collision result after reset.
An owned moving support applies a skin-width settle during carry so its separate
Move does not erase support contact. Broad ground probes cannot brake airborne
motion or trigger premature shared hops.

## Records and presentation

Movement compatibility 5 gets independent existing V4 record buckets; old PBs,
history and unlocks retain their meaning. Threshold assets are unchanged and remain
uncalibrated for this movement; scripted completion times are not rank calibration.
Any valid main-module completion still supports Bronze progression.

MusicDirector owns only two persistent streaming music sources and context fades.
Existing SFX and scene/loading ownership remain separate. Music volume is an
additive settings field with a 0.65 default and its own PlayerPrefs key. Original
user MP3s are untouched; rendered WAV loops use LFS and runtime Vorbis streaming.
See Audio150/README.md for provenance, processing, choices and limits.

## Acceptance

Require full Unity regression suites, representative normal-world traversal,
shared profile/input/restore checks, music and frontend verification, then a signed
Android development APK. Do not equate automated feasibility with human feel,
musical seam approval, touch latency, sustained phone FPS or thermal performance.
Stop after delivery for S23 feedback; no new worlds or platform expansion.
