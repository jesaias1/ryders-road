# Agent Working Agreement

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

Additional rules:

- Use stable content IDs according to `ARCHITECTURE.md`; never use hierarchy
  positions, display names, scene names, or list indexes as identity.
- Add save migrations instead of silently changing persisted meaning.
- Keep runtime assemblies independent of Editor assemblies.
- Do not add a future system merely to make a folder non-empty.
- Preserve a 60 FPS mobile budget and record device-dependent checks honestly.
- Do not begin Phase 2 until Phase 1B records physical Android feedback.
