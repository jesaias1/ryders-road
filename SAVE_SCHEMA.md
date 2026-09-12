## 0.16.0 content compatibility

Movement compatibility remains 5 and save schema remains V4. Windward becomes
content 4 for its benchmark route; Sky City/Mountain/Ancient Abyss/Foundry become
3/4/8/4 for fatal scenery semantics without reauthoring their routes. Spiral stays
2. Existing versioned PB/history buckets preserve prior results and unlocks.
Rank thresholds are unchanged and not newly calibrated. See ADR 0030.

## 0.15.0 shared compatibility and music settings

The user's rollout request authorizes normal compatibility 5. Existing V4 versioned
PB/history rules separate these results from compatibility 1–4 without rewriting
old records or unlocks. No rank-threshold or content-version migration occurs.
GameSettings adds musicVolume (default 0.65). PlayerPrefs settings.music-volume is
the runtime authority, matching existing settings practice; old save meaning is
unchanged and absent music keys use the default. No schema-version bump is needed.
Earlier trial-only compatibility restrictions below are historical (ADR 0029).

## 0.14.0 isolated movement candidate

No schema or persisted meaning changes. Compatibility 4 exists only in transient
Campaign trial E / Flow Lab. Trial attempts, rewards, completions and PBs skip save
writes; E gets its own session-best key. Accepted Campaign still uses compatibility
1 with the existing V4 historical/versioned PB extension. Physical approval is
required before any explicit future Campaign compatibility promotion (ADR 0027).

## 0.13.0 compatible PB extension (V4 additive)

`progression.historicalRecords` and `progression.versionedBests` are additive
ModuleProgressRecord arrays. Missing arrays normalize empty on read. Before the
first new completion for a module with an old PB, snapshot its entire aggregate
record once into historicalRecords, including splits and its original metadata.
Never mutate that snapshot. Future PB buckets use module ID + content version +
movement compatibility + rank threshold version. Invalid runs never write a PB
bucket. Slower runs on a revised route can establish their first compatible PB.

Existing modules/attempts/completion counts/highest ranks/reward IDs still mean
what they meant in V4 and continue owning campaign access. The aggregate PB may
improve later, but its original snapshot and prior version buckets remain. Old
aggregate metadata was overwritten on each completion, even when its PB stayed;
it cannot reliably identify the PB's route version. Do not seed new compatible
PBs or rank comparisons from it. The UI labels retained history and shows blank
current-route PB until matching new evidence exists.

No existing field or schema meaning changes, so no destructive migration or
version increment is necessary. VersionedBestTests loads old V4 JSON, exercises
slower/faster changed routes, invalid runs, version separation, serialization
reload and preserved unlocks. Legacy backup/migration tests remain required.

## 0.11.0 Campaign presentation and World 005

ADR 0023 and Docs/PRODUCTION_0110.md supersede historical production stop gates.
Accepted Campaign controls and all isolated 0.10.0 movement trials remain intact.
World 005 is Windward Observatory, using reserved `module.005.foundry-pulse`;
Spiral stays separate. V4 identities and persisted meanings are unchanged.
Continuous directional skies and opt-in profile lighting replace Campaign's
mismatched illustrated cubemap. Loading keeps the existing transition host with
an editable Jesaias emblem and lightweight opacity pulse. Physical S23 approval,
rank calibration, final audio mix and sustained performance remain open.

## 0.9.9 physical feedback and Campaign Flow trial

Schema remains V4. Solar Foundry content version 2 records revised scenery recovery/composition without changing IDs or rewriting historical PBs. Campaign Flow trials have no save-service writes: attempts, completions and rewards are transient; their session bests separate road/content-version/mode. Normal Campaign persistence retains its existing behavior. See ADR 0021.

## 0.9.8 Solar Foundry

V4 is unchanged. New module.004.solar-foundry records use existing fields and content version 1. Existing IDs, PB metadata and record meanings remain intact; module.004.the-spiral is distinct and stays outside Campaign. Earned Bronze on 003 unlocks 004, without rewriting historical saves. No migration is needed for this additive content. Legacy-save and fresh-save regression coverage is in SolarFoundryTests.

# Save Schema

## 0.9.4 Mountain World authority

Schema stays V4. Existing completed/completionCount retain historical all-finish meaning; Campaign eligibility now checks earned best/highest ranks and validity. Practice-only records do not unlock roads. Later practice never erases an earned rank, and later genuine Campaign completions preserve earlier access. Module 002 content version becomes 2. No save wipe or migration. See ADR 0016.

## 0.9.3 content compatibility

Schema remains V4. Module 001 advances to content version 2 after widening introductory landings; route/Restore/Patch/module IDs and persisted meaning stay stable. Prior PBs retain recorded content-version metadata. No migration or new economy/pickup data was introduced.

## 0.9.1 compatibility note

Schema remains V4. Flow Shards are attempt-local and never write to economy,
inventory or module progression. Module 003 now declares content version 6;
historical PBs and their recorded content-version metadata remain intact.
No persisted field meaning or stable module ID was changed, so no migration is
required for this continuation.


