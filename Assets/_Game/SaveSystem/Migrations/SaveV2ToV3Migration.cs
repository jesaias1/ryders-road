using System;
using UnityEngine;

namespace Avoidance.SaveSystem.Migrations
{
    public sealed class SaveV2ToV3Migration : ISaveMigration
    {
        public int FromVersion => 2;
        public int ToVersion => 3;

        public string Migrate(string sourceJson)
        {
            var versionTwo = JsonUtility.FromJson<SaveV2>(sourceJson)
                ?? throw new InvalidOperationException("Could not deserialize SaveV2.");
            var versionThree = new SaveDocument
            {
                schemaVersion = 3,
                gameVersion = string.IsNullOrWhiteSpace(versionTwo.gameVersion)
                    ? "unknown"
                    : versionTwo.gameVersion,
                settings = versionTwo.settings ?? new Avoidance.Core.Services.GameSettings(),
                progression = new ProgressionData()
            };
            ModuleProgressionData.EnsureArrays(versionThree.progression);
            return JsonUtility.ToJson(versionThree);
        }

        [Serializable]
        private sealed class SaveV2
        {
            public int schemaVersion;
            public string gameVersion;
            public Avoidance.Core.Services.GameSettings settings;
        }
    }
}
