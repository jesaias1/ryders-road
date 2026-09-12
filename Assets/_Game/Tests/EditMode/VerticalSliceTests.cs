using System.Linq;
using Avoidance.Gameplay.Audio;
using Avoidance.Gameplay.Levels;
using NUnit.Framework;
using UnityEngine;

namespace Avoidance.Tests.EditMode
{
    public sealed class VerticalSliceTests
    {
        [Test] public void SolidSceneryRequiresExplicitTraversalOptIn()
        {
            var distant=new Avoidance.Gameplay.Worlds.BiomeWorldObject("test.distant",null,Vector3.zero,Vector3.zero,Vector3.one);
            var scenery=new Avoidance.Gameplay.Worlds.BiomeWorldObject("test.scenery",null,Vector3.zero,Vector3.zero,Vector3.one,hasPlayableArchitecture:true);
            var route=new Avoidance.Gameplay.Worlds.BiomeWorldObject("test.route",null,Vector3.zero,Vector3.zero,Vector3.one,hasPlayableArchitecture:true,traversableArchitecture:true);
            Assert.That(distant.GeometryKind,Is.EqualTo(Avoidance.Gameplay.Worlds.WorldGeometryKind.NonCollidingScenery));
            Assert.That(scenery.GeometryKind,Is.EqualTo(Avoidance.Gameplay.Worlds.WorldGeometryKind.FatalScenery));
            Assert.That(route.GeometryKind,Is.EqualTo(Avoidance.Gameplay.Worlds.WorldGeometryKind.Traversable));
        }
        [Test] public void WindwardHasRealGapsAndShorterOptionalChord()
        {
            var module=Resources.Load<ModuleDefinition>("Modules/Module_005_Windward");
            var route=module.Blocks.Where(b=>!b.StableId.Contains("skill")).ToArray();
            for(int i=0;i<route.Length-1;i++)
            {
                var b=route[i+1];
                Assert.That(Mathf.Min(b.Size.x,b.Size.z),Is.GreaterThanOrEqualTo(7f),"Benchmark recovery width");
            }
            var arc=module.Blocks.Where(b=>b.StableId.StartsWith("m05.arc.")).ToArray();
            var chord=module.Blocks.Where(b=>b.StableId.Contains("skill")).ToArray();
            Assert.That(arc.Length,Is.EqualTo(6));
            Assert.That(chord.Length,Is.EqualTo(3));
            Assert.That(module.CrumblingBlocks.Count,Is.EqualTo(2));
            var pressure=module.CrumblingBlocks;
            Assert.That(Vector3.Distance(pressure[0].Pose.Position,pressure[1].Pose.Position)
                -(pressure[0].Size.x+pressure[1].Size.x)*.5f,Is.GreaterThan(1.25f));
            var west=module.Blocks.Single(b=>b.StableId=="m05.west.restore");
            var east=module.Blocks.Single(b=>b.StableId=="m05.east.restore");
            float Length(ModuleBlockDefinition[] points)=>points.Zip(points.Skip(1),(a,b)=>Vector3.Distance(a.Pose.Position,b.Pose.Position)).Sum();
            Assert.That(Length(new[]{west}.Concat(chord).Concat(new[]{east}).ToArray()),
                Is.LessThan(Length(new[]{west}.Concat(arc).Concat(new[]{east}).ToArray())*.75f),"Direct chord saves physical distance");
            Assert.That(ModuleDefinitionValidator.Validate(module),Is.Empty);
        }
        [Test] public void SliceAudioAndLightingUseSharedProfiles()
        {
            var audio=Resources.Load<GameplayAudioProfile>("GameplayAudioProfile");
            foreach(var cue in new[]{GameplayAudioCue.Jump,GameplayAudioCue.Landing,GameplayAudioCue.Footstep,GameplayAudioCue.Restore,GameplayAudioCue.Patch,GameplayAudioCue.Boost})
            {
                Assert.That(audio.TryGet(cue,out var clip,out var gain),Is.True,cue.ToString());
                Assert.That(clip.length,Is.InRange(.03f,3));Assert.That(gain,Is.InRange(.05f,.5f));
            }
            var windward=Resources.Load<ModuleDefinition>("Modules/Module_005_Windward");
            Assert.That(windward.EnvironmentProfile.NearArchitectureShadows,Is.True);
            Assert.That(windward.EnvironmentProfile.SkyboxMaterial.shader.name,Is.EqualTo("RydersRoad/World Sky"));
        }
    }
}
