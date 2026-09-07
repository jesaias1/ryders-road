# Phase 0.4.5 - Big Spiral Showcase Map

Version: `0.4.5-spiral-showcase`

## Added

- Added `THE SPIRAL`, stable ID `module.004.the-spiral`, as the first large
  showcase parkour module.
- Added a continuous six-section route around a broken floating tower:
  base intro, moving parts, water run, boost ascent, surf/momentum wall, and
  corruption/final ascent.
- Added five Restore Points plus start, a summit Patch Block, five optional
  shortcut metadata entries, and two authored optional surf surfaces.
- Added `ModuleSurfSurfaceDefinition` so module assets can author surf routes
  without changing movement physics.
- Added Spiral benchmark/dev teleport anchors:
  `Benchmark_Spiral_Start`, `Benchmark_Spiral_Low`,
  `Benchmark_Spiral_Mid`, `Benchmark_Spiral_Surf`,
  `Benchmark_Spiral_High`, and `Benchmark_Spiral_Summit`.
- Added a `DEVTP` development touch button and made `F6`/dev teleports
  invalidate the current run for personal-best saving.
- Expanded module telemetry and diagnostics with boost, water, surf, dev
  teleport, peak speed, renderer, shadow-caster, light, collider, rigidbody,
  and particle-system counts where runtime data is available.
- Follow-up S23 feedback showed the route still felt impossible, so the Bronze
  spine now includes broad connector terraces through the base, moving, water,
  boost, surf, and final sections. The standard block path is regression-tested
  to keep phone-hostile gaps from returning.

## Preserved

- No movement profile, touch control, camera control, Restore, rank, save, or
  Patch Block completion rule was redesigned.
- Default mobile play remains left-thumb movement, right-thumb camera drag,
  and right-side Tap Jump.
- Bronze remains any valid completion. The standard Spiral route does not
  require bhop, air-strafe, surf, shortcut routing, or boost overshoot.
- The follow-up Bronze spine pass still does not change movement physics,
  camera rules, Restore rules, rank rules, or Patch Block completion.

## Provisional

- Rank thresholds are uncalibrated: Silver `360s`, Gold `260s`, Diamond
  `210s`.
- Route length, completion time, shortcut value, difficulty, visual density,
  and sustained performance require physical Android testing.
- Samsung Galaxy S23 install/launch/module-load smoke has been performed in
  later 0.4.7 passes, but full manual route completion and sustained
  performance approval are still pending.
- Ghosts, Crash Wave, online leaderboards, economy, cosmetics, final hub, and
  final narrative systems were not started.