## Storage

The local file is `Application.persistentDataPath/Saves/save.json`. Writes go
to `save.json.tmp`. When a primary exists, the store replaces it and preserves
the previous valid bytes as `save.json.bak`; platforms without `File.Replace`
use a copy/delete/move fallback. Load order is primary, backup, then new save.
A recovered backup is immediately written back as primary.

This provides practical corruption resistance, not transactional durability
against every possible storage or power failure.

`0.4.7-brand-alpha-presentation` changes the player-facing product name to
`Ryder's Road` but keeps the Android package ID stable. On editor/desktop paths
where Unity may include the product name, `SavePathMigrationUtility` copies
legacy `RYDERS BLOCK/Saves/save.json` and `.bak` into a missing current
`Ryder's Road/Saves` directory. It never overwrites an existing current save.

## Current schema: V4

```json
{
  "schemaVersion": 4,
  "gameVersion": "0.4.7",
  "settings": {
    "masterVolume": 1.0,
    "lookSensitivity": 0.5,
    "diagnosticsVisible": true
  },
  "progression": {
    "modules": [
      {
        "moduleId": "module.001.first-steps",
        "completed": true,
        "bestTimeSeconds": 38.421,
        "bestRank": "Silver",
        "bestScore": 99961579,
        "completionCount": 1,
        "attemptCount": 3,
        "bestSplits": [
          { "checkpointId": "restore.module-001.midpoint", "seconds": 18.25 }
        ],
        "latestCompletionTimeSeconds": 38.421,
        "latestRank": "Silver",
        "highestRank": "Silver",
        "moduleContentVersion": 1,
        "movementCompatibilityVersion": 1,
        "rankThresholdVersion": 1,
        "rankCalibrationState": "Uncalibrated",
        "runValidity": "ValidUnassisted"
      }
    ],
    "exceptionalUnlocks": [],
    "claimedRewardIds": [
      "reward.module-001.bronze.shards",
      "reward.module-001.bronze.package",
      "reward.module-001.bronze.skin"
    ]
  },
  "economy": {
    "balances": [
      { "currencyId": "currency.patch-shards", "amount": 0 }
    ],
    "lootPackagesOpened": 0
  },
  "inventory": {
    "items": [
      {
        "contentId": "loot.package.starter",
        "quantity": 1,
        "firstAcquiredUtc": "2026-08-16T12:00:00Z",
        "lastAcquiredUtc": "2026-08-16T12:00:00Z",
        "sourceId": "reward.module.001.bronze"
      }
    ],
    "unlocks": [
      {
        "contentId": "skin.ryder.default",
        "unlockType": "Skin",
        "unlocked": true,
        "unlockedUtc": "2026-08-16T12:00:00Z",
        "sourceId": "default"
      }
    ],
    "equippedCosmetics": [
      { "slotId": "hands", "contentId": "skin.ryder.default" }
    ]
  }
}
```

Progression is keyed by stable module ID. Normal unlocks are derived from
completed previous modules; `exceptionalUnlocks` exists for future special
cases without duplicating ordinary campaign state.

Economy and inventory containers are present as a progression foundation only.
They use stable content IDs and currently start empty. Runtime helper methods
can add/spend currency, grant stackable items, set unlocks, and equip cosmetics
only after the target content has an unlocked record. Module completion rewards
now use stable reward IDs recorded in `claimedRewardIds` so first-completion
rewards do not duplicate. No loot package opening flow, shop, purchase,
skin-equipment UI, ghosts, or online records exist yet.

Phase 1 and Phase 2 did not change the save schema. Phase 3 migrates V2 to V3
by preserving settings and adding empty progression arrays. Phase 3B, the
0.3.8 movement-lock pass, the 0.3.9 Movement V1 pass, 0.4.x visual/camera
passes, and the 0.4.7 brand pass initially kept V3 unchanged and added only
visual, control, movement, camera, fullscreen, product-name, or build-version
data outside saves. The 2026-08-16 progression-foundation pass migrates V3 to
V4 by preserving settings/progression and adding empty economy and inventory
containers. Existing V4 documents without `claimedRewardIds` normalize that
array to empty on load/save. Movement beta reports are separate development
JSON files under
`Application.persistentDataPath/SessionReports`. Module attempt reports are
separate development JSON files under
`Application.persistentDataPath/ModuleReports`. They are not progression
records and are safe to delete between test sessions.

Raw best times are the source of truth. If rank thresholds change, current
displayed rank can be recalculated from the stored raw time and the current
threshold data without forcing a replay.

## Migrations

Migrations implement `ISaveMigration`, advance exactly one version, and are
registered in the bootstrap composition root. Loading rejects a future schema
or a version with a missing migration. `SaveV1ToV2Migration` demonstrates
copying and clamping V1 volume into the settings container. Every migration
requires a regression test using representative old JSON.

## Development reset

`ISaveService.ResetDevelopmentData` deletes primary, backup, and temporary
files and creates a fresh current document. It throws in non-development
players. Never expose this method through release UI.
