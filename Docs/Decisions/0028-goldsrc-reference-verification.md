# ADR 0028 - GoldSrc reference verification

2026-09-09. The user supplied Xash3D FWGS and HLSDK as additional references for
the existing movement-foundation mission. This continues ADR 0027 and its 0.14.1
whole-right-surface correction. No engine replacement or additional motor.

## Reference and implementation boundary

Inspected current upstream HEADs: Xash3D FWGS
`1de8289f2980aa34c70760ae0ebfb2e00ddb0f9e` and hlsdk-portable
`1a1f11041c345c945fb64825530e86e79faa8f06`. Read engine/server/sv_pmove.c
and SDK pm_shared/pm_shared.c, including the SDK's restrictive header.
These are reference-only; no source, compiled engine code or assets are imported.
Technical findings and permanent source links: ../GOLDSRC_MOVEMENT_REFERENCE.md.

## Decision

Retain compatibility 4, the 0.14.1 input behavior and all movement profile values.
The inspected Foundation already implements explicit camera-relative two-axis
wish input, projected acceleration, no neutral-air drag, legitimate-contact
hold-hop and momentum-preserving clean takeoff. Its post-acceleration bound
preserves a heading change, unlike the older energy-intersection rejection.
Do not replace functioning movement or increase speeds merely to match a new
reference. Quantify correction at the bound and disclose its limits for S23 review.

Correct only the motor diagnostic AirAppliedWishDelta to report the actual net
velocity change projected onto wish after the energy bound. It previously exposed
the pre-bound allowance while the older candidate reported actual change. The
overlay and trial CSV now agree with AirNetDelta. RouteAirControlMath's public
out-parameter contract remains unchanged. Movement, contacts and timing are
identical; no physics compatibility bump or record migration is warranted.

Add real-controller tests of steering and alignment at 7.8/10.8/14/17 m/s,
post-bound diagnostic correctness for compatibility 3 and 4, and ground stopping
versus airborne coasting. Extend whole-right-surface testing to drive the real
left joystick pointer handlers as well as continuous right look/hold, camera,
router and motor. Existing representative Campaign, surf, Boost, release and
save-isolation tests remain required.

## Product and physical gate

One shared movement model remains the intended product direction. E is temporary
evaluation isolation, not a permanent advanced-physics setting. A/normal Campaign
and prior candidates remain recoverable. Trial save writes remain disabled;
official saves, PBs, history, progression, control preferences and ranks retain
their meanings. No global promotion, physical-device acceptance or phone FPS claim.

Deliver 0.14.2 Android APK, commit and push the coherent milestone, then stop for
S23 feedback. Evaluate the remaining envelope/ground-response questions before any
further tuning. See ../PRODUCTION_0142.md for measured evidence and exact artifact.
