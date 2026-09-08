using System.IO;
using System.Linq;
using Avoidance.EditorTools;
using Avoidance.Gameplay.Audio;
using Avoidance.Gameplay.Levels;
using NUnit.Framework;
using UnityEngine;

namespace Avoidance.Tests.EditMode
{
    public sealed class Production110Tests
    {
        [Test] public void FifthRoadIsCompleteAndStartupPreservesIt()
        {
            var module=Resources.Load<ModuleDefinition>("Modules/Module_005_Windward");
            Assert.That(ModuleDefinitionValidator.Validate(module),Is.Empty);
            Assert.That(module.RestorePoints.Count,Is.EqualTo(3));
            Assert.That(module.Blocks.Count,Is.InRange(20,26));
            Assert.That(module.OptionalShortcuts.Count,Is.EqualTo(1));
            Assert.That(ModuleSelectionState.GetNextCampaignModuleId("module.004.solar-foundry"),Is.EqualTo(module.StableModuleId));
            Assert.That(ModuleSelectionState.GetNextCampaignModuleId(module.StableModuleId),Is.Null);
            Assert.That(ModuleSelectionState.IsCampaignModule(ModuleSelectionState.SpiralModuleId),Is.False);
            var before=File.ReadAllBytes(WindwardAuthoring.ModulePath);
            FoundationProjectSetup.Apply();Assert.That(File.ReadAllBytes(WindwardAuthoring.ModulePath),Is.EqualTo(before));
            int triangles=module.EnvironmentBiomeProfile.WorldObjects.Sum(p=>p.Prefab.GetComponentsInChildren<MeshFilter>().Sum(m=>m.sharedMesh.triangles.Length/3));
            Assert.That(triangles,Is.LessThan(35000));
        }
        [Test] public void CampaignHasDistinctTextureFreeAtmospheresAndQuietAudio()
        {
            var modules=Resources.LoadAll<ModuleDefinition>("Modules").Where(m=>ModuleSelectionState.IsCampaignModule(m.StableModuleId)).ToArray();
            Assert.That(modules.Length,Is.EqualTo(5));
            Assert.That(modules.Select(m=>m.EnvironmentProfile.SkyboxMaterial.GetColor("_Zenith")).Distinct().Count(),Is.EqualTo(5));
            foreach(var m in modules)
            {
                var e=m.EnvironmentProfile;Assert.That(e.AuthoredLighting,Is.True);Assert.That(e.OverrideBiomeFog,Is.True);
                Assert.That(e.SkyboxMaterial.shader.name,Is.EqualTo("RydersRoad/World Sky"));
                Assert.That(e.SkyboxMaterial.GetTexturePropertyNames(),Is.Empty);
                Assert.That(e.AmbienceClip,Is.Not.Null);Assert.That(e.AmbienceGain,Is.InRange(.01f,.1f));
            }
            var audio=Resources.Load<GameplayAudioProfile>("GameplayAudioProfile");
            Assert.That(audio.TryGet(GameplayAudioCue.Footstep,out var clip,out var volume),Is.True);
            Assert.That(volume,Is.LessThan(.2f));
        }
    }
}
