using System.Linq;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Visuals;
using NUnit.Framework;
using UnityEngine;

namespace Avoidance.Tests.EditMode
{
    public sealed class World170Tests
    {
        [Test] public void StartupPreservesAuthoredSpiralGeometryAndPresentation()
        {
            var module=Resources.Load<ModuleDefinition>("Modules/Module_004_TheSpiral");
            var before=JsonUtility.ToJson(module);
            Avoidance.EditorTools.FoundationProjectSetup.Apply();
            Assert.That(JsonUtility.ToJson(module),Is.EqualTo(before));
            Assert.That(module.ContentVersion,Is.EqualTo(3));
        }
        [Test] public void AllWorldsUseCohesiveLightingAndColliderFreeSkins()
        {
            var names=new[]{"Module_001_FirstSteps","Module_002_MovingParts","Module_003_FlowError","Module_004_SolarFoundry","Module_005_Windward","Module_004_TheSpiral"};
            var versions=new[]{4,5,9,5,6,3};
            for(int i=0;i<names.Length;i++)
            {
                var module=Resources.Load<ModuleDefinition>("Modules/"+names[i]);
                Assert.That(module.ContentVersion,Is.EqualTo(versions[i]));
                Assert.That(module.EnvironmentProfile.CohesiveLighting,Is.True,names[i]);
                Assert.That(module.EnvironmentProfile.ShadowStrength,Is.InRange(.6f,.85f));
                Assert.That(module.EnvironmentProfile.BloomIntensity,Is.InRange(.03f,.15f));
                var library=module.VisualProfile.PrefabLibrary;
                Assert.That(library,Is.Not.Null,names[i]);
                var deck=library.PrefabFor(ModuleMaterialRole.Normal);
                Assert.That(deck.GetComponentsInChildren<Collider>(true),Is.Empty,names[i]);
                Assert.That(deck.GetComponentsInChildren<MeshFilter>().Sum(m=>m.sharedMesh.triangles.Length/3),Is.LessThan(1000),names[i]);
            }
        }
        [Test] public void FoundryAddsOptionalExistingMechanicsWithoutReplacingSafeCourse()
        {
            var module=Resources.Load<ModuleDefinition>("Modules/Module_004_SolarFoundry");
            Assert.That(module.BoostBlocks.Any(b=>b.StableId=="boost.m04.mastery.cooling-vent"),Is.True);
            Assert.That(module.SurfSurfaces.Any(b=>b.StableId=="surf.m04.mastery.cooling-spillway"),Is.True);
            Assert.That(module.CrumblingBlocks.Count,Is.EqualTo(2));Assert.That(module.MovingBlocks.Count,Is.EqualTo(1));
            Assert.That(module.Blocks.Any(b=>b.StableId=="m04.cooling.safe"),Is.True);
        }
    }
}
