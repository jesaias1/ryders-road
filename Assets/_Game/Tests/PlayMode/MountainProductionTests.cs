using System.Collections;
using Avoidance.Core.Services;
using Avoidance.SaveSystem;
using Avoidance.Gameplay.Timing;
using Avoidance.Gameplay.Blocks;
using System.IO;
using System.Linq;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Avoidance.Tests.PlayMode
{
    public sealed class MountainProductionTests
    {
        private ServiceContainer priorServices;
        private ServiceContainer testServices;
        private JsonSaveService save;
        [SetUp] public void IsolateSave()
        {
            priorServices=GameServices.Current;testServices=new ServiceContainer();
            save=new JsonSaveService(new MemoryStore(),"0.9.4-test");save.Initialize();testServices.Register<ISaveService>(save);GameServices.Publish(testServices);
        }
        [TearDown] public void RestoreServices()
        {
            if(priorServices!=null)GameServices.Publish(priorServices);else GameServices.Clear(testServices);
        }
        private sealed class MemoryStore : ISaveFileStore
        {
            private string json;public bool PrimaryExists=>json!=null;public bool BackupExists=>false;
            public bool TryReadPrimary(out string value){value=json;return value!=null;}
            public bool TryReadBackup(out string value){value=null;return false;}
            public void WriteAtomic(string value,bool preserveBackup=false){json=value;}
            public void DeleteAll(){json=null;}
        }
        [UnityTest] public IEnumerator FreshCampaignBronzeNextReplayAndContinueUseRealButtons()
        {
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");yield return new WaitForSecondsRealtime(.3f);
            Click("CAMPAIGN Button");
            var cards=Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(x=>x.name.StartsWith("Journey module.")).OrderBy(x=>x.name).ToArray();
            Assert.That(cards.Length,Is.EqualTo(3));Assert.That(cards[0].GetComponentsInChildren<Button>().Count(x=>x.interactable),Is.EqualTo(1));
            for(int i=1;i<3;i++)
            {
                Assert.That(cards[i].GetComponentsInChildren<Button>().Any(x=>x.interactable),Is.False);
                foreach(var button in cards[i].GetComponentsInChildren<Button>())button.onClick.Invoke();
            }
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("ModuleSelector"));
            Assert.That(Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Any(x=>x.name.Contains("QA PRACTICE")),Is.False);
            CaptureNow("Logs/Phase094VisualQA/06-fresh-campaign.png");
            Click("CONTINUE Button");yield return Ready("module.001.first-steps");
            var controller=Object.FindAnyObjectByType<ModuleSceneController>();
            var session=(ModuleRunSession)typeof(ModuleSceneController).GetField("_runSession",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(controller);
            session.Timer.Start();session.Timer.Tick(999);
            Assert.That(Object.FindAnyObjectByType<PatchBlock>().TryComplete(Object.FindAnyObjectByType<ParkourMotor>()),Is.True);
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(save.Current.progression.modules.Single(x=>x.moduleId=="module.001.first-steps").bestRank,Is.EqualTo("Bronze"));
            Assert.That(Object.FindObjectsByType<Text>(FindObjectsSortMode.None).Any(x=>x.text.Contains("NEXT ROAD UNLOCKED")),Is.True);
            CaptureNow("Logs/Phase094VisualQA/07-bronze-results.png");
            Click("NEXT MODULE Button");yield return Ready("module.002.moving-parts");
            save.Reload();Assert.That(ModuleProgressionData.GetContinueModuleId(save.Current.progression,ModuleSelectionState.GetCampaignModuleIds()),Is.EqualTo("module.002.moving-parts"));
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");yield return new WaitForSecondsRealtime(.3f);Click("CAMPAIGN Button");
            var first=Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Single(x=>x.name=="Journey module.001.first-steps");first.GetComponentsInChildren<Button>().Single(x=>x.interactable).onClick.Invoke();
            yield return Ready("module.001.first-steps");
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");yield return new WaitForSecondsRealtime(.3f);Click("CAMPAIGN Button");Click("CONTINUE Button");yield return Ready("module.002.moving-parts");
        }
        private static void Click(string name)=>Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(x=>x.name==name).onClick.Invoke();
        private static IEnumerator Ready(string id)
        {
            var deadline=Time.realtimeSinceStartup+12;
            while(Time.realtimeSinceStartup<deadline)
            {
                var c=Object.FindAnyObjectByType<ModuleSceneController>();var loading=Object.FindAnyObjectByType<LoadingPresentation>();
                if(c!=null&&c.ActiveModule.StableModuleId==id&&loading.GetComponent<CanvasGroup>().alpha==0)break;
                yield return null;
            }
            Assert.That(Object.FindAnyObjectByType<ModuleSceneController>()?.ActiveModule.StableModuleId,Is.EqualTo(id));
            Assert.That(Object.FindAnyObjectByType<LoadingPresentation>().GetComponent<CanvasGroup>().blocksRaycasts,Is.False);
            Assert.That(Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled,Is.True);
        }
        [UnityTest] public IEnumerator OrdinaryMountainJumpsAndMovingDocksHaveTruthfulSupport()
        {
            ModuleSelectionState.Select("module.002.moving-parts",true);yield return new UnitySceneLevelLoader().LoadAsync("ModuleRunner");yield return new WaitForSecondsRealtime(.3f);
            Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled=false;
            var motor=Object.FindAnyObjectByType<ParkourMotor>();var m=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
            foreach(var moving in Object.FindObjectsByType<MovingBlock>(FindObjectsSortMode.None))moving.enabled=false;
            Physics.SyncTransforms();
            Assert.That(Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Any(r=>r.isPartOfStaticBatch),Is.True,"Mountain geometry is combined at scene setup");
            foreach(var restore in m.RestorePoints)Assert.That(GameObject.Find(restore.StableId+".platform"),Is.Null,"Uses authored support without duplicate floor");
            Assert.That(GameObject.Find(m.PatchBlock.StableId+".platform"),Is.Null);
            foreach(var b in m.Blocks)foreach(var x in new[]{-.35f,0,.35f})foreach(var z in new[]{-.35f,0,.35f})
            {
                var top=b.Pose.Position+new Vector3(x*b.Size.x,b.Size.y*.5f,z*b.Size.z);
                Assert.That(Physics.Raycast(top+Vector3.up*.15f,Vector3.down,out var hit,.3f,LayerMask.GetMask("Ground"),QueryTriggerInteraction.Ignore),Is.True,b.StableId);
                Assert.That(hit.point.y,Is.EqualTo(top.y).Within(.035f),b.StableId);
            }
            foreach(var surface in Object.FindObjectsByType<Avoidance.Gameplay.Worlds.AuthoredSurface>(FindObjectsSortMode.None))Assert.That(surface.HasMatchingCollision,Is.True,surface.CollisionDetails);
            var route=m.Blocks.TakeWhile(b=>b.StableId!="m02.risky.01").ToArray();
            for(int i=0;i<route.Length-1;i++)
            {
                if(route[i].StableId=="m02.rhythm.02"||route[i].StableId=="m02.rejoin")continue;
                Jump(motor,route[i].Pose.Position,route[i].Size,route[i+1].Pose.Position,route[i+1].Size,route[i].StableId+" -> "+route[i+1].StableId);
            }
            foreach(var definition in m.MovingBlocks)
            {
                var moving=Object.FindObjectsByType<MovingBlock>(FindObjectsSortMode.None).Single(x=>x.name==definition.StableId);
                var from=m.Blocks.Single(b=>b.StableId==(definition.StableId=="moving.m02.first"?"m02.rhythm.02":"m02.rejoin"));
                var to=m.Blocks.Single(b=>b.StableId==(definition.StableId=="moving.m02.first"?"m02.after-moving.01":"m02.lift.exit"));
                moving.transform.position=definition.PathPoints[0];if(moving.TryGetComponent<Rigidbody>(out var body))body.position=definition.PathPoints[0];Physics.SyncTransforms();
                Jump(motor,from.Pose.Position,from.Size,definition.PathPoints[0],definition.Size,definition.StableId+" boarding");
                moving.transform.position=definition.PathPoints[1];if(body!=null)body.position=definition.PathPoints[1];Physics.SyncTransforms();
                Jump(motor,definition.PathPoints[1],definition.Size,to.Pose.Position,to.Size,definition.StableId+" exit");
            }
        }
        [UnityTest] public IEnumerator LiveLiftCarryRestoresShardsAndRetryRemainIndependent()
        {
            ModuleSelectionState.Select("module.002.moving-parts");yield return new UnitySceneLevelLoader().LoadAsync("ModuleRunner");yield return new WaitForSecondsRealtime(.3f);
            var motor=Object.FindAnyObjectByType<ParkourMotor>();var coordinator=Object.FindAnyObjectByType<PlayerRuntimeCoordinator>();coordinator.enabled=false;
            var module=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
            var input=new IdleInput();
            foreach(var moving in Object.FindObjectsByType<MovingBlock>(FindObjectsSortMode.None))
            {
                moving.ResetForModule();motor.ResetMotion(moving.transform.position+Vector3.up*.35f,Quaternion.identity,0);Physics.SyncTransforms();
                for(int i=0;i<5;i++)motor.Simulate(input,.02f);
                var relative=motor.transform.position-moving.transform.position;var origin=moving.transform.position;
                for(int frame=0;frame<80;frame++){yield return new WaitForFixedUpdate();motor.Simulate(input,.02f);}
                Assert.That(Vector3.Distance(origin,moving.transform.position),Is.GreaterThan(1),moving.name+" actually moves");
                Assert.That(Vector3.Distance(motor.transform.position-moving.transform.position,relative),Is.LessThan(.35f),moving.name+" carries capsule");
            }
            var challenge=Object.FindAnyObjectByType<FlowChallenge>();Assert.That(challenge.Total,Is.EqualTo(2));
            var pickup=Object.FindObjectsByType<FlowPickup>(FindObjectsSortMode.None).First();
            motor.ResetMotion(pickup.transform.position,Quaternion.identity,0);Physics.SyncTransforms();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
            Assert.That(challenge.Count,Is.EqualTo(1));
            foreach(var restorePoint in module.RestorePoints)
            {
                motor.ResetMotion(restorePoint.RestorePosition,Quaternion.identity,0);Physics.SyncTransforms();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
                motor.ResetMotion(new Vector3(0,-30,0),Quaternion.identity,0);
                var restore=motor.GetComponent<Avoidance.Gameplay.Respawn.RestoreController>();restore.Tick(false,.2f);
                Assert.That(Vector3.Distance(motor.transform.position,restorePoint.RestorePosition),Is.LessThan(.2f),restorePoint.StableId);
                Assert.That(challenge.Count,Is.EqualTo(1),"Restore retains attempt shards");
                restore.Tick(false,1);
            }
            Assert.That(Object.FindAnyObjectByType<PatchBlock>().TryComplete(motor),Is.True);yield return new WaitForSecondsRealtime(.3f);
            Click("RETRY Button");yield return Ready("module.002.moving-parts");
            Assert.That(Object.FindAnyObjectByType<FlowChallenge>().Count,Is.Zero);
            Assert.That(Vector3.Distance(Object.FindAnyObjectByType<ParkourMotor>().transform.position,module.StartPoint.Position),Is.LessThan(.3f));
        }
        [UnityTest] public IEnumerator MovingSweepsDoNotIntersectWorldGeometry()
        {
            ModuleSelectionState.Select("module.002.moving-parts",true);
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleRunner");
            yield return new WaitForSecondsRealtime(.3f);
            Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled=false;
            var module=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
            foreach(var moving in Object.FindObjectsByType<MovingBlock>(FindObjectsSortMode.None))moving.enabled=false;
            Physics.SyncTransforms();
            var failures=new System.Collections.Generic.HashSet<string>();
            foreach(var def in module.MovingBlocks)
            for(int segment=0;segment<def.PathPoints.Count-1;segment++)
            for(int step=0;step<=40;step++)
            {
                var center=Vector3.Lerp(def.PathPoints[segment],def.PathPoints[segment+1],step/40f);
                var sweep=new Bounds(center,def.Size-Vector3.one*.05f);
                foreach(var block in module.Blocks)
                foreach(var renderer in GameObject.Find(block.StableId).GetComponentsInChildren<Renderer>())
                    if(renderer.enabled&&sweep.Intersects(renderer.bounds))
                        failures.Add(def.StableId+" intersects visible body "+block.StableId);
                foreach(var hit in Physics.OverlapBox(center,def.Size*.5f-Vector3.one*.025f,Quaternion.identity,LayerMask.GetMask("Ground"),QueryTriggerInteraction.Ignore))
                {
                    if(hit.GetComponentInParent<MovingBlock>()!=null)continue;
                    failures.Add(def.StableId+" intersects "+hit.transform.parent?.name+"/"+hit.name);
                }
            }
            foreach(var block in module.Blocks)
            foreach(var hit in Physics.OverlapBox(block.Pose.Position,block.Size*.5f-Vector3.one*.025f,Quaternion.identity,LayerMask.GetMask("Ground"),QueryTriggerInteraction.Ignore))
            {
                if(hit.name==block.StableId||hit.GetComponentInParent<MovingBlock>()!=null)continue;
                failures.Add(block.StableId+" intersects "+hit.transform.parent?.name+"/"+hit.name);
            }
            Directory.CreateDirectory("Logs/Phase095VisualQA");
            File.WriteAllLines("Logs/Phase095VisualQA/moving-clearance.txt",failures);
            Assert.That(failures,Is.Empty,string.Join("\n",failures));
        }
        [UnityTest] public IEnumerator AncientAbyssSweepsAndBlocksDoNotIntersectWorldGeometry()
        {
            ModuleSelectionState.Select("module.003.flow-error",true);
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleRunner");
            yield return new WaitForSecondsRealtime(.3f);
            Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled=false;
            var module=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
            foreach(var moving in Object.FindObjectsByType<MovingBlock>(FindObjectsSortMode.None))moving.enabled=false;
            Physics.SyncTransforms();
            var failures=new System.Collections.Generic.HashSet<string>();
            foreach(var def in module.MovingBlocks)
            for(int segment=0;segment<def.PathPoints.Count-1;segment++)
            for(int step=0;step<=40;step++)
            {
                var center=Vector3.Lerp(def.PathPoints[segment],def.PathPoints[segment+1],step/40f);
                var sweep=new Bounds(center,def.Size-Vector3.one*.05f);
                foreach(var block in module.Blocks)
                foreach(var renderer in GameObject.Find(block.StableId).GetComponentsInChildren<Renderer>())
                    if(renderer.enabled&&sweep.Intersects(renderer.bounds))
                        failures.Add(def.StableId+" intersects visible body "+block.StableId);
                foreach(var hit in Physics.OverlapBox(center,def.Size*.5f-Vector3.one*.025f,Quaternion.identity,LayerMask.GetMask("Ground"),QueryTriggerInteraction.Ignore))
                {
                    if(hit.GetComponentInParent<MovingBlock>()!=null)continue;
                    failures.Add(def.StableId+" intersects "+hit.transform.parent?.name+"/"+hit.name);
                }
            }
            foreach(var block in module.Blocks)
            foreach(var hit in Physics.OverlapBox(block.Pose.Position,block.Size*.5f-Vector3.one*.025f,Quaternion.identity,LayerMask.GetMask("Ground"),QueryTriggerInteraction.Ignore))
            {
                if(hit.name==block.StableId||hit.GetComponentInParent<MovingBlock>()!=null)continue;
                failures.Add(block.StableId+" intersects "+hit.transform.parent?.name+"/"+hit.name);
            }
            Directory.CreateDirectory("Logs/Phase095VisualQA");
            File.WriteAllLines("Logs/Phase095VisualQA/ancient-clearance.txt",failures);
            Assert.That(failures,Is.Empty,string.Join("\n",failures));
        }
        [UnityTest] public IEnumerator StartupPosterIsVisibleBeforeLoadEventAndJourneyIsCentered()
        {
            var old=Object.FindAnyObjectByType<LoadingPresentation>();
            if(old!=null)Object.Destroy(old.gameObject);
            yield return null;
            var presentation=new GameObject("Startup Test Presentation").AddComponent<LoadingPresentation>();
            Assert.That(presentation.GetComponent<CanvasGroup>().alpha,Is.EqualTo(1));
            Assert.That(presentation.GetComponent<CanvasGroup>().blocksRaycasts,Is.True);
            Assert.That(presentation.GetComponentInChildren<RawImage>().texture,Is.EqualTo(Resources.Load<Texture2D>(BrandPresentation.LogoResourcePath)));
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(presentation.GetComponent<CanvasGroup>().alpha,Is.Zero);
            CaptureNow("Logs/Phase095VisualQA/01-main-menu.png");
            Click("CAMPAIGN Button");
            var journey=GameObject.Find("Campaign Journey").GetComponent<RectTransform>();
            Assert.That((journey.anchorMin+journey.anchorMax)*.5f,Is.EqualTo(new Vector2(.5f,.5f)));
            CaptureNow("Logs/Phase095VisualQA/02-campaign.png");
        }
        [UnityTest] public IEnumerator RevisedCrossingDocksAndOptionalLinesRemainReachable()
        {
            foreach(var id in new[]{"module.002.moving-parts","module.003.flow-error"})
            {
                ModuleSelectionState.Select(id,true);yield return new UnitySceneLevelLoader().LoadAsync("ModuleRunner");
                yield return new WaitForSecondsRealtime(.3f);
                Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled=false;
                var motor=Object.FindAnyObjectByType<ParkourMotor>();
                var module=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
                foreach(var moving in Object.FindObjectsByType<MovingBlock>(FindObjectsSortMode.None))moving.enabled=false;
                var ids=id.Contains("002")?new[]{"m02.rejoin","m02.mastery.lift-cut","m02.lift.exit"}
                    :new[]{"m03.moving-setup","m03.mastery.moving-bypass.01","m03.mastery.moving-bypass.02","m03.mastery.moving-bypass.03","m03.motion-exit"};
                for(int i=0;i<ids.Length-1;i++)
                {
                    var from=module.Blocks.Single(b=>b.StableId==ids[i]);var to=module.Blocks.Single(b=>b.StableId==ids[i+1]);
                    if(Vector3.ProjectOnPlane(to.Pose.Position-from.Pose.Position,Vector3.up).magnitude<5.6f)
                        Jump(motor,from.Pose.Position,from.Size,to.Pose.Position,to.Size,ids[i]+" -> "+ids[i+1]);
                    else EdgeJump(motor,from.Pose.Position,from.Size,to.Pose.Position,to.Size,ids[i]+" -> "+ids[i+1]);
                }
                if(id.Contains("003"))
                {
                    var def=module.MovingBlocks.Single();var moving=Object.FindAnyObjectByType<MovingBlock>();
                    var from=module.Blocks.Single(b=>b.StableId=="m03.moving-setup");var to=module.Blocks.Single(b=>b.StableId=="m03.motion-exit");
                    moving.transform.position=def.PathPoints[0];if(moving.TryGetComponent<Rigidbody>(out var body))body.position=def.PathPoints[0];Physics.SyncTransforms();
                    Jump(motor,from.Pose.Position,from.Size,def.PathPoints[0],def.Size,"003 ferry boarding");
                    moving.transform.position=def.PathPoints[1];if(body!=null)body.position=def.PathPoints[1];Physics.SyncTransforms();
                    Jump(motor,def.PathPoints[1],def.Size,to.Pose.Position,to.Size,"003 ferry exit");
                }
            }
        }
        private static void EdgeJump(ParkourMotor motor,Vector3 from,Vector3 size,Vector3 to,Vector3 toSize,string label)
        {
            var dir=Vector3.ProjectOnPlane(to-from,Vector3.up).normalized;
            var edge=Mathf.Min(Mathf.Abs(dir.x)>.001f?size.x*.5f/Mathf.Abs(dir.x):100,Mathf.Abs(dir.z)>.001f?size.z*.5f/Mathf.Abs(dir.z):100);
            motor.ResetMotion(from+dir*(edge-1.15f)+Vector3.up*(size.y*.5f+.04f),Quaternion.LookRotation(dir),0);Physics.SyncTransforms();
            var input=new FastInput();for(int frame=0;frame<12;frame++)motor.Simulate(input,1f/60);
            Assert.That(motor.IsGrounded,Is.True,label+" runup");
            input.JumpPressed=true;motor.Simulate(input,1f/60);input.JumpPressed=false;bool airborne=false;
            for(int frame=0;frame<110;frame++){motor.Simulate(input,1f/60);airborne|=!motor.IsGrounded;if(airborne&&motor.IsGrounded)break;}
            Directory.CreateDirectory("Logs/Phase095VisualQA");File.AppendAllText("Logs/Phase095VisualQA/revised-jumps.txt",label+": "+motor.transform.position+" support="+motor.GroundTransform?.name+"\n");
            Assert.That(motor.IsGrounded,Is.True,label);var delta=motor.transform.position-to;
            Assert.That(Mathf.Abs(delta.x),Is.LessThan(toSize.x*.5f+.2f),label+" x");
            Assert.That(Mathf.Abs(delta.z),Is.LessThan(toSize.z*.5f+.2f),label+" z");
            Assert.That(Mathf.Abs(delta.y-toSize.y*.5f),Is.LessThan(.3f),label+" y");
        }
        private sealed class FastInput : Avoidance.Input.IPlayerInputSource
        {
            public Vector2 Move=>Vector2.up;public Vector2 LookDelta=>Vector2.zero;public bool JumpPressed{get;set;}public void ResetState(){}
        }
        private sealed class IdleInput : Avoidance.Input.IPlayerInputSource
        {
            public Vector2 Move=>Vector2.zero;public Vector2 LookDelta=>Vector2.zero;public bool JumpPressed=>false;public void ResetState(){}
        }
        private sealed class RouteInput : Avoidance.Input.IPlayerInputSource
        {
            public Vector2 Move=>new Vector2(0,.9f);public Vector2 LookDelta=>Vector2.zero;public bool JumpPressed{get;set;}public void ResetState(){JumpPressed=false;}
        }
        private static void Jump(ParkourMotor motor,Vector3 from,Vector3 fromSize,Vector3 to,Vector3 toSize,string label)
        {
            var direction=to-from;direction.y=0;direction.Normalize();
            motor.ResetMotion(from+Vector3.up*(fromSize.y*.5f+.04f)-direction*.5f,Quaternion.LookRotation(direction),0);Physics.SyncTransforms();
            var input=new RouteInput();for(int f=0;f<8;f++)motor.Simulate(input,1f/60);
            input.JumpPressed=true;motor.Simulate(input,1f/60);input.JumpPressed=false;bool air=false,landed=false;
            for(int f=0;f<110;f++){motor.Simulate(input,1f/60);air|=!motor.IsGrounded;if(air&&motor.IsGrounded){landed=true;break;}}
            Directory.CreateDirectory("Logs/Phase094VisualQA");File.AppendAllText("Logs/Phase094VisualQA/jumps.txt",$"{label}: {motor.transform.position} ground={motor.GroundTransform?.name} speed={motor.LastTakeoffHorizontalSpeed}\n");
            Assert.That(landed,Is.True,label+" fatal="+motor.GetComponent<Avoidance.Gameplay.Respawn.SceneryLandingRecovery>()?.LastFatalCollider?.transform.parent?.name+"/"+motor.GetComponent<Avoidance.Gameplay.Respawn.SceneryLandingRecovery>()?.LastFatalCollider?.name+" at "+motor.transform.position);var delta=motor.transform.position-to;
            Assert.That(Mathf.Abs(delta.x),Is.LessThan(toSize.x*.5f+.2f),label+" x");Assert.That(Mathf.Abs(delta.z),Is.LessThan(toSize.z*.5f+.2f),label+" z");
            Assert.That(Mathf.Abs(motor.transform.position.y-(to.y+toSize.y*.5f)),Is.LessThan(.3f),label+" landing height (ground probe clearance)");
        }
        [UnityTest]
        public IEnumerator CaptureFiveMountainGameplayViews()
        {
            ModuleProgressionData.RecordCompletion(save.Current.progression,"module.001.first-steps",999,"Bronze",1,System.Array.Empty<ModuleSplitRecord>(),true,2,1,1,"Uncalibrated","ValidUnassisted","2026-09-06T00:00:00Z");
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");yield return new WaitForSecondsRealtime(.3f);
            Click("CAMPAIGN Button");Click("CONTINUE Button");yield return Ready("module.002.moving-parts");
            yield return new WaitForSecondsRealtime(.5f);
            var module = Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
            Assert.That(module.StableModuleId, Is.EqualTo("module.002.moving-parts"));
            Assert.That(Object.FindAnyObjectByType<LoadingPresentation>().GetComponent<CanvasGroup>().blocksRaycasts, Is.False);
            Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled = false;
            var camera = Camera.main;
            var spawnCameraPosition = camera.transform.position;
            foreach (var behavior in camera.GetComponents<MonoBehaviour>()) behavior.enabled = false;
            var folder = "Logs/Phase094VisualQA/" + (module.ContentVersion == 1 ? "before" : "after");
            Directory.CreateDirectory(folder);
            var ids = new[] { "m02.start", "m02.rhythm.02", "m02.restore.platform", "m02.rejoin", "m02.final-runup" };
            var names = new[] { "01-opening", "02-broken-pass", "03-waterfall-garden", "04-high-ridge", "05-summit" };
            for (var i = 0; i < ids.Length; i++)
            {
                var block = module.Blocks.Single(x => x.StableId == ids[i]);
                var position = i == 0 ? spawnCameraPosition : block.Pose.Position + Vector3.up * (block.Size.y * .5f + 1.55f);
                var target = i < 4 ? module.Blocks.SkipWhile(x => x.StableId != ids[i]).Skip(1).First().Pose.Position : module.PatchBlock.Pose.Position;
                var direction = target - position; direction.y = 0;
                camera.transform.position = position; camera.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(9, 0, 0);
                yield return null;
                CaptureNow(folder + "/" + names[i] + ".png");
            }
            File.WriteAllText(folder + "/scene-budget.txt", $"renderers={Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Length} colliders={Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Length}");
        }
        private static void CaptureNow(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));var camera=Camera.main;
                var render = new RenderTexture(1560, 720, 24);
                var image = new Texture2D(1560, 720, TextureFormat.RGB24, false);
                var previous = RenderTexture.active; var aspect = camera.aspect;
                var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
                try
                {
                    camera.targetTexture = render; camera.aspect = 1560f / 720;
                    foreach (var canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1; }
                    Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = render;
                    image.ReadPixels(new Rect(0, 0, 1560, 720), 0, 0); image.Apply();
                    File.WriteAllBytes(path, image.EncodeToPNG());
                }
                finally
                {
                    foreach (var canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
                    camera.targetTexture = null; camera.aspect = aspect; RenderTexture.active = previous;
                    Object.Destroy(render); Object.Destroy(image);
                }
        }
    }
}
