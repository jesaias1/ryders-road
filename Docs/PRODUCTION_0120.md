# 0.12.0 — playable Campaign vertical slice

Starting checkpoint: d9852a1f591510b02e2174f1a23dff9c256aaef1; clean working tree.
Authority: ADR 0024. User physical feedback accepts Campaign controls as the
production foundation for now and rejects Landing/Flow as the ordinary default.
No motor, input, camera or movement-profile changes. Bhop, momentum, Editor
controls, saved settings and all isolated experiments remain available.

## Scope and journey

Use the existing five-road Campaign rather than skipping middle unlocks: Sky City
introduces movement, Mountain develops moving crossings, Ancient Abyss adds Boost,
ferry and pressure, Solar Foundry develops mechanical flow, Windward concludes
with deliberate jumps and an optional chord. This compact five-road exception
preserves existing progress and showcases the requested Mountain and Windward
work. Spiral remains its own legacy challenge; Worlds 006–008 remain unbuilt.

First-road guidance explains movement/look/jump, then cyan Restores/gold Patch,
and clears after three jumps. Completed first roads do not repeat it. Final-road
results acknowledge five restored roads, retain rank/PB feedback and offer replay.
Bronze remains sufficient; no economy, save rewrite or narrative system.

## Windward v2

The previous 4.6 m pads at roughly 4.7–5 m spacing were nearly a walkway. The new
route uses 6 m approach spacing, a 22 m wind arc with 3.2 m landings, diagonal
ascent jumps, a turn at the lens and a broad final Patch landing. Ordinary landing
widths are 3.2–6 m, with 1.39–2.70 m geometric separation at every standard link.
There are 26 standard landings / 25 jumps, about 154.5 m of centerline travel.
Three Restores remain; east and lens restore headings face their onward route.
All stable block, shortcut, Restore and Patch IDs are retained; content is v2.
The five-pad chord spans the inner instrument and bypasses eleven arc landings.
It asks for a later takeoff, preserving optional skill without Campaign gating.
Existing Abyss/Foundry Boost systems supply speed opportunities; no new ramp/surf
mechanic was needed. New restrained Boost feedback uses the existing event hook.

## Frontend and transitions

The title retains the floating-world artwork, with a centered panel, gold Campaign
action, smaller challenge/settings actions and quieter progress text. Experiments
are in a development submenu, retained in Editor/development builds. Campaign
Continue and More Roads no longer overlap. Both landscapes retain safe areas.

Native startup retains the supplied editable Jesaias source and startup image.
Ordinary transitions use the approved Ryder's Road logo, dark background and
unscaled travelling gold indicator. No video or intentional minimum hold.
World/route construction yields when its editable 4 ms budget is exceeded, so
loading can animate between work batches. Individual Instantiate, resource-load
and static-batch operations remain synchronous and can still cause brief stalls.
Player/attempt creation follows construction; persistent transition readiness,
timeouts, automatic activation and menu recovery are retained.

## Environment and audio

Mountain's rejected strip is rebuilt as a snow-fed cleft, shelves with impact
pools, two cascades and a widening split plunge. The landmark is oriented toward
the route and placed outside its clearance envelope. Snow ridges replace the
old jagged cylindrical crowns. Shared mountain route/near collision is unchanged.
Water has directional/ambient surface shading, scrolling streaks and fog, using
opaque geometry without a particle blanket or transparent floor.

Mountain, Abyss, Foundry and Windward use quieter cool ambient fill against their
authored warm sun and opt-in near-architecture shadows. Sky City is preserved.
Continuous directional skies, fog identities and the absence of heavy grading
remain. Physical S23 performance and final visual approval are still open.

Existing licensed jump/landing/UI/Crumble sounds and quiet footsteps/Restore/wind
remain. Original offline Patch ceramic resonances and a short Boost release use
GameplayAudioProfile and existing MovementFeedback/master-volume contracts.
Source generator: Tools/Render-SliceAudio.py; attribution: THIRD_PARTY_ASSETS.md.
No music system or new third-party assets.

## Validation and delivery

Final suites: **245/245 EditMode, 90/90 PlayMode**, zero failures. XML/logs:
`Logs/production120-final2-editmode.*` and `Logs/production120-final-playmode.*`.
Source validation and LFS hydration pass. Coverage includes geometric gap/landing
width, every standard and optional Windward link with the accepted motor, a
continuous stop-and-go standard run with no per-link resets, inability to walk
any standard link, three Restores, matching collision, Bronze unlock/replay,
finale feedback, actual loading-indicator travel across construction frames,
nonoverlapping Campaign actions, existing loading recovery and all isolated trial
road/mode saves. Prior motor/manual-camera/Editor checks remain green.

