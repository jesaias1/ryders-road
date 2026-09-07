# ADR 0009: Restore Movement V1 default mobile controls

Status: Accepted - 2026-08-12

Supersedes: ADR 0008 as the default player-facing control direction.

## Decision

Default mobile play uses left-thumb movement, right-thumb manual camera drag,
and right-side Tap Jump. Jump must not interrupt held movement. Right-side tap
classification and right-side drag camera classification stay in the input
layer and both emit the existing abstract input values consumed by gameplay.

The `0.3.8-movement-lock` flow-steer/auto-camera direction remains available
as a development experiment, not the default. Flick Jump, fixed Jump, and broad
right jump zones are also legacy/development comparisons.

Movement V1 also formalizes normal-vs-momentum jump behavior. Ordinary base-run
jumps prioritize landing precision through shorter airtime and lower takeoff
retention. Earned momentum uses a continuous retention curve so bhop, boost,
surf, shortcuts, and air-strafe skill can still extend range.

## Consequences

The next physical Android test should decide whether Movement V1 feels good
enough to stop changing the fundamental control system and move on to the large
visual/art phase.

Level authors should build Bronze routes around normal run/jump precision.
Advanced movement may create optional faster or longer paths but must not be
required for normal progression.

Android fullscreen behavior is handled by `AndroidDisplayService` behind
`IPlatformDisplayService` and must remain isolated from gameplay scripts.
Samsung Galaxy S23 validation is still required before fullscreen is approved.
