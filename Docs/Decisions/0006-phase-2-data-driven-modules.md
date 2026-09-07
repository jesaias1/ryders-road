# ADR 0006: Data-driven first parkour modules

Status: Accepted - 2026-08-11

## Decision

Phase 2 introduces `ModuleDefinition` assets as the source of module identity,
metadata, route content, Restore Points, Patch Blocks, shortcuts, mechanics,
visual profile, and environment profile. `ModuleRunner` builds the selected
module from data; reusable mechanics remain separate components.

The first three modules are short prototypes, but Module 03 is shaped as a
compact vertical-ascent proof around a central floating structure so future
large continuous maps can grow without redesigning movement.

## Consequences

Level geometry is editable without changing player or camera scripts. Future
modules can become spirals, bridges, towers, waterfalls, shafts, machines, or
floating islands using the same data pattern. The current left-thumb steering
and auto-camera behavior remains provisional until physical-device feedback is
recorded.
