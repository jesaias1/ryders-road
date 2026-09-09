# Movement correction evidence — 0.15.0

The user's physical report supersedes 0.14.2's conclusion that its steering was
sufficient. Earlier numerical correctness did not establish acceptable thumb feel.
This milestone changes the response and promotes it to normal play under explicit
user authorization. The phone remains disconnected; no physical test is claimed.

## Reference calibration

Primary design reference is CS1.6/GoldSrc. The inspected
[ReGameDLL_CS movement implementation](https://github.com/rehlds/ReGameDLL_CS/blob/b0889847fe6d03898be88acc9e366660efb40ab5/regamedll/pm_shared/pm_shared.cpp)
is a CS-specific reconstruction, not official Valve source. Its capped 30-unit
air projection uses the uncapped wish speed in the acceleration budget. Its bunny
limiter uses 1.2 times maximum speed and reduces overspeed to 80% of that threshold;
stamina/weapon state also matter. Server options can bypass the limiter and enable
held auto-bhop. Do not equate stock CS with configured bhop servers.

[FWGS portable HLSDK](https://github.com/FWGS/hlsdk-portable/blob/1a1f11041c345c945fb64825530e86e79faa8f06/pm_shared/pm_shared.c)
carries Half-Life SDK movement, not CS1.6-specific tuning. Its limiter
uses 1.7 and 65%, a material distinction. See the earlier
[Xash/HLSDK investigation](../GOLDSRC_MOVEMENT_REFERENCE.md) for inspected engine/SDK
ownership: an Xash engine revision alone does not prescribe a mod's movement.
No source was imported into the commercial Unity project. Xash server movement was also rechecked at [362af785](https://github.com/FWGS/xash3d-fwgs/blob/362af785c81856a730248bc959cb8d448dbd8960/engine/server/sv_pmove.c); it continues to delegate movement to the game DLL.

[id's Quake III implementation](https://github.com/id-Software/Quake-III-Arena/blob/dbe4ddb10315479fc00086f08e25d968b4b43c49/code/game/bg_pmove.c)
provides the related projection/ground-friction comparison; its ground/air
acceleration constants are 10/1 and friction is 6 with stop speed 100. It is a
secondary reference, not the product's desired stock configuration.

`Tools/Measure-ReferencePrinciples.py` implements independent arithmetic scenarios
at 120 Hz, separately normalizing CS 250 u/s and Q3 320 u/s to 7.8 m/s. Full
parameters/results are in `reference-calibration.json`. No actual reference engine
was launched; collision, weapon, stamina, prediction and stock jump limiters are
not simulated. These measurements explain principles, not frame-exact equivalence.
For the selected CS scenario, rest-to-run is 0.408 s and ground release 0.550 s.
Fixed 10-degree input at speed provides zero correction under literal projection.
The new tangential steering deliberately departs from that restriction. Thus
"closer to GoldSrc" here means input-driven momentum and no ordinary energy
envelope, not a claim of closer numerical matching at every angle.

## Actual Unity before/after

SharedMovementTests uses real CharacterControllers, separate airborne fixtures,
and actual old compatibility-4 versus new compatibility-5 profile assets. Input
is held for 0.5 seconds; initial momentum is forward. Values below are 60 Hz.
30/50/60/120 Hz are also regression cases.

| Initial m/s | 10-degree correction: old → new | 90-degree side input: old → new |
| ---: | ---: | ---: |
| 7.8 | 0.62° → 9.14° | 61.25° → 74.18° |
| 10.8 | 0° → 8.47° | 56.42° → 66.71° |
| 14 | 0° → 7.64° | 46.84° → 59.19° |
| 17 | 0° → 6.95° | 40.10° → 54.08° |

Ground release takes 0.108–0.133 s at 120–30 Hz; reversal crosses zero in 0.067 s.
Neutral-air yaw changes preserve initial horizontal velocity exactly. Straight
overspeed input adds no bonus. Explicit opposing input reverses in the test window.
Clean held hops retain 14 m/s across four seconds and at least five takeoffs.
A deliberate test-only angled strafe earns speed beyond 12 m/s from 10.8; runtime
has no such input generator. The emergency 18 m/s bound remains enforced.

The actual normal Campaign touch test injects two concurrent UI pointers through
the left joystick, right hold/look surface, input router, camera and motor. This
is not Android OS touch delivery. Whole-right acquisition, release/cancel/menu
ownership and editor/manual-camera regressions remain in the full suite.

Representative continuous authored routes and surf/Boost/platform checks are
reported in PRODUCTION_0150.md. Scripted pilots supply explicit stick corrections;
they do not change authored geometry or add runtime guidance. Feasibility times
are not human ranks. Legacy camera/motor comparison tests explicitly select their
archived profile so they remain meaningful after the normal default changes.
