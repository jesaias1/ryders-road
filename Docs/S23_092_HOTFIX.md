# 0.9.2 S23 frontend/loading hotfix

2026-09-06. Physical QA candidate only. Stop further Campaign production.

## Reproduced blocker and fix

The real Campaign/card and Spiral buttons selected the right stable IDs and
loaded ModuleRunner. Both reproductions had a live correct module and player,
but LoadingPresentation remained alpha=1 and blocksRaycasts=true. Evidence:
`Logs/phase092-reproduction-results.xml` (2 failing real-button tests) and the
initial entries in `Logs/Phase092VisualQA/navigation-trace.txt`.

The UI scene MonoBehaviour owned StartCoroutine(LoadScene), which yielded the
loader iterator. Single-scene activation destroyed that caller; the iterator's
completion/finally notification after the async wait did not execute. Bootstrap
survived, explaining why the menu itself loaded. The earlier test drove the
iterator from a surviving test runner and missed this lifecycle.

SceneTransitionHost now owns the request and AsyncOperation on a persistent root.
ILevelLoader keeps its existing public iterator; callers only observe requests.
Menu and ModuleRunner report explicit readiness after initialization. The host
checks requested scene/module identity, automatic activation, and readiness,
then publishes completion after a normal render frame. Presentation exceptions
cannot interrupt loading. The overlay also observes terminal request state;
alpha and touch interception are released together. Retry and Next use the same
path, including replacement of ModuleRunner by another ModuleRunner instance.

No additive loading, cancellation tokens, or media-controlled activation were
involved. LoadingTransitionProfile provides a 20 s activation/readiness deadline,
2 s first-frame/stall watchdog, and 0.18 s reveal. There is no forced video duration.
A failed transition reports actual scene/module, progress, activation/readiness,
service/bootstrap availability, overlay and video state in development logs and
shows a recovery action in all builds. An unfinished Unity scene operation cannot
be cancelled; recovery refuses competing loads and advises app restart if needed.

## Video evidence and configuration

The exact Android-specific reason for 0.9.1 missing motion is **not established**.
No S23 was connected in the one ADB inventory check, and no device decoder logs
were available. The old asset decodes and advances frames in Editor, so blaming
its codec as the confirmed release blocker would be unsupported.

Original file retained unchanged:
`C:/Users/lin4s/Downloads/Ryders Block Assets/Ryders Road Loading Screen.mp4`.
Runtime resource:
`Assets/_Game/UI/Resources/Loading/RydersRoad_Loading.mp4`.
The derivative is 1280x720, 24 fps, 11 s, H.264 Main level 3.1, yuv420p,
CRF 20, no B frames, 24-frame GOP, fast-start MP4, video only. Source aspect and
content are retained. Transcode evidence: `Logs/phase092-video-transcode.log`.

VideoPlayer explicitly uses VideoClip source and a persistent 1280x720 ARGB32
RenderTexture, no audio output, loop enabled, wait-for-first-frame, and DSP clock.
The texture resolution is decoding resolution, not a fixed game viewport.
Prepare begins ahead of requests; prepared playback is retained across transitions.
Frame polling switches the poster to decoded output without depending solely on
a frameReady callback. A tested UnscaledGameTime configuration stalled after
pause/resume in this Editor run; DSPTime passed the actual advancement check.
This is not evidence about the previous Android failure.

Missing/disabled media, decoder error, or first-frame/stall timeout uses the poster
with a development warning. Gameplay readiness dismisses either video or poster;
loopPointReached is never awaited. A quick load may finish before motion is visible.
The source uses Resources/VideoClip packaging, not a filesystem URL.

## Fullscreen and preservation

The confirmed artificial bars were AspectRatioFitter.FitInParent on 16:9 loading
and menu backgrounds, exposing navy canvas behind them on wider displays. Both
now use EnvelopeParent/COVER with cropped overflow. Backgrounds are outside
safe-area parents; interactive foreground remains within SafeAreaFitter. Canvas
reference resolution controls UI scale, not a 1920x1080 viewport. Main and gameplay
camera rects remain (0,0,1,1); no fixed-resolution gameplay render target was added.

Existing Android FullScreenWindow, renderOutsideSafeArea, hidden status/navigation
insets, transparent bars, short-edge cutout and immersive-sticky fallback remain.
Both landscape orientations are allowed. Bootstrap reapplies on focus/resume with
bounded startup retries, not permanent enforcement against revealed navigation.
Physical system-bar, notch, rotation, resume and flash behavior remains unverified.

Branding, menu design, world art, modules, movement, Classic camera, editor controls,
V4 saves and Bronze progression remain unchanged. Module 003 content remains v6.

## Verification

- Source validation passed: `Logs/phase092-source-validation.log`.
- EditMode: 213/213, `Logs/phase092-editmode-final-results.xml`.
- PlayMode: 19/19, `Logs/phase092-playmode-final-results.xml`.
- Eight new real-navigation cases cover independent Campaign and Spiral entry,
  Module 003 card entry, actual Patch completion/results/Retry/Next, missing video,
  throwing presentation subscriber, missing scene/recovery, and readiness timeout.
- Render/decoded-frame inspection: `Logs/Phase092VisualQA/menu.png`, `campaign.png`,
  `loading.png`, `module003.png`, `decoded-video-frame.png`. S23-shaped 1560x720
  viewport, COVER bounds and full camera rect assertions passed. The decoded
  source frame is separately 1280x720. Captures are Editor evidence, not phone QA.
- Existing movement/manual camera, editor, collision/mechanic and progression
  regressions remained green. Test completion calls the real Patch contract;
  it does not represent physically playing a route or qualifying a rank time.

## APK

Built successfully: `Builds/Android/RYDERS-ROAD-0.9.2-s23-loading-fullscreen-hotfix-dev.apk`.

- APK size: 179,015,667 bytes.
- SHA-256: `e7bac5342c3f7239ebe32ed3de90ffb419af1c9bfd475ce107c51cccd30f1d2c`.
- Build: 0 errors, 1 legacy-icon warning; `Logs/phase092-android-build.log`.
- Manifest: version `0.9.2-s23-loading-fullscreen-hotfix`, unchanged package
  `com.rydersblockstudio.rydersblock`, ARM64-only, minimum API 26, target API 36,
  user-landscape orientation (0xb). Evidence: `Logs/phase092-apk-badging.txt`
  and `Logs/phase092-apk-manifest.txt`.
- Exact 3,705,679-byte runtime MP4 verified inside
  `assets/bin/Data/e8d1a1ae7dc5eb14b96aa9bc0fac61ba.resource` at offset 0.
  Video SHA-256: `aa9fc44d1612c720632310c129e672b19b61075dfe221bfed8e5ccc055df1805`.
  Evidence: `Logs/phase092-package-verification.json`.
- Original source file was only read for transcoding; no replacement or deletion.
- No physical S23 testing or Android decoder verification is claimed.

## Physical S23 acceptance — then stop

1. Cold launch and main menu fill the display without left/right bars.
2. Campaign and Spiral both reach gameplay; Module 003 can be entered.
3. Loading video visibly moves when load duration permits; poster is only the
   preparation/failure fallback, and loading never becomes stuck.
4. Gameplay controls work after transitions; Patch results Retry and Next work.
5. Home/resume preserves immersive mode, both landscapes remain usable, and there
   are no black flashes or Unity-default frames during transitions.

No physical-device testing was performed by the agent. Wait for user feedback
before any further Campaign or world production.
