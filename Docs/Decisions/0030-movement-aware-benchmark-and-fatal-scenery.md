# ADR 0030 — Movement-aware benchmark and fatal scenery

2026-09-11. The user's S23 feedback approves the substantially improved air control
and asks for space to use it. Do not reduce movement tuning to fit old geometry.

## Scope and ownership

Windward Observatory (`module.005.foundry-pulse`) is the single route benchmark.
Its existing north sweep, direct chord, diagonal ascent and telescope finish make
different lines readable within one world. MovementBenchmarkAuthoring is an
explicit, idempotent Editor asset operation. Runtime loads serialized geometry.
No new world, motor, easy physics, route assist, rank calibration or music change.

The arrival runway and offset stepping courts support ordinary hops and intentional
skips. North galleries are broad enough to carve and recover; the chord cuts the
long sweep. Ascent and telescope terraces support further skip choices. Intermediate
safe surfaces remain. Fewer tiny-pad decisions per metre replace the compressed
old rhythm; the final ascent pad is merged into a broad recovery terrace. The two
pressure platforms use a three-second collapse window for their larger footprint. Shared movement profile,
acceleration, air steering, jump arc and 18 m/s safety cap remain byte-identical.

## Geometry contract

`WorldGeometryKind` defines Traversable, FatalScenery and NonCollidingScenery.
Module blocks, moving supports, Boosts and authored surf remain gameplay geometry.
BiomeWorldObject's historical `HasPlayableArchitecture` field means retained mesh
collision, not authorization to traverse it. An explicit `traversableArchitecture`
opt-in is now required for a biome mesh intended as a real route opportunity.
Otherwise retained scenery collision is fatal. Non-colliding world objects keep
the existing collider-removal path; depth and distant art are preserved.

AuthoredSurface carries the runtime classification and immutable source-mesh
collision truth. Its legacy RestoreOnLanding field/setter remain for compatibility,
but fatal behavior now includes every contact normal. ModuleSceneController applies
the data policy during composition. There are no runtime object-name checks.

SceneryLandingRecovery forwards fatal contact to RestoreController.RequestFatalContact.
That immediately records the normal fall, clears input and suspends motor simulation
until the existing short Restore delay finishes. Suspension also stops remaining
post-Move grounding/landing callbacks and platform-carry continuation. ResetMotion
clears it. No second held jump, hidden checkpoint or wait for a lower void is possible.
Spawn protection does not authorize fatal scenery. Legitimate surfaces retain the
same movement/collision rules; the lower world threshold remains for total misses.

Benchmark undersides use narrow keels and diagonal braces instead of broad flat
secondary slabs. The instrument is lower and displaced from the chord; the
telescope follows the new finish below the terraces. Bright route decks remain
distinct from dark structural silhouettes. No giant near-route death floor.

## Record compatibility

Route and collision semantics affect record comparability. Campaign content versions
become Sky City 3, Mountain 4, Ancient Abyss 8, Solar Foundry 4 and Windward 4.
Only Windward route geometry is reauthored; other version bumps identify fatal
scenery semantics. Spiral has no retained solid biome scenery and stays content 2.
Movement stays compatibility 5. Existing V4 versioned best/history machinery keeps
old records and unlocks; no schema change or final threshold calibration.

## Acceptance and remaining gate

Test ordinary continuous completion, held chains, controlled high-speed skips with
imperfect aim, actual fatal collisions and all valid decks. Audit each existing
world, including non-colliding Spiral. Run the full regression suites and Android
build, inspect runtime captures, then deliver and stop for physical S23 feedback.
Scripted pilots and controlled incoming velocity fixtures establish feasibility,
not human ease, proof of earning that speed along a complete run, or rank thresholds.
