using System.Linq;
using Avoidance.Gameplay.Audio;
using Avoidance.Gameplay.Visuals;
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
        [Test] public void WindwardHasCommittedGapsAndSharedSkipGeometry()
        {
            var module=Resources.Load<ModuleDefinition>("Modules/Module_005_Windward");
            var route=module.Blocks.ToList();
            route.InsertRange(route.FindIndex(b=>b.StableId=="m05.arc.03")+1,
                module.CrumblingBlocks.Select(b=>new ModuleBlockDefinition(b.StableId,b.Pose.Position,b.Size,ModuleMaterialRole.Crumbling)));
            Assert.That(module.ContentVersion,Is.EqualTo(6));
            Assert.That(route.Count,Is.EqualTo(20));
            Assert.That(route.Any(b=>b.StableId.Contains("skill")),Is.False,"Same-route skips, no separate chord");
            for(int i=0;i<route.Count-1;i++)
            {
                var a=route[i];var b=route[i+1];
                float dx=Mathf.Max(0,Mathf.Abs(a.Pose.Position.x-b.Pose.Position.x)-(a.Size.x+b.Size.x)*.5f);
                float dz=Mathf.Max(0,Mathf.Abs(a.Pose.Position.z-b.Pose.Position.z)-(a.Size.z+b.Size.z)*.5f);
                Assert.That(new Vector2(dx,dz).magnitude,Is.GreaterThanOrEqualTo(1.5f),a.StableId+" must not become a walkable seam");
            }
            Assert.That(route.Count(b=>b.Size.x*b.Size.z>=100),Is.EqualTo(5),"Large surfaces are sparse anchors");
            foreach(var endpoints in new[]{("arrival","west.restore"),("west.restore","east.restore"),("east.restore","lens.restore"),("lens.restore","patch.base")})
            {
                int first=route.FindIndex(b=>b.StableId=="m05."+endpoints.Item1),last=route.FindIndex(b=>b.StableId=="m05."+endpoints.Item2);
                for(int i=first+1;i<last;i++)
                {
                    var incoming=Vector3.ProjectOnPlane(route[i].Pose.Position-route[i-1].Pose.Position,Vector3.up);
                    var outgoing=Vector3.ProjectOnPlane(route[i+1].Pose.Position-route[i].Pose.Position,Vector3.up);
                    Assert.That(Vector3.Angle(incoming,outgoing),Is.LessThan(15),"Coherent sequence heading at "+route[i].StableId);
                }
            }
            Assert.That(module.CrumblingBlocks.Count,Is.EqualTo(2));
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
