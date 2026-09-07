# RYDER'S ROAD Physical-Device Feedback

Status: **Brand Alpha Presentation development build ready for phone testing; approval pending**.
This file is for observations from a real Android phone. Do not enter Editor,
Game view, simulated input, or remote-streaming results as physical-device
approval evidence.

## Phase 1B Rescue Feedback Recorded - 2026-08-11

Known physical-device information:

- Platform: Android physical device.
- Native resolution observed: approximately `2340 x 990` in landscape.
- Performance observed: approximately 60 FPS.
- Device manufacturer/model: not recorded.
- Android version: not recorded.
- Build tested: Phase 1 Android development build, exact filename not recorded.

Reported issues:

- Holding the left joystick straight forward caused rightward drift.
- Movement felt janky and not yet precise enough for first-person block
  parkour.
- Large gameplay surfaces rendered bright magenta/pink.
- Platforms and void were not visually distinct enough.
- Diagnostics dominated the phone screen.
- Joystick and jump controls looked and felt like crude developer placeholders.
- Some world-space labels appeared mirrored/reversed or incorrectly oriented.
- Overall presentation was still far from
  `Docs/VisualReferences/target_gameplay.png`.

## Alpha Vertical Slice Test Build

Use build `0.4.7-brand-alpha-presentation`.

APK filename:
`RYDERS-ROAD-0.4.7-brand-alpha-dev.apk`

The default control scheme is provisional but should not be broadly redesigned
during this test. The 0.4.7 feel sweep launches in Classic Mode: left-thumb
movement, right-thumb camera drag, and right-side Tap Jump. Easy Mode remains
available from the run menu for left-thumb movement/steering, Smart Parkour
Camera, and a broad right-side Jump Zone. Approval requires real phone
feedback.

Samsung Galaxy S23 feedback on 2026-08-14 reported that movement felt janky
and the first-person hands/graphics blinked or jittered. The next device pass
should specifically compare the responsive Classic default against the previous
Easy default and check whether the smoothed hands still shimmer.

Samsung Galaxy S23 ADB smoke on 2026-08-16 installed and launched the current
development APK successfully. The focused activity was fullscreen landscape,
status/navigation bars were hidden, and a targeted logcat slice showed no
crash, ANR, or `NullReferenceException` lines. ADB tapping the pause control
opened the in-run pause menu, and app resume from home returned to the focused
Unity activity with bars still hidden. This pass did not record manual route
completion, sustained thermals, or subjective touch-feel approval.

Samsung Galaxy S23 user feedback later on 2026-08-16 confirmed the current APK
works with no jitters and slightly better movement. The same feedback said
`THE SPIRAL` is impossible to beat and the level/design identity still feels
like the old prototype. `THE SPIRAL` was retuned afterward into a broader,
calmer Easy/Bronze route while preserving advanced optional lines and movement
physics. A fresh phone approval pass is still required for the retuned APK.

The Phase 3 rank thresholds are also provisional and uncalibrated.

The Phase 0.4.5 Spiral Showcase build adds `THE SPIRAL`, stable ID
`module.004.the-spiral`, as the first large map. Physical route, pacing,
fullscreen, performance, and visual approval are still pending.

Phase 0.4.6 keeps `THE SPIRAL` as the alpha vertical slice target and cleans up
the normal play surface with a run menu plus hidden development controls.
Phase 0.4.7 changes the player-facing title to `RYDER'S ROAD`, adds supplied
branding assets, and strengthens Android fullscreen handling. This still needs
physical Android approval.

## Samsung Galaxy S23 Brand/Fullscreen Checklist

Rate each item from 1 (poor) to 10 (excellent), then add notes if anything is
wrong:

- App installs over the previous alpha:
- Launcher label says `Ryder's Road`:
- Launcher icon is recognizable:
- Launcher icon is not badly cropped in the Samsung launcher:
- Splash/loading into the game has no old public title:
- Title screen logo is sharp:
- Title screen fits landscape-left:
- Title screen fits landscape-right:
- `PLAY` opens the module selector:
- `SETTINGS` opens and all labels fit:
- Easy/Classic setting affects the next run:
- FOV setting affects the next run:
- Audio setting affects volume:
- Top status bar is hidden immediately after launch:
- Top status bar remains hidden after tapping pause/run menu:
- Top status bar remains hidden after app switch/resume:
- Navigation bar stays transient/hidden during gameplay:
- Safe areas keep HUD and controls clear of cutouts:
- THE SPIRAL route remains readable with the new HUD/menu:
- Overall alpha presentation feels ready to show a tester:

