# Phase 0.4.7 - Brand Alpha Presentation

Version: `0.4.7-brand-alpha-presentation`

## Scope

This pass changes the player-facing product identity to `RYDER'S ROAD` and
prepares the current alpha build for remote phone installation. It is a brand,
presentation, Android fullscreen reliability, and phone feel stabilization
pass. It does not change module rules, ranks, score, progression, or saves.

## Branding

- Android app label: `Ryder's Road`
- Player-facing title: `RYDER'S ROAD`
- Legacy internal codename: `RYDERS BLOCK`
- Package ID preserved: `com.rydersblockstudio.rydersblock`
- Build output: `Builds/Android/RYDERS-ROAD-0.4.7-brand-alpha-dev.apk`

Approved source assets:

- `Assets/Branding/Source/RydersRoad_Logo_Source.png`
- `Assets/Branding/Source/RydersRoad_AppIcon_Source.png`

Runtime/build derivatives:

- `Assets/Branding/Runtime/RydersRoad_Logo_UI.png`
- `Assets/Branding/Resources/Branding/RydersRoad_Logo_UI.png`
- `Assets/Branding/Android/RydersRoad_Icon_Legacy.png`
- `Assets/Branding/Android/RydersRoad_Icon_AdaptiveForeground.png`
- `Assets/Branding/Android/RydersRoad_Icon_AdaptiveBackground.png`

Icon mask previews:

- `Docs/Branding/RydersRoad_IconMask_circle.png`
- `Docs/Branding/RydersRoad_IconMask_rounded-square.png`
- `Docs/Branding/RydersRoad_IconMask_squircle.png`
- `Docs/Branding/RydersRoad_IconMask_samsung-like.png`

The supplied black-backed logo source is preserved untouched. The runtime UI
logo is an alpha-cleaned derivative. Mask previews are artifact checks only and
do not prove physical launcher quality.

## Runtime Presentation

`ModuleSelector` now opens as a branded front door with the approved logo,
`PLAY`, and `SETTINGS`. `PLAY` opens the existing module selector flow.
`SETTINGS` exposes alpha controls for Easy/Classic, steering speed, look
sensitivity, FOV, graphics quality, haptics, and audio.

The run menu uses the new title while preserving Resume, Retry, Module Select,
Easy/Classic mode, look sensitivity, and touch-zone behavior.

After Samsung Galaxy S23 feedback, the follow-up feel sweep restores Classic
left movement/right manual camera/right Tap Jump as the phone default, lowers
the joystick dead zone, feeds camera drag on the activation frame, tightens the
default movement profile, and smooths/lower-corners the first-person hands.
Easy Smart Parkour Camera remains selectable rather than removed.

## Fullscreen

Android fullscreen is enforced by both project settings and runtime service:

- Start in fullscreen.
- Requested visible insets are set to none.
- Render outside safe area.
- FullScreenWindow.
- Modern Android insets hide status/navigation bars.
- Legacy sticky immersive flags are also applied.
- Status/navigation bar colors are cleared to transparent.
- Immersive mode is reapplied during startup, resume, and focus return.

Samsung Galaxy S23 must still physically verify that the top status bar,
clock, status icons, and navigation UI stay hidden during active gameplay and
after pause/resume.

## Save Compatibility

Save schema remains V3. Android package ID is unchanged, so Android app data is
intended to remain in the same package. For editor/desktop product-name paths,
`SavePathMigrationUtility` copies legacy `RYDERS BLOCK/Saves/save.json` and
`.bak` into the missing current `Ryder's Road/Saves` directory without
overwriting an existing current save.

## Explicitly Not Started

Debug Trace, Crash Wave, ghosts, enemies, multiplayer, economy, cosmetics,
procedural levels, final narrative systems, and Phase 0.5.0 were not started.
