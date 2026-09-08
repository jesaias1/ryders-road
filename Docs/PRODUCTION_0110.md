# 0.11.0 — world atmospheres and Windward Observatory

Implementation authority: ADR 0023. Starting checkpoint: `277b438` (0.10.0),
clean working tree. The user's physical 0.9.9 report provisionally prefers accepted
Campaign controls for predictability and natural looking. Accepted movement,
input and camera source/tuning assets remain unchanged. All 0.10.0 and earlier
trial variants remain recoverable. No global promotion, new movement buttons,
artificial strafing, physical approval or production freeze.

## Minimal loading and branding

`Assets/Branding/Source/Jesaias_Emblem.svg` is an editable contour trace of the
user-selected September 8 emblem, preserving both strokes, negative spaces and
diamond. No substitute logo/font. `Tools/Trace-Jesaias.py` regenerates the
transparent Unity PNG and `Assets/Branding/Android/Jesaias_Startup.png` from
that SVG. The original game logo and launcher icons remain unchanged.

LoadingPresentation uses a plain dark background, safe-area-centered emblem and
unscaled opacity pulse. No video decoder, RenderTexture, media-readiness wait or
intentional display delay. SceneTransitionHost still owns activation, readiness,
bounded failure and menu recovery. Missing emblem produces studio text.
Native startup matches the palette with zero logo hold. Android cold-launch
continuity, scaling and resume still require S23 confirmation.

## Atmosphere and existing worlds

The illustrated Ancient Abyss source was imported as an automatic cubemap. Its
horizontal borders differ by mean 11.65 and maximum 90 RGB levels (0–255): cloud
and island silhouettes do not meet. Seamless face filtering cannot repair source
content. Three other worlds used derivatives of the same illustration.

Campaign now uses `RydersRoad/World Sky`: continuous normalized 3D direction,
smooth zenith/horizon/depth gradients, three bounded noise octaves for cloud
structure and small directional sun glow. No longitude wrap, cube-face texture,
hard horizon plane, sky geometry or volumetric pass. Per-world materials and
existing environment profiles carry editable palette, fog, sun and ambient values.
Opt-in authored lighting selects flat ambient and bypasses the old 003 intensity
floor/ambient override. Legacy content retains its defaults.

- Sky City keeps its authored architecture and crisp blue identity.
- Mountain gets softer warm alpine light, blue-gray depth and matte materials.
  Existing split peaks, waterfall, garden and ridge composition remain intact.
- Ancient Abyss gets cool dramatic depth and warm directional ruin light.
- Solar Foundry gets amber industrial haze; its side-mounted crane, scenery
  recovery, ferry, shortcut, Crumble rules and route spacing remain intact.
- Windward uses lavender evening air, muted slate sails, porcelain and brass.

No existing-world route, collider or rank changes were made for visual polish.
Shared gameplay surfaces remain the high-contrast route layer. No new
postprocessing, fake floor, landing disc, route-center helper or arm pass.

## Fifth Campaign road

**Windward Observatory**, stable ID `module.005.foundry-pulse`, retains the
reserved fifth identity while evolving its unbuilt concept after Solar Foundry.
Arrival, west anchorage, broad wind arc, east anchorage, lens ascent and telescope
terrace form the road. The normal route uses broad ordinary jumps; five narrower
chord pads bypass eleven arc pads for optional mastery. Three Restore Points
bound recovery and a Patch Block finishes the road.

All architecture is original baked construction. Under-route braces and near
instruments have matching mesh collision with explicit scenery landing recovery.
Far sail banks are noncolliding and outside ordinary jump reach. The explicit
WindwardAuthoring tool writes serialized content; startup preserves it. World
geometry passes a 35,000-triangle ceiling.

The existing Campaign catalog appends 005 after Solar Foundry. Bronze on 004
unlocks 005; V4 saves and PB metadata retain their meaning. No migration, economy
extension or replacement progression system. Spiral remains separate and Worlds
006–008 remain unbuilt. New rank times are provisional; Bronze has no deadline.

## Audio

Existing licensed Kenney jump/landing/Crumble/UI foley remains. Quiet
distance-based ground footsteps reuse that source and reject airborne travel.
Restore Point uses the interface click; Restore uses softened contact foley.
The deterministic original wind bed is generated offline by Tools/Render-Wind.py:
one quiet mono source per world, paused with gameplay. Existing master volume
remains authoritative. No new third-party samples.

Dedicated completion, Boost, machinery and richer location-specific ambience
remain future sound work. The final speaker/headphone mix requires physical review.

## Validation

Source validation and LFS hydration pass. Final suites: **243/243 EditMode** and
**87/87 PlayMode**, zero failures. Logs/XML: `Logs/production110-final-*`.
Coverage includes accepted/candidate movement, manual camera/Editor controls,
Foundry scenery recovery and crane sightlines, all 20 Campaign trial road/mode
combinations and save isolation, loading/navigation/failure, every World 005
standard and shortcut jump, three Restores, Bronze unlock/Continue/replay and
ground-only footsteps. Initial outdated four-road/poster test expectations and
an ambiguous replay selector were corrected; both full suites then passed.

Reviewed 25 actual 94-degree player-camera views, four sky directions per world
and loading. The first cloud pass was rejected as blotchy; final clouds are
restrained with smooth depth gradients. No source-wrap cuts were visible in
reviewed final views. Tracked contact sheets: [Production110QA](Production110QA/).
Full-resolution originals: `Logs/Production110QA`. These are Editor renders.

No device was queried, installed, launched or physically played. Automated
evidence cannot establish phone comfort, art approval, calibrated ranks, speaker
mix, thermal behavior or sustained 60 FPS.


Android ARM64 IL2CPP development build **Succeeded**, zero errors, one existing
Unity legacy-icon deprecation warning, 2m50s. Package remains
`com.rydersblockstudio.rydersblock`, version `0.11.0-worlds-and-windward`, minimum
SDK 26 / target 36. ZIP CRC and ARM64 library checks pass. Exact APK:
`C:\Users\lin4s\Documents\Riders Block\Builds\Android\RYDERS-ROAD-0.11.0-worlds-and-windward-dev.apk`

182,818,218 bytes; SHA256:
`D2F2A181D36B0F7515FFB31B383997F725003AC6F786D5EA7D2B06477B66BAB7`.
Build log: `Logs/production110-android-build.log`. Compact tracked evidence:
[validation.json](Production110QA/validation.json). Traversal fixtures set up
individual takeoffs with real CharacterController simulation; they are not a
continuous human run or a phone-feel certification.

S23 physical checklist:
1. Install this exact APK; cold launch and resume. Check emblem, no blank flash,
   menu → road → Retry transitions and both landscape safe areas.
2. Check accepted controls, free looking and ordinary jump predictability;
   trials should remain separate from normal Campaign records.
3. Inspect sky in a full turn on each world, landing contrast, Mountain depth
   and Foundry crane visibility. Try falling onto Foundry lower scenery.
4. Complete 004 → Next → 005; finish the broad arc, use all Restores, try the
   inner chord, Retry and replay for a PB. Bronze must be sufficient.
5. Listen on speaker/headphones, vary master volume and pause/resume. Run for
   10–15 minutes to assess stable frame pacing, temperature and battery.

Stop for this feedback. Next recommended production is a bounded response to
physical findings, then reserved World 006 Broken Meridian. Do not resume a
camera tuning loop or mark movement/worlds/game Gold without physical approval.
