# RYDERS BLOCK Ranking And Scoring

## Permanent Rule

Any valid completion of a main RYDERS BLOCK module earns at least Bronze and is
sufficient to continue the normal campaign. Higher ranks represent mastery,
never mandatory progression.

## Timer Rules

The run timer starts when a module run begins and stops when the Patch Block is
activated. Falling into Null Space and using Restore do not reset the timer.
Those mistakes affect the final time naturally.

`RESTART MODULE` starts a fresh run, resets the timer, rebuilds transient block
states, and returns the player to the module start.

Timer display uses `MM:SS.mmm`, while internal records store seconds as a
floating-point value.

## Ranks

Ranks are:

- Bronze
- Silver
- Gold
- Diamond

Bronze has no maximum completion-time requirement. Silver, Gold, and Diamond
use module-specific thresholds stored on `ModuleDefinition`.

Rank thresholds must satisfy:

`DiamondTime < GoldTime < SilverTime`

Current Phase 3 thresholds are provisional and `Uncalibrated`:

| Module | Silver | Gold | Diamond |
| --- | ---: | ---: | ---: |
| `module.001.first-steps` | 42.000s | 32.000s | 26.000s |
| `module.002.moving-parts` | 62.000s | 48.000s | 38.000s |
| `module.003.flow-error` | 90.000s | 68.000s | 52.000s |

Physical Android timing data is required before these numbers become
calibrated.

## Score

Time is the authoritative competitive result. Score exists as a satisfying
large-number representation.

For valid comparable runs in the same module:

`faster time = better score`

The Phase 3 formula is:

```text
score = max(1, 100000000000 - ceil(completionSeconds * 1000000))
```

Because the formula subtracts elapsed microseconds from a constant, a lower
completion time produces a higher score at the score service's precision.
There are no random bonuses, style bonuses, collectibles, or hidden
multipliers.

## Personal Bests

The save stores raw best completion time by stable module ID. A faster valid
run replaces the stored personal best. A slower valid run updates latest
completion data but does not replace the PB. Invalid development runs never
replace PB time, PB rank, PB score, PB splits, or highest competitive rank.

Best displayed rank is recalculated from raw best time and current thresholds,
so threshold retuning can improve a stored run's current rank without forcing a
replay. Historical raw time remains the source of truth.

## Splits

Restore Point activations record split times for the current run. When a PB run
has a matching split, the HUD can show a small delta:

- negative means ahead of PB;
- positive means behind PB.

The split list is unbounded by design so future long spiral/tower maps can use
many Restore Points.

## Run Validity

Phase 3 run validity values are:

- `ValidUnassisted`
- `ValidAssisted`
- `InvalidDevelopment`
- `InvalidContentVersion`
- `InvalidOther`

All normal Phase 3 runs are `ValidUnassisted`. Future power-ups can use
`ValidAssisted`. Developer overrides, skips, teleport-style testing, forced
Patch Blocks, and inappropriate time-scale changes should be
`InvalidDevelopment`.

Only legitimate valid runs update official personal bests.

## Progression

Normal campaign unlocks are derived from completion:

- Module 01 is unlocked initially.
- Module 02 unlocks after any valid Module 01 completion.
- Module 03 unlocks after any valid Module 02 completion.

Bronze is enough. Silver, Gold, and Diamond are never required for normal
campaign progression.

The save also contains an exceptional-unlock list for future special cases, but
normal Phase 3 unlocks are derived to avoid inconsistent duplicated state.

## Calibration Workflow

Use `RYDERS BLOCK > Rank Calibration` to inspect module estimates, current
thresholds, calibration state, local completion samples, fastest time, median,
average, attempt count, and fall count. The tool can suggest provisional values
from local reports, but it does not automatically overwrite thresholds.

When thresholds change, increment the threshold version. Do not delete raw PB
times. Current displayed rank should be computed from stored raw time and the
current threshold data unless a later compatibility rule explicitly marks the
record invalid.
