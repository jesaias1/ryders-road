# RYDER'S ROAD Alpha QA

Version: `0.4.7-brand-alpha-presentation`
Date: 2026-08-12

## Scope

This pass locks the player-facing rename to `RYDER'S ROAD`, integrates the
supplied logo/app icon assets, strengthens Android fullscreen handling, and
prepares a remote-install APK. It does not redesign movement, Smart Camera,
module rules, ranks, score, progression, or saves.

## P0 Audit

- Unity source validation, project validation, EditMode tests, PlayMode tests,
  and Android APK build must pass before release to a phone.
- Build/version labels must read `0.4.7-brand-alpha-presentation`.
- Android remains landscape-left/landscape-right, fullscreen, ARM64, IL2CPP.
- Android requested visible insets must be none, with transient system bars by
  swipe and runtime immersive reapply.
- Development shortcuts must not crowd normal gameplay.

## P1 Audit

- Default play remains left movement, right camera drag, right Tap Jump.
- Touch look sensitivity must be player-accessible and saved.
- App pause/focus loss must clear stale touches.
- Android launcher label must read `Ryder's Road` while package ID remains
  `com.rydersblockstudio.rydersblock`.
- THE SPIRAL development teleport cycle must remain available for review.
- Retry, Module Select, results, PB invalidation for development teleports, and
  rank display must remain intact.

## P2 Audit

- Runtime diagnostics should keep reporting route, rank, and performance counts
  for local review.
- `MobileBalanced` remains the default visual quality target.
- Visual density, HUD, controls, and hands remain prototype art pending phone
  review.

## P3 Audit

- Haptics remain skipped for this pass unless real device feedback proves they
  are necessary.
- Full graphics settings, reference ghosts, new levels, and new movement
  mechanics remain out of scope.

## Local Verification

- Baseline source validation before edits: passed.
- Baseline Unity project validation before edits: passed.
- Baseline EditMode tests before edits: 105/105 passed.
- Baseline PlayMode tests before edits: 4/4 passed.
- Post-change source validation: passed.
- Post-change Unity project validation: passed.
- Post-change EditMode tests: 123/123 passed.
- Post-change PlayMode tests: 4/4 passed.
- Android development build: succeeded with 0 warnings and 0 errors.
- APK copied to
  `C:\Users\lin4s\Downloads\RYDERS-ROAD-0.4.7-brand-alpha-dev.apk`.
- APK size: `119009577` bytes.
- APK SHA256:
  `022D4D724D94327D54AD2AFF65D06C420A05083B7C68A0BF5389A6AA49803D00`.

## Android Hotfix Verification - 2026-08-14

- Samsung Galaxy S23 connected through ADB:
  `SM-S911B`, package `com.rydersblockstudio.rydersblock`.
- Fixed Android immersive-mode `NullReferenceException` caused by disposing
  `currentActivity` before its UI-thread callback ran.
- Fixed title-screen `PLAY` and `SETTINGS` positions so they stay inside the
  visible S23 landscape safe area.
- Source validation after hotfix: passed.
- Unity project validation after hotfix: passed.
- EditMode tests after hotfix: 123/123 passed.
- PlayMode tests after hotfix: 4/4 passed.
- Android development build after hotfix: succeeded with 0 warnings and
  0 errors.
- Installed and launched the hotfix APK directly on the connected S23.
- ADB logcat after launch showed no `NullReferenceException` from immersive
  mode and no Unity development console crash.
- User physical-device confirmation: app is open on the phone and playable.
- Hotfix APK:
  `C:\Users\lin4s\Documents\Riders Block\Builds\Android\RYDERS-ROAD-0.4.7-brand-alpha-dev.apk`.
- Hotfix APK size: `121387850` bytes.
- Hotfix APK SHA256:
  `46D0EF590DA3FF30908CDEC4C0BECA87EA89767036C189079E8DA1296BBAB717`.

## Movement Feel Sweep Verification - 2026-08-14

- User physical-device feedback before this sweep: movement felt janky and
  unresponsive, and the first-person hands/graphics sometimes blinked or
  jittered.
- Restored Classic manual mobile play as the default:
  left movement, right camera drag, and right Tap Jump. Easy Smart Camera
  remains selectable for comparison.
- Tuned the default movement profile to
  `PARKOUR_RESPONSIVE_PHYSICAL_V2` with faster ground response, stronger air
  control, slightly snappier jump timing, and lower touch dead zones.
- Reduced touch-camera latency by feeding right-drag camera input on the same
  frame the drag is classified as camera movement.
