# RYDER'S ROAD Phase 0.5.8 Report

Target: `0.5.8-s23-physical-acceptance-and-targeted-world-polish`

## Scope

This update responds only to the latest S23 feedback. It balances the hands,
turns the orange boost into a real gap closer, exposes crumble language earlier,
connects Spiral dressing around a central structure, and removes Android
autorotation. GoldSrc movement and normal-route spacing are unchanged.

## Changes

1. Left arm moved forward/up; right arm moved slightly back/down for balanced visibility.
2. Both palm rolls moved four degrees inward; scale and animation architecture are unchanged.
3. Boost target changed from route 36 at roughly 21m to route 32 at roughly 10m.
4. Boost strengths remain `12.4` vertical and `7.2` horizontal.
5. Four lower crumble blocks now form a visible optional bridge; three upper crumbles remain.
6. Six central core pieces create one broken sky spire from base to summit approach.
7. Lower gateway and detached ruin props received island foundations.
8. All new world pieces remain visual-only and cannot alter gameplay collision.
9. Android now uses fixed `LandscapeLeft`; all autorotation flags are disabled.
10. Build version advanced without save-schema or stable gameplay-ID migration.

## Verification

- Source validator: passed.
- Unity project validator: passed.
- EditMode: `174/174` passed.
- PlayMode: `5/5` passed.
- Android IL2CPP ARM64 clean build: passed, zero errors.
- APK size: `160,687,330` bytes.
- SHA-256: `97B08A6917E9C55D183D4148053AE44A8847F0256C965650B21DD257777E4D0E`.
- Manifest: `screenOrientation=landscape`, non-resizable activity.
- S23 install/launch: passed; title and grounded Spiral start loaded at 60 FPS,
  fullscreen 2340x1080, with no app crash.
- Device evidence: `Logs/device-058-title.png` and
  `Logs/device-058-spiral-start.png`.
- Startup still logs the known nonfatal optional Play Asset Delivery class lookup.

## Human Check

1. Confirm neither arm dominates and both palms feel naturally turned inward.
2. Confirm the first orange boost reliably lands on its visible route target.
3. Confirm the lower orange crumble bridge is visible during the first climb.
4. Confirm the central spire makes Spiral feel like one place rather than loose props.
5. Rotate the phone and confirm the game stays fixed widescreen as requested.

This update is complete and work stops here. Future changes require a new,
single-update prompt.
