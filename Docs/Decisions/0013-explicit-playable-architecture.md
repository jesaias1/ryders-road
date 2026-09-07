# ADR 0013 — Explicit playable architecture and optional flow feedback

Accepted from physical S23 failure report, 2026-09-06.

Near architecture that visibly invites landing must opt into `AuthoredSurface`
and use collision matched to its source mesh. BiomeWorldObject explicitly declares
HasPlayableArchitecture; runtime preserves only those authored colliders. Distant
scenery remains collider-free and must stay spatially separate from landing choices.
This supersedes blanket collider-free near-world statements in historical phases.
Unity static batching may replace the render mesh, so validation retains the
authored mesh identity. Dynamic mechanics still use their data-authored colliders.

Flow Shards are an optional, attempt-local mastery challenge. Restore retains
collection, Retry resets it. They do not affect Bronze, rank, currency or V4 saves.
Movement, input and camera remain independent of feedback. Audio must use actual
approved clips; missing clips stay silent. Haptics are preference-aware and bounded.

Shared scene-loader events drive a removable UI loading-film adapter. Gameplay
assemblies do not reference UI. Results suppress input after completion without
changing the movement solver. Module 003 content version is 6; existing historical
PB records remain intact and retain their recorded content-version metadata.

See `Docs/S23_091_CONTINUATION.md` for affected objects, evidence and remaining QA.
