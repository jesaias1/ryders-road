# Agent Working Agreement

The current player-facing game title is `RYDER'S ROAD`. `RYDERS BLOCK` may
remain as a legacy internal codename in stable technical identifiers. Do not
perform broad internal renames solely for cosmetic consistency.

Ryder is the main playable character.

Use the approved `RYDER'S ROAD` logo and application icon assets as the primary
branding sources rather than recreating the brand with arbitrary fonts or
generated substitutes.

This is a long-lived commercial Unity project. Before changing source or
content, read `ARCHITECTURE.md`, `ART_DIRECTION.md`, `TESTING.md`,
`SAVE_SCHEMA.md` when persistence is involved, and the latest decision records.

Every coding agent must:

1. Read architecture and module documentation before editing.
2. Inspect working systems before replacing them.
3. Avoid unrelated rewrites.
4. Preserve public contracts unless a documented migration is required.
5. Keep tunable values out of hardcoded gameplay scripts.
6. Maintain Android landscape-left and landscape-right compatibility.
7. Preserve Editor controls alongside touch controls.
8. Add regression tests for changed behavior.
9. Update documentation after every phase.
10. Report what was verified and what still requires physical-device testing.
11. Never claim physical-device testing was performed unless it actually was.
12. Leave the repository compiling and testable.
13. Avoid creating a monolithic game manager.
14. Keep systems modular and removable.
15. Treat `Docs/VisualReferences/target_gameplay.png` as the long-term target
    without copying protected game assets, branding, textures, characters,
    sounds, interface, or exact visual language.
16. Before major environment, UI, lighting, material, VFX, block-art or
    first-person presentation work, inspect
    `Docs/VisualReferences/target_gameplay.png` and `ART_DIRECTION.md`.
17. Visual changes must never silently alter the GoldSrc-style player movement
    model.

Additional rules:

- Use stable content IDs according to `ARCHITECTURE.md`; never use hierarchy
  positions, display names, scene names, or list indexes as identity.
- Add save migrations instead of silently changing persisted meaning.
- Keep runtime assemblies independent of Editor assemblies.
- Do not add a future system merely to make a folder non-empty.
- Preserve a 60 FPS mobile budget and record device-dependent checks honestly.
- Do not begin Phase 2 until Phase 1B records physical Android feedback.
- Mr Ryder is a coder and game engineer creatively blocked by bugs and
  unfinished systems. The player exists inside his code and repairs broken
  sections through first-person block parkour, ending at a Patch Block.
- Lore terminology must never couple movement, camera, checkpoint, restore, or
  level-completion rules to narrative presentation.
- Phase 1 may use `Null Space`, `Restore Point`, `Restore`, and temporary
  `Patch Block` labels, but it must not implement a narrative system.
- Any valid completion of a main RYDERS BLOCK module earns at least Bronze and
  is sufficient to continue the normal campaign. Higher ranks represent
  mastery, never mandatory progression.
- Visual production must follow the Phase 3B pillars in `ART_DIRECTION.md`:
  readability before detail, simple shapes with strong composition, color as
  gameplay communication, huge sky/depth, polished indie identity, visible
  flow, mobile-first cost control, and levels as memorable places.
- Touch controls must prioritize thumb comfort over screen-edge utilization:
  normal play should keep left/right thumb travel inside ergonomic inner
  regions while preserving broad touch acquisition and landscape-left/right
  safe-area support.
- Large RYDERS BLOCK maps should provide a memorable macro structure and
  visible physical progression. Standard routes support ordinary campaign
  completion; advanced movement creates faster lines rather than mandatory
  progression.
- Smart Parkour Camera is an Easy Mode camera presentation system only. It
  may frame broad course flow, velocity, branches, merges, shortcuts, and
  vertical ascent, but it must never move the player, change velocity, route
  select, auto-jump, magnetize landings, prevent falling, or target individual
  platform centers.
- Classic Mode must preserve manual right-thumb camera behavior and
  CS1.6/GoldSrc-style movement mastery. Any Easy Mode camera tuning must be
  regression-tested against Classic manual camera and editor controls.
