using System.Linq;
using Avoidance.EditorTools;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Worlds;
using NUnit.Framework;
using UnityEngine;

namespace Avoidance.Tests.EditMode
{
    public sealed class SkyCity093Tests
    {
        private static ModuleDefinition Module => Resources.Load<ModuleDefinition>("Modules/Module_001_FirstSteps");
        [Test]
        public void StartupSetupPreservesAuthoredCityAndRoute()
        {
            var module=Module;
            var route=JsonUtility.ToJson(module); var city=JsonUtility.ToJson(module.EnvironmentBiomeProfile);
            FoundationProjectSetup.Apply();
            Assert.That(JsonUtility.ToJson(module),Is.EqualTo(route));
            Assert.That(JsonUtility.ToJson(module.EnvironmentBiomeProfile),Is.EqualTo(city));
            Assert.That(module.ContentVersion,Is.EqualTo(2));
        }
        [Test]
        public void NearArchitectureIsSolidAndDistantDistrictsStayOutsideJumpReach()
        {
            var module=Module;
            foreach(var place in module.EnvironmentBiomeProfile.WorldObjects)
            {
                var instance=Object.Instantiate(place.Prefab,place.Position,place.Rotation); instance.transform.localScale=place.Scale;
                try
                {
                    if(place.DepthBand==BiomeDepthBand.NearEnvironment)
                    {
                        Assert.That(place.HasPlayableArchitecture,Is.True,place.StableId);
                        foreach(var mesh in instance.GetComponentsInChildren<MeshFilter>())
                        {
                            if(mesh.name.StartsWith("Leaf")) continue;
                            Assert.That(mesh.GetComponent<AuthoredSurface>()?.HasMatchingCollision,Is.True,place.StableId+"/"+mesh.name);
                        }
                    }
                    else
                    {
                        Assert.That(instance.GetComponentsInChildren<Collider>(),Is.Empty,place.StableId);
                        if(place.DepthBand==BiomeDepthBand.LowerAbyss) continue;
                        var renderers=instance.GetComponentsInChildren<Renderer>(); var bounds=renderers[0].bounds;
                        foreach(var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                        foreach(var block in module.Blocks)
                        {
                            var closest=bounds.ClosestPoint(block.Pose.Position);
                            var delta=closest-block.Pose.Position; delta.y=0;
                            Assert.That(delta.magnitude,Is.GreaterThan(8),place.StableId+" must not invite a false landing from "+block.StableId);
                        }
                    }
                }
                finally { Object.DestroyImmediate(instance); }
            }
        }
        [Test]
        public void IntroRouteKeepsStableContractsAndHasWelcomingLandings()
        {
            var module=Module;
            Assert.That(module.Blocks.Count,Is.EqualTo(12));
            Assert.That(module.Blocks.All(b=>b.Size.x>=2.5f),Is.True);
            Assert.That(module.RestorePoints.Single().StableId,Is.EqualTo("restore.module-001.midpoint"));
            Assert.That(module.PatchBlock.StableId,Is.EqualTo("patch.module-001"));
            Assert.That(ModuleSelectionState.GetNextCampaignModuleId(module.StableModuleId),Is.EqualTo("module.002.moving-parts"));
            Assert.That(module.EnvironmentProfile.SkyboxMaterial,Is.Not.Null);
            Assert.That(module.EnvironmentProfile.OverrideBiomeFog,Is.True);
            Assert.That(module.Decorations,Is.Empty);
            Assert.That(module.EnvironmentBiomeProfile.WorldObjects.Select(p=>p.StableId).Distinct().Count(),Is.EqualTo(module.EnvironmentBiomeProfile.WorldObjects.Count));
        }
        [Test]
        public void BakedWorldStaysWithinMobileGeometryBudget()
        {
            var placements=Module.EnvironmentBiomeProfile.WorldObjects;
            var triangles=placements.Sum(p=>p.Prefab.GetComponentsInChildren<MeshFilter>().Sum(m=>m.sharedMesh.triangles.Length/3));
            var renderers=placements.Sum(p=>p.Prefab.GetComponentsInChildren<Renderer>().Length);
            Assert.That(triangles,Is.LessThan(220000),"Instanced world triangle budget");
            Assert.That(renderers,Is.LessThan(160),"World renderer budget before batching");
        }
    }
}
