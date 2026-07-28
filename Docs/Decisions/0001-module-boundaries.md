# ADR 0001: One-way module boundaries

Status: Accepted — 2026-07-29

## Decision

Use small feature assemblies with `Game.Core` as contracts/foundation.
Bootstrap is the only composition assembly and Editor/tests are platform
isolated. No global monolithic game manager is permitted.

## Consequences

Systems can be replaced through contracts without unrelated rewrites. New
cross-module requirements may need a small neutral contract or data model in
Core. Circular assembly references are a design failure, not something to
work around with reflection or duplicated behavior.