## Phase 2 Session Record

- Date and local time:
- Tester:
- Device manufacturer/model:
- Android version:
- Chipset/RAM (if known):
- Native resolution and aspect ratio:
- Refresh rate:
- Notch, hole-punch, rounded corners, or cutout:
- Build filename:
- Build version shown in diagnostics:
- Install source/method:
- Fresh install or update:
- Session duration:

## Install, Orientation, and UI

- APK installs without warning or error:
- App launches into landscape:
- Landscape-left result:
- Landscape-right result:
- Portrait remains blocked:
- Safe areas keep joystick, jump, HUD, and diagnostics unobstructed:
- Text is readable at normal holding distance:
- `ModuleSelector` fits on the screen:
- Development buttons are usable but not distracting:
- No magenta gameplay materials:
- Platforms are visually distinct from the void:
- Problems and reproduction steps:

## Global Controls

Rate each item from 1 (poor) to 10 (excellent):

- Thumb comfort:
- Left-thumb movement comfort:
- Right-thumb camera comfort:
- Camera precision:
- Camera smoothness:
- Camera speed:
- Movement precision:
- Normal Jump Distance:
- Jump Height:
- Jump Snappiness:
- Landing Precision:
- Right-side Tap Jump:
- Accidental jump avoidance:
- Parkour flow:
- Jump responsiveness:
- Air control:
- Momentum:
- Bhop:
- Surf:
- Fullscreen presentation:
- Restore experience:
- Visual readability:
- Overall satisfaction:

Answer with yes, no, or notes:

- Are the controls close enough to screen center?
- Should they move even farther inward?
- Do you ever run out of thumb travel or hit the screen edge?
- Does moving straight stay straight?
- Does normal forward movement accidentally trigger Jump?
- Does a right-side quick tap trigger Jump reliably while movement is held?
- Does right-thumb drag look/camera start promptly?
- Does right-thumb drag ever jump on release?
- Do top-right UI/development buttons avoid jump?
- Are right-side Tap Jumps sometimes missed?
- Is right-side Tap Jump too sensitive?
- Is right-side Tap Jump not sensitive enough?
- Does a 90 degree turn require a reasonable single swipe?
- Does camera feel jittery, laggy, floaty, or sticky? Answer each separately.
- Can you chain jumps naturally?
- Does movement continue naturally during right-tap Jump?
- Which movement profile feels best: Default, Forgiving, Precise, or Momentum?
- Can you play without looking at the controls?
- Does this feel close to the simplicity and precision needed for
  Minecraft-style parkour?
- Do you trust where the character will land?
- Does a missed jump feel like your mistake rather than the controls?
- Are any controls blocked by phone edges, notches, or navigation gestures?
- Is the Android top status bar hidden during gameplay?
- Does the navigation bar stay hidden after tapping, pausing, or resuming?

## THE SPIRAL Physical Tests

Test the standard route first. Do not use `DEVTP` or `F6` when recording a
personal-best attempt.

- Can an ordinary player understand where to go from the start?
- Does the tower/summit stay visible enough to motivate the climb?
- Are the five Restore Points spaced fairly?
- Does the standard route feel completable without bhop, air-strafe, surf,
  shortcut routing, or boost overshoot?
- Which section feels too easy, too hard, too long, or unclear?
- Are water flow directions readable on phone?
- Are boost landing destinations visible before launch?
- Is the optional surf line visually distinct without looking mandatory?
- Does the final corruption/ascent section feel exciting without becoming
  unfair?
- Does the map sustain acceptable FPS and thermals during a full 4-8 minute
  run?
- Are lower/future route views readable, or do decorations create confusion?

## Parkour Movement Physical Tests

Test A - Thumb resting position:

- Hold the phone naturally.
- Place left/right thumbs where they naturally rest.
- Controls activate without stretching toward screen edges:

Test B - Full control range:

