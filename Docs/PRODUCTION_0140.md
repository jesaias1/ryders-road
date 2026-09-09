# 0.14.0 - movement foundation physical candidate

2026-09-09. Implemented as movement.foundation-v1, compatibility 4, in the existing
motor. This is the intended future shared standard, pending the user's physical
approval. It is not Gold and has not been tested on a physical Android device.
The S23 is disconnected; no adb install or device wait was attempted.

## Start and compare

From the title choose DEVELOPMENT > CAMPAIGN FLOW TRIAL. E - FOUNDATION is selected
by default; choose any of the five existing roads and START TRIAL. A - ACCEPTED
compares the accepted motor and saved controls on the same geometry. B/C/D retain
the previous experiments. All trial results stay session-only. Ordinary Campaign
still uses compatibility 1 and its existing PB/progression behavior.

Left thumb moves in the view-relative forward/back/left/right direction. Start the
right thumb on the inward Jump glyph to jump and hold it; drag that same contact
to look while hopping. Keep holding through landings for automatic repeat jumps.
Release to stop the hold. Outside the glyph, drag to look without holding; a quick
right-side tap still jumps. Looking across the glyph after starting elsewhere
does not arm it. Editor WASD/mouse and held Space use the same candidate physics.

For the existing diagnostic surf and bhop rooms, enter FLOW LAB and cycle MOTOR
A/B/D/E three times from its initial B to E. ROOM selects the venue. Leaving E
restores the previous control geometry. Release Jump to ride the surf surface;
holding it requests a jump as soon as valid surface contact allows it.

## Engineering and tuning

ADR 0027 explains the full model, inspected Quake/Source references and rollback.
Reuse ADR 0022's projected wish acceleration, post-acceleration energy bound,
controller-contact grounding, collision clipping, surf detach and hop grace.
The candidate uses full two-axis manual wish input rather than Flow's x-to-yaw
mapping. Acceleration remains 26 m/s^2; opposing input uses 30 m/s^2. An adaptive
wish cap max(8.2 * input amount, speed * .95) permits deliberate high-speed oblique
corrections while straight overspeed input stays capped. There is no velocity-
derived wish angle, automatic strafe or speed gain from camera rotation alone.
The existing 10.8 soft/18 hard m/s envelope remains; boost overspeed is retained.

Clean landings retain 100% rather than 99% momentum. Contact-rearmed held jumps
reuse the tap buffer and motor jump eligibility; air holding cannot double jump.
A buffered tap waits out the candidate's movement lock. Accepted consumption
ordering is preserved. Grounded aligned overspeed is retained without snapping to
10.8; ordinary stopping and reversing still brake. Jump height 1.66 m, .335 s apex,
asymmetric gravity, ordinary takeoff and timing windows stay at prior values.
These are deliberate mobile adaptations, not exact Quake III/Source movement.

IHeldJumpInputSource extends capabilities without changing IPlayerInputSource.
The existing touch ownership registry holds the Jump pointer; Jump IDragHandler
passes deltas through the same normalization/sensitivity as the look surface.
The layout asset owns the inner rest center and button size. Existing safe-area
parenting and both landscape settings remain. No camera solver, art, route,
mechanic, world or Campaign rank changes were needed.

## Verification

- Full Unity 6000.5.6f1 EditMode: 246/246 passed (foundation-final-edit.xml).
- Full PlayMode: 123/123 passed (foundation-final-play.xml).
- Final focused motor/Flow Lab checks: 68/68 passed after the candidate-only lock
  correction and two additional tests (foundation-final-contracts.xml).
- Source foundation validator and LFS hydration checks passed.
- Real CharacterController checks cover held landing chains/release and no free
  energy at 30/50/60/120 Hz; ordinary/high-speed left/right correction, yaw-only
  coasting and reversal; taps/buffering/coyote/no double jump; wall clipping,
  slope/surf contact and exit, and restore clearing. Candidate buffer survives lock.
- UI tests drive the real Jump down/drag/up handlers and input router, apply the
  camera look, reject an unrelated pointer release, retain left movement, and clear
  held state on reset. They do not certify physical thumb comfort or OS touch loss.
- All five real roads load in all five trial modes. Actual completion/Retry/exit,
  memory-backed save write counts, PB/reward/progression bytes and control
  preferences remain isolated. Accepted Campaign returns on normal selection.
