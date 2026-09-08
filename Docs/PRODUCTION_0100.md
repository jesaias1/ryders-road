# 0.10.0 — real-route air control

User-reported S23 feedback on 0.9.9 is the authority. Reviewed sampled frames from
1000026100.mp4 (21.03 s, Flow Lab) and 1000026101.mp4 (27.44 s, Foundry). Lab
chains and the Foundry finish are visible; neither proves satisfying correction.
Original recordings remain outside Git. ADR 0022 records diagnosis and design.

## Compare on the phone

Home → Campaign Flow Trial: A accepted; B new/manual; C new/landing; D old Flow.
All four roads are available; Foundry is initially selected. B/C physics match.
Home → Flow Lab starts AIR CONTROL CIRCUIT. MOTOR cycles new → accepted → old
Flow → new; VIEW compares pitch. ROOM retains landing precision, air, bhop and
authored surf entry/exit exercises. The compact circuit adds right/left offsets,
a 2.6 m precision surface and a substantial right turn. Gold-colored finish
stripes are exercise markers, not a Gold rank or approval.

Keep forward throttle while deflecting the left thumb toward the correction.
Diagonal input now supplies immediate lateral wish; steering also turns yaw as
before. Pull back to brake; release to coast in air. Pure sideways without
throttle turns the view without acceleration. Stronger overspeed needs a wider
deliberate angle. Right tap jumps; right vertical drag adjusts pitch.

## Measured behavior

At 60 Hz, identical normalized (0.65, 1) input over 1/3 second:

| Initial speed | Old lateral displacement | New lateral displacement | Old/new lateral velocity |
|---|---:|---:|---:|
| 8.2 m/s | 0.010 m | 0.467 m | 0.110 / 2.202 m/s |
| 10.8 m/s | 0.000 m | 0.208 m | 0.000 / 1.942 m/s |

At 14 m/s with a deliberate normalized (1,1) input: 0 → 0.277 m displacement,
0 → 2.759 m/s lateral velocity, preserving the initial energy ceiling. Full
reverse input reduces forward 14 → 4 m/s in 1/3 second. Zero-throttle yaw input
preserves the velocity vector. Reversing a poor strafe reduces lateral drift.
These are real CharacterController simulation measurements at controlled time
steps, not measured phone latency or feel. 30/120 Hz cases exercise the same
contracts; turning integration shows small timestep differences.

The speed ceilings did not increase. Ordinary ground response, jump height,
gravity, takeoff/landing multipliers, accepted assets and Campaign geometry remain.
New contact grounding avoids airborne ground braking. Buffered jump timing stays;
a short aligned landing grace protects imperfect clean taps. No held auto-hop.

## Evidence and limits

240/240 EditMode and 83/83 PlayMode pass; source and LFS hydration checks pass.
ARM64 development APK build succeeded: 0 errors, 1 existing legacy-icon warning.
Exact artifact: C:\Users\lin4s\Documents\Riders Block\Builds\Android\RYDERS-ROAD-0.10.0-real-route-air-control-dev.apk
(184,896,303 bytes). SHA256:
731DC13315F426E8C396947D7AF2EFCF1D0EFA7B634CE67A36BD59D356FCBE38.
Package remains com.rydersblockstudio.rydersblock. The S23 was attached but ADB
reported unauthorized; this new candidate was not installed or physically played.
Logs live in Logs/Movement100. Source tests include useful/poor
steering, reversal, no-input/yaw-only, soft/overspeed limits, ordinary jumps,
actual small-platform correction, grounded transitions, real wall/surf contact,
buffered/late hops, all road/mode loads and trial save isolation. Foundry's actual
standard jumps/ferry and Sky City jumps are exercised with both motors. The
diagnostic circuit's five transitions pass with all three motors. A fixed
off-axis small platform is missed by straight input and reached by deliberate
correction at 30/60/120 Hz. Course traversal fixtures
set up deliberate inputs; they do not certify two-thumb playability.

MovementTrials CSVs under Application.persistentDataPath contain the last 9000
steps per context/profile, flushed on retry, profile switch, scene exit or pause.
They include time, yaw, input, wish, position, velocity, projected speed, requested
and applied wish delta, net acceleration, contact delta, ground/surf and limits.
Accepted mode has no new air-acceleration instrumentation; its vectors/contact
state remain available. Trace files are diagnostic and never uploaded by runtime.

S23 checklist: compare D then B on Foundry's early offsets; repeat in C; make
late left/right corrections and recover from one deliberately poor approach;
chain taps into the circuit's small landing and turn; pull back to brake; try
surf entry/steer/exit; check landing visibility and both landscape orientations.
Report whether correction feels immediate, controllable and predictable, and
whether ordinary jumps overshoot. No global promotion; STOP for physical feedback.

The initial broad regression run exposed unhydrated working-tree LFS pointers
left by the previous synchronization setup, causing missing meshes/video. Actual
binary contents were restored with git lfs checkout; all affected tests now pass.
Tools/Validate-LfsAssets.ps1 guards that precondition on future checkouts. No asset
history rewrite, new models, or art redesign was part of this milestone.
