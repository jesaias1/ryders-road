# Phase 0.7.5 Module 003 Gold-Standard Vertical Slice

## Strategy & Scope

- **SYSTEMS ARE GLOBAL**: Depth bands, atmospheric fade, subtle mist motion,
  hidden telemetry, and arm visibility gating are reusable presentation
  systems.
- **CONTENT POLISH IS ONE LEVEL AT A TIME**: Campaign Module 003 (Flow Error /
  Ancient Abyss) is the sole level receiving gold-slice composition and
  placement. Modules 001, 002, and The Spiral are preserved.
- **Gameplay Contracts Locked**: GoldSrc-style movement, 94 FOV world camera
  authority, input, player colliders, Restore/Patch, ranks, progression, saves,
  and stable IDs remain unchanged.

## Root Causes Diagnosed and Resolved

1. **Large Structures Beside The Player**: Module 003's generated biome
   landmarks and authored decoration prefabs could place huge visual forms
   close to, or aligned with, the playable route. Without explicit depth-band
   metadata, the renderer treated everything as generic background dressing.
   - Fix: `BiomeDepthBand` and per-object depth fade now separate gameplay,
     near environment, mid world, far world, and lower abyss presentation.
     Large landmarks are authored far/down/off-route; near decorations are
     modest below-platform supports.
2. **Flat Abyss Read**: Earlier mist layers gave bottom coverage but not a
   continuous deep world.
   - Fix: Ancient Abyss now uses six mist-ocean layers with a deep deck,
     horizon veil, summit depth, sparse side wisps, and subtle `RuntimeWorldDrift`
     motion.
3. **Presentation Noise**: First-person arms and default debug telemetry could
   interfere with judging the vertical slice.
   - Fix: `FirstPersonArmProfile.ShowFirstPersonArms` is false for this phase,
     which hides arm rendering while preserving Ryder Arm V2 assets, rig data,
     fallback references, and pose values. `GameConfiguration.DeveloperTelemetryEnabled`
     is false so diagnostics are hidden by default but still available through
     DIAG cycling.

## Module 003 Gold Slice Content

- Hero temple `biome.ancient-abyss.hero.temple-complex` is far/down across the
  abyss at `(68, -42, 96)` with `FarWorld` haze treatment.
- Abyss tower and distant monoliths sit in the `LowerAbyss` band, partially
  swallowed by the mist ocean.
- Celestial arch is moved off-route/far; mid-world island, broken aqueduct, and
  sunken spire create secondary readable compositions.
- Water buttress, boost flank, spire bases, tree island, and summit supports
  are smaller and lower so the route stays visually open.
- Module 003 gets warm sun, cool ambient fill, subtle URP post processing,
  atmospheric dust, landing bursts, water-entry foam bursts, and the existing
  Restore/Patch/boost/crumble feedback.

## Verification & Status

- Source validator: passed (`Logs/phase075-gold-source-validator-final.log`).
- Foundation Project Validator: passed
  (`Logs/phase075-gold-foundation-validator.log`).
- EditMode Tests: 188/188 passed
  (`Logs/phase075-gold-editmode-results.xml`).
- PlayMode Tests: 6/6 passed
  (`Logs/phase075-gold-playmode-results.xml`).
- Android Build: succeeded with one warning and zero errors
  (`Logs/phase075-gold-android-build.log`).
- APK:
  `Builds/Android/RYDERS-ROAD-0.7.5-module003-gold-standard-vertical-slice-dev.apk`,
  172,055,830 bytes, SHA-256
  `0B95C62BAD98183D5E8AC20019466142189F57C724FE6B6FAEE2EAF05F8D4375`.
- Phone Status: Samsung S23 not connected during this pass. Human
  physical-device acceptance remains pending.