- Use movement and camera for two minutes.
- Either thumb repeatedly reaches the physical edge of the phone:

Test C - Forward run:

- Push left movement upward normally.
- Expected: run forward, no Jump.
- Result:

Test D - Hold forward:

- Hold forward for 10 seconds.
- Expected: continuous run, zero automatic repeated jumps.
- Result:

Test E - Right Jump Zone:

- While running forward, touch the right gameplay region.
- Expected: one clean Jump.
- Result:

Test F - Movement continuity:

- After right-side Jump, expected left movement remains active with no reacquire.
- Result:

Test G - Rapid parkour:

- Hold left movement forward and perform 10+ consecutive right-side jumps.
- Successful intended jumps:
- Missed taps:
- Accidental jumps:

Test H - Manual right-thumb camera:

- Complete a straight lane, a 45 degree turn, a 90 degree turn, a jump gap, and
  the manual-camera spiral.
- Camera keeps route visible:
- Landing visibility:
- Motion sickness or discomfort:
- Screen-edge hits:

Test I - Jump calibration:

- Use the Jump Calibration Lane.
- Run at normal speed and jump onto ordinary platforms.
- Expected: good landings dominate and frequent overshoot is gone.
- 20 normal jumps:
- Undershoots:
- Good landings:
- Overshoots:
- Advanced momentum jumps still travel farther when earned:
- Bhop chain still builds/preserves speed intentionally:
- Air-strafe still changes trajectory without feeling like flying:
- Surf entry/exit still works:

Test J - Camera alternating:

- Alternate Tap Jump, camera correction, Tap Jump, camera correction for at
  least 20 attempts.
- Tap accidentally rotates camera:
- Drag accidentally jumps:
- Tiny corrections feel precise:
- Large intentional swipe can turn around quickly:

Test K - Compare optional development profiles:

| Profile | Rating 1-10 | Notes |
| --- | --- | --- |
| Default | | |
| Forgiving | | |
| Precise | | |
| Momentum | | |

Test L - Fullscreen/system bars:

- Launch, rotate between landscape orientations, background/resume, and tap
  near the top edge.
- Android status bar remains hidden:
- Android time/status icons are not persistently visible:
- Android navigation bar stays hidden after resume:
- Full Diagnostics display reasonable safe-area values:

## Module 01 - First Steps

- Can you finish the module?
- Time to finish:
- Number of falls:
- Number of restores:
- Is the first route readable?
- Are standard blocks and small gaps fair?
- Is the boost block understandable?
- Is the Restore Point obvious?
- Is the Patch Block obvious?
- Any section too easy, too hard, or confusing:

## Module 02 - Moving Parts

- Can you finish the module?
- Time to finish:
- Number of falls:
- Number of restores:
- Do moving blocks feel stable?
- Does the player stay carried by moving blocks?
- Are crumbling blocks readable before they disappear?
- Are shortcuts tempting without feeling required?
- Any section too easy, too hard, or confusing:

## Module 03 - Flow Error

- Can you finish the module?
- Time to finish:
- Number of falls:
- Number of restores:
- Does it feel like a small vertical climb around a structure?
- Can you understand where the route goes next?
- Does water flow feel useful or annoying?
- Do boosts support the climb?
- Do Restore Points preserve progress fairly?
- Does this direction make you want larger spiral/tower maps?
- After Module 03, would you voluntarily play Module 04 immediately?
- Any section too easy, too hard, or confusing:

## Performance and Device Behavior

Read FPS/minimum FPS from diagnostics after at least ten continuous minutes.

- Average FPS:
- Lowest observed FPS:
- Noticeable stutter locations:
- Touch latency:
- Thermal state after 5 minutes:
- Thermal state after 10 minutes:
- Battery use:
- Audio behavior:
- Background/resume behavior:
- Call/notification interruption behavior:
- Back gesture/navigation-bar interference:
- Problems and reproduction steps:

## Phase 3 Timer, Ranks, And Progression

For each module, record:

| Module | First completion | Best after 3 attempts | Best after 5 attempts | Falls | Rank | Voluntary retry? |
| --- | --- | --- | --- | --- | --- | --- |
| Module 01 | | | | | | |
| Module 02 | | | | | | |
| Module 03 | | | | | | |

