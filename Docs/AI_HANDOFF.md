# Current authority — 0.12.0 vertical slice

Updated 2026-09-09. ADR 0024, ADR 0025 and PRODUCTION_0120.md are current authority.
The user authorized a visual-reference follow-up after delivery; keep the completed
slice as the base. Physical feedback remains pending. Worlds 006–008 remain unbuilt.
The old 0.11.0 recommendation to start World 006 is superseded.

## Current visual references

See [reference index](VisualReferences/README.md): September 9 gameplay quality
(primary finish/composition target), menu concept and loading brand reference.
Original August target remains required context. ART_DIRECTION.md defines their
Unity/mobile interpretation. Do not treat images as layouts, mechanics, exact UI
copy or evidence of implementation. Preserve real responsive UI, separate assets,
Ryder's Road-led transitions, secondary Jesaias and no artificial loading delays.
This follow-up changes docs/reference files only. The delivered APK still uses
the previous approved runtime logo; white-on-navy branding and the concept's
finished environmental quality are direction, not completed runtime changes.

## Accepted foundation

The user completed S23 movement comparisons and chose accepted Campaign controls
for predictable movement and natural looking. Bhop is acceptable; chasing exact
mouse-style air strafing is not the production priority. Landing/Flow is too wild
for normal routes and is not globally promoted. Motor/input/camera source and
tuning, saved preferences, Editor controls and all isolated trials are preserved.
This is a provisional production foundation, not a freeze or Gold approval.

## Completed work

- Five existing sequential roads form the slice: Sky City, Mountain, Ancient
  Abyss, Solar Foundry and Windward. Five is a deliberate bounded exception to
  3–4, preserving unlock meaning while including the requested world revisions.
  First-road guidance and finale acknowledgment extend real Campaign systems.
  Bronze remains sufficient; Spiral is separate; V4 and stable IDs are unchanged.
- Windward v2 replaces the walkway with 25 standard jumps, 1.39–2.70 m edge gaps,
  3.2–6 m landings, a larger wind arc and diagonal ascent. Three Restores retain
  their IDs and face onward. The five-pad optional chord bypasses eleven arc
  landings and needs later takeoffs. Existing Abyss/Foundry Boost opportunities
  remain; no new ramp/surf mechanic or movement rewrite was needed.
- Title/Campaign hierarchy, card readability and results spacing improve.
  Continue and More Roads have separate hit areas. Flow Lab/trials are nested
  under development access; Settings and both landscape safe areas remain.
- Native Jesaias startup and editable source remain. Ordinary loading uses the
  approved game logo and an unscaled travelling indicator. Budgeted world/route
  construction yields between work batches. Identity resolves in Awake; player
  and attempt creation follow construction. Persistent host, readiness, automatic
  activation, timeout and recovery contracts remain; no artificial logo hold.
- Mountain's waterfall landmark has new snow ridges, a source cleft, ledge pools,
  two cascades and split plunge. It faces the route from outside traversal bounds.
  Mountain route/near collision is preserved. Shaded water, quieter ambient fill
  and opt-in near shadows improve four worlds. Sky City and continuous skies are
  preserved. Original offline Patch/Boost cues extend shared audio profiles;
  licensed foley, footsteps, Restores, wind and master-volume control remain.

## Verification and artifact

245/245 EditMode and 90/90 PlayMode pass; source/LFS hydration checks pass.
Coverage includes a continuous accepted-motor Windward run without per-link
resets, every standard/chord jump, failed no-jump walks at all standard links,
Restores/collision, Bronze/finale/PB replay, live loading-indicator movement,
navigation/recovery, trial save isolation and prior movement/camera/Editor checks.
Final logs/XML: Logs/production120-final2-editmode.* and
Logs/production120-final-playmode.*. Reviewed 25 player-camera views, 20 sky
directions, waterfall views and frontend/loading/finale. Tracked evidence:
Docs/Production120QA. Older local PNGs may coexist; tracked sheets use final views.

Android ARM64 IL2CPP development APK:
`C:\Users\lin4s\Documents\Riders Block\Builds\Android\RYDERS-ROAD-0.12.0-vertical-slice-dev.apk`
Package com.rydersblockstudio.rydersblock, code 1, min SDK 26 / target 36.
Final build size/hash/results are in PRODUCTION_0120.md and
Docs/Production120QA/validation.json. No device was queried, installed or tested.

## Remaining limits and next direction

Physical comfort, chord timing, ranks, waterfall/art approval, speaker/headphone
mix and sustained S23 performance remain open. Old Windward PBs retain historical
content metadata and are not directly comparable to v2. Individual resource loads,
Instantiate and static batching can still briefly stall the main thread. Near
shadows add GPU work. Shared block art remains visible. No Gold designation.

S23 checklist: exact-build cold launch/resume/both landscapes; accepted manual
look/tap-jump; Windward normal/chord routes and Restores; Bronze/finale/Retry/PB;
Mountain waterfall/full sky turns/landing contrast; existing Boost/ferry flow;
master volume/pause; 10–15 minute thermal/frame-pacing run. Stop for this feedback.

Private origin/main: https://github.com/jesaias1/ryders-road.git. Starting commit
d9852a1f591510b02e2174f1a23dff9c256aaef1 was clean. This handoff ships with the
milestone; final delivery records its SHA/push status. APK, raw logs, backups and
caches remain ignored. Fresh checkout: git lfs pull; git lfs checkout;
Tools/Validate-LfsAssets.ps1. Historical details remain in production reports/Git.
