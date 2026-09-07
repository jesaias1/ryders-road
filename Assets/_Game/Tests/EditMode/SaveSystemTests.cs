using System.Collections.Generic;
using System.IO;
using Avoidance.SaveSystem;
using Avoidance.SaveSystem.Migrations;
using NUnit.Framework;
using UnityEngine;

namespace Avoidance.Tests.EditMode
{
    public sealed class SaveSystemTests
    {
        [Test]
        public void Initialize_CreatesNewSave_WhenNoSaveExists()
        {
            var store = new MemorySaveFileStore();
            var service = CreateService(store);

            var result = service.Initialize();

            Assert.That(result.Success, Is.True);
            Assert.That(result.Source, Is.EqualTo(SaveLoadSource.New));
            Assert.That(service.Current.schemaVersion, Is.EqualTo(SaveSchema.CurrentVersion));
            Assert.That(store.PrimaryExists, Is.True);
            Assert.That(service.Current.economy, Is.Not.Null);
            Assert.That(service.Current.economy.balances, Is.Empty);
            Assert.That(service.Current.inventory, Is.Not.Null);
            Assert.That(service.Current.inventory.items, Is.Empty);
            Assert.That(service.Current.inventory.unlocks, Is.Empty);
            Assert.That(service.Current.inventory.equippedCosmetics, Is.Empty);
        }

        [Test]
        public void Reload_LoadsExistingSave()
        {
            var store = new MemorySaveFileStore();
            var writer = CreateService(store);
            writer.Initialize();
            writer.Current.settings.masterVolume = 0.35f;
            writer.Save();

            var reader = CreateService(store);
            var result = reader.Reload();

            Assert.That(result.Source, Is.EqualTo(SaveLoadSource.Primary));
            Assert.That(reader.Current.settings.masterVolume, Is.EqualTo(0.35f).Within(0.001f));
        }

        [Test]
        public void Reload_RecoversBackup_WhenPrimaryIsCorrupt()
        {
            var store = new MemorySaveFileStore();
            var service = CreateService(store);
            service.Initialize();
            service.Current.settings.masterVolume = 0.42f;
            service.Save();
            service.Current.settings.masterVolume = 0.73f;
            service.Save();
            store.Primary = "{corrupt";

            var recovered = CreateService(store);
            var result = recovered.Reload();

            Assert.That(result.Success, Is.True);
            Assert.That(result.Source, Is.EqualTo(SaveLoadSource.Backup));
            Assert.That(recovered.Current.settings.masterVolume, Is.EqualTo(0.42f).Within(0.001f));
        }

        [Test]
        public void Reload_MigratesExampleV1ToV2()
        {
            var store = new MemorySaveFileStore
            {
                Primary = "{\"schemaVersion\":1,\"gameVersion\":\"0.0.0\",\"masterVolume\":0.25}"
            };
            var service = CreateService(store);

            var result = service.Reload();

            Assert.That(result.Success, Is.True);
            Assert.That(service.Current.schemaVersion, Is.EqualTo(SaveSchema.CurrentVersion));
            Assert.That(service.Current.settings.masterVolume, Is.EqualTo(0.25f).Within(0.001f));
            Assert.That(service.Current.progression.modules, Is.Empty);
            Assert.That(service.Current.economy.balances, Is.Empty);
            Assert.That(service.Current.inventory.items, Is.Empty);
        }

        [Test]
        public void Reload_MigratesV2ToV3ProgressionShape()
        {
            var store = new MemorySaveFileStore
            {
                Primary = "{\"schemaVersion\":2,\"gameVersion\":\"0.2.0\",\"settings\":{\"masterVolume\":0.8,\"lookSensitivity\":0.4,\"diagnosticsVisible\":true},\"progression\":{}}"
            };
            var service = CreateService(store);

            var result = service.Reload();

            Assert.That(result.Success, Is.True);
            Assert.That(service.Current.schemaVersion, Is.EqualTo(SaveSchema.CurrentVersion));
            Assert.That(service.Current.progression, Is.Not.Null);
            Assert.That(service.Current.progression.modules, Is.Not.Null);
            Assert.That(service.Current.progression.exceptionalUnlocks, Is.Not.Null);
            Assert.That(service.Current.progression.claimedRewardIds, Is.Not.Null);
            Assert.That(service.Current.economy.balances, Is.Empty);
            Assert.That(service.Current.inventory.equippedCosmetics, Is.Empty);
        }

