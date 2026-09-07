## 0.9.9 physical feedback and Campaign Flow trial

The user now physically prefers the promising Flow Lab candidate to fully manual controls; the old scheme is not assumed to be final. Campaign Flow Trial compares accepted controls, Flow/manual pitch and Flow/landing framing on all four roads. Landing framing remains bounded and manual vertical drag stays available. Its 16-degree default survives manual Restore; no platform targeting or motor assistance is added. Promotion needs the next S23 decision. See ADR 0021.

## 0.9.7 autonomous production continuation

The user's 2026-09-08 mission authorizes independent production beyond historical
STOP gates; candidate movement promotion still requires physical approval.
ADR 0019 and Docs/PRODUCTION_097.md record the isolated Flow Lab landing-view
comparison, ordinary precision room, session-best isolation, contact foley and
Ancient Abyss startup preservation. Campaign motor/input/profile/save/content
bytes are preserved. Current verification and artifact: Docs/AI_HANDOFF.md.
No physical approval, final audio mix or production freeze is claimed.
# RYDERS BLOCK Camera Design

## Current Phase

`0.7.7-camera-recovery-platform-visual-truth` restores manual first-person
pitch after physical testing rejected the 0.7.6 fixed-pitch experiment. Unity
positive X pitch looks downward, so fresh gameplay starts at `+9` degrees and
manual look clamps to `-75` degrees up / `+75` degrees down. Vertical FOV
remains `94` degrees. Camera height remains `1.62 m` inside the unchanged
`1.8 m` controller, and near clip remains `0.04 m`.

Left-thumb flow modes use:

- left-thumb movement/steering;
- right-side Tap Jump;
- right-side vertical drag for manual pitch;
- broad Smart Parkour yaw framing without overriding manual pitch.

Normal phone play uses:

- left-thumb movement and horizontal yaw;
- right-side Tap Jump;
- right-side drag for manual camera pitch;
- no camera roll, target lock, or route-selected pitch;
- existing editor mouse/keyboard controls.

The existing gesture classifier emits Tap Jump on a short release up to
`0.16 s`; movement above `1.4%` of the right-region reference length commits
to camera drag and cannot emit a jump. Fall Restore preserves recent pitch and
pre-death yaw. Fresh spawn and manual restart use authored yaw plus the `+9`
degree default.

## Hard Boundaries

Smart Camera may read player position, body yaw, current camera yaw, motor
velocity, grounded state, surf state, vertical speed, touch movement input, and
broad route-camera graph data.

Smart Camera must never:

- move the player;
- change velocity, acceleration, air control, friction, gravity, surf, water,
  boost, bhop, or jump rules;
- auto-jump;
- choose a gameplay route;
- steer toward a platform center;
- magnetize landings;
- prevent falling;
- change Restore, Patch Block, rank, score, save, or completion behavior.

## Runtime Structure

`ParkourCameraProfile` owns tunable values. The runtime default is safe for
prototype use, but production tuning should move to authored profile assets.

`SmartParkourCameraController` computes desired camera yaw and pitch from five
ordered signals:

1. Camera Cone: do nothing inside the inner cone, respond smoothly toward the
   outer cone.
2. Velocity Look-Ahead: prefer filtered real horizontal velocity at speed.
3. Branch Intent: use sustained velocity, steering, and position evidence to
   lean or commit to a branch.
4. Weak Route Guidance: blend broad route direction only when it agrees with
   player motion and the player is near the route corridor.
5. Vertical Framing: gently pitch for ascent/descent and airborne falling.

`FirstPersonCameraRig` may apply Smart Camera as a local yaw offset in Easy
Mode. Touch pitch remains player-owned, and right-side horizontal camera input
is ignored while flow steering owns yaw. Classic manual and editor profiles
retain their established horizontal look behavior. Automatic landing pitch is
disabled for touch so it does not fight the player's selected angle.

## Route-Camera Graphs

Route-camera data describes broad course flow, not individual blocks. Good
nodes mark sections like start, low spiral, water line, boost ascent, surf wall,
high route, shortcut merge, and summit. Bad nodes sit on every platform center.

Branches should use stable IDs, corridor radii, merge IDs when they rejoin,
and conservative vertical framing hints. Shortcuts should require sustained
intent before the camera frames them.

`THE SPIRAL` currently builds a prototype route graph at runtime in
`ModuleSceneController`. Future phases should move this to a focused authoring
asset once the physical phone feel is approved.

## Physical Testing Status

No physical-device approval is implied by automated tests or APK generation.
The camera must be tested on a real Android phone for comfort, motion sickness,
route readability, shortcut trust, fullscreen safe area, orientation behavior,
and sustained 60 FPS before this system can be considered final.
