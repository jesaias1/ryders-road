# Current authority — 0.10.0 real-route air control

Updated 2026-09-08. STOP for the user's physical S23 movement feedback after this
milestone. No further worlds/art/arms or global movement promotion in this pass.
The user tested 0.9.9 and supplied two videos: open lab chains are promising, but
deliberate midair correction on Campaign feels weak in both camera modes.
The clips show lab chains and a Foundry finish; neither is movement approval.

ADR 0022 and PRODUCTION_0100.md are the implementation/report authority.
Training/Movement_RealRoute is an isolated compatibility-3 candidate:
- Flow thumb x adds an explicit fixed wish-direction offset as well as familiar
  yaw; forward/back throttle stays deliberate, pure yaw produces no acceleration.
- Projected acceleration 26 and opposing-input braking 30 m/s² are separate.
  Run 7.8, wish cap 8.2, soft energy 10.8, hard safety 18 m/s remain unchanged.
- Energy limiting occurs after wish acceleration, permitting correction at the
  ceiling. Overspeed is preserved within safety; no optimal strafe computation.
- Ordinary landing requires controller contact, avoiding early probe/friction.
  Buffered taps and a brief aligned clean-hop grace preserve momentum.
- Manual/landing camera choices have the same physics and existing pitch controls.

Home → Campaign Flow Trial: A accepted/saved controls; B new/manual view;
C identical new motor/landing view; D previous 0.9.9 Flow/manual view.
Real roads 001–004 are available; Foundry starts selected. No trial attempts,
PBs, progression or rewards write to V4. Retry retains trial, normal selection
clears it. Accepted and previous mastery asset bytes remain unchanged.

Home → Flow Lab starts AIR CONTROL CIRCUIT, with five offset/precision/turn
jumps. MOTOR cycles new → accepted → previous → new. Existing straight/bhop,
landing and authored surf entry/exit exercises remain under ROOM. Controls/view
are session-only. CSV traces buffer the last 9000 simulation steps per context
and profile, flushing to persistentDataPath/MovementTrials on retry/profile
change/exit/pause. Trace data has no save-service or movement authority.

Measured at 60 Hz over 1/3 second using normalized (0.65,1):
at 8.2 m/s lateral displacement improves 0.010 → 0.467 m;
at 10.8 m/s, 0.000 → 0.208 m without increasing the ceiling.
At 14 m/s a stronger deliberate diagonal gives 0.277 m correction. Mild
overspeed steering can still be projection-limited; this is not auto-steering.
Actual small-platform tests distinguish missed straight input from successful
deliberate correction. Full per-rate measurements are in Logs/Movement100.

Verification: 240/240 EditMode, 83/83 PlayMode, source validation, LFS hydration
and diff checks pass. Includes real Sky City and Foundry candidate jumps/ferry,
all 16 trial road/mode loads and save isolation, five diagnostic jumps with
three motors, 30/60/120 Hz correction/braking/hops/walls/surf and both camera
presentations. Circuit and trial-menu renders were inspected. Automated tests
do not certify phone feel, performance, Gold or global replacement.

APK: Builds/Android/RYDERS-ROAD-0.10.0-real-route-air-control-dev.apk.
ARM64 development build succeeded, 0 errors, 1 existing legacy-icon warning;
184,896,303 bytes. SHA256:
731DC13315F426E8C396947D7AF2EFCF1D0EFA7B634CE67A36BD59D356FCBE38.
Package remains com.rydersblockstudio.rydersblock. Build log:
Logs/Movement100/android-build.log. Build completed in 3m25s.
S23 is attached but reported unauthorized during this pass; this candidate has
not been installed or physically played by the agent.

Private GitHub synchronization: https://github.com/jesaias1/ryders-road.git,
origin/main. Commit/push coherent milestones; never make public. This handoff
ships with the movement milestone commit; final delivery reports its SHA.
Recoverable parent before this work: a3dcbdc81756ee6d240b316ba42b1b8af339f40c.
Prior history remains in GitBackups/ryders-road-pre-lfs-migration.bundle (local,
ignored). No further history rewrite. ChatGPT GitHub App access may still need
the user's account-side repository grant.

The earlier LFS migration left some working-tree pointers unhydrated. Restored
actual binaries with git lfs checkout; missing mesh/video regressions then
passed. Before opening a fresh checkout in Unity run git lfs pull, git lfs
checkout, and Tools/Validate-LfsAssets.ps1. Startup now preserves existing
versioned Kenney models instead of overwriting them from workstation caches.
Campaign content/art, stable IDs and V4 are preserved.

S23: compare D then B on Foundry, repeat C; deliberately correct left/right,
recover a poor strafe, brake, chain the circuit and try surf entry/steer/exit.
Check landing visibility, ordinary overshoot and both landscape orientations.
Await feedback before another movement iteration or global promotion.

