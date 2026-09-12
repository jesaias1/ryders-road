using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Respawn;
using Avoidance.Gameplay.Blocks;
using Avoidance.Input;
using Avoidance.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Avoidance.Tests.PlayMode
{
    public sealed class FlowSequence162Tests
    {
        private sealed class Input : IPlayerInputSource, IHeldJumpInputSource
        {
            public Vector2 Move {get;set;} public Vector2 LookDelta=>Vector2.zero;
            public bool JumpPressed {get;set;} public bool JumpHeld {get;set;}=true;
            public void ResetState(){Move=Vector2.zero;JumpHeld=false;}
        }
        [UnityTest] public IEnumerator HeldSequencesCarryLandingErrorsWithoutPerHopReset()
        {
            ModuleSelectionState.Select("module.005.foundry-pulse");
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleRunner");yield return null;
            Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled=false;
            var motor=Object.FindAnyObjectByType<ParkourMotor>();var restore=motor.GetComponent<RestoreController>();
            var module=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
            var all=Quality130Tests.WindwardRoute(module);
            var crumbles=Object.FindObjectsByType<CrumblingBlock>();foreach(var c in crumbles)c.enabled=false;
            var rows=new List<string>{"sequence,mode,error,from,to,skipped,takeoffSpeed,landingSpeed,flightDistance,width,depth,headingChange,landingX,landingZ"};
            var outcomes=new List<string>{"sequence,mode,error,reachedRecovery,landings,finalZ,fatal"};
            var failures=new List<string>();
            foreach(var bounds in new[]{("acceleration","arrival","west.restore"),("sweep","west.restore","east.restore"),("diagonal","east.restore","lens.restore"),("final","lens.restore","patch.base")})
            foreach(string mode in new[]{"slow","flow","expert"})
            foreach(string error in new[]{"clean","left","right","early","late","heading"})
            {
                int first=System.Array.FindIndex(all,b=>b.StableId=="m05."+bounds.Item2),last=System.Array.FindIndex(all,b=>b.StableId=="m05."+bounds.Item3);
                var sequence=all.Skip(first).Take(last-first+1).ToArray();
                float speed=mode=="slow"?7.02f:mode=="expert"?17.5f:bounds.Item1=="diagonal"||bounds.Item1=="final"?13.5f:14f;
                restore.RestoreNow();restore.Tick(false,1);foreach(var c in crumbles)c.ResetForModule();
                float offset=error=="left"?-.75f:error=="right"?.75f:0;
                var start=sequence[0].Pose.Position+new Vector3(offset,.35f,sequence[0].Size.z*.5f-(mode=="slow"?.7f:.08f)+(error=="early"?-.35f:error=="late"?.15f:0));
                motor.ResetMotion(start,Quaternion.identity,0);Physics.SyncTransforms();
                var input=new Input{JumpHeld=false};for(int i=0;i<5;i++)motor.Simulate(input,1f/120);
                motor.ApplyLaunch(Quaternion.Euler(0,error=="heading"?5:0,0)*Vector3.forward*speed,true);input.JumpHeld=mode!="slow";
                int jumps=motor.JumpCount,landings=0,previousIndex=0;bool air=false,reached=false;
                int slowStop=0,slowNext=1;var slowWish=Vector3.forward;
                var takeoff=start;float takeoffSpeed=speed;var takeoffHeading=Vector3.forward;
                for(int frame=0;frame<1200&&!restore.IsRestorePending;frame++)
                {
                    // Explicit fixture stick input follows a broad sequence centreline.
                    // No landing prediction, velocity writes, per-hop resets or runtime assistance.
                    float z=motor.transform.position.z+8;
                    int ahead=1;while(ahead<sequence.Length-1&&sequence[ahead].Pose.Position.z<z)ahead++;
                    var a=sequence[ahead-1].Pose.Position;var b=sequence[ahead].Pose.Position;
                    float x=Mathf.Lerp(a.x,b.x,Mathf.InverseLerp(a.z,b.z,z));
                    if(landings==0)x+=offset;
                    var wish=new Vector3(Mathf.Clamp((x-motor.transform.position.x)/8,-.55f,.55f),0,1).normalized;
                    if(error=="heading"&&landings==0)wish=Quaternion.Euler(0,5,0)*wish;
                    input.JumpPressed=false;
                    if(mode=="slow")
                    {
                        input.JumpHeld=false;
                        if(slowStop>0){slowStop--;input.Move=Vector2.zero;}
                        else
                        {
                            if(motor.IsGrounded)
                            {
                                slowWish=sequence[slowNext].Pose.Position-motor.transform.position;slowWish.y=0;slowWish.Normalize();
                                var deck=sequence[previousIndex];var local=motor.transform.position-deck.Pose.Position;
                                float edgeX=Mathf.Abs(slowWish.x)>.001f?(deck.Size.x*.5f-Mathf.Sign(slowWish.x)*local.x)/Mathf.Abs(slowWish.x):100;
                                float edgeZ=Mathf.Abs(slowWish.z)>.001f?(deck.Size.z*.5f-Mathf.Sign(slowWish.z)*local.z)/Mathf.Abs(slowWish.z):100;
                                input.JumpPressed=Mathf.Min(edgeX,edgeZ)<.6f;
                            }
                            input.Move=new Vector2(slowWish.x,slowWish.z)*.9f;
                        }
                    }
                    else input.Move=new Vector2(wish.x,wish.z);
                    motor.Simulate(input,1f/120);foreach(var c in crumbles)c.Tick(1f/120);
                    if(motor.JumpCount>jumps)
                    {
                        jumps=motor.JumpCount;takeoff=motor.transform.position;takeoffSpeed=motor.LastTakeoffHorizontalSpeed;
                        takeoffHeading=motor.ActualHorizontalVelocity.normalized;
                    }
                    air|=!motor.IsGrounded;
                    if(air&&motor.IsGrounded)
                    {
                        air=false;
                        int hit=System.Array.FindIndex(sequence,b=>Mathf.Abs(motor.transform.position.x-b.Pose.Position.x)<=b.Size.x*.5f+.3f
                            &&Mathf.Abs(motor.transform.position.z-b.Pose.Position.z)<=b.Size.z*.5f+.3f
                            &&Mathf.Abs(motor.transform.position.y-b.Pose.Position.y-.3f)<.45f);
                        if(hit<0)continue;
                        landings++;
                        string skipped=string.Join(";",sequence.Skip(previousIndex+1).Take(Mathf.Max(0,hit-previousIndex-1)).Select(b=>b.StableId));
                        var deck=sequence[hit];var d=motor.transform.position-deck.Pose.Position;
                        rows.Add($"{bounds.Item1},{mode},{error},{sequence[previousIndex].StableId},{deck.StableId},{skipped},{takeoffSpeed:F3},{motor.HorizontalSpeed:F3},{Vector3.ProjectOnPlane(motor.transform.position-takeoff,Vector3.up).magnitude:F3},{deck.Size.x},{deck.Size.z},{Vector3.SignedAngle(takeoffHeading,motor.ActualHorizontalVelocity,Vector3.up):F2},{d.x:F3},{d.z:F3}");
                        previousIndex=hit;slowNext=Mathf.Min(hit+1,sequence.Length-1);slowStop=24;
                        if(hit==sequence.Length-1){reached=true;break;}
                    }
                    if(motor.transform.position.y<sequence[0].Pose.Position.y-3)break;
                }
                if(reached)
                {
                    input.JumpHeld=false;input.JumpPressed=false;input.Move=Vector2.zero;
                    for(int i=0;i<120;i++)motor.Simulate(input,1f/120);
                    bool stopped=motor.IsGrounded&&motor.HorizontalSpeed<.2f&&!restore.IsRestorePending;
                    if(!stopped)failures.Add(bounds.Item1+"/"+mode+"/"+error+" cannot stop on recovery at "+motor.transform.position);
                }
                outcomes.Add($"{bounds.Item1},{mode},{error},{reached},{landings},{motor.transform.position.z:F3},{restore.IsRestorePending}");
                if(!reached)failures.Add(bounds.Item1+"/"+mode+"/"+error+" at "+motor.transform.position+" lands="+landings);
                yield return null;
            }
            Directory.CreateDirectory("Logs/Flow162QA");File.WriteAllLines("Logs/Flow162QA/held.csv",rows);File.WriteAllLines("Logs/Flow162QA/outcomes.csv",outcomes);
            Assert.That(failures,Is.Empty,string.Join("\n",failures));
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
    }
}
