# RYDER'S ROAD Phase 0.6.1 Report

Target: `0.6.1-parkour-feel-and-route-rhythm`

## Delivered

- Retimed standard production Crumble to collapse after about one second.
  Warning stages now occur at one third and two thirds of the collapse delay.
- Preserved Spiral's 19 mandatory crumble supports as short/medium pressure
  chains, with normal/safe breathers and Restore decks between chains.
- Refined first-person hands so palms angle farther inward, the right hand is
  more visible, and run/jump/fall/landing motion reads more clearly. The hands
  remain camera-space presentation only.
- Traced the large title/menu sky rectangle to `Selector Sky Haze`,
  `Selector R Beacon`, and the two high `Selector Hero Pillar` primitives.
  Removed those objects and made the approved title panorama the opaque menu
  backdrop.
- Removed the accidental high Spiral far-spire decoration and added lower-void
  cloud/ruin depth pieces.
- Preserved GoldSrc movement physics, 94 FOV, Classic/Easy camera contracts,
  touch and Editor controls, Restore rules, ranks, saves, progression, stable
  IDs, and Android fixed landscape.

## Verification

- Source validation passed: `Tools/Validate-Foundation.ps1`.
- EditMode `178/178` passed: `Logs/phase061-editmode-results.xml`.
- PlayMode `6/6` passed: `Logs/phase061-playmode-results.xml`, including the
  selector artifact regression.
- Android IL2CPP ARM64 development build succeeded with zero errors and one
  legacy-icon warning: `Logs/phase061-android-build.log`.
- APK:
  `Builds/Android/RYDERS-ROAD-0.6.1-parkour-feel-and-route-rhythm-dev.apk`.
- APK size: 164,474,820 bytes.
- SHA-256:
  `7E2C2B120BEC6117A95E0767C7A9B3F27E9C7ADE9F27E2B38922B7A52CE37617`.
- Samsung Galaxy S23 `SM-S911B`, serial `RFCW100J1HW`: ADB install succeeded,
  launch focused `com.unity3d.player.UnityPlayerActivity`, first frame logged
  at 60.000004 Hz, and no fatal exception appeared in the captured launch log.
- S23 title screenshot `Logs/phase061-s23-title-2.png` shows the title UI over
  the approved panorama with the giant rectangle gone.

## Phone Checklist

1. Does Crumble collapse after roughly one second?
2. Do Crumble sequences force you to keep moving instead of waiting?
3. Are normal/safe blocks spaced well as breathers between crumble chains?
4. Do the palms now angle inward enough?
5. Do both arms appear the same actual length?
6. Is the right hand properly visible?
7. Do the hands visibly feel like running when moving?
8. Does jump/fall/landing hand motion feel natural?
9. Do hands stay out of the important landing view?
10. Is the giant rectangle gone from the title/menu sky?
11. Are other strange world artifacts gone?
12. Does the lower void feel deeper and more dangerous?
13. Does Spiral still feel readable and fair?
14. Does the phone hold fullscreen 60 FPS without obvious heat, bars, or
    pause/resume problems?

## Acceptance Gate

Physical gameplay acceptance was not performed. Human S23 testing is still
required for crumble feel, full Spiral traversal, in-motion hand feel, sustained
performance, thermals, pause/resume, and final visual acceptance.
