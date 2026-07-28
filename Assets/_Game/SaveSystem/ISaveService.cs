using System.Collections.Generic;

namespace Avoidance.SaveSystem
{
    public interface ISaveService
    {
        SaveDocument Current { get; }
        int SchemaVersion { get; }
        SaveLoadResult Initialize();
        bool Save();
        SaveLoadResult Reload();
        void ResetDevelopmentData();
        void RegisterMigration(ISaveMigration migration);
    }

    public enum SaveLoadSource
    {
        New,
        Primary,
        Backup
    }

    public readonly struct SaveLoadResult
    {
        public SaveLoadResult(bool success, SaveLoadSource source, string message)
        {
            Success = success;
            Source = source;
            Message = message;
        }

        public bool Success { get; }
        public SaveLoadSource Source { get; }
        public string Message { get; }
    }

    public interface ISaveMigration
    {
        int FromVersion { get; }
        int ToVersion { get; }
        string Migrate(string sourceJson);
    }

    public interface ISaveFileStore
    {
        bool PrimaryExists { get; }
        bool BackupExists { get; }
        bool TryReadPrimary(out string json);
        bool TryReadBackup(out string json);
        void WriteAtomic(string json, bool preserveBackup = false);
        void DeleteAll();
    }
}