- Calmed camera/head response and moved the first-person hands lower/outward
  with smoothed presentation and less overlapping cube geometry to reduce
  shimmer.
- Source validation after the feel sweep: passed.
- Unity project validation after the feel sweep: passed.
- EditMode tests after the feel sweep: 125/125 passed.
- PlayMode tests after the feel sweep: 4/4 passed.
- Android development build after the feel sweep: succeeded with 0 warnings
  and 0 errors.
- Installed and launched the feel-sweep APK directly on the connected S23.
- ADB foreground check confirmed
  `com.rydersblockstudio.rydersblock/com.unity3d.player.UnityPlayerActivity`
  as the resumed and focused activity.
- Focused ADB screenshot confirmed the title scene renders in landscape
  fullscreen without visible Android status/navigation bars.
- Focused Unity logcat did not show the previous immersive-mode
  `NullReferenceException`. It did show a Unity Android
  `AssetPackManager` class lookup exception, with no matching project-side
  Play Asset Delivery configuration found; treat as an optional direct-APK
  Unity probe unless it correlates with a visible device failure.
- Feel-sweep APK:
  `C:\Users\lin4s\Documents\Riders Block\Builds\Android\RYDERS-ROAD-0.4.7-brand-alpha-dev.apk`.
- Feel-sweep APK size: `216278475` bytes.
- Feel-sweep APK SHA256:
  `40AA2B3E4E892DCB3B561E5164EBD641D4AEEE7DF32A988F774D34B5794375D6`.

## S23 Device Smoke - 2026-08-16

- Samsung Galaxy S23 connected through ADB:
  `SM-S911B`, package `com.rydersblockstudio.rydersblock`.
- Installed current development APK directly on the connected S23 with
  `adb install -r -d`: succeeded.
- APK:
  `C:\Users\lin4s\Documents\Riders Block\Builds\Android\RYDERS-ROAD-0.4.7-brand-alpha-dev.apk`.
- APK size: `216281725` bytes.
- APK SHA256:
  `FD868D48331BC0729CD5141FFF4765EF71DCE412FA90A779C12146376F5CA72A`.
- ADB foreground check confirmed
  `com.rydersblockstudio.rydersblock/com.unity3d.player.UnityPlayerActivity`
  as the focused activity.
- Targeted logcat slice showed no `AndroidRuntime`, fatal exception,
  `NullReferenceException`, crash, or ANR lines for the focused launch.
- Device window state reported landscape `2340 x 1080`, fullscreen task,
  hidden status/navigation bars, and `layoutInDisplayCutoutMode=always`.
- ADB screenshot confirmed the module scene renders in landscape fullscreen
  with first-person hands visible and no Android system bars visible.
- Screenshot evidence:
  `C:\Users\lin4s\Documents\Riders Block\Logs\device-launch.png`.
- ADB tap on the pause control opened the in-run pause menu; the screenshot
  still showed hidden Android system bars.
- Home/app relaunch returned to the focused Unity activity with the pause menu
  still visible, landscape unchanged, and status/navigation bars hidden.
- Pause/resume screenshot evidence:
  `C:\Users\lin4s\Documents\Riders Block\Logs\device-pause.png`,
  `C:\Users\lin4s\Documents\Riders Block\Logs\device-resume.png`.
- No manual route-completion, sustained thermal/performance, or subjective
  touch-feel approval was recorded in this automated smoke pass.

## S23 Player Feedback and Spiral Retune - 2026-08-16

- User physical-device feedback after the current development APK: the game
  works now with no jitters, and movement feels a little better.
- User physical-device feedback also reported that `THE SPIRAL` is impossible
  to beat and that the overall level/design identity still looks like the old
  prototype.
- Retuned `THE SPIRAL` as an Easy/Bronze phone route without changing movement
  physics, camera rules, Restore rules, ranking rules, or stable module IDs.
- Former hard-gate route pieces were widened and changed to normal route
  materials where appropriate.
- Moving platforms were widened/slowed and endpoint pauses increased.
- Water force/max added velocity were reduced for calmer standard traversal.
- Boost horizontal/vertical strengths were reduced slightly and landing/climb
  platforms widened.
- Final crumbling blocks were widened and given longer fall delays.
- Source validation after Spiral retune: passed.
- EditMode tests after Spiral retune: 131/131 passed.
- PlayMode tests after Spiral retune: 4/4 passed.
- Android development build after Spiral retune: succeeded with 0 warnings and
  0 errors.
- Retuned APK installed directly on the connected S23 with `adb install -r -d`:
  succeeded.
- Retuned APK size: `216281725` bytes.
- Retuned APK SHA256:
  `8E20409CC3F556602A1AB6932281CBE9169646A1903A8406A7A9503A701F95F7`.
