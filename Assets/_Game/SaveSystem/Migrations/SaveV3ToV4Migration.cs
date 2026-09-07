using System;
using UnityEngine;

namespace Avoidance.SaveSystem.Migrations
{
    public sealed class SaveV3ToV4Migration : ISaveMigration
    {
        public int FromVersion => 3;
        public int ToVersion => 4;

        public string Migrate(string sourceJson)
        {
            var versionThree = JsonUtility.FromJson<SaveDocument>(sourceJson)
                ?? throw new InvalidOperationException("Could not deserialize SaveV3.");
            var versionFour = new SaveDocument
            {
                schemaVersion = 4,
                gameVersion = string.IsNullOrWhiteSpace(versionThree.gameVersion)
                    ? "unknown"
                    : versionThree.gameVersion,
                settings = versionThree.settings ?? new Avoidance.Core.Services.GameSettings(),
                progression = versionThree.progression ?? new ProgressionData(),
                economy = new EconomyData(),
                inventory = new InventoryData()
            };

            ModuleProgressionData.EnsureArrays(versionFour.progression);
            EconomyProgressionData.EnsureArrays(versionFour.economy);
            InventoryProgressionData.EnsureArrays(versionFour.inventory);
            return JsonUtility.ToJson(versionFour);
        }
    }
}
