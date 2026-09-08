using System;
using System.Linq;
using Avoidance.EditorTools;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Worlds;
using Avoidance.SaveSystem;
using NUnit.Framework;
using UnityEngine;

namespace Avoidance.Tests.EditMode
{
    public sealed class Mountain094Tests
    {
        private static ModuleDefinition Module => Resources.Load<ModuleDefinition>("Modules/Module_002_MovingParts");
        [Test] public void StartupPreservesMountainAndAcceptedNeighbours()
        {
            var modules=Resources.LoadAll<ModuleDefinition>("Modules").Where(m=>ModuleSelectionState.IsCampaignModule(m.StableModuleId)).ToArray();
            var routes=modules.Select(JsonUtility.ToJson).ToArray();var worlds=modules.Select(m=>JsonUtility.ToJson(m.EnvironmentBiomeProfile)).ToArray();
            FoundationProjectSetup.Apply();
            Assert.That(modules.Select(JsonUtility.ToJson).ToArray(),Is.EqualTo(routes));
            Assert.That(modules.Select(m=>JsonUtility.ToJson(m.EnvironmentBiomeProfile)).ToArray(),Is.EqualTo(worlds));
            Assert.That(Module.ContentVersion,Is.EqualTo(3));
        }
        [Test] public void MountainHasClimbFivePlacesTrueCollisionAndBoundedCost()
        {
            var m=Module;Assert.That(m.Decorations,Is.Empty);Assert.That(m.DisableAutomaticDistantFragments,Is.True);
            Assert.That(m.PatchBlock.Pose.Position.y-m.StartPoint.Position.y,Is.GreaterThan(12));
            Assert.That(m.RestorePoints.Count,Is.EqualTo(2));Assert.That(m.OptionalShortcuts.Count,Is.EqualTo(2));Assert.That(m.MovingBlocks.Count,Is.EqualTo(2));
            Assert.That(m.EnvironmentBiomeProfile.CombineStaticGeometry,Is.True);
            Assert.That(m.RestorePoints.All(r=>m.Blocks.Any(b=>b.StableId==r.SupportBlockStableId)),Is.True);
            Assert.That(m.Blocks.Any(b=>b.StableId==m.PatchBlock.SupportBlockStableId),Is.True);
            var places=m.EnvironmentBiomeProfile.WorldObjects;
            foreach(var suffix in new[]{"arrival.cliff","pass.fractured-abutment","garden.waterfall-shrine","ridge.old-lift","summit.stone-crown","landmark.split-peak"})
                Assert.That(places.Any(p=>p.StableId.EndsWith(suffix)),Is.True,suffix);
            foreach(var p in places)
            {
                var instance=UnityEngine.Object.Instantiate(p.Prefab,p.Position,p.Rotation);instance.transform.localScale=p.Scale;
                try
                {
                    if(p.HasPlayableArchitecture)
                    {
                        foreach(var mesh in instance.GetComponentsInChildren<MeshFilter>())
                            if(!mesh.name.StartsWith("Leaf"))Assert.That(mesh.GetComponent<AuthoredSurface>()?.HasMatchingCollision,Is.True,p.StableId+"/"+mesh.name);
                    }
                    else
                    {
                        Assert.That(instance.GetComponentsInChildren<Collider>(),Is.Empty,p.StableId);
                        if(p.DepthBand==BiomeDepthBand.LowerAbyss)continue;
                        var renderers=instance.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
                        foreach(var b in m.Blocks){var delta=bounds.ClosestPoint(b.Pose.Position)-b.Pose.Position;delta.y=0;Assert.That(delta.magnitude,Is.GreaterThan(8),p.StableId+" near "+b.StableId);}
                    }
                }
                finally{UnityEngine.Object.DestroyImmediate(instance);}
            }
            var triangles=places.Sum(p=>p.Prefab.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3));
            var count=places.Sum(p=>p.Prefab.GetComponentsInChildren<Renderer>().Length);
            Assert.That(triangles,Is.LessThan(180000));Assert.That(count,Is.LessThan(190));
            System.IO.Directory.CreateDirectory("Logs/Phase094VisualQA");System.IO.File.WriteAllText("Logs/Phase094VisualQA/world-budget.json",$"{{\"triangles\":{triangles},\"renderersBeforeBatching\":{count},\"placements\":{places.Count}}}");
        }
        [Test] public void FreshBronzeSequentialProgressionAndExplicitCampaignEnd()
        {
            var p=new ProgressionData();var ids=ModuleSelectionState.GetCampaignModuleIds();
            Assert.That(ids,Is.EqualTo(new[]{"module.001.first-steps","module.002.moving-parts","module.003.flow-error","module.004.solar-foundry", "module.005.foundry-pulse"}));
            Assert.That(ids.Select(id=>ModuleProgressionData.IsUnlocked(p,ids,id)),Is.EqualTo(new[]{true,false,false,false,false}));
            Assert.That(ModuleProgressionData.GetContinueModuleId(p,ids),Is.EqualTo(ids[0]));
            for(int i=0;i<ids.Length;i++)
            {
                ModuleProgressionData.RecordCompletion(p,ids[i],999,"Bronze",1,Array.Empty<ModuleSplitRecord>(),true,2,1,1,"Uncalibrated","ValidUnassisted","2026-09-06T00:00:00Z");
                Assert.That(ModuleProgressionData.GetContinueModuleId(p,ids),Is.EqualTo(i+1<ids.Length?ids[i+1]:null));
                for(int j=0;j<=i;j++)Assert.That(ModuleProgressionData.IsUnlocked(p,ids,ids[j]),Is.True);
            }
            Assert.That(ModuleSelectionState.GetNextCampaignModuleId(ids[ids.Length-1]),Is.Null);
            Assert.That(ModuleProgressionData.IsUnlocked(p,ids,ModuleSelectionState.SpiralModuleId),Is.False);
            Assert.That(ModuleProgressionData.IsUnlocked(p,ids,"module.999.unknown"),Is.False);
        }
        [Test] public void LegacyEarnedProgressSurvivesLaterPracticeAndOutOfOrderRecords()
        {
            var ids=ModuleSelectionState.GetCampaignModuleIds();var p=new ProgressionData();
            var r=ModuleProgressionData.GetOrCreateRecord(p,ids[1]);r.completed=true;r.runValidity="InvalidDevelopment";
            Assert.That(ModuleProgressionData.IsUnlocked(p,ids,ids[1]),Is.False,"Practice-only records do not grant entry");
            r.bestRank="Bronze";r.bestTimeSeconds=88;r.moduleContentVersion=1;r.highestRank="Silver";
            var json=JsonUtility.ToJson(p);
            Assert.That(ids.Take(3).All(id=>ModuleProgressionData.IsUnlocked(p,ids,id)),Is.True);
            Assert.That(ModuleProgressionData.IsUnlocked(p,ids,ids[3]),Is.False);
            Assert.That(ModuleProgressionData.GetContinueModuleId(p,ids),Is.EqualTo(ids[2]));
            Assert.That(JsonUtility.ToJson(p),Is.EqualTo(json),"Derivation must not rewrite PBs, ranks or content metadata");
            var fresh=new ProgressionData{exceptionalUnlocks=new[]{ids[2]}};
            Assert.That(ids.Take(3).All(id=>ModuleProgressionData.IsUnlocked(fresh,ids,id)),Is.True);
            Assert.That(ModuleProgressionData.IsUnlocked(fresh,ids,ids[3]),Is.False);
        }
        [Test] public void PracticeNeverOpensNextRoadAndAnAttemptIsNotCompletion()
        {
            var ids=ModuleSelectionState.GetCampaignModuleIds();var p=new ProgressionData();
            ModuleProgressionData.RecordAttempt(p,ids[0]);Assert.That(ModuleProgressionData.IsUnlocked(p,ids,ids[1]),Is.False);
            ModuleProgressionData.RecordCompletion(p,ids[0],2,"Diamond",1,Array.Empty<ModuleSplitRecord>(),false,2,1,1,"Uncalibrated","InvalidDevelopment","2026-09-06T00:00:00Z");
            Assert.That(ModuleProgressionData.IsUnlocked(p,ids,ids[1]),Is.False);
        }
    }
}
