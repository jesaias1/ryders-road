# THE SPIRAL Showcase Design

Version: `0.4.5-spiral-showcase`

`THE SPIRAL` is the first large RYDERS BLOCK showcase module.

- Stable ID: `module.004.the-spiral`
- Route style: continuous vertical spiral around a broken floating tower
- Target first completion: 4-8 minutes
- Provisional clean completion: 2.5-4 minutes
- Restore Points: 5 plus start
- Campaign rule: Bronze is any valid completion

## Structure

The map has six major sections:

1. Base intro with wide readable jumps, gentle turns, one longer jump, and a
   clear view of the tower/summit.
2. Moving Parts with side-cycle and lift platforms plus a small optional
   momentum skip.
3. Water Run with cyan flow wrapping the tower, safe entries, correction
   room, and a faster water-speed line.
4. Boost Ascent with orange/gold launch pads, visible destinations, and one
   larger memorable launch.
5. Surf/Momentum Wall with a standard outer route and an optional inner surf
   line.
6. Corruption/Final Ascent with crumbling blocks, moving blocks, larger gaps,
   a final boost, and the summit Patch Block.

## Standard Route

The standard route must remain completable by ordinary campaign players using
run, jump, camera control, Restore Points, and readable platforming. It must
not require bhop, air-strafe, surf, shortcut routing, or boost overshoot
mastery. If a jump feels unreasonable on phone, change the level first.

## Advanced Routes

Advanced movement should create faster lines, not mandatory progression.
Current optional shortcut metadata:

- `shortcut.module-004.inner-gap`
- `shortcut.module-004.water-speed`
- `shortcut.module-004.boost-overshoot`
- `shortcut.module-004.surf-line`
- `shortcut.module-004.high-risk-drop`

These are provisional routing ideas. They need physical Android timing data
before rank thresholds or shortcut time-saves can be treated as calibrated.

## Development Teleports

Editor and development builds can cycle benchmark/dev teleport anchors with
`F6` or the `DEVTP` touch button. Using a development teleport invalidates the
current run for personal-best saving.

The intended anchors are:

- `Benchmark_Spiral_Start`
- `Benchmark_Spiral_Low`
- `Benchmark_Spiral_Mid`
- `Benchmark_Spiral_Surf`
- `Benchmark_Spiral_High`
- `Benchmark_Spiral_Summit`

## Performance Notes

This map is intentionally larger than previous modules, but still uses the
existing runtime primitive kit and `MobileBalanced` visual profile. Diagnostics
report available runtime counts for visual objects, renderers, shadow casters,
realtime lights, colliders, active rigidbodies, particle emitters, active
particle systems, boost uses, water pulses, surf use, and peak module speed.

Draw calls, batches, triangle count, sustained FPS, thermals, and Android
system-bar behavior still require Unity/device tooling or physical phone
testing and must not be fabricated.