Answer with yes, no, or notes:

- Did Bronze feel like legitimate completion?
- Did you understand Silver, Gold, and Diamond immediately?
- Did seeing `0.X seconds to next rank` make you want another attempt?
- Was Retry fast enough?
- Did you ever feel forced to improve rank to continue?
- Was Next Module always clearly available?
- Did checkpoint split feedback help?
- Did the timer create excitement or unwanted stress?
- Was the HUD readable without blocking jumps?
- Did Diamond feel desirable?
- Did the selector lock/unlock behavior make sense?
- Did any invalid/development flow appear in normal play?

## Phase 3B Visual Direction

Rate each item from 1 (poor) to 10 (excellent):

- Sky:
- Atmosphere:
- Depth:
- Blocks:
- Floating islands:
- Water:
- Mechanic readability:
- Gloves:
- HUD:
- Controls appearance:
- Overall art direction:

Answer with yes, no, or notes:

- Does this now look like a real game rather than a Unity prototype?
- Is it clearly moving toward `Docs/VisualReferences/target_gameplay.png`?
- Can you always identify the next platform?
- Does Null Space feel deep?
- Are the colors attractive without being overwhelming?
- Is water clear and attractive?
- Can you identify Restore, Patch, Boost, Moving, and Crumbling without text?
- Do gloves stay out of the way?
- Is the HUD unobtrusive?
- Is Module 03 clearly the strongest visual showcase?
- Does the vertical/ascent structure feel exciting?
- Does the game still feel smooth?

## Phase 0.4.0 Visual Identity / Hero Map Pass

Rate each item from 1 (poor) to 10 (excellent):

- Module 03 hero view:
- Sky color/depth:
- Warm horizon:
- Route block surface detail:
- Water shimmer/readability:
- Boost/patch glow:
- First-person gloves:
- Rank strip/HUD:
- Controls unobtrusiveness:
- Overall move toward the target image:

Answer with yes, no, or notes:

- Does Module 03 now feel like the visual showcase?
- Can you see where the route goes while noticing the surrounding islands?
- Do decorative islands/trees/clouds ever look like playable platforms?
- Do block trims/insets improve landing readability?
- Do water and boost effects help rather than distract?
- Do the gloves feel like part of the game without blocking jumps?
- Does the rank strip stay readable without stealing attention?
- Does the game still hold smooth performance after ten minutes?

## Phase 0.4.6B Smart Parkour Camera

Test Easy Mode first, then switch to Classic from the run menu and retest one
module.

Rate each item from 1 (poor) to 10 (excellent):

- Easy Mode camera comfort:
- Easy Mode route readability:
- Camera turn smoothness:
- Shortcut framing:
- Merge/reacquire behavior:
- Vertical ascent framing:
- Falling/Restore recovery:
- Classic Mode manual camera:
- Fullscreen/no phone top bar:
- Overall phone feel:

Answer with yes, no, or notes:

- Did Easy Mode let you run and jump using only left movement and right Jump?
- Did the camera ever feel like it was steering you instead of framing you?
- Did it ever point at individual platforms too strongly?
- Did it ever hide the next landing?
- Did it recover after stopping, falling, restoring, or turning around?
- Did shortcuts stay optional and skill-based?
- Did Classic Mode still feel unchanged?
- Did bhop, air-strafe, boost, water, and surf still feel under your control?
- Did the phone show any top status bar, clock, icons, or navigation bar?
- Did motion feel comfortable after ten minutes?

## Diagnostics and Reports

- Diagnostics visibility and readability:
- Active touch count appears correct:
- Profile/effect state appears correct:
- Restore/fall counts appear correct:
- FPS appears plausible:
- Module completion reports were produced:
- Device logs attached (path/link):

For each defect, record:

1. Short title:
2. Severity:
3. Exact reproduction steps:
4. Expected result:
5. Actual result:
6. Frequency:
7. Screenshot/video/log:

## Tester Conclusion

- Best module:
- Worst module:
- Most important control change:
- Most important comfort change:
- Most important readability change:
- Most important performance issue:
- Overall 1-10:
- Other notes:

Phase 2 approval: **leave blank pending review**

Phase 3 authorization: **blocked until Phase 2 phone feedback is reviewed**
