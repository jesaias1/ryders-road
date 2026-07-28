using System.Collections.Generic;
using Avoidance.SaveSystem;
using Avoidance.SaveSystem.Migrations;
using NUnit.Framework;

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
            Assert.That(service.Current.schemaVersion, Is.EqualTo(2));
            Assert.That(service.Current.settings.masterVolume, Is.EqualTo(0.25f).Within(0.001f));
        }

        private static JsonSaveService CreateService(MemorySaveFileStore store)
        {
            var service = new JsonSaveService(store, "0.0.1");
            service.RegisterMigration(new SaveV1ToV2Migration());
            return service;
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
