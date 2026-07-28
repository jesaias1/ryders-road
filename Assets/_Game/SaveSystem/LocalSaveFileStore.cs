using System;
using System.IO;

namespace Avoidance.SaveSystem
{
    public sealed class LocalSaveFileStore : ISaveFileStore
    {
        private readonly string _primaryPath;
        private readonly string _backupPath;
        private readonly string _temporaryPath;

        public LocalSaveFileStore(string directory, string fileName = "save.json")
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("Save directory is required.", nameof(directory));
            }

            Directory.CreateDirectory(directory);
            _primaryPath = Path.Combine(directory, fileName);
            _backupPath = _primaryPath + ".bak";
            _temporaryPath = _primaryPath + ".tmp";
        }

        public bool PrimaryExists => File.Exists(_primaryPath);
        public bool BackupExists => File.Exists(_backupPath);

        public bool TryReadPrimary(out string json) => TryRead(_primaryPath, out json);
        public bool TryReadBackup(out string json) => TryRead(_backupPath, out json);

        public void WriteAtomic(string json, bool preserveBackup = false)
        {
            File.WriteAllText(_temporaryPath, json);

            if (!File.Exists(_primaryPath))
            {
                File.Move(_temporaryPath, _primaryPath);
                return;
            }

            if (preserveBackup)
            {
                File.Delete(_primaryPath);
                File.Move(_temporaryPath, _primaryPath);
                return;
            }

            try
            {
                File.Replace(_temporaryPath, _primaryPath, _backupPath);
            }
            catch (PlatformNotSupportedException)
            {
                ReplaceWithPortableFallback();
            }
            catch (IOException)
            {
                ReplaceWithPortableFallback();
            }
        }

        public void DeleteAll()
        {
            DeleteIfExists(_temporaryPath);
            DeleteIfExists(_primaryPath);
            DeleteIfExists(_backupPath);
        }

        private static bool TryRead(string path, out string json)
        {
            try
            {
                if (File.Exists(path))
                {
                    json = File.ReadAllText(path);
                    return true;
                }
            }
            catch (IOException)
            {
                // The caller treats unreadable content as unavailable and can recover from backup.
            }
            catch (UnauthorizedAccessException)
            {
                // The caller returns a failed load result with no destructive recovery.
            }

            json = null;
            return false;
        }

        private void ReplaceWithPortableFallback()
        {
            File.Copy(_primaryPath, _backupPath, true);
            File.Delete(_primaryPath);
            File.Move(_temporaryPath, _primaryPath);
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
