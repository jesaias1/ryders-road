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
    public sealed class SolarFoundryTests
    {
        private const string Id="module.004.solar-foundry";
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
        private sealed class RunInput : Avoidance.Input.IPlayerInputSource
        {
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
        [UnityTest] public IEnumerator BronzeSupportsJumpsAndFerryHaveTruthfulClearance()
        {
            yield return Open();
            var motor=Object.FindAnyObjectByType<ParkourMotor>();
            var module=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
            Assert.That(module.StableModuleId,Is.EqualTo(Id));Assert.That(motor.Profile.MovementMastery,Is.False);
            var moving=Object.FindAnyObjectByType<MovingBlock>();moving.enabled=false;
            foreach(var b in module.Blocks)
            foreach(float x in new[]{-.35f,0,.35f})foreach(float z in new[]{-.35f,0,.35f})
            {
                var top=b.Pose.Position+new Vector3(x*b.Size.x,.3f,z*b.Size.z);
                Assert.That(Physics.Raycast(top+Vector3.up*.1f,Vector3.down,out var hit,.2f,LayerMask.GetMask("Ground"),QueryTriggerInteraction.Ignore),Is.True,b.StableId);
                Assert.That(hit.point.y,Is.EqualTo(top.y).Within(.03f),b.StableId);
            }
            foreach(var surface in Object.FindObjectsByType<AuthoredSurface>())
                Assert.That(surface.HasMatchingCollision,Is.True,surface.CollisionDetails);
            var route=module.Blocks.Where(b=>!b.StableId.Contains("mastery")).ToArray();
            for(int i=0;i<route.Length-1;i++)
            {
                if(route[i].StableId=="m04.transfer.dock" || route[i].StableId=="m04.furnace.court")continue;
                Jump(motor,route[i].Pose.Position,route[i].Size,route[i+1].Pose.Position,route[i+1].Size,route[i+1].StableId);
            }
            var fins=module.CrumblingBlocks;
            var court=module.Blocks.Single(b=>b.StableId=="m04.furnace.court");
            var safe=module.Blocks.Single(b=>b.StableId=="m04.cooling.safe");
            Jump(motor,court.Pose.Position,court.Size,fins[0].Pose.Position,fins[0].Size,"first cooling fin");
            Jump(motor,fins[0].Pose.Position,fins[0].Size,fins[1].Pose.Position,fins[1].Size,"second cooling fin");
            Jump(motor,fins[1].Pose.Position,fins[1].Size,safe.Pose.Position,safe.Size,"cooling exit");
            var ferry=module.MovingBlocks.Single();
            foreach(var endpoint in new[]{0,1})
            {
                moving.transform.position=ferry.PathPoints[endpoint];
                if(moving.TryGetComponent<Rigidbody>(out var body))body.position=ferry.PathPoints[endpoint];
                Physics.SyncTransforms();
                var b=module.Blocks.Single(x=>x.StableId==(endpoint==0?"m04.transfer.dock":"m04.transfer.exit"));
                if(endpoint==0)Jump(motor,b.Pose.Position,b.Size,ferry.PathPoints[0],ferry.Size,"ferry board");
                else Jump(motor,ferry.PathPoints[1],ferry.Size,b.Pose.Position,b.Size,"ferry exit");
            }
            motor.ResetMotion(new Vector3(0,50,0),Quaternion.identity,0);Physics.SyncTransforms();
            for(int step=0;step<=50;step++)
            {
                var p=Vector3.Lerp(ferry.PathPoints[0],ferry.PathPoints[1],step/50f);
                // Body and a riding capsule swept together, including the rendered platform depth.
                var center=p+Vector3.up*.4f;var half=new Vector3(ferry.Size.x*.5f,1.6f,ferry.Size.z*.5f);
                var overlaps=Physics.OverlapBox(center,half,Quaternion.identity,LayerMask.GetMask("Ground"),QueryTriggerInteraction.Ignore)
                    .Where(c=>!c.transform.IsChildOf(moving.transform)).ToArray();
                Assert.That(overlaps,Is.Empty,"ferry step "+step+": "+string.Join(",",overlaps.Select(c=>c.name)));
            }
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        private static void Jump(ParkourMotor motor,Vector3 from,Vector3 fromSize,Vector3 to,Vector3 toSize,string name)
        {
            var direction=to-from;direction.y=0;direction.Normalize();
            motor.ResetMotion(from+Vector3.up*(fromSize.y*.5f+.04f)-direction*.5f,Quaternion.LookRotation(direction),0);Physics.SyncTransforms();
            var input=new RunInput();for(int i=0;i<8;i++)motor.Simulate(input,1f/60);
            input.JumpPressed=true;motor.Simulate(input,1f/60);input.JumpPressed=false;bool air=false,landed=false;
            for(int i=0;i<110;i++){motor.Simulate(input,1f/60);air|=!motor.IsGrounded;if(air&&motor.IsGrounded){landed=true;break;}}
            Assert.That(landed,Is.True,name+" / "+motor.transform.position);
            var delta=motor.transform.position-to;
            Assert.That(Mathf.Abs(delta.x),Is.LessThan(toSize.x*.5f+.2f),name+" x");
            Assert.That(Mathf.Abs(delta.z),Is.LessThan(toSize.z*.5f+.2f),name+" z");
            Assert.That(Mathf.Abs(delta.y-toSize.y*.5f),Is.LessThan(.3f),name+" top");
        }
        [UnityTest] public IEnumerator OptionalBracketsCarryAndRecoveryWorkWithProductionMotor()
        {
            yield return Open();var motor=Object.FindAnyObjectByType<ParkourMotor>();
            var module=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
            var moving=Object.FindAnyObjectByType<MovingBlock>();moving.ResetForModule();
            motor.ResetMotion(moving.transform.position+Vector3.up*.35f,Quaternion.identity,0);Physics.SyncTransforms();
            var idle=new RunInput{Move=Vector2.zero};
            for(int i=0;i<5;i++)motor.Simulate(idle,.02f);
            var offset=motor.transform.position-moving.transform.position;var origin=moving.transform.position;
            for(int i=0;i<90;i++){yield return new WaitForFixedUpdate();motor.Simulate(idle,.02f);}
            Assert.That(Vector3.Distance(origin,moving.transform.position),Is.GreaterThan(1));
            Assert.That(Vector3.Distance(offset,motor.transform.position-moving.transform.position),Is.LessThan(.35f));
            moving.enabled=false;
            var bracketRoute=new[]{module.Blocks.Single(b=>b.StableId=="m04.transfer.dock")}
                .Concat(module.Blocks.Where(b=>b.StableId.Contains("mastery")))
                .Concat(new[]{module.Blocks.Single(b=>b.StableId=="m04.transfer.exit")}).ToArray();
            for(int i=0;i<bracketRoute.Length-1;i++)
            {
                var from=bracketRoute[i];var to=bracketRoute[i+1];var direction=to.Pose.Position-from.Pose.Position;direction.y=0;direction.Normalize();
                // Deliberate edge takeoff for the optional narrow route, using ordinary production physics.
                float extent=Mathf.Min(from.Size.x*.5f/Mathf.Max(.001f,Mathf.Abs(direction.x)),from.Size.z*.5f/Mathf.Max(.001f,Mathf.Abs(direction.z)));
                Jump(motor,from.Pose.Position+direction*Mathf.Max(0,extent-.85f),from.Size,to.Pose.Position,to.Size,to.StableId);
            }
            var challenge=Object.FindAnyObjectByType<FlowChallenge>();Assert.That(challenge.Total,Is.EqualTo(2));
            motor.ResetMotion(Object.FindObjectsByType<FlowPickup>().First().transform.position,Quaternion.identity,0);Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();Assert.That(challenge.Count,Is.GreaterThan(0));
            int count=challenge.Count;
            foreach(var point in module.RestorePoints)
            {
                motor.ResetMotion(point.RestorePosition,Quaternion.identity,0);Physics.SyncTransforms();
                yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
                motor.ResetMotion(new Vector3(0,-40,0),Quaternion.identity,0);
                var restore=motor.GetComponent<RestoreController>();restore.Tick(false,1);
                Assert.That(Vector3.Distance(motor.transform.position,point.RestorePosition),Is.LessThan(.25f),point.StableId);
                Assert.That(challenge.Count,Is.EqualTo(count));restore.Tick(false,1);
            }
            var crumble=Object.FindObjectsByType<CrumblingBlock>().First();crumble.enabled=false;crumble.Activate(motor);crumble.Tick(1.04f);
            Assert.That(crumble.Phase,Is.EqualTo(CrumblingBlockPhase.Gone));Assert.That(crumble.SolidColliderEnabled,Is.False);
            crumble.ResetForModule();Assert.That(crumble.SolidColliderEnabled,Is.True);
            Assert.That(Object.FindAnyObjectByType<PatchBlock>().TryComplete(motor),Is.True);
            yield return new WaitForSecondsRealtime(.3f);
            Object.FindObjectsByType<Button>().Single(b=>b.name=="RETRY Button").onClick.Invoke();
            var deadline=Time.realtimeSinceStartup+12;yield return null;
            while((UnitySceneLevelLoader.ActiveRequest==null || !UnitySceneLevelLoader.ActiveRequest.IsTerminal) && Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(Object.FindAnyObjectByType<FlowChallenge>().Count,Is.Zero);
            Assert.That(Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule.StableModuleId,Is.EqualTo(Id));
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        [UnityTest] public IEnumerator RenderFiveAuthoredPlaces()
        {
            yield return Open();
            var module=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
            var camera=Camera.main;
            foreach(var component in camera.GetComponents<MonoBehaviour>())component.enabled=false;
            string[] ids={"m04.intake.start","m04.intake.gantry","m04.transfer.dock","m04.furnace.court","m04.crown.restore"};
            Vector3[] targets={new Vector3(0,0,10),new Vector3(8,2,24),new Vector3(16,3,36),new Vector3(15,6,69),new Vector3(-5,10,103)};
            for(int i=0;i<ids.Length;i++)
            {
                var b=module.Blocks.Single(x=>x.StableId==ids[i]);camera.transform.position=b.Pose.Position+Vector3.up*1.92f;
                var direction=targets[i]-camera.transform.position;direction.y=0;
                camera.transform.rotation=Quaternion.LookRotation(direction)*Quaternion.Euler(9,0,0);
                yield return null;Capture("Logs/Production098QA/view-"+i+".png");
            }
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        [UnityTest] public IEnumerator FourthRoadPaginationLegacyUnlockBronzeAndReplay()
        {
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");yield return new WaitForSecondsRealtime(.2f);
            Click("CAMPAIGN Button");Click("MORE ROADS Button");yield return null;
            var card=Object.FindObjectsByType<Transform>().Single(t=>t.name=="Journey "+Id);
            var locked=card.GetComponentInChildren<Button>();Assert.That(locked.interactable,Is.False);locked.onClick.Invoke();
            Assert.That(Object.FindAnyObjectByType<DevelopmentModuleSelector>(),Is.Not.Null);
            foreach(var oldId in ModuleSelectionState.GetCampaignModuleIds().Take(3))
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
            Capture("Logs/Production098QA/campaign-fourth-road.png");
            Click("RUN AGAIN Button");yield return new WaitForSecondsRealtime(.7f);
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
