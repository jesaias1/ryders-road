# ADR 0007: Bronze progression and raw-time personal bests

Status: Accepted - 2026-08-12

## Decision

Any valid completion of a main RYDERS BLOCK module earns at least Bronze and
unlocks the next normal campaign module. Silver, Gold, and Diamond are mastery
targets only.

Persist raw best completion time by stable module ID. Current displayed rank is
computed from raw time and current module thresholds so threshold recalibration
can improve displayed rank without deleting or rewriting the original result.

## Consequences

Casual players can finish the future campaign with Bronze completions. Rank
thresholds can be tuned after physical Android testing without forcing replays.
Invalid development runs can still be useful for testing but must not overwrite
official personal bests or competitive highest-rank records.
