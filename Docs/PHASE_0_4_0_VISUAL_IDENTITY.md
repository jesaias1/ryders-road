# Phase 0.4.0 - Visual Identity / Hero Map Pass

Version: `0.4.0-visual-identity`

## Scope

This phase is the first major graphical pass after Movement V1. It moves the
project closer to `Docs/VisualReferences/target_gameplay.png` without changing
the player movement model, checkpoint rules, ranking, progression, save data,
or module completion logic.

## Added

- Expanded `ModuleVisualProfile` with presentation-kit fields for surface
  highlights, insets, circuit lines, warm accents, flower accents, HUD opacity,
  trim scale, and hero accent density.
- Expanded `ModuleEnvironmentProfile` with `MobileLow`, `MobileBalanced`, and
  `HeroPreview` quality tiers plus distant-island and horizon-layer controls.
- Added visual-only block insets, edge trims, dark undersides, side shading,
  tech ticks, and role-specific cues.
- Added richer sky bands, warm horizon glow, high blue depth, layered cloud
  shelves, distant islands, tower accents, island grass caps, and small
  vegetation/flower accents.
- Strengthened Module 03 as the hero visual slice with extra authored islands,
  towers, trees, and cloud frames around the existing compact ascent.
- Added reusable `RuntimeVisualPulse` and pooled module burst emitters for
  lightweight water, boost, patch, restore, and crumble feedback.
- Upgraded first-person gloves with forearm sleeves, rails, thumbs, fingertips,
  and subtle visual-only movement reactions.
- Added a compact rank strip and results accent to the module HUD.
- Added named `VisualBenchmarkAnchor` objects and documented the F6 benchmark
  cycle in `VISUAL_BENCHMARKS.md`.

## Preserved

- GoldSrc/CS 1.6-style movement foundation: velocity-based acceleration,
  friction, air acceleration, air-strafe, bhop, surf, boost and water as
  authored mechanics.
- Default phone controls: left-thumb movement, right-thumb camera, and
  right-side Tap Jump.
- Editor keyboard/mouse controls.
- Module definitions, ranks, score, personal bests, unlocks, saves, Restore
  rules, Patch completion, and telemetry contracts.

## Still Provisional

- No physical Android test was performed for this phase.
- Phone visual readability, safe areas, fullscreen status-bar hiding, sustained
  60 FPS, thermals, touch comfort, and Module 03 hero readability still require
  physical-device review.
- This is not final production art, final UI, final audio, final hub flow, or a
  narrative system.