- ADB launch after install confirmed focused Unity activity, hidden
  status/navigation bars, and no targeted fatal/crash/ANR/`NullReferenceException`
  lines.
- ADB `PLAY` tap opened the module selector with `THE SPIRAL` visible as a
  development-selectable module.
- ADB tap on `THE SPIRAL` loaded the module into gameplay on the connected S23
  with no targeted fatal/crash/ANR/`NullReferenceException` lines.
- Screenshot evidence:
  `C:\Users\lin4s\Documents\Riders Block\Logs\device-spiral-retune-loaded.png`,
  `C:\Users\lin4s\Documents\Riders Block\Logs\device-spiral-retune-after-play.png`,
  `C:\Users\lin4s\Documents\Riders Block\Logs\device-spiral-retune-gameplay.png`.
- Manual route-completion approval, sustained thermal/performance, and
  subjective touch-feel approval for the retuned route are still pending.

## Visual Readability Pass - 2026-08-16

- Started addressing user feedback that the overall design still feels like the
  old prototype.
- Refreshed `ModuleVisualProfile` and `ModuleEnvironmentProfile` defaults:
  warmer horizon, deeper Null Space, stronger stone side/underside contrast,
  less blue-normal route tops, and stronger warm/cyan accents.
- Added visual-only warm/cyan landing accents to normal route blocks.
- Added visual-only Patch beacon and halo pieces to improve destination
  readability without changing Patch completion rules.
- Added visual-only route flow chevrons to normal, precision, and crumbling
  route blocks; moving, boost, water, and surf blocks keep mechanic-specific
  direction cues.
- Source validation after the visual/readability pass: passed.
- EditMode tests after the visual/readability pass: 132/132 passed.
- PlayMode tests after the visual/readability pass: 4/4 passed.
- Android development build after the visual/readability pass: succeeded with 0 warnings
  and 0 errors.
- Installed the visual-pass APK directly on the connected S23 with
  `adb install -r -d`: succeeded.
- ADB launch/title/module-selector/`THE SPIRAL` gameplay smoke showed no
  targeted fatal/crash/ANR/`NullReferenceException` lines.
- Screenshot evidence:
  `C:\Users\lin4s\Documents\Riders Block\Logs\device-visual-pass-title-loaded.png`,
  `C:\Users\lin4s\Documents\Riders Block\Logs\device-visual-pass-spiral.png`.
- This is not final art approval; manual route readability, sustained
  performance, and visual taste feedback are still required.

## Progression Save Foundation - 2026-08-16

- Added save schema V4 for empty economy balances, loot package count,
  inventory item records, unlock records, and equipped cosmetic slots.
- Added runtime helpers for stable-ID currency add/spend, stackable inventory
  grants, unlock records, and cosmetic equips gated by unlock state.
- Added one-time Bronze completion rewards to the four current modules:
  Patch Shards, one loot package item, and one skin unlock record per module.
- Added compact earned-reward text to the module results panel for newly
  granted rewards.
- V3-to-V4 migration preserves settings, module progression, exceptional
  unlocks, attempts, splits, ranks, and timestamps while adding empty economy,
  inventory, and claimed reward containers.
- Source validation after the V4 save pass: passed.
- EditMode tests after the V4 reward pass: 139/139 passed.
- PlayMode tests after the V4 reward display pass: 4/4 passed from
  `Logs/playmode-results-reward-ui.xml`.
- Android development build after the V4 save pass: succeeded with 0 warnings
  and 0 errors.
- V4 APK installed directly on the connected Samsung Galaxy S23 with
  `adb install -r -d`: succeeded.
- V4 reward display APK size: `216344972` bytes.
- V4 APK SHA256:
  `6886D5AF5154CECE4FB1701A00609322134A646D9AEE74C36D4CD949385A6C04`.
- First launch after install was still extracting IL2CPP resources when paused,
  so the on-device save remained V3; the second launch was allowed to finish,
  then Home/pause wrote schema V4.
- On-device `save.json` under
  `/sdcard/Android/data/com.rydersblockstudio.rydersblock/files/Saves`
  confirmed `schemaVersion: 4`, preserved the existing `module.004.the-spiral`
  attempt record, and wrote empty `economy` and `inventory` arrays.
- Reward APK relaunch after install kept the on-device save at `schemaVersion:
  4` and normalized empty `claimedRewardIds` alongside the empty economy and
  inventory containers.
- Launch/pause logcat showed no targeted fatal/crash/ANR/`NullReferenceException`
  lines. Development profiler buffering warnings remain expected for direct
  development APK sessions.
