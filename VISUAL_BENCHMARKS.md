# RYDER'S ROAD Visual Benchmarks

Version: `0.4.7-brand-alpha-presentation`

## Purpose

These benchmark views give artists and developers stable comparison points for
the current visual identity pass. They are PC/editor development aids, not
physical Android approval.

## Current Visual Gap Analysis

Before Phase 0.4.0, the game had a readable Phase 3B visual slice but still
looked closer to colored runtime primitives than the intended polished
first-person sky-parkour game in `Docs/VisualReferences/target_gameplay.png`.

- Sky and atmosphere needed more blue/violet depth, warm horizon layering, and
  stronger cloud shelves.
- Blocks had useful role colors but needed a reusable surface kit: light tops,
  insets, trims, dark undersides, and small original tech accents.
- Module 03 read as a compact climb, but not enough like the hero map preview.
- Water, boost, restore, patch, moving, and crumbling surfaces needed stronger
  first-glance identity without labels.
- Floating islands and towers needed more silhouette, grass caps, and warm/cyan
  accents while staying non-playable decoration.
- First-person gloves were present but still too placeholder-like.
- HUD/rank presentation needed a compact polished mobile strip without blocking
  the central route.
- VFX used one-off particle objects and needed a small reusable pooled path.
- Mobile graphics still needed an explicit quality target and validation.

## Fixed Views

Press `F6` in `ModuleRunner` during Editor or development builds to cycle
through the active module's benchmark anchors. The scene creates named anchor
objects for these review positions:

| Benchmark | Intent |
| --- | --- |
| `Benchmark_TitleScreen` | Branded title/front-door read with approved logo, sky route, and first-menu composition. |
| `Benchmark_ModuleSelector` | Module selector read for title/settings/menu density and right-panel readability. |
| `Benchmark_Module01_Start` | Calm first-route read with sky, clouds, gloves, HUD, and controls. |
| `Benchmark_Module02_Mid` | Moving-block/shortcut read with suspended landmarks and depth. |
| `Benchmark_Module03_Opening` | Arrival-court read with the Bronze route, broken causeway, reclaimed sanctuary, and first temple reveal. |
| `Benchmark_Module03_Early` | Early-flow read through the procession ruins toward the moving crossing. |
| `Benchmark_Module03_Middle` | Mid-course read across Crumble and Boost beats with the sunken civilization below. |
| `Benchmark_Module03_High` | Exposed mastery ascent with earlier progress below and the temple as orientation anchor. |
| `Benchmark_Module03_Patch` | Final Patch approach framed by the colossal temple and open sky. |
| `Benchmark_Spiral_Start` | First view of THE SPIRAL route, tower, sky depth, HUD, controls, and summit direction. |
| `Benchmark_Spiral_Low` | Lower ascent review after the base intro and first Restore Point. |
| `Benchmark_Spiral_Mid` | Mid-map review around water exit/boost approach and route wrap readability. |
| `Benchmark_Spiral_Water` | Water-section read for flow direction, exit landing, and standard-route comfort. |
| `Benchmark_Spiral_Boost` | Boost approach read for launch direction, overshoot safety, and next-platform visibility. |
| `Benchmark_Spiral_Surf` | Optional surf/momentum wall review and standard outer-route visibility. |
| `Benchmark_Spiral_High` | Final ascent review with lower-route depth and corruption readability. |
| `Benchmark_Spiral_Summit` | Summit Patch Block payoff and top-of-tower framing. |

`Benchmark_Current_Start` is also created for the currently loaded module as a
convenience anchor.

## Screenshot Output

No automated screenshots are used as pass/fail evidence. Use Unity Game view
or an Android screen recording from these anchors for review.

Suggested screenshot paths:

- `Docs/VisualReferences/Benchmarks/phase040-module01-start.png`
- `Docs/VisualReferences/Benchmarks/phase040-module02-mid.png`
- `Docs/VisualReferences/Benchmarks/phase040-module03-hero.png`
- `Docs/VisualReferences/Benchmarks/phase040-module03-look-down.png`
- `Docs/VisualReferences/Benchmarks/phase040-module03-look-up.png`
- `Docs/VisualReferences/Benchmarks/phase045-spiral-start.png`
- `Docs/VisualReferences/Benchmarks/phase046-spiral-water.png`
- `Docs/VisualReferences/Benchmarks/phase046-spiral-boost.png`
- `Docs/VisualReferences/Benchmarks/phase045-spiral-mid.png`
- `Docs/VisualReferences/Benchmarks/phase045-spiral-summit.png`
- `Docs/VisualReferences/Benchmarks/phase047-title-screen.png`
- `Docs/VisualReferences/Benchmarks/phase047-module-selector.png`

Icon mask preview artifacts:

- `Docs/Branding/RydersRoad_IconMask_circle.png`
- `Docs/Branding/RydersRoad_IconMask_rounded-square.png`
- `Docs/Branding/RydersRoad_IconMask_squircle.png`
- `Docs/Branding/RydersRoad_IconMask_samsung-like.png`

These icon previews are not physical launcher approval.

## PC-Only Performance Counters

The diagnostics overlay publishes:

- `Visual objects`
- `Particle emitters`
- `Renderers`
- `Shadow casters`
- `Realtime lights`
- `Colliders`
- `Active rigidbodies`
- `Active particle systems`
- `Boost uses`
- `Water pulses`
- `Surf used`
- `Peak module speed`
- `Visual quality profile`

These are local development counters only. Do not record phone FPS, thermals,
touch comfort, safe-area approval, or visual approval until the APK is tested
on a physical Android device.

## Quality Profiles

Phase 0.4.0 targets `MobileBalanced`.

- `MobileLow`: future reduction target for optional clouds, decoration, and
  VFX if a device misses the 60 FPS budget.
- `MobileBalanced`: current Android development slice and validator target.
- `HeroPreview`: reserved for richer local capture work, not the phone build
  default.

Low quality must never remove essential route readability or mechanic identity.
