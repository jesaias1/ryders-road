using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avoidance.Core.Services;
using Avoidance.Gameplay.Blocks;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Visuals;
using Avoidance.UI;
using Avoidance.SaveSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Avoidance.Tests.PlayMode
{
    public sealed class Quality130Tests
    {
        private ServiceContainer previous, isolated;
        [SetUp] public void IsolateSave()
        {
            previous=GameServices.Current;isolated=new ServiceContainer();
            var save=new JsonSaveService(new MemoryStore(),"quality-test");save.Initialize();
            isolated.Register<ISaveService>(save);GameServices.Publish(isolated);
        }
        [TearDown] public void RestoreSave(){if(previous!=null)GameServices.Publish(previous);else GameServices.Clear(isolated);}
        private sealed class MemoryStore : ISaveFileStore
        {
            private string json;public bool PrimaryExists=>json!=null;public bool BackupExists=>false;
            public bool TryReadPrimary(out string value){value=json;return value!=null;}
            public bool TryReadBackup(out string value){value=null;return false;}
            public void WriteAtomic(string value,bool preserveBackup=false){json=value;}
            public void DeleteAll(){json=null;}
        }
        public static ModuleBlockDefinition[] WindwardRoute(ModuleDefinition m, bool fast=false)
        {
            var list=m.Blocks.Where(b=>!b.StableId.Contains("skill")).ToList();
            int a=list.FindIndex(b=>b.StableId=="m05.arc.03")+1;
            list.InsertRange(a,m.CrumblingBlocks.Select(b=>new ModuleBlockDefinition(b.StableId,b.Pose.Position,b.Size,ModuleMaterialRole.Crumbling)));
            if(fast)
            {
                int west=list.FindIndex(b=>b.StableId=="m05.west.restore"),east=list.FindIndex(b=>b.StableId=="m05.east.restore");
                list.RemoveRange(west+1,east-west-1);list.InsertRange(west+1,m.Blocks.Where(b=>b.StableId.Contains("skill")));
            }
            return list.ToArray();
        }
        public static ModuleBlockDefinition[] FoundryFast(ModuleDefinition m)
        {
            var list=m.Blocks.Where(b=>!b.StableId.Contains("mastery")).ToList();
            list.InsertRange(list.FindIndex(b=>b.StableId=="m04.transfer.dock")+1,m.Blocks.Where(b=>b.StableId.Contains("mastery")));
            list.InsertRange(list.FindIndex(b=>b.StableId=="m04.furnace.court")+1,m.CrumblingBlocks.Take(1).Select(b=>new ModuleBlockDefinition(b.StableId,b.Pose.Position,b.Size,ModuleMaterialRole.Crumbling)));
            return list.ToArray();
        }
        private sealed class Input : Avoidance.Input.IPlayerInputSource
        {
            public Vector2 Move{get;set;}=Vector2.up;public Vector2 LookDelta=>Vector2.zero;
            public bool JumpPressed{get;set;}public void ResetState(){JumpPressed=false;}
        }
        [UnityTest] public IEnumerator ContinuousWindwardNormalAndFastLine() => VerifyContinuous(false);
        [UnityTest] public IEnumerator FoundationContinuousWindwardAndFoundry() => VerifyContinuous(true);
        [UnityTest] public IEnumerator SlowOrdinaryWindwardCompletion() => VerifyContinuous(false,.7f);
        private IEnumerator VerifyContinuous(bool foundation,float inputScale=1)
        {
            foreach(var id in new[]{"module.005.foundry-pulse","module.004.solar-foundry"})
            foreach(bool fast in new[]{false,true})
            {
                if(inputScale<1 && (fast||id.Contains("solar")))continue;
                if(id.Contains("solar") && !fast) continue;
                if(foundation) CampaignFlowTrial.Launch(id,CampaignTrialMode.Foundation);
                else ModuleSelectionState.Select(id);
                yield return new UnitySceneLevelLoader().LoadAsync("ModuleRunner");yield return null;
                Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled=false;
                var motor=Object.FindAnyObjectByType<ParkourMotor>();
                var m=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
                var route=id.Contains("005")?WindwardRoute(m,fast):FoundryFast(m);
                var crumble=Object.FindObjectsByType<CrumblingBlock>();foreach(var c in crumble)c.enabled=false;
                float elapsed=0;var input=new Input();
                var measures=new List<string>{"from,to,fromWidth,fromLength,toWidth,toLength,approachSpeed,takeoffSpeed,landingSpeed,flightDistance"};
                motor.ResetMotion(route[0].Pose.Position+Vector3.up*.34f,Quaternion.identity,0);Physics.SyncTransforms();
                int captureFrame=0,simulationStep=0;
                bool capture=foundation && !fast && id.Contains("005")
                    && System.Environment.GetEnvironmentVariable("RYDERS_FOUNDATION_CAPTURE")=="1";
                void Step()
                {
                    var command=input.Move;input.Move*=inputScale;
                    motor.Simulate(input,1f/60);input.Move=command;
                    foreach(var c in crumble)c.Tick(1f/60);elapsed+=1f/60;
                    if(capture && elapsed<12 && simulationStep++%4==0)
                        Capture("foundation-route-"+(captureFrame++).ToString("D4"),Camera.main);
                }
                for(int link=0;link<route.Length-1;link++)
                {
                    var from=route[link];var to=route[link+1];
                    if(!fast){input.Move=Vector2.zero;for(int i=0;i<12;i++)Step();}
                    if(from.StableId=="m05.lens.restore")
                    {
                        // Safe pilot crosses the recovery terrace before turning.
                        // Cutting straight from its near-left corner is an expert gap.
                        for(int i=0;i<240;i++)
                        {
                            var across=from.Pose.Position-motor.transform.position;across.y=0;
                            if(across.magnitude<.7f)break;
                            motor.transform.rotation=Quaternion.LookRotation(across);input.Move=Vector2.up;Step();
                        }
                    }
                    var direction=to.Pose.Position-motor.transform.position;direction.y=0;direction.Normalize();
                    motor.transform.rotation=Quaternion.LookRotation(direction);Physics.SyncTransforms();input.Move=Vector2.up;
                    float Remaining()
                    {
                        var local=motor.transform.position-from.Pose.Position;
                        float x=Mathf.Abs(direction.x)>.001f?(from.Size.x*.5f-Mathf.Sign(direction.x)*local.x)/Mathf.Abs(direction.x):100;
                        float z=Mathf.Abs(direction.z)>.001f?(from.Size.z*.5f-Mathf.Sign(direction.z)*local.z)/Mathf.Abs(direction.z):100;
                        return Mathf.Min(x,z);
                    }
                    float margin=fast?.25f:.6f;
                    // Crown court asks for an earlier takeoff into the shallow beam.
                    if(from.StableId=="m04.crown.restore")margin=1.15f;
                    for(int i=0;i<150 && Remaining()>margin;i++)Step();
                    var takeoffPosition=motor.transform.position;float approachSpeed=motor.HorizontalSpeed;
                    input.JumpPressed=true;Step();input.JumpPressed=false;
                    bool air=false,land=false;
                    for(int i=0;i<110;i++){
                        if(motor.Profile.MovementFoundation)
                        {
                            // Test pilot only: explicit stick correction, never runtime assistance.
                            float v=motor.VerticalSpeed,g=motor.Profile.FallGravity;
                            float height=motor.transform.position.y-to.Pose.Position.y-.3f;
                            float remaining=v>0 ? v/motor.Profile.JumpGravity
                                +Mathf.Sqrt(Mathf.Max(0,2*(height+v*v/(2*motor.Profile.JumpGravity))/g))
                                :(v+Mathf.Sqrt(Mathf.Max(0,v*v+2*g*height)))/g;
                            var offset=to.Pose.Position-motor.transform.position;offset.y=0;
                            var wanted=offset/Mathf.Max(.08f,remaining)-motor.ActualHorizontalVelocity;
                            var local=motor.transform.InverseTransformDirection(wanted);
                            input.Move=Vector2.ClampMagnitude(new Vector2(local.x,local.z)/2f,1);
                        }
                        Step();air|=!motor.IsGrounded;if(air&&motor.IsGrounded)
                        {
                            // Ground probes can report contact before the capsule finishes
                            // descending to the top. Observe stable arrival without resetting it.
                            if(Mathf.Abs(motor.transform.position.y-to.Pose.Position.y-.3f)<.3f){land=true;break;}
                        }}
                    var delta=motor.transform.position-to.Pose.Position;
                    measures.Add($"{from.StableId},{to.StableId},{from.Size.x},{from.Size.z},{to.Size.x},{to.Size.z},{approachSpeed:F3},{motor.LastTakeoffHorizontalSpeed:F3},{motor.HorizontalSpeed:F3},{Vector3.ProjectOnPlane(motor.transform.position-takeoffPosition,Vector3.up).magnitude:F3}");
                    Assert.That(land&&Mathf.Abs(delta.y-.3f)<.3f&&Mathf.Abs(delta.x)<to.Size.x*.5f+.2f&&Mathf.Abs(delta.z)<to.Size.z*.5f+.2f,Is.True,
                        (fast?"fast":"normal")+" "+from.StableId+" -> "+to.StableId+" at "+motor.transform.position+" speed "+motor.Velocity.magnitude);
                    yield return null;
                }
                for(int step=0;step<120;step++)
                {
                    var finishDirection=m.PatchBlock.Pose.Position-motor.transform.position;finishDirection.y=0;
                    if(finishDirection.magnitude<.9f)break;
                    motor.transform.rotation=Quaternion.LookRotation(finishDirection);input.Move=Vector2.up;Step();
                }
                var patch=Object.FindAnyObjectByType<PatchBlock>();
                Assert.That(patch.GetComponent<Collider>().bounds.Intersects(motor.GetComponent<CharacterController>().bounds),Is.True,"Finish trigger reached continuously");
                Assert.That(patch.TryComplete(motor),Is.True);
                Directory.CreateDirectory("Logs/Quality130QA");
                Directory.CreateDirectory("Logs/Benchmark160QA");
                File.WriteAllLines("Logs/Benchmark160QA/"+(inputScale<1?"slow-":foundation?"trial-":"normal-")+id+"-"+(fast?"direct":"safe")+".csv",measures);
                File.WriteAllText("Logs/Quality130QA/"+(foundation?"foundation-":"")+id+"-"+(fast?"fast":"normal")+".txt",elapsed.ToString("F3",System.Globalization.CultureInfo.InvariantCulture));
                Debug.Log("QUALITY130 "+id+" "+(fast?"fast":"normal")+" inputScale="+inputScale+" continuous seconds="+elapsed);
            }
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        [UnityTest] public IEnumerator CaptureAffectedGameplayAndAuditSkins()
        {
            foreach(var id in new[]{"module.003.flow-error","module.004.solar-foundry","module.005.foundry-pulse"})
            {
                ModuleSelectionState.Select(id,true);yield return new UnitySceneLevelLoader().LoadAsync("ModuleRunner");yield return null;
                Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled=false;
                var camera=Camera.main;foreach(var c in camera.GetComponents<MonoBehaviour>())c.enabled=false;
                var m=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
                var route=id.Contains("005")?WindwardRoute(m):m.Blocks.Where(b=>!b.StableId.Contains("mastery")).ToArray();
                foreach(int i in new[]{0,route.Length/4,route.Length/2,route.Length*3/4,route.Length-2})
                {
                    camera.transform.position=route[i].Pose.Position+Vector3.up*1.92f;
                    camera.transform.LookAt(route[i+1].Pose.Position+Vector3.up*.3f);
                    Capture(id+"-"+i,camera);yield return null;
                }
                if(id=="module.003.flow-error")
                {
                    camera.transform.position=new Vector3(0,2.1f,5);camera.transform.LookAt(new Vector3(0,.3f,5));Capture("abyss-plate",camera);
                    var boost=m.BoostBlocks[0];camera.transform.position=boost.Pose.Position+new Vector3(0,2,-5);camera.transform.LookAt(boost.Pose.Position);
                    Assert.That(Object.FindObjectsByType<Transform>().Any(t=>t.name=="Energy Streak"),Is.False);
                    Capture("boost-idle",camera);
                    Object.FindAnyObjectByType<GameplayVfxService>().PlayBoost(boost.Pose.Position,boost.LaunchDirection);
                    yield return new WaitForSecondsRealtime(.08f);Capture("boost-active",camera);
                    var target=m.Blocks.Single(b=>b.StableId=="m03.boost-landing");
                    var support=GameObject.Find(target.StableId);
                    Assert.That(support.GetComponentsInChildren<Transform>().Any(t=>t.name.Contains("Tile [")),Is.False);
                    camera.transform.position=target.Pose.Position+new Vector3(0,1.92f,-3);camera.transform.LookAt(target.Pose.Position+new Vector3(0,.2f,3));Capture("abyss-court",camera);
                    foreach(var marker in Object.FindObjectsByType<Transform>().Where(t=>t.name=="Restore Shrine Visual"))
                    {
                        var signal=marker.GetComponent<RestoreSurfaceMarker>();Assert.That(signal,Is.Not.Null);
                        Assert.That(marker.GetComponentsInChildren<Collider>(),Is.Empty);
                        Assert.That(marker.GetComponent<MeshFilter>().sharedMesh.bounds.size.y,Is.EqualTo(0).Within(.001f));
                        signal.Activate();Assert.That(signal.IsPulsing,Is.True);
                        camera.transform.position=marker.position+new Vector3(0,1.6f,-3);camera.transform.LookAt(marker.position);
                        Capture("restore-"+marker.parent.name,camera);
                    }
                    File.WriteAllText("Logs/Quality130QA/abyss-renderers.txt",string.Join("\n",Object.FindObjectsByType<MeshRenderer>().Select(r=>r.name+" parent="+r.transform.parent?.name+" position="+r.transform.position+" bounds="+r.bounds)));
                }
            }
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        private static void Capture(string name,Camera camera)
        {
            Directory.CreateDirectory("Logs/Quality130QA");
            var rt=new RenderTexture(1560,720,24);var texture=new Texture2D(1560,720,TextureFormat.RGB24,false);
            var active=RenderTexture.active;float aspect=camera.aspect;
            camera.targetTexture=rt;camera.aspect=1560f/720;camera.Render();RenderTexture.active=rt;
            texture.ReadPixels(new Rect(0,0,1560,720),0,0);texture.Apply();File.WriteAllBytes("Logs/Quality130QA/"+name+".png",texture.EncodeToPNG());
            camera.targetTexture=null;camera.aspect=aspect;RenderTexture.active=active;Object.Destroy(rt);Object.Destroy(texture);
        }
    }
}
