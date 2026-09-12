using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Respawn;
using Avoidance.Gameplay.Worlds;
using Avoidance.Input;
using Avoidance.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Avoidance.Tests.PlayMode
{
    public sealed class Benchmark160Tests
    {
        private sealed class Input : IPlayerInputSource,IHeldJumpInputSource
        {
            public Vector2 Move{get;set;}public Vector2 LookDelta=>Vector2.zero;
            public bool JumpPressed{get;set;}public bool JumpHeld{get;set;}
            public void ResetState(){Move=Vector2.zero;JumpHeld=JumpPressed=false;}
        }
        private static IEnumerator Open(string id)
        {
            ModuleSelectionState.Select(id);
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleRunner");yield return null;
            Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled=false;
        }
        [UnityTest] public IEnumerator AuditAllWorldSceneryAndActualFatalLandings()
        {
            var report=new List<string>{"module,solidObjects,colliders,previouslyUnmarkedObjects,previouslyUnmarkedColliders,actualFatalDrops"};
            foreach(string id in ModuleSelectionState.GetCampaignModuleIds().Concat(new[]{ModuleSelectionState.SpiralModuleId}))
            {
                yield return Open(id);
                var module=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
                var motor=Object.FindAnyObjectByType<ParkourMotor>();var restore=motor.GetComponent<RestoreController>();
                var relay=motor.GetComponent<SceneryLandingRecovery>();var input=new Input();
                var objects=module.EnvironmentBiomeProfile?.WorldObjects.Where(w=>w.IsValid&&w.HasPlayableArchitecture).ToArray()??new BiomeWorldObject[0];
                int oldObjects=objects.Count(w=>w.Prefab.GetComponentsInChildren<AuthoredSurface>(true).Any(s=>!s.RestoreOnLanding));
                int oldSurfaces=objects.Sum(w=>w.Prefab.GetComponentsInChildren<AuthoredSurface>(true).Count(s=>!s.RestoreOnLanding));
                var surfaces=Object.FindObjectsByType<AuthoredSurface>().Where(s=>s.GetComponent<Collider>().enabled).ToArray();
                foreach(var surface in surfaces)
                {
                    Assert.That(surface.GeometryKind,Is.EqualTo(WorldGeometryKind.FatalScenery),id+"/"+surface.name);
                    Assert.That(surface.HasMatchingCollision,Is.True);
                }
                int drops=0;
                foreach(var surface in surfaces)
                {
                    if(drops>=5)break;
                    var collider=surface.GetComponent<Collider>();var b=collider.bounds;bool sampled=false;
                    foreach(float x in new[]{.2f,.5f,.8f})foreach(float z in new[]{.2f,.5f,.8f})
                    {
                        if(sampled)continue;
                        var origin=new Vector3(Mathf.Lerp(b.min.x,b.max.x,x),b.max.y+2,Mathf.Lerp(b.min.z,b.max.z,z));
                        if(!collider.Raycast(new Ray(origin,Vector3.down),out var hit,b.size.y+4)||hit.normal.y<.5f)continue;
                        restore.RestoreNow();restore.Tick(false,1);
                        motor.ResetMotion(hit.point+Vector3.up*1.4f,Quaternion.identity,0);Physics.SyncTransforms();
                        input.Move=Vector2.zero;input.JumpHeld=true;int count=restore.RestoreCount;
                        for(int step=0;step<90&&!restore.IsRestorePending;step++)motor.Simulate(input,1f/60);
                        if(!restore.IsRestorePending)continue; // A legitimate route surface may occlude this scenery sample.
                        int jumps=motor.JumpCount;var position=motor.transform.position;
                        for(int step=0;step<5;step++)motor.Simulate(input,1f/60);
                        Assert.That(motor.JumpCount,Is.EqualTo(jumps),"No second jump after fatal contact");
                        Assert.That(motor.transform.position,Is.EqualTo(position),"Fatal contact freezes the pending restore");
                        restore.Tick(false,.1f);Assert.That(restore.RestoreCount,Is.EqualTo(count+1));
                        drops++;sampled=true;
                    }
                }
                if(surfaces.Length>0)Assert.That(drops,Is.GreaterThan(0),id);
                // All ordinary route decks remain legitimate, including side recovery space.
                foreach(var block in module.Blocks)
                {
                    restore.RestoreNow();motor.ResetMotion(block.Pose.Position+Vector3.up*.6f,Quaternion.identity,0);Physics.SyncTransforms();
                    input.ResetState();for(int i=0;i<12;i++)motor.Simulate(input,1f/60);
                    Assert.That(restore.IsRestorePending,Is.False,id+" false fatal "+block.StableId);
                }
                if(surfaces.Length>0)
                {
                    restore.RestoreNow();relay.NotifyContact(surfaces[0].GetComponent<Collider>(),Vector3.right);
                    Assert.That(restore.IsRestorePending,Is.True,"Spawn protection cannot make scenery landable");restore.Tick(false,.1f);
                }
                report.Add($"{id},{objects.Length},{surfaces.Length},{oldObjects},{oldSurfaces},{drops}");
                Directory.CreateDirectory("Logs/Benchmark160QA");File.WriteAllLines("Logs/Benchmark160QA/scenery-audit.csv",report);
            }
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }

        [UnityTest] public IEnumerator HeldChainsAtOrdinaryAndCarriedSpeed()
        {
            yield return Open("module.005.foundry-pulse");
            var motor=Object.FindAnyObjectByType<ParkourMotor>();var restore=motor.GetComponent<RestoreController>();
            var results=new List<string>{"initialSpeed,seconds,jumps,distance,finalSpeed"};
            foreach(float speed in new[]{7.8f,14f})
            {
                restore.RestoreNow();motor.ResetMotion(new Vector3(-24,.35f,8),Quaternion.identity,0);Physics.SyncTransforms();
                var input=new Input();for(int i=0;i<8;i++)motor.Simulate(input,1f/120);
                motor.ApplyLaunch(Vector3.forward*speed,true);input.JumpHeld=true;
                int frames=speed>10?360:384;int before=motor.JumpCount;
                for(int i=0;i<frames;i++)
                {
                    // Human-expressible small alternating stick corrections; no route lookup.
                    input.Move=new Vector2(Mathf.Sin(i*.035f)*.08f,1).normalized;
                    motor.Simulate(input,1f/120);
                    Assert.That(restore.IsRestorePending,Is.False,"Held chain "+speed+" at "+motor.transform.position);
                }
                Assert.That(motor.JumpCount-before,Is.GreaterThanOrEqualTo(4));
                Assert.That(motor.transform.position.z,Is.GreaterThan(30));
                Assert.That(motor.transform.position.y,Is.GreaterThan(0));
                results.Add($"{speed},{frames/120f},{motor.JumpCount-before},{motor.transform.position.z-8:F3},{motor.HorizontalSpeed:F3}");
            }
            Directory.CreateDirectory("Logs/Benchmark160QA");File.WriteAllLines("Logs/Benchmark160QA/held-chains.csv",results);
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }

        [UnityTest] public IEnumerator MeasuredSkipsAndImperfectLandingRecovery()
        {
            yield return Open("module.005.foundry-pulse");
            CaptureOverview();
            var module=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
            var motor=Object.FindAnyObjectByType<ParkourMotor>();var restore=motor.GetComponent<RestoreController>();
            var rows=new List<string>{"from,to,initialSpeed,launchSpeed,landingSpeed,flightDistance,landingOffset"};
            foreach(var link in new[]{("arrival","approach.b",17f),("approach.b","west.restore",17f),("ascent.a","ascent.c",17f),("lens.a","lens.c",17f)})
            foreach(float error in new[]{0f,.75f})
            {
                var from=module.Blocks.Single(b=>b.StableId=="m05."+link.Item1);var to=module.Blocks.Single(b=>b.StableId=="m05."+link.Item2);
                var dir=to.Pose.Position-from.Pose.Position;dir.y=0;dir.Normalize();
                float Edge(Vector3 size)=>Mathf.Min(Mathf.Abs(dir.x)>.001f?size.x*.5f/Mathf.Abs(dir.x):1000,Mathf.Abs(dir.z)>.001f?size.z*.5f/Mathf.Abs(dir.z):1000);
                var start=from.Pose.Position+dir*(Edge(from.Size)-.12f)+Vector3.up*.35f;
                restore.RestoreNow();motor.ResetMotion(start,Quaternion.LookRotation(dir),0);Physics.SyncTransforms();
                var input=new Input();for(int i=0;i<5;i++)motor.Simulate(input,1f/120);
                motor.ApplyLaunch(dir*link.Item3,true);input.JumpPressed=true;motor.Simulate(input,1f/120);input.JumpPressed=false;
                float launch=motor.LastTakeoffHorizontalSpeed;bool air=false,land=false;
                for(int i=0;i<180;i++)
                {
                    // Explicit bounded test input toward the near portion of the broad landing.
                    var target=to.Pose.Position-dir*(Edge(to.Size)-1.1f)+Vector3.Cross(Vector3.up,dir)*error;
                    float v=motor.VerticalSpeed,g=motor.Profile.FallGravity,h=motor.transform.position.y-to.Pose.Position.y-.3f;
                    float t=v>0?v/motor.Profile.JumpGravity+Mathf.Sqrt(Mathf.Max(0,2*(h+v*v/(2*motor.Profile.JumpGravity))/g)):(v+Mathf.Sqrt(Mathf.Max(0,v*v+2*g*h)))/g;
                    var wanted=(target-motor.transform.position)/Mathf.Max(.08f,t)-motor.ActualHorizontalVelocity;wanted.y=0;
                    var local=motor.transform.InverseTransformDirection(wanted);input.Move=Vector2.ClampMagnitude(new Vector2(local.x,local.z)/4,1);
                    motor.Simulate(input,1f/120);air|=!motor.IsGrounded;
                    Assert.That(restore.IsRestorePending,Is.False,link+" fatal collision");
                    if(air&&motor.IsGrounded){land=true;break;}
                }
                var delta=motor.transform.position-to.Pose.Position;
                Assert.That(land&&Mathf.Abs(delta.x)<to.Size.x*.5f+.15f&&Mathf.Abs(delta.z)<to.Size.z*.5f+.15f,Is.True,link+" at "+motor.transform.position);
                rows.Add($"{from.StableId},{to.StableId},{link.Item3},{launch:F3},{motor.HorizontalSpeed:F3},{Vector3.ProjectOnPlane(motor.transform.position-start,Vector3.up).magnitude:F3},{error}");
                Directory.CreateDirectory("Logs/Benchmark160QA");File.WriteAllLines("Logs/Benchmark160QA/skips.csv",rows);
            }
            Directory.CreateDirectory("Logs/Benchmark160QA");File.WriteAllLines("Logs/Benchmark160QA/skips.csv",rows);
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        private static void CaptureOverview()
        {
            var camera=Camera.main;var position=camera.transform.position;var rotation=camera.transform.rotation;
            var rt=new RenderTexture(1560,900,24);var image=new Texture2D(1560,900,TextureFormat.RGB24,false);
            var active=RenderTexture.active;float aspect=camera.aspect;
            try
            {
                camera.transform.position=new Vector3(35,100,-12);camera.transform.LookAt(new Vector3(35,0,40));
                camera.targetTexture=rt;camera.aspect=1560f/900;camera.Render();RenderTexture.active=rt;
                image.ReadPixels(new Rect(0,0,1560,900),0,0);image.Apply();
                Directory.CreateDirectory("Logs/Benchmark160QA");File.WriteAllBytes("Logs/Benchmark160QA/overview.png",image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture=null;camera.aspect=aspect;camera.transform.SetPositionAndRotation(position,rotation);
                RenderTexture.active=active;Object.Destroy(rt);Object.Destroy(image);
            }
        }
    }
}
