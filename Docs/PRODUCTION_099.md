# 0.9.9 — S23 fixes and Campaign Flow trial

Baseline checkpoint: `38d222a`. Physical evidence is the user's S23 report of
0.9.8, received 2026-09-08. Decision: ADR 0021.

## What changed

- Replaced the cross-course overhead crane with an outside side structure. The
  ferry and its destination now have open sky through the jump. Camera helpers
  did not compensate for geometry. The ordinary route remains equally accessible.
- Audited Foundry's lower architecture: its solid ledges sat above the void
  threshold and could hold the player indefinitely. Explicit scenery landing
  markers now request normal Restore recovery on walkable contacts. Walls remain
  solid, side brushes do not reset, and route/shortcut supports are not marked.
  Three Restore Points, shard retention and mechanism resets retain their rules.
- Home → CAMPAIGN FLOW TRIAL makes all four real roads available for isolated
  comparison. A uses accepted motor/saved controls. B uses the promising existing
  Flow motor/manual pitch. C uses that motor with the existing landing-view
  candidate: initial 16° down, up to 8° extra on descent, 24° framed target cap,
  36°/s limit, manual drag takeover and 1.2 s hold. No new motor tuning was made.
- Trial HUD shows time, speed, peak speed, jumps and a separate session best.
  Completion/Retry work; returning home clears the trial. Normal Campaign remains
  unchanged. Trials make no save-service writes, including attempts and rewards,
  and do not persist the Flow control preference.
- Foundry content version is 2; its ID and every route/platform/Restore/Patch ID
  remain. Historical saves/PBs retain recorded meaning and content versions.
- Reviewed current Sky City, Mountain and Foundry gameplay captures against the
  visual target. ART_DIRECTION.md records the actual cohesion mismatch and a
  shared material/detail/motif direction. Only Foundry composition changes ship
  here; this does not claim a completed art-cohesion pass.

## Verification

Focused PlayMode: 7/7 in `Logs/production099-focused-final.xml`.
Tests cover real falls onto sampled low ledges, side-contact rejection, ordinary
and shortcut supports, a transfer view cone at grounded/jumping eye heights,
whole ferry/rider clearance and carry, Restore/shards/crumble/retry, and actual
standard-route jumps with accepted and candidate Flow inputs. These are controlled
individual jumps, not a continuous human speedrun.

All four roads × three trial modes load through the real runner. Memory-backed
save tests verify no writes or mutations across launch, completion and Retry,
unchanged control preference, session time isolation, and a normal baseline run
after exit. Real touch tap/vertical drag and the landing-view Restore default are
checked. Existing movement tests measure skill gain and guard against yaw-only
energy at 30/60/120 Hz; a higher displayed speed alone is not treated as success.

Actual gameplay-FOV captures reviewed: `Logs/Production099QA/trial-menu.png`,
`foundry-flow.png`, `trial-results.png`, and revised five-place Foundry renders
copied into that directory. The beam is out of the jump view and the optional
line remains visible. No generic post-process filter was added.

adb was checked once: no attached device. No new APK installation, physical
movement test, audio audition, FPS/thermal test or physical approval is claimed.
The user's preference for Flow is recorded; the replacement decision remains open.

## S23 checklist

1. Normal Campaign → Solar Foundry: ride the ferry and jump its exit; confirm the
   overhead obstruction is gone. Drop onto low pipes/braces and confirm prompt
   recovery to the latest Restore. Check ordinary platforms and shard shortcut.
2. Home → CAMPAIGN FLOW TRIAL. Start C on Solar Foundry, then Sky City and Mountain
   or Abyss. Can you see near edges and lower landings while chaining jumps?
3. Compare B on the same road. Right tap jumps; vertical drag adjusts view. Does
   C's modest downward framing help, interfere, or need a different default?
   A remains available as the accepted reference. Use the same route each time.
4. Try gentle left-thumb arcs and timely repeat taps versus stopping to line up.
   Compare time/speed and misses; do not judge by top speed alone. Check Restore,
   Retry, both landscape orientations and normal Campaign after exiting trials.

## Continue

Use the physical comparison to choose the foundation of Campaign controls; the
old manual scheme is not assumed to be the final design. Avoid speculative
parameter churn before that decision. Independent work remains authorized:
remaining event audio and listening review, then a complete fifth world with
ordinary flowing lines plus optional skill cuts, following the shared visual
rules. Worlds 005–008 remain unbuilt. Neither fixes nor trials are Gold or frozen.

## Final build evidence

- EditMode **240/240**: `Logs/production099-editmode.xml`.
- PlayMode **57/57**: `Logs/production099-playmode.xml`.
- Post-build source validation: `Logs/production099-source-validation-final.log`.
- Candidate measurement at 60 Hz: normalized curved input 9.222 m/s versus
  straight 8.200; yaw without throttle stays 8.200. This is a controlled airborne
  motor measurement, not a physical speedrun or proof of phone feel.
- Player/motor profiles, Input, SaveSystem and first three Campaign module assets
  match `38d222a`. Normal camera behavior remains regression-tested; only the
  opt-in landing-view Restore default changes in the camera implementation.
- Android build: **Succeeded, 0 errors / 1 existing legacy-icon warning**,
  `Logs/production099-build.log`. ARM64, unchanged package
  `com.rydersblockstudio.rydersblock`, min SDK 26 / target SDK 36.
- APK: `Builds/Android/RYDERS-ROAD-0.9.9-campaign-flow-trial-dev.apk`;
  184,845,229 bytes. SHA256:
  `078895133B6220DF457847C6B1F4C9136B7B1DD86E871760098C038560EB7533`.
  Manifest/hash evidence: `Logs/production099-package.json`.
