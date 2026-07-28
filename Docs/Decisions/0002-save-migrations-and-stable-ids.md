# ADR 0002: Versioned saves and semantic stable IDs

Status: Accepted — 2026-07-29

## Decision

Persist a schema version and game version, migrate one schema step at a time,
keep a previous-file backup, and use immutable semantic stable IDs for future
content records.

## Consequences

Persisted meaning cannot be casually renamed or reordered. Released ID changes
require aliases/migrations. Every schema transition requires test fixtures.
Scene and hierarchy refactors do not invalidate player records.
