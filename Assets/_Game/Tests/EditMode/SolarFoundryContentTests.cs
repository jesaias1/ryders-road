using System.Linq;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Worlds;
using NUnit.Framework;
using UnityEngine;
using System.IO;
using Avoidance.EditorTools;

namespace Avoidance.Tests.EditMode
{
    public sealed class SolarFoundryContentTests
    {
        [Test] public void StartupPreservesFoundryAndLegacySpiralIdentity()
        {
            const string path="Assets/_Game/Levels/Resources/Modules/Module_004_SolarFoundry.asset";
            var before=File.ReadAllBytes(path);
            FoundationProjectSetup.Apply();
            Assert.That(File.ReadAllBytes(path),Is.EqualTo(before));
            Assert.That(ModuleSelectionState.GetNextCampaignModuleId("module.003.flow-error"),Is.EqualTo("module.004.solar-foundry"));
            Assert.That(ModuleSelectionState.IsCampaignModule(ModuleSelectionState.SpiralModuleId),Is.False);
        }
        [Test] public void FoundryHasCompleteBoundedContentAndExplicitCollision()
        {
            var module=Resources.Load<ModuleDefinition>("Modules/Module_004_SolarFoundry");
            Assert.That(ModuleDefinitionValidator.Validate(module),Is.Empty);
            Assert.That(module.RestorePoints.Count,Is.EqualTo(3));
            Assert.That(module.MovingBlocks.Count,Is.EqualTo(1));Assert.That(module.CrumblingBlocks.Count,Is.EqualTo(2));
            Assert.That(module.OptionalShortcuts.Count,Is.EqualTo(1));
            Assert.That(module.StableModuleId,Is.Not.EqualTo(ModuleSelectionState.SpiralModuleId));
            var world=module.EnvironmentBiomeProfile.WorldObjects;
            Assert.That(world.Select(x=>x.StableId).Distinct().Count(),Is.EqualTo(world.Count));
            int triangles=0,renderers=0;
            foreach(var place in world)
            {
                var meshes=place.Prefab.GetComponentsInChildren<MeshFilter>();renderers+=meshes.Length;
                foreach(var mesh in meshes)
                {
                    triangles+=mesh.sharedMesh.triangles.Length/3;
                    if(place.HasPlayableArchitecture)Assert.That(mesh.GetComponent<AuthoredSurface>()?.HasMatchingCollision,Is.True,place.StableId);
                    else Assert.That(mesh.GetComponent<Collider>(),Is.Null);
                }
            }
            Assert.That(triangles,Is.LessThan(90000));Assert.That(renderers,Is.LessThan(70));
            Debug.Log($"Solar Foundry world: {triangles} triangles, {renderers} renderers before combining.");
        }
    }
}
