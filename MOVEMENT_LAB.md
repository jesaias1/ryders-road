## Flow Lab (0.9.6)

Home → FLOW LAB opens four separate training exercises using this scene's existing
player/touch composition. Legacy Movement Lab remains available by direct loading.
ROOM / RETRY / A-B / CONTROLS / EXIT are evaluation actions, with no new abilities.
See Docs/MOVEMENT_MASTERY_SLICE.md for controls, measurements and physical checks.

# Movement Laboratory

## Controls

- WASD: move
- Mouse: look
- Escape: release the Editor pointer; left click captures it again
- Space: jump
- R: restore at the latest Restore Point
- F1: toggle Normal and Full Diagnostics
- F2: cycle movement profile
- F3: visualize touch zones
- F4: toggle camera bob, landing response, smoothing, and speed FOV

Mobile defaults to left-thumb movement, right-thumb camera drag, and right-side
Tap Jump. A short right-side tap emits JumpIntent without interrupting left
movement; a right-side drag controls camera and never jumps on release. Flow
steering, broad right jump zones, fixed Jump, and Flick Jump remain available
only through legacy/development profile cycling. Temporary top-right `DIAG`,
`PROFILE`, `CAMERA FX`, `LOOK`, `MOVE`, and `RESTORE` buttons expose beta
controls on a phone without requiring a hardware keyboard.

Fixed jump and legacy Flick Jump profiles remain available through development
cycling for comparison, but Flick Jump is not the default player-facing
profile.

## Default movement values

| Value | Default |
| --- | ---: |
| Walk / run speed | 5.2 / 7.4 m/s |
| Acceleration / deceleration | 42 / 56 m/s^2 |
| Ground friction | 14 |
| Soft / hard momentum limit | 10.8 / 18 m/s |
| Air acceleration / wish speed / control | 11.5 / 7.8 / 0.62 |
| Air turn / momentum gain / contribution cap | 4 / 0.25 / 2.8 |
| Perfect / good / buffered hop windows | 0.055 / 0.12 / 0.18 s |
| Perfect / good / buffered / late retention | 1.00 / 0.96 / 0.90 / 0.72 |
| Jump height / time to apex | 1.62 m / 0.34 s |
| Jump / fall gravity | 28.03 / 37.84 m/s^2 |
| Base / high takeoff retention | 0.88 / 1.00 |
| Landing momentum retention | 0.98 |
| Maximum fall speed | 34 m/s |
| Coyote time | 0.14 s |
| Jump buffer | 0.16 s |
| Ground snap | 0.24 m |
| Edge tolerance | 0.10 m |
| Platform inheritance | 1.0 |
| Restore movement lock | 0.08 s |
| Steering yaw / low / high | 220 / 260 / 150 deg/s |
| Steering accel / decel | 8 / 12 |
| Ground / air / surf steering | 1.00 / 0.72 / 0.85 |

Compare `Default`, `Forgiving`, `Precise`, and `Experimental` during physical
retest. Do not select final tuning before physical-device feedback is reviewed.

## Laboratory sections

Straight Input Test, running/seams, standard jumps, precision blocks,
increasing gaps, turning jumps, ascending/descending jumps, head clearance,
edge landing, moving platform, maximum-jump measurement, Jump Calibration Lane,
air control, Restore Point, finish approach, momentum baseline, repeated
tap-jump lane, bhop timing lane, air-strafe offset lane, authored surf ramps,
manual-camera spiral check, and temporary Patch Block all sit above Null Space.

In the current `0.3.9-movement-v1` build, `Bootstrap` loads the development
module selector first. Use the selector's MovementLab option to return to this
lab.

