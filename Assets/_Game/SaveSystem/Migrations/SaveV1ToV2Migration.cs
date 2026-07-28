using System;
using UnityEngine;

namespace Avoidance.SaveSystem.Migrations
{
    public sealed class SaveV1ToV2Migration : ISaveMigration
    {
        public int FromVersion => 1;
        public int ToVersion => 2;

        public string Migrate(string sourceJson)
        {
            var versionOne = JsonUtility.FromJson<SaveV1>(sourceJson)
                ?? throw new InvalidOperationException("Could not deserialize SaveV1.");
            var versionTwo = new SaveDocument
            {
                schemaVersion = 2,
                gameVersion = string.IsNullOrWhiteSpace(versionOne.gameVersion)
                    ? "unknown"
                    : versionOne.gameVersion
            };
            versionTwo.settings.masterVolume = Mathf.Clamp01(versionOne.masterVolume);
            return JsonUtility.ToJson(versionTwo);
        }

        [Serializable]
        private sealed class SaveV1
        {
            public int schemaVersion;
            public string gameVersion;
            public float masterVolume = 1f;
        }
    }
}
