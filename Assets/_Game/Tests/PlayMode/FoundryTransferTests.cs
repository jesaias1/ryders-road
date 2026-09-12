using System.Collections;
using System.Linq;
using Avoidance.Gameplay.Blocks;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Respawn;
using Avoidance.Input;
using Avoidance.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Avoidance.Tests.PlayMode
{
    public sealed class FoundryTransferTests
    {
        private sealed class Input:IPlayerInputSource
        {
            public Vector2 Move{get;set;}public Vector2 LookDelta=>Vector2.zero;public bool JumpPressed{get;set;}
            public void ResetState(){Move=Vector2.zero;JumpPressed=false;}
        }
        [UnityTest] public IEnumerator CoolingVentLaunchAndSpillwayUseSharedMotor()
        {
            ModuleSelectionState.Select("module.004.solar-foundry",true);
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleRunner");yield return null;
            Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled=false;
            var motor=Object.FindAnyObjectByType<ParkourMotor>();var restore=motor.GetComponent<RestoreController>();
            var module=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
            var vent=module.BoostBlocks.Single(b=>b.StableId=="boost.m04.mastery.cooling-vent");
            var target=module.Blocks.Single(b=>b.StableId=="m04.cooling.safe");
            var boost=Object.FindObjectsByType<JumpBoostBlock>().Single(b=>b.name==vent.StableId);
            motor.ResetMotion(vent.Pose.Position+Vector3.up*.35f,Quaternion.identity,0);Physics.SyncTransforms();
            var input=new Input();for(int i=0;i<5;i++)motor.Simulate(input,1f/120);
            motor.ApplyLaunch(Vector3.forward*7.8f,true);
            Assert.That(boost.TryLaunch(motor,Time.time),Is.True);
            bool landed=false;
            for(int frame=0;frame<240&&!restore.IsRestorePending;frame++)
            {
                var toward=Vector3.ProjectOnPlane(target.Pose.Position-motor.transform.position,Vector3.up).normalized;
                input.Move=new Vector2(toward.x,toward.z);motor.Simulate(input,1f/120);
                if(frame>10&&motor.IsGrounded){landed=true;break;}
            }
            Assert.That(landed&&!restore.IsRestorePending,Is.True,"Boost must land on cooling court at "+motor.transform.position);
            Assert.That(Mathf.Abs(motor.transform.position.z-target.Pose.Position.z),Is.LessThan(target.Size.z*.5f+.3f));
            var ramp=module.SurfSurfaces.Single(s=>s.StableId=="surf.m04.mastery.cooling-spillway");
            var surface=Object.FindObjectsByType<SurfSurface>().Single(s=>s.name==ramp.StableId);
            Assert.That(surface.IsValidSurface(surface.transform.up),Is.True);
            restore.RestoreNow();restore.Tick(false,1);
            motor.ResetMotion(ramp.Pose.Position+Quaternion.Euler(ramp.Pose.EulerAngles)*new Vector3(0,0,-4)+Vector3.up*3,Quaternion.identity,0);
            Physics.SyncTransforms();motor.ApplyLaunch(Vector3.forward*10,true);input.Move=Vector2.up;
            float contact=0;bool jumped=false;
            for(int frame=0;frame<300&&!restore.IsRestorePending;frame++)
            {
                if(motor.IsSurfing)contact+=1f/120;
                input.JumpPressed=!jumped&&contact>.15f;
                if(input.JumpPressed)jumped=true;
                motor.Simulate(input,1f/120);
                if(jumped){Assert.That(motor.IsSurfing,Is.False,"Manual jump detaches from spillway");break;}
            }
            Assert.That(contact,Is.GreaterThan(.15f));Assert.That(jumped,Is.True);
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
    }
}