- Candidate ordinary Foundry jump/ferry links and the actual Abyss Boost trigger
  at 3/5/7.8 m/s approach pass with unchanged collision geometry.
- Continuous candidate Windward ordinary 25.116 s / direct 17.333 s and Foundry
  optional 19.850 s reach Patch. The test pilot uses explicit air braking and
  corrections, immediate heading changes and 60 Hz simulation, without per-link
  position or velocity resets. Constant-forward old pilot input overshot the fast
  Foundry transfer; corrected scripted inputs land on the unchanged supports.
  These are feasibility observations, not rank calibration or proof of good feel.
- Held-only input completes the actual Flow Lab bhop lane without repeated presses.
  Candidate surf-room entry/exit/retry and reversible profile controls pass.

Local complete XML/logs are under Logs/foundation-*. Retained machine-readable
summary and inspected trial/control images are under Movement140QA. Optional
RYDERS_FOUNDATION_CAPTURE=1 records scripted Editor evidence. The Windward clip
shows 12 continuous simulated seconds at 15 fps; the bhop clip shows held input
on the existing lane. Neither clip is phone input or human gameplay.

## Limits and physical checklist

No physical-device testing, 60 FPS/thermal certification or feel approval is claimed.
The Quake-inspired energy envelope is intentionally bounded. Very small oblique
inputs can still be projection-limited; meaningful direction changes require
intentional stick input. Surf remains the existing authored approximation and
walkable ramps keep CharacterController slope behavior. No new ramp-launch system
or whole-Campaign retuning was introduced. Continuous coverage is representative,
not exhaustive human play across every line and surface. Gesture/comfort judgement
must come from the S23, especially the boundary between the glyph and look area.

On S23 compare E with A on the same road:

1. Hold the Jump glyph and drag both yaw and pitch; release, tap, pause/resume and
   Restore. Check thumb comfort and accidental activation in both landscapes.
2. Chain hops; then deliberately stop holding. Confirm clean retention without
   unwanted extra jumps, and ordinary walk/run/jump accessibility.
3. At ordinary and faster speeds, move left/right, combine looking with movement,
   correct a poor approach, brake and reverse. Looking alone must not bend momentum.
4. Try Foundry transfers, Windward normal/direct lines, Abyss Boost and Flow Lab surf.
   Check landing predictability, retained speed and useful control after launches.
5. Confirm trials do not change Campaign PBs/rewards/unlocks; check sustained frame
   pacing and heat during 10-15 minutes. Report road, A/E, symptom and a short clip.

## Promotion / recovery

Baseline before this milestone: f78864629d3af76da7439daa1f3d0fb428869e7b.
Accepted compatibility-1 profile bytes and 0.13.0 content are preserved. Rejection
requires only selecting A or normal Campaign; prior APKs remain available locally.
After explicit S23 approval, promote one shared compatibility-4 model with an
explicit controls/versioned-PB/rank-calibration rollout. Never relabel historical
records or silently promote E. No save migration occurs in this candidate build.
STOP after APK/Git delivery for physical feedback; no Worlds 006-008 or PC port.

## Android artifact

Verified local output:
C:/Users/lin4s/Documents/Riders Block/Builds/Android/RYDERS-ROAD-0.14.0-movement-foundation-dev.apk
Build uses the existing AndroidDevelopmentBuilder, IL2CPP ARM64 development workflow.

Build succeeded in 149.932 seconds: 178,334,788 bytes, zero errors, one Unity
legacy-icon deprecation warning. APK CRC and aapt metadata passed: package
com.rydersblockstudio.rydersblock, versionCode 1, ARM64, min SDK 26 / target 36.
SHA-256: `74B51EA7D328133AB8D9CAD76E255C0361673A1E804D867B12E4DC3B6C7E0B65`.
The final capture-only replay passed 1/1 after refreshing its test HUD per frame;
this test-only adjustment does not change the built player. APKs stay local.

[Held-hop capture](Movement140QA/foundation-held-bhop-scripted.mp4) and
[continuous Windward capture](Movement140QA/foundation-windward-scripted.mp4) are
scripted Editor evidence, with no human/phone control or performance claim.
[Validation summary](Movement140QA/validation.json), [trial selector](Movement140QA/trial-menu.png)
and [candidate controls](Movement140QA/foundry-flow.png) are retained for review.
