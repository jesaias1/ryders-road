using System.Collections;
using System.IO;
using System.Linq;
using Avoidance.Core.Services;
using Avoidance.Gameplay.Blocks;
using Avoidance.Gameplay.Camera;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Respawn;
using Avoidance.Gameplay.Worlds;
using Avoidance.Input;
using Avoidance.SaveSystem;
using Avoidance.UI;
using Avoidance.UI.Touch;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Avoidance.Tests.PlayMode
{
    public sealed class Production099Tests
    {
        private const string Foundry="module.004.solar-foundry";
        private ServiceContainer previous, isolated;
        private JsonSaveService save;
        private Store store;
        private sealed class Store : ISaveFileStore
        {
            private string json;
            public int Writes;
            public bool PrimaryExists=>json!=null;
            public bool BackupExists=>false;
            public bool TryReadPrimary(out string value){value=json;return value!=null;}
            public bool TryReadBackup(out string value){value=null;return false;}
            public void WriteAtomic(string value,bool preserveBackup=false){json=value;Writes++;}
            public void DeleteAll(){json=null;}
        }
        private sealed class Idle : IPlayerInputSource
        {
            public Vector2 Move=>Vector2.zero;
            public Vector2 LookDelta=>Vector2.zero;
            public bool JumpPressed=>false;
            public void ResetState(){}
        }
        [SetUp] public void Isolate()
        {
            CampaignFlowTrial.Clear();previous=GameServices.Current;isolated=new ServiceContainer();store=new Store();
            save=new JsonSaveService(store,"trial-test");save.Initialize();isolated.Register<ISaveService>(save);GameServices.Publish(isolated);
        }
        [TearDown] public void Cleanup()
        {
            CampaignFlowTrial.Clear();Time.timeScale=1;
            if(previous!=null)GameServices.Publish(previous);else GameServices.Clear(isolated);
        }
        private static IEnumerator Open()
        {
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleRunner");
            yield return new WaitForSecondsRealtime(.2f);
            Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled=false;
        }
        [UnityTest] public IEnumerator SceneryFallsRecoverButRouteAndSideContactsRemainPlayable()
        {
            ModuleSelectionState.Select(Foundry,true);yield return Open();
            var motor=Object.FindAnyObjectByType<ParkourMotor>();var restore=motor.GetComponent<RestoreController>();
            var relay=motor.GetComponent<SceneryLandingRecovery>();var idle=new Idle();
            var surfaces=Object.FindObjectsByType<AuthoredSurface>().Where(s=>s.RestoreOnLanding).ToArray();
            Assert.That(surfaces.Length,Is.GreaterThan(10));
            foreach(var surface in surfaces)
            {
                Assert.That(surface.HasMatchingCollision,Is.True);
                relay.NotifyContact(surface.GetComponent<Collider>(),Vector3.right);
                Assert.That(restore.IsRestorePending,Is.False,"Wall brushes must not restore");
            }
            // Sample real low ledges, then fall with the actual character controller onto each.
            int recovered=0;
            foreach(float x in new[]{-18f,-12f,-8f,0f,8f,16f,23f})
            foreach(float z in new[]{6f,15f,34f,55f,75f,100f})
            {
                if(recovered>=5)break;
                motor.ResetMotion(new Vector3(0,50,0),Quaternion.identity,0);Physics.SyncTransforms();
                if(!Physics.Raycast(new Vector3(x,1,z),Vector3.down,out var hit,16,LayerMask.GetMask("Ground"),QueryTriggerInteraction.Ignore))continue;
                if(hit.normal.y<.8f || hit.collider.GetComponent<AuthoredSurface>()?.RestoreOnLanding!=true)continue;
                motor.ResetMotion(hit.point+Vector3.up*1.5f,Quaternion.identity,0);Physics.SyncTransforms();
                int count=restore.RestoreCount;
                for(int step=0;step<120 && restore.RestoreCount==count;step++)
                {motor.Simulate(idle,1f/60);restore.Tick(false,1f/60);}
                Assert.That(restore.RestoreCount,Is.EqualTo(count+1),"Stranded at "+hit.point);
                restore.Tick(false,1);recovered++;
            }
            Assert.That(recovered,Is.GreaterThanOrEqualTo(3));
            var module=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
            foreach(var block in module.Blocks)
            {
                motor.ResetMotion(block.Pose.Position+Vector3.up*.36f,Quaternion.identity,0);Physics.SyncTransforms();
                for(int i=0;i<5;i++)motor.Simulate(idle,1f/60);
                Assert.That(restore.IsRestorePending,Is.False,block.StableId);
            }
            // View cone through the transfer approach: no overhead crane across the jump.
            foreach(float eyeY in new[]{4.92f,5.8f,6.5f})
            foreach(float yaw in new[]{5f,15f,25f,35f,45f})
            foreach(float pitch in new[]{-20f,-10f,0f,10f})
            {
                var origin=new Vector3(11,eyeY,29);
                foreach(var hit in Physics.RaycastAll(origin,Quaternion.Euler(pitch,yaw,0)*Vector3.forward,17))
                    Assert.That(hit.collider.GetComponentsInParent<Transform>().Any(t=>t.name.Contains("transfer-crane")),Is.False,"Jump view obstruction");
            }
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        [UnityTest] public IEnumerator TrialMenuAllRoadsModesRetryCompletionAndExitPreserveSave()
        {
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
            yield return new WaitForSecondsRealtime(.2f);
            int preference=PlayerPrefs.GetInt(TouchInputCoordinator.ControlProfilePreferenceKey,-1);
            string before=JsonUtility.ToJson(save.Current);int writes=store.Writes;
            Click("DEVELOPMENT Button");Click("CAMPAIGN FLOW TRIAL Button");yield return null;
            Capture("trial-menu");Click("Trial Start");yield return new WaitForSecondsRealtime(.6f);
            Assert.That(CampaignFlowTrial.Mode,Is.EqualTo(CampaignTrialMode.Foundation));
            Assert.That(Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule.StableModuleId,Is.EqualTo(Foundry));
            foreach(var id in ModuleSelectionState.GetCampaignModuleIds())
            foreach(var mode in new[]{CampaignTrialMode.Accepted,CampaignTrialMode.FlowManual,CampaignTrialMode.FlowLanding,CampaignTrialMode.PreviousFlow,CampaignTrialMode.Foundation})
            {
                CampaignFlowTrial.Launch(id,mode);yield return Open();
                var player=Object.FindAnyObjectByType<PlayerRuntimeCoordinator>();var motor=player.Motor;
                var touch=Object.FindAnyObjectByType<TouchInputCoordinator>();
                Assert.That(motor.Profile.MovementMastery,Is.EqualTo(mode!=CampaignTrialMode.Accepted));
                if(mode==CampaignTrialMode.Foundation)
                {
                    Assert.That(motor.Profile.CompatibilityVersion,Is.EqualTo(4));
                    Assert.That(touch.JumpLookEnabled && touch.RuntimeProfile.EnableFixedButton,Is.True);
                    Assert.That(touch.RuntimeProfile.FlowSteeringEnabled,Is.False);
                    player.Input.SetMode(PlayerInputMode.Touch);
                    var button=Object.FindAnyObjectByType<JumpTouchControl>();
                    var e=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
                        {pointerId=901,position=new Vector2(1000,400),delta=new Vector2(20,-10)};
                    Assert.That(touch.TryClaim(TouchControlRole.Movement,900),Is.True);
                    touch.SetMovement(Vector2.up);
                    button.OnPointerDown(e);button.OnDrag(e);player.Input.Sample();
                    Assert.That(player.Input.JumpPressed && player.Input.JumpHeld,Is.True);
                    Assert.That(player.Input.Move,Is.EqualTo(Vector2.up));
                    Assert.That(player.Input.LookDelta.sqrMagnitude,Is.GreaterThan(0));
                    var yaw=player.transform.eulerAngles.y;
                    player.CameraRig.ApplyLook(player.Input,PlayerInputMode.Touch,1f/60,motor);
                    Assert.That(Mathf.Abs(Mathf.DeltaAngle(yaw,player.transform.eulerAngles.y)),Is.GreaterThan(.01f));
                    e.position=new Vector2(700,600);button.OnDrag(e);player.Input.Sample();
                    Assert.That(player.Input.JumpHeld,Is.True);Assert.That(player.Input.JumpPressed,Is.False);
                    var stranger=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){pointerId=902};
                    button.OnPointerUp(stranger);Assert.That(touch.JumpHeld,Is.True);
                    button.OnPointerUp(e);player.Input.Sample();Assert.That(player.Input.JumpHeld,Is.False);
                    Assert.That(player.Input.JumpPressed,Is.False,"Release must not emit a second jump");
                    button.OnPointerDown(e);touch.ResetState();player.Input.Sample();
                    Assert.That(player.Input.JumpHeld,Is.False);Assert.That(player.Input.JumpPressed,Is.False);
                    touch.Release(TouchControlRole.Movement,900);

                }
                else if(mode!=CampaignTrialMode.Accepted)
                {
                    Assert.That(touch.RuntimeProfile.FlowSteeringEnabled,Is.True);
                    Assert.That(touch.RuntimeProfile.EnableFixedButton,Is.False);
                    Assert.That(player.CameraRig.Pitch,Is.EqualTo(mode==CampaignTrialMode.FlowLanding?16:9).Within(.1f));
                }
                Assert.That(JsonUtility.ToJson(save.Current),Is.EqualTo(before));Assert.That(store.Writes,Is.EqualTo(writes));
                if(id==Foundry && (mode==CampaignTrialMode.FlowLanding || mode==CampaignTrialMode.Foundation))
                {
                    player.Input.SetMode(PlayerInputMode.Touch);touch.SetMovement(Vector2.up);
                    Assert.That(touch.TryClaim(TouchControlRole.Look,77),Is.True);
                    touch.BeginLookGesture(77,new Vector2(1000,400),1);touch.EndLookGesture(77,new Vector2(1000,400),1.05f,800);
                    touch.Release(TouchControlRole.Look,77);Assert.That(touch.ConsumeJumpPressed(),Is.True);
                    touch.AddLookDelta(new Vector2(0,20));player.Input.Sample();float pitch=player.CameraRig.Pitch;
                    player.CameraRig.ApplyLook(player.Input,PlayerInputMode.Touch,1f/60,motor);
                    Assert.That(player.CameraRig.Pitch,Is.LessThan(pitch));
                    player.GetComponent<RestoreController>().RestoreNow();Assert.That(touch.Movement,Is.EqualTo(Vector2.zero));
                    Assert.That(player.CameraRig.Pitch,Is.EqualTo(mode==CampaignTrialMode.FlowLanding?16:9).Within(.1f));
                    Capture("foundry-flow");
                    Assert.That(Object.FindAnyObjectByType<PatchBlock>().TryComplete(motor),Is.True);
                    yield return new WaitForSecondsRealtime(.3f);Capture("trial-results");
                    Assert.That(CampaignFlowTrial.Best(Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule),Is.GreaterThan(0));
                    Click("RETRY Button");yield return new WaitForSecondsRealtime(.6f);
                    Assert.That(CampaignFlowTrial.Mode,Is.EqualTo(mode));
                }
            }
            Assert.That(JsonUtility.ToJson(save.Current),Is.EqualTo(before));Assert.That(store.Writes,Is.EqualTo(writes));
            Assert.That(PlayerPrefs.GetInt(TouchInputCoordinator.ControlProfilePreferenceKey,-1),Is.EqualTo(preference));
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
            Assert.That(CampaignFlowTrial.Active,Is.False);
            ModuleSelectionState.Select(Foundry,true);yield return Open();
            Assert.That(Object.FindAnyObjectByType<ParkourMotor>().Profile.MovementMastery,Is.False);
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        private static void Click(string name)=>Object.FindObjectsByType<Button>().Single(b=>b.name==name).onClick.Invoke();
        private static void Capture(string name)
        {
            Directory.CreateDirectory("Logs/Production099QA");var camera=Camera.main;
            var rt=new RenderTexture(1560,720,24);var tex=new Texture2D(1560,720,TextureFormat.RGB24,false);
            var active=RenderTexture.active;var aspect=camera.aspect;
            var canvases=Object.FindObjectsByType<Canvas>().Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
            try
            {
                camera.targetTexture=rt;camera.aspect=1560f/720;
                foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}
                Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;
                tex.ReadPixels(new Rect(0,0,1560,720),0,0);tex.Apply();File.WriteAllBytes("Logs/Production099QA/"+name+".png",tex.EncodeToPNG());
            }
            finally
            {
                foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}
                camera.targetTexture=null;camera.aspect=aspect;RenderTexture.active=active;Object.Destroy(rt);Object.Destroy(tex);
            }
        }
    }
}
