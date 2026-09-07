# Phase 0.4.6 - Alpha Vertical Slice Lock

Version: `0.4.6-alpha-vertical-slice`
Date: 2026-08-12

## Intent

Phase 0.4.6 locks the current playable alpha around THE SPIRAL. It is a polish
and verification pass, not a new feature pass.

## Implemented

- Bumped project and validator versioning to `0.4.6-alpha-vertical-slice`.
- Added a compact run menu for Resume, Retry, Module Select, Look sensitivity,
  and touch-zone visibility.
- Hid development controls behind an explicit `DEV` drawer in Editor and
  development builds.
- Wired touch look-sensitivity presets to `ISettingsService` persistence.
- Reset touch state when the app is paused or loses focus.
- Added THE SPIRAL Water and Boost benchmark anchors so development teleports
  cover START, LOW, MID, WATER, BOOST, SURF, HIGH, and SUMMIT.
- Added regression coverage for persisted touch sensitivity and benchmark
  naming.
- Added `ALPHA_QA.md` for local/physical review tracking.

## Preserved

- Movement V1 physics, default left movement/right camera/right Tap Jump,
  Editor controls, Restore, Patch completion, ranks, scoring, progression,
  saves, and personal-best invalidation for development teleports remain
  intact.
- Debug Trace, Crash Wave, ghosts, enemies, multiplayer, economy, cosmetics,
  procedural generation, new movement mechanics, campaign hub, and Phase 0.5.0
  remain unstarted.

## Requires Phone Review

- Samsung Galaxy S23 fullscreen behavior, especially persistent top status bar
  and navigation UI.
- Landscape-left and landscape-right safe areas.
- Multitouch comfort and accidental Tap Jump rejection.
- The Spiral standard-route completion without advanced movement.
- Runtime performance, thermals, readability, and audio comfort.

No physical-device validation was performed during this pass.
