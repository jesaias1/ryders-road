# 0.16.1 — Windward route cadence correction

User physical feedback rejects the oversized 0.16.0 deck chain. Windward
Observatory (module.005.foundry-pulse) is the only redesigned Campaign level.
It is now content version 5; existing PB history and campaign unlocks survive.

## Route changes

25 supports become 22: the three separate chord decks are removed. The previously
removed ascent.d stays removed. All 22 remaining supports form one shared sweep.
The arrival is an 8 x 14 m setup runway. Approach pads alternate 5 x 3, 7 x 4 and
5 x 4 m surfaces, with actual commitment gaps. West Restore is a 10 x 10 m anchor.
The north sweep turns through offset 7–8 m galleries into two pressure fins
(3 x 5 and 5 x 6 m), then a wider exit and an 11 x 11 m east Restore. Pressure
collapse delay is 1.8 seconds. The rising diagonal alternates 7 x 7, 5 x 4 and
8 x 6 m landings. The former 28 x 14 m lens terrace becomes 12 x 10 m. Its final
approach alternates 7 x 8, 5 x 4 and 8 x 6 m before the 12 x 12 m Patch anchor.
Only five supports have at least 100 square metres of area. Every adjacent beat
has at least two metres of horizontal open separation; no near-touching walkway.

Safe players use every landing and pause at recovery anchors. Flow players take
the same sweep with fewer pauses, carrying speed through the north exit and
rising diagonal. Carried-speed skip opportunities omit approach.a, approach.b,
the first pressure fin, or lens.b. Expert players combine these omissions with
off-angle landings and manual air steering. There are no separate difficulty
routes. Feasibility tests using controlled incoming speed do not prove that a
human can earn that speed in every preceding segment, or establish rank times.

## World and scenery

Repeated hanging braces are removed. Five tapered observatory foundations ground
the anchor spaces; compact dark housings sit beneath intermediate decks. Existing
wind-instrument and sail silhouettes frame the route. The telescope moves below
the revised finish. These are actual baked meshes with retained fatal collision,
not newly traversable lower rescue surfaces. All-face fatal scenery behavior from
0.16.0 is retained and regression-tested across Campaign; no new scenery rules.

## Preserved systems

Movement/profile compatibility 5, acceleration, air control, bhop, jump arc, cap,
manual/touch/editor controls, camera, music, branding, other Campaign route assets,
Restore/Patch behavior, ranking and V4 save meaning are unchanged. Diamond remains
an explicitly provisional 19-second target; Bronze remains available for any valid
completion. No threshold is declared calibrated by the scripted pilots.

## Verification and delivery

Full EditMode: 249/249 passed (Logs/cadence-editmode.xml).
Full PlayMode: 151/151 passed (Logs/cadence-playmode-final.xml).
Source validation and LFS hydration passed. Every one of 21 adjacent gaps rejects
walking. Shared Campaign and development-trial geometry both complete continuously.
Scripted safe timing is 31.400 s; fewer pauses, 26.750 s. Slower 70% ground approach
with full airborne correction takes 47.716 s. A separate 90% stop/go test without
predictive airborne aiming also passes. These are test pilots, not human timing.

Four controlled 17 m/s skips pass both centred and 0.75 m offset aim: arrival to
approach.b, approach.a to approach.c, arc.03 to arc.05, lens.a to lens.c. Recorded
flights span 10.11–10.20 m, with landing speeds of 13.41–16.79 m/s. Incoming speed
is injected only in the test fixture; no runtime boost or assistance was added.
Scenery audit covers 253 retained colliders and five actual fatal drops in each
of the five solid-scenery Campaign worlds. Normal decks remain valid.
Runtime overview and five route views were inspected; see Cadence161QA.

Android development build succeeded with zero errors and one existing Unity
legacy-icon warning. Package and v2 signature verification passed; ZIP integrity
passed (651 entries).

- APK: `C:\Users\lin4s\Documents\Riders Block\Builds\Android\RYDERS-ROAD-0.16.1-route-cadence-dev.apk`
- Bytes: 182,889,711
- SHA-256: `9BBA2EBC2E37EC3930B01191DA887515BBA0D0DEC5418E97FD813FA9A2693D4B`
- Package: `com.rydersblockstudio.rydersblock`
- Version: `0.16.1-route-cadence`; ARM64; min SDK 26, target SDK 36
- Build log: `Logs/cadence-android-build.log` No physical device test, FPS, thermal,
multitouch comfort, human completion timing or final art acceptance is claimed.
After delivery, stop for physical S23 feedback.
