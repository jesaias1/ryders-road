# ADR 0004: Compose movement from replaceable runtime components

Status: Accepted — 2026-07-29

## Decision

Use `PlayerInputRouter`, `ParkourMotor`, `FirstPersonCameraRig`,
`CheckpointService`, `RestoreController`, presentation components, diagnostics,
and session reporting as separate collaborators coordinated by a thin player
runtime component. Store tuning in focused ScriptableObject profiles.

## Consequences

Movement can be tuned or replaced without rewriting touch UI, camera effects,
save storage, or future level content. Narrative labels have no authority over
gameplay rules. Transform-based moving-platform translation is supported now;
rotation requires a separately tested extension.
