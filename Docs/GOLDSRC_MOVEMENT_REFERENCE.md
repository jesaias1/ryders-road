# GoldSrc movement reference - Ryder's Road 0.14.2

Inspected 2026-09-09, alongside ADR 0022, ADR 0027, the 0.10.0 real-route
candidate and current 0.14.1 source. The implementation is original Unity code;
the restrictive SDK source is not reusable project code. No upstream code or
assets were copied into this repository.

## Upstream observations

[Xash3D server movement](https://github.com/FWGS/xash3d-fwgs/blob/1de8289f2980aa34c70760ae0ebfb2e00ddb0f9e/engine/server/sv_pmove.c)
sets command timing and delegates player movement to the game DLL. Engine choice
alone therefore does not specify one universal bhop model.

[HLSDK movement](https://github.com/FWGS/hlsdk-portable/blob/1a1f11041c345c945fb64825530e86e79faa8f06/pm_shared/pm_shared.c)
combines flattened view-forward and view-right with directional commands.
PM_AirAccelerate caps the wish projection at 30 engine units, but its acceleration
budget uses the uncapped wish speed, timestep and friction multiplier. This is a
projection cap, not a total-speed cap. Ground acceleration also uses projection;
ground friction depends on speed, stop speed and surface/edge factors. Successful
jumping precedes ground friction. Default held jumping is release-gated. A
separate jump-speed limiter, conditional on multiplayer `bj`, reduces velocity
above 1.7 times max speed to 65% of that threshold. Collision movement clips
against planes; gravity is split around movement. These details are not universal
across Quake, Half-Life, Counter-Strike, Source or server modifications.

## Independent mathematical model used here

With horizontal velocity v, unit input direction w and stick magnitude m:

- Projected speed p = dot(v, w).
- Acceleration allowance d = min(max(0, wishCap - p), rate * m * dt).
- Tentative velocity u = v + d * w.
- The existing mobile envelope scales u only if its length exceeds
  max(length(v), 10.8 m/s). A separate emergency horizontal cap remains 18 m/s.

The first three relationships make directional input matter. Turning the camera
changes future wish direction; it never transforms existing velocity. A neutral
stick contributes no air acceleration. Below the envelope, perpendicular momentum
is unchanged. Oblique input can add speed; forward input with an already excessive
projection adds nothing. Opposing input spends velocity before reversing direction.

The fourth relationship is a deliberate adaptation, not a GoldSrc property. It
can reduce perpendicular momentum at the envelope and prevents further energy
gain above 10.8, including during Boost overspeed. It still allows heading change;
it does not reject all lateral acceleration at the ceiling. We preserve this
candidate pending phone evidence rather than assume unrestricted gain feels better.

## Ryder's Road comparison and diagnosis

| Area | Current Foundation E | Earlier restriction / consequence |
| --- | --- | --- |
| Wish direction | Full camera-relative two-axis stick, including lateral-only | Earlier Flow used lateral input for yaw; 0.10 added a fixed 65-degree offset but lateral-only still meant yaw-only |
| Projection | max(8.2*m, speed*.95) | Compatibility 3's fixed 8.2 cap requires increasingly large oblique angles at high speed |
| Air acceleration | 26 m/s squared times stick amount; 30 for opposing input | No velocity lerp, optimal-angle helper or camera-generated speed in E |
| Speed envelope | Post-acceleration scaling; incoming overspeed retained | Older pre-acceleration energy intersection rejected useful lateral acceleration at its ceiling |
| Ground | Target-velocity response; 52*1.12 acceleration, 62+12 neutral stopping rate | This is not speed-proportional friction; reverse input also applies separate braking |
| Landing | Ordinary landing requires controller contact | Earlier probe grounding could apply ground behavior before touchdown |
| Hopping | Valid contact rearms hold; clean takeoff skips ground response and retains momentum | Ordinary first takeoff retains 90% at base run; late hops use existing timing retention |
| Jump arc | 1.66 m / .335 s apex; about 29.58 rising and 40.83 falling gravity | Asymmetric designer-authored arc, buffer/coyote windows, Unity contact timing |
| Surf/Boost | Existing authored SurfSurface/SurfProfile, plane gravity and acceleration, detach protection; existing Boost impulses | No general engine surf clone, automatic surfing on all slopes or new ramp-launch mechanic |

The adaptive cap creates an overspeed threshold near acos(.95), about 18.2 degrees
at full input once its speed term dominates. A ten-degree change at high speed
can deliberately do nothing. Side input or a stronger angle produces correction;
opposing input brakes. Small-stick input scales acceleration but the adaptive
projection floor is not scaled by stick magnitude. This is documented candidate
behavior, not an exact analog translation of HLSDK command scaling.

All tuning stays in existing profiles. No speed, gravity, jump, energy envelope,
surface, camera, input preference, accepted baseline or route asset changed here.
The existing candidate is suitable for physical evaluation of these principles;
automated success cannot settle whether the remaining bound feels restrictive.

## Diagnostic refinement

Before 0.14.2, RouteAirControl reported `appliedWishDv` before its energy bound.
The motor now reports dot(actual post-bound net change, wish). This matches the
older candidate's diagnostic convention and enables meaningful comparisons of
requested acceleration, actual acceleration and heading. CSV columns/public
helper contracts are unchanged. Old trace files' applied values should not be
compared as post-bound measurements; recapture with this build. These are
development traces, never official save/PB records.

## Remaining S23 questions

Hold anywhere in the right gameplay area; drag yaw/pitch continuously while the
left thumb strafes through several landings. Lift in flight to stop repeat hops.
Evaluate both landscapes, UI exclusion, pause, Restore and thumb comfort.

Compare E and A on the same Campaign road. Does 10.8 m/s leave enough reward for
good strafing? Are small-angle high-speed corrections predictable, or does the
projection floor need tuning? Is the distinct ground stopping/reversing response
too abrupt after releasing hold? Do Foundry transfers, Windward turns, Abyss Boost
and authored surf preserve useful control? Measure phone frame pacing and thermal
behavior. No physical testing or feel approval has occurred for this milestone.
