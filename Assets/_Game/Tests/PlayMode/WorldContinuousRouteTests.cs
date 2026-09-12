using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Avoidance.Gameplay.Blocks;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Respawn;
using Avoidance.Input;
using Avoidance.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Avoidance.Tests.PlayMode
{
    public sealed class WorldContinuousRouteTests
    {
        private sealed class Input:IPlayerInputSource
        {
            public Vector2 Move{get;set;} public Vector2 LookDelta=>Vector2.zero;
            public bool JumpPressed{get;set;} public void ResetState(){Move=Vector2.zero;JumpPressed=false;}
        }
        private sealed class Node
        {
            public string id;public Vector3 authored,size;public MovingBlock moving;public ModuleMovingBlockDefinition motion;public bool boost;
            public Vector3 Position=>moving!=null?moving.transform.position:authored;
        }
        [UnityTest] public IEnumerator CompleteEveryCampaignRouteFromRest()
        {
            var failures=new List<string>();var previousSimulation=Physics.simulationMode;
            Physics.simulationMode=SimulationMode.Script;
            try
            {
                foreach(string id in WorldReauthorTests.Worlds.Take(5))
                foreach(float pace in new[]{.8f,1f})
                {
                    ModuleSelectionState.Select(id,true);
                    yield return new UnitySceneLevelLoader().LoadAsync("ModuleRunner");yield return null;
                    Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled=false;
                    var motor=Object.FindAnyObjectByType<ParkourMotor>();var restore=motor.GetComponent<RestoreController>();
                    var module=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
                    var nodes=module.Blocks.Where(b=>!b.StableId.Contains("mastery")&&!b.StableId.Contains("risky")&&!b.StableId.Contains("skill"))
                        .Select(b=>new Node{id=b.StableId,authored=b.Pose.Position,size=b.Size}).ToList();
                    nodes.AddRange(module.CrumblingBlocks.Select(b=>new Node{id=b.StableId,authored=b.Pose.Position,size=b.Size}));
                    nodes.AddRange(module.BoostBlocks.Where(b=>!b.StableId.Contains("mastery")).Select(b=>new Node{id=b.StableId,authored=b.Pose.Position,size=b.Size,boost=true}));
                    var movingTicks=new List<Action>();
                    foreach(var def in module.MovingBlocks)
                    {
                        var moving=Object.FindObjectsByType<MovingBlock>().Single(m=>m.name==def.StableId);moving.enabled=false;moving.ResetForModule();
                        movingTicks.Add((Action)Delegate.CreateDelegate(typeof(Action),moving,typeof(MovingBlock).GetMethod("FixedUpdate",BindingFlags.Instance|BindingFlags.NonPublic)));
                        nodes.Add(new Node{id=def.StableId,authored=def.Pose.Position,size=def.Size,moving=moving,motion=def});
                    }
                    nodes=nodes.OrderBy(n=>n.authored.z).ToList();
                    var crumbles=Object.FindObjectsByType<CrumblingBlock>();foreach(var c in crumbles){c.enabled=false;c.ResetForModule();}
                    var input=new Input();float elapsed=0;var rows=new List<string>{"from,to,elapsed,takeoffSpeed,landingSpeed,x,y,z"};
                    var rideTrace=new List<string>();
                    void Step()
                    {
                        foreach(var tick in movingTicks)tick();Physics.SyncTransforms();
                        motor.Simulate(input,Time.fixedDeltaTime);foreach(var c in crumbles)c.Tick(Time.fixedDeltaTime);
                        Physics.Simulate(Time.fixedDeltaTime);elapsed+=Time.fixedDeltaTime;
                        if(id=="module.002.moving-parts"&&motor.transform.position.z>100&&elapsed<40)
                            rideTrace.Add($"{elapsed:F3} player={motor.transform.position} ground={motor.GroundTransform?.name} grounded={motor.IsGrounded} velocity={motor.Velocity} stick={input.Move} lift={nodes.Single(n=>n.id=="moving.m02.final").Position}");
                    }
                    motor.ResetMotion(module.StartPoint.Position,Quaternion.Euler(module.StartPoint.EulerAngles),0);Physics.SyncTransforms();
                    for(int i=0;i<8;i++)Step();bool failed=false;
                    for(int link=0;link<nodes.Count-1&&!failed;link++)
                    {
                        var from=nodes[link];var to=nodes[link+1];input.ResetState();
                        for(int i=0;i<18;i++)Step();
                        if(from.moving!=null)
                        {
                            for(int i=0;i<200;i++)
                            {
                                var center=Vector3.ProjectOnPlane(from.Position-motor.transform.position,Vector3.up);
                                if(center.magnitude<.45f)break;
                                motor.transform.rotation=Quaternion.LookRotation(center);input.Move=Vector2.up*.5f;Step();
                            }
                            input.ResetState();for(int i=0;i<18;i++)Step();
                            var end=from.motion.PathPoints.Last();
                            for(int i=0;i<1500&&Vector3.Distance(from.Position,end)>.35f;i++)Step();
                            rows.Add($"ride,{from.id},{elapsed:F3},0,0,{motor.transform.position.x:F3},{motor.transform.position.y:F3},{motor.transform.position.z:F3}");
                        }
                        if(to.moving!=null)
                            for(int i=0;i<1500&&Vector3.Distance(to.Position,to.motion.PathPoints[0])>.35f;i++)Step();
                        // Explicit stick pilot walks to the takeoff edge. Its aerial
                        // correction supplies input only; no teleports or velocity writes.
                        Vector3 direction=Vector3.forward;
                        float Remaining()
                        {
                            var local=motor.transform.position-from.Position;
                            float x=Mathf.Abs(direction.x)>.001f?(from.size.x*.5f-Mathf.Sign(direction.x)*local.x)/Mathf.Abs(direction.x):100;
                            float z=Mathf.Abs(direction.z)>.001f?(from.size.z*.5f-Mathf.Sign(direction.z)*local.z)/Mathf.Abs(direction.z):100;
                            return Mathf.Min(x,z);
                        }
                        if(!from.boost)
                        {
                            for(int i=0;i<500;i++)
                            {
                                direction=Vector3.ProjectOnPlane(to.Position-motor.transform.position,Vector3.up).normalized;
                                motor.transform.rotation=Quaternion.LookRotation(direction);input.Move=Vector2.up*pace;
                                if(Remaining()<.35f)break;Step();
                            }
                            input.JumpPressed=true;Step();input.JumpPressed=false;
                        }
                        bool air=!motor.IsGrounded,landed=false;
                        for(int frame=0;frame<240&&!restore.IsRestorePending;frame++)
                        {
                            if(to.boost&&motor.VerticalSpeed>12)
                            {
                                // Actual trigger launch occurred; follow its destination.
                                link++;from=to;to=nodes[link+1];air=true;
                            }
                            float v=motor.VerticalSpeed,g=motor.Profile.FallGravity;
                            float height=motor.transform.position.y-to.Position.y-to.size.y*.5f;
                            float remaining=v>0?v/motor.Profile.JumpGravity+Mathf.Sqrt(Mathf.Max(0,2*(height+v*v/(2*motor.Profile.JumpGravity))/g))
                                :(v+Mathf.Sqrt(Mathf.Max(0,v*v+2*g*height)))/g;
                            var offset=Vector3.ProjectOnPlane(to.Position-motor.transform.position,Vector3.up);
                            var wanted=offset/Mathf.Max(.12f,remaining)-motor.ActualHorizontalVelocity;
                            var local=motor.transform.InverseTransformDirection(wanted);
                            input.Move=Vector2.ClampMagnitude(new Vector2(local.x,local.z)/3,1);
                            Step();air|=!motor.IsGrounded;
                            if(air&&motor.IsGrounded&&Mathf.Abs(motor.transform.position.y-to.Position.y-to.size.y*.5f)<.3f)
                            {landed=true;break;}
                        }
                        var delta=motor.transform.position-to.Position;
                        rows.Add($"{from.id},{to.id},{elapsed:F3},{motor.LastTakeoffHorizontalSpeed:F3},{motor.HorizontalSpeed:F3},{motor.transform.position.x:F3},{motor.transform.position.y:F3},{motor.transform.position.z:F3}");
                        if(!landed||Mathf.Abs(delta.x)>to.size.x*.5f+.25f||Mathf.Abs(delta.z)>to.size.z*.5f+.25f||restore.IsRestorePending)
                        {failures.Add($"{id} pace {pace} {from.id} -> {to.id} at {motor.transform.position} grounded={motor.IsGrounded} fatal={restore.IsRestorePending}");failed=true;}
                        yield return null;
                    }
                    if(!failed)
                    {
                        var patch=Object.FindAnyObjectByType<PatchBlock>();
                        for(int i=0;i<300&&!patch.Completed;i++)
                        {
                            var direction=Vector3.ProjectOnPlane(module.PatchBlock.Pose.Position-motor.transform.position,Vector3.up);
                            if(direction.sqrMagnitude>.01f)motor.transform.rotation=Quaternion.LookRotation(direction);
                            input.Move=Vector2.up*.5f;Step();
                        }
                        if(!patch.Completed)failures.Add(id+" did not enter Patch trigger");
                    }
                    Directory.CreateDirectory("Logs/WorldReauthorQA");File.WriteAllLines("Logs/WorldReauthorQA/"+id+"-continuous-"+pace+".csv",rows);
                    if(rideTrace.Count>0)File.WriteAllLines("Logs/WorldReauthorQA/ride-"+pace+".txt",rideTrace);
                    yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
                }
            }
            finally{Physics.simulationMode=previousSimulation;}
            Assert.That(failures,Is.Empty,string.Join("\n",failures));
        }
    }
}