- Screenshot evidence:
  `C:\Users\lin4s\Documents\Riders Block\Logs\device-v4-save-launch.png`,
  `C:\Users\lin4s\Documents\Riders Block\Logs\device-v4-helpers-launch.png`,
  `C:\Users\lin4s\Documents\Riders Block\Logs\device-rewards-launch.png`,
  `C:\Users\lin4s\Documents\Riders Block\Logs\device-reward-ui-launch.png`.
- No loot package opening UI, shop, skin-equipment UI, or
  cosmetic presentation changes were added in this foundation pass.

## Spiral Bronze Spine Reinforcement - 2026-08-16

- User follow-up physical-device feedback: the app now works without jitter and
  movement feels a little better, but `THE SPIRAL` still felt impossible.
- Added 30 broad normal-route Bronze connector terraces across Spiral's base,
  moving, water, boost, surf, and final sections so standard completion no
  longer depends on long diagonal jumps, boost overshoot, surf, shortcuts, or
  moving-platform timing mastery.
- Preserved movement physics, touch controls, camera rules, Restore rules,
  ranking rules, stable module IDs, and Patch Block completion.
- Added `Phase049SpiralShowcase_BronzeSpineCapsPhoneHostileGaps`, which caps
  the standard adjacent block spine at `<= 3.5m` edge gaps and requires
  high-rise joins to be heavily overlapped.
- Source validation after the Bronze spine reinforcement: passed.
- EditMode tests after the Bronze spine reinforcement: 141/141 passed from
  `Logs/editmode-results-spiral-bronze.xml`.
- PlayMode tests after the Bronze spine reinforcement: 4/4 passed from
  `Logs/playmode-results-spiral-bronze.xml`.
- Android development build after the Bronze spine reinforcement: succeeded
  with 0 warnings and 0 errors.
- Reinforced APK installed directly on the connected Samsung Galaxy S23 with
  `adb install -r -d`: succeeded.
- Reinforced APK size: `216358751` bytes.
- Reinforced APK SHA256:
  `4A2ACCE3C833DA047B74658AF149C087E27049155E8CDDCF2877836DA75E3EE4`.
- ADB launch confirmed the focused Unity activity in landscape fullscreen.
- ADB `PLAY` tap opened the module selector with `THE SPIRAL` visible as a
  development-selectable module.
- ADB tap on `THE SPIRAL` loaded the module into gameplay on the connected
  S23 with no targeted fatal/crash/ANR/`NullReferenceException` lines.
- Screenshot evidence:
  `C:\Users\lin4s\Documents\Riders Block\Logs\device-spiral-bronze-title.png`,
  `C:\Users\lin4s\Documents\Riders Block\Logs\device-spiral-bronze-selector.png`,
  `C:\Users\lin4s\Documents\Riders Block\Logs\device-spiral-bronze-gameplay.png`.
- Manual route-completion approval, sustained thermal/performance, and
  subjective readability approval are still pending.

## Spiral Start Composition Follow-Up - 2026-08-16

- ADB gameplay screenshot showed the Spiral start view was still crowded by
  overhead slabs and nearby decoration.
- Moved Spiral's player start onto a longer open runway before the tower climb
  and aimed `Benchmark_Spiral_Start` down that first route.
- Moved the decorative tower landmark out of the center opening read, softened
  underside/side contrast, and replaced rectangular gameplay sky bands with a
  procedural blue/violet skybox.
- Split the title front door into `CAMPAIGN` and direct `SPIRAL` entry, added
  a generated title panorama, and wired generated route-stone texture material
  into module route surfaces.
- Follow-up Campaign structure pass made modules 001-003 the explicit normal
  Campaign order and kept `THE SPIRAL` as a direct separate mode.
- Source validation after the composition follow-up: passed.
- EditMode tests after the final sky/runway follow-up: 142/142 passed from
  `Logs/editmode-results-artmode-skyblue.xml`.
- PlayMode tests after the final sky/runway follow-up: 4/4 passed from
  `Logs/playmode-results-artmode-skyblue.xml`.
- Android development build after the final sky/runway follow-up: succeeded
  with 0 warnings and 0 errors.
- Final sky/runway APK installed directly on the connected Samsung Galaxy S23
  with `adb install -r -d`: succeeded.
- Final sky/runway APK size: `216374144` bytes.
- Final sky/runway APK SHA256:
  `094D2FB23325EBF7EC12DEF415F0EBBF8EC43E90F39A7`.
