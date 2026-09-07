# ADR 0005: Keep device-rescue fixes inside Phase 1 systems

Status: Accepted - 2026-08-11

## Decision

Treat `0.1.1-device-rescue` as a Phase 1B stabilization pass. Fix the reported
Android control, material, readability, diagnostics, and temporary UI problems
inside existing movement, input, visual, UI, diagnostics, and validation
systems.

Do not use this pass to start Phase 2 or add ranks, ghosts, scoring,
progression, economy, shops, online systems, final artwork, or lore systems.

## Consequences

The prototype can become readable and testable on a real phone without
expanding product scope. Movement remains data-driven through profiles, visual
roles remain replaceable, and physical-device retest remains the approval gate
before later phases resume.
