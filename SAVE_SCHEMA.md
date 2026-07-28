# Save Schema

## Storage

The local file is `Application.persistentDataPath/Saves/save.json`. Writes go
to `save.json.tmp`. When a primary exists, the store replaces it and preserves
the previous valid bytes as `save.json.bak`; platforms without `File.Replace`
use a copy/delete/move fallback. Load order is primary, backup, then new save.
A recovered backup is immediately written back as primary.

This provides practical corruption resistance, not transactional durability
against every possible storage or power failure.

## Current schema: V2

```json
{
  "schemaVersion": 2,
  "gameVersion": "0.0.1",
  "settings": {
    "masterVolume": 1.0,
    "lookSensitivity": 0.5,
    "diagnosticsVisible": true
  },
  "progression": {}
}
```

`progression` is intentionally empty.
Currency, level progress, inventory, best times, and ranks do not exist yet.

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
