using Avoidance.SaveSystem;
using NUnit.Framework;
using UnityEngine;
namespace Avoidance.Tests.EditMode
{
    public sealed class VersionedBestTests
    {
        [Test] public void LegacyRecordsAndUnlocksSurviveNewSlowerRouteAndReload()
        {
            var doc=JsonUtility.FromJson<SaveDocument>("{\"schemaVersion\":4,\"progression\":{\"modules\":[{\"moduleId\":\"module.004.solar-foundry\",\"completed\":true,\"bestTimeSeconds\":28.421,\"highestRank\":\"Diamond\",\"moduleContentVersion\":2}]}}");
            var p=doc.progression;
            Assert.That(ModuleProgressionData.GetVersionedBest(p,"module.004.solar-foundry",3,1,2),Is.Null);
            bool Record(double seconds,int content=3,int movement=1,int threshold=2,bool valid=true)
                => ModuleProgressionData.RecordVersionedCompletion(p,"module.004.solar-foundry",seconds,valid?"Gold":"None",100,null,valid,content,movement,threshold,"Uncalibrated",valid?"ValidUnassisted":"InvalidDevelopment","2026-09-09");
            Assert.That(Record(44),Is.True);Assert.That(Record(45),Is.False);Assert.That(Record(42),Is.True);
            Assert.That(Record(1,valid:false),Is.False);Assert.That(Record(52,threshold:3),Is.True);Assert.That(Record(56,movement:3),Is.True);
            Assert.That(Record(20,content:4),Is.True);
            Assert.That(p.historicalRecords[0].bestTimeSeconds,Is.EqualTo(28.421));
            Assert.That(p.historicalRecords[0].moduleContentVersion,Is.EqualTo(2));
            var reloaded=JsonUtility.FromJson<SaveDocument>(JsonUtility.ToJson(doc)).progression;
            Assert.That(ModuleProgressionData.GetRecord(reloaded,"module.004.solar-foundry").bestTimeSeconds,Is.EqualTo(20));
            Assert.That(ModuleProgressionData.GetVersionedBest(reloaded,"module.004.solar-foundry",3,1,2).bestTimeSeconds,Is.EqualTo(42));
            Assert.That(ModuleProgressionData.GetVersionedBest(reloaded,"module.004.solar-foundry",3,1,3).bestTimeSeconds,Is.EqualTo(52));
            Assert.That(ModuleProgressionData.IsUnlocked(reloaded,new[]{"module.004.solar-foundry","module.005.foundry-pulse"},"module.005.foundry-pulse"),Is.True);
        }
    }
}