        [Test]
        public void Reload_MigratesV3ToV4EconomyInventoryShape()
        {
            var store = new MemorySaveFileStore
            {
                Primary = "{\"schemaVersion\":3,\"gameVersion\":\"0.3.6\",\"settings\":{\"masterVolume\":0.65,\"lookSensitivity\":0.7,\"diagnosticsVisible\":true},\"progression\":{\"modules\":[{\"moduleId\":\"module.001.first-steps\",\"completed\":true,\"bestTimeSeconds\":38.421,\"bestRank\":\"Silver\",\"bestScore\":99961579,\"completionCount\":1,\"attemptCount\":3,\"bestSplits\":[{\"checkpointId\":\"restore.module-001.midpoint\",\"seconds\":18.25}],\"latestCompletionTimeSeconds\":38.421,\"latestRank\":\"Silver\",\"highestRank\":\"Silver\",\"firstCompletionUtc\":\"2026-08-16T12:00:00Z\",\"lastCompletionUtc\":\"2026-08-16T12:04:00Z\",\"moduleContentVersion\":1,\"movementCompatibilityVersion\":1,\"rankThresholdVersion\":1,\"rankCalibrationState\":\"Uncalibrated\",\"runValidity\":\"ValidUnassisted\"}],\"exceptionalUnlocks\":[\"module.004.the-spiral\"]}}"
            };
            var service = CreateService(store);

            var result = service.Reload();

            Assert.That(result.Success, Is.True);
            Assert.That(service.Current.schemaVersion, Is.EqualTo(SaveSchema.CurrentVersion));
            Assert.That(service.Current.settings.masterVolume, Is.EqualTo(0.65f).Within(0.001f));
            Assert.That(service.Current.progression.modules, Has.Length.EqualTo(1));
            Assert.That(service.Current.progression.modules[0].moduleId, Is.EqualTo("module.001.first-steps"));
            Assert.That(service.Current.progression.modules[0].bestSplits, Has.Length.EqualTo(1));
            Assert.That(service.Current.progression.exceptionalUnlocks, Has.Length.EqualTo(1));
            Assert.That(service.Current.progression.exceptionalUnlocks[0], Is.EqualTo("module.004.the-spiral"));
            Assert.That(service.Current.progression.claimedRewardIds, Is.Empty);
            Assert.That(service.Current.economy.balances, Is.Empty);
            Assert.That(service.Current.economy.lootPackagesOpened, Is.Zero);
            Assert.That(service.Current.inventory.items, Is.Empty);
            Assert.That(service.Current.inventory.unlocks, Is.Empty);
            Assert.That(service.Current.inventory.equippedCosmetics, Is.Empty);
        }

        [Test]
        public void EconomyProgression_AddsAndSpendsStableCurrency()
        {
            var economy = new EconomyData();

            var afterGrant = EconomyProgressionData.AddBalance(economy, "currency.patch-shards", 125);
            var spent = EconomyProgressionData.TrySpend(economy, "currency.patch-shards", 40);
            var failedSpend = EconomyProgressionData.TrySpend(economy, "currency.patch-shards", 200);

            Assert.That(afterGrant, Is.EqualTo(125));
            Assert.That(spent, Is.True);
            Assert.That(failedSpend, Is.False);
            Assert.That(EconomyProgressionData.GetBalance(economy, "currency.patch-shards"), Is.EqualTo(85));
            Assert.That(economy.balances, Has.Length.EqualTo(1));
        }

        [Test]
        public void InventoryProgression_StacksItemsAndPreservesFirstAcquiredTime()
        {
            var inventory = new InventoryData();

            InventoryProgressionData.GrantItem(
                inventory,
                "loot.package.starter",
                1,
                "2026-08-16T12:00:00Z",
                "reward.module.001.bronze");
            var item = InventoryProgressionData.GrantItem(
                inventory,
                "loot.package.starter",
                2,
                "2026-08-16T13:00:00Z",
                "reward.module.002.bronze");

            Assert.That(item.quantity, Is.EqualTo(3));
            Assert.That(item.firstAcquiredUtc, Is.EqualTo("2026-08-16T12:00:00Z"));
            Assert.That(item.lastAcquiredUtc, Is.EqualTo("2026-08-16T13:00:00Z"));
            Assert.That(item.sourceId, Is.EqualTo("reward.module.002.bronze"));
            Assert.That(InventoryProgressionData.GetQuantity(inventory, "loot.package.starter"), Is.EqualTo(3));
            Assert.That(inventory.items, Has.Length.EqualTo(1));
        }

