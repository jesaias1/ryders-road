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
            var standard=Quality130Tests.WindwardRoute(module);
            for(int i=0;i<standard.Length-1;i++)Jump(motor,standard[i].Pose.Position,standard[i].Size,standard[i+1].Pose.Position,standard[i+1].Size,standard[i+1].StableId);
            var skill=new[]{module.Blocks.Single(b=>b.StableId=="m05.west.restore")}.Concat(module.Blocks.Where(b=>b.StableId.Contains("skill"))).Concat(new[]{module.Blocks.Single(b=>b.StableId=="m05.east.restore")}).ToArray();
            for(int i=0;i<skill.Length-1;i++)
            {
                var from=skill[i];var to=skill[i+1];
                var direction=to.Pose.Position-from.Pose.Position;direction.y=0;direction.Normalize();
                Jump(motor,from.Pose.Position,from.Size,to.Pose.Position,to.Size,to.StableId);
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
        [UnityTest] public IEnumerator WindwardStandardRouteContinuousStopAndGoRun()
        {
            yield return Open();
            var motor=Object.FindAnyObjectByType<ParkourMotor>();
            var route=Quality130Tests.WindwardRoute(Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule);
            motor.ResetMotion(route[0].Pose.Position+Vector3.up*.34f,Quaternion.identity,0);Physics.SyncTransforms();
            var input=new RunInput();
            for(int link=0;link<route.Length-1;link++)
            {
                input.Move=Vector2.zero;
                for(int i=0;i<20;i++)motor.Simulate(input,1f/60);
                var from=route[link];var to=route[link+1];
                var direction=to.Pose.Position-motor.transform.position;direction.y=0;direction.Normalize();
                motor.transform.rotation=Quaternion.LookRotation(direction);Physics.SyncTransforms();
                input.Move=new Vector2(0,.9f);
                float Remaining()
                {
                    var local=motor.transform.position-from.Pose.Position;
                    float x=Mathf.Abs(direction.x)>.001f?(from.Size.x*.5f-Mathf.Sign(direction.x)*local.x)/Mathf.Abs(direction.x):100;
                    float z=Mathf.Abs(direction.z)>.001f?(from.Size.z*.5f-Mathf.Sign(direction.z)*local.z)/Mathf.Abs(direction.z):100;
                    return Mathf.Min(x,z);
                }
                for(int i=0;i<90 && Remaining()>.7f;i++)motor.Simulate(input,1f/60);
                input.JumpPressed=true;motor.Simulate(input,1f/60);input.JumpPressed=false;
                bool airborne=false,landed=false;
                for(int i=0;i<100;i++)
                {
                    motor.Simulate(input,1f/60);airborne|=!motor.IsGrounded;
                    if(airborne&&motor.IsGrounded){landed=true;break;}
                }
                var delta=motor.transform.position-to.Pose.Position;
                Assert.That(landed && Mathf.Abs(delta.y-.3f)<.3f && Mathf.Abs(delta.x)<to.Size.x*.5f+.2f && Mathf.Abs(delta.z)<to.Size.z*.5f+.2f,Is.True,
                    from.StableId+" -> "+to.StableId+" at "+motor.transform.position);
                yield return null;
            }
            Assert.That(Object.FindAnyObjectByType<PatchBlock>().TryComplete(motor),Is.True);
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        [UnityTest] public IEnumerator WindwardCannotWalkTheStandardParkourLinks()
        {
            yield return Open();
            var motor=Object.FindAnyObjectByType<ParkourMotor>();
            var route=Quality130Tests.WindwardRoute(Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule);
            for(int i=0;i<route.Length-1;i++)
            {
                var a=route[i];var b=route[i+1];var direction=b.Pose.Position-a.Pose.Position;direction.y=0;
                motor.ResetMotion(a.Pose.Position+Vector3.up*.34f,Quaternion.LookRotation(direction),0);Physics.SyncTransforms();
                bool arrived=false;var input=new RunInput{Move=Vector2.up};
                for(int frame=0;frame<120;frame++)
                {
                    motor.Simulate(input,1f/60);var d=motor.transform.position-b.Pose.Position;
                    arrived|=motor.IsGrounded && Mathf.Abs(d.y-.3f)<.25f && Mathf.Abs(d.x)<b.Size.x*.5f && Mathf.Abs(d.z)<b.Size.z*.5f;
                }
                Assert.That(arrived,Is.False,a.StableId+" -> "+b.StableId+" must require a jump");
            }
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        [UnityTest] public IEnumerator FrontendAndLiveLoadingUseGameIdentityAndSeparateActions()
        {
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");yield return new WaitForSecondsRealtime(.25f);
            Assert.That(Object.FindObjectsByType<Button>().Any(b=>b.name=="CAMPAIGN FLOW TRIAL Button"),Is.False);
            Capture("Logs/Production120QA/title.png");
            Click("CAMPAIGN Button");yield return null;Capture("Logs/Production120QA/journey.png");
            var next=Object.FindObjectsByType<Button>().Single(b=>b.name=="CONTINUE Button").GetComponent<RectTransform>();
            var more=Object.FindObjectsByType<Button>().Single(b=>b.name=="MORE ROADS Button").GetComponent<RectTransform>();
            Assert.That(more.anchorMax.x,Is.LessThan(next.anchorMin.x));
            ModuleSelectionState.Select(Id,true);
            var request=SceneTransitionHost.Instance.Begin("ModuleRunner");
            var loading=Object.FindAnyObjectByType<LoadingPresentation>();
            Assert.That(loading.GetComponentInChildren<RawImage>().texture,Is.EqualTo(Resources.Load<Texture2D>(BrandPresentation.LogoResourcePath)));
            var traveller=loading.GetComponentsInChildren<RectTransform>().Single(t=>t.name=="Loading Traveller");
            float first=traveller.anchoredPosition.x;bool moved=false;int frames=0;
            while(!request.IsTerminal && frames<600)
            {
                yield return null;frames++;moved|=Mathf.Abs(traveller.anchoredPosition.x-first)>.5f;
                if((frames==2 || frames==5) && Camera.main!=null)Capture("Logs/Production120QA/loading-"+frames+".png");
            }
            Assert.That(request.State,Is.EqualTo(SceneLoadState.Ready));Assert.That(moved,Is.True,"Indicator must visibly move across real construction frames");
            yield return new WaitForSecondsRealtime(.25f);
            Assert.That(loading.GetComponent<CanvasGroup>().blocksRaycasts,Is.False);
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
                    yield return null;Capture("Logs/Production120QA/"+id+"-"+i+".png");
                }
                if(id=="module.002.moving-parts")
                {
                    camera.transform.position=new Vector3(4,5,26);
                    camera.transform.LookAt(new Vector3(-62,1,51));Capture("Logs/Production120QA/waterfall-player.png");
                    camera.transform.position=new Vector3(10,11,73);
                    camera.transform.LookAt(new Vector3(-62,-2,51));Capture("Logs/Production120QA/waterfall-ridge.png");
                }
                Assert.That(RenderSettings.skybox.shader.name,Is.EqualTo("RydersRoad/World Sky"));
                foreach(float yaw in new[]{0f,90f,180f,270f})
                {
                    camera.transform.rotation=Quaternion.Euler(-12,yaw,0);
                    Capture("Logs/Production120QA/sky-"+id+"-"+yaw+".png");
                }
            }
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        private static void Jump(ParkourMotor motor,Vector3 from,Vector3 fromSize,Vector3 to,Vector3 toSize,string name)
        {
            var direction=to-from;direction.y=0;direction.Normalize();
            motor.ResetMotion(from+Vector3.up*(fromSize.y*.5f+.04f),Quaternion.LookRotation(direction),0);Physics.SyncTransforms();
            var input=new RunInput{FlowSteeringEnabled=motor.Profile.MovementMastery};
            float edge=Mathf.Min(Mathf.Abs(direction.x)>.001f?fromSize.x*.5f/Mathf.Abs(direction.x):100,Mathf.Abs(direction.z)>.001f?fromSize.z*.5f/Mathf.Abs(direction.z):100);
            for(int i=0;i<60 && Vector3.Dot(motor.transform.position-from,direction)<edge-(name.Contains("skill")?.35f:.7f);i++)motor.Simulate(input,1f/60);
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
            Assert.That(Object.FindObjectsByType<Text>().Any(t=>t.text.Contains("FIVE ROADS RESTORED")),Is.True);
            Capture("Logs/Production120QA/finale.png");
            Assert.That(Object.FindObjectsByType<Button>().Any(b=>b.name=="NEXT MODULE Button" && b.gameObject.activeInHierarchy),Is.False);
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");yield return new WaitForSecondsRealtime(.2f);
            Click("CAMPAIGN Button");Click("MORE ROADS Button");yield return null;
            Capture("Logs/Production120QA/campaign-fourth-road.png");
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
