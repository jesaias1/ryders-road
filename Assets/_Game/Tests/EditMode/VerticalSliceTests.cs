using System.Linq;
using Avoidance.Gameplay.Audio;
using Avoidance.Gameplay.Levels;
using NUnit.Framework;
using UnityEngine;

namespace Avoidance.Tests.EditMode
{
    public sealed class VerticalSliceTests
    {
        [Test] public void WindwardHasRealGapsAndShorterOptionalChord()
        {
            var module=Resources.Load<ModuleDefinition>("Modules/Module_005_Windward");
            var route=module.Blocks.Where(b=>!b.StableId.Contains("skill")).ToArray();
            for(int i=0;i<route.Length-1;i++)
            {
                var a=route[i];var b=route[i+1];var d=b.Pose.Position-a.Pose.Position;
                float x=Mathf.Max(0,Mathf.Abs(d.x)-(a.Size.x+b.Size.x)*.5f);
                float z=Mathf.Max(0,Mathf.Abs(d.z)-(a.Size.z+b.Size.z)*.5f);
                Assert.That(new Vector2(x,z).magnitude,Is.GreaterThan(1),a.StableId+" -> "+b.StableId);
                Assert.That(Mathf.Min(b.Size.x,b.Size.z),Is.GreaterThanOrEqualTo(3.2f),"Mobile landing width");
            }
            var arc=module.Blocks.Where(b=>b.StableId.StartsWith("m05.arc.")).ToArray();
            var chord=module.Blocks.Where(b=>b.StableId.Contains("skill")).ToArray();
            Assert.That(arc.Length,Is.EqualTo(6));
            Assert.That(chord.Length,Is.EqualTo(3));
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