        [Test]
        public void InventoryProgression_EquipsOnlyUnlockedCosmetics()
        {
            var inventory = new InventoryData();

            var lockedEquip = InventoryProgressionData.TryEquipCosmetic(
                inventory,
                "hands",
                "skin.ryder.neon");
            InventoryProgressionData.SetUnlock(
                inventory,
                "skin.ryder.neon",
                "Skin",
                true,
                "2026-08-16T14:00:00Z",
                "loot.package.starter");
            var unlockedEquip = InventoryProgressionData.TryEquipCosmetic(
                inventory,
                "hands",
                "skin.ryder.neon");

            Assert.That(lockedEquip, Is.False);
            Assert.That(unlockedEquip, Is.True);
            Assert.That(InventoryProgressionData.HasUnlock(inventory, "skin.ryder.neon"), Is.True);
            Assert.That(InventoryProgressionData.GetEquippedCosmetic(inventory, "hands"), Is.EqualTo("skin.ryder.neon"));
            Assert.That(inventory.unlocks, Has.Length.EqualTo(1));
            Assert.That(inventory.equippedCosmetics, Has.Length.EqualTo(1));
        }

        [Test]
        public void SavePathMigration_CopiesLegacySaveWhenCurrentSaveIsMissing()
        {
            var root = CreateTemporaryMigrationRoot();
            try
            {
                var current = Path.Combine(root, SavePathMigrationUtility.CurrentProductName, "Saves");
                var legacy = Path.Combine(root, SavePathMigrationUtility.LegacyProductName, "Saves");
                Directory.CreateDirectory(legacy);
                File.WriteAllText(Path.Combine(legacy, SavePathMigrationUtility.PrimaryFileName), "{\"schemaVersion\":3}");
                File.WriteAllText(Path.Combine(legacy, SavePathMigrationUtility.BackupFileName), "{\"schemaVersion\":3,\"backup\":true}");

                var copied = SavePathMigrationUtility.CopyLegacySaveIfCurrentMissing(
                    current,
                    new[] { legacy });

                Assert.That(copied, Is.True);
                Assert.That(File.ReadAllText(Path.Combine(current, SavePathMigrationUtility.PrimaryFileName)), Does.Contain("\"schemaVersion\":3"));
                Assert.That(File.Exists(Path.Combine(current, SavePathMigrationUtility.BackupFileName)), Is.True);
            }
            finally
            {
                DeleteTemporaryMigrationRoot(root);
            }
        }

        [Test]
        public void SavePathMigration_DoesNotOverwriteCurrentSave()
        {
            var root = CreateTemporaryMigrationRoot();
            try
            {
                var current = Path.Combine(root, SavePathMigrationUtility.CurrentProductName, "Saves");
                var legacy = Path.Combine(root, SavePathMigrationUtility.LegacyProductName, "Saves");
                Directory.CreateDirectory(current);
                Directory.CreateDirectory(legacy);
                File.WriteAllText(Path.Combine(current, SavePathMigrationUtility.PrimaryFileName), "current");
                File.WriteAllText(Path.Combine(legacy, SavePathMigrationUtility.PrimaryFileName), "legacy");

                var copied = SavePathMigrationUtility.CopyLegacySaveIfCurrentMissing(
                    current,
                    new[] { legacy });

                Assert.That(copied, Is.False);
                Assert.That(File.ReadAllText(Path.Combine(current, SavePathMigrationUtility.PrimaryFileName)), Is.EqualTo("current"));
            }
            finally
            {
                DeleteTemporaryMigrationRoot(root);
            }
        }

        private static JsonSaveService CreateService(MemorySaveFileStore store)
        {
            var service = new JsonSaveService(store, "0.0.1");
            service.RegisterMigration(new SaveV1ToV2Migration());
            service.RegisterMigration(new SaveV2ToV3Migration());
            service.RegisterMigration(new SaveV3ToV4Migration());
            return service;
        }

        private static string CreateTemporaryMigrationRoot()
        {
            return Path.Combine(
                Application.temporaryCachePath,
                "RydersRoadSaveMigration_" + System.Guid.NewGuid().ToString("N"));
        }

        private static void DeleteTemporaryMigrationRoot(string root)
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }

        private sealed class MemorySaveFileStore : ISaveFileStore
        {
            public string Primary;
            public string Backup;
            public bool PrimaryExists => Primary != null;
            public bool BackupExists => Backup != null;

            public bool TryReadPrimary(out string json)
            {
                json = Primary;
                return PrimaryExists;
            }

            public bool TryReadBackup(out string json)
            {
                json = Backup;
                return BackupExists;
            }

            public void WriteAtomic(string json, bool preserveBackup = false)
            {
                if (Primary != null && !preserveBackup)
                {
                    Backup = Primary;
                }

                Primary = json;
            }

            public void DeleteAll()
            {
                Primary = null;
                Backup = null;
            }
        }
    }
}
