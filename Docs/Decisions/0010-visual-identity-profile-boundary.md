# 0010 - Visual Identity Profile Boundary

Date: 2026-08-12

## Decision

Phase 0.4.0 visual identity work remains profile-driven and presentation-only.
`ModuleVisualProfile` and `ModuleEnvironmentProfile` own colors, lighting,
sky, density, HUD color, and reusable visual-kit presentation values.
`ModuleSceneController` may render those values as temporary runtime geometry,
particles, and HUD styling, but gameplay systems must not inspect material
colors, decorative meshes, benchmark anchors, or visual-only objects to decide
movement, Restore, Patch, rank, score, progression, or saves.

## Rationale

RYDERS BLOCK needs a stronger visual identity while the movement model is being
protected after Movement V1. Keeping art in profile/content layers lets the
game move toward the target reference without silently changing feel or module
rules.

## Consequences

- Visual profiles can evolve or be replaced without a save migration.
- Module definitions may add visual-only decorations, but playable surfaces and
  triggers remain explicit gameplay data.
- Visual benchmark anchors are developer review tools only.
- Validation checks the 0.4.0 visual profile, quality tier, benchmark docs, and
  URP-safe material roles.
- Future production art can replace runtime primitive kit pieces without
  rewriting movement or progression systems.
