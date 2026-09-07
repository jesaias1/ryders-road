using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Avoidance.SaveSystem
{
    public static class SavePathMigrationUtility
    {
        public const string LegacyProductName = "RYDERS BLOCK";
        public const string CurrentProductName = "Ryder's Road";
        public const string SaveDirectoryName = "Saves";
        public const string PrimaryFileName = "save.json";
        public const string BackupFileName = "save.json.bak";

        public static string ResolveCurrentSaveDirectory()
        {
            return Path.Combine(Application.persistentDataPath, SaveDirectoryName);
        }

        public static IReadOnlyList<string> CandidateLegacySaveDirectories(
            string currentSaveDirectory)
        {
            if (string.IsNullOrWhiteSpace(currentSaveDirectory))
            {
                return Array.Empty<string>();
            }

            var candidates = new List<string>();
            var currentDataDirectory = Directory.GetParent(currentSaveDirectory)?.FullName;
            var companyDirectory = currentDataDirectory == null
                ? null
                : Directory.GetParent(currentDataDirectory)?.FullName;
            if (!string.IsNullOrWhiteSpace(companyDirectory))
            {
                candidates.Add(Path.Combine(companyDirectory, LegacyProductName, SaveDirectoryName));
            }

            return candidates
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(path => !PathsEqual(path, currentSaveDirectory))
                .ToArray();
        }

        public static bool CopyLegacySaveIfCurrentMissing(
            string currentSaveDirectory,
            IEnumerable<string> legacySaveDirectories)
        {
            if (string.IsNullOrWhiteSpace(currentSaveDirectory)
                || legacySaveDirectories == null)
            {
                return false;
            }

            var currentPrimary = Path.Combine(currentSaveDirectory, PrimaryFileName);
            if (File.Exists(currentPrimary))
            {
                return false;
            }

            foreach (var legacyDirectory in legacySaveDirectories)
            {
                if (string.IsNullOrWhiteSpace(legacyDirectory)
                    || PathsEqual(legacyDirectory, currentSaveDirectory))
                {
                    continue;
                }

                var legacyPrimary = Path.Combine(legacyDirectory, PrimaryFileName);
                if (!File.Exists(legacyPrimary))
                {
                    continue;
                }

                Directory.CreateDirectory(currentSaveDirectory);
                File.Copy(legacyPrimary, currentPrimary, overwrite: false);
                CopyBackupIfPresent(legacyDirectory, currentSaveDirectory);
                Debug.Log(
                    $"Copied legacy save data from {legacyDirectory} for {CurrentProductName}.");
                return true;
            }

            return false;
        }

        private static void CopyBackupIfPresent(
            string legacyDirectory,
            string currentSaveDirectory)
        {
            var legacyBackup = Path.Combine(legacyDirectory, BackupFileName);
            var currentBackup = Path.Combine(currentSaveDirectory, BackupFileName);
            if (File.Exists(legacyBackup) && !File.Exists(currentBackup))
            {
                File.Copy(legacyBackup, currentBackup, overwrite: false);
            }
        }

        private static bool PathsEqual(string a, string b)
        {
            return string.Equals(
                Path.GetFullPath(a).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                Path.GetFullPath(b).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
