using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Avoidance.SaveSystem
{
    public sealed class JsonSaveService : ISaveService
    {
        private readonly ISaveFileStore _store;
        private readonly string _gameVersion;
        private readonly SortedDictionary<int, ISaveMigration> _migrations =
            new SortedDictionary<int, ISaveMigration>();

        public JsonSaveService(ISaveFileStore store, string gameVersion)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _gameVersion = string.IsNullOrWhiteSpace(gameVersion) ? "unknown" : gameVersion;
        }

        public SaveDocument Current { get; private set; }
        public int SchemaVersion => SaveSchema.CurrentVersion;

        public SaveLoadResult Initialize() => Reload();

        public SaveLoadResult Reload()
        {
            var primaryError = "Primary save not found or unreadable.";
            var backupError = "Backup save not found or unreadable.";
            if (_store.TryReadPrimary(out var primary)
                && TryDeserializeAndMigrate(primary, out var primaryDocument, out primaryError))
            {
                Current = primaryDocument;
                return new SaveLoadResult(true, SaveLoadSource.Primary, "Loaded primary save.");
            }

            if (_store.TryReadBackup(out var backup)
                && TryDeserializeAndMigrate(backup, out var backupDocument, out backupError))
            {
                Current = backupDocument;
                SaveInternal(true);
                return new SaveLoadResult(true, SaveLoadSource.Backup, "Recovered from backup save.");
            }

            Current = CreateNewDocument();
            var written = Save();
            var details = written
                ? "Created a new save."
                : $"Could not write a new save. Primary: {primaryError}; backup: {backupError}";
            return new SaveLoadResult(written, SaveLoadSource.New, details);
        }

        public bool Save() => SaveInternal(false);

        private bool SaveInternal(bool preserveBackup)
        {
            if (Current == null)
            {
                Current = CreateNewDocument();
            }

            Current.schemaVersion = SaveSchema.CurrentVersion;
            Current.gameVersion = _gameVersion;
            try
            {
                _store.WriteAtomic(JsonUtility.ToJson(Current, true), preserveBackup);
                return true;
            }
            catch (Exception exception) when (
                exception is IOException
                || exception is UnauthorizedAccessException
                || exception is InvalidOperationException)
            {
                Debug.LogError($"Save write failed: {exception.Message}");
                return false;
            }
        }

        public void ResetDevelopmentData()
        {
            if (!Debug.isDebugBuild && !Application.isEditor)
            {
                throw new InvalidOperationException("Save reset is only available in development.");
            }

            _store.DeleteAll();
            Current = CreateNewDocument();
            Save();
        }

        public void RegisterMigration(ISaveMigration migration)
        {
            if (migration == null)
            {
                throw new ArgumentNullException(nameof(migration));
            }

            if (migration.ToVersion != migration.FromVersion + 1)
            {
                throw new ArgumentException("Migrations must advance exactly one schema version.", nameof(migration));
            }

            _migrations.Add(migration.FromVersion, migration);
        }

        private bool TryDeserializeAndMigrate(
            string sourceJson,
            out SaveDocument document,
            out string error)
        {
            document = null;
            error = null;
            if (string.IsNullOrWhiteSpace(sourceJson))
            {
                error = "Save is empty.";
                return false;
            }

            try
            {
                var header = JsonUtility.FromJson<SaveHeader>(sourceJson);
                if (header == null || header.schemaVersion <= 0)
                {
                    error = "Save schema version is missing or invalid.";
                    return false;
                }

                if (header.schemaVersion > SaveSchema.CurrentVersion)
                {
                    error = "Save was created by a newer game version.";
                    return false;
                }

                var migratedJson = sourceJson;
                var version = header.schemaVersion;
                while (version < SaveSchema.CurrentVersion)
                {
                    if (!_migrations.TryGetValue(version, out var migration))
                    {
                        error = $"No migration is registered from schema {version}.";
                        return false;
                    }

                    migratedJson = migration.Migrate(migratedJson);
                    version = migration.ToVersion;
                }

                document = JsonUtility.FromJson<SaveDocument>(migratedJson);
                if (document == null || document.schemaVersion != SaveSchema.CurrentVersion)
                {
                    error = "Migrated save did not match the current schema.";
                    return false;
                }

                document.settings ??= new Avoidance.Core.Services.GameSettings();
                document.progression ??= new ProgressionData();
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }

        private SaveDocument CreateNewDocument()
        {
            return new SaveDocument
            {
                schemaVersion = SaveSchema.CurrentVersion,
                gameVersion = _gameVersion
            };
        }

        [Serializable]
        private sealed class SaveHeader
        {
            public int schemaVersion;
        }
    }
}