The initial focused pass found a marginal chord test takeoff and a test capture
without a camera during construction. The chord now uses a later takeoff in its
fixture; normal route fixtures keep a more generous margin. Full suite clearance
also rejected early waterfall placements; the final landmark is farther outside
all standard and optional approach bounds. Tests were not weakened to accept it.

Reviewed 25 final player-camera route views, 20 sky directions, two waterfall
views, title, Campaign, loading and finale. Final contact sheets/key views are in
`Docs/Production120QA`; full PNGs remain in `Logs/Production120QA`. Earlier files
in the local log directory are not all final; tracked sheets select the last five
route views per world. This is Editor rendering and simulated input, not a claim
of fun or physical touch approval. No obvious longitude/texture-wrap cut was
identified in the reviewed directions; phone rotation review remains necessary.

Mountain has 62,348 world triangles, 169 renderers before existing static
batching, 47 placements; Windward remains below its 35,000-triangle guard. URP
keeps one 2048 main-light shadow map, one cascade and a 50 m shadow distance; no
additional shadowed lights or postprocessing features were added. Near shadow
casters increase GPU work and still need device frame-pacing/thermal measurement.

No phone was queried, installed, launched or physically played. No sustained
60 FPS, final audio mix, calibrated rank or Gold claim is made.

### Remaining limitations and physical checklist

- This is a five-road candidate, not a finished full Campaign. World geometry
  still uses the shared block kit; further place-specific art refinement may
  follow physical feedback. Waterfall composition/shading is revised, not
  physically approved or photorealistic water.
- Windward thresholds and its shortcut time-saving estimate are provisional.
  Historical PBs and content-version metadata remain unchanged; older-layout
  Windward PBs are not directly comparable to the longer v2 route.
- Individual resource loads, prefab instantiation and static batching can still
  briefly stall the main thread. Construction yields between these operations,
  not inside a single Unity call. No artificial logo hold hides this limitation.
- Original Patch/Boost cues and stronger near shadows need speaker/headphone and
  S23 performance review. A successful APK does not establish phone comfort.

1. Install the exact APK below. Cold launch/resume; confirm studio identity only
   at startup, game logo/travelling indicator on longer transitions, and no
   blocked controls or black flashes. Rotate both landscape ways.
2. Start/replay Sky City: check manual looking/tap-jump guidance and accepted
   bhop/controls. Check Settings, Campaign pagination and Continue separately.
3. Complete Windward's broad standard route; verify jumps are required and fair,
   three Restores face onward, the optional chord saves effort/time, and no
   scenery strands Ryder. Retry, finish, check Bronze/finale and PB replay.
4. Inspect Mountain waterfall from garden/ridge, platform contrast, shadows and
   full sky turns. Check Abyss/Foundry Boost/ferry interactions and their cues.
5. Test master volume/pause on speakers and headphones; play 10–15 minutes for
   frame pacing, heat and battery. Report exact road/link and control mode for
   any problem.

STOP after delivery for this feedback. Do not begin Worlds 006–008 or reopen the
movement/camera foundation without a concrete physical finding.


### Android artifact and Git checkpoint

ARM64 IL2CPP development build succeeded: **0 errors**, 1 existing Unity legacy
icon warning, 2m22s. ZIP CRC and APK package/ABI inspection pass. Package remains
`com.rydersblockstudio.rydersblock`; version `0.12.0-vertical-slice`, version code
1, minimum SDK 26 / target SDK 36. Exact APK:

`C:\Users\lin4s\Documents\Riders Block\Builds\Android\RYDERS-ROAD-0.12.0-vertical-slice-dev.apk`

182,971,646 bytes. SHA256:
`E86A7EC5C5629AB84307E84D326272311266841AAE11C8C731BBF7450CAB4FE9`.
Build log: `Logs/production120-android-build.log`. Compact tracked evidence:
[Production120QA/validation.json](Production120QA/validation.json). APK and raw
logs remain local/ignored. No device connection or installation was attempted.

This report and AI_HANDOFF ship in the coherent milestone commit on the existing
private origin/main. The final delivery response records the exact SHA and push
result. No history rewrite, public repository change, backups or build outputs.