- ADB title/Spiral smoke loaded gameplay on the connected S23 with no targeted
  fatal/crash/ANR/`NullReferenceException` lines.
- Screenshot evidence:
  `C:\Users\lin4s\Documents\Riders Block\Logs\device-artmode-title-delayed.png`,
  `C:\Users\lin4s\Documents\Riders Block\Logs\device-artmode-skyblue-spiral.png`.
- This remains a production-direction pass, not final art approval. Spiral
  still needs a deeper layout/art rebuild to reduce stacked-route clutter and
  establish a memorable final environment.

## Campaign Mode Structure Follow-Up - 2026-08-16

- Made modules 001-003 the explicit normal Campaign order:
  `module.001.first-steps`, `module.002.moving-parts`,
  `module.003.flow-error`.
- Kept `module.004.the-spiral` as a separate direct Spiral mode instead of a
  fourth Campaign unlock.
- Updated the Campaign selector copy and module display names to match the
  player-facing mode split.
- Source validation after the Campaign structure follow-up: passed.
- EditMode tests after the Campaign structure follow-up: 142/142 passed from
  `Logs/editmode-results-campaign-split.xml`.
- PlayMode tests after the Campaign structure follow-up: 4/4 passed from
  `Logs/playmode-results-campaign-split.xml`.
- Android development build after the Campaign structure follow-up: succeeded
  with 0 warnings and 0 errors.
- Campaign structure APK installed directly on the connected Samsung Galaxy
  S23 with `adb install -r -d`: succeeded.
- Campaign structure APK size: `216372011` bytes.
- Campaign structure APK SHA256:
  `B850C7D16C595EDBE5E1B0F55B2C16367C8051D4C0826373CA84C61A2E1200C9`.
- ADB title/Campaign selector smoke showed the Campaign panel lists only the
  three normal Campaign levels, with no targeted fatal/crash/ANR/
  `NullReferenceException` lines.
- Screenshot evidence:
  `C:\Users\lin4s\Documents\Riders Block\Logs\device-campaign-split-title.png`,
  `C:\Users\lin4s\Documents\Riders Block\Logs\device-campaign-split-selector.png`.

## Meshy Art Kit Integration - 2026-08-18

- Copied Meshy source into `Assets/_Game/Art/MeshySource` and generated
  production environment prefabs/materials/textures under `Assets/_Game/Art`.
- Added `ART_ASSET_MANIFEST.md`; it marks the 1.98M-triangle
  `RR_FP_Arm_Right` as `WAITING FOR OPTIMIZED REMESH`.
- No runtime arm prefab, mirrored left-arm mesh, or active
  `FirstPersonArmProfile.asset` is generated from the high-poly source.
- Fixed a device-visible double-scale bug where gameplay block prefab visuals
  were inheriting authored block scale twice and overlapping the Spiral view.
- Source validation after the Meshy pass: passed.
- Unity project validation after the Meshy pass: passed.
- EditMode tests after the Meshy pass: 149/149 passed from
  `Logs/editmode-meshy-art-kit-final-results.xml`.
- PlayMode tests after the Meshy pass: 4/4 passed from
  `Logs/playmode-meshy-art-kit-final-results.xml`.
- Android development build after the Meshy pass: succeeded with 0 errors.
- Meshy APK installed directly on connected Samsung Galaxy S23 with
  `adb install -r`: succeeded.
- Meshy APK size: `251480411` bytes.
- Meshy APK SHA256:
  `22A8E9659B4AD3759AD4D2A6DAFA2751CF82DBDE9A3BC2932E1CB05F5C0F8FC3`.
- ADB title/Spiral smoke loaded gameplay on the connected S23 with no
  targeted app fatal/crash/ANR/`NullReferenceException` lines.
- Screenshot evidence:
  `C:\Users\lin4s\Documents\Riders Block\Logs\rr_050_meshy_title_final.png`,
  `C:\Users\lin4s\Documents\Riders Block\Logs\rr_050_meshy_after_tap.png`.
- This is integration validation, not final art approval. Spiral layout,
  material taste, sustained performance, and the optimized first-person arms
  still need follow-up passes.

## Physical Device Checks Required

- Continue testing both landscape-left and landscape-right safe areas.
- Confirm right drag camera and right Tap Jump can be used simultaneously
  without accidental jumps across a longer session.
- Confirm The Spiral standard route can be completed without bhop, surf,
  shortcuts, or boost overshoot.
- Confirm fallback after falls and Retry feel quick and reliable.
- Confirm sustained performance, thermals, readability, and audio comfort.

Only a physical-device smoke test has been recorded for this pass. Full route,
performance, thermal, and comfort approval is still pending.
