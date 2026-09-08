using System.Collections;
using System.IO;
using System.Linq;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Blocks;
using Avoidance.Gameplay.Worlds;
using Avoidance.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Avoidance.Core.Services;
using Avoidance.SaveSystem;
using Avoidance.Gameplay.Respawn;
using UnityEngine.UI;
using Avoidance.Gameplay.Timing;

namespace Avoidance.Tests.PlayMode
{
    public sealed class WindwardTests
    {
        private const string Id="module.005.foundry-pulse";
        private ServiceContainer previous,isolated;
        private JsonSaveService save;
        [SetUp] public void IsolateSave()
        {
            previous=GameServices.Current;isolated=new ServiceContainer();
            save=new JsonSaveService(new MemoryStore(),"foundry-test");save.Initialize();isolated.Register<ISaveService>(save);GameServices.Publish(isolated);
        }
        [TearDown] public void RestoreServices()
        {if(previous!=null)GameServices.Publish(previous);else GameServices.Clear(isolated);}
        private sealed class MemoryStore : ISaveFileStore
        {
            private string json;public bool PrimaryExists=>json!=null;public bool BackupExists=>false;
            public bool TryReadPrimary(out string value){value=json;return value!=null;}
            public bool TryReadBackup(out string value){value=null;return false;}
            public void WriteAtomic(string value,bool preserveBackup=false){json=value;}
            public void DeleteAll(){json=null;}
        }
        private sealed class RunInput : Avoidance.Input.IPlayerInputSource, Avoidance.Input.IFlowSteeringInputSource
        {
            public bool FlowSteeringEnabled {get;set;}
            public Avoidance.Input.AutoCameraProfileKind AutoCameraProfile=>Avoidance.Input.AutoCameraProfileKind.Balanced;
            public Vector2 Move {get;set;}=new Vector2(0,.9f);
            public Vector2 LookDelta=>Vector2.zero;public bool JumpPressed {get;set;}
            public void ResetState(){JumpPressed=false;}
        }
        private static IEnumerator Open()
        {
            ModuleSelectionState.Select(Id,true);
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleRunner");
            yield return new WaitForSecondsRealtime(.3f);
            Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled=false;
        }
        [UnityTest] public IEnumerator FootstepsObserveGroundTravelWithoutChangingMotion()
        {
            yield return Open();
            var motor=Object.FindAnyObjectByType<ParkourMotor>();
            var audio=motor.GetComponent<Avoidance.Gameplay.Audio.GroundTravelAudio>();audio.enabled=false;
            var feedback=motor.GetComponent<Avoidance.Gameplay.Audio.MovementFeedback>();
            int steps=0;feedback.CueRequested+=cue=>{if(cue==Avoidance.Gameplay.Audio.GameplayAudioCue.Footstep)steps++;};
            var input=new RunInput();
            for(int i=0;i<34;i++){motor.Simulate(input,1f/60);audio.Tick(1f/60);}
            Assert.That(steps,Is.GreaterThan(0));
            var velocity=motor.Velocity;audio.Tick(0);Assert.That(motor.Velocity,Is.EqualTo(velocity));
            motor.ResetMotion(new Vector3(-18,40,20),Quaternion.identity,0);Physics.SyncTransforms();
            input.Move=Vector2.zero;motor.Simulate(input,1f/60);
            int before=steps;for(int i=0;i<100;i++)audio.Tick(1f/60);
            Assert.That(steps,Is.EqualTo(before),"Airborne travel never produces foot contact");
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        [UnityTest] public IEnumerator StandardAndOptionalRoutesUseAcceptedMotor()
        {
            yield return Open();
            var motor=Object.FindAnyObjectByType<ParkourMotor>();
            Assert.That(motor.Profile.MovementMastery,Is.False);
            var module=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
            var standard=module.Blocks.Where(b=>!b.StableId.Contains("skill")).ToArray();
            for(int i=0;i<standard.Length-1;i++)Jump(motor,standard[i].Pose.Position,standard[i].Size,standard[i+1].Pose.Position,standard[i+1].Size,standard[i+1].StableId);
            var skill=new[]{module.Blocks.Single(b=>b.StableId=="m05.west.restore")}.Concat(module.Blocks.Where(b=>b.StableId.Contains("skill"))).Concat(new[]{module.Blocks.Single(b=>b.StableId=="m05.east.restore")}).ToArray();
            for(int i=0;i<skill.Length-1;i++)
            {
                var from=skill[i];var to=skill[i+1];
                var direction=to.Pose.Position-from.Pose.Position;direction.y=0;direction.Normalize();
                Jump(motor,from.Pose.Position+direction*.8f,from.Size,to.Pose.Position,to.Size,to.StableId);
            }
            foreach(var surface in Object.FindObjectsByType<AuthoredSurface>())Assert.That(surface.HasMatchingCollision,Is.True,surface.CollisionDetails);
            foreach(var point in module.RestorePoints)
            {
                motor.ResetMotion(point.RestorePosition,Quaternion.identity,0);Physics.SyncTransforms();
                yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
                var restore=motor.GetComponent<RestoreController>();restore.Tick(false,1);
                motor.ResetMotion(new Vector3(0,-40,0),Quaternion.identity,0);restore.Tick(false,1);
                Assert.That(Vector3.Distance(motor.transform.position,point.RestorePosition),Is.LessThan(.25f),point.StableId);
            }
            Assert.That(Object.FindAnyObjectByType<PatchBlock>().TryComplete(motor),Is.True);
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        [UnityTest] public IEnumerator RenderCampaignPlayerViewsAndSkyWrap()
        {
            foreach(var id in ModuleSelectionState.GetCampaignModuleIds())
            {
                ModuleSelectionState.Select(id,true);yield return new UnitySceneLevelLoader().LoadAsync("ModuleRunner");
                yield return new WaitForSecondsRealtime(.3f);
                Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled=false;
                var module=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
                var camera=Camera.main;foreach(var c in camera.GetComponents<MonoBehaviour>())c.enabled=false;
                var route=module.Blocks.Where(b=>!b.StableId.Contains("skill")&&!b.StableId.Contains("mastery")).ToArray();
                int[] points={0,route.Length/4,route.Length/2,route.Length*3/4,route.Length-2};
                foreach(int i in points)
                {
                    var b=route[i];camera.transform.position=b.Pose.Position+Vector3.up*1.92f;
                    var d=route[i+1].Pose.Position-b.Pose.Position;d.y=0;
                    camera.transform.rotation=Quaternion.LookRotation(d)*Quaternion.Euler(9,0,0);
                    yield return null;Capture("Logs/Production110QA/"+id+"-"+i+".png");
                }
                Assert.That(RenderSettings.skybox.shader.name,Is.EqualTo("RydersRoad/World Sky"));
                foreach(float yaw in new[]{0f,90f,180f,270f})
                {
                    camera.transform.rotation=Quaternion.Euler(-12,yaw,0);
                    Capture("Logs/Production110QA/sky-"+id+"-"+yaw+".png");
                }
            }
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        private static void Jump(ParkourMotor motor,Vector3 from,Vector3 fromSize,Vector3 to,Vector3 toSize,string name)
        {
            var direction=to-from;direction.y=0;direction.Normalize();
            motor.ResetMotion(from+Vector3.up*(fromSize.y*.5f+.04f)-direction*.5f,Quaternion.LookRotation(direction),0);Physics.SyncTransforms();
            var input=new RunInput{FlowSteeringEnabled=motor.Profile.MovementMastery};for(int i=0;i<8;i++)motor.Simulate(input,1f/60);
            input.JumpPressed=true;motor.Simulate(input,1f/60);input.JumpPressed=false;bool air=false,landed=false;
            for(int i=0;i<110;i++){motor.Simulate(input,1f/60);air|=!motor.IsGrounded;if(air&&motor.IsGrounded){landed=true;break;}}
            Assert.That(landed,Is.True,name+" / "+motor.transform.position);
            var delta=motor.transform.position-to;
            Assert.That(Mathf.Abs(delta.x),Is.LessThan(toSize.x*.5f+.2f),name+" x");
            Assert.That(Mathf.Abs(delta.z),Is.LessThan(toSize.z*.5f+.2f),name+" z");
            Assert.That(Mathf.Abs(delta.y-toSize.y*.5f),Is.LessThan(.3f),name+" top");
        }
        [UnityTest] public IEnumerator FifthRoadUnlockBronzeAndReplay()
        {
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");yield return new WaitForSecondsRealtime(.2f);
            Click("CAMPAIGN Button");Click("MORE ROADS Button");yield return null;
            var card=Object.FindObjectsByType<Transform>().Single(t=>t.name=="Journey "+Id);
            var locked=card.GetComponentInChildren<Button>();Assert.That(locked.interactable,Is.False);locked.onClick.Invoke();
            Assert.That(Object.FindAnyObjectByType<DevelopmentModuleSelector>(),Is.Not.Null);
            foreach(var oldId in ModuleSelectionState.GetCampaignModuleIds().Take(4))
                ModuleProgressionData.RecordCompletion(save.Current.progression,oldId,999,"Bronze",1,System.Array.Empty<ModuleSplitRecord>(),true,1,1,1,"Uncalibrated","ValidUnassisted","2026-09-08T00:00:00Z");
            var oldRecords=JsonUtility.ToJson(save.Current.progression);
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");yield return new WaitForSecondsRealtime(.2f);
            Click("CAMPAIGN Button");Click("MORE ROADS Button");yield return null;
            card=Object.FindObjectsByType<Transform>().Single(t=>t.name=="Journey "+Id);
            Assert.That(card.GetComponentInChildren<Button>().interactable,Is.True);
            Assert.That(ModuleProgressionData.GetContinueModuleId(save.Current.progression,ModuleSelectionState.GetCampaignModuleIds()),Is.EqualTo(Id));
            Assert.That(JsonUtility.ToJson(save.Current.progression),Is.EqualTo(oldRecords),"Unlock queries preserve historical records");
            Click("CONTINUE Button");yield return new WaitForSecondsRealtime(.7f);
            var deadline=Time.realtimeSinceStartup+12;
            while(Object.FindAnyObjectByType<ModuleSceneController>()==null && Time.realtimeSinceStartup<deadline)yield return null;
            var controller=Object.FindAnyObjectByType<ModuleSceneController>();Assert.That(controller.ActiveModule.StableModuleId,Is.EqualTo(Id));
            var run=(ModuleRunSession)typeof(ModuleSceneController).GetField("_runSession",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(controller);
            run.Timer.Start();run.Timer.Tick(999);
            Assert.That(Object.FindAnyObjectByType<PatchBlock>().TryComplete(Object.FindAnyObjectByType<ParkourMotor>()),Is.True);
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(ModuleProgressionData.GetRecord(save.Current.progression,Id).bestRank,Is.EqualTo("Bronze"));
            Assert.That(ModuleSelectionState.GetNextCampaignModuleId(Id),Is.Null);
            Assert.That(Object.FindObjectsByType<Button>().Any(b=>b.name=="NEXT MODULE Button" && b.gameObject.activeInHierarchy),Is.False);
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");yield return new WaitForSecondsRealtime(.2f);
            Click("CAMPAIGN Button");Click("MORE ROADS Button");yield return null;
            Capture("Logs/Production110QA/campaign-fourth-road.png");
            Object.FindObjectsByType<Transform>().Single(t=>t.name=="Journey "+Id).GetComponentInChildren<Button>().onClick.Invoke();yield return new WaitForSecondsRealtime(.7f);
            deadline=Time.realtimeSinceStartup+12;
            while(Object.FindAnyObjectByType<ModuleSceneController>()==null && Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule.StableModuleId,Is.EqualTo(Id));
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        private static void Click(string name)=>Object.FindObjectsByType<Button>().Single(b=>b.name==name).onClick.Invoke();
        private static void Capture(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));var camera=Camera.main;
            var rt=new RenderTexture(1560,720,24);var image=new Texture2D(1560,720,TextureFormat.RGB24,false);
            var active=RenderTexture.active;var aspect=camera.aspect;
            var canvases=Object.FindObjectsByType<Canvas>().Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
            try
            {
                camera.targetTexture=rt;camera.aspect=1560f/720;
                foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}
                Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1560,720),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
            }
            finally
            {
                foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}
                camera.targetTexture=null;camera.aspect=aspect;RenderTexture.active=active;Object.Destroy(rt);Object.Destroy(image);
            }
        }
    }
}
