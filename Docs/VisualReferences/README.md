# Visual references

## Current direction: September 9, 2026

The user's visual-direction update supplements the completed 0.12.0 slice;
it does not restart production. These are concept references, not screenshots
of the Unity build. Match files by their visible contents, not attachment order.

| Reference | Purpose |
| --- | --- |
| [Gameplay quality target](2026-09-09_gameplay_quality_target.png) | Primary finished-quality benchmark for composition, lighting, material cohesion, atmospheric depth and route readability. |
| [Loading brand reference](2026-09-09_loading_brand_reference.png) | White Ryder's Road emblem/wordmark on navy, with secondary Jesaias identity. |
| [Main menu concept](2026-09-09_main_menu_concept.png) | Campaign-first hierarchy, separate responsive controls and an expansive world backdrop. |
| [Original gameplay target](target_gameplay.png) | Preserved August 11 reference for parkour readability and sky/void framing; inspect alongside the newer target. |

The September gameplay reference takes precedence for finish, restrained palette
and environmental cohesion where the older image differs. Neither image defines
jump distances, mechanics, camera behavior, controls, ranks, HUD values or exact
level geometry. Sample menu version, slogans and Flow Lab prominence are concept
content, not additional user requirements. Current gameplay and navigation
contracts remain authoritative. See [ART_DIRECTION](../../ART_DIRECTION.md),
[production report](../PRODUCTION_0120.md) and [ADR 0025](../Decisions/0025-visual-reference-continuity.md).

## Interpretation in the existing project

- Compose foreground landing edges, supporting near architecture, readable
  mid-distance route and a distant destination silhouette. Preserve open jump
  view cones and physical progress through each world's own places.
- Cohesion comes from ivory stone/ceramic, restrained dark teal structure,
  consistent material scale/roughness and small functional cyan/warm accents.
  Mountain, Abyss, Foundry and Windward retain their local identities.
- Warm directional light, legible shaded tops, fading far silhouettes and huge
  sky/depth should establish atmosphere. Vegetation and water frame places;
  they must not hide landings or look like false safe surfaces.
- Interpret the target using existing Unity assets and modular presentation
  profiles. Favor authored masses, shared materials, static batching and bounded
  shadows. Dense foliage, volumetric clouds, reflection effects and many lights
  are not implied requirements. Maintain the 60 FPS mobile target; measure on S23.
- Menu composition uses real UGUI buttons, text, focus/hit areas and responsive
  safe-area layout, with separate logo/icon/background assets. Background fills
  the display while controls fit both landscape directions. Preserve functional
  Campaign, Spiral, Settings and separate development access. Never import the
  flattened menu as the interface or bake buttons into a background.
- Ordinary transitions should foreground the supplied Ryder's Road identity:
  white emblem/wordmark, navy field, restrained blue glow and optional separate
  tagline. Jesaias stays a small secondary watermark or startup studio splash.
  Retain the runtime travelling indicator and real readiness-driven activation;
  use unscaled animation without minimum holds or fabricated progress.
- Use approved source logo/icon assets and reference-derived production assets,
  not arbitrary fonts or generated replacement branding. This reference update
  does not itself replace the existing runtime logo or application icon.

## Provenance and storage

Original user-supplied PNGs are copied byte-for-byte, outside Unity Assets and
Resources, so reference archiving adds no runtime texture or APK cost. They are
visual guidance, not a third-party asset license or permission to copy protected
content. No cropping, retouching, UI flattening or runtime derivatives in this
update. Existing production branding sources remain in `Assets/Branding/Source`.

- `2026-09-09_gameplay_quality_target.png`
  - Source: `ChatGPT Image Sep 9, 2026, 12_00_55 AM.png`
  - SHA256: `22AC17C9EAA9A9D273722C1F326964CC3A5CC674A7C122B0E180C2D3A4E81D84`
- `2026-09-09_loading_brand_reference.png`
  - Source: `ChatGPT Image Sep 9, 2026, 12_00_38 AM.png`
  - SHA256: `73B6B35C3D90DA2C94EE09910FCE9C9816779B68C62072C95CDD3FCC8298D792`
- `2026-09-09_main_menu_concept.png`
  - Source: `ChatGPT Image Sep 9, 2026, 12_00_27 AM.png`
  - SHA256: `CC47B61BA8B7D32BD43708EA5E626170E411F3A1E4B103AF0FE73FD1FB746DE1`
