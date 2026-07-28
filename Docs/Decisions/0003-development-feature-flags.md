# ADR 0003: Release-safe development feature flags

Status: Accepted — 2026-07-29

## Decision

Store lightweight flag overrides locally, expose them in development
diagnostics, and force flags off in release unless each definition explicitly
allows release use.

## Consequences

Experiments can be removed or replaced without branching core architecture.
Adding a release-enabled flag is an explicit content decision. Flags do not
replace save migrations or permanent configuration.
